using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EFArchiver.WebTestApp.Models
{
    public class Profile
    {
        public Guid Id { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid PersonId { get; set; }
        [JsonIgnore]
        public Person? Person { get; set; }

        public Profile(string bio, string avatarUrl)
        {
            Bio = bio;
            AvatarUrl = avatarUrl;
        }
    }
}
