using System;
using System.Linq;
using NetMining.ClusteringAlgo;
using NetMining.Data;
using NetMining.Files;

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

        public static String CheckForNoise(String labelFile, String clusterFileName)
        {
            // need to calculate ns, ms and cs, as described in Yang and Leskovec ICDM2012 
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(labelFile);
            int labelCol = delimitedLabelFile.Data[0].Length;
            LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

            //get the Partion file
            Partition clusterFile = new Partition(clusterFileName);
            int[] assignments = new int[labels.LabelIndices.Length];
            // initialize assignments array to -1
            // ultimately, nodes that have been removed as part of a critical attack set will stay at -1 assignment
            for (int i=0; i< assignments.Length; i++)
            {
                assignments[i] = -1;
            }
            int noiseThreshold;
            //if (assignments.Length == 550) noiseThreshold = 500;
            //else if (assignments.Length == 770) noiseThreshold = 700;
            //else noiseThreshold = 1100;

            if (assignments.Length == 220) noiseThreshold = 200;
            else if (assignments.Length == 440) noiseThreshold = 400;
            else noiseThreshold = 800;


            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++)
                {
                    int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                    int id = clusterFile.Clusters[cluster].Points[j].Id;
                    assignments[id] = clusterid;
                }
            }
            int[] ns = new int[clusterFile.Clusters.Count];
            int[] ms = new int[clusterFile.Clusters.Count];
            int[] cs = new int[clusterFile.Clusters.Count];
            Boolean[] isAllNoise = new Boolean[clusterFile.Clusters.Count];
            // if we're doing this without reassign, we need new nodes and edges valuse
            int edges = 0;
            int nodes = 0;

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                ns[cluster] = clusterFile.Clusters[cluster].Points.Count;
                isAllNoise[cluster] = true;
                for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++) // for each vertex in this cluster
                {
                    nodes++;
                    if (clusterFile.Clusters[cluster].Points[j].Id < noiseThreshold)
                    {
                        isAllNoise[cluster] = false;
                    }
                    for (int k=0; k < clusterFile.Graph.Nodes[clusterFile.Clusters[cluster].Points[j].Id].Edge.Length; k++) // for each edge k adjacent to j
                    {
                        edges++;
                        int edge = clusterFile.Graph.Nodes[clusterFile.Clusters[cluster].Points[j].Id].Edge[k];
                        if (cluster == assignments[edge])
                        {
                            ms[cluster]++;
                            //if (cluster == 7) Console.WriteLine("ms " + edge);
                        } else
                        {
                            cs[cluster]++;
                            //if (cluster == 7) Console.WriteLine("cs " + edge);
                        }
                    }
                }
                

            }

            String report = "";

            double[] internalDensity = new double[clusterFile.Clusters.Count];
            double[] averageDegree = new double[clusterFile.Clusters.Count];
            double[] expansion = new double[clusterFile.Clusters.Count];
            double[] cutRatio = new double[clusterFile.Clusters.Count];
            double[] conductance = new double[clusterFile.Clusters.Count];
            double[] separability = new double[clusterFile.Clusters.Count];
            double WAinternalDensity = 0;
            double WAaverageDegree = 0;
            double WAexpansion = 0;
            double WAcutRatio = 0;
            double WAconductance = 0;
            double WAseparability = 0;

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                double totalPossibleInternalEdges = ((ns[cluster] * (ns[cluster] - 1)) / 2);
                internalDensity[cluster] = totalPossibleInternalEdges == 0 ? 0 : (double)ms[cluster] / totalPossibleInternalEdges;
                averageDegree[cluster] = 2.0 * ms[cluster] / ns[cluster];
                expansion[cluster] = (double)cs[cluster] / ns[cluster];
                cutRatio[cluster] = (double)cs[cluster] / (ns[cluster]*(assignments.Length - ns[cluster]));
                conductance[cluster] = (double)cs[cluster] / (2 * ms[cluster] + cs[cluster]);
                separability[cluster] = (double)ms[cluster] / cs[cluster];
            }
            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {
                WAinternalDensity += internalDensity[cluster] * ns[cluster];
                WAaverageDegree += averageDegree[cluster] * ns[cluster];
                WAexpansion += expansion[cluster] * ns[cluster];
                WAcutRatio += cutRatio[cluster] * ns[cluster];
                WAconductance += conductance[cluster] * ns[cluster];
                WAseparability += separability[cluster] * ns[cluster];
            }

            WAinternalDensity /= (double) nodes;
            WAaverageDegree /= (double)nodes;
            WAexpansion /= (double)nodes;
            WAcutRatio /= (double)nodes;
            WAconductance /= (double)nodes;
            WAseparability /= (double)nodes;

            for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
            {   

                report += clusterFileName.Substring(clusterFileName.LastIndexOf('\\') + 1) + "," + cluster + "," + 
                   (isAllNoise[cluster] ? 1 : 0) + "," + ns[cluster] + "," + ms[cluster] + "," + cs[cluster] +
                   "," + internalDensity[cluster] + "," + internalDensity.Min() + "," + WAinternalDensity + "," + internalDensity.Max() +
                   //"," + averageDegree[cluster] + ","  + averageDegree.Min() + "," + averageDegree.Average() + "," + averageDegree.Max() +
                   "," + averageDegree[cluster] + "," + averageDegree.Min() + "," + WAaverageDegree + "," + averageDegree.Max() +
                   "," + expansion[cluster] + "," + expansion.Min() + "," + WAexpansion + "," + expansion.Max() + 
                   "," + cutRatio[cluster] + "," + cutRatio.Min() + "," + WAcutRatio + "," + cutRatio.Max() +
                   "," + conductance[cluster] + "," + conductance.Min() + "," + WAconductance + "," + conductance.Max() + 
                   "," + separability[cluster] + "," + separability.Min() + "," + WAseparability + "," + separability.Max() + "\n";
            }
                return report;
        }
    }
}
