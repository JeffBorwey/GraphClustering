using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using NetMining.ADT;
using NetMining.Data;
using NetMining.Files;

namespace NetMining.Graphs
{
    using GraphStringRep = List<List<EdgeValuePair<String>>>;

    public class LightWeightGraph : AbstractDataset
    {
        public LightWeightNode[] Nodes;
        public int NumNodes;
        public Boolean IsWeighted;

        #region "Constructors"
        public LightWeightGraph() 
            : base(DataType.Graph)
        {
            Nodes = new LightWeightNode[1];
            NumNodes = 1;
            Nodes[0] = new LightWeightNode(0, false, new List<int>());
            IsWeighted = false;
        }

        /// <summary>
        /// Make a deep Copy of a Graph
        /// </summary>
        /// <param name="g">LightWeightGraph to copy</param>
        public LightWeightGraph(LightWeightGraph g)
            : base(DataType.Graph)
        {
            NumNodes = g.NumNodes;
            Nodes = new LightWeightNode[NumNodes];
            for (int i = 0; i < NumNodes; i++)
                Nodes[i] = new LightWeightNode(g[i]);
            IsWeighted = g.IsWeighted;
        }

        public LightWeightGraph(LightWeightNode[] nodes, Boolean isWeighted)
            : base(DataType.Graph)
        {
            Nodes = nodes;
            NumNodes = nodes.Length;
            IsWeighted = isWeighted;
        }

        //construct a subgraph using some exclusion rules
        public LightWeightGraph(LightWeightGraph lwg, bool[] S)
            : base(DataType.Graph)
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
        #endregion

        #region "Graph Generators"
        public static bool GeoGraphIsConnected(DistanceMatrix distances, float threshold)
        {
            return GetGeometricGraph(distances, threshold).isConnected();
        }

