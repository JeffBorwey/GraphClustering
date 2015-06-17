using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.ClusteringAlgo;
using NetMining.Data;
using NetMining.Graphs;
using NetMining.Graphs.Generator;
using NetMining.ADT;
using NetMining.Files;
using NetMining.Evaluation;
using System.IO;
namespace debugNetData
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            // CHECK ACCURACY ONE AT A TIME
             DelimitedFile delimitedLabelFile =
                    new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\synth\\unEqDensity\\set1\\synthD2K8.1.data");
                int labelCol = delimitedLabelFile.Data[0].Length;
                LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

                //get the Partion file
                Partition clusterFile =
                    new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\synth\\unEqDensity\\set1\\synthD2K8_KNN_4_Beta108Partial_Weights.cluster");
                //Calculate the Error
                ExternalEval error = new ExternalEval(clusterFile, labels);

                //using (StreamWriter sw = new StreamWriter("wineABPartial\\wineABPartialResults.txt", true))
                //{
                //    sw.WriteLine("wine_NoWeights6_21_1.cluster");
                 //   sw.WriteLine(error.TextResults);
                 //   sw.WriteLine("");
               // }
                Console.WriteLine(error.TextResults);
            
            Console.ReadKey(); 
           // */

            /*
            for (int i = 0; i < 703; i++)
            {
                // DOING EXTERNAL EVALUATION
                //start by parsing label file
                DelimitedFile delimitedLabelFile =
                    new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\ecoli\\ecoli.data");
                int labelCol = delimitedLabelFile.Data[0].Length;
                LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

                //get the Partion file
                Partition clusterFile =
                    new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\ecoli\\ecoli_NoWeights8_21_" + i + ".cluster");

                //Calculate the Error
                ExternalEval error = new ExternalEval(clusterFile, labels);

                using (StreamWriter sw = new StreamWriter("ecoli\\ecoliResultsNoWeights8.txt", true))
                {
                    sw.WriteLine("ecoli_NoWeights8_21_" + i + ".cluster");
                    sw.WriteLine(error.TextResults);
                    sw.WriteLine("");
                }
                Console.WriteLine(error.TextResults);
            }
            //Console.ReadKey(); 
            
            //*/
            /*
            // Create a lightweight graph
            //LabeledPointSet data = new LabeledPointSet("iris.data", LabeledPointSet.LabelLocation.LastColumn);
           // LightWeightGraph lwg = LightWeightGraph.GetGraphFromFile("iris_Euclidean_KNN_30.graph");
            //LightWeightGraph lwg = LightWeightGraph.GetGraphFromFile("iris_Euclidean_4477.graph");
            //LabeledPointSet data = new LabeledPointSet("karate.data", LabeledPointSet.LabelLocation.LastColumn);
            //LightWeightGraph lwg = LightWeightGraph.GetGraphFromFile("karate.graph");
            LabeledPointSet data = new LabeledPointSet("football.data", 1);
            LightWeightGraph lwg = LightWeightGraph.GetGraphFromGML("football.gml");
            //LabeledPointSet data = new LabeledPointSet("wine.txt", LabeledPointSet.LabelLocation.LastColumn);
            //LightWeightGraph lwg = LightWeightGraph.GetGraphFromFile("wine_v2_GeometricMean_Euclidean_1226.graph");
            lwg.IsWeighted = true;
            // Put the ground truth cluster number into the Label field
            for (int i = 0; i < lwg.NumNodes; i++)
            {
                lwg.Nodes[i].Label = data.Labels.LabelIndices[i];
            }
            
            // This begins creation of a new graph, which will only contain nodes with edges between clusters
            List<int> listOfNodes = new List<int>();
            List<LightWeightGraph.LightWeightNode> newNodes = new List<LightWeightGraph.LightWeightNode>();
            bool[] S = new bool[lwg.NumNodes]; // S is a list of vertices that will be excluded
            for (int i = 0; i < S.Length; i++ ) 
            {
                S[i] = true;
            }
                for (int i = 0; i < lwg.NumNodes; i++)
                {
                    // Go through the list of edges.  If an edge ends in another cluster, keep that node

                    for (int j = 0; j < lwg.Nodes[i].Edge.Length; j++)
                    {
                        int currentCluster = lwg.Nodes[i].Label;
                        int terminatingNode = lwg.Nodes[i].Edge[j];
                        int terminatingCluster = lwg.Nodes[terminatingNode].Label;
                        if (currentCluster != terminatingCluster)
                        {
                            S[i] = false;
                            listOfNodes.Add(i);
                            newNodes.Add(lwg.Nodes[i]);
                            break;
                        }
                    }
                }
            // Create a subgraph based on exclusion rules array S
            // True nodes in S[] are excluded
            
            LightWeightGraph newGraph = new LightWeightGraph(lwg, S);
            
            VertexCover vertexCover = new VertexCover(newGraph, 1);
            List<int> vc = vertexCover.VC;
            //vc represents a vertex cover of the subgraph, we need to convert the nodes to 
            // the original graph
            List<int> vcContrived = new List<int>();
            for (int i = 0; i < vc.Count; i++ )
            {
                vcContrived.Add(listOfNodes[vc[i]]);
            }

            // We have a vertex cover based on the subgraph.  We need to use that vertex cover as |S| to calculate VAT
            // We need to create bool[] s to represent the attack set... listOfNodes identifies the order of nodes in newGraph
            bool[] s = new bool[lwg.NumNodes];
            for (int i = 0; i < vc.Count; i++ )
            {
                s[listOfNodes[vc[i]]] = true;
            }
            bool[] s1 = (bool[])s.Clone();
            bool[] s2 = (bool[])s.Clone();
            int sizeS = 0;
            for (int i = 0; i < lwg.NumNodes; i++ )
            {
                if (s[i] == true)
                {
                    sizeS++;
                }
            }
            // creates a VAT: the false is for _reassignnodes, not sure what it does
            VATContrived vatc = new VATContrived(lwg, s1, vcContrived, sizeS);
            vatc.HillClimb();
            VAT vat = new VAT(lwg, false, 1, 0);
            vat.HillClimb();
            //find the maximum sized component in the attacked graph
            var components = lwg.GetComponents(previsitedList: s2);

            if (components.Count == 1 || components.Count == 0)
                Console.Out.WriteLine("Invalid VAT");

            int cMax = components.Select(c => c.Count).Max();

            //calculate VAT
            double myVat = sizeS / (lwg.NumNodes - sizeS - cMax + 1.0f);


            Console.Out.WriteLine(myVat);

            Console.ReadKey(); 
             */

                // create a lwg
                //LabeledPointSet data = new LabeledPointSet("wine.data", LabeledPointSet.LabelLocation.LastColumn);
                //LightWeightGraph lwg = LightWeightGraph.GetGraphFromFile("wine_Euclidean_KNN_6.graph");
                    
                // VAT(graph, reassign nodes, alpha beta)
                //VAT vat = new VAT(lwg2, false, 1, 0);
                //vat.HillClimb();
          /*  
// THIS IS THE HIGHLY DESIRABLE WHILE LOOP, SET UP FOR ECOLI 8 CLUSTERS
            int KNN = 2;
            int dim = 2;
            for (dim = 1; dim <4; dim++)
            {
                //PointSet points = new PointSet("wine\\wine.txt");
                //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile("ecoliLOO\\ecoli_LOO_" + KNN + ".graph");
                //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                int numClusters = 2;
                int beta = 0;
                while (numClusters < 8)
                {                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                    HVATClust vClust = new HVATClust(lwg2, 2, false, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    Partition p = vClust.GetPartition();
                    //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                    p.SavePartition("ecoliLOO\\ecoli_NoWeights_LOO_" + KNN + "_" + beta + ".cluster", "ecoliLOO\\ecoli_LOO_" + KNN + ".graph");
                    beta++;
                    numClusters = p.Clusters.Count;
                }
            }
          //*/  

            /*
            // ONET TIME THROUGH, FOR SETS WITH ONLY 2 CLUSTERS
            int KNN = 6;
            PointSet points = new PointSet("wine\\wine.txt");
            //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(), KNN);
            //LightWeightGraph lwg2 = LightWeightGraph.GetGeometricGraph(points.GetDistanceMatrix(), .53713);
            //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile("breast_w\\breast_w_Euclidean_127817.graph");
            
            
            IPointGraphGenerator gen;
            
            
            var knnGen = new KNNGraphGenerator();
            knnGen.SetMinimumConnectivity();
            //knnGen.SetSkipLast(true);
            knnGen.SetMinOffset(0);
            //knnGen.SetK(3);
            gen = knnGen;
             
            // These 3 lines just for the Geometric graphs
            //var rGen = new GeoGraphGenerator();
            //rGen.SetMinimumConnectivity();
            //gen = rGen;
            
            HVATClust vClust = new HVATClust(points, 3, gen, true, 1, 0, false,true);
            //HVATClust vClust = new HVATClust(lwg2, 8, true, 1, 0, true, true);
            Partition p = vClust.GetPartition();
            //p.SavePartition("ecoli\\ecoliHVAT"+KNN+"_lwg_weights_810.cluster", "ecoli\\ecoli_Euclidean_KNN_"+KNN+".graph");
            p.SavePartition("wineABPartial\\wineHVAT" + KNN + "_points_weights_310.cluster", "wine\\wine.txt");
            //*/
               
            
            //lwg.IsWeighted = true;
                //PointSet points = new PointSet("iris.txt");
                //var distMatrix = points.GetDistanceMatrix();
                //var lwg = LightWeightGraph.GetMinKnnGraph(distMatrix, 1);
                //lwg.IsWeighted = true;

           //     Console.ReadKey(); 

                /*
                This is how to use random generator to create a randomly generated graph
                //Load the data
                LabeledPointSet data = new LabeledPointSet("iris.data", LabeledPointSet.LabelLocation.LastColumn);
                //Create a graph generator and configure it
                NetMining.Graphs.Generator.RandomGraphGenerator randomGraphGen = new NetMining.Graphs.Generator.RandomGraphGenerator();
                randomGraphGen.SetAlpha(3);
                randomGraphGen.SetExpP(1);
                //randomGraphGen.UseNormalizedProb(true);
                //Create a graph using the generator and save it
                var g = randomGraphGen.GenerateGraph(data.Points.GetDistanceMatrix());
                g.SaveGML("iris_random.gml"); 
                */
                
