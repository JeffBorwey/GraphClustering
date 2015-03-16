using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using NetMining.ADT;
using NetMining.ClusteringAlgo;
using NetMining.Data;

namespace NetMining.SOM
{
    //This is a Self Organizing Map(Kohonen SOM)
    //The structure of the neurons is important
    
    public class HexagonalSelfOrganizingMap
    {
        private double radius; //This is the max radius
        private double neighborhoodRadius;

        private double initialLearningRate;
        private double learningRate;
        private double influence;
        private double timeConstant;

        private List<SOMNeuron> neurons;
        private QuadTree neuronQuadTree;

        public PointSet data;
        public double[] conversionMatrix;
        private int totalItterations;
        public int itterations;
        private Random rng;
        private int _dimension;


        public bool doneExecuting()
        {
            return itterations >= totalItterations;
        }

        public HexagonalSelfOrganizingMap(PointSet data, int dimension, double learningRate)
        {
            _dimension = dimension;

           //Get initial learning rate
            this.initialLearningRate = learningRate;
            this.learningRate = learningRate;
            influence = 0;
            itterations = 1;
            
            this.data = data;
            rng = new Random();

            int numNeurons = dimension * dimension;

            //Scale each attribute from [0,1] and store the conversion matrix
            conversionMatrix = data.GetMinMaxWeights().Max.Coordinates;

            foreach (KPoint d in data.PointList)
                d.Normalize(conversionMatrix);

            //Here We will initialize Our Neurons
            neurons = new List<SOMNeuron>();
            KPoint zero = KPoint.Zero(data[0].Dimensions);
            KPoint one = KPoint.One(data[0].Dimensions);
            double halfNeuronDelta = 1/Math.Sqrt(3.0);

            //Initialize our Quadtree for reference
            double centerX = (dimension - 0.5)*(2*halfNeuronDelta)/2.0;
            double centerY = (dimension - 1.0)/2.0;
            QuadTreePointStruct center = new QuadTreePointStruct() {Index = -1, X = centerX, Y = centerY};
            QuadTreePointStruct halfDistance = new QuadTreePointStruct() { Index = -1, X = centerX + halfNeuronDelta, Y = centerY +0.5};
            neuronQuadTree = new QuadTree(new QuadTreeBoundingBox(){Center = center, HalfLength = halfDistance});

            int neuronIndex = 0;
            for (int r = 0; r < dimension; r++)
            {
                double xPos = (r%2 == 1) ? halfNeuronDelta : 0.0;
                for (int c = 0; c < dimension; c++)
                {
                    //Generate our neurons
                    double[] posNeuron = { xPos , (double)r};
                    SOMNeuron neuron = new SOMNeuron(zero, one, rng, new KPoint(posNeuron), neuronIndex);
                    QuadTreePointStruct neuronQTPos = new QuadTreePointStruct()
                    {
                        Index = neuronIndex,
                        X = posNeuron[0],
                        Y = posNeuron[1]
                    };
                    neuronQuadTree.Insert(neuronQTPos);
                    neurons.Add(neuron);
                    xPos += 2*halfNeuronDelta;
                    neuronIndex++;
                }
            }

           


            //Get the max radius
            SetRadius();

            timeConstant = (double)totalItterations / Math.Log(radius);
        }

        public void runLargeEpochs(double learningRate, int numLargeEpoch)
        {
            //Init itteration count
            this.initialLearningRate = learningRate;
            this.learningRate = learningRate;
            influence = 0;
            this.totalItterations = numLargeEpoch * data.Count;
            itterations = 1;
            this.timeConstant = (double)totalItterations / Math.Log(radius);

            List<KPoint> d = new List<KPoint>();
            do
            {
                if (d.Count == 0)
                {
                    d.AddRange(data.PointList);
                }
                int point = rng.Next(0, d.Count);
                Epoch(d[point]);
                //delete
                d[point] = d[d.Count - 1];
                d.RemoveAt(d.Count - 1);
            } while (!doneExecuting());
        }

        public void Epoch()
        {
            Epoch(data[rng.Next(0, data.Count)]);
        }

        public void Epoch(KPoint p)
        {
            //get a random item
            SOMNeuron bmu = GetBestMatchingUnit(p);
            neighborhoodRadius = radius * Math.Exp(-(double)itterations / timeConstant);
            double neighborSqr = neighborhoodRadius*neighborhoodRadius;

            QuadTreePointStruct center = new QuadTreePointStruct()
            {
                Index = -1,
                X = bmu.position[0],
                Y = bmu.position[1]
            };
            QuadTreePointStruct halfLength = new QuadTreePointStruct()
            {
                Index = -1,
                X = neighborhoodRadius,
                Y = neighborhoodRadius
            };

            List<int> neuronsInArea =
                neuronQuadTree.QueryRange(new QuadTreeBoundingBox() {Center = center, HalfLength = halfLength});

            //Now we need to update each neuron
            foreach (int i in neuronsInArea)
            {
                SOMNeuron n = neurons[i];
                double distToBMU = n.position.elucideanDistance(bmu.position);
                if (distToBMU <= neighborhoodRadius)
                {
                    influence = Math.Exp(-(distToBMU*distToBMU) / (2 * neighborSqr));
                    n.updateWeights(p, learningRate, influence);
                }
            }

            learningRate = this.initialLearningRate * Math.Exp(-(double)itterations / (double)this.totalItterations);
            itterations++;
        }

