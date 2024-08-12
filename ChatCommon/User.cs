
using ChatDb.Models;
using System.Net;
using System.Text.Json.Serialization;

namespace ChatCommon
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public IPEndPoint? EndPoint { get; set; }

        public static User FromModels(UserEntity user) => new User ()
        {
            Id = user.Id,
            Name = user.Name
        };
        
    }
}
