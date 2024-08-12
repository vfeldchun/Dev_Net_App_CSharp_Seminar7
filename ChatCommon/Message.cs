
using ChatDb.Models;
using System.Text.Json;

namespace ChatCommon
{
    public class Message
    {
        public Command Command { get; set; } = Command.None;
        public int? Id { get; set; }
        public string FromName { get; set; }
        public string? ToName { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Message FromJson(string json)
        {
            return JsonSerializer.Deserialize<Message>(json);
        }

        public override string ToString() 
        {
            return $"От: {FromName} Для: {ToName} Сообщение: {Text}";
        }

        public static Message FromMoidels (MessageEntity entity) => new Message ()
            { 
                Id = entity.Id,
                //FromName = entity.SenderId,
                //ToName = entity.RecipientId,
                CreatedAt = entity.CreatedAt,
                Text = entity.Text,
            };
       
    }
}
