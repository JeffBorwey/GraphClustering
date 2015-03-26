using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMining.Data;
using NetMining.Graphs;

namespace NetMining.ClusteringAlgo
{
    public class HVATClust : IClusteringAlgorithm
    {
        readonly AbstractDataset _data;
        private readonly int _minK;
        private readonly bool _useKnn;
        private readonly bool _weighted;
        private readonly bool _reassignNodes;
        private readonly float _alpha;
        private readonly float _beta;
        private readonly int _kNNOffset;
        private readonly DistanceMatrix _mat;

        private StringBuilder meta;
        public HVATClust(AbstractDataset data, int k, bool weighted = true, bool useKnn = true, int kNNOffset = 0, float alpha = 1.0f, float beta = 0.0f, bool reassignNodes = true)
            :this(k, weighted, useKnn, kNNOffset, alpha, beta, reassignNodes)
        {
            _data = data;
        }

        public HVATClust(LightWeightGraph data, int k, bool weighted, float alpha = 1.0f, float beta = 0.0f, bool reassignNodes = true)
            : this(k, weighted, false, 0, alpha, beta, reassignNodes)
        {
            _data = data;
        }


        private HVATClust(int k, bool weighted, bool useKnn, int kNNOffset = 0, float alpha = 1.0f, float beta = 0.0f, bool reassignNodes = true)
        {
            _minK = k;
            _useKnn = useKnn;
            _weighted = weighted;
            _kNNOffset = kNNOffset;
            _alpha = alpha;
            _beta = beta;
            _reassignNodes = reassignNodes;

            meta = new StringBuilder();
            meta.AppendLine("HVatClust");
        }

        public Partition GetPartition()
        {
            int iter = 0;
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
                        var subMatrix = mat.GetReducedDataSet(clusterSubset);

                        //Generate our graph
                        if (_useKnn)
                            lwg = LightWeightGraph.GetMinKnnGraph(subMatrix.Mat, _kNNOffset);
                        else
                            lwg = LightWeightGraph.GetMinGeoGraph(subMatrix.Mat);
                    }

                    subsetMap.Add(c.ClusterId, clusterSubset.ToArray());
                    lwg.IsWeighted = _weighted;
                    VAT v = new VAT(lwg, _reassignNodes, _alpha, _beta);
                    ////VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                }

                //Now find the minimum vat value
                int minVatCluster = 0;
                float minVatValue = float.MaxValue;
                foreach (var c in vatMap)
                {
                    if (c.Value.MinVat < minVatValue)
                    {
                        minVatCluster = c.Key;
                        minVatValue = c.Value.MinVat;
                    }
                }

                //now merge the partition into the cluster
                var minVAT = vatMap[minVatCluster];
                var subPartition = minVAT.GetPartition();
                var nodeIndexMap = subsetMap[minVatCluster];

                meta.AppendFormat("Vat: MinVat={0}\n", minVAT.MinVat);
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
