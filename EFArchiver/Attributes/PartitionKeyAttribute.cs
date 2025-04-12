using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFArchiver.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PartitionKeyAttribute : Attribute
    {
        // if the property is a DateTime define the threshoold (eg. older then 365 days)
        public int? ThresholdDays { get; set; }
        // if the property is int, bool, enum use this to filter on a specific value
        public object? EqualTo { get; set; }
        public PartitionKeyAttribute()
        {
            
        }
    }
}
