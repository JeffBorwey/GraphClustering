/*
 * Jeffrey K Borwey
 * CS490 - LLoyds algorithm
 * 4/8/2014
 * Each cluster has a location and a set of points associated with it
 */

using System;
using System.Collections.Generic;
using System.Text;
using NetMining.Data;
namespace NetMining.ClusteringAlgo
{
    public class Cluster
    {
        public int ClusterId;
        public List<ClusteredItem> Points;

        /// <summary>
        /// Creates a Cluster using a range of values 
        /// </summary>
        /// <param name="clusterId">Id of the cluster</param>
        /// <param name="ids">List of Item ids</param>
        public Cluster(int clusterId, List<int> ids)
        {
            ClusterId = clusterId;
            Points = new List<ClusteredItem>();

            foreach (int i in ids) 
                AddPoint(p: new ClusteredItem(i));
        }

        public Cluster(int id)
        {
            ClusterId = id;
            Points = new List<ClusteredItem>();
        }

        public void AddPoint(ClusteredItem p)
        {
            Points.Add(p);
            p.ClusterId = ClusterId;
        }

        public void clearPoints()
        {
            Points.Clear();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Cluster {0}:", ClusterId);
            sb.AppendLine();
            sb.Append("     {");
            for (int p = 0; p < Points.Count; p++)
            {
                sb.Append(Points[p].Id);
                if (p != Points.Count - 1)
                    sb.Append(", ");
            }
            sb.Append("}");

            return sb.ToString();
        }
    }
}
