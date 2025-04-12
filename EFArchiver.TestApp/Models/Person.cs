using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFArchiver.TestApp.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
