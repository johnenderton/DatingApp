using System;

namespace Entities
{
    public class Message
    {
        // Message ID
        public int Id { get; set; }

        // These Properties define relationship between appuser and message
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public AppUser Sender { get; set; } // Related property
        public int RecipientId { get; set; }
        public string RecipientUsername { get; set; }
        public AppUser Recipient { get; set; }

        // Message specific properties
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; } = DateTime.Now;
        // Message in the Server will be deleted if both sender and recipient delete it
        public bool SenderDeleted { get; set; }
        public bool RecipientDeleted { get; set; }
    }
}