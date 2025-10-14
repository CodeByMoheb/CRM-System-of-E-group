using System;
using System.Collections.Generic;
using System.Linq;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ManagerDashboardViewModel
    {
        // High-level KPI cards
        public int TotalEmployees { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public int PendingLeaves { get; set; }
        public int ActiveShifts { get; set; }
        public int TotalServices { get; set; }
        public int TodayBookings { get; set; }

        // Tables
        public List<Employee> RecentEmployees { get; set; } = new();
        public List<Leave> RecentLeaveRequests { get; set; } = new();
        public List<Booking> RecentBookings { get; set; } = new();

        // Charts data (server-side prepared)
        public List<string> AttendanceLabels { get; set; } = new(); // e.g., last 7 days
        public List<int> AttendancePresentCounts { get; set; } = new();
        public List<int> AttendanceAbsentCounts { get; set; } = new();

        public List<string> ServiceLabels { get; set; } = new();
        public List<int> ServiceUsageCounts { get; set; } = new();

        // Leave policy overview
        public List<LeaveEntitlementPolicy> LeavePolicies { get; set; } = new();
        public List<Shift> Shifts { get; set; } = new();
    }
}


