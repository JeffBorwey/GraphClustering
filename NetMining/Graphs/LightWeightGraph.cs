using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NetMining.ADT;
using NetMining.Data;
using NetMining.Files;

namespace NetMining.Graphs
{
    using GraphStringRep = List<List<EdgeValuePair<String>>>;

    public class LightWeightGraph
    {
        public LightWeightNode[] Nodes;
        public int NumNodes;
        public Boolean IsWeighted;

        public static bool GeoGraphIsConnected(DistanceMatrix distances, float threshold)
        {
            return GetGeometricGraph(distances, threshold).isConnected();
        }

        public static bool KNNGraphIsConnected(DistanceMatrix distances, int neighbors)
        {
            return GetKNNGraph(distances, neighbors).isConnected();
        }

        public GraphStringRep getGSR()
        {
            GraphStringRep gsr = new GraphStringRep();

            for (int i = 0; i < NumNodes; i++)
            {
                List<EdgeValuePair<String>> l = new List<EdgeValuePair<String>>();
                gsr.Add(l);
                l.Add(new EdgeValuePair<String>(i.ToString(), 1));
                foreach (var a in Nodes[i].Edge)
                    l.Add(new EdgeValuePair<String>(a.ToString(), 1));
            }

            return gsr;
        }

        public LightWeightGraph()
        {
            Nodes = new LightWeightNode[1];
            NumNodes = 1;
            Nodes[0] = new LightWeightNode(0, false, new List<int>());
            IsWeighted = false;
        }

        //Threshold based
        public static LightWeightGraph GetGeometricGraph(DistanceMatrix distances, float threshold)
        {
            //construct the geo graph
            int numNodes = distances.Count;
            var nodes = new LightWeightNode[numNodes];

            List<int>[] edges = new List<int>[numNodes];
            List<float>[] weights = new List<float>[numNodes];
            for (int i = 0; i < numNodes; i++)
                edges[i] = new List<int>();
            //Add Edges
            for (int i = 0; i < numNodes - 1; i++)
            {
                for (int j = i + 1; j < numNodes; j++)
                    if (distances[i, j] <= threshold)
                    {
                        edges[i].Add(j);
                        edges[j].Add(i);
                    }
            }
            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightNode(i, true, edges[i], weights[i]);

            return new LightWeightGraph(nodes, true);
        }

        internal LightWeightGraph(LightWeightNode[] nodes, Boolean isWeighted)
        {
            Nodes = nodes;
            NumNodes = nodes.Length;
            IsWeighted = isWeighted;
        }

        /// <summary>
        /// Constructs a map from a 2 node pair to the edge index
        /// </summary>
        /// <returns>Returns a dictionary mapping edges to indexes</returns>
        public Dictionary<KeyValuePair<int, int>, int> GetEdgeIndexMap()
        {
            var map = new Dictionary<KeyValuePair<int, int>, int>();

            foreach (var node in Nodes)
            {
                foreach (int edgeTo in node.Edge.Where(edgeTo => node.Id < edgeTo))
                {
                    map.Add(new KeyValuePair<int, int>(node.Id, edgeTo), map.Count);
                }
            }

            return map;
        }

        class distXY
        {
            internal readonly int X, Y;
            internal double Dist;
            public distXY(int x, int y, double dist) { X = x; Y = y; Dist = dist; }
        }

