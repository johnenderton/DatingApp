namespace Helpers
{
    public class MessageParams : PaginationParams
    {
        public string Username { get; set; } // Current log in user
        public string Container { get; set; } = "Unread";
        
    }
}