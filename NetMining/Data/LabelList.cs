using System;
using System.Collections.Generic;

namespace NetMining.Data
{
    public class LabelList
    {
        public readonly int[] LabelIndices;
        public readonly List<String> UniqueLabels; 
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
    }
}
