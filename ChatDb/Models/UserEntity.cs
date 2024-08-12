using System.ComponentModel.DataAnnotations;

namespace ChatDb.Models
{
    public class UserEntity
    {
        [Key]
        public int Id { get; set; }
        public required string Name {  get; set; }        
    }
}
