using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NetMining.Data;
using NetMining.Graphs;

namespace NetMining.ClusteringAlgo
{
    /// <summary>
    /// This class contains a list of Clusters produced by a clustering algorithm
    /// </summary>
    public class Partition
    {
        public List<Cluster> Clusters;
        public String MetaData = "";

        //Data associated with partition
        public AbstractDataset Data;

        public PointSet Points
        {
            get
            {
                if (Data.Type == AbstractDataset.DataType.PointSet)
                    return (PointSet) Data;
                return null;
            } 
        }

        public DistanceMatrix Distances
        {
            get
            {
                if (Data.Type == AbstractDataset.DataType.DistanceMatrix)
                    return (DistanceMatrix)Data;
                return null;
            }
        }

        public LightWeightGraph Graph
        {
            get
            {
                if (Data.Type == AbstractDataset.DataType.Graph)
                    return (LightWeightGraph)Data;
                return null;
            }
        }

        /// <summary>
        /// Merges a sub-partition into the list of clusters
        /// </summary>
        /// <param name="partition">Partition to be merged</param>
        /// <param name="dataItemMap">This is a map from the given partion indexes to this
        /// Partitions indexes</param>
        /// <param name="clusterId">ID of the cluster to replace</param>
        public void MergeSubPartition(Partition partition, int[] dataItemMap, int clusterId)
        {
            Boolean noneAdded = true;

            //Setup new list of clusters by adding or merging
            foreach (Cluster c in partition.Clusters)
            {
                int newClustId = (noneAdded) ? clusterId : Clusters.Count;

                foreach (var i in c.Points) 
                {
                    i.ClusterId = newClustId;
                    i.Id = dataItemMap[i.Id];
                }

                c.ClusterId = newClustId;

                if (noneAdded)
                    Clusters[newClustId] = c;
                else
                    Clusters.Add(c);
                    

                noneAdded = false;
            }
        }


        /// <summary>
        /// This Will count the number of items in each cluster
        /// </summary>
        /// <returns>Returns the number of items assigned to clusters</returns>
        public int GetClusteredItemCount()
        {
            return Clusters.Sum(c => c.Points.Count);
        }

        /// <summary>
        /// This will save the partitioning into the .cluster file format
        /// </summary>
        /// <param name="saveLocation">The path to the file</param>
        /// <param name="dataFile">The path to the dataFile used</param>
        /// <param name="metaData">This is additional information to append to the end
        /// of the file</param>
        public void SavePartition(String saveLocation, String dataFile)
        {
            //Assumes .Cluster endinge
            using (StreamWriter sw = new StreamWriter(saveLocation))
            {
                if (Data.Type == AbstractDataset.DataType.PointSet)
                    sw.WriteLine("Points {0}", dataFile);
                else if (Data.Type == AbstractDataset.DataType.DistanceMatrix)
                    sw.WriteLine("DistanceMatrix {0}", dataFile);
                else if (Data.Type == AbstractDataset.DataType.Graph)
                    sw.WriteLine("Graph {0}", dataFile);

                sw.WriteLine("Clusters {0}", Clusters.Count);
                foreach (Cluster c in Clusters)
                {
                    sw.WriteLine(c.Points.Count);
                    foreach (ClusteredItem p in c.Points)
                        sw.Write("{0} ", p.Id);
                    sw.WriteLine();
                }
                sw.WriteLine("Meta {0}", MetaData);
            }
        }

