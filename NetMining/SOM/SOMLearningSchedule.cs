//Class specifies a learning schedule for the data
namespace NetMining.SOM
{
    class SOMLearningSchedule
    {
        public readonly double LearningRate;
        public readonly bool WithReplacement;
        public readonly int Itterations;

        public SOMLearningSchedule(double learningRate, bool withReplacement, int itterations)
        {
            LearningRate = learningRate;
            WithReplacement = withReplacement;
            Itterations = itterations;
        }
    }
}
