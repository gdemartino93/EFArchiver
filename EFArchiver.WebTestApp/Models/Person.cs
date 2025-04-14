using EFArchiver.Attributes;

namespace EFArchiver.WebTestApp.Models
{
    [PartitionedEntity]
    public class Person
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        [PartitionKey(ThresholdDays = 90)]
        public DateTime CreatedAt { get; set; }
        public Profile? Profile { get; set; }

        public Person()
        {
            
        }
    }
}