        /// <summary>
        /// This will save the partitioning into the .cluster file format
        /// </summary>
        /// <param name="saveLocation">The path to the file</param>
        /// <param name="dataFile">The path to the dataFile used</param>
        /// <param name="metaData">This is additional information to append to the end
        /// of the file</param>
        public void SavePartition2(String saveLocation, String dataFile)
        {
            //Assumes .Cluster endinge
            using (StreamWriter sw = new StreamWriter(saveLocation))
            {
                //if (Data.Type == AbstractDataset.DataType.PointSet)
                //    sw.WriteLine("Points {0}", dataFile);
                //else if (Data.Type == AbstractDataset.DataType.DistanceMatrix)
                //    sw.WriteLine("DistanceMatrix {0}", dataFile);
                //else if (Data.Type == AbstractDataset.DataType.Graph)
                //    sw.WriteLine("Graph {0}", dataFile);

                //sw.WriteLine("Clusters {0}", Clusters.Count);

                String[] assignments = new String[((LightWeightGraph)Data).Nodes.Count()];
                foreach (Cluster c in Clusters)
                {
                    foreach (ClusteredItem p in c.Points)
                    {
                        assignments[p.Id] = p.Id + ", " + ((LightWeightGraph)Data).Nodes[p.Id].sharedName + ", " + c.ClusterId;
                    }

                }
                // we need to put in the nodes that have not been assigned
                for (int i = 0; i < assignments.Length; i++)
                {
                    if (assignments[i] == null)
                    {
                        assignments[i] = i + ", " + ((LightWeightGraph)Data).Nodes[i].sharedName + ", NA";
                    }
                }

                for (int i = 0; i < assignments.Count(); i++)
                {

                    sw.WriteLine(assignments[i]);

                    //foreach (ClusteredItem p in c.Points)
                    //    sw.Write("{0} ", p.Id);
                    //sw.WriteLine();
                    //sw.WriteLine("Meta {0}", MetaData);
                }

            }
        }

        /// <summary>
        /// Creates a Dataset partition
        /// </summary>
        /// <param name="clusters">List of clusters</param>
        /// <param name="data">The dataset used to create the partition</param>
        /// <param name="m">Meta Data about the partition</param>
        public Partition(List<Cluster> clusters, AbstractDataset data, String m = "")
        {
            Clusters = clusters;
            MetaData = m;
            Data = data;
        }

        /// <summary>
        /// Reads a .cluster file into a partition
        /// </summary>
        /// <param name="filename"></param>
        public Partition(String filename)
        {
            Clusters = new List<Cluster>();

            using (StreamReader sr = new StreamReader(filename))
            {
                String dataString = sr.ReadLine();
                String dataType = dataString.Substring(0, dataString.IndexOf(' '));
                String dataFileName = dataString.Substring(dataString.IndexOf(' ') + 1);
                String folder = filename.Substring(0, filename.LastIndexOf('\\'));
                
                //Get the DataPoints
                switch (dataType)
                {
                    case "Points":
                        Data = new PointSet(dataFileName);
                        break;
                    case "DistanceMatrix":
                        Data = new DistanceMatrix(dataFileName);
                        break;
                    case "Graph":
                        String extension = dataFileName.Substring(dataFileName.LastIndexOf('.') + 1);
                        if (extension == "gml")
                            Data = LightWeightGraph.GetGraphFromGML(dataFileName);
                        else if (extension == "graph")
                            Data = LightWeightGraph.GetGraphFromFile(dataFileName);
                        break;
                    default:
                        throw new InvalidDataException("dataType");
                }
                

                //Parse the Clusters
                String line = sr.ReadLine();
                int numClusters = int.Parse(line.Split(' ')[1]);

                for (int i = 0; i < numClusters; i++)
                {
                    Cluster C = new Cluster(i);
                    int numItems = int.Parse(sr.ReadLine());
                    line = sr.ReadLine();
                    String[] split = line.Split(' ');
                    for (int k = 0; k < numItems; k++)
                    {
                        int pointIndex = int.Parse(split[k]);
                        C.AddPoint(new ClusteredItem(pointIndex));
                    }
                    Clusters.Add(C);
                }
            }
        }


        public KPoint[] GetClusterKPoints(int index)
        {
            KPoint[] myPoints = Clusters[index].Points.Select(p => Points[p.Id]).ToArray();
            //return Clusters[index].Points.Select(p => Points[p.Id]).ToArray();
            return myPoints;
        }
        public int DataCount 
        {
            get { return Data.Count; }
        }


