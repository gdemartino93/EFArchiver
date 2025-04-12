using EFArchiver.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EFArchiver.Helpers
{
    /// <summary>
    /// retrn metadata from entity classes marked with [PartitionKey]
    /// </summary>
    public static class PartitionMetadataHelper
    {
        /// <summary>
        /// find the prop marked with [PartitionKey] in the specified entity
        /// </summary>
        /// <returns>The PropertyInfo of the partition key, or null if not found.</returns>
        public static PropertyInfo? GetPartitionKeyProperty<T>()
        {
            var type = typeof(T);
            var props = type.GetProperties(); // get all the public props
            foreach (var prop in props)
            {
                // search for the [PartitionKey] attribute on each property then return the first one found
                var attribute = prop.GetCustomAttribute<PartitionKeyAttribute>();
                if (attribute != null)
                {
                    return prop;
                }
            }
            return null;
        }

        /// <summary>
        /// retrieves the settings from the [PartitionKey] attribute of the entity type
        /// </summary>
        /// <returns>return istance of attribute for access at parameters (eg. equalTo, thresholdDays) or NULL if not found</returns>
        public static PartitionKeyAttribute? GetPartitionKeySettings<T>()
        {
            var prop = GetPartitionKeyProperty<T>();
            if (prop == null)
            {
                return null;
            }
            return prop.GetCustomAttribute<PartitionKeyAttribute>();
        }
    }
}
