
using System;
using System.Text;
using NetMining.ClusteringAlgo;
using NetMining.Data;

namespace NetMining.Evaluation
{
    public static class ExternalEval
    {
        public static String GreedyErrorEval(Partition clusterFile, LabelList labels, int[,] clusterMatching)
        {
            int truthCount = labels.UniqueLabels.Count;
            int partitionCount = clusterFile.Clusters.Count;
            int[] assigned = new int[truthCount];
            bool[] ourCIsAssigned = new bool[partitionCount];
            for (int i = 0; i < partitionCount; i++)
                ourCIsAssigned[i] = false;

            int sumCorrect = 0;
            StringBuilder sb = new StringBuilder();

            //for each real cluster, assign the best of our clusters that hasn't
            //already been assigned
            for (int realC = 0; realC < truthCount; realC++)
            {
                int assignedClust = 0;
                for (int ourC = 0; ourC < partitionCount; ourC++)
                {
                    int num = clusterMatching[realC, ourC];
                    if (assigned[realC] < num && !ourCIsAssigned[ourC])
                    {
                        assigned[realC] = num;
                        assignedClust = ourC;
                    }
                }

                int sumRealC = 0;
                for (int i = 0; i < partitionCount; i++)
                    sumRealC += clusterMatching[realC, i];

                if (assigned[realC] == 0)
                {
                    sb.AppendFormat("Label {0} was not assigned", labels.UniqueLabels[realC]);
                }
                else
                {
                    ourCIsAssigned[assignedClust] = true;
                    sb.AppendFormat("Cluster {0} Assigned to Label {1} Accuracy: ({2}/{3}) {4}%",
                        assignedClust, labels.UniqueLabels[realC], assigned[realC], sumRealC, 100.0 * (double)assigned[realC] / (double)sumRealC);
                }

                sb.AppendLine();
                sumCorrect += assigned[realC];
            }

            sb.AppendFormat("Total Accuracy: ({0}/{1}) {2}%", sumCorrect, clusterFile.DataCount, 100.0 * (double)sumCorrect / (double)clusterFile.DataCount);
            sb.AppendLine();
            return sb.ToString();
        }

        private static String OptimalErrorEval(Partition clusterFile, LabelList labels, int[,] clusterMatching)
        {
            return "To Be Implemented";
        }
    }
}
