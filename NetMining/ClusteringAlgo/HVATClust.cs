using System.Collections.Generic;
using System.Linq;
using NetMining.Data;

namespace NetMining.ClusteringAlgo
{
    public class HVATClust : IClusteringAlgorithm
    {
        readonly PointSet _data;
        readonly int _minK;
        readonly bool _useKnn;
        readonly bool _weighted;
        readonly float _alpha;
        readonly float _beta;
        private readonly int _kNNOffset;
        public HVATClust(PointSet data, int k, bool weighted, bool useKnn, int kNNOffset = 0, float alpha = 1.0f, float beta = 0.0f)
        {
            _data = data;
            _minK = k;
            _useKnn = useKnn;
            _weighted = weighted;
            _kNNOffset = kNNOffset;
            _alpha = alpha;
            _beta = beta;
        }

        public Partition GetPartition()
        {
            DistanceMatrix mat = _data.GetDistanceMatrix();

            //Setup our partition with a single cluster, with all points
            List<Cluster> clusterList = new List<Cluster> { new Cluster(0, Enumerable.Range(0, _data.Count).ToList()) };
            Partition partition = new Partition(clusterList, _data);

            //Dictionary to hold VAT 
            var vatMap = new Dictionary<int, VATClust>();
            //Dictionary to hold subset array1
            var subsetMap = new Dictionary<int, int[]>();
            while (clusterList.Count < _minK)
            {
                //Calculate the VAT for all values
                foreach (var c in partition.Clusters.Where(c => !vatMap.ContainsKey(c.ClusterId)))
                {
                    //We must calculate a graph for this subset of data
                    List<int> clusterSubset = c.Points.Select(p => p.Id).ToList();
                    var subMatrix = mat.GetReducedDataSet(clusterSubset);

                    //Now calculate Vat
                    VATClust v = new VATClust(subMatrix.Mat, _weighted, _useKnn, _kNNOffset, _alpha, _beta);
                    vatMap.Add(c.ClusterId, v);
                    subsetMap.Add(c.ClusterId, subMatrix.DataMap);
                }

                //Now find the minimum vat value
                int minVatCluster = 0;
                float minVatValue = float.MaxValue;
                foreach (var c in vatMap)
                {
                    if (c.Value.VATResult.minVat < minVatValue)
                    {
                        minVatCluster = c.Key;
                        minVatValue = c.Value.VATResult.minVat;
                    }
                }

                //now merge the partition into the cluster
                var subPartition = vatMap[minVatCluster].GetPartition();
                partition.MergeSubPartition(subPartition, subsetMap[minVatCluster], minVatCluster);
                vatMap.Remove(minVatCluster);
                subsetMap.Remove(minVatCluster);
            }

            return partition;
        }
    }
}
