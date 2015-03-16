using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphClustering
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //LightWeightGraph lwg = Form1.getGraphFromFile("iris/iris.txt0.graph");

            //LightWeightGraph lwg = Form1.getGraphFromFile("Swiss\\swiss.graph");
            //lwg.isWeighted = false;
            //lwg.saveGML("iris.gml");
            //get betweenss
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //ClusterAlgo.VAT v = new ClusterAlgo.VAT(lwg);
            //sw.Stop();
            //MessageBox.Show(sw.ElapsedMilliseconds.ToString());
            //return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 f = new Form1();



            Application.Run(f);
        }
    }
}
