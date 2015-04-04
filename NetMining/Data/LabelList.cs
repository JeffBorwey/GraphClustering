using System;
using System.Collections.Generic;
using NetMining.ClusteringAlgo;
using NetMining.Files;

namespace NetMining.Data
{
    public class LabelList
    {
        public readonly int[] LabelIndices;
        public readonly List<String> UniqueLabels;

        public LabelList(DelimitedFile f, int col) : this(f.GetColumn(col))
        {
            
        }

        public LabelList(String[] labels)
        {
            LabelIndices = new int[labels.Length];
            UniqueLabels = new List<string>();

            for (int i = 0; i < labels.Length; i++)
            {
                if (!UniqueLabels.Contains(labels[i]))
                {
                    UniqueLabels.Add(labels[i]);
                }
                LabelIndices[i] = UniqueLabels.IndexOf(labels[i]);
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
                    int actualMatching = LabelIndices[k.Id];
                    int foundMatching = k.ClusterId;
                    clusterMatching[actualMatching, foundMatching]++;
                }
            }
            return clusterMatching;
        }
    }
}
