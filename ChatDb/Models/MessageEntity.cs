using System.ComponentModel.DataAnnotations;

namespace ChatDb.Models
{
    public class MessageEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Text { get; set; }
        public int SenderId { get; set; }   
        public int RecipientId { get; set; }
        public bool Received { get; set; } = false;
        public DateTime CreatedAt { get; set; }

    }
}
