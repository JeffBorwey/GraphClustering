using System;
using System.Collections.Generic;

namespace NetMining.Graphs
{
    static public class BetweenessCentrality
    {

        public static float[] BrandesBcNodes(LightWeightGraph g)
        {
            int numnodes = g.NumNodes;
            float[] bcMap = new float[numnodes];
            
            for (int v = 0; v < numnodes; v++)
            {
                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (g.IsWeighted) ? new DikstraProvider2(g, v) : 
                                                            new BFSProvider(g,v) as ShortestPathProvider;
                float[] delta = new float[numnodes];
                
                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    var wList = asp.fromList[w];
                    foreach (int n in wList)
                    {
                        delta[n] += ((float)asp.numberOfShortestPaths[n] / (float)asp.numberOfShortestPaths[w]) * (1.0f + delta[w]);
                        if (n != v)
                            bcMap[n] += delta[n];
                    }
                }
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numnodes; v++)
                bcMap[v] /= 2f;

            return bcMap;
        }

        /// <summary>
        /// Calculates Betweeness centrality of the edges in an undirected graph
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static float[] BrandesBcEdges(LightWeightGraph g)
        {
            var edgeMap = g.GetEdgeIndexMap();
            int numNodes = g.NumNodes;
            int numEdges = edgeMap.Count;
            float[] bcEdge = new float[numEdges];
            float[] bcNode = new float[numNodes];
            for (int v = 0; v < numNodes; v++)
            {
                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (g.IsWeighted) ? new DikstraProvider2(g, v) :
                                                            new BFSProvider(g, v) as ShortestPathProvider;

                //numberOfShortestPaths = sigma
                float[] deltaNode= new float[numNodes];
                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    float coeff = (1.0f + deltaNode[w]) / (float)asp.numberOfShortestPaths[w];
                    foreach (int n in  asp.fromList[w])
                    {
                        //make sure the first index is the smallest, this is an undirected graph
                        KeyValuePair<int, int> edgeNodePair = (w < n)
                            ? new KeyValuePair<int, int>(w, n)
                            : new KeyValuePair<int, int>(n, w);

                        int edgeIndex = edgeMap[edgeNodePair];
                        float contribution = asp.numberOfShortestPaths[n] * coeff;
                        bcEdge[edgeIndex] += contribution;
                        deltaNode[n] += contribution;
                    }
                    //Add the betweeness contribution to W
                    if (v != w)
                        bcNode[w] += deltaNode[w];
                }
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numEdges; v++)
                bcEdge[v] /= 2f;

            return bcEdge;
        }
    }
}
