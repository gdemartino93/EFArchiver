using EFArchiver.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFArchiver.Helpers
{
    public class PartitionEntityScanner
    {
        /// <summary>
        /// return all the entities in DbContext decorated with [PartitionedEntity]
        /// </summary>
        public static IEnumerable<IEntityType> GetPartitionedEntities(DbContext context)
        {
            return context.Model
                .GetEntityTypes() // retrieve all classes that have a DbSet
                .Where(e => e.ClrType.GetCustomAttributes(typeof(PartitionedEntityAttribute), inherit: false).Any()); // see if the original class has decorator  [PartitionedEntity]
        }
    }
}