        //construct a subgraph using some exclusion rules
        public LightWeightGraph(LightWeightGraph lwg, bool[] S)
        {
            int sSize = S.Count(c => c);

            //Setup our node array to be filled
            Nodes = new LightWeightNode[lwg.NumNodes - sSize];
            NumNodes = Nodes.Length;
            IsWeighted = lwg.IsWeighted;

            int nodeID = 0;
            int[] oldIDToNewID = new int[lwg.NumNodes];
            int[] oldLabel = new int[NumNodes];
            //Now we need to itterate over each node in lwg
            for (int v = 0; v < lwg.NumNodes; v++)
            {
                if (!S[v])
                {
                    oldIDToNewID[v] = nodeID;
                    oldLabel[nodeID] = lwg.Nodes[v].Label;
                    nodeID++;
                }
            }

            List<int>[] edgesList = new List<int>[NumNodes];
            List<float>[] edgeWeightList = new List<float>[NumNodes];
            for (int i = 0; i < lwg.NumNodes - sSize; i++)
            {
                edgesList[i] = new List<int>();
                edgeWeightList[i] = new List<float>();
            }

            //now we should add our edges
            nodeID = 0;
            for (int v = 0; v < lwg.NumNodes; v++)
            {
                if (!S[v]) //if this is not a removed node we should add the edges
                {
                    var edges = lwg.Nodes[v].Edge;
                    var edgeWeights = lwg.Nodes[v].EdgeWeights;
                    //Go through all of the edges and only add those not removed
                    for (int u = 0; u < lwg.Nodes[v].Count; u++)
                    {
                        int edgeTo = edges[u];
                        if (!S[edgeTo]) //this edge is still valid so we should add it
                        {
                            edgesList[nodeID].Add(oldIDToNewID[edgeTo]);
                            if (lwg.IsWeighted)
                                edgeWeightList[nodeID].Add(edgeWeights[u]);
                        }
                    }
                    nodeID++;
                }
            }

            for (int i = 0; i < NumNodes; i++)
                Nodes[i] = new LightWeightNode(i, oldLabel[i], lwg.IsWeighted, edgesList[i], (IsWeighted) ? edgeWeightList[i] : null);
        }

        /// <summary>
        /// Creates a minimum connectivity KNN Graph with an offset(if supplied
        /// </summary>
        /// <param name="distances">Distance matrix</param>
        /// <param name="add">Offset to K to create graphs that are less(negative) or more connected(positive)</param>
        /// <returns></returns>
        public static LightWeightGraph GetMinKnnGraph(DistanceMatrix distances, int add = 0)
        {
            int pointCount = distances.Count;
            int minK = BinSearchKNNMinConnectivity(1, pointCount - 1, pointCount, distances) + add;

            if (minK >= distances.Count)
                minK = distances.Count - 1;
            return GetKNNGraph(distances, minK);
        }

        public static int BinSearchKNNMinConnectivity(int min, int max, int pointCount, DistanceMatrix distance)
        {
            int mid = (min + max) / 2;

            if (mid > 0)
            {
                Boolean graphUpIsCon = KNNGraphIsConnected(distance, mid);//graph.isConnected();

                Boolean graphDownIsCon = KNNGraphIsConnected(distance, mid - 1);
                if (graphUpIsCon && !graphDownIsCon)
                {
                    return mid;
                }

                if (graphUpIsCon)
                {//Both are connected, try low end
                    return BinSearchKNNMinConnectivity(min, mid - 1, pointCount, distance);
                }
                if (!graphDownIsCon)
                {
                    return BinSearchKNNMinConnectivity(mid + 1, max, pointCount, distance);
                }
            }

            return 0;
        }

        public static LightWeightGraph GetMinGeoGraph(DistanceMatrix distances)
        {
            List<float> distList = distances.GetSortedDistanceList();
            int minDistanceIndex = BinSearchGeoMinConnectivity(0, distList.Count - 1, distances.Count, distances, distList);
            return GetGeometricGraph(distances, distList[minDistanceIndex]);
        }

        public static int BinSearchGeoMinConnectivity(int min, int max, int pointCount, DistanceMatrix distance, List<float> distList)
        {
            int mid = (min + max) / 2;

            if (mid > 0)
            {
                Boolean graphUpIsCon = GeoGraphIsConnected(distance, distList[mid]);//graph.isConnected();

                Boolean graphDownIsCon = GeoGraphIsConnected(distance, distList[mid - 1]);
                if (graphUpIsCon && !graphDownIsCon)
                {
                    return mid;
                }

                if (graphUpIsCon && graphDownIsCon)
                {//Both are connected, try low end
                    return BinSearchGeoMinConnectivity(min, mid - 1, pointCount, distance, distList);
                }
                if (!graphUpIsCon && !graphDownIsCon)
                {
                    return BinSearchGeoMinConnectivity(mid + 1, max, pointCount, distance, distList);
                }
            }

            return 0;
        }

