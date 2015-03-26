using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Data
{
    public abstract class AbstractDataset
    {
        //stores the type of data
        public readonly DataType Type;

        protected AbstractDataset(DataType type)
        {
            Type = type;
        }

        /// <summary>
        /// Abstract Count property
        /// </summary>
        public abstract int Count { get; }

        public enum DataType
        {
            PointSet,
            DistanceMatrix,
            Graph
        };
    }
}
