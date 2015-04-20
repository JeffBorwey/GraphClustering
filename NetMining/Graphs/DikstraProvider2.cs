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

            
            for (int i = 0; i < numNodes; i++)
            {
                fromList[i] = new List<int>();
            }

            //now we need to setup our heap
            //set each node to infinite distance
            ADT.IndexedItem[] items = new IndexedItem[numNodes];
            for(int i = 0; i < numNodes; i++)
            {
                items[i] = new IndexedItem(i, double.MaxValue);
            }
            items[v].NodeWeight = 0;
            MinHeapDikstra minHeap = new MinHeapDikstra(numNodes, items[v]);

            //dikstra main
            while (!minHeap.isEmpty())
            {
                var h = minHeap.extractMin();

                int uIndex = h.NodeIndex;
                S.Push(uIndex);

                //check all edges
                var u = g.Nodes[uIndex];
                int uEdgeCount = g.Nodes[uIndex].Count;
                for (int i = 0; i < uEdgeCount; i++)
                {
                    double newWeight = h.NodeWeight + u.EdgeWeights[i];
                    int toIndex = u.Edge[i];
                    var to = items[toIndex];
                    double toNodeWeight = to.NodeWeight;
                    if (newWeight < toNodeWeight)
                    {
                        to.NodeWeight = newWeight;
                        fromList[toIndex].Clear();
                        fromList[toIndex].Add(uIndex);
                        numberOfShortestPaths[toIndex] = numberOfShortestPaths[uIndex];
                        if (to.HeapIndex == -1) //first encounter
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
