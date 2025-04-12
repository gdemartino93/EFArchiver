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
    /// utility class to extract metadata from entity market with [PartitionKey]
    /// </summary>
    public static class PartitionMetadataHelper
    {
        /// <summary>
        /// retrieves the prop marked with [PartitionKey] from the specified generic entity type
        /// </summary>
        /// <typeparam name="T">The entity type to inspect</typeparam>
        /// <returns>the PropertyInfo of the partition key or null if not found</returns>
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
        /// retrieves the property marked with [PartitionKey] from the specified CLR type
        /// This overload is used when iterating through entities dynamically
        /// </summary>
        /// <param name="entityType">the CLR type of the entity</param>
        /// <returns> PropertyInfo of the partition key or null if not found</returns>
        public static PropertyInfo? GetPartitionKeyProperty(Type entityType)
        {
            var props = entityType.GetProperties();
            foreach (var prop in props)
            {
                var attribute = prop.GetCustomAttribute<PartitionKeyAttribute>();
                if (attribute != null)
                {
                    return prop;
                }
            }
            return null;
        }

        /// <summary>
        /// retrieves the [PartitionKey] attribute instance from the specified generic entity type.
        /// </summary>
        /// <typeparam name="T">the entity type to inspect</typeparam>
        /// <returns> PartitionKeyAttribute instance or null if not found.</returns>
        public static PartitionKeyAttribute? GetPartitionKeySettings<T>()
        {
            var prop = GetPartitionKeyProperty<T>();
            if (prop == null)
            {
                return null;
            }
            return prop.GetCustomAttribute<PartitionKeyAttribute>();
        }

        /// <summary>
        /// retrieves the [PartitionKey] attribute instance from the specified CLR type
        /// this overload is used when iterating through entities dynamically
        /// </summary>
        /// <param name="entityType">the CLR type of the entity</param>
        /// <returns>PartitionKeyAttribute instance or null if not found</returns>
        public static PartitionKeyAttribute? GetPartitionKeySettings(Type entityType)
        {
            var prop = GetPartitionKeyProperty(entityType);
            if (prop == null)
            {
                return null;
            }
            return prop.GetCustomAttribute<PartitionKeyAttribute>();
        }
    }
}
