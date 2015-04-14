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
    public class StochasticMSTGraphGenerator : IPointGraphGenerator
    {
        private double _normAlpha = 0.2;
        private double _xScale = 2;
        public void SetTypeNorm(double alpha, double xScale)
        {
            _normAlpha = alpha;
            _xScale = xScale;
        }


        /// <summary>
        /// Generates a Random graph using the defined paramaters
        /// </summary>
        /// <param name="d">The DistanceMatrix used for construction</param>
        /// <returns></returns>
        public LightWeightGraph GenerateGraph(DistanceMatrix d)
        {
            return GetNormalizedRandomGraph(d);
        }

        public LightWeightGraph GetNormalizedRandomGraph(DistanceMatrix d)
        {
            int numNodes = d.Count;
            var nodes = new LightWeightGraph.LightWeightNode[numNodes];
            // make an array to hold all possible edges, less the edges in the mst
            List<oneNode> myDistances = new List<oneNode>();

            LightWeightGraph mst = LightWeightGraph.GetStackedMST(d, 1);
            LightWeightGraph.LightWeightNode[] mstNodes = mst.Nodes;

            //Create a list to hold edge values
            List<int>[] edges = new List<int>[numNodes];
            List<float>[] weights = new List<float>[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                edges[i] = new List<int>();
                weights[i] = new List<float>();
            }

            double largestMSTEdge = 0.0;
            // add edges from the mst to the edges list, to facilitate adding additional edges later on
            // Also find the largest edge to use as a cutoff
            for (int i = 0; i < numNodes; i++)
            {
                for (int j = 0; j < mstNodes[i].Edge.Length; j++)
                {
                    largestMSTEdge = Math.Max(largestMSTEdge, mst[i].EdgeWeights[j]);
                    edges[i].Add(mstNodes[i].Edge[j]);
                    weights[i].Add(mstNodes[i].EdgeWeights[j]);
                }
            }
            // cycle through each possible edge
            // if the edge exists in the mst, continue
            // otherwise, add the edge to distances array, and add distance to the edge to the cummulative total
            for (int i = 0; i < numNodes - 1; i++)
            {
                for (int j = i + 1; j < numNodes; j++)
                {
                    double dist = d[i, j];
                    if (dist >= largestMSTEdge || mstNodes[i].Edge.Contains(j))
                    {
                        continue;
                    }
                    else
                    {
                        //Probability function goes here

                        double addlProb = _normAlpha * (1.0 / Math.Exp(_xScale*dist / largestMSTEdge));
                        oneNode nd = new oneNode { prob = addlProb, fromNode = i, toNode = j, alreadyExists = false };
                        myDistances.Add(nd);
                    }
                }
            }

            // walk through the array until you find the random number
            for (int m = 0; m < myDistances.Count; m++)
            {
                double rand = Utility.Util.Rng.NextDouble();
                if (myDistances[m].prob > rand)
                {
                    // we have found the edge to add.
                    // add the edge if it does not already exist
                    if (!myDistances[m].alreadyExists)
                    {
                        int from = myDistances[m].fromNode;
                        int to = myDistances[m].toNode;
                        float dist = d[from, to];
                        edges[from].Add(to);
                        weights[from].Add(dist);
                        edges[to].Add(from);
                        weights[to].Add(dist);
                        myDistances[m].alreadyExists = true;
                    }
                }
            }

            for (int i = 0; i < numNodes; i++)
                nodes[i] = new LightWeightGraph.LightWeightNode(i, true, edges[i], weights[i]);

            return new LightWeightGraph(nodes, true);
        }

        class oneNode
        {
            public double prob;
            public int fromNode;
            public int toNode;
            public bool alreadyExists;
        }
    }
}
