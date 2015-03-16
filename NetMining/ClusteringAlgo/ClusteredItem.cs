using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.ClusteringAlgo
{
    /// <summary>
    /// DataPoint holds an Id(relating it to data) and a ClusterID(relating it to a cluster)
    /// </summary>
    public class ClusteredItem
    {
        public int Id;
        public int ClusterId;
        public ClusteredItem(int id)
        {
            Id = id;
            ClusterId = -1;
        }
    }
}
