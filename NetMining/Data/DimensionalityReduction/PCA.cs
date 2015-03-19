using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace NetMining.Data.DimensionalityReduction
{
    public class PCA
    {
        private readonly Matrix<double> _m;
        private readonly List<EigenPair> _eigenPairs;
        public readonly double[] Contributions;
        public readonly double[] RunningTotalArr;
        public PCA(PointSet points)
        {
            //Build a matrix
            _m = Matrix<double>.Build.Dense(points.Count, points.Dimensions);

            //Add our values
            for (int r = 0; r < points.Count; r++)
                for (int c = 0; c < points.Dimensions; c++)
                    _m[r, c] = points[r][c];

            //Now Find the standard Deviation
            for (int d = 0; d < points.Dimensions; d++)
            {
                var col = _m.Column(d);
                double mean = col.Mean();
                double std = col.StandardDeviation();
                for (int r = 0; r < points.Count; r++)
                {
                    _m[r, d] = (_m[r, d] - mean) / std;
                }
            }

            //Calculate Covariance matrix
            var cov = ((_m.Transpose()).Multiply(_m)).Multiply(1 / ((double)points.Count - 1));

            //Calculate EigenValue Decomposition
            var eigen = cov.Evd();
            double eigenSum = eigen.EigenValues.SumMagnitudes();
            _eigenPairs = new List<EigenPair>();
            for (int d = 0; d < points.Dimensions; d++)
            {
                var eVec = eigen.EigenVectors.Column(d);
                _eigenPairs.Add(new EigenPair(eigen.EigenValues[d].Magnitude, eVec));
            }

            //fast eigenVector
            //Matrix<double> mMinus = m.Subtract(mean_vec);
            //Get the Eigenvectors
            /*
            var mT = m.Transpose();
            Svd<double> svd = mT.Svd();
            Matrix<double> eigenVectors = svd.U;
            */

            //Sort the list by eigenValue
            _eigenPairs.Sort((x, y) => y.Value.CompareTo(x.Value));

            //Finally Calculate the Eigenvalue contribution
            RunningTotalArr = new double[points.Dimensions];
            Contributions = new double[points.Dimensions];
            int n = 0;
            double runningTotal = 0.0;
            foreach (double contribution in _eigenPairs.Select(ep => ep.Value / eigenSum))
            {
                runningTotal += contribution;
                Contributions[n] = contribution;
                RunningTotalArr[n] = runningTotal;
                n++;
            }
        }

        public PointSet GetPCAProjection(int kVectors)
        {
            if (kVectors < 1)
                kVectors = 1;
            else if (kVectors > _eigenPairs.Count)
                kVectors = _eigenPairs.Count - 1;

            //transformation matrix
            Matrix<double> transformMat = Matrix<double>.Build.Dense(_m.ColumnCount, kVectors);
            for (int eVec = 0; eVec < kVectors; eVec++)
            {
                for (int d = 0; d < _m.ColumnCount; d++)
                    transformMat[d, eVec] = _eigenPairs[eVec].Vector[d];
            }

            //Project the points from R^d to R^k and construct a pointset from the transformed points
            var transformedMat = _m.Multiply(transformMat);
            List<KPoint> points = transformedMat.EnumerateRows().Select(r => new KPoint(r.ToArray())).ToList();
            return new PointSet(points);
        }

        public class EigenPair
        {
            public double Value;
            public Vector<double> Vector;

            public EigenPair(double va, Vector<double> ve)
            {
                Value = va;
                Vector = ve;
            }
        }
    }
}
