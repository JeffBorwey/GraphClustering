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
    public class HToughnessClust : IClusteringAlgorithm
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
        public HToughnessClust(AbstractDataset data, int k, IPointGraphGenerator graphGen, bool weighted = true, double alpha = 1.0f, double beta = 0.0f, bool reassignNodes = true, bool hillClimb = true)
            : this(k, weighted, graphGen, alpha, beta, reassignNodes, hillClimb)
        {
            _data = data;
        }

        public HToughnessClust(LightWeightGraph data, int k, bool weighted, double alpha = 1.0f, double beta = 0.0f, bool reassignNodes = true, bool hillClimb = true)
            : this(k, weighted, null, alpha, beta, reassignNodes, hillClimb)
        {
            _data = data;
        }

        private HToughnessClust(int k, bool weighted, IPointGraphGenerator graphGen = null, double alpha = 1.0f, double beta = 0.0f, bool reassignNodes = true, bool hillClimb = true)
        {
            _minK = k;
            _weighted = weighted;
            _graphGen = graphGen;
            _alpha = alpha;
            _beta = beta;
            _reassignNodes = reassignNodes;
            _hillClimb = hillClimb;

            meta = new StringBuilder();
            meta.AppendLine("HToughnessClust");
        }

        public Partition GetPartition()
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
            var vatMap = new Dictionary<int, Toughness>();

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
                    Toughness v = new Toughness(lwg, _reassignNodes, _alpha, _beta);
                    _vatNodeRemovalOrder = v.NodeRemovalOrder;
                    _vatNumNodesRemoved = v.NumNodesRemoved;
                    if (_hillClimb)
                        v.HillClimb();
                    ////VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                }

                meta.AppendLine("All calculated Toughnesses:");
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

                meta.AppendFormat("Toughness: MinToughness={0}\n", minVAT.MinVat);
                meta.AppendFormat("Removed Count:{0} \n", minVAT.NumNodesRemoved);
                meta.AppendLine(String.Join(",",
                    minVAT.NodeRemovalOrder.GetRange(0, minVAT.NumNodesRemoved).Select(c => nodeIndexMap[c])));

                partition.MergeSubPartition(subPartition, nodeIndexMap, minVatCluster);
                vatMap.Remove(minVatCluster);
                subsetMap.Remove(minVatCluster);
            }
            partition.MetaData = meta.ToString();
            return partition;
        }
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
            var vatMap = new Dictionary<int, Toughness>();

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
                    Toughness v = new Toughness(lwg, _reassignNodes, _alpha, _beta, nodeRemovalOrder, numNodesRemoved);
                    if (_hillClimb) // why is this commented out??
                        v.HillClimb();
                    ////VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                }

                meta.AppendLine("All calculated Toughnesses:");
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

                meta.AppendFormat("Toughness: MinToughness={0}\n", minVAT.MinVat);
                meta.AppendFormat("Removed Count:{0}\n", minVAT.NumNodesRemoved);
                meta.AppendLine(String.Join(",",
                    minVAT.NodeRemovalOrder.GetRange(0, minVAT.NumNodesRemoved).Select(c => nodeIndexMap[c])));

                partition.MergeSubPartition(subPartition, nodeIndexMap, minVatCluster);
                vatMap.Remove(minVatCluster);
                subsetMap.Remove(minVatCluster);
            }
            partition.MetaData = meta.ToString();
            return partition;
        }

    }
}
