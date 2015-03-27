using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NetMining.Data;
using NetMining.Files;
using NetMining.SOM;
using NetMining.Graphs;
using NetMining.ClusteringAlgo;
using NetMining.Evaluation;
using NetMining.ExtensionMethods;

namespace GraphClustering
{

    public partial class Form1 : Form
    {
        public string pointSetFile;
        public string pointSetFileShort;
        PointSet points;
        List<float> distances = null;
        int minConnectIndex = 0;
        string graphPrefix;
        DistanceMatrix distMatrix;
        Boolean formLoaded = false;

        public static String JAVA_PATH;

        public void findJavaPath()
        {
            //Here we check to see if the java path has been defined before
            string jpath = Properties.Settings.Default.JavaPath;
            //if it has been defined, we can simply set our path and exit
            if (jpath != "null")
            {
                JAVA_PATH = jpath;
                return;
            }
            
            //Otherwise we must try to locate the file
            JAVA_PATH = "C:\\Program Files (x86)\\Java\\jre7\\bin\\java.exe";
            if (File.Exists(JAVA_PATH))
                return;
            JAVA_PATH = "C:\\Program Files (x86)\\Java\\jre6\\bin\\java.exe";
            if (File.Exists(JAVA_PATH))
                return;
            JAVA_PATH = "C:\\Program Files\\Java\\jre7\\bin\\java.exe";
            if (File.Exists(JAVA_PATH))
                return;
            JAVA_PATH = "C:\\Program Files\\Java\\jre6\\bin\\java.exe";
            if (File.Exists(JAVA_PATH))
                return;
            JAVA_PATH = "C:\\Windows\\SysWOW64\\java.exe";
            if (File.Exists(JAVA_PATH))
                return;
            JAVA_PATH = "C:\\Windows\\System32\\java.exe";
            if (File.Exists(JAVA_PATH))
                return;
            
            //file could not be found, we must prompt the user
            if (MessageBox.Show("Could not find java.exe! Do you want to locate it?", "Locate Java.exe", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                //open the file dialog
                if (locateJavaDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    JAVA_PATH = locateJavaDialog.FileName;
                    return;
                }
            }

            JAVA_PATH = "null";
            button16.Enabled = false;
            button17.Enabled = false;
        }



        //Constants
        public const int CALC_THREAD_COUNT = 3;

        //==================Initialize Components==================
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //First we will figure out the java path, either from settings or input
            findJavaPath();
            //Now we need to update the system variable
            if (Properties.Settings.Default.JavaPath == "null" && JAVA_PATH != "null")
            {
                Properties.Settings.Default.JavaPath = JAVA_PATH;
                Properties.Settings.Default.Save();
            }

            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog2.InitialDirectory = Application.StartupPath;
            openFileDialog3.InitialDirectory = Application.StartupPath;
            externalEvalClusterFD.InitialDirectory = Application.StartupPath;
            externalEvalLabelFD.InitialDirectory = Application.StartupPath;

            openFileDialog2.Filter = "Graph Files (*.graph)|*.graph";
            openFileDialog3.Filter = "Cluster Files (*.cluster)|*.cluster";
            externalEvalClusterFD.Filter = "Cluster Files (*.cluster)|*.cluster";

            comboBox4.SelectedIndex = 0;
            embeddingComboBox.SelectedIndex = 0;
            DistanceMetricSelect.SelectedIndex = 0;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            formLoaded = true;
        }
        //==================Initialize Components==================

        //GenGraphBrowse
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pointSetFile = openFileDialog1.FileName;
                textBox4.Text = openFileDialog1.FileName;
                textBox3.Text = openFileDialog1.FileName;
                textBox2.Text = openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;

                pointSetFileShort = openFileDialog1.SafeFileName;
                
                //Disable all of the elements
                button2.Enabled = false;
                DistanceMetricSelect.Enabled = false;
                trackBar1.Enabled = false;
                trackBar2.Enabled = false;
                embeddingComboBox.Enabled = false;
                
                callCalcDist();

