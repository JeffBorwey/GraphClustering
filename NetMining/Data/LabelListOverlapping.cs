using System;
using System.Collections.Generic;
using NetMining.ClusteringAlgo;
using NetMining.Files;

namespace NetMining.Data
{
    public class LabelListOverlapping
    {
        public readonly List<List<int>> LabelIndices;
        public readonly List<String> UniqueLabels;

        /// <summary>
        /// Creates a LabelList
        /// </summary>
        /// <param name="f">File holding the label columns</param>
        /// <param name="col">0-Based index of the label column</param>
        public LabelListOverlapping(DelimitedFile f)
        {
            LabelIndices = new List<List<int>>();
            UniqueLabels = new List<string>();
            for (int i = 0; i < f.Data.Count; i++)
            {
                List<int> sublist = new List<int>();
                for (int j = 1; j < f.Data[i].Length; j++)
                {
                    if (!UniqueLabels.Contains(f.Data[i][j]))
                    {
                        UniqueLabels.Add(f.Data[i][j]);
                    }
                    sublist.Add(UniqueLabels.IndexOf(f.Data[i][j]));
                }
                LabelIndices.Add(sublist);
            }
        }

        

        public int[,] GetMatching(Partition p)
        {
            //create a count mapping
            //[actual cluster label, number in found clusters]
            int[,] clusterMatching = new int[UniqueLabels.Count, p.Clusters.Count];
            foreach (Cluster c in p.Clusters)
            {
                foreach (ClusteredItem k in c.Points)
                {
                    foreach (int m in LabelIndices[k.Id])
                    {
                        int actualMatching = m;
                        int foundMatching = k.ClusterId;
                        clusterMatching[actualMatching, foundMatching]++;
                    }
                }
            }
            return clusterMatching;
        }
    }
}
