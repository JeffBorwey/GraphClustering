/*
 * Jeffrey K Borwey
 * CS490 - LLoyds algorithm
 * 4/8/2014
 * These classes represent KDimensional data points
 * DataPoint extends KPoint to represent assignment of datapoints to clusters
 */

using System;
using System.Linq;
using System.Text;

namespace NetMining.Data
{
    public class KPoint
    {
        public double[] Coordinates;
        public readonly int Dimensions;
        public KPoint(int k)
        {
            Dimensions = k;
            Coordinates = new double[k];
        }

        public KPoint(double[] location)
        {
            Coordinates = location;
            Dimensions = Coordinates.Length;
        }

        public double this[int i]
        {
            get { return Coordinates[i]; }
            set { Coordinates[i] = value; }
        }

        public static KPoint Zero(int dimensions)
        {
            double[] dim = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
                dim[i] = 0.0;
            return new KPoint(dim);
        }

        public static KPoint One(int dimensions)
        {
            double[] dim = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
                dim[i] = 1.0;
            return new KPoint(dim);
        }

        public KPoint(KPoint min, KPoint max, Random rng)
        {
            Dimensions = min.Dimensions;
            Coordinates = new double[Dimensions];

            //generate random coordinates
            for (int k = 0; k < Dimensions; k++)
            {
                Coordinates[k] = min[k] + rng.NextDouble() * (max[k] - min[k]);
            }
        }

        //This calculates the center of a set of points
        public KPoint(KPoint[] points)
        {
            if (points.Length == 0)
                throw new Exception("No Points");

            Dimensions = points[0].Dimensions;
            double[] coords = new double[Dimensions];

            foreach (KPoint p in points)
            {
                if (p.Dimensions != Dimensions)
                    throw new Exception("Incompatible Dimensions");

                for (int k = 0; k < Dimensions; k++) 
                    coords[k] += p[k];
            }

            for (int k = 0; k < Dimensions; k++)
            {
                coords[k] /= points.Length;
            }

            Coordinates = coords;
        }

        public enum NormType
        {
            GeometricMean,
            ArithmeticMean,
            Max,
            MaxAttr,
            ZeroMeanOneStd,
            None
        }

        public enum DistType
        {
            Euclidean,
            PMCC,
            Manhattan,
            Chebyshev
        }

        public double GetDistance(KPoint point, DistType type)
        {
            switch (type)
            {
                case DistType.Euclidean:
                    return elucideanDistance(point);
                case DistType.PMCC:
                    return 1.0-getPMCC(point);
                case DistType.Manhattan:
                    return minkowskiDistance(point, 1.0);
                case DistType.Chebyshev:
                    return minkowskiDistance(point, 20.0);
            }
            return elucideanDistance(point); 
        }

        public void Normalize(NormType type)
        {
            double factor = 1;
            if (type == NormType.GeometricMean)
            {
                foreach (double v in Coordinates)
                    factor *= v;
                factor = Math.Pow(factor, 1.0 / Coordinates.Count());
            }
            else if (type == NormType.ArithmeticMean)
            {
                double sum = Coordinates.Sum();
                sum /= (double)Coordinates.Count();
                factor = sum;
            }
            else if (type == NormType.Max)
            {
                double max = 0;
                foreach (double v in Coordinates)
                    if (v > max)
                        max = v;
                factor = max;
            }
            else if (type == NormType.None)
                return;
            else if (type == NormType.MaxAttr)
                return;

            for (int i = 0; i < Coordinates.Count(); i++)
            {
                Coordinates[i] /= factor;
            }
        }

        public void Normalize(double[] normFactors)
        {
            for (int i = 0; i < Coordinates.Count(); i++)
                Coordinates[i] /= normFactors[i];
        }

        public double getPMCC(KPoint endPoint)
        {
            double ux, uy, vx, vy, w;
            ux = uy = vx = vy = w = 0.0;
            for (int i = 0; i < Coordinates.Length; i++)
            {
                ux += Coordinates[i];
                uy += endPoint[i];

                vx += Coordinates[i] * Coordinates[i];
                vy += endPoint[i] * endPoint[i];

                w += Coordinates[i] * endPoint[i];
            }
            double n = (double)Coordinates.Length;
            double numerator = n * w - ux * uy;
            double denominator = Math.Sqrt((n * vx - ux * ux) * (n * vy - uy * uy));
            return numerator / denominator;
        }

        public double minkowskiDistance(KPoint point, double p)
        {
            if (Dimensions != point.Dimensions)
                throw new Exception("Incompatible Dimensions");
            if (p < 1.0)
                throw new Exception("Invalid Metric");

            double sum = 0.0;
            for (int i = 0; i < Dimensions; i++)
            {
                sum += Math.Pow(Math.Abs(point[i] - this[i]), p);
            }
            return Math.Pow(sum, 1 / p);
        }

        public double distanceSquared(KPoint point)
        {
            if (Dimensions != point.Dimensions)
                throw new Exception("Incompatible Dimensions");

            double sum = 0.0;
            for (int i = 0; i < Dimensions; i++)
            {
                sum += Math.Pow(point[i] - this[i], 2);
            }
            return sum;
        }

        public double elucideanDistance(KPoint point)
        {
            return Math.Sqrt(distanceSquared(point));
        }

        public KPoint Clone()
        {
            return new KPoint(Coordinates);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int k = 0; k < Dimensions; k++)
            {
                sb.AppendFormat("{0:f4}", Coordinates[k]);
                if (k != Dimensions - 1)
                    sb.Append(", ");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
