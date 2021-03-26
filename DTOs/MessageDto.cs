using System;
using System.Text.Json.Serialization;

namespace DTOs
{
    public class MessageDto
    {
        // Message ID
        public int Id { get; set; }

        // These Properties define relationship between appuser and message
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public string SenderPhotoUrl { get; set; }
        public int RecipientId { get; set; }
        public string RecipientUsername { get; set; }
        public string RecipientPhotoUrl { get; set; }

        // Message specific properties
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }

        [JsonIgnore] // this property will not be sent back to client
        public bool SenderDeleted { get; set; }

        [JsonIgnore] // this property will not be sent back to client
        public bool RecipientDeleted { get; set; }
    }
}