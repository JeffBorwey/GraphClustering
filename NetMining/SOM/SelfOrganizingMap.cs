using System;
using System.Collections.Generic;
using System.Drawing;
using NetMining.ClusteringAlgo;
using NetMining.Data;

namespace NetMining.SOM
{
    //This is a Self Organizing Map(Kohonen SOM)
    //The structure of the neurons is important
    
    public class SelfOrganizingMap
    {
        private double radius; //This is the max radius
        private double neighborhoodRadius;

        private double initialLearningRate;
        private double learningRate;
        private double influence;
        private double timeConstant;

        private List<int> dimensions; // These are the dimensions the neurons are mapping to
        private List<SOMNeuron> neurons;

        public PointSet data;
        public double[] conversionMatrix;
        private int totalItterations;
        public int itterations;
        private Random rng;


        public bool doneExecuting()
        {
            return itterations >= totalItterations;
        }

        public SelfOrganizingMap(PointSet data, List<int> dimensions, int totalItt, double learningRate)
        {
           //Get initial learning rate
            this.initialLearningRate = learningRate;
            this.learningRate = learningRate;
            influence = 0;
            this.totalItterations = totalItt;
            itterations = 1;
            
            this.dimensions = dimensions;
            this.data = data;
            rng = new Random((int)DateTime.Now.Ticks);

            int numNeurons = 1;
            foreach (int d in dimensions)
                numNeurons *= d;

            int[] pos = new int[dimensions.Count];
            for (int i = 0; i < dimensions.Count; i++)
                pos[i] = 0;

            //Scale each attribute from [0,1] and store the conversion matrix
            conversionMatrix = data.GetMinMaxWeights().Max.Coordinates;

            foreach (KPoint d in data.PointList)
                d.Normalize(conversionMatrix);

            //Here We will initialize Our Neurons
            neurons = new List<SOMNeuron>();
            KPoint zero = KPoint.Zero(data[0].Dimensions);
            KPoint one = KPoint.One(data[0].Dimensions);
            for (int i = 0; i < numNeurons; i++)
            {
                //Generate our neurons
                double[] posNeuron = new double[dimensions.Count];
                for (int d = 0; d < dimensions.Count; d++)
                    posNeuron[d] = (double)pos[d];
                SOMNeuron neuron = new SOMNeuron(zero, one, rng, new KPoint(posNeuron), i);
                neurons.Add(neuron);
                addOneCarry(ref pos, dimensions);
            }

            //Get the max radius
            setRadius();

            this.timeConstant = (double)totalItterations / Math.Log(radius);
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
            //Now we need to update each neuron
            foreach (SOMNeuron n in neurons)
            {
                double distToBMUSqr = n.position.elucideanDistance(bmu.position);
                if (distToBMUSqr <= neighborSqr)
                {
                    influence = Math.Exp(-(distToBMUSqr) / (2 * neighborSqr));
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
                double d = n.weights.elucideanDistance(data);
                if (d < dist)
                {
                    dist = d;
                    best = n;
                }
            }
            return best;
        }

        private void setRadius()
        {
            double[] min = new double[dimensions.Count];
            double[] max = new double[dimensions.Count];
            for (int i = 0; i < dimensions.Count; i++)
            {
                min[i] = 0;
                max[i] = (double)(dimensions[i] - 1);
            }
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

        public Bitmap GetUMatrix(int cellWidth, bool use8Neighbors)
        {
            if (dimensions.Count == 2)
            {
                Bitmap img = new Bitmap(dimensions[0] * cellWidth, dimensions[1] * cellWidth);
                Graphics g = Graphics.FromImage(img);
                double maxValue = 0.0;
                double minValue = double.MaxValue;
                double[,] weights = new double[dimensions[0], dimensions[1]];

                for (int x = 0; x < dimensions[0]; x++)
                {
                    for (int y = 0; y < dimensions[1]; y++)
                    {
                        weights[x, y] = 0;
                        SOMNeuron n = neurons[utilGet1dIndex(x, y)];
                        int neighborCount = 0;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;

                                if (!(dx == 0 && dy == 0) && (nx >= 0 && nx < dimensions[0]) && (ny >= 0 && ny < dimensions[1]))
                                {
                                    if ((!use8Neighbors && (Math.Abs(dx) == Math.Abs(dy))) || use8Neighbors)
                                    {
                                        SOMNeuron no = neurons[utilGet1dIndex(nx, ny)];
                                        weights[x, y] += n.weights.elucideanDistance(no.weights);
                                        neighborCount++;
                                    }

                                }
                            }
                        }


                        weights[x, y] /= (double)neighborCount;
                        maxValue = Math.Max(maxValue, weights[x, y]);
                        minValue = Math.Min(minValue, weights[x, y]);
                    }
                }

                //Now that we have a weight matrix and the maxvalue
                //we can print our nodes!
                for (int x = 0; x < dimensions[0]; x++)
                {
                    for (int y = 0; y < dimensions[1]; y++)
                    {
                        double distanceShifted = weights[x, y] - minValue;
                        double intensity = distanceShifted / (maxValue-minValue);
                        int bright = (int)((1.0-intensity)*255.0);
                        Color c = Color.FromArgb(bright, bright, bright);
                        Brush b = new SolidBrush(c);

                        //get x

                        g.FillRectangle(b, new Rectangle(new Point(x * cellWidth, y * cellWidth), new Size(cellWidth, cellWidth)));
                    }
                }

                return img;
            }
            else
            {
                return new Bitmap(10,10);
            }
        }

        public Bitmap AddCountData(int cellWidth, Bitmap img)
        {
            Graphics g = Graphics.FromImage(img);

            int[,] count = new int[dimensions[0], dimensions[1]];
            foreach (var p in data.PointList)
            {
                SOMNeuron bmu = GetBestMatchingUnit(p);
                var pos = utilGetPosFromID(bmu.id);
                count[pos.Item1, pos.Item2]++;
            }
            
            int max = 0;
            for (int x = 0; x < dimensions[0]; x++)
                for (int y = 0; y < dimensions[1]; y++)
                    max = Math.Max(max, count[x, y]);

            for (int x = 0; x < dimensions[0]; x++)
            {
                for (int y = 0; y < dimensions[1]; y++)
                {
                    int intensity = (int)(255.0 * (double)count[x, y] / (double)max);
                    Color c = Color.FromArgb(intensity, 255 , 0, 0);
                    Brush b = new SolidBrush(c);
                    g.FillRectangle(b, new Rectangle(new Point(x * cellWidth, y * cellWidth), new Size(cellWidth, cellWidth)));
                }
            }

            return img;
        }

        private int utilGet1dIndex(int x, int y)
        {
            return x + y * dimensions[0];
        }

        private Tuple<int, int> utilGetPosFromID(int id)
        {
            int x = id % dimensions[0];
            int y = id / dimensions[0];
            return new Tuple<int, int>(x, y);
        }

        //Utility for Neuron Generation
        private static void addOneCarry(ref int[] arr, List<int> dimensions) 
        {
            arr[0] += 1;
            for (int i = 0; i < dimensions.Count - 1; i++)
            {
                if (arr[i] == dimensions[i])
                {
                    arr[i] = 0;
                    arr[i + 1]++;
                }
            }
        }
    }
}
