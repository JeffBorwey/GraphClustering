using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NetMining.ADT
{
    public class QuadTree
    {
        public const int NODE_CAPACITY = 4;


        private QuadTreeBoundingBox _boundary;

        public QuadTree UpperLeft = null;
        public QuadTree UpperRight = null;
        public QuadTree LowerLeft = null;
        public QuadTree LowerRight = null;

        private readonly QuadTreePointStruct[] _quadTreePoints;
        private int _countPoints;

        public QuadTree(QuadTreeBoundingBox boundary)
        {
            _boundary = boundary;
            _quadTreePoints = new QuadTreePointStruct[NODE_CAPACITY];
            _countPoints = 0;
        }

        public bool Insert(QuadTreePointStruct p)
        {
            if (!_boundary.ContainsPoint(p)) return false;

            if (_countPoints < NODE_CAPACITY)
            {
                _quadTreePoints[_countPoints++] = p;
                return true;
            }

            //If we don't have room, we must subdivide to make room
            if (UpperLeft == null)
                Subdivide();

            if (UpperLeft.Insert(p)) return true;
            if (UpperRight.Insert(p)) return true;
            if (LowerLeft.Insert(p)) return true;
            if (LowerRight.Insert(p)) return true;

            return false;
        }

        private void Subdivide()
        {
            QuadTreePointStruct halfDistance = new QuadTreePointStruct()
            {
                Index = -1,
                X = _boundary.HalfLength.X/2,
                Y = _boundary.HalfLength.Y/2
            };

            //Create our 4 regions
            UpperLeft = new QuadTree(new QuadTreeBoundingBox() { Center = new QuadTreePointStruct() { Index = -1, X = _boundary.Center.X - halfDistance.X, Y = _boundary.Center.Y + halfDistance.Y }, HalfLength = halfDistance });
            UpperRight = new QuadTree(new QuadTreeBoundingBox() { Center = new QuadTreePointStruct() { Index = -1, X = _boundary.Center.X + halfDistance.X, Y = _boundary.Center.Y + halfDistance.Y }, HalfLength = halfDistance });
            LowerLeft = new QuadTree(new QuadTreeBoundingBox() { Center = new QuadTreePointStruct() { Index = -1, X = _boundary.Center.X - halfDistance.X, Y = _boundary.Center.Y - halfDistance.Y }, HalfLength = halfDistance });
            LowerRight = new QuadTree(new QuadTreeBoundingBox() { Center = new QuadTreePointStruct() { Index = -1, X = _boundary.Center.X + halfDistance.X, Y = _boundary.Center.Y - halfDistance.Y }, HalfLength = halfDistance });

            //for each point, determine where it lies
            for (int i = 0; i < _countPoints; i++)
            {
                if (UpperLeft.Insert(_quadTreePoints[i])) continue;
                if (UpperRight.Insert(_quadTreePoints[i])) continue;
                if (LowerLeft.Insert(_quadTreePoints[i])) continue;
                LowerRight.Insert(_quadTreePoints[i]);
            }

            //Set the point count to 0
            _countPoints = 0;
        }

        public List<int> QueryRange(QuadTreeBoundingBox area)
        {
            List<int> includedPoints = new List<int>();

            QueryRange(area, ref includedPoints);

            return includedPoints;
        }

        private void QueryRange(QuadTreeBoundingBox area, ref List<int> pointList)
        {
            if (!area.Overlaps(_boundary))
                return;


            for (int i = 0; i < _countPoints; i++)
            {
                if(area.ContainsPoint(_quadTreePoints[i]))
                    pointList.Add(_quadTreePoints[i].Index);
            }

            if (UpperLeft == null)
                return;

            UpperLeft.QueryRange(area, ref pointList);
            UpperRight.QueryRange(area, ref pointList);
            LowerLeft.QueryRange(area, ref pointList);
            LowerRight.QueryRange(area, ref pointList);
        }
    }

    public struct QuadTreePointStruct
    {
        public double X, Y;
        public int Index;
    }

    public struct QuadTreeBoundingBox
    {
        public QuadTreePointStruct Center;
        public QuadTreePointStruct HalfLength;

        public bool ContainsPoint(QuadTreePointStruct p)
        {
            return ((p.X >= Center.X - HalfLength.X) && (p.X <= Center.X + HalfLength.X) &&
                    (p.Y >= Center.Y - HalfLength.Y) && (p.Y <= Center.Y + HalfLength.Y));
        }

        public bool Overlaps(QuadTreeBoundingBox b)
        {
            if (Center.X + HalfLength.X < b.Center.X - b.HalfLength.X) return false;
            if (Center.X - HalfLength.X > b.Center.X + b.HalfLength.X) return false;
            if (Center.Y + HalfLength.Y < b.Center.Y - b.HalfLength.Y) return false;
            if (Center.Y - HalfLength.Y > b.Center.Y + b.HalfLength.Y) return false;
            return true;
        }
    }
}
