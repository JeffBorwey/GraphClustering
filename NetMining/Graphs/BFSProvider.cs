using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Graphs
{
    class BFSProvider : ShortestPathProvider
    {
        public BFSProvider(LightWeightGraph g, int startV)
            : base(g.Nodes.Length)
        {
            int numNodes = g.Nodes.Length;
            numberOfShortestPaths[startV] = 1;
            bool[] isVisited = new bool[numNodes];
            int[] dist = new int[numNodes];

            for (int i = 0; i < numNodes; i++)
            {
                fromList[i] = new List<int>(5);
            }

            //now we need to set our node to 0
            dist[startV] = 0;

            Queue<int> Q = new Queue<int>();
            Q.Enqueue(startV);
            isVisited[startV] = true;
            while (Q.Count > 0)
            {
                //Grab an item
                int v = Q.Dequeue();
                S.Push(v);
                var nodeV = g.Nodes[v];
                int dV = dist[v];

                int edgeCount = nodeV.Count;
                for (int i = 0; i < edgeCount; i++)
                {
                    int w = nodeV.Edge[i];
                    
                    if (!isVisited[w])
                    {
                        Q.Enqueue(w);
                        isVisited[w] = true;
                        dist[w] = dV + 1;
                    }
                    if (dist[w] == (dV + 1))
                    {
                        numberOfShortestPaths[w] += numberOfShortestPaths[v];
                        fromList[w].Add(v);
                    }
                }
            }
        }
    }
}
