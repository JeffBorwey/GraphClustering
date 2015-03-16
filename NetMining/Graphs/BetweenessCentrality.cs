using System;
using System.Collections.Generic;

namespace NetMining.Graphs
{
    static class BetweenessCentrality
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

        public static float[] BrandesBcEdges(LightWeightGraph g)
        {
            var edgeMap = g.GetEdgeIndexMap();
            int numnodes = g.NumNodes;
            float[] bcMap = new float[edgeMap.Count];
            for (int v = 0; v < numnodes; v++)
            {
                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (g.IsWeighted) ? new DikstraProvider2(g, v) :
                                                            new BFSProvider(g, v) as ShortestPathProvider;

                float[] deltaEdge = new float[edgeMap.Count];
                float[] deltaNode = new float[g.Nodes.Length];
                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    var wList = asp.fromList[w];
                    float coeff = (1.0f + deltaNode[w]) / asp.numberOfShortestPaths[w];
                    foreach (int n in wList)
                    {
                        //make sure the first index is the smallest, this is an undirected graph
                        KeyValuePair<int, int> edgeNodePair = (w < n)
                            ? new KeyValuePair<int, int>(w, n)
                            : new KeyValuePair<int, int>(n, w);
                        if (!edgeMap.ContainsKey(edgeNodePair)) //This should never happen, undirected graph
                            Console.WriteLine("Error");
                        int edgeIndex = edgeMap[edgeNodePair];
                        float contribution = asp.numberOfShortestPaths[n] * coeff;
                        bcMap[edgeIndex] += contribution;
                        deltaNode[w] += contribution;
                    }
                    //Add the betweeness contribution to W
                    //if (v != w)
                    //
                }
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numnodes; v++)
                bcMap[v] /= 2f;

            return bcMap;
        }
    }
}
