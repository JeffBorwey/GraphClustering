using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMining.ExtensionMethods;
namespace NetMining.Graphs
{
    public class VAT
    {
        internal bool[] removedNodes;
        internal LightWeightGraph g;
        internal List<int> nodeRemovalOrder;

        internal int numNodesRemoved = 0;
        internal float minVat = float.MaxValue;//This will hold the best score so far
        internal float alpha, beta;
        //Vat computes given a graph
        public VAT(LightWeightGraph lwg, float alpha = 1.0f, float beta = 0.0f)
        {
            //set our alpha and beta variables
            this.alpha = alpha; this.beta = beta;

            //first we set our variables up
            removedNodes = new bool[lwg.NumNodes];
            nodeRemovalOrder = new List<int>();
            //We will make a copy of the graph and set the label equal to the index
            g = new LightWeightGraph(lwg, removedNodes);
            for (int i = 0; i < g.NumNodes; i++)
                g.Nodes[i].Label = i;

            //This is where our estimate for Vat is calculated
            for (int n = 0; n < g.NumNodes / 2; n++)  // this was 32, I think a typo?
            {
                //get the graph
                LightWeightGraph gItter = new LightWeightGraph(g, removedNodes);

                //get the betweeness
                float[] BC = BetweenessCentrality.BrandesBc(gItter);

                //get the index of the maximum
                int indexMaxBC = BC.IndexOfMax();
                int labelOfMax = gItter.Nodes[indexMaxBC].Label;

                //now we should add it to our list 
                nodeRemovalOrder.Add(labelOfMax);
                removedNodes[labelOfMax] = true;
                //calculate vat and update the record
                float vat = CalculateVAT(removedNodes);
                if (vat < minVat)
                {
                    minVat = vat;
                    numNodesRemoved = n + 1;
                }
                Console.WriteLine("Node {0} removed", n);
            }

            //Now we need to set up S to reflect the actual minimum
            for (int i = 0; i < removedNodes.Length; i++)
                removedNodes[i] = false;
            for (int i = 0; i < numNodesRemoved; i++)
                removedNodes[nodeRemovalOrder[i]] = true;

            //hillclimbing would go here
        }

        public LightWeightGraph GetAttackedGraph()
        {
            return new LightWeightGraph(g, removedNodes);
        }

        //Clean up with GetComponents
        public LightWeightGraph GetAttackedGraphWithReassignment()
        {
            LightWeightGraph.LightWeightNode[] nodes = new LightWeightGraph.LightWeightNode[g.NumNodes];

                
            //get the connectivity structure
            List<int>[] edges = new List<int>[g.NumNodes];
            List<float>[] edgeWeights = new List<float>[g.NumNodes];

            for (int i = 0; i < g.NumNodes; i++)
            {
                List<int> edgeList = new List<int>();
                List<float> weightList = new List<float>();
                edges[i] = edgeList;
                edgeWeights[i] = weightList;
            }

            //Now do a BFS to figure out what each node belongs to
            int[] componentIndex = new int[g.NumNodes];

            //This will provide our visited flags for BFS
            bool[] isVisited = (bool[])removedNodes.Clone();

            int componentId = 1;
            Queue<int> q = new Queue<int>();
            for (int i = 0; i < g.NumNodes; i++)
            {
                if (!isVisited[i])
                {
                    //BFS to count the size of the component
                    q.Enqueue(i);
                    isVisited[i] = true;
                    while (q.Count > 0)
                    {
                        int v = q.Dequeue();
                        componentIndex[v] = componentId;
                        foreach (int u in g.Nodes[v].Edge)
                            if (!isVisited[u])
                            {
                                q.Enqueue(u);
                                isVisited[u] = true;
                            }
                    }
                    componentId++;
                }
            }

            //Assign the nodes to a component based on degree count
            for (int i = 0; i < numNodesRemoved; i++)
            {
                int n = nodeRemovalOrder[i];
                int[] componentDegreeCount = new int[componentId];
                for (int e = 0; e < g.Nodes[n].Edge.Count(); e++)
                {
                    int adjacentNode = g.Nodes[n].Edge[e];
                    componentDegreeCount[componentIndex[adjacentNode]]++;
                }

                //Now we must pick the biggest
                int comp = 1;
                for (int c = 1; c < componentDegreeCount.Length; c++)
                {
                    if (componentDegreeCount[c] > componentDegreeCount[comp])
                        comp = c;
                }
                componentIndex[n] = comp;
            }

            //Now that we have the components, we can build our edge list
            for (int v = 0; v < g.NumNodes; v++)
            {
                LightWeightGraph.LightWeightNode n = g.Nodes[v];
                for (int e = 0; e < n.Edge.Count(); e++)
                {
                    int edge = n.Edge[e];
                    //If they are in the same component, we can add the edge safely
                    if (componentIndex[v] == componentIndex[edge])
                    {
                        edges[v].Add(edge);
                        if (g.IsWeighted)
                            edgeWeights[v].Add(n.EdgeWeights[e]);
                    }
                }
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new LightWeightGraph.LightWeightNode(i, g.IsWeighted, edges[i], edgeWeights[i]);
            }

            return new LightWeightGraph(nodes, g.IsWeighted);
        }

        //convert to using lwg.GetComponents
        public String GetClusters()
        {
            StringBuilder sb = new StringBuilder();

            //This will provide our visited flags for BFS
            bool[] isVisited = (bool[])removedNodes.Clone();

            int componentId = 0;
            Queue<int> q = new Queue<int>();
            for (int i = 0; i < g.NumNodes; i++)
            {
                if (!isVisited[i])
                {
                    //BFS to count the size of the component
                    sb.AppendLine("Cluster ID: " + componentId.ToString());
                    q.Enqueue(i);
                    isVisited[i] = true;
                    while (q.Count > 0)
                    {
                        int v = q.Dequeue();
                        sb.Append(v.ToString() + " ");
                        foreach (int u in g.Nodes[v].Edge)
                            if (!isVisited[u])
                            {
                                q.Enqueue(u);
                                isVisited[u] = true;
                            }
                    }
                    sb.AppendLine();
                    componentId++;
                }
            }

            sb.AppendLine(); sb.AppendLine(); sb.AppendLine("Removed Nodes: ");

            for (int i = 0; i < numNodesRemoved; i++)
                sb.Append(nodeRemovalOrder[i].ToString() + " ");

            return sb.ToString();
        }

        //Use GetComponents
        private float CalculateVAT(bool[] S)
        {
            //This will provide our visited flags for BFS
            bool[] isVisited = (bool[])S.Clone();

            //We must get the size of S
            int sizeS = S.Count(c => c);

            if (sizeS == 0)
                return float.MaxValue;


            int cMax = 0;
            Queue<int> q = new Queue<int>();
            for (int i = 0; i < g.NumNodes; i++)
            {
                if (!isVisited[i])
                {
                    //BFS to count the size of the component
                    int sizeComponent = 0;

                    q.Enqueue(i);
                    isVisited[i] = true;
                    while (q.Count > 0)
                    {
                        int v = q.Dequeue();
                        sizeComponent++;
                        foreach (int u in g.Nodes[v].Edge)
                            if (!isVisited[u])
                            {
                                q.Enqueue(u);
                                isVisited[u] = true;
                            }
                    }
                    cMax = Math.Max(cMax, sizeComponent);
                }
            }

            //return (float)sizeS / (float)(g.numNodes - sizeS - cMax + 1);
            return (float)(alpha * sizeS + beta) / (float)(g.NumNodes - sizeS - cMax + 1);
        }
    }
}
