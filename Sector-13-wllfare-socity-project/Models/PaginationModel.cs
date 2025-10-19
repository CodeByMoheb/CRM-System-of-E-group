namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class PaginationModel<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
        
        public int StartIndex => (CurrentPage - 1) * PageSize + 1;
        public int EndIndex => Math.Min(CurrentPage * PageSize, TotalItems);
    }
}
