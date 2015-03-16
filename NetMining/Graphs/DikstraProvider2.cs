using System.Collections.Generic;
using NetMining.ADT;
namespace NetMining.Graphs
{
    class DikstraProvider2 : ShortestPathProvider
    {
        public DikstraProvider2(LightWeightGraph g, int v) : base(g.Nodes.Length)
        
        {
            int numNodes = g.Nodes.Length;
            numberOfShortestPaths[v] = 1;
            fromList = new List<int>[numNodes]; //List of nodes (we will use this to 
            //countPostcessors = new int[numNodes]; //This will hold a count of the number of shortestpaths stemming from 
            
            //we must set each node to infinite distance
            for (int i = 0; i < numNodes; i++)
            {
                g.Nodes[i].NodeWeight = float.MaxValue;
                fromList[i] = new List<int>(5);
            }
            //now we need to set our node to 0
            g.Nodes[v].NodeWeight = 0.0f;

            //now we need to setup our heap
            ADT.IndexedItem[] items = new IndexedItem[numNodes];
            for(int i = 0; i < numNodes; i++)
            {
                var n = g.Nodes[i];
                items[i] = new IndexedItem(n.Id, n.NodeWeight);
            }
            MinHeapDikstra minHeap = new MinHeapDikstra(numNodes, items[v]);

            //dikstra main
            while (!minHeap.isEmpty())
            {
                var h = minHeap.extractMin();

                int uIndex = h.NodeIndex;
                this.S.Push(uIndex);
                //check all edges
                var u = g.Nodes[uIndex];
                int uEdgeCount = g.Nodes[uIndex].Count;
                for (int i = 0; i < uEdgeCount; i++)
                {
                    float newWeight = h.NodeWeight + u.EdgeWeights[i];
                    int toIndex = u.Edge[i];
                    var to = items[toIndex];
                    float toNodeWeight = to.NodeWeight;
                    if (newWeight < toNodeWeight)
                    {
                        to.NodeWeight = newWeight;
                        fromList[toIndex].Clear();
                        fromList[toIndex].Add(uIndex);
                        numberOfShortestPaths[toIndex] = numberOfShortestPaths[uIndex];
                        if (to.HeapIndex == -1) //forst encounter
                        {
                            minHeap.addItem(to);
                        }
                        else
                        {
                            minHeap.decreaseKey(to.HeapIndex);
                        }
                    }
                     else if (newWeight == toNodeWeight)
                    {
                        fromList[toIndex].Add(uIndex);//Add the node
                        numberOfShortestPaths[toIndex] += numberOfShortestPaths[uIndex];
                    }
                }
            }
        }


    }
}
