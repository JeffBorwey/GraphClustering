using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class ListExtensions
    {
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
