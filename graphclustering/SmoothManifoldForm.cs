using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;

namespace GraphClustering
{
    public partial class SmoothManifoldForm : Form
    {
        Bitmap selectedFile;
        String fileName;
        List<Color> colors = null;
        List<BoundBox> colorBounds = null;
        private static Random rng = new Random();
        private static ScriptHelp sHelp = new ScriptHelp();
        int[,] colorIndex;


        public SmoothManifoldForm()
        {
            InitializeComponent();
            openBMPDialog.Filter = "Bitmap File (*.bmp)|*.bmp";
        }

        private void SmoothManifoldForm_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private int getColorIndex(Color c)
        {
            if (colors == null)
                return -1;
            for (int i = 0; i < colors.Count; i++)
            {
                Color cl = colors[i];
                if (cl.R == c.R && cl.G == c.G && cl.B == c.B)
                    return i;
            }
            return -1;
        }

        private int addSetColor(Color c, int x, int y)
        {
            if (getColorIndex(c) == -1)
            {
                colors.Add(c);
                colorBounds.Add(new BoundBox(selectedFile.Width, selectedFile.Height));
            }
            int index = getColorIndex(c);
            colorIndex[x, y] = index;
            return index;
        }

        //add the colors to the listview
        private void addColorsToListView()
        {
            listView1.Clear();
            for (int c = 0; c < colors.Count; c++)
            {
                String text = "Color" + (c + 1).ToString();
                Color co = colors[c];
                ListViewItem lvi = new ListViewItem();
                lvi.BackColor = co;
                lvi.Text = text;
                lvi.Font = new Font(lvi.Font.FontFamily, 16);
                
                if (co.GetBrightness() > 0.5)
                    lvi.ForeColor = Color.Black;
                else
                    lvi.ForeColor = Color.White;

                listView1.Items.Add(lvi);
            }

            listView1.Items[0].Selected = true;
        }

