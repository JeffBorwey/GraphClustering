using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.Data;
namespace NetMining.ADT
{
    public class BallTree
    {

        public BallTree(KPoint points)
        {

        }

        private class BallTreeNode
        {
            public KPoint Point = null;
            public int PivotDim;
            public BallTreeNode leftNode, rightNode;


            public MinHeapPriorityQueue<KPoint> NearestNeighbors(KPoint p, int k, MinHeapPriorityQueue<KPoint> Q, BallTreeNode B)
            {
                if (p[B.PivotDim] > Q.peek().elucideanDistance(p))
                {
                    return Q;
                }
                else if (B.IsLeaf())
                {
                    if (p.elucideanDistance(B.Point) < p.elucideanDistance(Q.peek()))
                    {
                        if (Q.Count >= k)
                            Q.extractMin();
                    
                    }
                }
                return Q;
            }

            public BallTreeNode(PointSet Points)
            {
                //if there is only a single point, set this as the data
                if (Points.Count == 1)
                {
                    Point = Points[0];
                }
                else
                {
                    //Find the dimension of greatest spread
                    var minMax = Points.GetMinMaxWeights();
                    double maxSpread = double.MinValue;
                    int maxDim = 0;
                    for (int d = 0; d < Points.Dimensions; d++)
                    {
                        double range = minMax.Max[d] - minMax.Min[d];
                        if (range > maxSpread)
                        {
                            maxSpread = d;
                            maxSpread = range;
                        }
                    }
                    PivotDim = maxDim;

                    //Pivot Select, Replace with QuickSelect
                    double[] pivots = new double[Points.Count];
                    for (int i = 0; i < Points.Count; i++)
                    {
                        pivots[i] = Points[i][PivotDim];
                    }
                    Array.Sort(pivots);


                    double pivotValue = pivots[(Points.Count - 1) / 2];

                    //Create left and right children
                    List<KPoint> left = new List<KPoint>();
                    List<KPoint> right = new List<KPoint>();

                    for (int i = 0; i < Points.Count; i++)
                    {
                        if (Points[i][PivotDim] <= pivotValue)
                            left.Add(Points[i]);
                        else
                            right.Add(Points[i]);
                    }

                    leftNode = new BallTreeNode(new PointSet(left));
                    rightNode = new BallTreeNode(new PointSet(right));
                }
            }

            
   
            public bool IsLeaf()
            {
                return Point != null;
            }

        }
    }
}
