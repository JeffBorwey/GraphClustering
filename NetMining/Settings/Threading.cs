using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Settings
{
    public static class Threading
    {
        private static int _numThreadsBc = 2;
        private static bool _threadHVAT = false;

        /// <summary>
        /// Gets or Sets whether HVat should run using multiple threads for betweeness calculation
        /// </summary>
        public static bool ThreadHVAT
        {
            get { return _threadHVAT; }
            set { _threadHVAT = value; }
        }

        /// <summary>
        /// Gets or sets the number of threads to use in betweeness centrality
        /// </summary>
        public static int NumThreadsBc
        {
            get { return _numThreadsBc; }
            set
            {
                if (value > 0)
                    _numThreadsBc = value;
            }
        }
    }
}