        private void selectBMPButton_Click(object sender, EventArgs e)
        {
            if (openBMPDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openBMPDialog.FileName;
                fileName = openBMPDialog.FileName;

                //Open the image file
                selectedFile = new Bitmap(fileName);
                colorIndex = new int[selectedFile.Width, selectedFile.Height];
                colors = new List<Color>();
                colorBounds = new List<BoundBox>();

                //=========find the boundaries of each item============
                Rectangle img = new Rectangle(0, 0, selectedFile.Width, selectedFile.Height);
                BitmapData bmd = selectedFile.LockBits(img, ImageLockMode.ReadOnly, selectedFile.PixelFormat);
                
                //Check to make sure we have the right format of image
                int PixelSize = -1; //ARGB
                if (selectedFile.PixelFormat == PixelFormat.Format24bppRgb)
                    PixelSize = 3;
                else if (selectedFile.PixelFormat == PixelFormat.Format32bppArgb)
                    PixelSize = 4;

                if (PixelSize == -1)
                {
                    MessageBox.Show(String.Format("Invalid Bitmap Format: {0}, please provide a 24bit RBG or 32bit ARGB file", selectedFile.PixelFormat.ToString()));
                    return;
                }

                //Iterate over each pixel and setup the index
                unsafe
                {
                    for (int y = 0; y < bmd.Height; y++)
                    {
                        byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);

                        for (int x = 0; x < bmd.Width; x++)
                        {
                            int B = row[x * PixelSize];   //Blue
                            int G = row[x * PixelSize + 1]; //Green
                            int R = row[x * PixelSize + 2]; //Red
                            //int A = row[x * PixelSize + 3]; // Alpha
                            Color c = Color.FromArgb(255, R, G, B);
                            int index = addSetColor(c, x, y);

                            //now we need to set the bounding Box
                            colorBounds[index].adjustValue(x, y);
                        }
                    }
                }

                //add our colors
                addColorsToListView();

                //set the image
                imagePreview.Image = Image.FromFile(fileName);

                //Enable Generate Button
                generateButton.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        class Sample2D
        {
            public double x, y;
            public int classIndex;
            public Sample2D(double _x, double _y, int index)
            {
                x = _x;
                y = _y;
                classIndex = index;
            }
        }

        class SampleND
        {
            public double[] location;
            public int classIndex;
            public SampleND(double[] location, int classIndex)
            {
                this.location = location;
                this.classIndex = classIndex;
            }
        }

        class BoundBox
        {
            public int left, top, right, bottom;
            public BoundBox(int width, int height)
            {
                left = width;
                top = height;
                right = 0;
                bottom = 0;
            }

            public void adjustValue(int x, int y)
            {
                if (x < left)
                    left = x;
                if (y < top)
                    top = y;
                if (x > right)
                    right = x;
                if (y > bottom)
                    bottom = y;
            }

            public Tuple<double, double> genPoint()
            {
                double width = (double)(right - left +1);
                double height = (double)(bottom - top +1);
                double _x = rng.NextDouble() * width + (double)left;
                double _y = rng.NextDouble() * height + (double)top;
                return new Tuple<double, double>(_x, _y);
            }
        }

        //This determines if a point is within a class
        private Boolean isInColor(double x, double y, int classID)
        {
            return getColorIndex(x,y) == classID;
        }

        private int getColorIndex(double x, double y)
        {
            int _x = (int)(x + 0.5);
            int _y = (int)(y + 0.5);
            return colorIndex[_x, _y];
        }

        private List<Sample2D> samplePoints;
        private void generate_Click(object sender, EventArgs e)
        {
            //get total samples
            int totalSamples = (int)numericUpDown1.Value;

            //First get our background color
            Color c = listView1.SelectedItems[0].BackColor;
            int indexBackground = getColorIndex(c);

            samplePoints = new List<Sample2D>();

            //now we need to find out our sampling technique
            if (comboBox1.SelectedIndex == 0) //Samples per Color
            {
                for (int i = 0; i < colors.Count; i++)
                {
                    if (i != indexBackground)
                    {
                        int countSamples = 0;

                        BoundBox bb = colorBounds[i];
                        while (countSamples < totalSamples)
                        {
                            //Generate a random Point
                            var point = bb.genPoint();
                            if (isInColor(point.Item1, point.Item2, i))
                            {
                                samplePoints.Add(new Sample2D(point.Item1, point.Item2, i));
                                countSamples++;
                            }
                        }
                    }
                }
            }
            else //Total Samples
            {
                BoundBox entireBound = new BoundBox(selectedFile.Width-1, selectedFile.Height-1);
                int countSamples = 0;
                while (countSamples < totalSamples)
                {
                    //Generate a random Point
                    var point = entireBound.genPoint();
                    int cIndex = getColorIndex(point.Item1,point.Item2);
                    if (cIndex != indexBackground)
                    {
                        samplePoints.Add(new Sample2D(point.Item1, point.Item2, cIndex));
                        countSamples++;
                    }
                }
            }

            //Now we have our Samples! Time to do something with it
            samplePreview.Image = makePointImage();

            saveButton.Enabled = true;
        }

        private Image makePointImage()
        {
            Bitmap i = new Bitmap(selectedFile.Width-1, selectedFile.Height-1);
            Graphics g = Graphics.FromImage(i);
            g.FillRectangle(Brushes.White, 0, 0, i.Width, i.Height);

            foreach (var p in samplePoints)
            {
                i.SetPixel((int)(p.x+.5), (int)(p.y+.5), colors[p.classIndex]);
            }
            
            return i;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            //get transform object
            TransformScript tfs = new TransformScript(textBox2.Text, (int)numericUpDown2.Value);
            StringBuilder pointFile = new StringBuilder();
            StringBuilder labelFile = new StringBuilder();
            try
            {
                foreach (var p in samplePoints)
                {
                    double[] tLoc = tfs.doTransform(p.x, p.y);
                    pointFile.AppendLine(String.Join("\t", tLoc));
                    labelFile.AppendLine(p.classIndex.ToString());
                }

                String fileWithoutType = fileName.Substring(0, fileName.LastIndexOf('.'));
                using (StreamWriter s1 = new StreamWriter(fileWithoutType + "_sample.data"))
                {
                    s1.Write(pointFile.ToString());
                }
                using (StreamWriter s1 = new StreamWriter(fileWithoutType + "_labels.data"))
                {
                    s1.Write(labelFile.ToString());
                }

                MessageBox.Show("Files Written to \n" + fileWithoutType + "_sample.data\n" + fileWithoutType + "_labels.data");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            sHelp.Show();
        }
    }
}
