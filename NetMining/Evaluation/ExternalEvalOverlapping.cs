
using System;
using System.Collections.Generic;
using System.Text;
using NetMining.ClusteringAlgo;
using NetMining.Data;
using NetMining.Files;
using System.IO;
using System.Linq;

namespace NetMining.Evaluation
{
    public class ExternalEvalOverlapping
    {
        public double TotalAccuracy;
        public int TotalMatched;
        public int TotalSize;
        public List<GroundTruthMatch> Matches;
        public readonly Partition P;
        public readonly LabelListOverlapping L;
        public String TextResults;
        public String ShorterTextResults;
        public String NoNoiseTextResults;
        public class GroundTruthMatch
        {
            public readonly int PartitionClusterId;
            public readonly int GroundTruthClusterId;
            public readonly String GroundTruthLabel;
            public readonly int NumMatched;
            public readonly int TotalSize;
            public readonly double Accuracy;

            public GroundTruthMatch(int partId, int gtId, int nMatched, int size, double accuracy, String label)
            {
                PartitionClusterId = partId;
                GroundTruthClusterId = gtId;
                GroundTruthLabel = label;
                NumMatched = nMatched;
                TotalSize = size;
                Accuracy = accuracy;
            }
        }

        public ExternalEvalOverlapping(Partition clusterFile, LabelListOverlapping labels)
        {
            P = clusterFile;
            L = labels;
            Matches = new List<GroundTruthMatch>();
            int[,] clusterMatching = labels.GetMatching(clusterFile);
            GreedyErrorEval(clusterMatching);
        }
        private void GreedyErrorEval(int[,] clusterMatching)
        {
            // first, figure out how many overlapping nodes there are
            int numOverlapNodes = 0;
            for (int i = 0; i < L.LabelIndices.Count; i++)
            {
                if (L.LabelIndices[i].Count > 1)
                {
                    numOverlapNodes++;
                }
            }

            // second, make an array of how many nodes are assigned to a cluster
            List<int>[] assignmentsN = new List<int>[L.LabelIndices.Count];
            for (int i = 0; i < assignmentsN.Length; i++)
            {
                assignmentsN[i] = new List<int>();
            }

            for (int cluster = 0; cluster < P.Clusters.Count; cluster++)
            {
                for (int j = 0; j < P.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = P.Clusters[cluster].Points[j].ClusterId;
                    int id = P.Clusters[cluster].Points[j].Id;
                    assignmentsN[id].Add(clusterid);
                }
            }
            // If a node is assigned to more than 1 cluster (assignmentsN[i].count > 1
            // and it is L.label.indices[i].count > 1, it is a node discovered
            int numOverlapNodesDiscovered = 0;
            for (int i = 0; i < L.LabelIndices.Count; i++)
            {
                if (L.LabelIndices[i].Count > 1 && assignmentsN[i].Count > 1)
                {
                    numOverlapNodesDiscovered++;
                }
            }

            int truthCount = L.UniqueLabels.Count;
            int partitionCount = P.Clusters.Count;
            int[] assigned = new int[truthCount];
            bool[] ourCIsAssigned = new bool[partitionCount];
            for (int i = 0; i < partitionCount; i++)
                ourCIsAssigned[i] = false;

            KeyValuePair<int, int>[] gtBySize = new KeyValuePair<int, int>[truthCount];
            int[] sums = new int[truthCount];
            //for (int gt = 0; gt < truthCount; gt++)
            // {
            //     int labelSize = 0;
            //    for (int i = 0; i < partitionCount; i++)
            //       labelSize += clusterMatching[gt, i];
            //   gtBySize[gt] = new KeyValuePair<int, int>(gt, labelSize);
            //}

            for (int i = 0; i < L.LabelIndices.Count; i++)
            {
                for (int j = 0; j < L.LabelIndices[i].Count; j++)
                {
                    sums[L.LabelIndices[i][j]]++;
                }
            }
            for (int gt = 0; gt < truthCount; gt++)
            {
                gtBySize[gt] = new KeyValuePair<int, int>(gt, sums[gt]);
            }
            //Sort descending by size
            // THIS LINE MAKES IT NOT WORK FOR K8 GRAPHS WITH NOISE!!
            // trying the if statement which will only sort for k=2,4 graphs
            // if (L.LabelIndices.GetLength(0) < 1000)
            // {
            Array.Sort(gtBySize, (x, y) => y.Value.CompareTo(x.Value));
            // }
            int sumCorrect = 0;
            int noNoiseSumCorrect = 0;
            StringBuilder sb = new StringBuilder();
            StringBuilder ssb = new StringBuilder();
            StringBuilder nonoisesb = new StringBuilder();
            int totalC = 0;
            int noNoiseC = 0;
            int totalOurClusterSize = 0;
            //for each real cluster, assign the best of our clusters that hasn't
            //already been assigned
            for (int gtIndex = 0; gtIndex < truthCount; gtIndex++)
            {


                int realC = gtBySize[gtIndex].Key;
                int assignedClust = 0;
                for (int ourC = 0; ourC < partitionCount; ourC++)
                {
                    int num = clusterMatching[realC, ourC];
                    if (assigned[realC] < num && !ourCIsAssigned[ourC])  //CHANGED THIS LINE!!
                                                                         //if (assigned[realC] < num) ;//&& !ourCIsAssigned[ourC])
                    {
                        assigned[realC] = num;
                        assignedClust = ourC;
                    }
                }

                int sumRealC = sums[realC];
                //int sumRealC = 0;
                //for (int i = 0; i < partitionCount; i++)
                //    sumRealC += clusterMatching[realC, i];

                int ourclustersize = P.Clusters[assignedClust].Points.Count;

                if (assigned[realC] == 0)
                {
                    Matches.Add(new GroundTruthMatch(-1, realC, 0, sumRealC, 0, L.UniqueLabels[realC]));
                    sb.AppendFormat("Label {0} was not assigned", L.UniqueLabels[realC]);
                }
                else
                {
                    ourCIsAssigned[assignedClust] = true;  // comment out to assign label to any cluster
                    sb.AppendFormat("Cluster {0} Assigned to Label {1} Accuracy: ({2}/{3} /{4}) {5}%",
                        assignedClust, L.UniqueLabels[realC], assigned[realC], sumRealC, ourclustersize, 100.0 * (double)assigned[realC] / (double)sumRealC);
                    Matches.Add(new GroundTruthMatch(assignedClust, realC, assigned[realC], sumRealC, (double)assigned[realC] / sumRealC, L.UniqueLabels[realC]));
                    if (!L.UniqueLabels[realC].Equals("NA"))
                    {
                        //noNoiseC += sumRealC;
                    }
                    totalC += sumRealC;
                    totalOurClusterSize += ourclustersize;
                }

                sb.AppendLine();
                sumCorrect += assigned[realC];
                if (!L.UniqueLabels[realC].Equals("NA"))
                {
                    noNoiseSumCorrect += assigned[realC];
                }

            }
            TotalMatched = sumCorrect;
            TotalSize = P.DataCount;
            TotalAccuracy = (double)sumCorrect / P.DataCount;

            for (int q = 0; q < gtBySize.Length - 1; q++)
            {
                noNoiseC += gtBySize[q].Value;
            }

            sb.AppendFormat("Total Accuracy: ({0}/{1}) {2}%", sumCorrect, P.DataCount, 100.0 * (double)sumCorrect / (double)P.DataCount);
            ssb.AppendFormat("({0}/{1}), {2}%", sumCorrect, P.DataCount, 100.0 * (double)sumCorrect / (double)P.DataCount);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("Rev Accuracy:   ({0}/{1}  /{2}) {3}%  Incorrect:{4}  Over:{5}  OverlapDiscovery {6} of {7}", sumCorrect,
                totalC, totalOurClusterSize, 100.0 * (double)sumCorrect / (double)totalC, totalC - sumCorrect, totalOurClusterSize - totalC, numOverlapNodesDiscovered, numOverlapNodes);
            sb.AppendLine();
            sb.AppendFormat("NoNoise Accuracy:   ({0}/{1}) {2}%", noNoiseSumCorrect, noNoiseC, 100.0 * (double)noNoiseSumCorrect / (double)noNoiseC);
            nonoisesb.AppendFormat("({0}/{1}), {2}%", noNoiseSumCorrect, noNoiseC, 100.0 * (double)noNoiseSumCorrect / (double)noNoiseC);
            sb.AppendLine();
            TextResults = sb.ToString();
            ShorterTextResults = ssb.ToString();
            NoNoiseTextResults = nonoisesb.ToString();
        }