                //Renable the elements
                button2.Enabled = true;
                DistanceMetricSelect.Enabled = true;
                embeddingComboBox.Enabled = true;
                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
            }

        }


        #region "Calculate/Update Distances Graph Gen"
        private void embeddingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            callCalcDist();
        }

        private void DistanceMetricSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            callCalcDist();
        }

        private void callCalcDist()
        {
            if (!formLoaded)
                return;

            trackBar2.Enabled = true;
            numericUpDown1.Enabled = true;

            KPoint.DistType distType = KPoint.DistType.Euclidean;
            if (DistanceMetricSelect.SelectedIndex == 1)
                distType = KPoint.DistType.PMCC;
            else if (DistanceMetricSelect.SelectedIndex == 2)
                distType = KPoint.DistType.Manhattan;
            else if (DistanceMetricSelect.SelectedIndex == 3)
                distType = KPoint.DistType.Chebyshev;

            if (embeddingComboBox.SelectedIndex == 0)
                CalculateMinConnectivityGeoGraph(distType);
            else if (embeddingComboBox.SelectedIndex == 1)
                calculateNearestNeighborGraph(distType);
            else //stacked MSTs
                calculateSMST(distType);
        }
        
        
        private void calculateSMST(KPoint.DistType distType)
        {
            points = new PointSet(pointSetFile);
            graphPrefix = pointSetFileShort.Substring(0, pointSetFileShort.IndexOf('.')) + "_" + distType.ToString() + "_MST_";
            int max = points.Count / 2;
            trackBar1.Minimum = 1;
            trackBar1.Maximum = max;
            trackBar1.Value = 1;
            trackBar2.Minimum = 1;
            trackBar2.Maximum = max;
            trackBar2.Value = 1;

            trackBar2.Enabled = false;
            numericUpDown1.Enabled = false;

            label1.Text = "";
            label4.Text = "";
            distMax.Text = "";
            distMin.Text = "Number of MSTs: 1";
        }

        //this calculates the distance list
        //And works 
        private void calculateNearestNeighborGraph(KPoint.DistType distType)
        {
            points = new PointSet(pointSetFile);

            graphPrefix = pointSetFileShort.Substring(0, pointSetFileShort.IndexOf('.')) + "_" + distType.ToString() + "_KNN_";

            //Now we set the Distance
            //Dista nce matrix
            distMatrix = points.GetDistanceMatrix(distType);
            distances = distMatrix.GetSortedDistanceList();


            minConnectIndex = LightWeightGraph.BinSearchKNNMinConnectivity(2, points.Count - 1, points.Count, distMatrix);

            label1.Text = String.Format("Minimum Connectivity:({0} Neighbors)", minConnectIndex);

            float sum = distances.Cast<float>().Sum();
            sum /= distances.Count;
            label4.Text = "Mean Dist:" + sum;

            //Set the track bars
            trackBar1.Minimum = 2;
            trackBar1.Maximum = minConnectIndex;
            trackBar2.Minimum = minConnectIndex;
            trackBar2.Maximum = points.Count-1;

            trackBar1.Value = trackBar2.Value = minConnectIndex;
            distMin.Text = String.Format("Min:({0} Neighbors)", trackBar1.Value);
            distMax.Text = String.Format("Max:({0} Neighbors)", trackBar2.Value);
        }

        //this calculates the distance list
        private void CalculateMinConnectivityGeoGraph(KPoint.DistType distType)
        {
            points = new PointSet(pointSetFile);

            graphPrefix = pointSetFileShort.Substring(0, pointSetFileShort.IndexOf('.')) + "_" + distType.ToString() + "_";

            //Now we set the Distance
            //Distance matrix
            distMatrix = points.GetDistanceMatrix(distType);
            distances = distMatrix.GetSortedDistanceList();

            //Find minimum Connectivity (can make this binary search)
            int pointCount = points.Count;

            minConnectIndex = LightWeightGraph.BinSearchGeoMinConnectivity(0, distances.Count - 1, pointCount, distMatrix, distances);

            label1.Text = String.Format("Minimum Connectivity:({0})={1}", minConnectIndex, distances[minConnectIndex]);

            float sum = 0;
            foreach (float dist in distances)
                sum += dist;
            sum /= distances.Count;
            label4.Text = "Mean Dist:" + sum;

            //Set the track bars
            trackBar1.Minimum = 0;
            trackBar1.Maximum = minConnectIndex;
            trackBar2.Minimum = minConnectIndex;
            trackBar2.Maximum = distances.Count-1;

            trackBar1.Value = trackBar2.Value = minConnectIndex;
            distMin.Text = String.Format("Min:({0}) {1}", trackBar1.Value, distances[trackBar1.Value]);
            distMax.Text = String.Format("Max:({0}) {1}", trackBar2.Value, distances[trackBar2.Value]);
        }

        #endregion

        //This Generates Graphs when clicked
        private void button2_Click(object sender, EventArgs e)
        {
            int numGraphs = ((int)trackBar2.Value - (int)trackBar1.Value) / (int)numericUpDown1.Value;

            //sanity check on gui
            if (numGraphs > 20 && MessageBox.Show("Are you sure you want to generate " + numGraphs + " different graphs?", "Generate Graphs", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                return;

            for (int i = trackBar1.Value; i <= trackBar2.Value; i+= (int)numericUpDown1.Value)
            {
                LightWeightGraph lwg = null;
                if (embeddingComboBox.SelectedIndex == 0)
                    lwg =  LightWeightGraph.GetGeometricGraph(distMatrix, distances[i]);
                else if (embeddingComboBox.SelectedIndex == 1)
                    lwg =  LightWeightGraph.GetKNNGraph(distMatrix, i);
                else
                    lwg = LightWeightGraph.GetStackedMST(distMatrix, (int)trackBar1.Value);

                //Save GML
                String folder = pointSetFile.Substring(0, pointSetFile.LastIndexOf('\\'));
                lwg.SaveGML(folder + "\\" + graphPrefix + i + ".gml");

                openFileDialog2.InitialDirectory = folder;

                //Save Graph format
                lwg.SaveGraph(folder + "\\" + graphPrefix + i + ".graph");
            }

            MessageBox.Show("Graphs have been Generated!");
        }

        /// <summary>
        /// Called when the Minimum scroll bar is changed on the graph generation tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (distances != null && distances.Count > 0)
            {
                if (embeddingComboBox.SelectedIndex == 0)
                    distMin.Text = String.Format("Min:({0}) {1}", trackBar1.Value, distances[trackBar1.Value]);
                else if (embeddingComboBox.SelectedIndex == 1)
                    distMin.Text = String.Format("Min:({0} Neighbors)", trackBar1.Value);
                else
                {
                    distMin.Text = String.Format("Number of MSTs: {0}", trackBar1.Value);
                    trackBar2.Value = trackBar1.Value;
                }
            }
        }

        /// <summary>
        /// Called when the Maximum scroll bar is changed on the graph generation tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (distances != null && distances.Count > 0)
            {
                if (embeddingComboBox.SelectedIndex == 0)
                    distMax.Text = String.Format("Max:({0}) {1}", trackBar2.Value, distances[trackBar2.Value]);
                else
                    distMax.Text = String.Format("Max:({0} Neighbors) ", trackBar2.Value);
            }
        }


        
        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox4.Text = openFileDialog1.FileName;
                textBox3.Text = openFileDialog1.FileName;
                textBox2.Text = openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;
                pointSetFile = textBox2.Text;
                pointSetFileShort = openFileDialog1.SafeFileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                listBox1.Items.Clear();

                foreach (String file in openFileDialog2.SafeFileNames)
                    listBox1.Items.Add(file);
            }
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabControl2.SelectedIndex = selectedTab;
        }

        int selectedTab = 0;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                case 1:
                    selectedTab = 0;
                    break;
                case 2:
                    selectedTab = 1;
                    break;
                case 3:
                    selectedTab = 2;
                    break;
                case 4:
                    selectedTab = 3;
                    break;
                case 5:
                    selectedTab = 4;
                    break;
            }
            tabControl2.SelectedIndex = selectedTab;
        }

        //This will Execute GV or VAT using C++ executable
        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0 && listBox1.Items.Count > 0)
            {
                bool useVat = false;
                bool useGN = false;
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        useVat = true; break;
                    case 1:
                        useGN = true; break;
                }

                if (useVat || useGN)
                {
                    //Generate our String
                    //-g minDistFaceGraph_geometric FaceData.txt -u
                    argSet = new List<Tuple<string, string>>();
                    foreach (String fn in openFileDialog2.FileNames)
                    {
                        String dir = fn.Substring(0, fn.LastIndexOf('\\'));
                        String shortName = fn.Substring(fn.LastIndexOf('\\') + 1, fn.LastIndexOf('.') - fn.LastIndexOf('\\') - 1);
                        String args = String.Format("-{0} {1} {2} -w", (useVat) ? "v" : "g", shortName, pointSetFileShort);

                        argSet.Add(new Tuple<string, string>(dir, args));
                    }


                    Thread t = new Thread(StartAppCalls);
                    t.Start();

                }
            }
            else if (textBox2.TextLength == 0)
                MessageBox.Show("Please select the point file used to generate the graphs", "Select Point File");
            else if (listBox1.Items.Count == 0)
                MessageBox.Show("Please select some graphs to cluster", "Select Graph(s)");
        }

        public static ManualResetEvent doneEvent;
        public static int threadCount = 0;
        public void StartAppCalls()
        {
            doneEvent = new ManualResetEvent(false);
            AppThreadWrapper[] appCalls = new AppThreadWrapper[argSet.Count];
            ThreadPool.SetMaxThreads(CALC_THREAD_COUNT, CALC_THREAD_COUNT);

            int i = 0;
            foreach (var a in argSet)
            {
                appCalls[i] = new AppThreadWrapper(a.Item1, a.Item2);
                Interlocked.Increment(ref threadCount);
                ThreadPool.QueueUserWorkItem(appCalls[i].ThreadPoolCallback, i);
                i++;
            }

            doneEvent.WaitOne();

            MessageBox.Show("All Calculations Are Done!");
        }


        List<Tuple<String, String>> argSet;
        public class AppThreadWrapper
        {
            string dir;
            string args;
            
            public AppThreadWrapper(string d, string a)
            {
                dir = d; args = a;
            }

            public void ThreadPoolCallback(Object threadContext)
            {
                try
                {
                    //here we start the mojo
                    Process p = new Process();

                    p.StartInfo.FileName = Application.StartupPath + "\\" + "VAT_BetweenessCentrality.exe";
                    p.StartInfo.WorkingDirectory = dir;
                    p.StartInfo.Arguments = args;
                    p.Start();
                    Thread.Sleep(100);
                    while (!p.HasExited)
                    {
                        Thread.Sleep(100);
                    }
                }
                finally
                {
                    if (Interlocked.Decrement(ref Form1.threadCount) == 0)
                    {
                        doneEvent.Set();
                    }
                }
                
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (openFileDialog3.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                listBox2.Items.Clear();
                foreach (String f in openFileDialog3.SafeFileNames)
                    listBox2.Items.Add(f);

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count > 0)
            {
                List<String> names = new List<string>(openFileDialog3.FileNames);
                Thread t = new Thread(() => doEvaluateCluster(names));
                t.Start();
                MessageBox.Show("Evaluating Clusters, this could take a while.  A window will appear when done", "Evaluate Clusters");
            }
        }

        public delegate void OpenForm(Form f);
        public void invokeOpenDisplayForm(Form f)
        {
            f.Show();
        }

        public void doEvaluateCluster(List<String> fileNames)
        {
            DataDisplayForm f2 = new DataDisplayForm();
            f2.initDisplay(openFileDialog3.FileNames);
            this.Invoke(new OpenForm(invokeOpenDisplayForm), f2);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox4.Text = openFileDialog1.FileName;
                textBox3.Text = openFileDialog1.FileName;
                textBox2.Text = openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;
                pointSetFile = textBox3.Text;
                pointSetFileShort = openFileDialog1.SafeFileName;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length > 0)
            {
                Thread t = new Thread(doKmeansThreaded);
                t.Start();
                MessageBox.Show("Running, will prompt when complete");
            }
            else
            {
                MessageBox.Show("Please select a point file to cluster", "Select Point File");
            }
        }

        private void doKmeansThreaded()
        {
            int numRuns = (int)numericUpDown2.Value;
            int numClustMin = (int)numClusters.Value;
            int numClustMax = (int)numClustersMax.Value;
            String folder = pointSetFile.Substring(0, pointSetFile.LastIndexOf('\\'));
            openFileDialog2.InitialDirectory = folder;


            PointSet points = new PointSet(pointSetFile);

            for (int i = 0; i < numRuns; i++)
            {
                for (int j = numClustMin; j <= numClustMax; j++)
                {
                    KMeans algo = new KMeans(points, j);
                    String newCluster = folder + "/" + pointSetFileShort.Substring(0,pointSetFileShort.LastIndexOf('.')) + "_kmeans" + j + "_" + i;
                    algo.GetPartition().SavePartition(newCluster, pointSetFile.Substring(pointSetFile.LastIndexOf("\\") + 1));
                }
            }
            MessageBox.Show("Complete!");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox4.Text = openFileDialog1.FileName;
                textBox3.Text = openFileDialog1.FileName;
                textBox2.Text = openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;
                pointSetFile = textBox4.Text;
                pointSetFileShort = openFileDialog1.SafeFileName;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            
            Thread t = new Thread(doSOMThreaded);
            t.Start();
            MessageBox.Show("Running, will prompt when complete");

        }

        private void doSOMThreaded()
        {
            int numRuns = (int)numericUpDown4.Value;
            int numClust = (int)numericUpDown3.Value;
            int numEpochs = (int)numericUpDown5.Value;
            double learningRate = (double)numericUpDown6.Value;

            String folder = pointSetFile.Substring(0, pointSetFile.LastIndexOf('\\'));
            openFileDialog2.InitialDirectory = folder;

            PointSet somPoints = new PointSet(pointSetFile);

            List<int> dimensions = new List<int>();
            dimensions.Add(numClust); // 1 dimensional "String"
            for (int i = 0; i < numRuns; i++)
            {

                SelfOrganizingMap algo = new SelfOrganizingMap(somPoints, dimensions, numEpochs, learningRate);
                //while (!algo.doneExecuting())
                //    algo.Epoch();
                algo.runLargeEpochs(0.5, 2);
                algo.runLargeEpochs(0.15, 1);
                algo.runLargeEpochs(0.05, 3);

                String newCluster = folder + "/" + pointSetFileShort.Substring(0,pointSetFileShort.LastIndexOf('.')) + "_som" + numClust + "_" + i;
                algo.GetClusterLazy().SavePartition(newCluster, pointSetFile.Substring(pointSetFile.LastIndexOf("\\") + 1));
            }
            MessageBox.Show("Complete!");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            
            Thread t = new Thread(doSOMMapThreaded);
            t.Start();
            MessageBox.Show("Running, will prompt when complete");

        }

        private void doSOMMapThreaded()
        {
            int w = (int)somWidth.Value;
            int h = (int)somHeight.Value;
            List<int> dim = new List<int>();
            dim.Add(w); dim.Add(h);
            int numEpochs = (int)numericUpDown5.Value;
            double learningRate = (double)numericUpDown6.Value;

            String folder = pointSetFile.Substring(0, pointSetFile.LastIndexOf('\\'));
            System.IO.Directory.CreateDirectory(folder);
            openFileDialog2.InitialDirectory = folder;

            String copiedFileName = folder + "/" + pointSetFileShort;
            if (!File.Exists(copiedFileName))
                File.Copy(pointSetFile, copiedFileName);

            PointSet somPoints = new PointSet(pointSetFile);

            HexagonalSelfOrganizingMap algo = new HexagonalSelfOrganizingMap(somPoints, w, learningRate);
            algo.runLargeEpochs(0.2, 1);
            algo.runLargeEpochs(0.05, 2);
            algo.runLargeEpochs(0.01, 2);

            String newCluster = folder + "/" + pointSetFileShort + "_som" + w + "x" + h + "_map_Epoch" + numEpochs + "_";

            var bmp = algo.GetUMatrix(10);
            bmp[0].Save(newCluster + ".bmp");
            bmp[1].Save(newCluster + "count" + ".bmp");

            MessageBox.Show("Complete!");
        }

        //This routine will normalize data
        private void button14_Click(object sender, EventArgs e)
        {
            //This code gets the normalization type from the dropdown
            KPoint.NormType norm = KPoint.NormType.None;
            if (comboBox4.SelectedIndex == 0)
                norm = KPoint.NormType.GeometricMean;
            else if (comboBox4.SelectedIndex == 1)
                norm = KPoint.NormType.ArithmeticMean;
            else if (comboBox4.SelectedIndex == 2)
                norm = KPoint.NormType.Max;
            else if (comboBox4.SelectedIndex == 3)
                norm = KPoint.NormType.MaxAttr;
            else if (comboBox4.SelectedIndex == 4)
                norm = KPoint.NormType.None;



            //This loads a list of datapoints and normalized is
            var normPoints = new PointSet(pointSetFile);

            List<int> featureSet = new List<int>();
            for (int i = 0; i < normPoints.Dimensions; i++)
            {
                if (checkedListBox1.GetItemCheckState(i) == CheckState.Checked)
                    featureSet.Add(i);
            }

            normPoints = normPoints.GetReducedAttributeSet(featureSet);
            normPoints.NormalizeDataSet(norm);

            //This will create a sub directory to place the file in
            String folder = pointSetFileShort.Substring(0, pointSetFileShort.IndexOf('.'));
            System.IO.Directory.CreateDirectory(folder);
            openFileDialog2.InitialDirectory = folder;

            //now we write the values
            String normFileName = folder + "/" + folder + "_" + norm.ToString() + featureSet.Count + "features" + ".txt";
            normPoints.Save(normFileName);

            MessageBox.Show("Normalized File Generated! " + normFileName);
        }

        //This is the point file prompt button for the normalization tab
        private void button15_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pointSetFile = openFileDialog1.FileName;
                textBox5.Text = openFileDialog1.FileName;
                pointSetFileShort = openFileDialog1.SafeFileName;
                button14.Enabled = true;

                checkedListBox1.Items.Clear();
                
                var pointData = new PointSet(pointSetFile);
                int attributeCount = pointData.Dimensions;
                for (int i = 1; i <= attributeCount; i++)
                {
                    checkedListBox1.Items.Add("f" + i);
                    if (checkAll.Checked == true)
                        checkedListBox1.SetItemChecked(i - 1, true);
                }
            }
        }


        //This Will set the trackbars on the CAST tab
        private void button16_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                trackBar3.Enabled = false;
                trackBar4.Enabled = false;
                button17.Enabled = false;

                textBox6.Text = openFileDialog1.FileName;
                textBox4.Text = openFileDialog1.FileName;
                textBox3.Text = openFileDialog1.FileName;
                textBox2.Text = openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;
                pointSetFile = textBox6.Text;
                pointSetFileShort = openFileDialog1.SafeFileName;

                points = new PointSet(pointSetFile);

                //Now we set the Distance
                //Distance matrix
                distMatrix = points.GetDistanceMatrix();
                distances = distMatrix.GetSortedDistanceList();

                //Find minimum Connectivity (can make this binary search)
                minConnectIndex = LightWeightGraph.BinSearchGeoMinConnectivity(0, distances.Count - 1, distMatrix.Count, distMatrix, distances);

                label25.Text = String.Format("Minimum Connectivity:({0})={1}", minConnectIndex, distances[minConnectIndex]);

                float sum = distances.Sum();
                sum /= distances.Count;
                label22.Text = "Mean Dist:" + sum;

                //Set the track bars
                trackBar4.Minimum = 0;
                trackBar4.Maximum = minConnectIndex;
                trackBar3.Minimum = minConnectIndex;
                trackBar3.Maximum = distances.Count - 1;

                trackBar4.Value = trackBar3.Value = minConnectIndex;
                label24.Text = String.Format("Min:({0}) {1}", trackBar4.Value, distances[trackBar4.Value]);
                label23.Text = String.Format("Max:({0}) {1}", trackBar3.Value, distances[trackBar3.Value]);

                trackBar3.Enabled = true;
                trackBar4.Enabled = true;
                button17.Enabled = true;
            }
        }

        //CAST Min Threshold
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            if (distances != null && distances.Count > 0)
            {
                label24.Text = String.Format("Min:({0}) {1}", trackBar4.Value, distances[trackBar4.Value]);
            }
        }

        //CAST Max threshold
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            if (distances != null && distances.Count > 0)
            {
                label23.Text = String.Format("Max:({0}) {1}", trackBar3.Value, distances[trackBar3.Value]);
            }
        }

        //Run CAST
        private void button17_Click(object sender, EventArgs e)
        {
            if (textBox6.Text.Length > 0)
            {
                int numGraphs = ((int)trackBar4.Value - (int)trackBar3.Value) / (int)numericUpDown7.Value;

                //sanity check on gui
                if (numGraphs > 20 && MessageBox.Show("Are you sure you want to generate " + numGraphs + " different graphs?", "Generate Graphs", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    return;


                CASTargSet = new List<Tuple<string, string>>();

                for (int i = (int)trackBar4.Value; i <= (int)trackBar3.Value; i += (int)numericUpDown7.Value)
                {
                    double val = distances[i];
                    String dir = textBox6.Text.Substring(0, textBox6.Text.LastIndexOf('\\'));
                    String shortName = textBox6.Text.Substring(textBox6.Text.LastIndexOf('\\') + 1, textBox6.Text.LastIndexOf('.') - textBox6.Text.LastIndexOf('\\') - 1);
                    String args = String.Format("{0} {1} {2}", pointSetFileShort, shortName + "_" + i + "_CAST.cluster", val);
                    CASTargSet.Add(new Tuple<string, string>(dir, args));
                }


                Thread t = new Thread(CASTStartAppCalls);
                t.Start();
            }
        }

        //Cast Threading Environment
        public static ManualResetEvent CASTdoneEvent;
        public static int CASTthreadCount = 0;
        public void CASTStartAppCalls()
        {
            CASTdoneEvent = new ManualResetEvent(false);
            CASTAppThreadWrapper[] appCalls = new CASTAppThreadWrapper[CASTargSet.Count];
            ThreadPool.SetMaxThreads(CALC_THREAD_COUNT, CALC_THREAD_COUNT);

            int i = 0;
            foreach (var a in CASTargSet)
            {
                appCalls[i] = new CASTAppThreadWrapper(a.Item1, a.Item2);
                Interlocked.Increment(ref CASTthreadCount);
                ThreadPool.QueueUserWorkItem(appCalls[i].ThreadPoolCallback, i);
                i++;
            }

            CASTdoneEvent.WaitOne();

            MessageBox.Show("All Calculations Are Done!");
        }


        List<Tuple<String, String>> CASTargSet;
        public class CASTAppThreadWrapper
        {
            string dir;
            string args;

            public CASTAppThreadWrapper(string d, string a)
            {
                dir = d; args = a;
            }

            public void ThreadPoolCallback(Object threadContext)
            {
                try
                {
                    //here we start the mojo
                    Process p = new Process();

                    p.StartInfo.FileName = JAVA_PATH;
                    p.StartInfo.WorkingDirectory = dir;
                    p.StartInfo.Arguments = "-jar ../CAST.jar " + args;
                    p.Start();
                    Thread.Sleep(100);
                    while (!p.HasExited)
                    {
                        Thread.Sleep(100);
                    }
                }
                finally
                {
                    if (Interlocked.Decrement(ref Form1.CASTthreadCount) == 0)
                    {
                        CASTdoneEvent.Set();
                    }
                }

            }

        }

        private void numClusters_ValueChanged(object sender, EventArgs e)
        {
            //Do some sanity checking
            if (numClustersMax.Value < numClusters.Value)
            {
                numClustersMax.Value = numClusters.Value;
            }
        }

        private void numClustersMax_ValueChanged(object sender, EventArgs e)
        {
            //Do some sanity checking
            if (numClustersMax.Value < numClusters.Value)
            {
                numClusters.Value = numClustersMax.Value;
            }
        }

        private void checkAll_CheckStateChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++) 
                checkedListBox1.SetItemChecked(i, checkAll.Checked);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox7.Text = openFileDialog1.FileName;
                pointSetFile = textBox7.Text;
                pointSetFileShort = openFileDialog1.SafeFileName;

                points = new PointSet(pointSetFile);
            }
        }

        //do heirarchical vat
        private void button18_Click(object sender, EventArgs e)
        {
            int clusterCount = (int)numericUpDown8.Value;
            int knnOffset = (int) hVatKNNOffset.Value;
            String pointFileName = textBox7.Text;
            PointSet hVatPoints = new PointSet(pointFileName);

            float vatAlpha = (float) hVATAlphaUpDown.Value;
            float vatBeta = (float)hVATBetaUpDown.Value;

            bool useKNN = comboBox3.SelectedIndex == 0;

            //Add hillClimbing
            //Add more heuristics?
            HVATClust hVat = new HVATClust(hVatPoints, clusterCount, hvatUseWeights.Checked, useKNN, knnOffset, vatAlpha, vatBeta);


            MessageBox.Show("Starting! Click Ok to continue");
            //Use a closure and join to get a return value
            Partition p = null;
            Thread th = new Thread(() => { p = hVat.GetPartition(); });
            th.Start();
            th.Join();
            MessageBox.Show("Complete!");

            //Save the file
            String clusterFileName = pointFileName.GetShortFilename().GetFilenameNoExtension();
            p.SavePartition(clusterFileName + "_HVAT", pointFileName);
        }

        //This selects the Cluster file we want to externally validate
        private void externalEvalBrowse_Click(object sender, EventArgs e)
        {
            if (externalEvalClusterFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                externalEvalClusterText.Text = externalEvalClusterFD.FileName;
            }
        }

        //This selects the label file we use for external validation
        private void externalEvalLabelBrowse_Click(object sender, EventArgs e)
        {
            if (externalEvalLabelFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                externalEvalLabelText.Text = externalEvalLabelFD.FileName;
            }
        }

        //This will calculate the errors
        //first it must ask what column to use as a label
        private void externalEvalCalculateButton_Click(object sender, EventArgs e)
        {
            //start by parsing label file
            DelimitedFile delimitedLabelFile = new DelimitedFile(externalEvalLabelText.Text);
            int labelCol = Prompt.ShowDialog("Enter the Column to use", "Select Attribute", 1, delimitedLabelFile.Data[0].Length);
            LabelList labels = new LabelList(delimitedLabelFile.GetColumn(labelCol-1));

            //get the Partion file
            Partition clusterFile = new Partition(externalEvalClusterText.Text);
            int countOfPoints = clusterFile.DataCount;
            
            //create a count mapping
            //[actual cluster label, number in found clusters]
            int[,] clusterMatching = new int[labels.UniqueLabels.Count, clusterFile.Clusters.Count];
            foreach (Cluster c in clusterFile.Clusters)
            {
                foreach (ClusteredItem k in c.Points)
                {
                    int actualMatching = labels.LabelIndices[k.Id];
                    int foundMatching = k.ClusterId;
                    clusterMatching[actualMatching, foundMatching]++;
                }
            }

            //One-To-One Mapping like Darla's
            String greedyError = ExternalEval.GreedyErrorEval(clusterFile, labels, clusterMatching);
            externalEvalResultText.Text = greedyError;

        }

        private void hvatHillclimb_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void smoothManifoldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void smoothManifoldToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SmoothManifoldForm smf = new SmoothManifoldForm();
            smf.Show();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void pCAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PCAForm pf = new PCAForm();
            pf.Show();
        }
        private void button19_Click_1(object sender, EventArgs e)
        {
            int somDim = (int) somWidth.Value;
            NetMining.Data.PointSet data = new PointSet(textBox4.Text);
            HexagonalSelfOrganizingMap hSOM = new HexagonalSelfOrganizingMap(data, somDim, 0.3);

            hSOM.runLargeEpochs(0.2, 1);
            hSOM.runLargeEpochs(0.05, 2);
            hSOM.runLargeEpochs(0.01, 2);
            //hSOM.runLargeEpochs(0.01, 4);
            //hSOM.runLargeEpochs(0.005, 6);

            //Setup out labels
            DelimitedFile f = new DelimitedFile(SOMLabelTextbox.Text);
            int labelIndex = Prompt.ShowDialog("Select Label Index", "Label File", 1, f.Data[0].Length)-1;
            String[] labels = f.GetColumn(labelIndex);
            //Now build our array of indicies
            List< String> labelNames = new List<string>();
            int[] labelIndexArr = new int[data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                if (!labelNames.Contains(labels[i]))
                {
                    labelNames.Add(labels[i]);
                }
                labelIndexArr[i] = labelNames.IndexOf(labels[i]);
            }

            var bmp = hSOM.GetUMatrix(10, labelNames.Count, labelIndexArr);
            bmp[0].Save("test" + somDim.ToString() + ".bmp");
            bmp[1].Save("count" + somDim.ToString() + ".bmp");
            for (int i = 2; i < bmp.Count; i++)
            {
                bmp[i].Save("count" + somDim.ToString() + "class_" + (i - 1).ToString() + ".bmp");
            }
            MessageBox.Show("Done!");
        }

        private void selectLabelSOM_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SOMLabelTextbox.Text = openFileDialog1.FileName;
            }
        }
    }
}