        public static bool KNNGraphIsConnected(DistanceMatrix distances, int neighbors)
        {
            return GetKNNGraph(distances, neighbors).isConnected();
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
            for (int i = 0; i < numNodes; i++)
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
                        edgeWeights[i].Add((float)e.Item2);
                        edgeWeights[e.Item1].Add((float)e.Item2);
                    }
                }
            }

            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightNode(i, true, edgeLists[i], edgeWeights[i]);

            return new LightWeightGraph(nodes, true);
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
            //int minK = BinSearchKNNMinConnectivity(1, pointCount - 1, pointCount, distances) + add;
            int minK = BinSearchKNNMin2(1, distances.Count - 1, distances) + add;
            if (minK >= distances.Count)
                minK = distances.Count - 1;
            return GetKNNGraph(distances, minK);
        }

        //Depreciated
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

        /// <summary>
        /// This function performs a 1 sided quadratic search to find the min connected KNN graph
        /// It also makes
        /// </summary>
        /// <param name="min">this should be 1</param>
        /// <param name="max">this should be n-1</param>
        /// <param name="distance">Pairwise distance matrix</param>
        /// <returns>Returns an the minimum k which produces a connected knn graph</returns>
        public static int BinSearchKNNMin2(int min, int max, DistanceMatrix distance)
        {
            int highestDisconnected = min;
            int lowestConnected = max;

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

                bool isConn = KNNGraphIsConnected(distance, i);
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
        #endregion

        #region "Save and Load from file"
        #region "GML"

        /// <summary>
        /// Saves a Light weight graph as a GML
        /// </summary>
        /// <param name="filename">File to save to</param>
        /// <param name="nodeDescriptors">A dictionary of dictionaries.  The Key is the attribute tag (eg: "label") and the value is a mapping from node index to attribute value</param>
        /// <param name="nodeGraphicsDescriptors">A dictionary of dictionaries for graphics attributes.  The Key is the attribute tag (eg: "fill", or "x") and the value is a mapping from node index to attribute value</param>
        public void SaveGML(String filename, Dictionary<String, Dictionary<int, String>> nodeDescriptors = null, Dictionary<String, Dictionary<int, String>> nodeGraphicsDescriptors = null)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("graph [");

                for (int i = 0; i < this.Nodes.Length; i++)
                {
                    sw.WriteLine("\tnode [");
                    sw.WriteLine("\t\tid " + i);

                    if (nodeDescriptors != null)
                    {
                        foreach (var kv in nodeDescriptors)
                        {
                            if (kv.Value.ContainsKey(i))
                            {
                                String tag = kv.Key;
                                String val = kv.Value[i];
                                sw.WriteLine("\t\t{0} {1}", tag, val);
                            }
                        }
                    }

                    if (nodeGraphicsDescriptors != null)
                    {
                        var attributes = (from kv in nodeGraphicsDescriptors 
                                          where kv.Value.ContainsKey(i) 
                                          select new KeyValuePair<string, string>(kv.Key, kv.Value[i])).ToList();

                        if (attributes.Count > 0)
                        {
                            sw.WriteLine("\t\tgraphics [");

                            foreach (var kv in attributes)
                            {
                                String tag = kv.Key;
                                String val = kv.Value;
                                sw.WriteLine("\t\t\t{0} {1}", tag, val);
                            }

                            sw.WriteLine("\t\t]");
                        }
                    }
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
                            sw.WriteLine("\t\tvalue " + Nodes[i].EdgeWeights[j]);
                        sw.WriteLine("\t]");
                    }
                }

                sw.WriteLine("]");
            }

        }

        /*
        //Depreciated
        public void SaveGML(String filename, NetVertDesciption[] descriptors = null,  Dictionary<int, Color> colors = null)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("graph [");

                for (int i = 0; i < this.Nodes.Length; i++)
                {
                    sw.WriteLine("\tnode [");
                    sw.WriteLine("\t\tid " + i);
                    if (descriptors != null)
                        sw.WriteLine("\t\tlabel \"{0}\"", descriptors[this[i].Label].Desc);
                    if (descriptors != null || colors != null)
                    {
                        sw.WriteLine("\t\tgraphics [");
                        if (descriptors != null)
                        {
                            sw.WriteLine("\t\t\tx {0}", descriptors[this[i].Label].X);
                            sw.WriteLine("\t\t\ty {0}", descriptors[this[i].Label].Y);
                        }
                        if (colors != null)
                        {
                            String colorString = "808080"; //Default color is gray
                            if (colors.ContainsKey(i))
                            {
                                Color c = colors[i];
                                colorString = c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
                            }
                            sw.WriteLine("\t\t\tfill #{0}", colorString);
                        }
                        sw.WriteLine("\t\t]");
                    }
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
                            sw.WriteLine("\t\tvalue " + Nodes[i].EdgeWeights[j]);
                        sw.WriteLine("\t]");
                    }
                }

                sw.WriteLine("]");
            }

        }
        */


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
                                else if (split[j] == "weight" || split[j] == "value")
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
        #endregion
        #region ".net"

        public static LWGWithNodeDescriptors GetGraphFromNetFile(String file)
        {
            using (StreamReader sr = new StreamReader(file))
            {
                String vertCountPattern = "[*]Vertices\\s+(?<vert>\\d+)";
                String firstLine = sr.ReadLine();
                var match = Regex.Match(firstLine, vertCountPattern, RegexOptions.IgnoreCase);
                int numVert = int.Parse(match.Groups["vert"].Value);

                String vertPattern =
                    "\\s+(?<vertid>\\d+)\\s+\"(?<vertName>[^\"]+)\"\\s+(?<x>\\d[.]\\d+)\\s+(?<y>\\d+[.]\\d+)";
                
                NetVertDesciption[] verts = new NetVertDesciption[numVert];
                for (int i = 0; i < numVert; i++)
                {
                    String line = sr.ReadLine();
                    match = Regex.Match(line, vertPattern, RegexOptions.IgnoreCase);
                    String vertID = match.Groups["vertid"].Value;
                    String vertName = match.Groups["vertName"].Value;
                    String x = match.Groups["x"].Value;
                    String y = match.Groups["y"].Value;
                    verts[i] = (new NetVertDesciption(int.Parse(vertID)-1, vertName, float.Parse(x), float.Parse(y)));
                }

                while (!sr.ReadLine().Contains("*Edges"))
                {
                }

                List<int>[] edges = new List<int>[numVert];
                List<float>[] edgeWeights = new List<float>[numVert];
                for (int i = 0; i < numVert; i ++)
                {
                    edges[i] = new List<int>();
                    edgeWeights[i] = new List<float>();
                }
                String edgePattern =
                    "\\s+(?<fIndex>\\d+)\\s+(?<tIndex>\\d+)\\s+(?<dist>\\d[.]\\d+)";
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    match = Regex.Match(line, edgePattern, RegexOptions.IgnoreCase);
                    int from = int.Parse(match.Groups["fIndex"].Value) - 1;
                    int to = int.Parse(match.Groups["tIndex"].Value) - 1;
                    float dist = float.Parse(match.Groups["dist"].Value);
                    edges[from].Add(to); edgeWeights[from].Add(dist);
                    edges[to].Add(from); edgeWeights[to].Add(dist);
                }

                LightWeightNode[] nodes = new LightWeightNode[numVert];
                for (int i = 0; i < numVert; i++)
                    nodes[i] = (new LightWeightNode(i, true, edges[i], edgeWeights[i]));
                return new LWGWithNodeDescriptors(verts, new LightWeightGraph(nodes, true));
            }
        }

        public class LWGWithNodeDescriptors
        {
            public NetVertDesciption[] Descriptors;
            public LightWeightGraph Lwg;

            public LWGWithNodeDescriptors(NetVertDesciption[] descriptors, LightWeightGraph lwg)
            {
                Descriptors = descriptors;
                Lwg = lwg;
            }
        }

        public class NetVertDesciption
        {
            public int Id;
            public String Desc;
            public float X, Y;

            public NetVertDesciption(int id, string desc, float x, float y)
            {
                Id = id;
                Desc = desc;
                X = x;
                Y = y;
            }
        }

        #endregion
        #region ".graph"
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

        #endregion
        #region "Graph String Rep"
        public GraphStringRep GetGraphStringRep()
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
        #endregion
        #endregion

        #region "Utility"
        public List<List<int>> GetComponents(bool sort = true, bool[] previsitedList = null)
        {
            List<List<int>> componentList = new List<List<int>>();
            bool[] isVisited = previsitedList ?? new bool[NumNodes];

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

        public override int Count
        {
            get { return NumNodes; }
        }

        #endregion

        #region "Operator Overloads"

        public LightWeightNode this[int i]
        {
            get { return Nodes[i]; }
        }

        #endregion


        class distXY
        {
            internal readonly int X, Y;
            internal double Dist;
            public distXY(int x, int y, double dist) { X = x; Y = y; Dist = dist; }
        }

        struct NodeWeightPair
        {
            public int Node;
            public float Weight;
        }

        
        public class LightWeightNode
        {
            internal readonly int Id;
            public int Label;
            public int[] Edge;
            public float[] EdgeWeights; //if null do nothing
            internal int Count;
            //holds the edge offset for this node, based upon the simple edge indexing scheme
            //Edges are indexed starting with node 0, from edge 0 to the last edge, then node 1, etc.

            public LightWeightNode(int i, Boolean initWeight, List<int> edges, List<float> weights = null)
            {
                Id = i;
                Label = i;
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

            /// <summary>
            /// Copy Constructor
            /// </summary>
            /// <param name="n">LightWeightNode to copy</param>
            public LightWeightNode(LightWeightNode n)
            {
                Id = n.Id;
                Label = n.Label;
                Edge = (int[]) n.Edge.Clone();
                EdgeWeights = (float[]) n.EdgeWeights.Clone();
                //NodeWeight = n.NodeWeight;
                Count = n.Count;
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
