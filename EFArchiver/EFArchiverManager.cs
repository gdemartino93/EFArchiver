using EFArchiver.Attributes;
using EFArchiver.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace EFArchiver
{
    public class EFArchiverManager
    {
        private readonly DbContext _dbContext;
        private const string SuffixTable = "Storage";
        public EFArchiverManager(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// partitions all entities in the DbContext that are decorated with [PartitionedEntity]
        /// this use the [PartitionKey] attribute to decide which records should be moved to storage table
        /// </summary>
        /// <param name="suffix">suffix to append to the archive table name (default is "Storage").</param>
        public async Task PartitionAllAsync(string suffix = SuffixTable)
        {
            // get all entities in the model marked with [PartitionedEntity]
            var entities = PartitionEntityScanner.GetPartitionedEntities(_dbContext);
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            foreach (var entityType in entities)
            {
                var clrType = entityType.ClrType;

                // get the prop marked with [PartitionKey] and its settings
                var partitionKeyProp = PartitionMetadataHelper.GetPartitionKeyProperty(clrType);
                var settings = PartitionMetadataHelper.GetPartitionKeySettings(clrType);

                if (partitionKeyProp == null || settings == null)
                {
                    continue;
                }

                // build dynamic predicate (eg., p => p.CreatedAt < DateTime.Now.AddDays(-90))
                var predicate = BuildPredicate(clrType, partitionKeyProp, settings);
                if (predicate == null)
                {
                    continue;
                }

                // create dynamically EntityArchiver<T> for the current entity
                var archiverType = typeof(EntityArchiver<>).MakeGenericType(clrType);
                var archiver = Activator.CreateInstance(archiverType, _dbContext);

                // call the ArchiveAsync method on the archiver
                var method = archiverType.GetMethod("ArchiveAsync");
                if (method != null)
                {
                    var task = (Task)method.Invoke(archiver, new object[] { predicate, suffix })!;
                    await task;
                }
            }
            await transaction.CommitAsync();

        }

        /// <summary>
        /// dinamically builds a predicate to filter entities based on the [PartitionKey] attribute settings
        ///  Possibile values are:
        /// - ThresholdDays (for DateTime properties)
        /// - EqualTo (for int, string, enum, etc.)
        /// </summary>
        /// <param name="clrType">CLR type of the entity eg. typeof(Person) </param>
        /// <param name="keyProp">prop marked with [PartitionKey]</param>
        /// <param name="settings">the settings ThresholdDays, EqualTo</param>
        /// <returns>lambda expression or null if not valid</returns>
        private static object? BuildPredicate(Type clrType, PropertyInfo keyProp, PartitionKeyAttribute settings)
        {

            var parameter = Expression.Parameter(clrType, "x"); // this create parameter of predicate eg. p => 
            var property = Expression.Property(parameter, keyProp); // create property access:eg. p.PartitionKe

            Expression? body = null;

            // case ThresholdDays: for datetime partitioning
            if (settings.ThresholdDays > 0 && keyProp.PropertyType == typeof(DateTime))
            {
                var threshold = DateTime.UtcNow.AddDays(-settings.ThresholdDays);
                var constant = Expression.Constant(threshold);
                body = Expression.LessThan(property, constant);
            }
            // casr EqualTo for match exact value
            else if (settings.EqualTo is not null)
            {
                var targetValue = Convert.ChangeType(settings.EqualTo, keyProp.PropertyType);
                var constant = Expression.Constant(targetValue, keyProp.PropertyType);
                body = Expression.Equal(property, constant);
            }

            if (body == null)
            {
                return null;
            }
            // Build: x => x.Property (operator) Value eg. x.Status == 3
            var lambaType = typeof(Func<,>).MakeGenericType(clrType, typeof(bool));
            return Expression.Lambda(lambaType, body, parameter).Compile();
        }
    }
}
