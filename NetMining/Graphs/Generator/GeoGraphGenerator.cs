using System.Collections.Generic;
using NetMining.Graphs;
using NetMining.Data;

namespace NetMining.Graphs.Generator
{
    /// <summary>
    /// This class implements a Geometric Graph Generator
    /// By default this will construct a minimum connectivity graph
    /// </summary>
    public class GeoGraphGenerator : IPointGraphGenerator
    {
        private enum GeoGraphType
        {
            MinimumConnectivity,
            SpecifiedThreshold
        }

        private GeoGraphType _graphType = GeoGraphType.MinimumConnectivity;
        private double _threshold = 1;

        /// <summary>
        /// Sets the Threshold for constructing a Geometric Graph
        /// </summary>
        /// <param name="threshold">Maximum acceptable distance (inclusive)</param>
        public void SetThreshold(double threshold)
        {
            _graphType = GeoGraphType.SpecifiedThreshold;
            _threshold = threshold;
            if (threshold <= 0)
                _threshold = 1;
        }

        public void SetMinimumConnectivity()
        {
            _graphType = GeoGraphType.MinimumConnectivity;
        }

        /// <summary>
        /// Generates a Geometric graph using the defined paramaters
        /// </summary>
        /// <param name="d">The DistanceMatrix used for construction</param>
        /// <returns></returns>
        public LightWeightGraph GenerateGraph(DistanceMatrix d)
        {
            if (_graphType == GeoGraphType.SpecifiedThreshold)
                return GetGeoGraph(d, _threshold);
            double minThreshold = QuadraticSearchMinGeo(d);
            return GetGeoGraph(d, minThreshold);
        }

        /// <summary>
        /// Gets the minimum threshold required for a connected geometric graph
        /// </summary>
        /// <param name="distance">Distance matrix used to create the graph</param>
        /// <returns>The minimum threshold that still creates a connected graph</returns>
        public static double QuadraticSearchMinGeo(DistanceMatrix distance)
        {
            //Construct our distance list
            List<double> distList = distance.GetSortedDistanceListD();

            int min = distance.Count - 2; //must be atleast n-1 (n-2 index)
            int max = distList.Count - 1; //Loose upper bound

            //Set our paramaters
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

                bool isConn = GetGeoGraph(distance, distList[i]).isConnected();
                connectedDict.Add(i, isConn);

                if (isConn && i < lowestConnected)
                {
                    lowestConnected = i;
                    if (connectedDict.ContainsKey(lowestConnected - 1) && !connectedDict[lowestConnected - 1])
                        return distList[lowestConnected];
                }
                else if (!isConn && i > highestDisconnected)
                {
                    highestDisconnected = i;
                    if (connectedDict.ContainsKey(highestDisconnected + 1) && connectedDict[highestDisconnected + 1])
                        return distList[highestDisconnected + 1];
                }
                inc++;
            }

            return distList[lowestConnected];
        }

        /// <summary>
        /// Creates a thresholded geometric graph
        /// </summary>
        /// <param name="d">distance matrix used to construct the graph</param>
        /// <param name="threshold">the maximum allowed distance</param>
        /// <returns></returns>
        public static LightWeightGraph GetGeoGraph(DistanceMatrix d, double threshold)
        {
            //construct the geo graph
            int numNodes = d.Count;
            var nodes = new LightWeightGraph.LightWeightNode[numNodes];

            //Create a list to hold edge values
            List<int>[] edges = new List<int>[numNodes];
            List<float>[] weights = new List<float>[numNodes];
            for (int i = 0; i < numNodes; i++)
                edges[i] = new List<int>();

            //Add Edges
            for (int i = 0; i < numNodes - 1; i++)
            {
                for (int j = i + 1; j < numNodes; j++)
                    if (d[i, j] <= threshold)
                    {
                        edges[i].Add(j);
                        edges[j].Add(i);
                    }
            }
            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightGraph.LightWeightNode(i, true, edges[i], weights[i]);

            return new LightWeightGraph(nodes, true);
        }
    }
}
}