/*
            // THIS CODE CREATES LEAVE ONE OUT GRAPHS!!
            LabeledPointSet data = new LabeledPointSet("breast_w\\breast_w.data", LabeledPointSet.LabelLocation.LastColumn);

            int tries = 0;
            int numleaveout = 55;
            bool failure = false;
            
            while (failure == false)
           
            {
                bool success = false;
                tries = 0;
                while (failure == false && success == false)
                {
                    NetMining.Graphs.Generator.LeaveOutGraphGenerator looGraphGen = new NetMining.Graphs.Generator.LeaveOutGraphGenerator();
                    
                    looGraphGen.SetNumLeaveOut(numleaveout);
                    var g = looGraphGen.GenerateGraph(data.Points.GetDistanceMatrix());

                    if (g.isConnected() && (looGraphGen._numLeftOut >= numleaveout))
                    {
                        success = true;

                        g.SaveGML("breast_wLOO\\breast_w_LOO_" + numleaveout + ".gml");
                        g.SaveGraph("breast_wLOO\\breast_w_LOO_" + numleaveout + ".graph");
                        Console.WriteLine("Success K=" + numleaveout + ".  " + looGraphGen._numLeftOut + " left out");
                        numleaveout++;
                    }
                    else
                    {
                        tries++;
                        if (tries > 1000)
                        {
                            failure = true;
                            Console.WriteLine("Failure at " + numleaveout);
                        }
                    }
                }
            }
            Console.ReadKey();
  
 //*/

 /*
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
                */
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

                //Console.ReadKey();

