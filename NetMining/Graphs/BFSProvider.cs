using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Graphs
{
    class BFSProvider : ShortestPathProvider
    {
        public BFSProvider(LightWeightGraph g, int v)
            : base(g.Nodes.Length)
        {
            int numNodes = g.Nodes.Length;
            numberOfShortestPaths[v] = 1;
            bool[] isVisited = new bool[numNodes];

            //we must set each node to infinite distance
            for (int i = 0; i < numNodes; i++)
            {
                g.Nodes[i].NodeWeight = float.MaxValue;
                fromList[i] = new List<int>(5);
            }

            //now we need to set our node to 0
            g.Nodes[v].NodeWeight = 0.0f;

            Queue<int> Q = new Queue<int>();
            Q.Enqueue(v);
            isVisited[v] = true;
            while (Q.Count > 0)
            {
                //Grab an item
                int u = Q.Dequeue();
                this.S.Push(u);
                var nodeU = g.Nodes[u];
                int edgeCount = nodeU.Count;
                for (int i = 0; i < edgeCount; i++)
                {
                    int w = nodeU.Edge[i];
                    if (!isVisited[w])
                    {
                        float newWeight = nodeU.NodeWeight + 1.0f;
                        numberOfShortestPaths[w] += numberOfShortestPaths[u];
                        g.Nodes[w].NodeWeight = newWeight;
                        Q.Enqueue(w);
                        isVisited[w] = true;
                        fromList[w].Add(u);
                    }
                }
            }
        }
    }
}
