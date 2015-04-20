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

        public static double[] BrandesBcNodes(LightWeightGraph g)
        {
            int numnodes = g.NumNodes;
            double[] bcMap = new double[numnodes];
            double[] delta = new double[numnodes];
            for (int v = 0; v < numnodes; v++)
            {
                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (g.IsWeighted) ? new DikstraProvider2(g, v) : 
                                                            new BFSProvider(g,v) as ShortestPathProvider;
                Array.Clear(delta, 0, numnodes);
                
                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    var wList = asp.fromList[w];
                    foreach (int n in wList)
                    {
                        delta[n] += ((double)asp.numberOfShortestPaths[n] / (double)asp.numberOfShortestPaths[w]) * (1.0f + delta[w]);
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
        public static double[] ParallelBrandesBcNodes2(LightWeightGraph g)
        {
            
            int numNodes = g.NumNodes;
            int numThreads = Settings.Threading.NumThreadsBc;
            int numExtra = numNodes%numThreads;

            double[] bcMap = new double[numNodes];
            //Create our threads use a closure to get our return arrays
            
            int i = 0;
            while (i < numNodes)
            {
                int numT = (numNodes - i > numExtra) ? numThreads : numExtra;
                CountdownEvent cde = new CountdownEvent(numT);
                BetweenessCalc2[] threadResults = new BetweenessCalc2[numT];
                for (int t = 0; t < numT; t++)
                {
                    int tIndex = t;
                    BetweenessCalc2 tResult = new BetweenessCalc2(g, i+t);
                    threadResults[tIndex] = tResult;
                    ThreadPool.QueueUserWorkItem(tResult.ThreadPoolCallback, cde);
                }
                cde.Wait();
                for (int t = 0; t < numT; t++)
                {
                    var threadR = threadResults[t].delta;
                    for (int n = 0; n < numNodes; n++)
                        bcMap[n] += threadR[n];
                }
                i += numThreads;
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numNodes; v++)
                bcMap[v] /= 2f;

            return bcMap;
        }

        internal class BetweenessCalc2
        {
            private readonly LightWeightGraph _g;
            private readonly int v;
            public double[] delta;
            internal BetweenessCalc2(LightWeightGraph g, int work)
            {
                _g = g;
                v = work;
                delta = new double[g.NumNodes];
            }

            public void ThreadPoolCallback(Object o)
            {
                PartialBetweenessComp();
                ((CountdownEvent)o).Signal();
            }

            private void PartialBetweenessComp()
            {
                for (int i = 0; i < delta.Length; i++)
                    delta[i] = 0;

                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (_g.IsWeighted)
                    ? new DikstraProvider2(_g, v)
                    : new BFSProvider(_g, v) as ShortestPathProvider;

                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    var wList = asp.fromList[w];
                    foreach (int n in wList)
                    {
                        delta[n] += ((double) asp.numberOfShortestPaths[n]/ asp.numberOfShortestPaths[w])*
                                    (1.0f + delta[w]);
                    }
                }
                delta[v] = 0;
            }
        }

        /// <summary>
        /// Calculates Node based Betweeness Centrality using multiple threads
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static double[] ParallelBrandesBcNodes(LightWeightGraph g)
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
            double[] bcMap = new double[numNodes];
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
            public double[] BcMap;
            internal BetweenessCalc(LightWeightGraph g, int[] work)
            {
                _g = g;
                _work = work;
                BcMap = new double[g.NumNodes];
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

                double[] delta = new double[numNodes];
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
                            delta[n] += ((double)asp.numberOfShortestPaths[n] / (double)asp.numberOfShortestPaths[w]) * (1.0f + delta[w]);
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
            double[] bcEdge = new double[numEdges];
            double[] bcNode = new double[numNodes];
            for (int v = 0; v < numNodes; v++)
            {
                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (g.IsWeighted) ? new DikstraProvider2(g, v) :
                                                            new BFSProvider(g, v) as ShortestPathProvider;

                //numberOfShortestPaths = sigma
                double[] deltaNode= new double[numNodes];
                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    double coeff = (1.0f + deltaNode[w]) / (double)asp.numberOfShortestPaths[w];
                    foreach (int n in  asp.fromList[w])
                    {
                        //make sure the first index is the smallest, this is an undirected graph
                        KeyValuePair<int, int> edgeNodePair = (w < n)
                            ? new KeyValuePair<int, int>(w, n)
                            : new KeyValuePair<int, int>(n, w);

                        int edgeIndex = edgeMap[edgeNodePair];
                        double contribution = asp.numberOfShortestPaths[n] * coeff;
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
            public double[] NodeBetweeness, EdgeBetweeness;
        }
    }
}
