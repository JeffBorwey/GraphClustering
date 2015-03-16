/*
 * Jeffrey K Borwey
 * CS490 - LLoyds algorithm
 * 4/8/2014
 * This implements the lloyd algorithm, with the assistance of the Cluster class
 * and the KPoint class
 */

using System;
using System.Collections.Generic;
using System.Linq;
using NetMining.Data;
namespace NetMining.ClusteringAlgo
{
    public class KMeans : IClusteringAlgorithm
    {
        public readonly int NumItterations;
        public PointSet Points;
        public List<ClusteredItem> ClusteredPoints;

        public readonly int NumClusters;
        private readonly Cluster[] _clusters;
        private readonly KPoint[] _clusterLocation;

        public KMeans(PointSet points, int numClusters)
        {
            //Assign the points and number of clusters
            Points = points;
            NumClusters = numClusters;
            //Create our dataPoints
            ClusteredPoints = new List<ClusteredItem>();
            for (int i = 0; i < points.Count; i++)
                ClusteredPoints.Add(new ClusteredItem(i));

            //Create a new RNG
            Random rng = new Random();

            //Now we find the min and max
            var minMax = points.GetMinMaxWeights();

            //Now we generate our clusters
            //Make random clusters until we have 3 valid clusters
            _clusters = new Cluster[numClusters];
            _clusterLocation = new KPoint[numClusters];
            do
            {
                for (int c = 0; c < numClusters; c++) { 
                    _clusters[c] = new Cluster(c);
                    _clusterLocation[c] = new KPoint(minMax.Min, minMax.Max, rng);
                }
                AssignPoints();
            } while (!ValidClusters());

            
            //KMeans Algorithm algo starts
            //1. assign points
            //2. calculate new centers
            do
            {
                foreach (Cluster cl in _clusters)
                    if (cl.Points.Count > 0)
                        _clusterLocation[cl.ClusterId] = new KPoint(cl.Points.Select(i => points[i.Id]).ToArray());
                NumItterations++;
            } while (AssignPoints());
        }

        //This returns true if every cluster has atleast 1 point
        private bool ValidClusters()
        {
            return _clusters.All(c => c.Points.Count != 0);
        }

        public Partition GetPartition()
        {
            return new Partition(_clusters.ToList(), Points, "KMeans Clustering - K = " + NumClusters);
        }

        //Returns true if something changed
        //Otherwise it returns false
        private bool AssignPoints()
        {
            bool changed = false;

            foreach (Cluster c in _clusters)
            {
                c.clearPoints();
            }

            foreach (ClusteredItem p in ClusteredPoints)
            {
                int cluster = 0;
                double distance = double.MaxValue;
                foreach (Cluster c in _clusters)
                {
                    double dist = _clusterLocation[c.ClusterId].distanceSquared(Points[p.Id]);
                    if (dist < distance)
                    {
                        distance = dist;
                        cluster = c.ClusterId;
                    }
                }

                if (cluster != p.ClusterId)
                    changed = true;

                p.ClusterId = cluster;
                _clusters[cluster].AddPoint(p);
            }
            return changed;
        }

        public double GetSquaredErrorDistortion()
        {
            double sum = ClusteredPoints.Sum(p => _clusterLocation[p.ClusterId].distanceSquared(Points[p.Id]));

            return sum / Points.Count;
        }
    }
}