// AUTOMATING A REPORT
/*         
            string prefix = "ecoli";
            DelimitedFile delimitedLabelFile =
                    new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\"+prefix+"\\"+prefix+".data");
            int labelCol = delimitedLabelFile.Data[0].Length;
            LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
            for (int n = 1; n < 41; n++)
            {
                int numLeftOut = n;
                using (StreamWriter sw = new StreamWriter(prefix + "LOO\\" + prefix + "_LOO_results.csv", true))
                {
                    sw.WriteLine("Left Out: " + numLeftOut);
                    sw.WriteLine("Beta, Weighted,, ,Unweighted");
                    int beta = 0;

                    // figure out largest beta for this numLeftOut series - weighted and unweighted
                    int maxWeighted = 0;
                    string[] filePaths = Directory.GetFiles(prefix + "LOO\\", prefix + "_Weights_LOO_" + numLeftOut + "_*.*");
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        string num = filePaths[i].Substring(filePaths[i].LastIndexOf("_") + 1);
                        num = num.Substring(0, num.IndexOf("."));
                        int numToInt = Int32.Parse(num);
                        if (numToInt > maxWeighted)
                        {
                            maxWeighted = numToInt;
                        }
                    }

                    int maxUnweighted = 0;
                    string[] filePathsUn = Directory.GetFiles(prefix + "LOO\\", prefix + "_NoWeights_LOO_" + numLeftOut + "_*.*");
                    for (int i = 0; i < filePathsUn.Length; i++)
                    {
                        string num = filePathsUn[i].Substring(filePathsUn[i].LastIndexOf("_") + 1);
                        num = num.Substring(0, num.IndexOf("."));
                        int numToInt = Int32.Parse(num);
                        if (numToInt > maxUnweighted)
                        {
                            maxUnweighted = numToInt;
                        }
                    }

                    int maxOfEverything = Math.Max(maxWeighted, maxUnweighted);
                    // maxOfEverything is the number of lines in theis section of the report i is BETA
                    for (int i = 0; i <= maxOfEverything; i++)
                    {
                        if (i <= maxWeighted)
                        {
                            sw.Write(i + ",");
                            Partition clusterFile =
                            new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\"+prefix+"LOO\\"+prefix+"_Weights_LOO_" + numLeftOut + "_" + i + ".cluster");

                            //Calculate the Error
                            ExternalEval error = new ExternalEval(clusterFile, labels);
                            sw.Write(error.ShorterTextResults + ",");

                            // write the VAT here
                            using (StreamReader sr = new StreamReader("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\"+prefix+"LOO\\"+prefix+"_Weights_LOO_" + numLeftOut + "_" + i + ".cluster"))
                            {
                                sr.ReadLine();
                                string line = sr.ReadLine();
                                line = line.Substring(line.IndexOf(" ") + 1);
                                int numberOfLines = Int32.Parse(line);
                                for (int j = 0; j < numberOfLines * 2 + 2; j++)
                                {
                                    sr.ReadLine();
                                }
                                double thisVat = Double.Parse(sr.ReadLine());
                                sw.Write(thisVat);
                                //Console.Write("Hello");
                            }
                            sw.Write(",");

                        }
                        else sw.Write(i + ",,,,");
                        if (i <= maxUnweighted)
                        {
                            Partition clusterFileUn =
                           new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\"+prefix+"LOO\\"+prefix+"_NoWeights_LOO_" + numLeftOut + "_" + i + ".cluster");

                            //Calculate the Error
                            ExternalEval errorUn = new ExternalEval(clusterFileUn, labels);
                            sw.Write(errorUn.ShorterTextResults);

                            // write the VAT here
                            sw.Write(",");
                            using (StreamReader sr2 = new StreamReader("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\"+prefix+"LOO\\"+prefix+"_NoWeights_LOO_" + numLeftOut + "_" + i + ".cluster"))
                            {
                                sr2.ReadLine();
                                string line = sr2.ReadLine();
                                line = line.Substring(line.IndexOf(" ") + 1);
                                int numberOfLines = Int32.Parse(line);
                                for (int j = 0; j < numberOfLines * 2 + 2; j++)
                                {
                                    sr2.ReadLine();
                                }
                                double thisVat = Double.Parse(sr2.ReadLine());
                                sw.Write(thisVat);
                                Console.Write("Hello");
                            }
                        }
                        else sw.Write(",,");
                        sw.WriteLine();

                    }



                }
            } Console.ReadKey();

//*/


            /*  
            // THIS IS THE HIGHLY DESIRABLE WHILE LOOP, Set up for NOISY DATA!!!!
            int[] minks = {4,4,101,3,7,3,3,5,2};
            string prefix = "synth\\unEqDensity\\set2after\\";
            int KNN = 3;
            int end = KNN + 1;
            int D = 4;
            int K = 4;
            for (KNN = 3; KNN < end; KNN++)
            {
                //PointSet points = new PointSet("wine\\wine.txt");
                //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                //synthD2K2_Euclidean_KNN_4.graph
                LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD"+D +"K"+K+"_Euclidean_KNN_"+ KNN + ".graph");
                //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                int numClusters = 1;
                int beta = 0;
                while (numClusters < K)
                {                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                    HVATClust vClust = new HVATClust(lwg2, 2, false, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    Partition p = vClust.GetPartition();
                    //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                    p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN +"_Beta"+beta+ "Partial_NoWeights.cluster", 
                                            prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                    beta++;
                    numClusters = p.Clusters.Count;
                }
            }
            /*
            KNN = 3;
            end = KNN + 1;
            D = 4;
            K = 4;
            for (KNN = 3; KNN < end; KNN++)
            {
                //PointSet points = new PointSet("wine\\wine.txt");
                //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                //synthD2K2_Euclidean_KNN_4.graph
                LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                int numClusters = 1;
                int beta = 0;
                while (numClusters < K)
                {                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                    HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    Partition p = vClust.GetPartition();
                    //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                    p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                            prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                    beta++;
                    numClusters = p.Clusters.Count;
                }
            }
            
            KNN = 3;
            end = KNN + 1;
            D = 4;
            K = 8;
            for (KNN = 3; KNN < end; KNN++)
            {
                //PointSet points = new PointSet("wine\\wine.txt");
                //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                //synthD2K2_Euclidean_KNN_4.graph
                LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                int numClusters = 1;
                int beta = 0;
                while (numClusters < K)
                {                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                    HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    Partition p = vClust.GetPartition();
                    //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                    p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                            prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                    beta++;
                    numClusters = p.Clusters.Count;
                }
            }

            //*/



