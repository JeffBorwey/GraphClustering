using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Statistics;
using NetMining.Data;

namespace GraphClustering
{
    public partial class PCAForm : Form
    {
        public string fileName;
        Matrix<double> m;
        Evd<double> eigen;
        List<EigenPair> eigenPairs;
        double[] runningTotalArr;
        double eigenSum;
        int dim,numPoints;
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
                fileName = openFileDialog1.FileName;

                //Cleanup
                chart1.Series[0].Points.Clear();
                chart1.ChartAreas[0].AxisY.Maximum = 1.0;
                chart1.ChartAreas[0].AxisY.Minimum = 0.0;

                //Load the file
                var points = new PointSet(textBox1.Text);
                dim = points.Dimensions;
                numPoints = points.Count;

                //Build a matrix
                m = Matrix<double>.Build.Dense(numPoints, dim);
                
                //Add our values
                for (int r = 0; r < numPoints; r++)
                    for (int c = 0; c < dim; c++)
                        m[r, c] = points[r][c];

                //Now Find the standard Deviation
                //double[] mean_vec_calc = new double[dim];
                for (int d = 0; d < dim; d++)
                {
                    Tuple<double, double> meanstd = m.Column(d).MeanStandardDeviation();
                    for (int r = 0; r < numPoints; r++) 
                    {
                        m[r, d] = (m[r, d] - meanstd.Item1)/meanstd.Item2;
                        //mean_vec_calc[d] += m[r, d];
                    }
                    //mean_vec_calc[d] /= (double)points.Count;
                }

                var cov = ((m.Transpose()).Multiply(m)).Multiply(1 / ((double)numPoints - 1));
                eigen = cov.Evd();
                eigenSum = eigen.EigenValues.SumMagnitudes();
                eigenPairs = new List<EigenPair>();
                for (int d = 0; d < dim; d++)
                {
                    var eVec = eigen.EigenVectors.Column(d);
                    eigenPairs.Add(new EigenPair(eigen.EigenValues[d].Magnitude, eVec));
                }

                //now sort the list
                eigenPairs.Sort((x, y) => y.value.CompareTo(x.value));
                
                //setup the graph
                var s = chart1.Series[0];
                var runningTotal = 0.0;
                runningTotalArr = new double[dim];
                int n = 0;
                foreach (EigenPair ep in eigenPairs)
                {
                    double contribution = ep.value / eigenSum;
                    runningTotal += contribution;
                    runningTotalArr[n] = runningTotal;
                    chart1.Series[0].Points.AddY(contribution);
                    chart1.Series[1].Points.AddY(runningTotal);
                    n++;
                }

                numericUpDown1.Minimum = 1;
                numericUpDown1.Maximum = dim;
                numericUpDown1.Value = 1;
                textBox2.Text = (100*runningTotalArr[0]).ToString()+"%";
                numericUpDown1.Enabled = true;

                //fast eigenVector
                //Matrix<double> mMinus = m.Subtract(mean_vec);
                //Get the Eigenvectors
                /*
                var mT = m.Transpose();
                Svd<double> svd = mT.Svd();
                Matrix<double> eigenVectors = svd.U;
                */
            }
        }

        class EigenPair
        {
            public double value;
            public Vector<double> vector;

            public EigenPair(double va, Vector<double> ve)
            {
                value = va;
                vector = ve;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                String saveFile = saveFileDialog1.FileName;
                int numberOfEVec = (int)numericUpDown1.Value;
                Matrix<double> transformMat = Matrix<double>.Build.Dense(dim, numberOfEVec);
                for (int eVec = 0; eVec < numberOfEVec; eVec++)
                {
                    for (int d = 0; d < dim; d++)
                        transformMat[d, eVec] = eigenPairs[eVec].vector[d];
                }

                Matrix<double> transformedSpace = m.Multiply(transformMat);

                //now save this stuff
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFile))
                {
                    for (int n = 0; n < numPoints; n++)
                    {
                        sw.WriteLine(String.Join("\t", transformedSpace.Row(n)));
                    }
                }

                MessageBox.Show("File saved to\n" + saveFile);
            }
        }

        private Color getColor(int p)
        {
            if (p < 50)
                return Color.Blue;
            if (p < 100)
                return Color.Orange;
            return Color.Green;

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            textBox2.Text = (100 * runningTotalArr[(int)numericUpDown1.Value-1]).ToString() + "%";
        }
    }
}
