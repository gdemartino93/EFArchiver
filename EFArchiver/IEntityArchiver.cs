using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFArchiver
{
    public interface IEntityArchiver<T> where T : class
    {
        /// <summary>
        /// Moves elements that satisfy a condition from main table to "_Storage" table
        /// </summary>
        Task ArchiveAsync(Func<T, bool> predicate, string suffixStorageTable);
    }
}
