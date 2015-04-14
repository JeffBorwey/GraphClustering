using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Utility
{
    public class Histogram
    {
        public int[] binnedCount;
        readonly double min = double.MaxValue;
        readonly double max = double.MinValue;
        public Histogram (List<double> data, int numBins)
        {
            binnedCount = new int[numBins];
            
            foreach (var d in data)
            {
                min = Math.Min(min, d);
                max = Math.Max(max, d);
            }

            double range = max - min;
            double stepSize = range / (double)numBins;
            foreach (var d in data)
            {
                double fStep = (d - min) / stepSize;
                int bin = (int)fStep;
                if (bin == binnedCount.Length)
                    --bin;
                binnedCount[bin]++;
            }

        }

        public void SaveHistogram(String file)
        {
            using (var sw = new System.IO.StreamWriter(file))
            {
                sw.WriteLine("Max:\t{0}\tMin:\t{1}", max, min);
                for (int i = 0; i < binnedCount.Length; i++)
                {
                    sw.WriteLine(binnedCount[i]);
                }
            }
        }
    }
}