// NOISY DATA AUTOMATED REPORT
            /*         
            int dataSet = 2;
            string path = "synth\\unEqDensity\\set" + dataSet + "\\";
            string prefix = "synthD8K8";
            int minKNN = 3;
            int maxKNN = minKNN + 0;
            DelimitedFile delimitedLabelFile =
                    new DelimitedFile(path + prefix + "." + dataSet + ".data");
            int labelCol = delimitedLabelFile.Data[0].Length;
            LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

            // figure out the largest beta for this series
            int maxWeighted = 0;
            string[] filePaths = Directory.GetFiles(path, prefix + "_KNN_*" + "_Weights.cluster");
            for (int i = 0; i < filePaths.Length; i++)
            {
                string num = filePaths[i].Substring(filePaths[i].IndexOf("a") + 1);
                num = num.Substring(0, num.IndexOf("P"));
                int numToInt = Int32.Parse(num);
                if (numToInt > maxWeighted)
                {
                    maxWeighted = numToInt;
                }
            }

            int maxUnweighted = 0;
            string[] filePathsUn = Directory.GetFiles(path, prefix + "_KNN_*" + "_NoWeights.cluster");
            for (int i = 0; i < filePathsUn.Length; i++)
            {
                string num = filePathsUn[i].Substring(filePathsUn[i].IndexOf("a") + 1);
                num = num.Substring(0, num.IndexOf("P"));
                int numToInt = Int32.Parse(num);
                if (numToInt > maxUnweighted)
                {
                    maxUnweighted = numToInt;
                }
            }

            int maxOfEverything = Math.Max(maxWeighted, maxUnweighted);

            using (StreamWriter sw = new StreamWriter(path + prefix + "_results.csv", true))
            {

                sw.WriteLine(path + prefix);
                sw.WriteLine("Beta, KNN="+minKNN+"Weighted,,vat,rem,Unweighted,,vat,rem,KNN="+(minKNN+1)+"Weighted,,vat,rem,Unweighted,,vat,rem,KNN="+(minKNN+2)+"Weighted,,vat,rem,Unweighted,,vat,rem,KNN="+(minKNN+3)+"Weighted,,vat,rem,Unweighted,,vat,rem,KNN="+(minKNN+4)+"Weighted,,vat,rem,Unweighted,,vat,rem");
                for (int beta = 0; beta <= maxOfEverything; beta++)
                {

                    sw.Write(beta + ",");

                    //beta = 0;

                    // maxOfEverything is the number of lines in theis section of the report i is BETA
                    for (int i = minKNN; i <= maxKNN; i++)
                    {

                        // figure out new maxWeighted and maxUnweighted for each i
                        maxWeighted = 0;
                        string[] filePathsA = Directory.GetFiles(path, prefix + "_KNN_" + i + "*" + "_Weights.cluster");
                        for (int j = 0; j < filePathsA.Length; j++)
                        {
                            string num = filePathsA[j].Substring(filePathsA[j].IndexOf("a") + 1);
                            num = num.Substring(0, num.IndexOf("P"));
                            int numToInt = Int32.Parse(num);
                            if (numToInt > maxWeighted)
                            {
                                maxWeighted = numToInt;
                            }
                        }

                        maxUnweighted = 0;
                        string[] filePathsUnA = Directory.GetFiles(path, prefix + "_KNN_" + i + "*" + "_NoWeights.cluster");
                        for (int j = 0; j < filePathsUnA.Length; j++)
                        {
                            string num = filePathsUnA[j].Substring(filePathsUnA[j].IndexOf("a") + 1);
                            num = num.Substring(0, num.IndexOf("P"));
                            int numToInt = Int32.Parse(num);
                            if (numToInt > maxUnweighted)
                            {
                                maxUnweighted = numToInt;
                            }
                        }

                        if (beta <= maxWeighted)
                        {
                            
                            Partition clusterFile =
                            new Partition(path + prefix + "_KNN_" + i + "_Beta" + beta + "Partial_Weights.cluster");

                            //Calculate the Error
                            ExternalEval error = new ExternalEval(clusterFile, labels);
                            sw.Write(error.NoNoiseTextResults + ",");

                            // write the VAT here
                            using (StreamReader sr = new StreamReader(path + prefix + "_KNN_" + i + "_Beta" + beta + "Partial_Weights.cluster"))
                            {
                                sr.ReadLine();
                                string line = sr.ReadLine();
                                line = line.Substring(line.IndexOf(" ") + 1);
                                int numberOfLines = Int32.Parse(line);
                                for (int j = 0; j < numberOfLines * 2 + 2; j++)
                                {
                                    sr.ReadLine();
                                }
                                double thisVat = Double.Parse(sr.ReadLine());
                                sw.Write(thisVat); sw.Write(",");
                                sr.ReadLine();
                                string removedLine = sr.ReadLine();
                                removedLine = removedLine.Substring(removedLine.IndexOf(":")+1);
                                int removed = Int32.Parse(removedLine);
                                sw.Write(removed);
                                //Console.Write("Hello");
                            }
                            sw.Write(",");

                        }
                        else sw.Write(",,,,");
                        if (beta <= maxUnweighted)
                        {
                            Partition clusterFileUn =
                           new Partition(path + prefix + "_KNN_" + i + "_Beta" + beta + "Partial_NoWeights.cluster");

                            //Calculate the Error
                            ExternalEval errorUn = new ExternalEval(clusterFileUn, labels);
                            sw.Write(errorUn.NoNoiseTextResults);

                            // write the VAT here
                            sw.Write(",");
                            using (StreamReader sr2 = new StreamReader(path + prefix + "_KNN_" + i + "_Beta" + beta + "Partial_NoWeights.cluster"))
                            {
                                sr2.ReadLine();
                                string line = sr2.ReadLine();
                                line = line.Substring(line.IndexOf(" ") + 1);
                                int numberOfLines = Int32.Parse(line);
                                for (int j = 0; j < numberOfLines * 2 + 2; j++)
                                {
                                    sr2.ReadLine();
                                }
                                double thisVat = Double.Parse(sr2.ReadLine());
                                sw.Write(thisVat); sw.Write(",");
                                sr2.ReadLine();
                                string removedLine = sr2.ReadLine();
                                removedLine = removedLine.Substring(removedLine.IndexOf(":")+1);
                                int removed = Int32.Parse(removedLine);
                                sw.Write(removed);
                                Console.Write("Hello");
                            }
                        }
                        else sw.Write(",,,");
                        sw.Write(",");


                    }

                    sw.WriteLine();

                }
            } Console.ReadKey();

            //*/
            //*  
            // USING A NEW BETWEENNESS-CENTRALITY NOT RECALCULATED WAY TO COMPUTE!!!!
            int[] minks = {4,4,101,3,7,3,3,5,2};
            string prefix = "synth\\unEqDensity\\set2after\\";
            int KNN = 3;
            int end = KNN + 1;
            int D = 8;
            int K = 8;
            for (KNN = 3; KNN < end; KNN++)
            {
                //PointSet points = new PointSet("wine\\wine.txt");
                //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                //synthD2K2_Euclidean_KNN_4.graph
                LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD"+D +"K"+K+"_Euclidean_KNN_"+ KNN + ".graph");
                //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                int numClusters = 1;
                int beta = 0;

                // Do it the old way the first time to compute inital 
                HVATClust vClust = new HVATClust(lwg2, 2, false, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                Partition p = vClust.GetPartition();
                //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_NoWeights.cluster",
                                        prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                List<int> nodeRemovalOrder = vClust._vatNodeRemovalOrder;
                int numNodesRemoved = vClust._vatNumNodesRemoved;
                beta++;
                numClusters = p.Clusters.Count;


                while (numClusters < K)
                {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                    //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    HVATClust hvClust = new HVATClust(lwg2, 2, false, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                    //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                    q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN +"_Beta"+beta+ "Partial_NoWeights.cluster", 
                                            prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                    beta++;
                    numClusters = q.Clusters.Count;
                }
            }
            // /*

        }  


    }
}
