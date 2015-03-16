namespace NetMining.ADT
{
    public class IndexedItem
    {
        public int HeapIndex;
        public float NodeWeight;
        public int NodeIndex;

        public IndexedItem(int nodeIndex, float nodeWeight)
        {
            NodeIndex = nodeIndex;
            HeapIndex = -1;
            NodeWeight = nodeWeight;
        }
    }
}
