namespace ClinicMS.Shared.Common
{
    // When user is on "Patients" page and sees page 1 of 5
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();   // actual records for this page
        public int TotalCount { get; set; }            // total records in DB matching filter
        public int Page { get; set; }                  // current page number (1-based)
        public int PageSize { get; set; }              // records per page (e.g. 10)

        // Computed — how many pages total exist?
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Is there a page before this one?
        public bool HasPreviousPage => Page > 1;

        // Is there a page after this one?
        public bool HasNextPage => Page < TotalPages;
    }
}