        //Stacked MST
        public static LightWeightGraph GetStackedMST(DistanceMatrix distances, int numMSTs)
        {
            int numNodes = distances.Count;

            //in a complete graph, there are n*(n-1)/2 edges
            //an MST contains n-1 edges
            //number of msts is n/2
            if (numMSTs > numNodes / 2)
                numMSTs = numNodes / 2;

            LightWeightNode[] nodes = new LightWeightNode[numNodes];

            List<int>[] edges = new List<int>[numNodes];
            //List<float>[] edgeWeights = new List<float>[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                edges[i] = new List<int>();
                //edgeWeights[i] = new List<float>();
            }

            //Add all of the distances to the Heap

            List<distXY> points = new List<distXY>();
            for (int x = 0; x < numNodes - 1; x++)
            {
                for (int y = x + 1; y < numNodes; y++)
                    points.Add(new distXY(x, y, distances[x, y]));
            }


            //Now we need to start making our MST
            for (int n = 0; n < numMSTs; n++)
            {
                MinHeapPriorityQueue<distXY> minheap = new MinHeapPriorityQueue<distXY>(((x, y) => x.Dist > y.Dist));
                minheap.addAll(points);

                DisjointSet ds = new DisjointSet(numNodes);
                int k = 0;
                while (k < numNodes - 1)
                {
                    distXY e = minheap.extractMin();
                    if (ds.diff(e.X, e.Y)) //different
                    {
                        ds.union(e.X, e.Y);
                        //change the dist
                        e.Dist = double.MaxValue;
                        //Add it
                        edges[e.X].Add(e.Y);
                        edges[e.Y].Add(e.X);
                        k++;
                    }
                }
            }

            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightNode(i, false, edges[i]);

            return new LightWeightGraph(nodes, false);
        }


        //Knn based
        public static LightWeightGraph GetKNNGraph(DistanceMatrix distances, int numNeighbors)
        {

            int numNodes = distances.Count;
            var nodes = new LightWeightNode[numNodes];

            List<int>[] edgeLists = new List<int>[numNodes];
            List<float>[] edgeWeights = new List<float>[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                edgeLists[i] = new List<int>();
                edgeWeights[i] = new List<float>();
            }

            //prevent redundant edges
            HashSet<Tuple<int, int>> addedEdges = new HashSet<Tuple<int, int>>();
            //Our comparator
            MinHeapPriorityQueue<Tuple<int, double>>.isGreaterThan comp =
                new MinHeapPriorityQueue<Tuple<int, double>>.isGreaterThan((x, y) => { return x.Item2 > y.Item2; });

            //Add Edges
            for (int i = 0; i < numNodes - 1; i++)
            {
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
                    if (!addedEdges.Contains(new Tuple<int, int>(e.Item1, i)))
                    {
                        //make sure we don't add this edge again in the future
                        addedEdges.Add(new Tuple<int, int>(i, e.Item1));
                        //Add the double edge now
                        edgeLists[i].Add(e.Item1);
                        edgeLists[e.Item1].Add(i);
                        edgeWeights[i].Add((float)e.Item2);
                        edgeWeights[e.Item1].Add((float)e.Item2);
                    }
                }
            }

            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightNode(i, true, edgeLists[i], edgeWeights[i]);

