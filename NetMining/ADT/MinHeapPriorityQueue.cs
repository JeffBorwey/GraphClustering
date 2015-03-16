using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMining.ADT
{
    public class MinHeapPriorityQueue<T>
    {
        public delegate bool isGreaterThan(T a, T b); //used for creating the heap

        List<T> items; //items in the heap
        Dictionary<T, int> itemIndex; //maps our items to the index
        isGreaterThan comp;

        public MinHeapPriorityQueue(isGreaterThan c)
        {
            comp = c;
            items = new List<T>();
            itemIndex = new Dictionary<T, int>();
        }

        public bool isEmpty() { return items.Count == 0; }

        public void decreaseKey(T item)
        {
            if (items.Count <= 1)
                return;
            int pos = itemIndex[item];
            int parent = (pos - 1) / 2;
            //if greater than parent
            if (comp(items[parent], items[pos]))
                heapifyUp(pos);
            else
                heapifyDown(pos);
        }

        public T peek() { return items[0]; }

        public void addAll(IEnumerable<T> put)
        {
            items.AddRange(put);
            for (int i = 0; i < items.Count; i++)
                itemIndex.Add(items[i], i);

            //itemIndex = items.Select((value, index) => new { value, index }).ToDictionary(k => k.value, v => v.index);
            /*foreach (T e in put)
            {
                items.Add(e);
            }*/

            heapify();
        }

        private void heapify()
        {
            int start = (items.Count - 2) / 2; //Parent of last element

            while (start >= 0)
            {
                heapifyDown(start);
                start--;
            }
        }

        public T extractMin()
        {
            T min = items[0]; //Get min item

            //set new min to last item
            items[0] = items[items.Count - 1];
            itemIndex[items[0]] = 0;
            itemIndex.Remove(min);
            items.RemoveAt(items.Count - 1);

            //Heapify
            heapifyDown(0);

            //return
            return min;
        }

        public int heapifyUp(int pos)
        {
            if (pos >= items.Count) return -1;

            while (pos > 0)
            {
                int parent = (pos - 1) / 2;
                if (comp(items[parent], items[pos]))
                {
                    swap(parent, pos);
                    pos = parent;
                }
                else break;
            }

            return pos;
        }

        public void heapifyDown(int pos)
        {
            if (pos >= items.Count) return;

            while (true)
            {
                int smallest = pos;
                int leftChild = 2 * pos + 1;
                int rightChild = leftChild + 1;

                if (leftChild < items.Count && comp(items[smallest], items[leftChild]))
                {
                    smallest = leftChild;
                }
                if (rightChild < items.Count && comp(items[smallest], items[rightChild]))
                {
                    smallest = rightChild;
                }
                if (smallest != pos)
                {
                    swap(smallest, pos);
                    pos = smallest;
                }
                else break; // if pos was not updated, we are done
            }
        }

        private void swap(int a, int b)
        {

            T temp = items[a];
            itemIndex[temp] = b; //change the indicies in the map
            items[a] = items[b];
            items[b] = temp;
            itemIndex[items[a]] = a;//change the indicies in the map
        }
    }
}