        public static Partition GetPartition(LightWeightGraph lwg)
        {
            //Get our cluster Assignment
            List<List<int>> componentList = lwg.GetComponents();

            //Setup our Clusters
            List<Cluster> clusterList = new List<Cluster>();
            for (int i = 0; i < componentList.Count; i++)
            {
                Cluster c = new Cluster(i);
                foreach (var n in componentList[i])
                {
                    c.AddPoint(new ClusteredItem(lwg[n].Label));
                }
                clusterList.Add(c);
            }

            
            return new Partition(clusterList, lwg);
        }
        public Dictionary<String, Dictionary<int, String>> getNodeDescriptors()
        {
            Dictionary<String, Dictionary<int, String>> nodeDescriptors = new Dictionary<String, Dictionary<int, String>>();
            Dictionary<int, String> newNodes = new Dictionary<int, String>();
            for (int i=0; i< Clusters.Count; i++)
            {
                for (int j=0; j< Clusters[i].Points.Count; j++)
                {
                    
                    newNodes.Add(Clusters[i].Points[j].Id, Clusters[i].Points[j].ClusterId + "");
                }
            }
            nodeDescriptors.Add("label", newNodes);
            return nodeDescriptors;
        }

        public static void combineClusters(String saveLocation, String clusterfileName, int minK)
        {

            //get the Partion file
            Partition partition = new Partition(saveLocation + clusterfileName + ".cluster");
            // we want to do (partition.Clusters.count - minK) merges
            int startPartitions = partition.Clusters.Count;
            LightWeightGraph g = (LightWeightGraph)partition.Data;
            //get the name of the graph file from the partition file
            String graphFile = "";
            using (StreamReader sr = new StreamReader(saveLocation + clusterfileName + ".cluster"))
            {
                String dataString = sr.ReadLine();
                graphFile = dataString.Substring(6);
            }

            for (int numMerges = 0; numMerges < startPartitions - minK; numMerges++)
            {
                int[,] connections = new int[partition.Clusters.Count, partition.Clusters.Count];

                
                    // for quick reference let's make a list of which nodes are in which clusters
                    int[] clustAssignments = new int[g.Nodes.Count()];
                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    for (int j = 0; j < partition.Clusters[i].Points.Count; j++)
                    {
                        clustAssignments[partition.Clusters[i].Points[j].Id] = partition.Clusters[i].Points[j].ClusterId;
                    }
                }
                // now go through each node and count its edges out to each cluster
                // add these edges to the connections[] matrix
                for (int i = 0; i < g.Nodes.Count(); i++)
                {
                    int currentCluster = clustAssignments[i];
                    for (int e = 0; e < g.Nodes[i].Edge.Count(); e++)
                    {
                        int adjacentNode = g.Nodes[i].Edge[e];
                        int adjacentCluster = clustAssignments[adjacentNode];
                        connections[currentCluster, adjacentCluster]++;
                    }
                }

                // keep a list of which partitions will be merged
                // List<int> merges = new List<int>();

                // find the largest connections[i,j] and merge clusters i and j
                int largestI = 0;
                int largestJ = 0;
                double largestValue = 0;
                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    for (int j = 0; j < partition.Clusters.Count; j++)
                    {
                        if (j <= i) continue;
                        int sizeI = partition.Clusters[i].Points.Count;
                        int sizeJ = partition.Clusters[j].Points.Count;
                        double score = ((double)connections[i, j]) / (sizeI * sizeJ);
                        //double score = connections[i, j];
                        //if (sizeI > 40 || sizeJ > 40) score = 0;
                        if (score > largestValue)
                        {
                            largestValue = score;
                            largestI = i;
                            largestJ = j;
                        }
                        // we want to merge smaller into larger clusters
                        if (sizeI > sizeJ)
                        {
                            int temp = largestI;
                            largestI = largestJ;
                            largestJ = temp;
                        }
                    }
                }
                // if everything's zero, there is no hope ;-)
                if (largestValue == 0)
                {
                    continue;
                }



                // now we want to merge cluster largestJ into cluster largestI, 
                // remove cluster largestJ, and renumber all clusters after the first
                // adds the points of the second cluster to the first cluster
                for (int i = 0; i < partition.Clusters[largestJ].Points.Count; i++)
                {
                    partition.Clusters[largestI].Points.Add(partition.Clusters[largestJ].Points[i]);
                }


                // remove largestJ cluster
                partition.Clusters.RemoveAt(largestJ);


                // renumber the clusters
                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    partition.Clusters[i].Points.Sort();
                    for (int j = 0; j < partition.Clusters[i].Points.Count; j++)
                    {
                        partition.Clusters[i].Points[j].ClusterId = i;
                    }
                }
            }
            partition.SavePartition(saveLocation + clusterfileName + minK +".cluster", graphFile);
        }

    }
    

}
