using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.ClusteringAlgo;
using NetMining.Data;
using NetMining.Graphs;

namespace debugNetData
{
    class Program
    {
        static void Main(string[] args)
        {
            PointSet swissPoints = new PointSet("iris.txt");
            LightWeightGraph minIris = LightWeightGraph.GetMinKnnGraph(swissPoints.GetDistanceMatrix());
            var map = minIris.GetEdgeIndexMap();
            float[] BCEdge = NetMining.Graphs.BetweenessCentrality.BrandesBcEdges(minIris);
            
            for (int n = 0; n < minIris.NumNodes; n++)
            {
                foreach (int e in minIris.Nodes[n].Edge)
                {
                    KeyValuePair<int, int> edge = new KeyValuePair<int, int>(n, e);
                    if (map.ContainsKey(edge))
                        Console.WriteLine("{0} {1} = {2}", edge.Key, edge.Value, BCEdge[map[edge]]);

                }
            }
            //minSwiss.SaveGML("iris.gml");
            //minSwiss.SaveGraph("iris.graph");
            /*
            minSwiss.SaveGML("SwissRoll.gml");
            HVATClust vClust = new HVATClust(swissPoints, 4, false, true, 1);
            Partition p = vClust.GetPartition();
            p.SavePartition("swissRoll", "SwissRoll.txt", p.MetaData);
            //LightWeightGraph lwg = LightWeightGraph.GetGraphFromFile("g.graph");


            PointSet points = new PointSet("iris.txt");
            var distMatrix = points.GetDistanceMatrix();
            
            var lwg = LightWeightGraph.GetMinKnnGraph(distMatrix, 1);
            lwg.IsWeighted = true;

            VAT v = new VAT(lwg);
            var nlwg = v.GetAttackedGraphWithReassignment();
            List<List<int>> components = nlwg.GetComponents();

            var dist2_0 = distMatrix.GetReducedDataSet(components[0]);
            var lwg2_0 = LightWeightGraph.GetMinKnnGraph(dist2_0.Mat, 1);
            bool lwg2_0C = lwg2_0.isConnected();
            lwg2_0.IsWeighted = true;
            var dist2_1 = distMatrix.GetReducedDataSet(components[1]);
            var lwg2_1 = LightWeightGraph.GetMinKnnGraph(dist2_1.Mat, 1);
            bool lwg2_1C = lwg2_1.isConnected();
            lwg2_1.IsWeighted = true;

            VAT v2_0 = new VAT(lwg2_0);
            List<List<int>> components2_0 = v2_0.GetAttackedGraphWithReassignment().GetComponents();
            VAT v2_1 = new VAT(lwg2_1);
            List<List<int>> components2_1 = v2_1.GetAttackedGraphWithReassignment().GetComponents();

            */

            Console.ReadKey();
        }
    }
}
