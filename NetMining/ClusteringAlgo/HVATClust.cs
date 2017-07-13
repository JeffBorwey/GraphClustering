using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NetMining.Data;
using NetMining.Graphs;
using NetMining.Graphs.Generator;

namespace NetMining.ClusteringAlgo
{
    /// <summary>
    /// This class implements a clustering algorithm that uses the resilience measure called integrity.
    /// </summary>
    public class HVATClust : IClusteringAlgorithm
    {
        readonly AbstractDataset _data;
        private readonly int _minK;
        private readonly bool _weighted;
        private readonly bool _reassignNodes;
        private readonly double _alpha;
        private readonly double _beta;
        private readonly bool _hillClimb;
        private readonly IPointGraphGenerator _graphGen;
        public List<int> _vatNodeRemovalOrder;
        public int _vatNumNodesRemoved;

        private StringBuilder meta;
        public HVATClust(AbstractDataset data, int k, IPointGraphGenerator graphGen, bool weighted = true, double alpha = 1.0f, double beta = 0.0f, bool reassignNodes = true, bool hillClimb = true)
            :this(k, weighted, graphGen, alpha, beta, reassignNodes, hillClimb)
        {
            _data = data;
        }

        public HVATClust(LightWeightGraph data, int k, bool weighted, double alpha = 1.0f, double beta = 0.0f, bool reassignNodes = true, bool hillClimb = true)
            : this(k, weighted, null, alpha, beta, reassignNodes, hillClimb)
        {
            _data = data;
        }

