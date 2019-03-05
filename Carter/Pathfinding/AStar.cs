using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Carter.Pathfinding
{
    public class AStarNode
    {
        public Point P;
        public int G;
        public int H;
        public int F { get { return G + H; } }
        public AStarNode Parent;
    }
    public static class AStar
    {
        /// <summary>
        /// <para>Finds a path from start to end based on a collision array of passable tiles</para>
        /// <para>Points represent indices of columns (X) and rows (Y) with origin(0,0) at top left corner</para>
        /// <para>Collision array represents impassable tiles on a flattened map, where rows are stored sequentially, each <paramref name="mapWidth"/> in length</para>
        /// </summary>
        /// <param name="start">Start point</param>
        /// <param name="end">End point</param>
        /// <param name="collision">Flattened map of impassable tiles, where rows are stored sequentially, each <paramref name="mapWidth"/> in length</param>
        /// <param name="mapWidth">Number of columns in a <paramref name="collision"/> map array</param>
        /// <param name="maxLength">Maximum search distance</param>
        public static List<Point> GetAPath(Point start, Point end, BitArray collision, int mapWidth, int maxLength = 0)
        {
            if (end == start)
                return new List<Point>();
            int mapHeight = collision.Length / mapWidth;
            List<AStarNode> OpenNodes = new List<AStarNode>();
            List<AStarNode> ClosedNodes = new List<AStarNode>();
            OpenNodes.Add(new AStarNode() { P = start, G = 0, H = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y), Parent = null });
            AStarNode endSquare = null;
            while (true)
            {
                int lowestF = int.MaxValue;
                int index = 0;
                for (int i = OpenNodes.Count - 1; i >= 0; i--)
                {
                    if (maxLength != 0 && OpenNodes[i].G > maxLength)
                    {
                        OpenNodes.RemoveAt(i);
                    }
                    else if (OpenNodes[i].F < lowestF)
                    {
                        lowestF = OpenNodes[i].F;
                        index = i;
                    }
                }

                if (OpenNodes.Count == 0)
                    break;

                AStarNode currentNode = OpenNodes[index];

                OpenNodes.Remove(currentNode);
                ClosedNodes.Add(currentNode);

                if (currentNode.P == end)
                {
                    endSquare = currentNode;
                    break;
                }

                Point[] targetPoints = new Point[4];
                targetPoints[0] = new Point(currentNode.P.X, currentNode.P.Y - 1); //top
                targetPoints[1] = new Point(currentNode.P.X + 1, currentNode.P.Y); //right
                targetPoints[2] = new Point(currentNode.P.X, currentNode.P.Y + 1); //bottom
                targetPoints[3] = new Point(currentNode.P.X - 1, currentNode.P.Y); //left
                for (int i = 0; i < targetPoints.Length; i++)
                {
                    int tile = targetPoints[i].X + targetPoints[i].Y * mapWidth;
                    if (targetPoints[i].X < 0 || targetPoints[i].X >= mapWidth || targetPoints[i].Y < 0 || targetPoints[i].Y >= mapHeight || collision[tile])
                        continue;
                    if (ClosedNodes.Find(c => c.P == targetPoints[i]) == null) // not in closed list
                    {
                        int g = currentNode.G + 1;
                        int h = Math.Abs(targetPoints[i].X - end.X) + Math.Abs(targetPoints[i].Y - end.Y);
                        var openNode = OpenNodes.Find(c => c.P == targetPoints[i]);
                        if (openNode != null && g + h < openNode.F) // if already in open list check if this is better path
                        {
                            openNode.Parent = currentNode;
                            openNode.G = g;
                        }
                        else if (openNode == null) // not in open list
                            OpenNodes.Add(new AStarNode() { Parent = currentNode, P = targetPoints[i], G = g, H = h });
                    }
                }
            }

            List<Point> returnList = new List<Point>();
            if (endSquare != null)
            {
                AStarNode curNode = endSquare;
                while (curNode != null)
                {
                    returnList.Add(curNode.P);
                    curNode = curNode.Parent;
                }
            }
            return returnList;
        }
    }
}
