using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Graphs
{
    abstract class ShortestPathProvider
    {
        public List<int>[] fromList;
       // public int[] countPostcessors;
        public int[] numberOfShortestPaths;
        public Stack<int> S;
        public ShortestPathProvider(int numNodes)
        {
            fromList = new List<int>[numNodes]; //List of nodes (we will use this to 
           // countPostcessors = new int[numNodes]; //This will hold a count of the number of shortestpaths stemming from 
            numberOfShortestPaths = new int[numNodes];
            S = new Stack<int>();
        }
    }
}