            return new LightWeightGraph(nodes, true);
        }

        public List<List<int>> GetComponents(bool sort = true)
        {
            List<List<int>> componentList = new List<List<int>>();
            bool[] isVisited = new bool[NumNodes];

            Queue<int> q = new Queue<int>();
            for (int i = 0; i < NumNodes; i++)
            {
                if (!isVisited[i])
                {
                    List<int> component = new List<int>();
                    //BFS to count the size of the component
                    q.Enqueue(i);
                    isVisited[i] = true;
                    while (q.Count > 0)
                    {
                        int v = q.Dequeue();
                        component.Add(v);
                        foreach (int u in Nodes[v].Edge)
                            if (!isVisited[u])
                            {
                                q.Enqueue(u);
                                isVisited[u] = true;
                            }
                    }
                    if (sort)
                        component.Sort((a, b) => a.CompareTo(b));
                    componentList.Add(component);
                }
            }

            return componentList;
        }

        public bool isConnected()
        {
            return GetComponents().Count == 1;
        }

        public void SaveGML(String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("graph [");

                for (int i = 0; i < this.Nodes.Length; i++)
                {
                    sw.WriteLine("\tnode [");
                    sw.WriteLine("\t\tid " + i);
                    sw.WriteLine("\t]");
                }

                for (int i = 0; i < Nodes.Length; i++)
                {
                    for (int j = 0; j < Nodes[i].Count; j++)
                    {
                        sw.WriteLine("\tedge [");
                        sw.WriteLine("\t\tsource " + i);
                        sw.WriteLine("\t\ttarget " + Nodes[i].Edge[j]);
                        if (this.IsWeighted)
                            sw.WriteLine("\t\tweight " + Nodes[i].EdgeWeights[j]);
                        sw.WriteLine("\t]");
                    }
                }

                sw.WriteLine("]");
            }

        }



        public static LightWeightGraph GetGraphFromGML(String file)
        {
            Boolean isDirected = false;
            Boolean isWeighted = false;
            Dictionary<string, int> nodeIdLookup = new Dictionary<string, int>();
            HashSet<Tuple<int, int>> edgeHashSet = new HashSet<Tuple<int, int>>();

            List<List<int>> edges = new List<List<int>>();
            List<List<float>> weights = new List<List<float>>();
            int numNodes = 0;

            using (StreamReader sr = new StreamReader(file))
            {
                String readToEnd = sr.ReadToEnd();
                String[] split = readToEnd.Split(new char[] { '\n', '\r', ' ' });

                for (int i = 0; i < split.Length; i++)
                {
                    switch (split[i])
                    {
                        case "directed":
                            isDirected = int.Parse(split[i + 1]) == 1;
                            break;
                        case "weighted":
                            isWeighted = int.Parse(split[i + 1]) == 1;
                            break;
                        case "node":
                            int j = i;
                            while (split[j] != "]")
                            {
                                if (split[j] == "id")
                                {
                                    String nodeId = split[j + 1];
                                    nodeIdLookup[nodeId] = numNodes++;
                                }
                                j++;
                            }
                            break;
                    }
                }

                //Add a number of edge lists equal to the number of nodes
                for (int i = 0; i < numNodes; i++)
                {
                    edges.Add(new List<int>());
                    weights.Add(new List<float>());
                }

                for (int i = 0; i < split.Length; i++)
                {
                    switch (split[i])
                    {
                        case "edge":
                            int sourceId = 0, targetId = 0;
                            float edgeWeight = 1.0f;
                            int j = i;

                            while (split[j] != "]")
                            {
                                if (split[j] == "source")
                                {
                                    sourceId = nodeIdLookup[split[j + 1]];
                                }
                                else if (split[j] == "target")
                                {
                                    targetId = nodeIdLookup[split[j + 1]];
                                }
                                else if (split[j] == "weight")
                                {
                                    edgeWeight = float.Parse(split[j + 1]);
                                }
                                j++;
                            }

                            Tuple<int, int> toEdge = new Tuple<int, int>(sourceId, targetId);

                            if (isDirected)
                            {
                                edges[toEdge.Item1].Add(toEdge.Item2);
                                weights[toEdge.Item1].Add(edgeWeight);
                            }
                            else //undirected case, try adding the back edge too
                            {
                                Tuple<int, int> fromEdge = new Tuple<int, int>(targetId, sourceId);
                                if (!edgeHashSet.Contains(toEdge))
                                {
                                    edges[toEdge.Item1].Add(toEdge.Item2);
                                    weights[toEdge.Item1].Add(edgeWeight);
                                    edgeHashSet.Add(toEdge);
                                }
                                if (!edgeHashSet.Contains(fromEdge))
                                {
                                    edges[fromEdge.Item1].Add(fromEdge.Item2);
                                    weights[fromEdge.Item1].Add(edgeWeight);
                                    edgeHashSet.Add(fromEdge);
                                }
                            }
                            break;
                    }
                }
            }

            List<LightWeightNode> nodes = new List<LightWeightNode>();
            for (int i = 0; i < numNodes; i++)
            {
                nodes.Add(new LightWeightNode(i, true, edges[i], weights[i]));
            }

            return new LightWeightGraph(nodes.ToArray(), isWeighted);
        }



        struct NodeWeightPair
        {
            public int Node;
            public float Weight;
        }

        public static LightWeightGraph GetGraphFromFile(String file)
        {
            DelimitedFile parsedFile = new DelimitedFile(file, false, true);
            int parsedFileRowCount = parsedFile.Data.Count;

            //Ensure it has atleast 1 point and lists if it is weighted
            if (parsedFileRowCount < 2)
                throw new InvalidDataException("file");

            String weightString = parsedFile.Data[0][0];
            bool isWeighted = true;

            if (weightString == "unweighted")
                isWeighted = false;
            else if (weightString != "weighted")
                throw new InvalidDataException("Invalid Weight Type");


            //Start parsing the file
            List<List<NodeWeightPair>> nodes = new List<List<NodeWeightPair>>();
            for (int i = 1; i < parsedFileRowCount; i++)
            {
                var row = parsedFile.Data[i];

                List<NodeWeightPair> nList = new List<NodeWeightPair>();
                nodes.Add(nList);

                int edgeSize = (isWeighted) ? 2 : 1;
                for (int j = 1; j < row.Length; j += edgeSize)
                {
                    int from = int.Parse(row[j]);
                    float weight = (isWeighted) ? float.Parse(row[j + 1]) : 1.0f;
                    nList.Add(new NodeWeightPair { Node = from, Weight = weight });
                }
            }

            //Construct the graph
            var lwn = new LightWeightNode[nodes.Count];

            List<int>[] edges = new List<int>[nodes.Count];
            List<float>[] edgeWeights = new List<float>[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                edges[i] = new List<int>();
                edgeWeights[i] = new List<float>();
            }

            for (int i = 0; i < lwn.Length; i++)
            {
                int count = nodes[i].Count;
                for (int j = 0; j < count; j++)
                {
                    var a = nodes[i][j];
                    edges[i].Add(a.Node);
                    edgeWeights[i].Add(a.Weight);
                }
            }

            for (int i = 0; i < lwn.Length; i++)
                lwn[i] = new LightWeightNode(i, true, edges[i], edgeWeights[i]);

            return new LightWeightGraph(lwn, isWeighted);
        }

        public void SaveGraph(String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                //Write the the weight identifer
                sw.WriteLine(IsWeighted ? "weighted" : "unweighted");

                //Write the Edges
                for (int i = 0; i < NumNodes; i++)
                {
                    sw.Write(i.ToString());

                    for (int e = 0; e < Nodes[i].Edge.Count(); e++)
                    {
                        sw.Write(" " + Nodes[i].Edge[e]);
                        if (IsWeighted)
                            sw.Write(" " + Nodes[i].EdgeWeights[e]);
                    }

                    sw.WriteLine();
                }
            }
        }


        public class LightWeightNode
        {
            internal readonly int Id;
            internal int Label;
            internal int[] Edge;
            internal float[] EdgeWeights; //if null do nothing
            internal float NodeWeight;
            internal int Count;
            //holds the edge offset for this node, based upon the simple edge indexing scheme
            //Edges are indexed starting with node 0, from edge 0 to the last edge, then node 1, etc.

            public LightWeightNode(int i, Boolean initWeight, List<int> edges, List<float> weights = null)
            {
                Id = i;
                Edge = edges.ToArray();
                Count = Edge.Length;
                if (initWeight)
                    if (weights != null)
                        EdgeWeights = weights.ToArray();
                    else
                    {
                        EdgeWeights = new float[Count];
                        for (int j = 0; j < Count; j++)
                            EdgeWeights[j] = 1.0f;
                    }
            }

            public LightWeightNode(int i, int label, Boolean initWeight, List<int> edges, List<float> weights = null)
            {
                Id = i;
                Edge = edges.ToArray();
                Label = label;
                Count = Edge.Length;
                if (initWeight)
                    if (weights != null)
                        EdgeWeights = weights.ToArray();
                    else
                    {
                        EdgeWeights = new float[Count];
                        for (int j = 0; j < Count; j++)
                            EdgeWeights[j] = 1.0f;
                    }
            }

            //override for the sake of dictionary
            public override bool Equals(Object obj)
            {
                LightWeightNode n = obj as LightWeightNode;
                if (n == null)
                    return false;

                return n.Id == Id;
            }


            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
    }


}
