using NetMining.Data;

namespace NetMining.Graphs.Generator
{
    public interface IPointGraphGenerator
    {

        LightWeightGraph GenerateGraph(DistanceMatrix d);
    }
}
