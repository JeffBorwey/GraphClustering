using System;
using System.Collections.Generic;
using System.IO;
using NetMining.Files;

namespace NetMining.Data
{
    public class DistanceMatrix
    {
        public float[,] Distances;

        public float this[int a, int b]
        {
            get { return Distances[a, b]; }
            set { Distances[a, b] = value; }
        }

        public int Count
        {
            get { return Distances.GetLength(0); }
        }

        /// <summary>
        /// Creates a distance matrix from an array
        /// </summary>
        /// <param name="mat"></param>
        public DistanceMatrix(float[,] mat)
        {
            Distances = mat;
        }

        /// <summary>
        /// Accepts a file and parses it as a distance matrix
        /// </summary>
        /// <param name="filename">Path to the delimited matrix</param>
        public DistanceMatrix(String filename)
        {
            DelimitedFile parsedFile = new DelimitedFile(filename);
            //parse each line
            int dimensions = parsedFile.Data.Count;
            if (dimensions == 0)
                throw new InvalidDataException("Empty matrix");
            int numCols = parsedFile.Data[0].Length;
            if (numCols != dimensions)
                throw new InvalidDataException("Non-Square Matrix");

            Distances = new float[dimensions,dimensions];
            for (int r = 0; r < dimensions; r++)
                for (int c = 0; c < dimensions; c++)
                    Distances[r, c] = float.Parse(parsedFile.Data[r][c]);
        }

        public List<float> GetSortedDistanceList()
        {
            List<float> distanceList = new List<float>();

            for (int i = 0; i < Count - 1; i++)
                for (int j = i + 1; j < Count; j++)
                    distanceList.Add(Distances[i, j]);

            distanceList.Sort((x,y) => x.CompareTo(y));
            return distanceList;
        }

        public DistanceMatrixWithMap GetReducedDataSet(List<int> itemList)
        {
            int count = itemList.Count;
            float[,] data = new float[count, count];

            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                    data[i, j] = Distances[itemList[i], itemList[j]];

            int[] map = itemList.ToArray();

            DistanceMatrixWithMap dsm = new DistanceMatrixWithMap { DataMap = map, Mat = new DistanceMatrix(data) };
            return dsm;
        }

        public struct DistanceMatrixWithMap
        {
            public DistanceMatrix Mat;
            public int[] DataMap;
        }
    }
}
