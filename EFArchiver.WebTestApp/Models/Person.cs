namespace EFArchiver.WebTestApp.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
