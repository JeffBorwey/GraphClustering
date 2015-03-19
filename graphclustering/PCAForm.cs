using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NetMining.Data;
using NetMining.Data.DimensionalityReduction;

namespace GraphClustering
{
    public partial class PCAForm : Form
    {
        private PCA _pca;
        public PCAForm()
        {
            InitializeComponent();
        }

        private void PCAForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //set file path
                textBox1.Text = openFileDialog1.FileName;
                PointSet p = new PointSet(textBox1.Text);
                _pca = new PCA(p);

                //Cleanup
                chart1.Series[0].Points.Clear();
                chart1.ChartAreas[0].AxisY.Maximum = 1.0;
                chart1.ChartAreas[0].AxisY.Minimum = 0.0;
                chart1.Series[1].Points.Clear();

                //Fill out the chart
                for (int i = 0; i < _pca.Contributions.Length; i++)
                {
                    chart1.Series[0].Points.AddY(_pca.Contributions[i]);
                    chart1.Series[1].Points.AddY(_pca.RunningTotalArr[i]);
                }

                numericUpDown1.Minimum = 1;
                numericUpDown1.Maximum = p.Dimensions;
                numericUpDown1.Value = 1;
                textBox2.Text = (100*_pca.RunningTotalArr[0]).ToString()+"%";
                numericUpDown1.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String saveFile = saveFileDialog1.FileName;
                int numberOfEVec = (int)numericUpDown1.Value;

                PointSet p = _pca.GetPCAProjection(numberOfEVec);
                p.Save(saveFile);
               
                MessageBox.Show("File saved to\n" + saveFile);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            textBox2.Text = (100 * _pca.RunningTotalArr[(int)numericUpDown1.Value-1]).ToString() + "%";
        }
    }
}
