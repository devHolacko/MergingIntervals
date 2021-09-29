using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingIntervalsProblem.Models
{
    public class MergeItem
    {
        public TimeSpan ArrivalTime { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string Action { get; set; }
    }
}
