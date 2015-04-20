namespace NetMining.ADT
{
    public class IndexedItem
    {
        public int HeapIndex;
        public double NodeWeight;
        public int NodeIndex;

        public IndexedItem(int nodeIndex, double nodeWeight)
        {
            NodeIndex = nodeIndex;
            HeapIndex = -1;
            NodeWeight = nodeWeight;
        }
    }
}
