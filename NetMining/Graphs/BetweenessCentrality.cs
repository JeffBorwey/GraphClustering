namespace NetMining.Graphs
{
    static class BetweenessCentrality
    {

        public static float[] BrandesBc(LightWeightGraph g)
        {
            //Stopwatch sr = new Stopwatch();
            //sr.Start();
            int numnodes = g.NumNodes;
            float[] bcMap = new float[numnodes];
            
            for (int v = 0; v < numnodes; v++)
            {
                //Get a shortest path, if weighted use Dikstra, if unweighted use BFS
                ShortestPathProvider asp = (g.IsWeighted) ? new DikstraProvider2(g, v) : 
                                                            new BFSProvider(g,v) as ShortestPathProvider;

                float[] delta = new float[numnodes];
                
                while (asp.S.Count > 0)
                {
                    int w = asp.S.Pop();
                    var wList = asp.fromList[w];
                    foreach (int n in wList)
                    {
                        delta[n] += ((float)asp.numberOfShortestPaths[n] / (float)asp.numberOfShortestPaths[w]) * (1.0f + delta[w]);
                        if (n != v)
                            bcMap[n] += delta[n];
                    }
                }
            }

            //divide all by 2 (undirected)
            for (int v = 0; v < numnodes; v++)
                bcMap[v] /= 2f;

            //sr.Stop();
            //Console.WriteLine(sr.ElapsedMilliseconds);
            return bcMap;
        }
    }
}
