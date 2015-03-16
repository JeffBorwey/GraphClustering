using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NetMining.ClusteringAlgo;
using NetMining.ExtensionMethods;
using NetMining.Evaluation;
namespace GraphClustering
{
    public partial class DataDisplayForm : Form
    {
        public DataDisplayForm()
        {
            InitializeComponent();
        }

        public void initDisplay(String[] fileNames)
        {
            foreach (String fn in fileNames)
            {
                String safeFileName = fn.GetShortFilename().GetFilenameNoExtension();

                Partition clusters = new Partition(fn);
                dataGridView1.Rows.Add(safeFileName, InternalEval.avgDunnIndex(clusters),
                InternalEval.AverageSilhouetteIndex(clusters), InternalEval.DaviesBouldinIndex(clusters),
                InternalEval.getSqrdErrorDistortion(clusters));
            }
        }

        public void initDisplay(List<String> filenames, double[,] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                String fn = filenames[i];
                String safeFileName = fn.Substring(fn.LastIndexOf('\\') + 1).Replace(".cluster", "");
                dataGridView1.Rows.Add(filenames, values[i, 0], values[i, 1], values[i, 2], values[i, 3]);
            }
        }

        private void DataDisplay_Load(object sender, EventArgs e)
        {

        }
    }
}
