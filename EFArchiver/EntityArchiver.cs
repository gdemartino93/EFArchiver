﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace EFArchiver
{
    public class EntityArchiver<T> : IEntityArchiver<T> where T : class
    {
        private readonly DbContext _dbContext;
        public EntityArchiver(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ArchiveAsync(Func<T, bool> predicate, string suffixStorageTable)
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(T));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Type {typeof(T).Name} not found in the EF models");
            }

            var set = _dbContext.Set<T>();
            // get all the navigation props of entity
            var navigationProps = entityType.GetNavigations()
                .Select(np => np.Name)
                .ToList();

            IQueryable<T> query = set;

            // apply dinamically include for each prop
            foreach (var navProp in navigationProps)
            {
                query = query.Include(navProp);
            }
            // load entities with navigation proprieties included
            var entitiesToArchive = query.Where(predicate).ToList();




            // create id to access mapped column names
            var tableMapping = entityType.GetTableMappings().FirstOrDefault();
            if (tableMapping == null)
            {
                throw new InvalidOperationException($"table mapping not found for: {typeof(T).Name}");
            }
            var tableName = tableMapping.Table.Name;
            var schema = tableMapping.Table.Schema ?? "dbo";
            var tableId = StoreObjectIdentifier.Table(tableName, schema);
            var storageTable = $"{tableName}_{suffixStorageTable}";

            foreach (var entity in entitiesToArchive)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
                //take the name of clumns and value to move
                var entryValues = entityType.GetProperties()
                    // where => include only the props that exist in the rntime type
                    .Where(p => entity.GetType().GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy) != null)
                    .Select(p => new
                    {
                        Name = p.GetColumnBaseName(),
                        // use runtime of entity to get the props (includes inherited)
                        Value = entity.GetType().GetProperty(p.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!.GetValue(entity)
                    })
                    .ToList();


                var columnList = string.Join(", ", entryValues.Select(x => $"[{x.Name}]"));
                var valueList = string.Join(", ", entryValues.Select(x => EntityArchiver<T>.FormatValueForSql(x.Value)));
                var insertSql = $"INSERT INTO [{storageTable}] ({columnList}) VALUES ({valueList})";
                await _dbContext.Database.ExecuteSqlRawAsync(insertSql);
                //remove from original table
                set.Remove(entity);
            }
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// safe format values
        /// </summary>
        private static string FormatValueForSql(object? value)
        {
            if (value == null)
                return "NULL";

            return value switch
            {
                string s => $"'{s.Replace("'", "''")}'",
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'",
                bool b => b ? "1" : "0",
                _ => $"'{value}'"
            };
        }
    }
}
