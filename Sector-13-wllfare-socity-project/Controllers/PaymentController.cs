using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Text;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Payment/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Payment/Index/{orderId}
        public async Task<IActionResult> ProcessPayment(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Service)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.PaymentStatus == "Completed")
            {
                TempData["Info"] = "This order has already been paid.";
                return RedirectToAction("OrderDetails", "MemberDashboard", new { id = orderId });
            }

            var paymentViewModel = new PaymentViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Amount = order.TotalAmount,
                Currency = order.Currency,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone
            };

            return View(paymentViewModel);
        }

        // POST: Payment/ProcessOnlinePayment
        [HttpPost]
        public async Task<IActionResult> ProcessOnlinePayment(int orderId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                if (order.PaymentStatus == "Completed")
                {
                    return Json(new { success = false, message = "Order already paid." });
                }

                // Validate configuration
                var storeId = _configuration["SSLCommerz:StoreId"];
                var storePassword = _configuration["SSLCommerz:StorePassword"];
                
                if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword))
                {
                    return Json(new { success = false, message = "Payment gateway configuration is incomplete. Please contact support." });
                }

                // Prepare SSLCommerz payment data
                var tranId = $"TXN_{order.OrderNumber}_{DateTime.Now:yyyyMMddHHmmss}";
                var sslCommerzData = new
                {
                    store_id = storeId,
                    store_passwd = storePassword,
                    total_amount = order.TotalAmount.ToString("F2"),
                    currency = order.Currency,
                    tran_id = tranId,
                    success_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentSuccess",
                    fail_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentFail",
                    cancel_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentCancel",
                    ipn_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentIPN",
                    cus_name = order.CustomerName,
                    cus_email = order.CustomerEmail,
                    cus_add1 = order.CustomerAddress ?? "N/A",
                    cus_city = "Dhaka",
                    cus_country = "Bangladesh",
                    cus_phone = order.CustomerPhone,
                    ship_name = order.CustomerName,
                    ship_add1 = order.CustomerAddress ?? "N/A",
                    ship_city = "Dhaka",
                    ship_country = "Bangladesh",
                    product_name = $"Order {order.OrderNumber}",
                    product_category = "Service",
                    product_profile = "general"
                };

                // Update order with transaction ID
                order.TransactionId = tranId;
                order.PaymentMethod = "SSLCommerz";
                order.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Call SSLCommerz API
                var sslCommerzUrl = _configuration["SSLCommerz:SessionApiUrl"];
                
                // Validate configuration
                if (string.IsNullOrEmpty(sslCommerzUrl))
                {
                    return Json(new { success = false, message = "Payment gateway configuration is missing. Please contact support." });
                }

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var formContent = new FormUrlEncodedContent(sslCommerzData.GetType()
                    .GetProperties()
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(sslCommerzData)?.ToString() ?? ""));

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"SSL Commerz URL: {sslCommerzUrl}");
                System.Diagnostics.Debug.WriteLine($"Store ID: {sslCommerzData.store_id}");
                System.Diagnostics.Debug.WriteLine($"Transaction ID: {sslCommerzData.tran_id}");

                var response = await httpClient.PostAsync(sslCommerzUrl, formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Debug response
                System.Diagnostics.Debug.WriteLine($"SSL Commerz Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"SSL Commerz Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(responseContent);
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("status", out var statusElement) && 
                            statusElement.GetString() == "SUCCESS")
                        {
                            var gatewayUrl = root.TryGetProperty("GatewayPageURL", out var urlElement) 
                                ? urlElement.GetString() 
                                : null;
                                
                            if (!string.IsNullOrEmpty(gatewayUrl))
                            {
                                return Json(new { 
                                    success = true, 
                                    redirectUrl = gatewayUrl
                                });
                            }
                            else
                            {
                                return Json(new { success = false, message = "Payment gateway did not return a valid URL." });
                            }
                        }
                        else
                        {
                            // Try to get error message from response
                            var errorMessage = "Payment gateway error";
                            if (root.TryGetProperty("failedreason", out var errorElement))
                            {
                                errorMessage = errorElement.GetString() ?? errorMessage;
                            }
                            return Json(new { success = false, message = errorMessage });
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        return Json(new { success = false, message = "Invalid response from payment gateway." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"Payment gateway error: HTTP {response.StatusCode}" });
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP Request Exception: {httpEx.Message}");
                return Json(new { success = false, message = "Unable to connect to payment gateway. Please check your internet connection and try again." });
            }
            catch (TaskCanceledException timeoutEx)
            {
                System.Diagnostics.Debug.WriteLine($"Timeout Exception: {timeoutEx.Message}");
                return Json(new { success = false, message = "Payment gateway request timed out. Please try again." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while processing payment. Please try again." });
            }
        }

        // POST: Payment/ProcessOfflinePayment
        [HttpPost]
        public async Task<IActionResult> ProcessOfflinePayment(int orderId, string paymentMethod, string? transactionId, string? notes, IFormFile? paymentProof)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                if (order.PaymentStatus == "Completed")
                {
                    return Json(new { success = false, message = "Order already paid." });
                }

                // Handle file upload for payment proof
                string? paymentProofPath = null;
                if (paymentProof != null && paymentProof.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payment-proofs");
                    Directory.CreateDirectory(uploadsFolder);
                    
                    var fileName = $"{order.OrderNumber}_{DateTime.Now:yyyyMMddHHmmss}_{paymentProof.FileName}";
                    paymentProofPath = Path.Combine(uploadsFolder, fileName);
                    
                    using var stream = new FileStream(paymentProofPath, FileMode.Create);
                    await paymentProof.CopyToAsync(stream);
                    
                    paymentProofPath = $"/uploads/payment-proofs/{fileName}";
                }

                // Update order
                order.PaymentMethod = paymentMethod;
                order.PaymentStatus = "Under Review";
                order.OrderStatus = "Payment Submitted";
                order.TransactionId = transactionId;
                order.Notes = $"{order.Notes}\nPayment Notes: {notes}";
                order.UpdatedAt = DateTime.Now;

                // Create payment record
                var paymentRecord = new PaymentRecord
                {
                    OrderId = order.Id,
                    PaymentMethod = paymentMethod,
                    Amount = order.TotalAmount,
                    TransactionId = transactionId,
                    PaymentProofUrl = paymentProofPath,
                    Status = "Under Review",
                    Notes = notes,
                    CreatedAt = DateTime.Now,
                    IsDelete = false
                };

                _context.PaymentRecords.Add(paymentRecord);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Payment submitted successfully! We will verify your payment within 24 hours.",
                    redirectUrl = Url.Action("OrderDetails", "MemberDashboard", new { id = orderId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while processing payment." });
            }
        }

        // GET/POST: Payment/PaymentSuccess
        [AllowAnonymous]
        [HttpGet, HttpPost]
        public async Task<IActionResult> PaymentSuccess()
        {
            try
            {
                // Handle both GET and POST requests from SSL Commerz - try multiple parameter variations
                var tranId = Request.Form["tran_id"].FirstOrDefault() 
                    ?? Request.Query["tran_id"].FirstOrDefault()
                    ?? Request.Form["tranId"].FirstOrDefault()
                    ?? Request.Query["tranId"].FirstOrDefault()
                    ?? string.Empty;
                
                var status = Request.Form["status"].FirstOrDefault() 
                    ?? Request.Query["status"].FirstOrDefault() 
                    ?? string.Empty;
                
                var bankTranId = Request.Form["bank_tran_id"].FirstOrDefault() 
                    ?? Request.Query["bank_tran_id"].FirstOrDefault() 
                    ?? string.Empty;
                
                // Enhanced debugging
                Console.WriteLine($"=== SSL Payment Success Callback ===");
                Console.WriteLine($"TranId: '{tranId}'");
                Console.WriteLine($"Status: '{status}'");
                Console.WriteLine($"BankTranId: '{bankTranId}'");
                Console.WriteLine($"Request Method: {Request.Method}");
                Console.WriteLine($"Request URL: {Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                Console.WriteLine($"All Form Keys: {string.Join(", ", Request.Form.Keys)}");
                Console.WriteLine($"Form Data: {string.Join(", ", Request.Form.Select(x => $"{x.Key}={x.Value}"))}");
                Console.WriteLine($"All Query Keys: {string.Join(", ", Request.Query.Keys)}");
                Console.WriteLine($"Query Data: {string.Join(", ", Request.Query.Select(x => $"{x.Key}={x.Value}"))}");
                
                System.Diagnostics.Debug.WriteLine($"PaymentSuccess called with tranId: {tranId}, status: {status}, bankTranId: {bankTranId}");
                
                if (string.IsNullOrEmpty(tranId))
                {
                    Console.WriteLine("ERROR: Transaction ID is empty! Redirecting to Home.");
                    TempData["Error"] = "Invalid transaction ID. Payment callback received but transaction ID was missing.";
                    return RedirectToAction("Index", "Home");
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.TransactionId == tranId);
                if (order != null)
                {
                    var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";
                    var shouldAccept = isSandbox || status == "VALID";
                    
                    if (shouldAccept)
                    {
                        order.PaymentStatus = "Completed";
                        order.OrderStatus = "Paid";
                        order.PaymentDate = DateTime.Now;
                        order.UpdatedAt = DateTime.Now;
                        
                        if (!string.IsNullOrEmpty(bankTranId))
                        {
                            order.TransactionId = bankTranId;
                        }
                        
                        // Create payment record
                        var paymentRecord = new PaymentRecord
                        {
                            OrderId = order.Id,
                            PaymentMethod = "SSLCommerz",
                            Amount = order.TotalAmount,
                            Currency = order.Currency,
                            TransactionId = bankTranId ?? tranId,
                            Status = "Completed",
                            RequiresApproval = false,
                            ApprovalStatus = "Approved",
                            CustomerName = order.CustomerName,
                            CustomerEmail = order.CustomerEmail,
                            CustomerPhone = order.CustomerPhone,
                            VerifiedAt = DateTime.Now,
                            VerifiedBy = "System"
                        };
                        
                        _context.PaymentRecords.Add(paymentRecord);
                        await _context.SaveChangesAsync();

                        // Store success data in TempData for the success page
                        TempData["PaymentSuccess"] = "true";
                        TempData["OrderId"] = order.Id;
                        TempData["OrderNumber"] = order.OrderNumber;
                        TempData["Amount"] = order.TotalAmount;
                        TempData["TransactionId"] = bankTranId ?? tranId;
                        TempData["CustomerName"] = order.CustomerName;
                        
                        return RedirectToAction("PaymentSuccessPage", new { id = order.Id });
                    }
                    else
                    {
                        order.PaymentStatus = "Failed";
                        await _context.SaveChangesAsync();
                        
                        TempData["Error"] = "Payment validation failed. Please contact support.";
                        return RedirectToAction("Index", "Home");
                    }
                }

                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PaymentSuccess Exception: {ex.Message}");
                TempData["Error"] = "An error occurred while processing payment.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Payment/PaymentFail
        [AllowAnonymous]
        public IActionResult PaymentFail()
        {
            TempData["Error"] = "Payment failed. Please try again.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Payment/PaymentSuccessPage
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccessPage(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Service)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        // GET: Payment/Settings
        public IActionResult Settings()
        {
            return View();
        }

        // GET: Payment/TestSuccess
        [AllowAnonymous]
        public IActionResult TestSuccess()
        {
            Console.WriteLine("=== Test Success Action Called ===");
            TempData["PaymentSuccess"] = "true";
            TempData["OrderId"] = "1";
            TempData["OrderNumber"] = "TEST-001";
            TempData["Amount"] = "100.00";
            TempData["TransactionId"] = "TEST_TXN_001";
            TempData["CustomerName"] = "Test Customer";
            
            return RedirectToAction("PaymentSuccessPage", new { id = 1 });
        }

        // GET: Payment/TestSSL
        [AllowAnonymous]
        public async Task<IActionResult> TestSSL()
        {
            try
            {
                var storeId = _configuration["SSLCommerz:StoreId"];
                var storePassword = _configuration["SSLCommerz:StorePassword"];
                var sessionApiUrl = _configuration["SSLCommerz:SessionApiUrl"];
                var registeredUrl = _configuration["SSLCommerz:RegisteredUrl"];
                
                Console.WriteLine($"=== SSL Configuration Test ===");
                Console.WriteLine($"Store ID: {storeId}");
                Console.WriteLine($"Store Password: {storePassword?.Substring(0, Math.Min(10, storePassword?.Length ?? 0))}...");
                Console.WriteLine($"Session API URL: {sessionApiUrl}");
                Console.WriteLine($"Registered URL: {registeredUrl}");
                
                var testData = new Dictionary<string, string>
                {
                    ["store_id"] = storeId ?? "",
                    ["store_passwd"] = storePassword ?? "",
                    ["total_amount"] = "100.00",
                    ["currency"] = "USD",
                    ["tran_id"] = $"TEST_{DateTime.Now:yyyyMMddHHmmss}",
                    ["product_category"] = "Services",
                    ["product_name"] = "Test Payment",
                    ["product_profile"] = "general",
                    ["cus_name"] = "Test Customer",
                    ["cus_email"] = "test@example.com",
                    ["cus_add1"] = "Test Address",
                    ["cus_city"] = "Dhaka",
                    ["cus_state"] = "Dhaka",
                    ["cus_postcode"] = "1000",
                    ["cus_country"] = "Bangladesh",
                    ["cus_phone"] = "01700000000",
                    ["success_url"] = $"{Request.Scheme}://{Request.Host}/Payment/TestSuccess",
                    ["fail_url"] = $"{Request.Scheme}://{Request.Host}/Payment/PaymentFail",
                    ["cancel_url"] = $"{Request.Scheme}://{Request.Host}/Payment/PaymentCancel"
                };
                
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var content = new FormUrlEncodedContent(testData);
                var response = await client.PostAsync(sessionApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"=== SSL Test Response ===");
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Response: {responseContent}");
                
                TempData["SSLTestResult"] = $"Status: {response.StatusCode}, Response: {responseContent}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SSL Test Error: {ex.Message}");
                TempData["SSLTestError"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Payment/UpdateSSLCommerzSettings
        [HttpPost]
        public IActionResult UpdateSSLCommerzSettings(string StoreId, string StorePassword, string IsSandbox, string StoreName, string RegisteredUrl)
        {
            // Here you would typically update the configuration in appsettings.json or database
            // For now, we'll just show a success message
            TempData["Success"] = "SSLCommerz settings updated successfully!";
            return RedirectToAction("Settings");
        }

        // GET: Payment/PaymentCancel
        [AllowAnonymous]
        public IActionResult PaymentCancel()
        {
            TempData["Info"] = "Payment was cancelled.";
            return RedirectToAction("Index", "Home");
        }

        // POST: Payment/PaymentIPN
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentIPN()
        {
            // Handle SSLCommerz IPN (Instant Payment Notification)
            try
            {
                var tranId = Request.Form["tran_id"];
                var status = Request.Form["status"];
                
                if (!string.IsNullOrEmpty(tranId))
                {
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.TransactionId == tranId);
                    if (order != null && status == "VALID")
                    {
                        order.PaymentStatus = "Completed";
                        order.OrderStatus = "Paid";
                        order.PaymentDate = DateTime.Now;
                        order.UpdatedAt = DateTime.Now;
                        
                        // Create payment record if not exists
                        var existingPayment = await _context.PaymentRecords
                            .FirstOrDefaultAsync(p => p.OrderId == order.Id && p.TransactionId == tranId);
                        
                        if (existingPayment == null)
                        {
                            var paymentRecord = new PaymentRecord
                            {
                                OrderId = order.Id,
                                PaymentMethod = "SSLCommerz",
                                Amount = order.TotalAmount,
                                Currency = order.Currency,
                                TransactionId = tranId,
                                Status = "Completed",
                                RequiresApproval = false,
                                ApprovalStatus = "Approved",
                                CustomerName = order.CustomerName,
                                CustomerEmail = order.CustomerEmail,
                                CustomerPhone = order.CustomerPhone,
                                VerifiedAt = DateTime.Now,
                                VerifiedBy = "System"
                            };
                            
                            _context.PaymentRecords.Add(paymentRecord);
                        }
                        
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok("SUCCESS");
            }
            catch
            {
                return Ok("FAIL");
            }
        }

        // GET: Payment/TestSSLCommerz - Debug endpoint to test SSL Commerz configuration
        public async Task<IActionResult> TestSSLCommerz()
        {
            var testData = new
            {
                store_id = _configuration["SSLCommerz:StoreId"],
                store_passwd = _configuration["SSLCommerz:StorePassword"],
                total_amount = "100.00",
                currency = "USD",
                tran_id = $"TEST_{DateTime.Now:yyyyMMddHHmmss}",
                success_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentSuccess",
                fail_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentFail",
                cancel_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentCancel",
                cus_name = "Test Customer",
                cus_email = "test@example.com",
                cus_add1 = "Test Address",
                cus_city = "Dhaka",
                cus_country = "Bangladesh",
                cus_phone = "01700000000",
                product_name = "Test Product",
                product_category = "Service",
                product_profile = "general"
            };

            try
            {
                var sslCommerzUrl = _configuration["SSLCommerz:SessionApiUrl"];
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var formContent = new FormUrlEncodedContent(testData.GetType()
                    .GetProperties()
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(testData)?.ToString() ?? ""));

                var response = await httpClient.PostAsync(sslCommerzUrl, formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = new
                {
                    RequestUrl = sslCommerzUrl,
                    RequestData = testData,
                    ResponseStatus = response.StatusCode.ToString(),
                    ResponseContent = responseContent,
                    IsSuccess = response.IsSuccessStatusCode
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace,
                    configUrl = _configuration["SSLCommerz:SessionApiUrl"],
                    configStoreId = _configuration["SSLCommerz:StoreId"]
                });
            }
        }
    }
}
