using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMining.ExtensionMethods;

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
                    }
                    if (w != v)
                        bcMap[w] += delta[w];
                }
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numnodes; v++)
                bcMap[v] /= 2f;

            return bcMap;
        }

        /// <summary>
        /// Calculates Node based Betweeness Centrality using multiple threads
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static float[] ParallelBrandesBcNodes(LightWeightGraph g)
        {
            int numNodes = g.NumNodes;
            int numThreads = Settings.Threading.NumThreadsBc;
            int workSize = numNodes/numThreads;
            int workSizeExtra = numNodes%numThreads;

            //Start getting a randomized work load
            List<int> nodes = new List<int>(numNodes);
            for (int i = 0; i < numNodes; i++)
                nodes.Add(i);
            nodes.Shuffle();

            //Create an array of work items for each thread and assign the nodes in a randomized order
            int[][] workItems = new int[numThreads][];
            for (int t = 0; t < numThreads; t++)
            {
                int size = workSize + (t == (numThreads - 1) ? workSizeExtra : 0);
                workItems[t] = new int[size];
                for (int i = 0; i < size; i++)
                    workItems[t][i] = nodes[t*workSize + i];
            }
            
            //Create our threads use a closure to get our return arrays
            BetweenessCalc[] threadResults = new BetweenessCalc[numThreads];
            //ManualResetEvent[] waitHandles = new ManualResetEvent[numThreads];
            CountdownEvent cde = new CountdownEvent(numThreads);
            for (int t = 0; t < numThreads; t++)
            {
                int tIndex = t;
                //waitHandles[tIndex] = new ManualResetEvent(false);
                BetweenessCalc tResult = new BetweenessCalc(g, workItems[tIndex]);
                threadResults[tIndex] = tResult;
                ThreadPool.QueueUserWorkItem(tResult.ThreadPoolCallback, cde);
            }
            cde.Wait();
            //WaitHandle.WaitAll(waitHandles);

            //Create our betweeness map and sum all of the thread results
            float[] bcMap = new float[numNodes];
            for (int t = 0; t < numThreads; t++)
            {
                var threadR = threadResults[t].BcMap;
                for (int n = 0; n < numNodes; n++)
                    bcMap[n] += threadR[n];
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numNodes; v++)
                bcMap[v] /= 2f;

            return bcMap;
        }

        internal class BetweenessCalc
        {
            private readonly LightWeightGraph _g;
            private readonly int[] _work;
            public float[] BcMap;
            internal BetweenessCalc(LightWeightGraph g, int[] work)
            {
                _g = g;
                _work = work;
                BcMap = new float[g.NumNodes];
            }

            public void ThreadPoolCallback(Object o)
            {
                PartialBetweenessComp();
                ((CountdownEvent) o).Signal();
            }

            private void PartialBetweenessComp()
            {
                int numIndices = _work.Length;
                int numNodes = _g.NumNodes;

                float[] delta = new float[numNodes];
                for (int i = 0; i < numIndices; i++)
                {
                    int v = _work[i];
                    //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                    ShortestPathProvider asp = (_g.IsWeighted) ? new DikstraProvider2(_g, v) :
                                                                new BFSProvider(_g, v) as ShortestPathProvider;

                    for (int j = 0; j < numNodes; j++)
                        delta[j] = 0.0f;

                    while (asp.S.Count > 0)
                    {
                        int w = asp.S.Pop();
                        var wList = asp.fromList[w];
                        foreach (int n in wList)
                        {
                            delta[n] += ((float)asp.numberOfShortestPaths[n] / (float)asp.numberOfShortestPaths[w]) * (1.0f + delta[w]);
                        }
                        if (w != v)
                            BcMap[w] += delta[w];
                    }
                }
            }
        }

        /// <summary>
        /// Calculates Betweeness centrality of the edges in an undirected graph
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static NodeEdgeBetweeness BrandesBcEdges(LightWeightGraph g)
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

            for (int v = 0; v < numNodes; v++)
                bcNode[v] /= 2f;

            return new NodeEdgeBetweeness() {EdgeBetweeness = bcEdge, NodeBetweeness = bcNode};
        }

        public struct NodeEdgeBetweeness
        {
            public float[] NodeBetweeness, EdgeBetweeness;
        }
    }
}
