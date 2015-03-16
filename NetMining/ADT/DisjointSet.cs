using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMining.ADT
{
    public class DisjointSet
    {
        List<Node> elements;
        int nodecount, setCount;
        internal LinkedList<int> trees;
        class Node
        {
            public int rank, index;
            public Node Parent;

            public Node(int rank, int index, Node Parent)
            {
                this.rank = rank;
                this.index = index;
                this.Parent = Parent;
            }
        }

        public DisjointSet(int numberElements)
        {
            elements = new List<Node>();
            trees = new LinkedList<int>();
            add(numberElements);
        }

        public void add(int numberElements)
        {
            for (int i = nodecount; i < nodecount + numberElements; i++)
            {
                Node newElement = new Node(0, i, null);
                trees.AddLast(i);
                elements.Add(newElement);
            }

            setCount += numberElements;
            nodecount += numberElements;
        }

        public Boolean diff(int idA, int idB)
        {
            return (find(idA) != find(idB));
        }

        public int find(int nodeID)
        {
            Node current = elements[nodeID];
            //Find the root of the set
            while (current.Parent != null) { current = current.Parent; }

            //do set compression to speed up future access
            Node root = current;
            current = elements[nodeID];
            while (root != current)
            {
                Node oldParent = current.Parent;
                current.Parent = root;
                current = oldParent;
            }
            

            return root.index;
        }


        public bool unionWithRemove(int idA, int idB)
        {
            if (idA == idB)
            {
                return false;
            }


            Node setA = elements[find(idA)];
            Node setB = elements[find(idB)];

            if (setA.rank > setB.rank) 
            {
                setB.Parent = setA;
                trees.Remove(setB.index);
            }
            else if (setA.rank < setB.rank)
            {
                setA.Parent = setB;
                trees.Remove(setA.index);
            }
            else //equal rank
            {
                setB.Parent = setA;
                trees.Remove(setB.index);
                ++setA.rank;
            }

            setCount--;
            return true;
        }

        public bool union(int idA, int idB)
        {
            if (idA == idB)
            {
                return false;
            }


            Node setA = elements[find(idA)];
            Node setB = elements[find(idB)];

            if (setA.rank > setB.rank)
            {
                setB.Parent = setA;
            }
            else if (setA.rank < setB.rank)
            {
                setA.Parent = setB;
            }
            else //equal rank
            {
                setB.Parent = setA;
                ++setA.rank;
            }

            setCount--;
            return true;
        }
    }

    
}
