using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.ClusteringAlgo;
using NetMining.ExtensionMethods;

namespace NetMining.Graphs
{
    public class VertexCover
    {
        private List<int> _VC;

        public List<int> VC
        {
            get { return new List<int>(_VC); }
        }


        /// <summary>
        /// Sets the vertex cover for a graph
        /// </summary>
        /// <param name="lwg">Graph to find the vertex cover of</param>
        /// /// <param name="method">Three methods: Greedy, Alom and two-approximation</param>
        public VertexCover(LightWeightGraph lwg, int method = 1)
        {
            // all we do is find a vertex cover, given the graph lwg
            // vertex cover is 
            _VC = new List<int>();

            //We will make a copy of the graph
            LightWeightGraph g = new LightWeightGraph(lwg);

            // let's do the greedy method first!
            // This method adds maximum degree vertex to vertex cover,
            // removes edges, recalculate max, and repeats
            if (method == 1)
            {
                int maxNode = 0;
                int maxDegree = g.Nodes[0].Edge.Length;
                for (int i = 1; i < g.NumNodes; i++)
                {
                    if (g.Nodes[i].Edge.Length > maxDegree)
                    {
                        maxNode = i;
                        maxDegree = g.Nodes[i].Edge.Length;
                    }
                }
                while (maxDegree > 0)
                {
                    // Add max degree node to list
                    _VC.Add(maxNode);

                    // remove all edges from maxNode and terminating in maxNode
                    g[maxNode].Edge = new int[0];
                    for (int i = 0; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Contains(maxNode))
                        {
                            List<int> newEdges = new List<int>();
                            for (int j = 0; j < g.Nodes[i].Edge.Length; j++)
                            {
                                if (g.Nodes[i].Edge[j] != maxNode)
                                {
                                    newEdges.Add(g.Nodes[i].Edge[j]);
                                }
                            }
                            g.Nodes[i].Edge = newEdges.ToArray();
                        }
                    }

                    // find a new maxNode and maxDegree
                    maxDegree = 0;
                    for (int i = 0; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Length > maxDegree)
                        {
                            maxNode = i;
                            maxDegree = g.Nodes[i].Edge.Length;
                        }
                    }
                }

            } // end method1

