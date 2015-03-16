using System;
using System.Collections.Generic;
using NetMining.Data;
using NetMining.Graphs;

namespace NetMining.ClusteringAlgo
{
    public class VATClust : IClusteringAlgorithm
    {
        private readonly DistanceMatrix _distanceMatrix;
        public readonly Boolean UseWeights;
        public readonly Boolean KNNGraph;
        public readonly VAT VATResult;
        readonly float _alpha;
        readonly float _beta;
        public VATClust(DistanceMatrix distanceMatrix, Boolean useWeights, Boolean knnGraph, int knnOffset = 0, float alpha = 1.0f, float beta = 0.0f)
        {
            _distanceMatrix = distanceMatrix;
            UseWeights = useWeights;
            KNNGraph = knnGraph;
            _alpha = alpha;
            _beta = beta;

            //Now compute a graph
            LightWeightGraph lwg = LightWeightGraph.GetMinKnnGraph(_distanceMatrix, knnOffset);
            lwg.IsWeighted = UseWeights;

            //Run VAT on it
            VATResult = new VAT(lwg, _alpha, _beta);
        }

        public Partition GetPartition()
        {
            //Get our graph
            LightWeightGraph lwg = VATResult.GetAttackedGraphWithReassignment();

            //Get our cluster Assignment
            List<List<int>> componentList = lwg.GetComponents();

            //Setup our Clusters
            List<Cluster> clusterList = new List<Cluster>();
            for (int i = 0; i < componentList.Count; i++)
            {
                Cluster c = new Cluster(i);
                foreach (var n in componentList[i])
                {
                    c.AddPoint(new ClusteredItem(n));
                }
                clusterList.Add(c);
            }

            String meta = "VATClust: \nRemoved Count:" + VATResult.numNodesRemoved + "\n"
                          + String.Join(",", VATResult.nodeRemovalOrder.GetRange(0, VATResult.numNodesRemoved));

            return new Partition(clusterList, _distanceMatrix, meta);
        }
    }
}