        private HVATClust(int k, bool weighted, IPointGraphGenerator graphGen = null, double alpha = 1.0f, double beta = 0.0f, bool reassignNodes = true, bool hillClimb = true)
        {
            _minK = k;
            _weighted = weighted;
            _graphGen = graphGen;
            _alpha = alpha;
            _beta = beta;
            _reassignNodes = reassignNodes;
            _hillClimb = hillClimb;

            meta = new StringBuilder();
            meta.AppendLine("HVatClust");
        }
        /// <summary>
        /// GetPartition() takes a connected graph (in the form of a dataset or lightweight graph), 
        /// and returns a partitioning of the graph created from applying the resilience measure.
        /// The minimum number of clusters in the partitioning is given by the instance variable _minK.
        /// </summary>
        /// <returns>A partition of the instance graph</returns>
        public Partition GetPartition()
        {
            DistanceMatrix mat = null;
            if (_data.Type == AbstractDataset.DataType.DistanceMatrix)
                mat = (DistanceMatrix)_data;
            else if (_data.Type == AbstractDataset.DataType.PointSet)
                mat = ((PointSet) _data).GetDistanceMatrix();

            //Setup our partition with a single cluster, with all points
            List<Cluster> clusterList = new List<Cluster> { new Cluster(0, Enumerable.Range(0, _data.Count).ToList()) };
            Partition partition = new Partition(clusterList, _data);
            
            //Dictionary to hold VAT 
            var vatMap = new Dictionary<int, VAT>();

            //Dictionary to hold subset array
            var subsetMap = new Dictionary<int, int[]>();
            
            while (clusterList.Count < _minK)
            {
                //Calculate the VAT for all values
                foreach (var c in partition.Clusters.Where(c => !vatMap.ContainsKey(c.ClusterId)))
                {
                    //We must calculate a graph for this subset of data
                    List<int> clusterSubset = c.Points.Select(p => p.Id).ToList();
                    
                    //Now calculate Vat
                    LightWeightGraph lwg;
                    if (_data.Type == AbstractDataset.DataType.Graph)
                    {
                        bool[] exclusion = new bool[_data.Count];
                        for (int i = 0; i < _data.Count; i++)
                            exclusion[i] = true;
                        foreach (var p in c.Points)
                            exclusion[p.Id] = false;
                        lwg = new LightWeightGraph((LightWeightGraph)_data, exclusion);
                    }
                    else //Distance matrix or Pointset
                    {
                        Debug.Assert(mat != null, "mat != null");
                        var subMatrix = mat.GetReducedDataSet(clusterSubset);

                        //Generate our graph
                        lwg = _graphGen.GenerateGraph(subMatrix.Mat);
                    }

                    subsetMap.Add(c.ClusterId, clusterSubset.ToArray());
                    lwg.IsWeighted = _weighted;
                    VAT v = new VAT(lwg, _reassignNodes, _alpha, _beta);
                    _vatNodeRemovalOrder = v.NodeRemovalOrder;
                    _vatNumNodesRemoved = v.NumNodesRemoved;
                    if (_hillClimb)
                        v.HillClimb();
                    ////VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                }

                meta.AppendLine("All calculated VATs:");
                //Now find the minimum vat value
                int minVatCluster = 0;
                double minVatValue = double.MaxValue;
                foreach (var c in vatMap)
                {
                    meta.Append(String.Format("{0} ", c.Value.MinVat));
                    if (c.Value.MinVat < minVatValue)
                    {
                        minVatCluster = c.Key;
                        minVatValue = c.Value.MinVat;
                    }
                }
                meta.AppendLine();

                //now merge the partition into the cluster
                var minVAT = vatMap[minVatCluster];
                var subPartition = minVAT.GetPartition();
                var nodeIndexMap = subsetMap[minVatCluster];

                meta.AppendFormat("Vat: MinVat={0}\r\n", minVAT.MinVat);
                meta.AppendFormat("Removed Count:{0}\r\n", minVAT.NumNodesRemoved);
                meta.AppendLine(String.Join(",",
                    minVAT.NodeRemovalOrder.GetRange(0, minVAT.NumNodesRemoved).Select(c => nodeIndexMap[c])));

                partition.MergeSubPartition(subPartition, nodeIndexMap, minVatCluster);
                vatMap.Remove(minVatCluster);
                subsetMap.Remove(minVatCluster);
            }
            partition.MetaData = meta.ToString();
            return partition;
        }
        /// <summary>
        /// GetPartition returns a clustering, but can take node removal order and number of nodes removed as 
        /// parameters instead of calculating them.  This saves time if these have previously been calculated, 
        /// such as for calculating with another resilience measure.  
        /// </summary>
        /// <param name="nodeRemovalOrder">A listing of node removal order, so that it is not recalculated</param>
        /// <param name="numNodesRemoved">The number of nodes to be removed with the optimal node removal order</param>
        /// <returns></returns>
        public Partition GetPartition(List<int> nodeRemovalOrder, int numNodesRemoved)
        {
            DistanceMatrix mat = null;
            if (_data.Type == AbstractDataset.DataType.DistanceMatrix)
                mat = (DistanceMatrix)_data;
            else if (_data.Type == AbstractDataset.DataType.PointSet)
                mat = ((PointSet)_data).GetDistanceMatrix();

            //Setup our partition with a single cluster, with all points
            List<Cluster> clusterList = new List<Cluster> { new Cluster(0, Enumerable.Range(0, _data.Count).ToList()) };
            Partition partition = new Partition(clusterList, _data);

            //Dictionary to hold VAT 
            var vatMap = new Dictionary<int, VAT>();

            //Dictionary to hold subset array
            var subsetMap = new Dictionary<int, int[]>();
            while (clusterList.Count < _minK)
            {
                //Calculate the VAT for all values
                foreach (var c in partition.Clusters.Where(c => !vatMap.ContainsKey(c.ClusterId)))
                {
                    //We must calculate a graph for this subset of data
                    List<int> clusterSubset = c.Points.Select(p => p.Id).ToList();

                    //Now calculate Vat
                    LightWeightGraph lwg;
                    if (_data.Type == AbstractDataset.DataType.Graph)
                    {
                        bool[] exclusion = new bool[_data.Count];
                        for (int i = 0; i < _data.Count; i++)
                            exclusion[i] = true;
                        foreach (var p in c.Points)
                            exclusion[p.Id] = false;
                        lwg = new LightWeightGraph((LightWeightGraph)_data, exclusion);
                    }
                    else //Distance matrix or Pointset
                    {
                        Debug.Assert(mat != null, "mat != null");
                        var subMatrix = mat.GetReducedDataSet(clusterSubset);

                        //Generate our graph
                        lwg = _graphGen.GenerateGraph(subMatrix.Mat);
                    }

                    subsetMap.Add(c.ClusterId, clusterSubset.ToArray());
                    lwg.IsWeighted = _weighted;
                    VAT v = new VAT(lwg, _reassignNodes, _alpha, _beta, nodeRemovalOrder, numNodesRemoved);
                    //if (_hillClimb)
                        v.HillClimb();
                    ////VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                }

                meta.AppendLine("All calculated VATs:");
                //Now find the minimum vat value
                int minVatCluster = 0;
                double minVatValue = double.MaxValue;
                foreach (var c in vatMap)
                {
                    meta.Append(String.Format("{0} ", c.Value.MinVat));
                    if (c.Value.MinVat < minVatValue)
                    {
                        minVatCluster = c.Key;
                        minVatValue = c.Value.MinVat;
                    }
                }
                meta.AppendLine();

                //now merge the partition into the cluster
                var minVAT = vatMap[minVatCluster];
                var subPartition = minVAT.GetPartition();
                var nodeIndexMap = subsetMap[minVatCluster];

                meta.AppendFormat("Vat: MinVat={0}\r\n", minVAT.MinVat);
                meta.AppendFormat("Removed Count:{0}\r\n", minVAT.NumNodesRemoved);
                meta.AppendLine(String.Join(",",
                    minVAT.NodeRemovalOrder.GetRange(0, minVAT.NumNodesRemoved).Select(c => nodeIndexMap[c])));

                partition.MergeSubPartition(subPartition, nodeIndexMap, minVatCluster);
                vatMap.Remove(minVatCluster);
                subsetMap.Remove(minVatCluster);
            }
            partition.MetaData = meta.ToString();
            return partition;
        }
        /// <summary>
        /// GetGPartition is different from GetPartition in 2 ways:
        /// 1. It does not require a connected graph.  
        /// 2. If there are too many clusters, it combines them such that the desired number of clusters is returned
        /// </summary>
        /// <returns>A partitioning of the graph</returns>
        public Partition GetGPartition()
        {
            DistanceMatrix mat = null;
            if (_data.Type == AbstractDataset.DataType.DistanceMatrix)
                mat = (DistanceMatrix)_data;
            else if (_data.Type == AbstractDataset.DataType.PointSet)
                mat = ((PointSet)_data).GetDistanceMatrix();

           //get the actual partition (if graph not necessarily connected)
            Partition partition = Partition.GetPartition((LightWeightGraph)_data);

            //Dictionary to hold VAT 
            var vatMap = new Dictionary<int, VAT>();

            //Dictionary to hold subset array
            var subsetMap = new Dictionary<int, int[]>();
            while (partition.Clusters.Count < _minK)
            //while (clusterList.Count < _minK)
            {
                Console.WriteLine("Count = " + partition.Clusters.Count);
                Console.WriteLine("mink = " + _minK);
                //Calculate the VAT for all values
                foreach (var c in partition.Clusters.Where(c => !vatMap.ContainsKey(c.ClusterId)))
                {
                    //We must calculate a graph for this subset of data
                    List<int> clusterSubset = c.Points.Select(p => p.Id).ToList();

                    //Now calculate Vat
                    LightWeightGraph lwg;
                    if (_data.Type == AbstractDataset.DataType.Graph)
                    {
                        bool[] exclusion = new bool[_data.Count];
                        for (int i = 0; i < _data.Count; i++)
                            exclusion[i] = true;
                        foreach (var p in c.Points)
                            exclusion[p.Id] = false;
                        lwg = new LightWeightGraph((LightWeightGraph)_data, exclusion);
                    }
                    else //Distance matrix or Pointset
                    {
                        Debug.Assert(mat != null, "mat != null");
                        var subMatrix = mat.GetReducedDataSet(clusterSubset);

                        //Generate our graph
                        lwg = _graphGen.GenerateGraph(subMatrix.Mat);
                    }

                    subsetMap.Add(c.ClusterId, clusterSubset.ToArray());
                    lwg.IsWeighted = _weighted;
                    VAT v = new VAT(lwg, _reassignNodes, _alpha, _beta);
                    _vatNodeRemovalOrder = v.NodeRemovalOrder;
                    _vatNumNodesRemoved = v.NumNodesRemoved;
                    if (_hillClimb)
                        v.HillClimb();
                    ////VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                    Console.WriteLine("Calculated Vat for cluster " + c.ClusterId);
                }

                meta.AppendLine("All calculated VATs:");
                //Now find the minimum vat value
                int minVatCluster = 0;
                double minVatValue = double.MaxValue;
                foreach (var c in vatMap)
                {
                    meta.Append(String.Format("{0} ", c.Value.MinVat));
                    if (c.Value.MinVat < minVatValue)
                    {
                        minVatCluster = c.Key;
                        minVatValue = c.Value.MinVat;
                    }
                }
                meta.AppendLine();
                
                //now merge the partition into the cluster
                var minVAT = vatMap[minVatCluster];
                var subPartition = minVAT.GetPartition();
                var nodeIndexMap = subsetMap[minVatCluster];

                meta.AppendFormat("Vat: MinVat={0}\r\n", minVAT.MinVat);
                meta.AppendFormat("Removed Count:{0}\r\n", minVAT.NumNodesRemoved);
                meta.AppendLine(String.Join(",",
                    minVAT.NodeRemovalOrder.GetRange(0, minVAT.NumNodesRemoved).Select(c => nodeIndexMap[c])));

                partition.MergeSubPartition(subPartition, nodeIndexMap, minVatCluster);
                vatMap.Remove(minVatCluster);
                subsetMap.Remove(minVatCluster);
                Console.WriteLine("Found min cluster");
                Console.WriteLine(meta);
            }
            partition.MetaData = meta.ToString();
            // The idea is now that we have partitions, combine them so that partition.Clusters.Count == minK
            //if (partition.Clusters.Count > _minK)
            //{
             //   combineClusters(partition, _minK);
           // }
            return partition;
        }
        /// <summary>
        /// combineClusters is used when the partitioning achieved has too many clusters.
        /// </summary>
        /// <param name="partition">A partitioning of a graph with any number of clusters</param>
        /// <param name="minK">The desired number of clusters</param>
        /// <returns>A new partitioning with the desired number of clusters</returns>
        public Partition combineClustersOld(Partition partition, int minK)
        {
            int[,] connections = new int[partition.Clusters.Count, partition.Clusters.Count];
            LightWeightGraph g = (LightWeightGraph)_data;

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
            // we want to do (partition.Clusters.count - minK) merges
            // keep a list of which partitions will be merged
            List<int> merges = new List<int>();
            for (int numMerges = 0; numMerges < partition.Clusters.Count - minK; numMerges++)
            {
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
                        if (score > largestValue)
                        {
                            largestValue = score;
                            largestI = i;
                            largestJ = j;
                        }
                    }
                }
                // if everything's zero, there is no hope ;-)
                if (largestValue == 0)
                {
                    continue;
                }
                merges.Add(largestI);
                merges.Add(largestJ);
                // it is possible to merge J multiple times, if its nodes are split between clusters.
                // we only want to merget J once, so we need to zero out all largestJ 
                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    connections[i, largestJ] = 0;
                }
            }
            // now we have the list *merges*, the idea is to take 2 numbers off it, 
            // the first is smaller than the second.  We need to merge the second into the first, 
            // remove the second, and renumber all clusters after the first
            for (int numMerges = 0; numMerges < merges.Count / 2; numMerges++)
            {
                int firstCluster = merges[numMerges * 2];
                int secondCluster = merges[(numMerges * 2) + 1];
                
                // adds the points of the second cluster to the first cluster
                for (int i = 0; i < partition.Clusters[secondCluster].Points.Count; i++)
                {
                    partition.Clusters[firstCluster].Points.Add(partition.Clusters[secondCluster].Points[i]);
                }

            }
            // remove all the second clusters (count from the bottom
            // so that the numbering doesn't get messed up...)
            int[] toRemove = new int[merges.Count / 2];
            for (int numMerges = 0; numMerges < merges.Count / 2; numMerges++)
            {
                int firstCluster = merges[numMerges * 2];
                int secondCluster = merges[(numMerges * 2) + 1];

                toRemove[numMerges] = secondCluster;
            }
            Array.Sort(toRemove);
            for (int i = toRemove.Length - 1; i >= 0; i--)
            {
                partition.Clusters.RemoveAt(toRemove[i]);
            }

            // renumber the clusters
            for (int i = 0; i < partition.Clusters.Count; i++)
            {
                partition.Clusters[i].Points.Sort();
                partition.Clusters[i].ClusterId = i;
            }
            return partition;
        }

        public Partition combineClusters(Partition partition, int minK)
        {
            // we want to do (partition.Clusters.count - minK) merges
            int startPartitions = partition.Clusters.Count;
            for (int numMerges = 0; numMerges < startPartitions - minK; numMerges++)
            {
                int[,] connections = new int[partition.Clusters.Count, partition.Clusters.Count];
                LightWeightGraph g = (LightWeightGraph)_data;

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
            return partition;
        }

    }
}
