using System;
using System.Linq;
using NetMining.ClusteringAlgo;
using NetMining.Data;

namespace NetMining.Evaluation
{
    public static class InternalEval
    {
        public static double CombinedScore(Partition clusters)
        {
            double combined = 1.0;

            combined *= AverageSilhouetteIndex(clusters);
            combined *= avgDunnIndex(clusters);
            combined /= DaviesBouldinIndex(clusters);

            return combined;
        }

        public enum DunnInterDistanceType
        {
            CentroidDist = 0,
            MaxDist = 1,
            MinDist = 2,
            AverageDist = 3,
            AverageToCentroid = 4
        };

        public enum DunnIntraDistanceType
        {
            Complete = 0, //Max
            Average = 1, //Average
            AverageToCentroid = 2
        };

        public static double geometricMean(double[] numbers)
        {
            double prod = 1.0;
            foreach (double a in numbers)
                prod *= a;

            return Math.Pow(prod, 1.0 / (double)numbers.Length);
        }

        public static double avgDunnIndex(Partition clusters)
        {
            double[] metrics = new double[15];
            for (int i = 0; i <= 4; i++)
            {
                for (int j = 0; j <= 2; j++)
                    metrics[j * 5 + i] = DunnIndex(clusters, (DunnInterDistanceType)i, (DunnIntraDistanceType)j);
            }

            return geometricMean(metrics);
        }

        public static double DunnIndex(Partition p, DunnInterDistanceType distType, DunnIntraDistanceType intraDistType)
        {
            //Insure all centroids are calculated
            KPoint[][] clusterPoints = new KPoint[p.Clusters.Count][];
            for (int c = 0; c < p.Clusters.Count; c++)
                clusterPoints[c] = p.GetClusterKPoints(c);

            KPoint[] centers = p.Clusters.Select(c => new KPoint(clusterPoints[c.ClusterId])).ToArray();

            //Between 2 clusters(center)
            double minInter = double.MaxValue;
            for (int i = 0; i < p.Clusters.Count - 1; i++)
            {
                for (int j = i + 1; j < p.Clusters.Count; j++)
                {
                    double distIJ = 0.0;
                    if (distType == DunnInterDistanceType.CentroidDist)
                        distIJ = centers[i].elucideanDistance(centers[j]);
                    else if (distType == DunnInterDistanceType.MaxDist)
                        distIJ = getInterClusterDist(clusterPoints[i], clusterPoints[j], true);
                    else if (distType == DunnInterDistanceType.MinDist)
                        distIJ = getInterClusterDist(clusterPoints[i], clusterPoints[j], false);
                    else if (distType == DunnInterDistanceType.AverageDist)
                    {
                        double sumDist = 0.0;
                        foreach (KPoint k in clusterPoints[i])
                        {
                            foreach (KPoint l in clusterPoints[j])
                                sumDist += k.elucideanDistance(l);
                        }
                        distIJ = sumDist / (double)(clusterPoints[i].Length * clusterPoints[j].Length);
                    }
                    else if (distType == DunnInterDistanceType.AverageToCentroid)
                    {
                        double distCentI = 0.0;
                        foreach (KPoint pointJ in clusterPoints[j])
                            distCentI += centers[i].elucideanDistance(pointJ);
                        distCentI /= (double)clusterPoints[j].Length;

                        double distCentJ = 0.0;
                        foreach (KPoint pointI in clusterPoints[i])
                            distCentJ += centers[j].elucideanDistance(pointI);
                        distCentJ /= (double)clusterPoints[i].Length;

                        distIJ = Math.Min(distCentI, distCentJ);
                    }

                    minInter = (distIJ < minInter) ? distIJ : minInter;
                }
            }


            //Pairwise
            double maxIntraClusterDistance = 0.0;
            if (intraDistType == DunnIntraDistanceType.Complete)
            {
                foreach (Cluster c in p.Clusters)
                {
                    for (int i = 0; i < c.Points.Count - 1; i++)
                    {
                        var pointI = clusterPoints[c.ClusterId][i];
                        for (int j = i + 1; j < c.Points.Count; j++)
                        {
                            var pointJ = clusterPoints[c.ClusterId][j];
                            double distIJ = pointI.elucideanDistance(pointJ);
                            maxIntraClusterDistance = (distIJ > maxIntraClusterDistance) ? distIJ : maxIntraClusterDistance;
                        }
                    }
                }
            }
            else if (intraDistType == DunnIntraDistanceType.Average)
            {
                foreach (Cluster c in p.Clusters)
                {
                    double sum = 0.0;
                    for (int i = 0; i < clusterPoints[c.ClusterId].Length - 1; i++)
                    {
                        for (int j = i + 1; j < clusterPoints[c.ClusterId].Length; j++)
                        {
                            sum += clusterPoints[c.ClusterId][i].elucideanDistance(clusterPoints[c.ClusterId][j]);
                        }
                    }
                    sum /= (double)(clusterPoints[c.ClusterId].Length * (clusterPoints[c.ClusterId].Length - 1));
                    maxIntraClusterDistance = (sum > maxIntraClusterDistance) ? sum : maxIntraClusterDistance;
                }
            }
            else if (intraDistType == DunnIntraDistanceType.AverageToCentroid)
            {
                foreach (Cluster c in p.Clusters)
                {
                    double sum = 0.0;
                    foreach (KPoint point in clusterPoints[c.ClusterId])
                        sum += 2 * centers[c.ClusterId].elucideanDistance(point);
                    sum /= (double)(clusterPoints[c.ClusterId].Length);
                    maxIntraClusterDistance = (sum > maxIntraClusterDistance) ? sum : maxIntraClusterDistance;
                }
            }
            double dunn = minInter / maxIntraClusterDistance;
            return dunn;

        }