        private SOMNeuron GetBestMatchingUnit(KPoint data)
        {
            SOMNeuron best = null;
            double dist = Double.MaxValue;
            foreach (SOMNeuron n in neurons)
            {
                double d = n.weights.distanceSquared(data);
                if (d < dist)
                {
                    dist = d;
                    best = n;
                }
            }
            return best;
        }

        private void SetRadius()
        {
            double halfNeuronDelta = 1 / Math.Sqrt(3.0);
            double[] min = {0.0,0.0};
            double[] max = { (_dimension - 0.5) * 2 * halfNeuronDelta, _dimension - 1.0 };
            radius = new KPoint(max).elucideanDistance(new KPoint(min));
        }

        //This is a lazy mode which just gets each neuron and gets all matchind dataPoints
        public Partition GetClusterLazy()
        {
            List<ClusteredItem> dataPoints = new List<ClusteredItem>();
            for (int i = 0; i < data.Count; i++)
                dataPoints.Add(new ClusteredItem(i));

            List<Cluster> clusters = new List<Cluster>();
            for (int i = 0; i < neurons.Count; i++)
                clusters.Add(new Cluster(i));

            foreach (var p in dataPoints)
                clusters[GetBestMatchingUnit(data[p.Id]).id].AddPoint(p);
            
            return new Partition(clusters, data, "SOM - Lazy clustering\n k = " + neurons.Count);
        }

