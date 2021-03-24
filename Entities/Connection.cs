namespace Entities
{
    public class Connection
    {
        public Connection()
        {
            
        }
        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
        }

        // By convention, if we give property class name + id
        // it will be recognized as primary key
        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}