using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.Graphs
{
    //This is used for constructing a graph
    public class EdgeValuePair<T>
    {
        public T to;
        public int weight;
        public EdgeValuePair(T vTo, int weightTo)
        {
            to = vTo;
            weight = weightTo;
        }
    }
}
