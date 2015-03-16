using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetMining.Files;

namespace NetMining.Data
{
    /// <summary>
    /// This Class handles loading data from a comma or tab seperated file
    /// that consists of floating point attributes
    /// </summary>
    public class PointSet
    {
        public List<KPoint> PointList;
        public readonly int Dimensions;
        public PointSet(List<KPoint> data)
        {
            if (data.Count == 0)
                throw new InvalidDataException("Empty Dataset");
            PointList = data;
            Dimensions = PointList[0].Dimensions;
        }

        public DistanceMatrix GetDistanceMatrix(KPoint.DistType distType = KPoint.DistType.Euclidean)
        {
            float[,] distMatrix = new float[Count,Count];
            for (int i = 0; i < Count-1; i++)
            {
                for (int j = 0; j < Count; j++)
                {
                    distMatrix[i, j] =
                        distMatrix[j, i] = (float)PointList[i].GetDistance(PointList[j], distType);
                }
            }
            return new DistanceMatrix(distMatrix);
        }

        public KPoint this[int i]
        {
            get { return PointList[i]; }
            set { PointList[i] = value; }
        }

        public int Count
        {
            get { return PointList.Count; }
        }

        public void NormalizeDataSet(KPoint.NormType norm)
        {
            if (norm == KPoint.NormType.MaxAttr)
            {
                double[] attributes = new double[Dimensions];
                for (int i = 0; i < attributes.Length; i++)
                    attributes[i] = 0.0;

                foreach (KPoint p in PointList)
                    for (int i = 0; i < attributes.Length; i++)
                        attributes[i] = Math.Max(attributes[i], p[i]);

                foreach (KPoint p in PointList)
                    p.Normalize(attributes);
            }
            else
            {
                foreach (KPoint p in PointList)
                    p.Normalize(norm);
            }
        }


        /// <summary>
        /// Calculates the minimum and maximum values for each attribute
        /// </summary>
        /// <returns>Returns min and max KPoints</returns>
        public MinMaxWeigts GetMinMaxWeights()
        {
            KPoint min = PointList[0].Clone();
            KPoint max = PointList[0].Clone();

            for (int i = 1; i < Count; i++)
            {
                for (int k = 0; k < Dimensions; k++)
                {
                    double attributeValue = this[i][k];
                    if (attributeValue < min[k])
                        min[k] = attributeValue;
                    else if (attributeValue > max[k])
                        max[k] = attributeValue;
                }
            }

            return new MinMaxWeigts {Min = min, Max = max};
        }

        public struct MinMaxWeigts
        {
            public KPoint Min, Max;
        }

        public PointSet(String filename)
        {
            PointList = new List<KPoint>();

            DelimitedFile parsedFile = new DelimitedFile(filename);
            var numAttributes = parsedFile.Data[0].Length;
            foreach (var stringArray in parsedFile.Data)
            {
               if (stringArray.Length != numAttributes)
                   throw new InvalidDataException("Non-Constant number of attributes");

                double[] points = stringArray.Select(double.Parse).ToArray();
                PointList.Add(new KPoint(points));
            }

            Dimensions = PointList[0].Dimensions;
        }

        public void Save(String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                for (int i = 0; i < Count; i++)
                {
                    sw.Write(string.Join("\t", PointList[i].Coordinates));
                    if (i != Count - 1)
                        sw.WriteLine();
                }
            }
        }

        /// <summary>
        /// This Function will return a reduced data set by selecting particular attributes
        /// </summary>
        /// <param name="featureSet">This is a list of attributes by index to include</param>
        /// <returns></returns>
        public PointSet GetReducedAttributeSet(List<int> featureSet)
        {
            List<KPoint> reducedData = new List<KPoint>();
            foreach (var p in PointList)
            {
                double[] d = new double[featureSet.Count];
                int i = 0;
                foreach (int f in featureSet)
                {
                    d[i] = p[f];
                    i++;
                }
                reducedData.Add(new KPoint(d));
            }
            return new PointSet(reducedData);
        }

        public DataSetWithMap GetReducedDataSet(List<int> rowList)
        {
            List<KPoint> subset = rowList.Select(i => PointList[i]).ToList();
            int[] map = rowList.ToArray();
            
            DataSetWithMap dsm = new DataSetWithMap();
            dsm.Data = new PointSet(subset);
            dsm.DataMap = map;
            return dsm;
        }

        public struct DataSetWithMap
        {
            public PointSet Data;
            public int[] DataMap;
        }
    }
}
