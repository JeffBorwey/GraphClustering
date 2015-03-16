using System;
using NetMining.Data;

namespace NetMining.SOM
{
    public class SOMNeuron
    {
        public KPoint position; // This is a map to the 
        public KPoint weights;
        public int id;
        //Initialize tyhe weights with a randon between min and max
        public SOMNeuron(KPoint min, KPoint max, Random rng, KPoint position, int id)
        {
            weights = new KPoint(min, max, rng);
            this.position = position;
            this.id = id;
        }

        //This updates the weights of the neuron to pull towards 
        public void updateWeights(KPoint target, double learningRate, double influence)
        {
            for (int i = 0; i < weights.Dimensions; i++)
                weights[i] += (target[i] - weights[i]) * learningRate * influence;
        }
    }
}