        public List<Bitmap> GetUMatrix(int cellWidth, int labelCount = 0, int[] labels = null)
        {
            cellWidth += (cellWidth%2); //make it even
            int halfCellWidth = cellWidth/2;
            double verticalOffset = (double) halfCellWidth/Math.Tan(Math.PI/6);
            double halfVerticalHeight = (double)halfCellWidth / Math.Cos(Math.PI / 6);

            List<Bitmap> images = new List<Bitmap>();
            List<Graphics> imgGraphics = new List<Graphics>();
            Size imageSize = new Size((int)((_dimension + .5) * cellWidth), (int)((_dimension - 1) * verticalOffset + 2 * halfVerticalHeight));
            Bitmap img = new Bitmap(imageSize.Width, imageSize.Height);
            Bitmap imgPointCount = new Bitmap(imageSize.Width, imageSize.Height);
            Graphics g = Graphics.FromImage(img);
            Graphics gpc = Graphics.FromImage(imgPointCount);
            images.Add(img); images.Add(imgPointCount);
            imgGraphics.Add(g); imgGraphics.Add(gpc);
            if (labels != null)
            {
                for (int i = 0; i < labelCount; i++)
                {
                    Bitmap classImg = new Bitmap(imageSize.Width, imageSize.Height);
                    Graphics classG = Graphics.FromImage(classImg);
                    images.Add(classImg); imgGraphics.Add(classG);
                }
            }

            //Calculate the distance matrix

            double maxValue = 0.0;
            double minValue = double.MaxValue;
            double[,] weights = new double[_dimension, _dimension];
            
            for (int x = 0; x < _dimension; x++)
            {
                for (int y = 0; y < _dimension; y++)
                {
                    weights[x, y] = 0;

                    int[] neighborCoordPairsEven = {x - 1, y - 1, x, y - 1, x - 1, y, x + 1, y, x - 1, y + 1, x, y + 1};
                    int[] neighborCoordPairsOdd  = {x, y - 1, x + 1, y - 1, x - 1, y, x + 1, y, x, y + 1, x + 1, y + 1};
                    SOMNeuron n = neurons[y*_dimension+x];
                    int neighborCount = 0;
                    for (int i = 0; i < 12; i += 2)
                    {
                        int index = 0;
                        if (y%2 == 0)
                            index = GetNeuronIndexFromPos(neighborCoordPairsEven[i], neighborCoordPairsEven[i + 1]);
                        else
                            index = GetNeuronIndexFromPos(neighborCoordPairsOdd[i], neighborCoordPairsOdd[i + 1]);

                        if (index >= 0)
                        {
                            weights[x, y] += n.weights.elucideanDistance(neurons[index].weights);
                            neighborCount++;
                        }
                    }

                    weights[x, y] /= (double) neighborCount;
                    maxValue = Math.Max(maxValue, weights[x, y]);
                    minValue = Math.Min(minValue, weights[x, y]);
                }
            }

            //now get neuron matching count
            int[] matchingCount = new int[_dimension * _dimension];
            int[,] matchingCountSpecific = (labelCount > 0) ? new int[_dimension*_dimension, labelCount] : null;
            for (int kIndex = 0; kIndex < data.Count; kIndex++)
            {
                var bmu = GetBestMatchingUnit(data[kIndex]);
                matchingCount[bmu.id]++;
                if (labelCount > 0)
                {
                    matchingCountSpecific[bmu.id, labels[kIndex]]++;
                }
            }
            
            int maxMatchingCount = matchingCount.Max();
            int[] maxMatchingLabel = (labelCount > 0) ? new int[labelCount] : null;
            if (labelCount > 0)
            {
                for (int n = 0; n < neurons.Count; n++)
                    for (int l = 0; l < labelCount; l++)
                        maxMatchingLabel[l] = Math.Max(maxMatchingLabel[l], matchingCountSpecific[n, l]);

            }
            
            //Now that we have a weight matrix and the maxvalue
            //we can print our nodes!
            for (int x = 0; x < _dimension; x++)
            {
                for (int y = 0; y < _dimension; y++)
                {
                    double distanceShifted = weights[x, y] - minValue;
                    double intensity = distanceShifted/(maxValue - minValue);
                    int bright = (int) ((1.0 - intensity)*255.0);
                    Color c = Color.FromArgb(bright, bright, bright);
                    Brush b = new SolidBrush(c);

                    //Calculate the brush for the point matching list
                    Color cCount = Color.White;
                    if (matchingCount[x + y*_dimension] > 0)
                    {
                        double countIntensity = (matchingCount[x + y*_dimension] - 1.0)/(maxMatchingCount - 1.0);
                        cCount = LerpColor(Color.Aqua, Color.Red, countIntensity);
                    }
                    Brush bC = new SolidBrush(cCount);

                    //get x
                    double xAdjusted = (double)x * cellWidth + halfCellWidth;
                    if (y%2 == 1)
                        xAdjusted += halfCellWidth;
                    double yAdjusted = (double)y * verticalOffset + halfVerticalHeight;
                    Point[] hexagon = GetCenteredHexagon(xAdjusted, yAdjusted, halfCellWidth, halfVerticalHeight);
                    g.FillPolygon(b, hexagon, FillMode.Winding);
                    gpc.FillPolygon(bC, hexagon, FillMode.Winding);

                    //Color the specific class images
                    for (int i = 0; i < labelCount; i++)
                    {
                        Color classColor = Color.White;
                        if (matchingCountSpecific[x + y * _dimension, i] > 0)
                        {
                            double countIntensity = (matchingCountSpecific[x + y * _dimension, i] - 1.0) / (maxMatchingLabel[i] - 1.0);
                            classColor = LerpColor(Color.Aqua, Color.Red, countIntensity);
                        }
                        Brush classBrush = new SolidBrush(classColor);
                        imgGraphics[i + 2].FillPolygon(classBrush, hexagon, FillMode.Winding);
                    }
                }
            }

            foreach (Graphics gr in imgGraphics)
                gr.Dispose();

            return images;
        }

        private Point[] GetCenteredHexagon(double x, double y, double halfWidth, double halfHeight)
        {
            Point[] points = new Point[6];

            double h = halfWidth*Math.Tan(Math.PI / 6); //30 degrees

            points[0] = new Point((int)(x + 0.5), (int)(y - halfHeight + 0.5));
            points[1] = new Point((int)(x + halfWidth + 0.5), (int)(y - h + 0.5));
            points[2] = new Point((int)(x + halfWidth + 0.5), (int)(y + h + 0.5));
            points[3] = new Point((int)(x + 0.5), (int)(y + halfHeight + 0.5));
            points[4] = new Point((int)(x - halfWidth + 0.5), (int)(y + h + 0.5));
            points[5] = new Point((int)(x - halfWidth + 0.5), (int)(y - h + 0.5));

            return points;
        }

        private Color LerpColor(Color a, Color b, double amount)
        {
            int r = interpolate(a.R, b.R, amount);
            int g = interpolate(a.G, b.G, amount);
            int bl = interpolate(a.B, b.B, amount);
            return Color.FromArgb(255, r, g, bl);
        }

        private int interpolate(int a, int b, double amount)
        {
            double diff = (b - a);
            double delta = amount*diff + (double) a;
            return (int) delta;
        }

        private int GetNeuronIndexFromPos(int x, int y)
        {
            if (x < 0 || x >= _dimension || y < 0 || y >= _dimension)
                return -1;
            return (_dimension*y + x);
        }
    }
}
