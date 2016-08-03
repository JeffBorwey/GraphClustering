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
using NetMining.ExtensionMethods;
using System.IO;
using System.Diagnostics;
namespace debugNetData
{
    class Program
    {
        static void Main(string[] args)
        {
           /*
             //int set = 2;
             String set = "";
            //String path = "C:\\Users\\John\\Dropbox\\ClustProject\\John\\ResilienceMeasureClustering\\synthDataNoise\\UnEqDensity\\set" + set + "\\";
            //String path = "C:\\Users\\John\\Dropbox\\ClustProject\\SyntheticLFRNets\\binary_networks\\John\\";
            String path = "C:\\Users\\John\\Dropbox\\ClustProject\\John\\PercentageConnected\\iris\\";
            String file = "iris_Euclidean_KNN_0.7_25";
            DelimitedFile delimitedLabelFile =
             //new DelimitedFile(path + file + ".data");
             new DelimitedFile(path + "iris.data");
             int labelCol = delimitedLabelFile.Data[0].Length;
             LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
             Partition clusterFile = new Partition(path + file + "vat.cluster");
             Partition clusterFile2 = new Partition(path + file + "int.cluster");
             Partition clusterFile3 = new Partition(path + file + "tou.cluster");
             Partition clusterFile4 = new Partition(path + file + "ten.cluster");
             Partition clusterFile5 = new Partition(path + file + "sca.cluster");
             ExternalEval error = new ExternalEval(clusterFile, labels);
             ExternalEval error2 = new ExternalEval(clusterFile2, labels);
             ExternalEval error3 = new ExternalEval(clusterFile3, labels);
             ExternalEval error4 = new ExternalEval(clusterFile4, labels);
             ExternalEval error5 = new ExternalEval(clusterFile5, labels);
             Console.WriteLine(error.TextResults); Console.WriteLine("");
             Console.WriteLine(error2.TextResults); Console.WriteLine("");
             Console.WriteLine(error3.TextResults); Console.WriteLine("");
             Console.WriteLine(error4.TextResults); Console.WriteLine("");
             Console.WriteLine(error5.TextResults); Console.WriteLine("");
             //Console.ReadKey();
             //*
             using (StreamWriter sw = new StreamWriter(path + "results.txt", true))
             {
                 sw.Write(file + "VAT");
                 sw.WriteLine(error.TextResults + ","); sw.Write(file + "INT");
                 sw.WriteLine(error2.TextResults + ","); sw.Write(file + "TOU");
                 sw.WriteLine(error3.TextResults + ","); sw.Write(file + "TEN");
                 sw.WriteLine(error4.TextResults + ","); sw.Write(file + "SCA");
                 sw.WriteLine(error5.TextResults + ",");

                 sw.WriteLine("");
             }

             Console.ReadKey();
            // */
            // */
            /*
            // THIS IS ACCURACY REPORT 7-4-16

            int set = 1;
            for (set = 1; set < 11; set++)
            {
                for (int D = 2; D <= 8; D = D * 2)
                {
                    for (int K = 2; K <= 8; K = K * 2)
                    {
                        //int D = 2; int K = 2;
                        String path = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\eq10N-unweighted-reassign-2dhill\\";
                        String dataPath = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\eq10N\\";
                        String file = "synthD" + D + "K" + K + "." + set;

                        DelimitedFile delimitedLabelFile =
                         //new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\polbooks\\polbooks.data");
                         new DelimitedFile(dataPath + file + ".data");
                        int labelCol = delimitedLabelFile.Data[0].Length;
                        LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));
                        if (!File.Exists(path + file + "VAT.cluster"))
                        {
                            continue;
                        }
                        //get the Partion files
                        Partition clusterFile = new Partition(path + file + "VAT.cluster");
                        Partition clusterFile2 = new Partition(path + file + "Int.cluster");
                        Partition clusterFile3 = new Partition(path + file + "Tou.cluster");
                        Partition clusterFile4 = new Partition(path + file + "Ten.cluster");
                        Partition clusterFile5 = new Partition(path + file + "Sca.cluster");
                        //new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\polbooks\\polbooks_Beta0.cluster");
                        //Calculate the Errors
                        ExternalEval error = new ExternalEval(clusterFile, labels);
                        ExternalEval error2 = new ExternalEval(clusterFile2, labels);
                        ExternalEval error3 = new ExternalEval(clusterFile3, labels);
                        ExternalEval error4 = new ExternalEval(clusterFile4, labels);
                        ExternalEval error5 = new ExternalEval(clusterFile5, labels);
                       
                        using (StreamWriter sw = new StreamWriter(path + "results2.txt", true))
                        {
                            sw.Write(file + ",");
                            sw.Write(error.NoNoiseTextResults + ",");  // use NoNoise for noisy data no reassign
                            sw.Write(error2.NoNoiseTextResults + ",");
                            sw.Write(error3.NoNoiseTextResults + ",");
                            sw.Write(error4.NoNoiseTextResults + ",");
                            sw.Write(error5.NoNoiseTextResults + ",");

                           sw.WriteLine("");
                        }
                       
                       // using (StreamWriter sw = new StreamWriter(path + "results.txt", true))
                       // {
                       //     sw.Write(file + ",");
                       //     sw.Write(error.ShorterTextResults + ",");  // use ShorterTextResults for no noise
                       //     sw.Write(error2.ShorterTextResults + ",");
                       //     sw.Write(error3.ShorterTextResults + ",");
                       //     sw.Write(error4.ShorterTextResults + ",");
                       //     sw.Write(error5.ShorterTextResults + ",");

                      //      sw.WriteLine("");
                      //  }
                        Console.Write(file + ",");
                        Console.Write(error.NoNoiseTextResults + ",");
                        Console.Write(error2.NoNoiseTextResults + ",");
                        Console.Write(error3.NoNoiseTextResults + ",");
                        Console.Write(error4.NoNoiseTextResults + ",");
                        Console.Write(error5.NoNoiseTextResults + ",");

                        Console.WriteLine("");
                    }
                }
            }
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
            string prefix = "synth\\eqDensity\\set1\\";
            int KNN = 101;
            int end = KNN + 1;
            int D = 2;
            int K = 8;
            for (KNN = 101; KNN < end; KNN++)
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



            // ---->>NOISY DATA AUTOMATED REPORT
            /*         
            int dataSet = 2;
            string path = "synthNoiseRemoval\\set" + dataSet + "\\";//C:\Users\John\Source\Repos\GraphClustering3\debugNetData\bin\Debug\
            string prefix = "synthD8K4";
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
                //num = num.Substring(num.IndexOf("a") + 1);
                //num = num.Substring(num.IndexOf("a") + 1);
                num = num.Substring(num.IndexOf("a") + 1);
                num = num.Substring(0, num.IndexOf("_"));
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
                //num = num.Substring(num.IndexOf("a") + 1);
                //num = num.Substring(num.IndexOf("a") + 1);
                num = num.Substring(num.IndexOf("a") + 1);
                num = num.Substring(0, num.IndexOf("_"));
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
                for (int beta = 0; beta <= maxOfEverything; beta+=1)
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
                            //num = num.Substring(num.IndexOf("a") + 1);
                            //num = num.Substring(num.IndexOf("a") + 1);
                            num = num.Substring(num.IndexOf("a") + 1);
                            num = num.Substring(0, num.IndexOf("_"));
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
                            //num = num.Substring(num.IndexOf("a") + 1);
                            //num = num.Substring(num.IndexOf("a") + 1);
                            num = num.Substring(num.IndexOf("a") + 1);
                            num = num.Substring(0, num.IndexOf("_"));
                            int numToInt = Int32.Parse(num);
                            if (numToInt > maxUnweighted)
                            {
                                maxUnweighted = numToInt;
                            }
                        }

                        if (beta <= maxWeighted)
                        {
                            
                            Partition clusterFile =
                            new Partition(path + prefix + "_KNN_" + i + "_Beta" + beta + "_Weights.cluster");

                            //Calculate the Error
                            ExternalEval error = new ExternalEval(clusterFile, labels);
                            sw.Write(error.NoNoiseTextResults + ",");//sw.Write(error.shorterTextResults + ",");

                            // write the VAT here
                            using (StreamReader sr = new StreamReader(path + prefix + "_KNN_" + i + "_Beta" + beta + "_Weights.cluster"))
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
                           new Partition(path + prefix + "_KNN_" + i + "_Beta" + beta + "_NoWeights.cluster");

                            //Calculate the Error
                            ExternalEval errorUn = new ExternalEval(clusterFileUn, labels);
                            sw.Write(errorUn.NoNoiseTextResults);//sw.Write(errorUn.ShorterTextResults);

                            // write the VAT here
                            sw.Write(",");
                            using (StreamReader sr2 = new StreamReader(path + prefix + "_KNN_" + i + "_Beta" + beta + "_NoWeights.cluster"))
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

            // */
            /*            
                       // USING A NEW BETWEENNESS-CENTRALITY NOT RECALCULATED WAY TO COMPUTE!!!!
                       int[] minks = {4,4,101,3,7,3,3,5,2};
                       string prefix = "synthData\\eqDensity\\set1\\";
                       int KNN = 11037;
                       int end = KNN + 1;
                       int D = 2;
                       int K = 2;

                       for (KNN =11037; KNN < end; KNN++)
                       {
                           //PointSet points = new PointSet("wine\\wine.txt");
                           //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                           //synthD2K2_Euclidean_KNN_4.graph
                           //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD"+D +"K"+K+"_Euclidean_KNN_"+ KNN + ".graph");
                           LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph");
                           //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                           int numClusters = 1;
                           int beta = 0;

                           // Do it the old way the first time to compute inital 
                           HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                           Partition p = vClust.GetPartition();
                           //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                           p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                                   prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           List<int> nodeRemovalOrder = vClust._vatNodeRemovalOrder;
                           int numNodesRemoved = vClust._vatNumNodesRemoved;
                           beta+=10;
                           numClusters = p.Clusters.Count;


                           while (numClusters < K)
                           {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                               //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               HVATClust hvClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                               //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                               q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN +"_Beta"+beta+ "Partial_Weights.cluster", 
                                                       prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                               beta+=10;
                               numClusters = q.Clusters.Count;
                           }
                       }

                       KNN = 26877;
                       end = KNN + 1;
                       D = 2;
                       K = 4;
                       for (KNN = 26877; KNN < end; KNN++)
                       {
                           //PointSet points = new PointSet("wine\\wine.txt");
                           //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                           //synthD2K2_Euclidean_KNN_4.graph
                           //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph");
                           //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                           int numClusters = 1;
                           int beta = 0;

                           // Do it the old way the first time to compute inital 
                           HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                           Partition p = vClust.GetPartition();
                           //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                           p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                                   prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           List<int> nodeRemovalOrder = vClust._vatNodeRemovalOrder;
                           int numNodesRemoved = vClust._vatNumNodesRemoved;
                           beta+=10;
                           numClusters = p.Clusters.Count;


                           while (numClusters < K)
                           {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                               //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               HVATClust hvClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                               //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                               q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                                       prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                               beta+=10;
                               numClusters = q.Clusters.Count;
                           }
                       }

                       KNN = 50794;
                       end = KNN + 1;
                       D = 2;
                       K = 8;
                       for (KNN = 50794; KNN < end; KNN++)
                       {
                           //PointSet points = new PointSet("wine\\wine.txt");
                           //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                           //synthD2K2_Euclidean_KNN_4.graph
                           //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph");
                           //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                           int numClusters = 1;
                           int beta = 0;

                           // Do it the old way the first time to compute inital 
                           HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                           Partition p = vClust.GetPartition();
                           //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                           p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                                   prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           List<int> nodeRemovalOrder = vClust._vatNodeRemovalOrder;
                           int numNodesRemoved = vClust._vatNumNodesRemoved;
                           beta+=10;
                           numClusters = p.Clusters.Count;


                           while (numClusters < K)
                           {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                               //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               HVATClust hvClust = new HVATClust(lwg2, 2, true, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                               //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                               q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_Weights.cluster",
                                                       prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                               beta+=10;
                               numClusters = q.Clusters.Count;
                           }
                       }
                       //===============
                       KNN = 11037;
                       end = KNN + 1;
                       D = 2;
                       K = 2;
                       for (KNN = 11037; KNN < end; KNN++)
                       {
                           //PointSet points = new PointSet("wine\\wine.txt");
                           //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                           //synthD2K2_Euclidean_KNN_4.graph
                           //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph");
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
                           beta+=100;
                           numClusters = p.Clusters.Count;


                           while (numClusters < K)
                           {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                               //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               HVATClust hvClust = new HVATClust(lwg2, 2, false, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                               //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                               q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_NoWeights.cluster",
                                                       prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                               beta+=100;
                               numClusters = q.Clusters.Count;
                           }
                       }

                       KNN = 26877;
                       end = KNN + 1;
                       D = 2;
                       K = 4;
                       for (KNN = 26877; KNN < end; KNN++)
                       {
                           //PointSet points = new PointSet("wine\\wine.txt");
                           //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                           //synthD2K2_Euclidean_KNN_4.graph
                           //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph");
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
                           beta+=100;
                           numClusters = p.Clusters.Count;


                           while (numClusters < K)
                           {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                               //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               HVATClust hvClust = new HVATClust(lwg2, 2, false, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                               //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                               q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_NoWeights.cluster",
                                                       prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                               beta+=100;
                               numClusters = q.Clusters.Count;
                           }
                       }

                       KNN = 50794;
                       end = KNN + 1;
                       D = 2;
                       K = 8;
                       for (KNN = 50794; KNN < end; KNN++)
                       {
                           //PointSet points = new PointSet("wine\\wine.txt");
                           //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                           //synthD2K2_Euclidean_KNN_4.graph
                           //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                           LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph");
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
                           beta+=100;
                           numClusters = p.Clusters.Count;


                           while (numClusters < K)
                           {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                               //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               HVATClust hvClust = new HVATClust(lwg2, 2, false, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                               Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                               //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                               q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "Partial_NoWeights.cluster",
                                                       prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                               beta+=100;
                               numClusters = q.Clusters.Count;
                           }
                       }
                       // */


            /* READGML OF POLBOOKS

                LightWeightGraph mygraph = LightWeightGraph.GetGraphFromGML("polblogs.gml");
                //Console.ReadKey();
                using (StreamWriter sw = new StreamWriter("polblogs.data", true))
                {
                    for (int i = 0; i< mygraph.NumNodes; i++)
                    {
                        sw.Write(i + " " + mygraph.Nodes[i].Label);

                       // for (int j = 0; j < mygraph.Nodes[i].Edge.Count(); j++)
                       // {
                       //     sw.Write(mygraph.Nodes[i].Edge[j] + " ");
                       // }

                        sw.WriteLine();

                    }
                }
            */
            /*
                        // THIS IS THE HIGHLY DESIRABLE WHILE LOOP, Set up for REALLIFE DATA!!!!
            
string prefix = "polbooks\\";
            
            
                            //PointSet points = new PointSet("wine\\wine.txt");
                            //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                            //synthD2K2_Euclidean_KNN_4.graph
                            LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromGML(prefix + "polbooks.gml");
                            //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                            int numClusters = 1;
                            int beta = 0;
                            while (numClusters < 3)
                            {                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                                HVATClust vClust = new HVATClust(lwg2, 2, false, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                                Partition p = vClust.GetPartition();
                                //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                                p.SavePartition(prefix + "polbooks_" + "Beta" + beta + ".cluster",
                                                        prefix + "polbooks.gml");
                                beta++;
                                numClusters = p.Clusters.Count;
                            }
              // */
            /*
                        // HVAT CALCULATION, FOR ARTIFICIAL SETS ---  IS A BUG REVEALED??
                        int set = 1;
                        int KNN = 100;
            
                        int D = 2;
                        int K = 8;
                        string path = "NOverlapEqDens\\set"+set+"\\";
                        string filename = "synthD"+D+"K"+K+"."+set+".txt";
                        //string filename = "synthD"+D+"K"+K+"_Euclidean_KNN_"+KNN+".graph";\
                        //string filename = "synthD"+D+"K"+K+"_Euclidean_"+KNN+".graph";
                        PointSet points = new PointSet(path + filename);
                        //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(), KNN);
                        //LightWeightGraph lwg2 = LightWeightGraph.GetGeometricGraph(points.GetDistanceMatrix(), .53713);
                        //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + filename);


                        IPointGraphGenerator gen;


                        var knnGen = new KNNGraphGenerator();
                        knnGen.SetMinimumConnectivity();
                        knnGen.SetK(KNN);
                        knnGen.SetSkipLast(true);
                        knnGen.SetMinOffset(0);
                        //knnGen.SetK(3);
                        gen = knnGen;

                        // These 3 lines just for the Geometric graphs
                        //var rGen = new GeoGraphGenerator();
                        //rGen.SetMinimumConnectivity();
                        //gen = rGen;
                        //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + "synthD2K2_Euclidian_KNN_100.graph");
                        HVATClust vClust = new HVATClust(points, K, gen, true, 1, 0, true, true);
                        //HVATClust vClust = new HVATClust(lwg2, K, true, 1, 0, true, true);
                        Partition p = vClust.GetPartition();
                        //p.SavePartition("ecoli\\ecoliHVAT"+KNN+"_lwg_weights_810.cluster", "ecoli\\ecoli_Euclidean_KNN_"+KNN+".graph");
                        p.SavePartition(path + "HIER_D"+D+"k"+K+"_Eq_set"+set+"_" + KNN + "_points_WeightsSL.cluster", path+filename);
                        //*/
            /* POINTS

                        int set = 3;
                        int KNN = 136030;
                        int D = 8;
                        int K = 4;
                        string path = "synthData\\unEqDensity\\set" + set + "\\";
                        string filename = "synthD" + D + "K" + K + "." + set + ".txt";
                        PointSet points = new PointSet(path + filename);
                        IPointGraphGenerator gen;
                        var rGen = new GeoGraphGenerator();
                        rGen.SetMinimumConnectivity();
                        gen = rGen;
                        HVATClust vClust = new HVATClust(points, K, gen, true, 1, 0, false, true);
                        Partition p = vClust.GetPartition();
                        p.SavePartition(path + "D" + D + "k" + K + "_UnEq_set" + set + "_" + KNN + "_points_Weights.cluster", path + filename);
            
                        set = 2;
                        KNN = 88405;
                        path = "synthData\\unEqDensity\\set" + set + "\\";
                        filename = "synthD" + D + "K" + K + "." + set + ".txt";
                        points = new PointSet(path + filename);
                        rGen = new GeoGraphGenerator();
                        rGen.SetMinimumConnectivity();
                        gen = rGen;
                        vClust = new HVATClust(points, K, gen, true, 1, 0, false, true);
                        p = vClust.GetPartition();
                        p.SavePartition(path + "D" + D + "k" + K + "_UnEq_set" + set + "_" + KNN + "_points_Weights.cluster", path + filename);
            
                        set = 3;
                        KNN = 87205;
                        path = "synthData\\unEqDensity\\set" + set + "\\";
                        filename = "synthD" + D + "K" + K + "." + set + ".txt";
                        points = new PointSet(path + filename);
                        rGen = new GeoGraphGenerator();
                        rGen.SetMinimumConnectivity();
                        gen = rGen;
                        vClust = new HVATClust(points, K, gen, true, 1, 0, false, true);
                        p = vClust.GetPartition();
                        p.SavePartition(path + "D" + D + "k" + K + "_UnEq_set" + set + "_" + KNN + "_points_Weights.cluster", path + filename);
  
             // */
            /* THREE TIMES TRHOUGH HVAT FOR GEOMETRIC GRAPHS
                    String set = "1HIER";
                    int KNN = 88848;
                    int D = 8;
                    int K = 8;
                    string path = "synthData\\eqDensity\\set" + set + "\\";
                    string filename = "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph";
                    LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + filename);
                    HVATClust vClust = new HVATClust(lwg2, K, true, 1, 0, false, true);
                    Partition p = vClust.GetPartition();
                    p.SavePartition(path + "D" + D + "k" + K + "_Eq_set" + set + "_" + KNN + "_lwg_Weights.cluster", path + filename);
                   // set = 2;
                   // KNN = 119751;
                    path = "synthData\\eqDensity\\set" + set + "\\";
                    filename = "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph";
                    lwg2 = LightWeightGraph.GetGraphFromFile(path + filename);
                    vClust = new HVATClust(lwg2, K, false, 1, 0, false, true);
                    p = vClust.GetPartition();
                    p.SavePartition(path + "D" + D + "k" + K + "_Eq_set" + set + "_" + KNN + "_lwg_NoWeights.cluster", path + filename);
                /*    set = 3;
                    KNN = 121350;
                    path = "synthData\\unEqDensity\\set" + set + "\\";
                    filename = "synthD" + D + "K" + K + "_Euclidean_" + KNN + ".graph";
                    lwg2 = LightWeightGraph.GetGraphFromFile(path + filename);
                    vClust = new HVATClust(lwg2, K, true, 1, 0, false, true);
                    p = vClust.GetPartition();
                    p.SavePartition(path + "D" + D + "k" + K + "_UnEq_set" + set + "_" + KNN + "_lwg_Weights.cluster", path + filename);
         //  */
            /* 
               // ACCURACY CHECK
                   int set = 2;
                   int knn = 1;
                   int D=2;
                   int K=8;
                   String file = "D"+D+"k"+K+"_UnEq_set"+set+"_"+knn+"_lwg_NoWeights.cluster";
                   String file2 = "D" + D + "k" + K + "_UnEq_set" + set + "_" + knn + "_lwg_Weights.cluster";
                   String file3 = "D" + D + "k" + K + "_UnEq_set" + set + "_" + knn + "_points_NoWeights.cluster";
                   String file4 = "D" + D + "k" + K + "_UnEq_set" + set + "_" + knn + "_points_Weights.cluster";
                   DelimitedFile delimitedLabelFile =
                                      new DelimitedFile("synth\\unEqDensity\\set"+set+"\\synthD"+D+"K"+K+"."+set+".data");
                   //new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\synth\\unEqDensity\\set1\\synthD4K8.1.data");
                   int labelCol = delimitedLabelFile.Data[0].Length;
                   LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

                   //get the Partion file
                   Partition clusterFile =
                       //new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\synth\\unEqDensity\\set1\\synthD4K8_KNN_3_Beta0Partial_Weights.cluster");
                       new Partition("synthData\\unEqDensity\\set"+set+"\\" + file);
                   Partition clusterFile2 =
                       new Partition("synthData\\unEqDensity\\set" + set + "\\" + file2);
                   Partition clusterFile3 =
                       new Partition("synthData\\unEqDensity\\set" + set + "\\" + file3);
                   Partition clusterFile4 =
                       new Partition("synthData\\unEqDensity\\set" + set + "\\" + file4);
                   //Calculate the Error
                   ExternalEval error = new ExternalEval(clusterFile, labels);
                   ExternalEval error2 = new ExternalEval(clusterFile2, labels);
                   ExternalEval error3 = new ExternalEval(clusterFile3, labels);
                   ExternalEval error4 = new ExternalEval(clusterFile4, labels);

                   using (StreamWriter sw = new StreamWriter("synthData\\unEqDensity\\set"+set+"\\allresults.txt", true))
                   {
                       sw.WriteLine(file);
                      sw.WriteLine(error.TextResults);
                      sw.WriteLine("");
                      sw.WriteLine(file2);
                      sw.WriteLine(error2.TextResults);
                      sw.WriteLine("");
                      sw.WriteLine(file3);
                      sw.WriteLine(error3.TextResults);
                      sw.WriteLine("");
                      sw.WriteLine(file4);
                      sw.WriteLine(error4.TextResults);
                      sw.WriteLine("");
                    }
                   Console.WriteLine(error.TextResults);
                   Console.WriteLine(error2.TextResults);
                   Console.WriteLine(error3.TextResults);
                   Console.WriteLine(error4.TextResults);

                   Console.ReadKey();   
            
          // */
            /*
                        // CALCULATING BETA VAT BASED ON POINT SETS ONLY!
                        // MINKNN AND GEOMETRIC ONLY!


                        int set = 1;
                        int KNN = 100;
                        int D = 2;
                        int K = 8;
                        int KNNinit = KNN;
                        int end = KNN + 1;
                        string prefix = "NOverlapEqDens\\set" + set + "aa\\";
                        string filename = "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph";
                        //string pointSetName = "synthD" + D + "K" + K + "." + set + ".txt";

                        for (KNN = KNNinit; KNN < end; KNN++)
                        {
                          //  PointSet points = new PointSet(prefix + "\\synthD"+D+"K"+K+"."+set+".txt");
                            //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                            //synthD2K2_Euclidean_KNN_4.graph
                            //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD"+D +"K"+K+"_Euclidean_KNN_"+ KNN + ".graph");
                            LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                            //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                            int numClusters = 1;
                            int beta = 65;

                        //    IPointGraphGenerator gen;
                        //    var knnGen = new KNNGraphGenerator();
                        //    knnGen.SetMinimumConnectivity();
                            //knnGen.SetSkipLast(true);
                        //    knnGen.SetMinOffset(0);
                
                            //knnGen.SetK(3);
                        //    gen = knnGen;
                
                            // These 3 lines just for the Geometric graphs
                            //var rGen = new GeoGraphGenerator();
                            //rGen.SetMinimumConnectivity();
                            //gen = rGen;
                            //LightWeightGraph lwg2 = knnGen.GenerateGraph(points.GetDistanceMatrix());
                
                

                            // Do it the old way the first time to compute inital 
                            //HVATClust vClust = new HVATClust(points, 2, gen, true, 1, beta, true, true);
                            HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                            Partition p = vClust.GetPartition();
                            //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                            p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "_Weights.cluster",
                                                    prefix + filename);
                            List<int> nodeRemovalOrder = vClust._vatNodeRemovalOrder;
                            int numNodesRemoved = vClust._vatNumNodesRemoved;
                            beta += 100;
                            numClusters = p.Clusters.Count;

                            //while (beta < 100)
                            while (numClusters < K + 3)
                            {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                                //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                                HVATClust hvClust = new HVATClust(lwg2, 2, true, 1, beta, true, true);
                                //HVATClust hvClust = new HVATClust(points, 2, gen, true, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                                Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                                //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                                q.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "_Weights.cluster",
                                                        prefix + filename);
                                beta += 100;
                                numClusters = q.Clusters.Count;
                            }
                        }



   
              //  */
            /*
                       //================================================

                       // This program prints the INTERNAL VALIDATIONS in a spreadsheet format
                       String dataSet = "D8K8";
                       String path = "NOverlapUneqDens";
                       String[] filePaths = Directory.GetFiles(path + "\\set1SecondTryFixedUp\\", "synth"+ dataSet + "*.cluster");

                       using (StreamWriter sw = new StreamWriter(path +"\\"+ dataSet+ "InternalValidation.csv", true))
                       {
                           sw.WriteLine("Name, Dunn, AvgSilhouette, DaviesBouldin");
                           for (int j = 0; j < filePaths.Length; j++)
                           {
                               String safeFileName = filePaths[j].GetShortFilename().GetFilenameNoExtension();
                               Partition clusters = new Partition(filePaths[j]);
                               sw.WriteLine(safeFileName + "," + InternalEval.avgDunnIndex(clusters) + ","
                               + InternalEval.AverageSilhouetteIndex(clusters) + "," + InternalEval.DaviesBouldinIndex(clusters));
                           }
                       }
            
                       //=================================================
           //  */
            /*
                        // THIS IS THE HIGHLY DESIRABLE WHILE LOOP, SET UP FOR ECOLI 8 CLUSTERS
                        int KNN = 4;
                        int dim = 8;
                        //for (dim = 1; dim < 4; dim++)
                        //{
                            //PointSet points = new PointSet("wine\\wine.txt");
                            //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                        LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile("hotnet24weighted.graph");
                            //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                            int numClusters = 2;
                            int beta = 0;
                            while (numClusters < 8)
                            {                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                                HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                                Partition p = vClust.GetPartition();
                                //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                                p.SavePartition("hotnet_" + beta + ".cluster", "hotnet24weighted.graph");
                                beta++;
                                numClusters = p.Clusters.Count;
                            }
                        //}
                        //*/
            //===============================================================================

            /*
            // CALCULATING BETA VAT BASED ON POINT SETS ONLY!
            // MINKNN AND GEOMETRIC ONLY!


            int set = 1;
            int KNN = 5;
            int D = 2;
            int K = 6;
            int KNNinit = KNN;
            int end = KNN + 1;
            string prefix = "NOverlapEqDens\\set" + set + "aa\\";
            string filename = "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph";
            //string pointSetName = "synthD" + D + "K" + K + "." + set + ".txt";

            for (KNN = KNNinit; KNN < end; KNN++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                //PointSet points = new PointSet(prefix + "\\synthD"+D+"K"+K+"."+set+".txt");
                PointSet points = new PointSet("timing\\ecoli.txt");
                LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(), KNN); // number= Kneighbors
                //synthD2K2_Euclidean_KNN_4.graph
                //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD"+D +"K"+K+"_Euclidean_KNN_"+ KNN + ".graph");
                //    LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(prefix + "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph");
                //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                int numClusters = 1;
                int beta = 0;

                //    IPointGraphGenerator gen;
                //    var knnGen = new KNNGraphGenerator();
                //    knnGen.SetMinimumConnectivity();
                //knnGen.SetSkipLast(true);
                //    knnGen.SetMinOffset(0);

                //knnGen.SetK(3);
                //    gen = knnGen;

                // These 3 lines just for the Geometric graphs
                //var rGen = new GeoGraphGenerator();
                //rGen.SetMinimumConnectivity();
                //gen = rGen;
                //LightWeightGraph lwg2 = knnGen.GenerateGraph(points.GetDistanceMatrix());



                // Do it the old way the first time to compute inital 
                //HVATClust vClust = new HVATClust(points, 2, gen, true, 1, beta, true, true);
                HVATClust vClust = new HVATClust(lwg2, 2, true, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                Partition p = vClust.GetPartition();
            //    p.SavePartition("timing\\ecoli_NoWeights_KNN" + KNN + "_Beta_" + beta + ".cluster", "timing\\ecoli_Euclidean_KNN_" + KNN + ".graph");
                //    p.SavePartition(prefix + "synthD" + D + "K" + K + "_KNN_" + KNN + "_Beta" + beta + "_Weights.cluster",
                //                             prefix + filename);
                List<int> nodeRemovalOrder = vClust._vatNodeRemovalOrder;
                int numNodesRemoved = vClust._vatNumNodesRemoved;
                beta += 1;
                numClusters = p.Clusters.Count;

                while (beta < 10)
                //while (numClusters < 6)
                {                                 //graph, mink, useweights,                       alpha, beta, reassign, hillclimb 
                    //HVATClust hvClust = new HVATClust(lwg2, 2, false, nodeRemovalOrder, numNodesRemoved, 1, beta, false, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    HVATClust hvClust = new HVATClust(lwg2, 2, true, 1, beta, true, true);
                    //HVATClust hvClust = new HVATClust(points, 2, gen, true, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                    Partition q = hvClust.GetPartition(nodeRemovalOrder, numNodesRemoved);
                    //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
             //       q.SavePartition("timing\\ecoli_NoWeights_KNN_" + KNN + "_Beta_" + beta + ".cluster",prefix + filename);
                    beta += 1;
                    numClusters = q.Clusters.Count;
                }

                using (StreamWriter swr = new StreamWriter("timing\\aecoliresults.txt", true))
                {

                    sw.Stop();
                    swr.WriteLine(sw.Elapsed.TotalMilliseconds);
                    
                }
}

            
             //**************************************************************************
             
            
             //  */

            /*
                         // HVAT CALCULATION, FOR REAL SETS ---  IS A BUG REVEALED??


             Stopwatch sw = Stopwatch.StartNew();            
             int set = 1;
                         int KNN = 5;

                         int D = 2;
                         int K =2;
                         string path = "realdata\\breast_w\\";
                         string filename = "breast_w_Euclidean_KNN_5.graph";
                         //LightWeightGraph lwg2;

                         //lwg2 = LightWeightGraph.GetGraphFromGML(path + filename);lwg2.IsWeighted = true;
                         //lwg2.SaveGraph(path + "polbooks.graph");
                         //string filename = "synthD"+D+"K"+K+"_Euclidean_KNN_"+KNN+".graph";\
                         //string filename = "synthD"+D+"K"+K+"_Euclidean_"+KNN+".graph";
                   //      PointSet points = new PointSet(path + filename);
                  //       LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(), KNN);
                         //LightWeightGraph lwg2 = LightWeightGraph.GetGeometricGraph(points.GetDistanceMatrix(), .53713);
                         LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + filename);
                         //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromGML(path + filename);


                     //    IPointGraphGenerator gen;

                       //  var knnGen = new KNNGraphGenerator();
                       //  knnGen.SetMinimumConnectivity();
                         //knnGen.SetK(KNN);
                         //knnGen.SetSkipLast(false);
                      //   knnGen.SetMinOffset(2);
                         //knnGen.SetK(3);
                      //   gen = knnGen;

                         // These 3 lines just for the Geometric graphs
                         //var rGen = new GeoGraphGenerator();
                         //rGen.SetMinimumConnectivity();
                         //gen = rGen;
                         //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + "synthD2K2_Euclidian_KNN_100.graph");
                      //   HVATClust vClust = new HVATClust(points, K, gen, false, 1, 0, true, true);
                         HVATClust vClust = new HVATClust(lwg2, K, false, 1, 0, true, true);
                         Partition p = vClust.GetPartition();
                         //p.SavePartition("ecoli\\ecoliHVAT"+KNN+"_lwg_weights_810.cluster", "ecoli\\ecoli_Euclidean_KNN_"+KNN+".graph");
                         p.SavePartition(path + "breastWHIER_KNN_" + KNN + "_lwg_NoWeights.cluster", path+filename);


                         using (StreamWriter swr = new StreamWriter("realdata\\breast_w\\aResultbreast_w.txt", true))
                         {

                             sw.Stop();
                             swr.WriteLine(sw.Elapsed.TotalMilliseconds);

                         }


             //*/
            /*
                        //CALCULATING THE RAND INDEX

                        //start by parsing label file
                        DelimitedFile delimitedLabelFile = new DelimitedFile("realData\\wine\\wine.data");
                        int labelCol = delimitedLabelFile.Data[0].Length;
                        LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

                        //get the Partion file
                        Partition clusterFile = new Partition("realData\\wine\\wineNoWeights6_21_1.cluster");
                        int[] assignments = new int[labels.LabelIndices.Length];

                        for (int cluster = 0; cluster < clusterFile.Clusters.Count; cluster++)
                        {
                            for (int j = 0; j < clusterFile.Clusters[cluster].Points.Count; j++ )
                            {
                                int clusterid = clusterFile.Clusters[cluster].Points[j].ClusterId;
                                int id = clusterFile.Clusters[cluster].Points[j].Id;
                                assignments[id] = clusterid;
                            }
                        }

                        // compare two arrays, assigments and labels.LabelIndices
                        int a=0;
                        int b=0;
                        for (int i=0; i< assignments.Length; i++)
                        {
                            for (int j=i+1; j < assignments.Length; j++)
                            {
                               //Check for case a -> i and j are in same cluster in assignments and LabelIndices
                                if (labels.LabelIndices[i] == labels.LabelIndices[j] && assignments[i] == assignments[j])
                                {
                                    a++;
                                }
                                else if (labels.LabelIndices[i] != labels.LabelIndices[j] && assignments[i] != assignments[j])
                                {
                                    b++;
                                }
                            }
                        }

                        int denominator = assignments.Length * (assignments.Length - 1) / 2;
                        double randIndex = (a + b) / (double)denominator;
                        Console.WriteLine("Rand Index: " + randIndex);

                        ExternalEval error = new ExternalEval(clusterFile, labels);
                        Console.WriteLine(error.TextResults);
                        Console.ReadKey(); 
                        */



            /*
// GN FILES CHECK ACCURACY ONE AT A TIME
                 DelimitedFile delimitedLabelFile =
                            //new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\polbooks\\polbooks.data");
                 new DelimitedFile("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\GNGraphs\\eqDensity\\set1HIER\\synthD4K4.1.data");
                    int labelCol = delimitedLabelFile.Data[0].Length;
                    LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

                    //get the Partion file
                    Partition clusterFile =
                        new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\synthData\\eqDensity\\set1HIER\\D4k4_Eq_set1HIER_7_lwg_NoWeights.cluster");
                        //new Partition("C:\\Users\\John\\Source\\Repos\\GraphClustering3\\debugNetData\\bin\\Debug\\polbooks\\polbooks_Beta0.cluster");
                    //Calculate the Error
                    ExternalEval error = new ExternalEval(clusterFile, labels);

                    using (StreamWriter sw = new StreamWriter("synthData\\eqDensity\\set1HIER\\results.txt", true))
                    {
                        sw.WriteLine("D4k4_Eq_set1HIER_7_lwg_NoWeights.cluster");
                       sw.WriteLine(error.TextResults);
                       sw.WriteLine("");
                    }
                    Console.WriteLine(error.TextResults);

                Console.ReadKey(); 
               // */



            /*  
  // PERFORM CLUSTERING FOR THE GN GRAPHS
              int num = 1;
              //int dim = 2;
            string subdir = "80";
              for (num = 1; num <= 100; num++)
              {

                string numString = "" + num;
                if (num < 10)
                {
                    numString = "00" + num;
                } else if (num < 100)
                {
                    numString = "0" + num;
                }
                
                //PointSet points = new PointSet("wine\\wine.txt");
                  //LightWeightGraph lwg2 = LightWeightGraph.GetKNNGraph(points.GetDistanceMatrix(),KNN); // number= Kneighbors
                  LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile("GNGraphs\\out"+subdir+"\\" + numString + ".graph");
                Console.WriteLine("Processing " + numString);
                lwg2.SaveGML("GNGraphs\\out80\\test.gml");
                if (lwg2.isConnected())
                {
                    Console.WriteLine("Graph " + subdir + " " + numString + " is connected");
                }
                  //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
                  //int numClusters = 4;
                  int beta = 0;
                  //while (numClusters < 8)
                  //{                                 //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                      HVATClust vClust = new HVATClust(lwg2, 4, false, 1, 0, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                      
                      //HVATClust vClust = new HVATClust(lwg2, 4, false, 1, beta, true, true);//new HVATClust(swissPoints, 4, false, true, 1);
                      
                Partition p = vClust.GetPartition();
                      //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
                      p.SavePartition("GNGraphs\\out"+subdir+"\\" + numString + ".cluster", "GNGraphs2\\out"+subdir+"\\"+numString + ".graph");
                      //beta++;
                      //numClusters = p.Clusters.Count;
                  //}
              }
            //*/
            /*
            String path = "C:\\Users\\John\\Dropbox\\ClustProject\\John\\24NodeGraphs\\";
            String grph = "symmetric24";
            LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + grph + ".graph");
            //k, weighted, double alpha = 1.0f, double beta = 0.0f, reassignNodes = true, hillClimb = true
            //graph, mink, useweights, alpha, beta, reassign, hillclimb 
            HVATClust vClust = new HVATClust(lwg2, 2, false, 1, 0, true, false);//new HVATClust(swissPoints, 4, false, true, 1);
            Partition p = vClust.GetPartition();
            //p.SavePartition("wineLOO\\wine_NoWeights"+KNN+"_21_" + beta + ".cluster", "wine\\wine_Euclidean_KNN_"+KNN+".graph");
            p.SavePartition(path + grph + ".cluster", path + grph + ".graph");
            //*/


            /*
            for (int set = 9; set < 11; set++)
            {
                String path = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\unEq10N\\";
                String pathDest = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\unEq10N-unweighted-reassign-2dhill\\";
                //String path = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\Uneq10N\\";
                //String pathDest = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\Uneq10N-unweighted-reassign-2dhill\\";
                Boolean useweights = false;
                Boolean reassign = true;
                Boolean hillclimb = true;
                for (int D = 2; D <= 8; D = D * 2)
                {
                    for (int K = 2; K <= 8; K = K * 2)
                    {
                        //D = 4;  K = 8;
                        String grph = "synthD" + D + "K" + K+"."+set;
                        if (!File.Exists(path + grph + ".graph"))
                        {
                            continue;
                        }
                        Console.WriteLine(grph);
                        LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + grph + ".graph");
                        //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                        HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                        Partition p = clust1.GetPartition();
                        p.SavePartition(pathDest + grph + "VAT.cluster", path + grph + ".graph");
                        HIntegrityClust clust2 = new HIntegrityClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                        Partition p2 = clust2.GetPartition();
                        p2.SavePartition(pathDest + grph + "Int.cluster", path + grph + ".graph");
                        HToughnessClust clust3 = new HToughnessClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                        Partition p3 = clust3.GetPartition();
                        p3.SavePartition(pathDest + grph + "Tou.cluster", path + grph + ".graph");
                        HTenacityClust clust4 = new HTenacityClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                        Partition p4 = clust4.GetPartition();
                        p4.SavePartition(pathDest + grph + "Ten.cluster", path + grph + ".graph");
                        HScatteringClust clust5 = new HScatteringClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                        Partition p5 = clust5.GetPartition();
                        p5.SavePartition(pathDest + grph + "Sca.cluster", path + grph + ".graph");

                    }
                }
            }
            //*/

            /*            // create graphs from data files
                        KPoint.DistType distType = KPoint.DistType.Euclidean;
                        //for (int set = 1; set < 11; set++)
                        //{
                            String path = "C:\\Users\\John\\Dropbox\\ClustProject\\John\\PercentageConnected\\iris\\";
            //for (int D = 8; D <= 8; D = D * 2)
            //{
            //for (int K = 2; K <= 8; K = K * 2)
            // {
            double percentage = .65;
            String grph = "iris";
                                    // String grph = "synthD8K8.1";
                                    //String grph = "synthD" + D + "K" + K + "." + set;
                                    PointSet points = new PointSet(path + grph + ".txt");
                                    String graphPrefix = grph + "_" + distType.ToString() + "_KNN_" + percentage + "_";
                                    DistanceMatrix distMatrix = points.GetDistanceMatrix(distType);
                                    List<double> distances = distMatrix.GetSortedDistanceList();
                                    int minConnectIndex = LightWeightGraph.BinSearchKNNMinConnectivity(2, points.Count - 1, points.Count, distMatrix, percentage);
                                    LightWeightGraph lwg = LightWeightGraph.GetKNNGraph(distMatrix, minConnectIndex);
                                    lwg.SaveGML(path + graphPrefix + minConnectIndex + ".gml");
                                    //lwg.SaveGraph(path + grph + ".graph");
                                    lwg.SaveGraph(path + graphPrefix + minConnectIndex + ".graph");

            //}
            //}
            //}
            //  */

            /*
                        DelimitedFile delimitedLabelFile =
                                new DelimitedFile("C:\\Users\\John\\Dropbox\\ClustProject\\John\\synthNoiseRemoval\\set2\\synthD2K4.data");
                        int labelCol = delimitedLabelFile.Data[0].Length;
                        LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol - 1));

                        //get the Partion file
                        Partition clusterFile =
                            new Partition("C:\\Users\\John\\Dropbox\\ClustProject\\John\\synthNoiseRemoval\\set2\\synthD8K4_KNN_3_Beta474_NoWeights.cluster");

                        //Calculate the Error
                        ExternalEval error = new ExternalEval(clusterFile, labels);

                        using (StreamWriter sw = new StreamWriter("C:\\Users\\John\\Dropbox\\ClustProject\\John\\synthNoiseRemoval\\set2\\cluisterAccuracy.txt", true))
                        {
                            sw.WriteLine("synthD8K4_KNN_3_Beta474_NoWeights.cluster");
                            sw.WriteLine(error.TextResults);
                            sw.WriteLine("");
                        }
                        Console.WriteLine(error.TextResults);

                    Console.ReadKey(); 

            */
            //*
            String path = "C:\\Users\\John\\Dropbox\\ClustProject\\John\\PercentageConnected\\Uneq0N\\";
            String pathDest = "C:\\Users\\John\\Dropbox\\ClustProject\\John\\PercentageConnected\\Uneq0N\\output\\";
            //String path = "C:\\Users\\John\\Dropbox\\ClustProject\\SyntheticLFRNets\\binary_networks\\John\\";
            //String pathDest = "C:\\Users\\John\\Dropbox\\ClustProject\\SyntheticLFRNets\\binary_networks\\John\\";
            //String path = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\Uneq10N\\";
            //String pathDest = "C:\\Users\\John\\Dropbox\\ClustProject\\LitData\\DataGeneration\\Uneq10N-unweighted-reassign-2dhill\\";
            Boolean useweights = false;
                        Boolean reassign = true;
                        Boolean hillclimb = false;
                        //for (int D = 2; D <= 8; D = D * 2)
                        //{
                        //    for (int K = 2; K <= 8; K = K * 2)
                        //    {
                        int K = 8;        
                        //D = 4;  K = 8;
                                String grph = "synthD4K8.6_Euclidean_KNN_0.4_100";

                                Console.WriteLine(grph);
                                LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + grph + ".graph");
                                //LightWeightGraph lwg2 = LightWeightGraph.GetGraphFromFile(path + grph + ".graph");
                                //graph, mink, useweights, alpha, beta, reassign, hillclimb 
                                HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                                Partition p = clust1.GetGPartition();
                                p.SavePartition(pathDest + grph + "VAT.cluster", path + grph + ".graph");
                                HIntegrityClust clust2 = new HIntegrityClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                                Partition p2 = clust2.GetGPartition();
                                p2.SavePartition(pathDest + grph + "Int.cluster", path + grph + ".graph");
                                HToughnessClust clust3 = new HToughnessClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                                Partition p3 = clust3.GetGPartition();
                                p3.SavePartition(pathDest + grph + "Tou.cluster", path + grph + ".graph");
                                HTenacityClust clust4 = new HTenacityClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                                Partition p4 = clust4.GetGPartition();
                                p4.SavePartition(pathDest + grph + "Ten.cluster", path + grph + ".graph");
                                HScatteringClust clust5 = new HScatteringClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
                                Partition p5 = clust5.GetGPartition();
                                p5.SavePartition(pathDest + grph + "Sca.cluster", path + grph + ".graph");
            //*/

        }
                
   
}
}
