namespace EFArchiver.WebTestApp.Models
{
    public class Profile
    {
        public Guid Id { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid PersonId { get; set; }
        public Person Person { get; set; }
    }
}
