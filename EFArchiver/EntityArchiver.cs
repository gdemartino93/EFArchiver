using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
            var set = _dbContext.Set<T>();

            var entitiesToArchive = set.Where(predicate).ToList();
            if (!entitiesToArchive.Any())
            {
                return;
            }

            var entityType = _dbContext.Model.FindEntityType(typeof(T));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Type {typeof(T).Name} not found in the EF models");
            }
            //obtain the name of table and schema db
            var tableName = entityType.GetTableName() ?? typeof(T).Name;
            var schema = entityType.GetSchema() ?? "dbo";

            var storageTable = $"{tableName}_{suffixStorageTable}";

            // create id to access mapped column names
            var tableId = StoreObjectIdentifier.Table(tableName, schema);

            foreach (var entity in entitiesToArchive)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
                //take the name of clumn and value to save
                var entryValues = entityType.GetProperties()
                    .Select(p => new
                    {
                        Name = p.GetColumnName(tableId),
                        Value = typeof(T).GetProperty(p.Name)!.GetValue(entity)
                    }).ToList();

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
