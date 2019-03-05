using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Carter.Pathfinding
{
    static class LOS
    {
        /*void UpdateLOS()
        {
            visible = new BitArray(level.width * level.height);
            //UInt16[] walls = level.layers.First(l => l.type == LayerType.Collision).gIDs;
            BitArray collision = level.GetCollisionBitArr();
            //BitArray collision = walls.ToBitArray();

            int distance = 10;
            int diagonal = (int)(distance * 1.4f);
            for (int y = player.Tile.Y - distance; y <= player.Tile.Y + distance; y++)
            {
                if (y < 0 || y >= level.height)
                    continue;
                for (int x = player.Tile.X - distance; x <= player.Tile.X + distance; x++)
                {
                    if (x < 0 || x >= level.width)
                        continue;
                    int tile = x + y * level.width;
                    int xDist = Math.Abs(player.Tile.X - x);
                    int yDist = Math.Abs(player.Tile.Y - y);
                    if (xDist + yDist <= diagonal)
                    {
                        if (HasLOS(player.Tile, new Point(x, y), collision))
                            visible[tile] = true;
                    }
                }
            }
        }*/
        public static bool HasLOS(Point start, Point end, BitArray collision, int colRowCount)
        {
            IEnumerable<Point> tiles = GetPointsOnLine(start.X, start.Y, end.X, end.Y);
            foreach(var tile in tiles)
            {
                int colTileIndex = tile.X + tile.Y * colRowCount;
                if (collision[colTileIndex] && tile != end)
                    return false;
            }
            return true;
        }
        public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return (steep ? new Point(y, x) : new Point(x, y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }
    }
}
