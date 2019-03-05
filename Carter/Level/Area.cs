using System;
using System.Collections.Generic;
using System.Text;

namespace Carter.Level
{
    class Area
    {
        Tile[,] _tiles;
        public Tile this[int x, int y]
        {
            get { return _tiles[x,y]; }
            set { _tiles[x,y] = value; }
        }
        public int Width { get { return _tiles.GetLength(0); } }
        public int Height { get { return _tiles.GetLength(1); } }

        public Area(Tile[,] tiles)
        {
            _tiles = tiles;
        }
    }
}
