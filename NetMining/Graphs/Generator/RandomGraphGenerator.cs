using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.Graphs;
using NetMining.Data;

namespace NetMining.Graphs.Generator
{
    /// <summary>
    /// This class implements a Random Graph Generator
    /// It constructs a MST, then adds edges at random, with shorter edges more likely,
    /// in the style of Kleinberg's small-world graph
    /// </summary>
    public class RandomGraphGenerator : IPointGraphGenerator
    {
        private int _alpha = 3;
        private int _expP = 1;

        /// <summary>
        /// Sets alpha for constructing a Random Graph
        /// </summary>
        /// <param name="alpha">Desired degree of resulting graph</param>
        public void SetAlpha(int alpha)
        {
            _alpha = alpha;
            if (alpha <= 0)
                _alpha = 3;
        }
        /// <summary>
        /// Sets expP for constructing a Random Graph  (probability of edge with distance d = 1/d^expP
        /// </summary>
        /// <param name="expP">Exponent for creating power law distribution</param>
        public void SetExpP(int expP)
        {
            _expP = expP;
        }

        /// <summary>
        /// Generates a Random graph using the defined paramaters
        /// </summary>
        /// <param name="d">The DistanceMatrix used for construction</param>
        /// <returns></returns>
        public LightWeightGraph GenerateGraph(DistanceMatrix d)
        {
            return GetRandomGraph(d, _alpha, _expP);
        }

        /// <summary>
        /// Creates a random graph, based on an mst.  
        /// </summary>
        /// <param name="d">distance matrix used to construct the graph</param>
        /// <param name="alpha">the resulting average degree of the graph</param>
        /// /// <param name="expP">The probability of adding an edge depends on its distance: 1/d^expP</param>
        /// <returns></returns>
        public static LightWeightGraph GetRandomGraph(DistanceMatrix d, int alpha, double expP)
        {
            int numNodes = d.Count;
            var nodes = new LightWeightGraph.LightWeightNode[numNodes];
            // make an array to hold all possible edges, less the edges in the mst
            oneNode[] myDistances = new oneNode[numNodes * (numNodes - 1) / 2 - (numNodes - 1)];

            LightWeightGraph mst = LightWeightGraph.GetStackedMST(d, 1);
            LightWeightGraph.LightWeightNode[] mstNodes = mst.Nodes;
            int myDistancesIndex = 0;
            double myDistancesTotal = 0;
            //Create a list to hold edge values
            List<int>[] edges = new List<int>[numNodes];
            List<float>[] weights = new List<float>[numNodes];
            for (int i = 0; i < numNodes; i++)
                edges[i] = new List<int>();

            // add edges from the mst to the edges list, to facilitate adding additional edges later on
            for (int i = 0; i < numNodes; i++)
            {
                for (int j = 0; j < mstNodes[i].Edge.Length; j++)
                {
                    edges[i].Add(mstNodes[i].Edge[j]);
                }
            }


            // cycle through each possible edge
            // if the edge exists in the mst, continue
            // otherwise, add the edge to distances array, and add distance to the edge to the cummulative total
            for (int i = 0; i < numNodes - 1; i++)
            {
                for (int j = i + 1; j < numNodes; j++)
                    if (mstNodes[i].Edge.Contains(j))
                    {
                        continue;
                    }
                    else
                    {
                        double addlProb = 1.0 / Math.Pow(d[i, j], expP);
                        myDistancesTotal += addlProb;
                        oneNode nd = new oneNode { prob = myDistancesTotal, fromNode = i, toNode = j, alreadyExists = false };
                        myDistances[myDistancesIndex] = nd;
                        myDistancesIndex++;
                    }
            }

            // how many edges do we want to add?
            int desiredNewEdges = (alpha * numNodes) - (numNodes - 1);

            // add edges randomly until we have added the desired number of edges
            while (desiredNewEdges > 0)
            {
                // generate a random number between 0 and myDistancesTotal
                Random rnd = new Random();
                double rand = rnd.NextDouble() * myDistancesTotal;

                // walk through the array until you find the random number
                for (int m = 0; m < myDistances.Length; m++)
                {
                    if (myDistances[m].prob > rand)
                    {
                        // we have found the edge to add.
                        // add the edge if it does not already exist
                        if (!myDistances[m].alreadyExists)
                        {
                            edges[myDistances[m].fromNode].Add(myDistances[m].toNode);
                            edges[myDistances[m].toNode].Add(myDistances[m].fromNode);
                            myDistances[m].alreadyExists = true;
                            desiredNewEdges--;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightGraph.LightWeightNode(i, true, edges[i], weights[i]);

            return new LightWeightGraph(nodes, true);
        }
    }
    struct oneNode
    {
        public double prob;
        public int fromNode;
        public int toNode;
        public bool alreadyExists;
    }
}