        public static double getSqrdErrorDistortion(Partition p)
        {
            //Insure all centroids are calculated
            KPoint[][] clusterPoints = new KPoint[p.Clusters.Count][];
            for (int c = 0; c < p.Clusters.Count; c++)
                clusterPoints[c] = p.GetClusterKPoints(c);

            KPoint[] centers = p.Clusters.Select(c => new KPoint(clusterPoints[c.ClusterId])).ToArray();

            double sum = 0.0;

            for (int i = 0; i < p.Clusters.Count; i++)
            {
                for (int j = 0; j < clusterPoints[i].Length; j++)
                    sum += centers[i].distanceSquared(clusterPoints[i][j]);
            }

            return sum / (double)p.GetClusteredItemCount();
        }

        public static double DaviesBouldinIndex(Partition p)
        {
            //Insure all centroids are calculated
            KPoint[][] clusterPoints = new KPoint[p.Clusters.Count][];
            for (int c = 0; c < p.Clusters.Count; c++)
                clusterPoints[c] = p.GetClusterKPoints(c);

            KPoint[] centers = p.Clusters.Select(c => new KPoint(clusterPoints[c.ClusterId])).ToArray();

            double db = 0.0;
            for (int i = 0; i < p.Clusters.Count; i++)
            {
                double Oi = averageDistanceFromCentroid(clusterPoints[i]);
                double maxStat = 0.0;
                for (int j = 0; j < p.Clusters.Count; j++)
                {
                    if (i != j)
                    {
                        double Oj = averageDistanceFromCentroid(clusterPoints[j]);

                        double dbi = (Oi + Oj) / centers[i].elucideanDistance(centers[j]);
                        if (dbi > maxStat)
                            maxStat = dbi;
                    }
                }
                db += maxStat;
            }

            return db / (double)p.Clusters.Count;
        }

        public static double AverageSilhouetteIndex(Partition p)
        {
            int count = 0;
            double sum = 0.0;
            foreach (Cluster c in p.Clusters)
            {
                foreach (ClusteredItem d in c.Points)
                {
                    sum += SilhouetteIndex(d, p);
                }
                count += c.Points.Count;
            }

            return sum / (double)count;
        }

        public static double SilhouetteIndex(ClusteredItem datum, Partition clusters)
        {
            //Calculate A
            double distA = 0.0;
            KPoint datumPoint = clusters.Points[datum.Id];
            foreach (ClusteredItem d in clusters.Clusters[datum.ClusterId].Points)
            {
                distA += datumPoint.elucideanDistance(clusters.Points[d.Id]);
            }
            distA /= (double)(clusters.Clusters[datum.ClusterId].Points.Count - 1);

            double minB = double.MaxValue;
            for (int i = 0; i < clusters.Clusters.Count; i++)
            {
                if (i != datum.ClusterId)
                {
                    double distB = 0.0;
                    foreach (ClusteredItem d in clusters.Clusters[i].Points)
                    {
                        distB += datumPoint.elucideanDistance(clusters.Points[d.Id]);
                    }
                    distB /= (double)(clusters.Clusters[i].Points.Count);

                    if (distB < minB)
                        minB = distB;
                }
            }

            return (minB - distA) / Math.Max(minB, distA);
        }

        //gets the distance between 2 clusters
        public static double getInterClusterDist(KPoint[] c1, KPoint[] c2, bool useMax)
        {
            double maxVal = (useMax) ? 0 : double.MaxValue;

            for (int i = 0; i < c1.Length - 1; i++)
            {
                for (int j = 1; j < c2.Length; j++)
                {
                    double dist = c1[i].elucideanDistance(c2[j]);
                    if (useMax)
                        maxVal = (dist > maxVal) ? dist : maxVal;
                    else  //use minval
                        maxVal = (dist < maxVal) ? dist : maxVal;
                }
            }

            return maxVal;
        }

        public static double averageDistanceFromCentroid(KPoint[] points)
        {
            KPoint center = new KPoint(points);
            return points.Select(p => p.elucideanDistance(center)).Sum() / (double)points.Length;
        }
    }
}