        private static String OptimalErrorEval(Partition clusterFile, LabelList labels, int[,] clusterMatching)
        {
            return "To Be Implemented";
        }

        public static double RandIndex(String labelFile, String clusterFileName)
        {
            //CALCULATING THE RAND INDEX

            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            int labelCol = delimitedLabelFile.Data[0].Length;
            LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            int[] assignments = new int[labels.LabelIndices.Length];

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignments[id] = clusterid;
                }
            }

            // compare two arrays, assigments and labels.LabelIndices
            int a = 0;
            int b = 0;
            for (int i = 0; i < assignments.Length; i++)
            {
                for (int j = i + 1; j < assignments.Length; j++)
                {
                    //Check for case a -> i and j are in same cluster in assignments and LabelIndices
                    if (labels.LabelIndices[i] == labels.LabelIndices[j] && assignments[i] == assignments[j])
                    {
                        a++;
                    }
                    else if (labels.LabelIndices[i] != labels.LabelIndices[j] && assignments[i] != assignments[j])
                    {
                        b++;
                    }
                }
            }

            int denominator = assignments.Length * (assignments.Length - 1) / 2;
            double randIndex = (a + b) / (double)denominator;
            //return "Group A: " + a + " Group B: " + b + " RandIndex: " + randIndex;
            return randIndex;
        }

        public static double Purity(String labelFile, String clusterFileName)
        {
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            int labelCol = delimitedLabelFile.Data[0].Length;
            LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            int[] majority = new int[clusterFile.Clusters.Count];

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                int[] assignments = new int[labels.UniqueLabels.Count];
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignments[labels.LabelIndices[id]]++;
                }
                // now find the max of assignments
                int maxAssign = 0;
                for (int k = 0; k < assignments.Length; k++)
                {
                    if (assignments[k] > maxAssign)
                    {
                        maxAssign = assignments[k];
                    }
                }
                majority[cluster] = maxAssign;
            }
            // add up majority[] and divide by number of vertices
            int total = 0;
            for (int i = 0; i < majority.Length; i++)
            {
                total += majority[i];
            }
            return (double)total / labels.LabelIndices.Length;
        }

        public static void OverlapReassignment(String labelFile, String clusterFileName, string newLabelFile, string newClusterFileName)
        {


            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            int labelCol = delimitedLabelFile.Data[0].Length;
            //LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
            LabelListOverlapping labelsO = new LabelListOverlapping(delimitedLabelFile);

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            //get the name of the graph file from the partition file
            String graphFile = "";
            using (StreamReader sr = new StreamReader(clusterFileName))
            {
                String dataString = sr.ReadLine();
                graphFile = dataString.Substring(6);
                bool foundMeta = false;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                    {
                        foundMeta = true;
                    }
                    if (foundMeta)
                    {
                        if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                        {
                            clusterFile.MetaData += line.Substring(5) + Environment.NewLine;
                        }
                        else
                        {
                            clusterFile.MetaData += (line + Environment.NewLine);
                        }

                    }

                }
            }


            //int[] assignments = new int[labels.LabelIndices.Length];
            List<int>[] assignmentsO = new List<int>[delimitedLabelFile.Data.Count];
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i] = new List<int>();
            }

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignmentsO[id].Add(clusterid);
                }
            }

            //Where assignmentsO[0].Count == 0 we want to assign that node to all adjacent clusters

            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    for (int j = 0; j < clusterFile.Graph.Nodes[i].Edge.Length; j++) // for each edge j
                    {
                        // if the adjacent node can be identified as part of a cluster, add its cluster to the list
                        // It's confusing, but it is possible that an adjacent node is also unassigned. I don't know
                        // what to do in that case, so I'm just skipping it. 
                        if (assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count != 0)
                        {
                            // for each adjacent node that has been assigned to a cluster
                            // assign the current node to all clusters adjacent to that edge
                            for (int k = 0; k < assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count; k++)
                            {
                                int clusterid = assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]][k];
                                if (!assignmentsO[i].Contains(clusterid))  // only add if not already there
                                {
                                    assignmentsO[i].Add(clusterid);
                                    // also add this node to the cluster
                                    ClusteredItem item = new ClusteredItem(i);
                                    item.ClusterId = clusterid;
                                    clusterFile.Clusters[clusterid].AddPoint(item);
                                }

                            }


                        }

                    }
                }
            }
            // at this point, if there are still unassigned clusters, we need to do something!
            // I am just assigning to cluster 0 to get it over with
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    assignmentsO[i].Add(0);
                }
            }

            //sort the clusters (just to make it tidier
            for (int i = 0; i < clusterFile.Clusters.Count; i++)
            {
                clusterFile.Clusters[i].Points.Sort();
            }

            // sort the assignments matrix (just because it's tidier)
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i].Sort();
            }



            // print the new data file
            using (StreamWriter sw = new StreamWriter(newLabelFile, false))
            {
                for (int i = 0; i < assignmentsO.Length; i++)
                {
                    sw.Write(i + " ");
                    for (int j = 0; j < assignmentsO[i].Count; j++)
                    {
                        sw.Write(assignmentsO[i][j] + " ");
                    }
                    sw.WriteLine("");
                }
            }
            // print the new parition file
            clusterFile.SavePartition(newClusterFileName, graphFile);
        }

        public static void OverlapReassignment2(String labelFile, String clusterFileName, string newLabelFile, string newClusterFileName, int numReassignments)
        {


            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            int labelCol = delimitedLabelFile.Data[0].Length;
            //LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
            LabelListOverlapping labelsO = new LabelListOverlapping(delimitedLabelFile);

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            //get the name of the graph file from the partition file
            String graphFile = "";
            using (StreamReader sr = new StreamReader(clusterFileName))
            {
                String dataString = sr.ReadLine();
                graphFile = dataString.Substring(6);
                bool foundMeta = false;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                    {
                        foundMeta = true;
                    }
                    if (foundMeta)
                    {
                        if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                        {
                            clusterFile.MetaData += line.Substring(5) + Environment.NewLine;
                        }
                        else
                        {
                            clusterFile.MetaData += (line + Environment.NewLine);
                        }

                    }

                }
            }


            //int[] assignments = new int[labels.LabelIndices.Length];
            List<int>[] assignmentsO = new List<int>[delimitedLabelFile.Data.Count];
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i] = new List<int>();
            }

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignmentsO[id].Add(clusterid);
                }
            }

            //Where assignmentsO[0].Count == 0 we want to assign that node to top *numReassignments* adjacent clusters

            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    //Console.Write(i + ":");
                    int[] clustAssgn = new int[clusterFile.Clusters.Count];
                    for (int j = 0; j < clusterFile.Graph.Nodes[i].Edge.Length; j++) // for each edge j
                    {
                        // if the adjacent node can be identified as part of a cluster, add its cluster to the list
                        // It's confusing, but it is possible that an adjacent node is also unassigned. I don't know
                        // what to do in that case, so I'm just skipping it.  

                        if (assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count != 0)
                        {
                            // for each adjacent node that has been assigned to a cluster
                            // keep an array of total edges pointing to each cluster

                            for (int k = 0; k < assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count; k++)
                            {
                                int clusterid = assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]][k];
                                clustAssgn[clusterid]++;

                            }
                        }
                    }
                    for (int q = 0; q < numReassignments; q++)
                    {
                        // find the largest cluster mass
                        int clustermax = clustAssgn[0];
                        int clusterindex = 0;
                        for (int r = 1; r < clustAssgn.Length; r++)
                        {
                            if (clustAssgn[r] > clustermax)
                            {
                                clustermax = clustAssgn[r];
                                clusterindex = r;
                            }
                        }

                        // if not already there, add the largest cluster mass
                        if (!assignmentsO[i].Contains(clusterindex))  // only add if not already there
                        {
                            assignmentsO[i].Add(clusterindex);
                            // also add this node to the cluster
                            ClusteredItem item = new ClusteredItem(i);
                            item.ClusterId = clusterindex;
                            clusterFile.Clusters[clusterindex].AddPoint(item);
                        }
                        //Console.Write(clustAssgn[clusterindex] + ",");
                        clustAssgn[clusterindex] = 0;
                    }


                }
            }
            // at this point, if there are still unassigned clusters, we need to do something!
            // I am just assigning to cluster 0 to get it over with
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    assignmentsO[i].Add(0);
                }
            }

            //sort the clusters (just to make it tidier
            for (int i = 0; i < clusterFile.Clusters.Count; i++)
            {
                clusterFile.Clusters[i].Points.Sort();
            }

            // sort the assignments matrix (just because it's tidier)
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i].Sort();
            }



            // print the new data file
            using (StreamWriter sw = new StreamWriter(newLabelFile, false))
            {
                for (int i = 0; i < assignmentsO.Length; i++)
                {
                    sw.Write(i + " ");
                    for (int j = 0; j < assignmentsO[i].Count; j++)
                    {
                        sw.Write(assignmentsO[i][j] + " ");
                    }
                    sw.WriteLine("");
                }
            }
            // print the new parition file
            clusterFile.SavePartition(newClusterFileName, graphFile);
        }

        // This method is for reassignment.  It finds the top 2 clusters that a node is assigned to.   If there is a great difference between
        // the 2 reassignment degrees, it assigns to both clusters.  
        public static void OverlapReassignment3(String labelFile, String clusterFileName, string newLabelFile, string newClusterFileName, int numReassignments)
        {

            int countTrue = 0;
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            int labelCol = delimitedLabelFile.Data[0].Length;
            //LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
            LabelListOverlapping labelsO = new LabelListOverlapping(delimitedLabelFile);

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            //get the name of the graph file from the partition file
            String graphFile = "";
            using (StreamReader sr = new StreamReader(clusterFileName))
            {
                String dataString = sr.ReadLine();
                graphFile = dataString.Substring(6);
                bool foundMeta = false;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                    {
                        foundMeta = true;
                    }
                    if (foundMeta)
                    {
                        if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                        {
                            clusterFile.MetaData += line.Substring(5) + Environment.NewLine;
                        }
                        else
                        {
                            clusterFile.MetaData += (line + Environment.NewLine);
                        }

                    }

                }
            }


            //int[] assignments = new int[labels.LabelIndices.Length];
            List<int>[] assignmentsO = new List<int>[delimitedLabelFile.Data.Count];
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i] = new List<int>();
            }

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignmentsO[id].Add(clusterid);
                }
            }

            //Where assignmentsO[0].Count == 0 we want to assign that node to top *numReassignments* adjacent clusters

            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    int[] topReassignments = new int[numReassignments];
                    int[] topReassignmentsNodes = new int[numReassignments];
                    Console.Write(i + ":");
                    int[] clustAssgn = new int[clusterFile.Clusters.Count];
                    for (int j = 0; j < clusterFile.Graph.Nodes[i].Edge.Length; j++) // for each edge j
                    {
                        // if the adjacent node can be identified as part of a cluster, add its cluster to the list
                        // It's confusing, but it is possible that an adjacent node is also unassigned. I don't know
                        // what to do in that case, so I'm just skipping it.  

                        if (assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count != 0)
                        {
                            // for each adjacent node that has been assigned to a cluster
                            // keep an array of total edges pointing to each cluster

                            for (int k = 0; k < assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count; k++)
                            {
                                int clusterid = assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]][k];
                                clustAssgn[clusterid]++;

                            }
                        }
                    }
                    for (int q = 0; q < numReassignments; q++)
                    {
                        // find the largest cluster mass
                        int clustermax = clustAssgn[0];
                        int clusterindex = 0;
                        for (int r = 1; r < clustAssgn.Length; r++)
                        {
                            if (clustAssgn[r] > clustermax)
                            {
                                clustermax = clustAssgn[r];
                                clusterindex = r;
                            }
                        }
                        topReassignments[q] = clustAssgn[clusterindex];
                        topReassignmentsNodes[q] = clusterindex;
                        clustAssgn[clusterindex] = 0;
                    }




                    for (int q = 0; q < numReassignments; q++)
                    {

                        if ((double)topReassignments[1] / topReassignments[0] < 0.30)
                        {
                            countTrue++;
                            if (q > 0) continue;
                        }
                        // if not already there, add the largest cluster mass
                        if (!assignmentsO[i].Contains(topReassignmentsNodes[q]))  // only add if not already there
                        {
                            assignmentsO[i].Add(topReassignmentsNodes[q]);
                            // also add this node to the cluster
                            ClusteredItem item = new ClusteredItem(i);
                            item.ClusterId = topReassignmentsNodes[q];
                            clusterFile.Clusters[topReassignmentsNodes[q]].AddPoint(item);
                        }
                        //Console.Write(topReassignments[q] + ",");



                    }
                    Console.WriteLine(countTrue);

                }
            }
            // at this point, if there are still unassigned clusters, we need to do something!
            // I am just assigning to cluster 0 to get it over with
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    assignmentsO[i].Add(0);
                }
            }

            //sort the clusters (just to make it tidier
            for (int i = 0; i < clusterFile.Clusters.Count; i++)
            {
                clusterFile.Clusters[i].Points.Sort();
            }

            // sort the assignments matrix (just because it's tidier)
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i].Sort();
            }



            // print the new data file
            using (StreamWriter sw = new StreamWriter(newLabelFile, false))
            {
                for (int i = 0; i < assignmentsO.Length; i++)
                {
                    sw.Write(i + " ");
                    for (int j = 0; j < assignmentsO[i].Count; j++)
                    {
                        sw.Write(assignmentsO[i][j] + " ");
                    }
                    sw.WriteLine("");
                }
            }
            // print the new parition file
            clusterFile.SavePartition(newClusterFileName, graphFile);
        }

        /// <summary>
        /// The idea here is that the file will measure variance of number of assignments to adjacent clusters.  
        /// Nodes with low variance (eg 6 adjacencies to cluster 1, 5 adjacencies to cluster 2)
        /// will be assigned to the top "numReassignments" adjacent clusters.  Nodes with 
        /// high variance in adjacencies (eg 10 adjacencies to cluster 1, 2 adjacencies to cluster 2)
        /// will be assigned to only one cluster.
        /// </summary>
        /// <param name="labelFile">A file with the format "node label" that identifies a ground truth</param>
        /// <param name="clusterFileName">One of our cluster format files</param>
        /// <param name="newLabelFile">Makes a new label file for the assigned labels, not very useful right now</param>
        /// <param name="newClusterFileName">A cluster file with the new assignments</param>
        /// <param name="numReassignments">The typical number of overlaps: eg an overlapping node overlaps *3* clusters</param>
        public static void OverlapReassignment4(String labelFile, String clusterFileName, string newLabelFile, string newClusterFileName, int numReassignments)
        {

            int countTrue = 0;
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            //int labelCol = delimitedLabelFile.Data[0].Length;
            //LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
            LabelListOverlapping labelsO = new LabelListOverlapping(delimitedLabelFile);

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            //get the name of the graph file from the partition file
            String graphFile = "";
            using (StreamReader sr = new StreamReader(clusterFileName))
            {
                String dataString = sr.ReadLine();
                graphFile = dataString.Substring(6);
                bool foundMeta = false;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                    {
                        foundMeta = true;
                    }
                    if (foundMeta)
                    {
                        if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                        {
                            clusterFile.MetaData += line.Substring(5) + Environment.NewLine;
                        }
                        else
                        {
                            clusterFile.MetaData += (line + Environment.NewLine);
                        }

                    }

                }
            }


            //int[] assignments = new int[labels.LabelIndices.Length];
            List<int>[] assignmentsO = new List<int>[delimitedLabelFile.Data.Count];
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i] = new List<int>();
            }

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignmentsO[id].Add(clusterid);
                }
            }
            // We now have assignmentsO which holds all the nodes with definite assignments
            //Where assignmentsO[0].Count == 0 we want to assign that node to top *numReassignments* adjacent clusters

            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    int[] topReassignments = new int[numReassignments];
                    int[] topReassignmentsNodestoClusters = new int[numReassignments];
                    Console.Write(i + ":");
                    int[] clustAssgn = new int[clusterFile.Clusters.Count];
                    for (int j = 0; j < clusterFile.Graph.Nodes[i].Edge.Length; j++) // for each edge j adjacent to unassigned node i
                    {
                        // if the adjacent node can be identified as part of a cluster, add its cluster to the list
                        // It's confusing, but it is possible that an adjacent node is also unassigned. I don't know
                        // what to do in that case, so I'm just skipping it.  

                        if (assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count != 0)
                        {
                            // for each adjacent node that has been assigned to a cluster
                            // keep an array of total edges pointing to each cluster
                            //CHANGE THIS BACK!
                            //      for (int k = 0; k < assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count; k++)
                            //      {
                            int clusterid = assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]][0]; // change 0 to k
                            clustAssgn[clusterid]++;

                            //       }
                        }
                    }
                    // Find the top *numReassignments* cluster adjacencies
                    // If there are 4 reassignments and 6 clusters, the lists will look something like this:
                    //     topReassignments { 34, 23, 4, 1}
                    //      topReassignmentNodestoClusters {3 2 6 1}
                    for (int q = 0; q < numReassignments; q++)
                    {
                        // find the largest cluster mass
                        int clustermax = clustAssgn[0];
                        int clusterindex = 0;
                        for (int r = 1; r < clustAssgn.Length; r++)
                        {
                            if (clustAssgn[r] > clustermax)
                            {
                                clustermax = clustAssgn[r];
                                clusterindex = r;
                            }
                        }
                        topReassignments[q] = clustAssgn[clusterindex];
                        topReassignmentsNodestoClusters[q] = clusterindex;
                        clustAssgn[clusterindex] = 0;
                    }
                    // let's normalize the topReassignments and find the standard deviation
                    double[] topReassignmentsNormalized = new double[topReassignments.Length];
                    double maxValue = topReassignments.Max();
                    for (int w = 0; w < topReassignmentsNormalized.Length; w++)
                    {
                        topReassignmentsNormalized[w] = (double)topReassignments[w] / maxValue;
                    }
                    // find standard deviation
                    double average = topReassignmentsNormalized.Average();
                    double sumOfSquaresOfDifferences = topReassignmentsNormalized.Select(val => (val - average) * (val - average)).Sum();
                    double sd = Math.Sqrt(sumOfSquaresOfDifferences / topReassignmentsNormalized.Length);

                    Console.WriteLine("sd= " + sd);
                    using (StreamWriter sw = new StreamWriter("C:\\Users\\John\\Dropbox\\ClustProject\\SyntheticLFRNets\\OverlapNets\\SF6-100\\analysisWDegree1.txt", true))
                    {
                        int isOverlapNode = 0;
                        if (labelsO.LabelIndices[i].Count > 1) isOverlapNode = 1;
                        sw.Write(i + "," + isOverlapNode + "," + clusterFile.Graph.Nodes[i].Edge.Length + ",");
                        for (int q = 0; q < topReassignments.Length; q++)
                        {
                            sw.Write(topReassignments[q] + ",");
                        }
                        sw.WriteLine();
                    }

                    for (int q = 0; q < numReassignments; q++)
                    {
                        //if ((double)topReassignments[1] / topReassignments[0] < 0.50)
                        if (sd > 0.30)
                        {
                            countTrue++;
                            if (q > 0) continue;
                        }
                        // if not already there, add the largest cluster mass
                        if (!assignmentsO[i].Contains(topReassignmentsNodestoClusters[q]))  // only add if not already there
                        {
                            assignmentsO[i].Add(topReassignmentsNodestoClusters[q]);
                            // also add this node to the cluster
                            ClusteredItem item = new ClusteredItem(i);
                            item.ClusterId = topReassignmentsNodestoClusters[q];
                            clusterFile.Clusters[topReassignmentsNodestoClusters[q]].AddPoint(item);
                        }
                        //Console.Write(topReassignments[q] + ",");



                    }
                    Console.WriteLine(countTrue);

                }
            }
            // at this point, if there are still unassigned clusters, we need to do something!
            // I am just assigning to cluster 0 to get it over with
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    assignmentsO[i].Add(0);
                }
            }

            //sort the clusters (just to make it tidier
            for (int i = 0; i < clusterFile.Clusters.Count; i++)
            {
                clusterFile.Clusters[i].Points.Sort();
            }

            // sort the assignments matrix (just because it's tidier)
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i].Sort();
            }



            // print the new data file
            using (StreamWriter sw = new StreamWriter(newLabelFile, false))
            {
                for (int i = 0; i < assignmentsO.Length; i++)
                {
                    sw.Write(i + " ");
                    for (int j = 0; j < assignmentsO[i].Count; j++)
                    {
                        sw.Write(assignmentsO[i][j] + " ");
                    }
                    sw.WriteLine("");
                }
            }
            // print the new parition file
            clusterFile.SavePartition(newClusterFileName, graphFile);
        }
        /// <summary>
        /// The idea here is that the file will measure variance of number of assignments to adjacent clusters.  
        /// Nodes with low variance (eg 6 adjacencies to cluster 1, 5 adjacencies to cluster 2)
        /// will be assigned to the top "numReassignments" adjacent clusters.  Nodes with 
        /// high variance in adjacencies (eg 10 adjacencies to cluster 1, 2 adjacencies to cluster 2)
        /// will be assigned to only one cluster.
        /// </summary>
        /// <param name="labelFile">A file with the format "node label" that identifies a ground truth</param>
        /// <param name="clusterFileName">One of our cluster format files</param>
        /// <param name="newLabelFile">Makes a new label file for the assigned labels, not very useful right now</param>
        /// <param name="newClusterFileName">A cluster file with the new assignments</param>
        /// <param name="numReassignments">The typical number of overlaps: eg an overlapping node overlaps *3* clusters</param>
        public static void OverlapReassignment5(String labelFile, String clusterFileName, string newLabelFile, string newClusterFileName, int numReassignments)
        {

            int countTrue = 0;
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            //int labelCol = delimitedLabelFile.Data[0].Length;
            //LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
            LabelListOverlapping labelsO = new LabelListOverlapping(delimitedLabelFile);

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            //get the name of the graph file from the partition file
            String graphFile = "";
            using (StreamReader sr = new StreamReader(clusterFileName))
            {
                String dataString = sr.ReadLine();
                graphFile = dataString.Substring(6);
                bool foundMeta = false;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                    {
                        foundMeta = true;
                    }
                    if (foundMeta)
                    {
                        if (line.Length > 0 && line.Substring(0, 1).Equals("M"))
                        {
                            clusterFile.MetaData += line.Substring(5) + Environment.NewLine;
                        }
                        else
                        {
                            clusterFile.MetaData += (line + Environment.NewLine);
                        }

                    }

                }
            }


            //int[] assignments = new int[labels.LabelIndices.Length];
            List<int>[] assignmentsO = new List<int>[delimitedLabelFile.Data.Count];
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i] = new List<int>();
            }

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignmentsO[id].Add(clusterid);
                }
            }
            // We now have assignmentsO which holds all the nodes with definite assignments
            //Where assignmentsO[0].Count == 0 we want to assign that node to top *numReassignments* adjacent clusters
            // first make alist of the nodes that will be assigned
            List<int> nodesToAssign = new List<int>();
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    nodesToAssign.Add(i);
                }
            }
            int iterations = 3;
            for (int z=0; z<iterations; z++) 
            //for (int i = 0; i < assignmentsO.Length; i++)
            {
                foreach (int i in nodesToAssign)
                //if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    int[] topReassignments = new int[numReassignments];
                    int[] topReassignmentsNodestoClusters = new int[numReassignments];
                    Console.Write(i + ":");
                    int[] clustAssgn = new int[clusterFile.Clusters.Count];
                    for (int j = 0; j < clusterFile.Graph.Nodes[i].Edge.Length; j++) // for each edge j adjacent to unassigned node i
                    {
                        // if the adjacent node can be identified as part of a cluster, add its cluster to the list
                        // It's confusing, but it is possible that an adjacent node is also unassigned. I don't know
                        // what to do in that case, so I'm just skipping it.  

                        if (assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count != 0)
                        {
                            // for each adjacent node that has been assigned to a cluster
                            // keep an array of total edges pointing to each cluster
                            //CHANGE THIS BACK!
                            //      for (int k = 0; k < assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]].Count; k++)
                            //      {
                            int clusterid = assignmentsO[clusterFile.Graph.Nodes[i].Edge[j]][0]; // change 0 to k
                            clustAssgn[clusterid]++;

                            //       }
                        }
                    }
                    // Find the top *numReassignments* cluster adjacencies
                    // If there are 4 reassignments and 6 clusters, the lists will look something like this:
                    //     topReassignments { 34, 23, 4, 1}
                    //      topReassignmentNodestoClusters {3 2 6 1}
                    for (int q = 0; q < numReassignments; q++)
                    {
                        // find the largest cluster mass
                        int clustermax = clustAssgn[0];
                        int clusterindex = 0;
                        for (int r = 1; r < clustAssgn.Length; r++)
                        {
                            if (clustAssgn[r] > clustermax)
                            {
                                clustermax = clustAssgn[r];
                                clusterindex = r;
                            }
                        }
                        topReassignments[q] = clustAssgn[clusterindex];
                        topReassignmentsNodestoClusters[q] = clusterindex;
                        clustAssgn[clusterindex] = 0;
                    }
                    // let's normalize the topReassignments and find the standard deviation
                    double[] topReassignmentsNormalized = new double[topReassignments.Length];
                    double maxValue = topReassignments.Max();
                    for (int w = 0; w < topReassignmentsNormalized.Length; w++)
                    {
                        topReassignmentsNormalized[w] = (double)topReassignments[w] / maxValue;
                    }
                    // find standard deviation
                    double average = topReassignmentsNormalized.Average();
                    double sumOfSquaresOfDifferences = topReassignmentsNormalized.Select(val => (val - average) * (val - average)).Sum();
                    double sd = Math.Sqrt(sumOfSquaresOfDifferences / topReassignmentsNormalized.Length);

                    //Console.WriteLine("sd= " + sd);

                    /* if (z == iterations - 1)
                    { 
                    using (StreamWriter sw = new StreamWriter("C:\\Users\\John\\Dropbox\\ClustProject\\SyntheticLFRNets\\OverlapNets\\GNDegree16\\GN8ClustersOn32Om6mix0625\\analysisWDegree.txt", true))
                    {
                        int isOverlapNode = 0;
                        if (labelsO.LabelIndices[i].Count > 1) isOverlapNode = 1;
                        sw.Write(i + "," + isOverlapNode + "," + clusterFile.Graph.Nodes[i].Edge.Length + ",");
                        for (int q = 0; q < topReassignments.Length; q++)
                        {
                            sw.Write(topReassignments[q] + ",");
                        }
                            //sw.Write(sd + ",");
                            sw.WriteLine();
                    }
                }
                    */
                    for (int q = 0; q < numReassignments; q++)
                    {
                        //if ((double)topReassignments[1] / topReassignments[0] < 0.50)
                        if (sd > 0.30)
                        {
                            countTrue++;
                            if (q > 0) continue;
                        }
                        // if not already there, add the largest cluster mass
                        if (!assignmentsO[i].Contains(topReassignmentsNodestoClusters[q]))  // only add if not already there
                        {
                            assignmentsO[i].Add(topReassignmentsNodestoClusters[q]);
                            // also add this node to the cluster
                            ClusteredItem item = new ClusteredItem(i);
                            item.ClusterId = topReassignmentsNodestoClusters[q];
                            clusterFile.Clusters[topReassignmentsNodestoClusters[q]].AddPoint(item);
                        }
                        //Console.Write(topReassignments[q] + ",");



                    }
                    Console.WriteLine(countTrue);

                }
            }
            // at this point, if there are still unassigned clusters, we need to do something!
            // I am just assigning to cluster 0 to get it over with
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                if (assignmentsO[i].Count == 0) // for each unassigned node i
                {
                    assignmentsO[i].Add(0);
                }
            }

            //sort the clusters (just to make it tidier
            for (int i = 0; i < clusterFile.Clusters.Count; i++)
            {
                clusterFile.Clusters[i].Points.Sort();
            }

            // sort the assignments matrix (just because it's tidier)
            for (int i = 0; i < assignmentsO.Length; i++)
            {
                assignmentsO[i].Sort();
            }



            // print the new data file
            using (StreamWriter sw = new StreamWriter(newLabelFile, false))
            {
                for (int i = 0; i < assignmentsO.Length; i++)
                {
                    sw.Write(i + " ");
                    for (int j = 0; j < assignmentsO[i].Count; j++)
                    {
                        sw.Write(assignmentsO[i][j] + " ");
                    }
                    sw.WriteLine("");
                }
            }
            // print the new parition file
            clusterFile.SavePartition(newClusterFileName, graphFile);
        }

        public static void convertForOnmi(String labelFile, String clusterFileName)
        {
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            LabelListOverlapping labelsO = new LabelListOverlapping(delimitedLabelFile);

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName+".cluster");
            // we can just write out the clusterfile
            using (StreamWriter sw = new StreamWriter(clusterFileName+".myC", false))
            {
                for (int i = 0; i < clusterFile.Clusters.Count; i++)
                {
                    for (int j = 0; j < clusterFile.Clusters[i].Points.Count; j++)
                    {
                        sw.Write(clusterFile.Clusters[i].Points[j].Id + " ");
                    }
                    sw.WriteLine("");
                }
            }
            // make a new data structure to hold the clusters
            List<List<int>> gt = new List<List<int>>();
            for (int s=0; s< clusterFile.Clusters.Count; s++)
            {
                gt.Add(new List<int>());
            }
            for (int i=0; i<labelsO.LabelIndices.Count; i++)
            {
                for(int j=0; j<labelsO.LabelIndices[i].Count; j++)
                {
                    gt[labelsO.LabelIndices[i][j]].Add(i);
                }
            }
            using (StreamWriter sw = new StreamWriter(clusterFileName + ".gt", false))
            {
                for (int i = 0; i < gt.Count; i++)
                {
                    for (int j = 0; j < gt[i].Count; j++)
                    {
                        sw.Write(gt[i][j] + " ");
                    }
                    sw.WriteLine("");
                }
            }

        }
        
    }
}
