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

    }



}