            // Alom method is much like greedy method, except in cases of highest degree tie.
            // If there is a tie, it chooses the vertex that has an edge the is not covered by the 
            // vertices that are part of the tie.
            if (method == 2)
            {

                int maxNode = 0;
                int maxDegree = g.Nodes[0].Edge.Length;
                for (int i = 1; i < g.NumNodes; i++)
                {
                    if (g.Nodes[i].Edge.Length > maxDegree)
                    {
                        maxNode = i;
                        maxDegree = g.Nodes[i].Edge.Length;
                    }
                }
                // check for a tie
                List<int> tieMax = new List<int>();
                for (int i = 1; i < g.NumNodes; i++)
                {
                    if (g.Nodes[i].Edge.Length == maxDegree)
                    {
                        tieMax.Add(i);
                    }
                }
                // if there is a tie, choose the correct one
                if (tieMax.Count > 1)
                {
                    int[] scores = new int[tieMax.Count];
                    // calculate a coverage score for each tied vertex
                    for (int i = 0; i < tieMax.Count; i++) // i represents one of the tied nodes 
                    {
                        for (int j = 0; j < g.Nodes[tieMax[i]].Edge.Length; j++) // j represents the edges in i
                        {
                            for (int k = 0; k < tieMax.Count; k++) // k is used to iterate across the 3 
                            {
                                if (k != i && g.Nodes[tieMax[k]].Edge.Contains(g.Nodes[tieMax[i]].Edge[j]))
                                {
                                    scores[i]++;
                                    break;
                                }
                            }
                        }
                    }
                    // we have an array of scores, choose the one with the FEWEST edges covered by the other vertices
                    int tieMaxIndex = 0;
                    int tieMinValue = int.MaxValue;
                    for (int i = 0; i < tieMax.Count; i++)
                    {
                        if (scores[i] < tieMinValue)
                        {
                            tieMaxIndex = i;
                            tieMinValue = scores[i];
                        }
                    }
                    maxNode = tieMax[tieMaxIndex]; ;
                }

                while (maxDegree > 0)
                {
                    // Add max degree node to list
                    _VC.Add(maxNode);

                    // remove all edges from maxNode and terminating in maxNode
                    g[maxNode].Edge = new int[0];
                    for (int i = 0; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Contains(maxNode))
                        {
                            List<int> newEdges = new List<int>();
                            for (int j = 0; j < g.Nodes[i].Edge.Length; j++)
                            {
                                if (g.Nodes[i].Edge[j] != maxNode)
                                {
                                    newEdges.Add(g.Nodes[i].Edge[j]);
                                }
                            }
                            g.Nodes[i].Edge = newEdges.ToArray();
                        }
                    }

                    // find a new maxNode and maxDegree
                    maxDegree = 0;
                    for (int i = 0; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Length > maxDegree)
                        {
                            maxNode = i;
                            maxDegree = g.Nodes[i].Edge.Length;
                        }
                    }
                    // check for a tie
                    tieMax = new List<int>();
                    for (int i = 1; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Length == maxDegree)
                        {
                            tieMax.Add(i);
                        }
                    }
                    // if there is a tie, choose the correct one
                    if (tieMax.Count > 1)
                    {
                        int[] scores = new int[tieMax.Count];
                        // calculate a coverage score for each tied vertex
                        for (int i = 0; i < tieMax.Count; i++) // i represents one of the tied nodes 
                        {
                            for (int j = 0; j < g.Nodes[tieMax[i]].Edge.Length; j++) // j represents the edges in i
                            {
                                for (int k = 0; k < tieMax.Count; k++) // k is used to iterate across the 3 
                                {
                                    if (k != i && g.Nodes[tieMax[k]].Edge.Contains(g.Nodes[tieMax[i]].Edge[j]))
                                    {
                                        scores[i]++;
                                        break;
                                    }
                                }
                            }
                        }
                        // we have an array of scores, choose the one with the FEWEST edges covered by the other vertices
                        int tieMaxIndex = 0;
                        int tieMinValue = int.MaxValue;
                        for (int i = 0; i < tieMax.Count; i++)
                        {
                            if (scores[i] < tieMinValue)
                            {
                                tieMaxIndex = i;
                                tieMinValue = scores[i];
                            }
                        }
                        maxNode = tieMax[tieMaxIndex]; ;
                    }
                }
            }
            // Third method is the standard 2-approximation.  
            // It chooses an edge at random, adds both end vertices to the vertex cover, 
            // removes all adjacent edges, and repeats.
            
            if (method == 3)
            {
                // make sure there is a node with degree > 0
                int degree = 0;
                for (int i = 0; i < g.NumNodes; i++)
                {
                    if (g.Nodes[i].Edge.Length > 0)
                    {
                        degree = g.Nodes[i].Edge.Length;
                        break;
                    }
                }

                while (degree > 0)
                {
                    // I'm going to choose the edges randomly...
                    int node1 = 5;
                    do
                    {
                        node1 = (int)(Utility.Util.Rng.NextDouble() * g.Nodes.Length);
                    } while (g.Nodes[node1].Edge.Length == 0);

                    int randEdge = (int)(Utility.Util.Rng.NextDouble() * g.Nodes[node1].Edge.Length);
                    int node2 = g.Nodes[node1].Edge[randEdge];
                    // Add the two vertices to the vertex cover
                    _VC.Add(node1);
                    _VC.Add(node2);

                    // remove all edges from and terminating in node1 and node2
                    g[node1].Edge = new int[0];
                    g[node2].Edge = new int[0];
                    for (int i = 0; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Contains(node1) || g.Nodes[i].Edge.Contains(node2))
                        {
                            List<int> newEdges = new List<int>();
                            for (int j = 0; j < g.Nodes[i].Edge.Length; j++)
                            {
                                if (g.Nodes[i].Edge[j] != node1 && g.Nodes[i].Edge[j] != node2)
                                {
                                    newEdges.Add(g.Nodes[i].Edge[j]);
                                }
                            }
                            g.Nodes[i].Edge = newEdges.ToArray();
                        }
                    }

                    // make sure there is an uncovered edge
                    degree = 0;
                    for (int i = 0; i < g.NumNodes; i++)
                    {
                        if (g.Nodes[i].Edge.Length > 0)
                        {
                            degree = g.Nodes[i].Edge.Length;
                            break;
                        }
                    }

                }

            }

        }


    }
}
