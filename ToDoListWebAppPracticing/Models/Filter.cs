namespace ToDoListWebAppPracticing.Models
{
    public class Filter
    {
        public Filter(string filterString)
        {
            FilterString = filterString ?? "all-all-all";
            string[] filter = FilterString.Split('-');
            CategoryId = filter[0];
            Due = filter[1];
            StatusId = filter[2];
        }

        public string FilterString { get; }
        public string CategoryId { get; }
        public string Due { get; }
        public string StatusId { get; }
        public bool IsCategory => CategoryId.ToLower() != "all";
        public bool IsDue => Due.ToLower() != "all";
        public bool IsStatus => StatusId.ToLower() != "all";
        public static Dictionary<string, string> DueFilterValues => new Dictionary<string, string>
        {
            { "future", "FUTURE" },
            { "today", "TODAY" },
            { "past", "PAST" }
        };

        public bool IsPast => Due.ToLower() == "past";
        public bool IsToday => Due.ToLower() == "today";
        public bool IsFuture => Due.ToLower() == "future";
    }
}
