using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.ADT;
using NetMining.Data;

namespace NetMining.Graphs.Generator
{
    public class LeaveOutGraphGenerator : IPointGraphGenerator
    {

        private enum KNNGraphType
        {
            MinimumConnectivity,
            SpecifiedK
        }

        private KNNGraphType _graphType = KNNGraphType.MinimumConnectivity;
        private int _k = 1;
        private int _minKNNOffset = 0;
        private bool _skipLast = false;
        private int _numSkip = 0;
        public int _numLeftOut = 0;
        private int _numLeaveOut = 0;
        static Random rnd = Utility.Util.Rng;
        /// <summary>
        /// Sets the number of neighbors for a created graph
        /// </summary>
        /// <param name="k">Number of neighbors</param>
        public void SetK(int k)
        {
            _graphType = KNNGraphType.SpecifiedK;
            _k = k;
            if (k <= 0)
                _k = 1;
        }

        /// <summary>
        /// Sets the Generator to create minimum connectivity graphs
        /// </summary>
        public void SetMinimumConnectivity()
        {
            _graphType = KNNGraphType.MinimumConnectivity;
        }

        public void SetMinOffset(int offset)
        {
            _minKNNOffset = offset;
        }

        public void SetSkipLast(bool skip)
        {
            _skipLast = skip;
        }

        public void SetCountSkip(int count)
        {
            _numSkip = count;
        }
        public void SetNumLeaveOut(int count)
        {
            _numLeaveOut = count;
        }
        public LightWeightGraph GenerateGraph(DistanceMatrix d)
        {
            if (_graphType == KNNGraphType.SpecifiedK)
            {
                if (_k > d.Count - 1)
                    return GetLOOGraph(d, d.Count - 1);
                return GetLOOGraph(d, _k);
            }
            if (_graphType == KNNGraphType.MinimumConnectivity)
            {
                int K = 1;
                if (_numSkip == 0)
                    K = QuadraticMinKNN(d) + _minKNNOffset;
                else
                {
                    Dictionary<int, int> kCount = new Dictionary<int, int>();
                    for (int i = 0; i < 10; i++)
                    {
                        int kRun = QuadraticMinKNN(d) + _minKNNOffset;
                        if (kCount.ContainsKey(kRun))
                            kCount[kRun]++;
                        else
                            kCount.Add(kRun, 1);
                    }

                    int max = 0;
                    int maxIndex = 0;
                    foreach (var kv in kCount)
                    {
                        Console.Write("{0}:{1},", kv.Key, kv.Value);
                        if (kv.Value > max)
                        {
                            maxIndex = kv.Key;
                            max = kv.Value;
                        }
                    }
                    Console.WriteLine();
                    K = maxIndex;
                }
                Console.WriteLine("Selected K:{0}", K);
                return GetLOOGraph(d, K, null, _numLeaveOut);
            }
            throw new InvalidEnumArgumentException("Invalid _graphType");
        }

        public int QuadraticMinKNN(DistanceMatrix distance)
        {
            int min = 1;
            int max = distance.Count - 1;
            int highestDisconnected = min;
            int lowestConnected = max;
            int numNodes = distance.Count;

            bool[] exclusion = new bool[numNodes];

            int countExcluded = 0;
            while (countExcluded < _numSkip)
            {
                int index = Utility.Util.Rng.Next(0, numNodes);
                if (!exclusion[index])
                {
                    exclusion[index] = true;
                    countExcluded++;
                }
            }

            //This dictionary is used to store results so recalculations are not needed
            Dictionary<int, bool> connectedDict = new Dictionary<int, bool>();
            int inc = 1;
            for (int i = min; i < max; i += (inc * inc))
            {
                if (i > lowestConnected)
                {
                    inc = 1;
                    i = highestDisconnected;
                    continue;
                }

                if (connectedDict.ContainsKey(i))
                    continue;

                bool isConn = GetLOOGraph(distance, i, exclusion).isConnected();
                connectedDict.Add(i, isConn);

                if (isConn && i < lowestConnected)
                {
                    lowestConnected = i;
                    if (connectedDict.ContainsKey(lowestConnected - 1) && !connectedDict[lowestConnected - 1])
                        return lowestConnected;
                }
                else if (!isConn && i > highestDisconnected)
                {
                    highestDisconnected = i;
                    if (connectedDict.ContainsKey(highestDisconnected + 1) && connectedDict[highestDisconnected + 1])
                        return highestDisconnected + 1;
                }
                inc++;
            }

            return lowestConnected;
        }

        public LightWeightGraph GetLOOGraph(DistanceMatrix distances, int numNeighbors, bool[] exclusion = null, int numToLeaveOut = 0)
        {
            int numNodes = distances.Count;

            var nodes = new LightWeightGraph.LightWeightNode[numNodes];

            List<int>[] edgeLists = new List<int>[numNodes];
            List<double>[] edgeWeights = new List<double>[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                edgeLists[i] = new List<int>();
                edgeWeights[i] = new List<double>();
            }

            //prevent redundant edges
            HashSet<Tuple<int, int>> addedEdges = new HashSet<Tuple<int, int>>();
            //Our comparator
            MinHeapPriorityQueue<Tuple<int, double>>.isGreaterThan comp = ((x, y) => x.Item2 > y.Item2);

            //Deal with _skipLast Choice
            int lastNeighbor = (_skipLast) ? numNodes - 1 : numNodes;
            //Add Edges
            for (int i = 0; i < lastNeighbor; i++)
            {
                
                if (exclusion != null && exclusion[i])
                    continue;
                // Deal with the leave one out
                double probability = (double)numToLeaveOut / numNodes;
                double rollDie = rnd.NextDouble();
                if (rollDie < probability)
                {
                    //Console.WriteLine("Skipped Node " + i);
                    _numLeftOut++;
                    continue;
                }

                
                //get list of edges
                List<Tuple<int, double>> edges = new List<Tuple<int, double>>();
                for (int j = 0; j < numNodes; j++)
                {
                    //Make sure we don't load our heap with repeated edges
                    if (i != j)
                    {
                        edges.Add(new Tuple<int, double>(j, distances[i, j]));
                    }
                }
                //Build the heap
                MinHeapPriorityQueue<Tuple<int, double>> heap = new MinHeapPriorityQueue<Tuple<int, double>>(comp);
                heap.addAll(edges);

                //Now add all of the neighbors
                for (int edgeNum = 0; edgeNum < numNeighbors; edgeNum++)
                {
                    if (heap.isEmpty())
                        break;

                    Tuple<int, double> e = heap.extractMin();

                    Tuple<int, int> edgeNodePair = (e.Item1 < i)
                            ? new Tuple<int, int>(e.Item1, i)
                            : new Tuple<int, int>(i, e.Item1);


                    //if (!addedEdges.Contains(edgeNodePair))
                    if (!addedEdges.Contains(edgeNodePair))
                    {
                        //make sure we don't add this edge again in the future
                        //addedEdges.Add(edgeNodePair);
                        addedEdges.Add(edgeNodePair);
                        //Add the double edge now
                        edgeLists[i].Add(e.Item1);
                        edgeLists[e.Item1].Add(i);
                        edgeWeights[i].Add((double)e.Item2);
                        edgeWeights[e.Item1].Add((double)e.Item2);
                    }
                }
            }

            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightGraph.LightWeightNode(i, true, edgeLists[i], edgeWeights[i]);

            return new LightWeightGraph(nodes, true);
        }
    }
}
