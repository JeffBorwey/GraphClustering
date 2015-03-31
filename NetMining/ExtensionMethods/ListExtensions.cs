using System;
using System.Collections.Generic;
using NetMining.Utility;

namespace NetMining.ExtensionMethods
{
    public static class ListExtensions
    {

        /// <summary>
        /// Shuffle based on the Fisher-Yates algorithm
        /// O(n) time complexity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public static void Shuffle<T>(this IList<T> data)
        {
            Random rng = Util.Rng;
            int i = data.Count;
            while (i > 1)
            {
                int j = rng.Next(--i + 1);
                T tmp = data[j];
                data[j] = data[i];
                data[i] = tmp;
            }
        }

        public static int IndexOfMin(this float[] self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (self.Length == 0)
            {
                throw new ArgumentException("List is empty.", "self");
            }

            float min = self[0];
            int minIndex = 0;

            for (int i = 1; i < self.Length; ++i)
            {
                if (self[i] < min)
                {
                    min = self[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        public static int IndexOfMax(this float[] self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (self.Length == 0)
            {
                throw new ArgumentException("List is empty.", "self");
            }

            float min = self[0];
            int maxIndex = 0;

            for (int i = 1; i < self.Length; ++i)
            {
                if (self[i] > min)
                {
                    min = self[i];
                    maxIndex = i;
                }
            }

            return maxIndex;
        }
    }
}
