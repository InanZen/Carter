using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InanZEngine.GUI;
using InanZEngine;
using System.Collections;
using System.Threading;
namespace Carter.Level
{
    class Generator
    {
        public class Level
        {
            readonly List<Room> _rooms;
            readonly TileType[,] _tiles;
            readonly int _width;
            readonly int _height;

            public TileType[,] Tiles { get { return _tiles; } }
            public TileType this[int x, int y] { get { return _tiles[x, y]; } set { _tiles[x, y] = value; } }
            public int Width { get { return _width; } }
            public int Height { get { return _height; } }
            public List<Room> Rooms { get { return _rooms; } }

            public Level(int width, int height)
            {
                _tiles = new TileType[width, height];
                _width = width;
                _height = height;
                _rooms = new List<Room>();
            }

            public void AddRoom(Room room, int posX, int posY)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    for (int y = 0; y < room.Height; y++)
                    {
                        if (room[x, y] == TileType.Floor)
                            _tiles[posX + x, posY + y] = TileType.Floor;
                    }
                }
                room.Position = new Point(posX, posY);
                room.Index = _rooms.Count;
                _rooms.Add(room);
            }
            public void AddRoom(Room room, int posX, int posY, Point fromTile, Point direction)
            {
                AddRoom(room, posX, posY);
                while (_tiles[fromTile.X, fromTile.Y] == TileType.Wall && fromTile.X > 0 && fromTile.X < _width - 1 && fromTile.Y > 0 && fromTile.Y < _height - 1)
                {
                    _tiles[fromTile.X, fromTile.Y] = TileType.Floor;
                    fromTile += direction;
                }
                fromTile -= direction;
                room.Entrances.Add(fromTile);
            }


            /* void AddTunnel(Point tile, Point direction)
             {
                 while (_tiles[tile.X, tile.Y] == TileType.Wall && tile.X > 0 && tile.X < _width - 1 && tile.Y > 0 && tile.Y < _height - 1)
                 {
                     _tiles[tile.X, tile.Y] = TileType.Floor;
                     tile += direction;
                 }
             }*/


            /* public Room GetRoomAt(Point tile)
             {
                 foreach (Room room in _rooms)
                 {
                     Rectangle r = new Rectangle(room.Position.X, room.Position.Y, room.Width, room.Height);
                     if (r.Contains(tile))
                         return room;
                 }
                 return null;
             }*/


            public Tile[,] GenerateTiles()
            {
                Tile[,] tiles = new Tile[_width, _height];
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (_tiles[x, y] == TileType.Floor)
                        {
                            var spriteDefinition = GetSpriteDefinition(x, y);
                            tiles[x, y] = new Tile() { Visible = true, Position = new Point(x, y), SpriteDefinition = spriteDefinition };
                        }
                    }
                }
                for (int i = 0; i < _rooms.Count; i++)
                {
                    Room room = _rooms[i];
                    var spriteDefs = LevelSprites.SpriteDefinitions.Keys.ToArray();
                    int sdIndex = (i+1) % (spriteDefs.Length - 1);
                    for (int x = 0; x < room.Width; x++)
                    {
                        for (int y = 0; y < room.Height; y++)
                        {
                            int posX = room.Position.X + x;
                            int posY = room.Position.Y + y;
                            if (room[x, y] == TileType.Floor)
                            {
                                tiles[posX, posY] = new Tile()
                                {
                                    Visible = true,
                                    SpriteDefinition = LevelSprites.SpriteDefinitions[spriteDefs[sdIndex]][0],
                                    Position = new Point(room.Position.X + x, room.Position.Y + y)
                                };
                            }
                            else if (!tiles[posX, posY].Visible)
                            {
                                tiles[posX, posY] = new Tile()
                                {
                                    Visible = true,
                                    SpriteDefinition = new SpriteDefinition("wall", new Rectangle(17 * 16, 3 * 16, 16, 16)),
                                    Position = new Point(room.Position.X + x, room.Position.Y + y)
                                };
                            }
                        }
                    }

                }

                return tiles;
            }
            SpriteDefinition GetSpriteDefinition(int tileX, int tileY)
            {
                if (_tiles[tileX, tileY] == TileType.Floor)
                {
                    Direction wallDirection = GetWallDirections(tileX, tileY);
                    return LevelSprites.SpriteDefinitions["floor_stone_lightgray"][(byte)wallDirection];
                }
                else
                    return new SpriteDefinition("wall", new Rectangle(17 * 16, 3 * 16, 16, 16));
            }
            Direction GetWallDirections(int tileX, int tileY)
            {
                Direction dir = Direction.Zero;
                if (_tiles[tileX, tileY - 1] == TileType.Wall)
                    dir |= Direction.North;
                if (_tiles[tileX + 1, tileY] == TileType.Wall)
                    dir |= Direction.East;
                if (_tiles[tileX, tileY + 1] == TileType.Wall)
                    dir |= Direction.South;
                if (_tiles[tileX - 1, tileY] == TileType.Wall)
                    dir |= Direction.West;
                return dir;
            }
        }

        public class Room : IQuadtreeStorable
        {
            readonly TileType[,] _tiles;
            readonly int _width;
            readonly int _height;
            readonly List<Point> _entrances;
            Point _position;
            Rectangle2D _posRect;
            Rectangle _roomRect;
            
            public TileType[,] Tiles { get { return _tiles; } }
            public TileType this[int x, int y] { get { return _tiles[x, y]; } }
            public int Width { get { return _width; } }
            public int Height { get { return _height; } }
            public List<Point> Entrances { get { return _entrances; } }

            public int Index { get; set; }
            public Point Position
            {
                get { return _position; }
                set
                {
                    _position = value;
                    _roomRect.Location = _position;
                    _posRect = new Rectangle2D(_position.X, _position.Y, _width, _height);
                }
            }
            public Rectangle Rectangle { get { return _roomRect; } }

            QuadtreeNode IQuadtreeStorable.TreeNode { get; set; }

            public Room(TileType[,] tiles)
            {
                _tiles = tiles;
                _width = tiles.GetLength(0);
                _height = tiles.GetLength(1);
                _entrances = new List<Point>();
                _roomRect = new Rectangle(0, 0, _width, _height);
            }

            bool IQuadtreeStorable.IsContained(Rectangle2D container)
            {
                return container.Contains(_posRect);
            }

            bool IQuadtreeStorable.Intersects(Rectangle2D rectangle)
            {
                return _posRect.Intersects(rectangle);
            }

            bool IQuadtreeStorable.Intersects(ConvexPolygon2D polygon)
            {
                return _posRect.Intersects(polygon);
            }

            bool IQuadtreeStorable.Intersects(Circle circle)
            {
                return _posRect.Intersects(circle);
            }
        }

        public enum TileType
        {
            Wall = 0,
            Floor = 1
        }
        [Flags]
        enum Direction
        {
            Zero = 0,
            North = 1,
            East = 2,
            South = 4,
            West = 8
        }
        static readonly Point[] Directions = new Point[]
        {
            new Point(0, -1),
            new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0)
        };
        static readonly Dictionary<Direction, Point> DirectionToVector = new Dictionary<Direction, Point>()
        {
            { Direction.Zero, Point.Zero },
            { Direction.North, new Point(0, -1) },
            { Direction.East, new Point(1, 0) },
            { Direction.South, new Point(0, 1) },
            { Direction.West, new Point(-1, 0) },
        };

        int celularRoomMaxSize = 25;
        int celularRoomMinSize = 12;
        public int maxRoomCount = 5;

        int squareRoomMaxSize = 15;
        int squareRoomMinSize = 8;

        int crossRoomMaxSize = 15;
        int crossRoomMinSize = 8;

        float celularStartFloorProbability = 0.6f;
        int celularAdjWallThreshold = 4;


        float crossRoomChance = 0.10f;
        float squareRoomChance = 0.25f;

        int buildRoomAttempts = 500;
        int placeRoomAttempts = 20;
        int generateCellularAttempts = 10;

        public bool includeShortcuts = true;
        int shotcutAttempts = 500;
        int shortcutLength = 5;
        int minPathfindingDistance = 50;

        Random random;


        Level level;
        public Level LVL { get { return level; } }

        public bool Visualise { get; set; }
        public bool Paused { get; set; }
        public bool Updated { get; set; }
        void Pause(int ms = 250)
        {
            //Paused = true;
            Updated = true;
            if (Visualise)
            {
                Thread.Sleep(ms);
                while (Paused)
                {
                    Thread.Sleep(10);
                }
            }
        }
        List<Point> _highlighted = new List<Point>();
        public List<Point> HighlightedTiles { get { return _highlighted; } }

        public Generator()
        {
            random = new Random();
        }
        public Generator(int seed)
        {
            random = new Random(seed);
        }
        public void SetSeed(int seed)
        {
            random = new Random(seed);
        }


        public void GenerateLevel(int width, int height)
        {
            level = new Level(width, height);

            // center room
            Room room = GenerateRoom();
            level.AddRoom(room, width / 2 - room.Width / 2, height / 2 - room.Height / 2);

            Pause();

            // other rooms
            for (int i = 0; i < buildRoomAttempts; i++)
            {
                room = GenerateRoom();
                if (PlaceRoom(room, level, out int roomX, out int roomY, out Point wallTile, out Point direction))
                {
                    level.AddRoom(room, roomX, roomY, wallTile, direction);
                    //AddTunnel(level, wallTile, direction);
                    if (level.Rooms.Count >= maxRoomCount)
                        break;
                    
                    Pause();
                }
            }

            Pause();
            if (includeShortcuts)
                AddShortcuts(level);


            // return new Area(tiles);
        }

        Point RandomDirection(Direction directionFlags)
        {
            if (directionFlags == Direction.Zero)
                return Directions[random.Next(0, 4)];

            List<int> dirsIndex = new List<int>(4);
            if (directionFlags.HasFlag(Direction.North))
                dirsIndex.Add(0);
            if (directionFlags.HasFlag(Direction.East))
                dirsIndex.Add(1);
            if (directionFlags.HasFlag(Direction.South))
                dirsIndex.Add(2);
            if (directionFlags.HasFlag(Direction.West))
                dirsIndex.Add(3);

            return Directions[dirsIndex[random.Next(0, dirsIndex.Count)]];
        }

        bool PlaceRoom(Room room, Level level, out int roomX, out int roomY, out Point wallTile, out Point direction)
        {
            // compile a list of wall tiles that have a wall on one side and a floor on the other 
            List<Tuple<Point, Direction>> wallTileCandidates = new List<Tuple<Point, Direction>>();
            Direction[] dirTable = new Direction[] { Direction.North, Direction.East, Direction.South, Direction.West };
            for (int x = 1; x < level.Width - 1; x++)
            {
                for (int y = 1; y < level.Height - 1; y++)
                {
                    if (level[x, y] == TileType.Wall)
                    {
                        Direction dir = Direction.Zero;
                        for (int d = 0; d < 4; d++)
                        {
                            Point searchDir = Directions[d];
                            if (level[x + searchDir.X, y + searchDir.Y] == TileType.Wall && level[x - searchDir.X, y - searchDir.Y] == TileType.Floor)
                            {
                                if (dir != Direction.Zero) // corner wall with multiple directions, not good candidate
                                {
                                    dir = Direction.Zero;
                                    break;
                                }
                                //dir |= dirTable[d];
                                dir = dirTable[d];
                            }
                        }
                        if (dir != Direction.Zero)
                            wallTileCandidates.Add(new Tuple<Point, Direction>(new Point(x, y), dir));
                    }
                }
            }

            for (int i = 0; i < placeRoomAttempts; i++)
            {
                // get random wall tile and its direction
                Tuple<Point, Direction> wallTileC = wallTileCandidates[random.Next(0, wallTileCandidates.Count)];
                wallTile = wallTileC.Item1;
                direction = DirectionToVector[wallTileC.Item2];

                _highlighted = new List<Point>() { wallTile };
                Pause();

                // offset room origin (top left corner)
                if (direction.X > 0)
                    roomX = wallTile.X;
                else if (direction.X < 0)
                    roomX = wallTile.X - (room.Width - 1); // by room width - 1 (right wall padding)
                else
                    roomX = wallTile.X - (room.Width / 2); // center the room

                if (direction.Y > 0)
                    roomY = wallTile.Y;
                else if (direction.Y < 0)
                    roomY = wallTile.Y - (room.Height - 1); // by room height - 1 (top wall padding)
                else
                    roomY = wallTile.Y - (room.Height / 2);

                _highlighted.Add(new Point(roomX, roomY));
                Pause();

                // verify that its not overlaping with other rooms or over the edge of the level
                if (VerifyRoomPosition(room, level, roomX, roomY))
                {
                    return true;
                }
            }

            roomX = 0;
            roomY = 0;
            wallTile = Point.Zero;
            direction = Point.Zero;
            return false;
        }

        bool VerifyRoomPosition(Room room, Level level, int roomX, int roomY)
        {
            if (roomX <= 0 || roomY <= 0)
                return false;

            for (int x = 1; x < room.Width - 1; x++)
            {
                for (int y = 1; y < room.Height - 1; y++)
                {
                    if (room[x, y] == TileType.Floor)
                    {
                        int levelX = roomX + x;
                        int levelY = roomY + y;
                        if (levelX >= level.Width - 1 || levelY >= level.Height - 1)
                            return false;

                        if ((level[levelX - 1, levelY - 1] == TileType.Floor) ||
                            (level[levelX    , levelY - 1] == TileType.Floor) ||
                            (level[levelX + 1, levelY - 1] == TileType.Floor) ||

                            (level[levelX - 1, levelY    ] == TileType.Floor) ||
                            (level[levelX    , levelY    ] == TileType.Floor) ||
                            (level[levelX + 1, levelY    ] == TileType.Floor) ||

                            (level[levelX - 1, levelY + 1] == TileType.Floor) ||
                            (level[levelX    , levelY + 1] == TileType.Floor) ||
                            (level[levelX + 1, levelY + 1] == TileType.Floor))
                            return false;
                    }
                }
            }
            return true;
        }

        void AddShortcuts(Level level)
        {
            /*
            QuadtreeNode quadTree = new QuadtreeNode(0, 0, level.Width, level.Height);
            foreach (Room room in level.Rooms)
            {
                quadTree.Insert(room);
            }*/
            int maxDistance = 10;
            BitArray walls = new BitArray(level.Width * level.Height);
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    walls[x + y * level.Width] = (level[x, y] == TileType.Wall);
                }
            }

            // TODO: check for room horizontal/vertical overlap and only make straight tunnels
            // check that center tile is floor
            List<Room> roomsToCheck = level.Rooms.ToList();

            for (int i = roomsToCheck.Count - 1; i >= 0; i--)
            {
                Room room = roomsToCheck[i];
                for (int j = 0; j < roomsToCheck.Count; j++)
                {
                    Room otherRoom = roomsToCheck[j];
                    if (room == otherRoom)
                        continue;

                    int distLeft = room.Position.X - (otherRoom.Position.X + otherRoom.Width); // number of tiles between left wall of room and right wall of other room
                    int distRight = otherRoom.Position.X - (room.Position.X + room.Width); // number of tiles between right wall of room and left wall of other room

                    int distTop = room.Position.Y - (otherRoom.Position.Y + otherRoom.Height); // number of tiles between top wall of room and bottom wall of other room
                    int distBottom = otherRoom.Position.Y - (room.Position.Y + room.Height); // number of tiles between bottom wall of room and top wall of other room

                    Point tunnelDirection;
                    Point tunnelStart;

                    //TODO: if all distances are negative the rooms are overlaping - choose the best direction based on center-center 

                    if (distLeft <= -3 && distRight <= -3) // overlap of at least 3 tiles (2 walls 1 floor) on the X axis (other room above or below room)
                    {
                        if (distTop > maxDistance || distBottom > maxDistance) // too far up or down
                            continue;

                        int leftX = Math.Max(room.Position.X, otherRoom.Position.X) + 1; // minimum X to consider is the maximum (rightmost) of the two rooms + 1 (wall)
                        int rightX = Math.Min(room.Position.X + room.Width, otherRoom.Position.X + otherRoom.Width) - 2; // maximum X to consider is the minimum (leftmost) of the two rooms's rightmost row X coordinates - 2 (1 to offset width to get to the rightmost row & 1 for the wall)
                        int y;

                        if (distTop > distBottom) // other room is above the room                        
                        {
                            tunnelDirection = DirectionToVector[Direction.North];
                            y = room.Position.Y; // top wall
                        }
                        else // else other room is below
                        {
                            tunnelDirection = DirectionToVector[Direction.South];
                            y = room.Position.Y + room.Height - 1; // bottom wall
                        }
                        tunnelStart = new Point(random.Next(leftX, rightX + 1), y);
                    }
                    else if (distTop <= -3 && distBottom <= -3) // overlap of at least 3 tiles (2 walls 1 floor) on the Y axis (other room to the left or to the right of room)
                    {
                        if (distLeft > maxDistance || distRight > maxDistance) // too far to the left or right
                            continue;

                        int topY = Math.Max(room.Position.Y, otherRoom.Position.Y) + 1; // minimum Y to consider is the maximum (bottommost) of the two rooms + 1 (wall)
                        int bottomY = Math.Min(room.Position.Y + room.Height, otherRoom.Position.Y + otherRoom.Height) - 2; // maximum Y to consider is the minimum (topmost) of the two rooms's bottom row Y coordinates - 2 (1 to get to the wall tile & 1 for the wall)
                        int x;

                        if (distLeft > distRight) // other room is to the left of the room  
                        {
                            tunnelDirection = DirectionToVector[Direction.West];
                            x = room.Position.X; // left wall
                        }
                        else // else other room is to the right
                        {
                            tunnelDirection = DirectionToVector[Direction.East];
                            x = room.Position.X + room.Width - 1; // right wall
                        }
                        tunnelStart = new Point(x, random.Next(topY, bottomY + 1));

                    }
                    else
                        continue;

                    // find floor in room A
                    _highlighted = new List<Point>() { tunnelStart };
                    Pause(100);

                    Point dirOpposite = new Point(tunnelDirection.X * -1, tunnelDirection.Y * -1);
                    Point roomCoords = tunnelStart - room.Position;
                    Point searchStart = tunnelStart;
                    while (room[roomCoords.X, roomCoords.Y] != TileType.Floor)
                    {
                        tunnelStart += dirOpposite;
                        roomCoords += dirOpposite;

                        _highlighted.Add(tunnelStart);
                        Pause(100);
                    }

                    _highlighted = new List<Point>() { tunnelStart };
                    Pause(500);

                    // find floor in room B
                    roomCoords = tunnelStart - otherRoom.Position;
                    while (roomCoords.X < 0 || roomCoords.X >= otherRoom.Width || roomCoords.Y < 0 || roomCoords.Y >= otherRoom.Height)
                    {
                        roomCoords += tunnelDirection;

                        _highlighted.Add(roomCoords + otherRoom.Position);
                        Pause(100);
                    }
                    Point tunnelEnd = roomCoords + otherRoom.Position;

                    _highlighted = new List<Point>() { tunnelStart, tunnelEnd };
                    Pause(500);

                    //while (tunnelEnd.X > 0 && tunnelEnd.X < level.Width - 1 && tunnelEnd.Y > 0 && tunnelEnd.Y < level.Height - 1 && level[tunnelEnd.X, tunnelEnd.Y] != TileType.Floor)
                    while (otherRoom[roomCoords.X, roomCoords.Y] != TileType.Floor)
                    {
                        tunnelEnd += tunnelDirection;
                        roomCoords += tunnelDirection;

                        // Check if somehow the coords are out of bounds
                        /*if (tunnelEnd.X <= 0 || tunnelEnd.X >= level.Width - 1 || tunnelEnd.Y <= 0 || tunnelEnd.Y >= level.Height - 1
                            || roomCoords.X < 0 || roomCoords.X >= otherRoom.Width || roomCoords.Y < 0 || roomCoords.Y >= otherRoom.Height)
                        {
                            faulty = true;
                            break;
                        }*/

                        _highlighted.Add(tunnelEnd);
                        Pause(100);
                    }

                    _highlighted = new List<Point>() { tunnelStart, tunnelEnd };
                    Pause(500);

                    // check for connection
                    var path = Pathfinding.AStar.GetAPath(tunnelStart, tunnelEnd, walls, level.Width, maxDistance * 5);

                    _highlighted = new List<Point>();
                    for (int p = path.Count - 1; p >= 0; p--)
                    {
                        _highlighted.Add(path[p]);
                        Pause(50);
                    }
                    Console.WriteLine("path len: {0}", path.Count);
                    Pause(1000);
                    _highlighted.Clear();

                    if (path.Count == 0)
                    {
                        // add tunnel
                        while (!tunnelStart.Equals(tunnelEnd))
                        {
                            level[tunnelStart.X, tunnelStart.Y] = TileType.Floor;
                            tunnelStart += tunnelDirection;
                        }
                    }
                }
                roomsToCheck.RemoveAt(i);

                /*
                Point roomCenter = room.Position + new Point(room.Width / 2, room.Height / 2);
                //Rectangle2D searchRect = new Rectangle2D(room.Position.X - searchDistance, room.Position.Y - searchDistance, room.Width + searchDistance, room.Height + searchDistance);
                Circle searchCircle = new Circle(roomCenter.ToVector2(), Math.Max(room.Width, room.Height) + searchDistance);
                List<IQuadtreeStorable> roomList = quadTree.GetObjects(searchCircle);

                foreach (var result in roomList)
                {
                    Room target = result as Room;
                    if (target != null && target != room)
                    {
                        Point targetCenter = target.Position + new Point(target.Width / 2, target.Height / 2);
                        var path = Pathfinding.AStar.GetAPath(roomCenter, targetCenter, walls, level.Width, searchDistance * 4);
                        if (path.Count == 0)
                        {
                            CreateShortcut(room, target, level);
                        }
                    }
                }*/
            }
        }

        void CreateTunnel(Level level, Room a, Room b, Point start, Point direction)
        {
            // find floor of room A
            Point dirOpposite = new Point(direction.X * -1, direction.Y * -1);
            while (level[start.X, start.Y] != TileType.Floor)
            {
                start += dirOpposite;
            }
            start += direction; // 1st wall next to the room floor
            a.Entrances.Add(start);

            // make tunnel to floor of room B
            while (level[start.X, start.Y] != TileType.Floor)
            {
                level[start.X, start.Y] = TileType.Floor;
                start += direction;
            }
            start += dirOpposite;
            b.Entrances.Add(start);
        }

        void CreateShortcut(Room a, Room b, Level level)
        {
            Point centerA = a.Position + new Point(a.Width / 2, a.Height / 2);
            Point centerB = b.Position + new Point(b.Width / 2, b.Height / 2);

            var points = Pathfinding.LOS.GetPointsOnLine(centerA.X, centerA.Y, centerB.X, centerB.Y);
            foreach (var p in points)
            {
                level[p.X, p.Y] = TileType.Floor;
            }
        }


        Room GenerateRoom()
        {
            double chance = random.NextDouble();
            if (chance < crossRoomChance)
                return GenerateRoomCross();
            if (chance < crossRoomChance + squareRoomChance)
                return GenerateRoomSquare();

            return GenerateRoomCelular();
        }

        Room GenerateRoomSquare()
        {
            int width = random.Next(squareRoomMinSize, squareRoomMaxSize + 1);
            int height = random.Next(Math.Max(squareRoomMinSize, width / 2), Math.Min(squareRoomMaxSize, width + width / 2) + 1);
            TileType[,] room = new TileType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                        room[x, y] = TileType.Wall; 
                    else
                        room[x, y] = TileType.Floor;
                }
            }
            return new Room(room);
        }
        Room GenerateRoomCross()
        {
            int width = random.Next(crossRoomMinSize, crossRoomMaxSize + 1);
            int height = random.Next(crossRoomMinSize, crossRoomMaxSize + 1);

            int xOffset = random.Next(2, width / 2);
            int yOffset = random.Next(2, height / 2);

            TileType[,] room = new TileType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1
                        || (x < xOffset && (y < yOffset || y > height - yOffset - 1)) || (x > width - xOffset - 1 && (y < yOffset || y > height - yOffset - 1))
                        )
                        room[x, y] = TileType.Wall; 
                    else
                        room[x, y] = TileType.Floor; 
                }
            }
            return new Room(room);
        }

        Room GenerateRoomCelular()
        {
            int width = random.Next(celularRoomMinSize, celularRoomMaxSize + 1);
            int height = random.Next(celularRoomMinSize, celularRoomMaxSize + 1); ;
            TileType[,] room = new TileType[width, height];

            int counter = 0;
            while (true)
            {
                counter++;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        room[x, y] = TileType.Wall;
                        if (x > 0 && x < width - 1 && y > 0 && y < height - 1 && random.NextDouble() < celularStartFloorProbability)
                        {
                            room[x, y] = TileType.Floor;
                        }
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            int adjWallCount = CountAdjacentWalls(x, y, room);
                            if (adjWallCount < celularAdjWallThreshold)
                                room[x, y] = TileType.Floor;
                            else if (adjWallCount > celularAdjWallThreshold)
                                room[x, y] = TileType.Wall;
                        }
                    }
                }

                int count = FloodFill(room);

                if (count > 0)
                {
                    Rectangle roomSize = GetRoomFloorSize(room);
                    TileType[,] newTiles = new TileType[roomSize.Width + 2, roomSize.Height + 2];
                    // loop trough area containing floor tiles and add them to a new room floor plan with 1 tile wall padding
                    for (int x = 0; x < roomSize.Width; x++)
                    {
                        for (int y = 0; y < roomSize.Height; y++)
                        {
                            newTiles[x + 1, y + 1] = room[roomSize.X + x, roomSize.Y + y];
                        }
                    }

                    return new Room(newTiles);
                }
            }
        }

        // Find the largest region and fill all other regions
        int FloodFill(TileType[,] room)
        {
            int width = room.GetLength(0);
            int height = room.GetLength(1);

            List<Point> largestRegion = new List<Point>();
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (room[x, y] == TileType.Floor)
                    {
                        List<Point> newRegion = new List<Point>();
                        Stack<Point> toBeFilled = new Stack<Point>();
                        toBeFilled.Push(new Point(x, y));
                        while (toBeFilled.Count > 0)
                        {
                            Point tile = toBeFilled.Pop();
                            if (!newRegion.Contains(tile))
                            {
                                newRegion.Add(tile);
                                room[tile.X, tile.Y] = TileType.Wall; // set room tile to wall to mark it as processed
                                
                                // north
                                Point adjPoint = new Point(tile.X, tile.Y - 1); 
                                if (room[adjPoint.X, adjPoint.Y] == TileType.Floor && !toBeFilled.Contains(adjPoint))
                                    toBeFilled.Push(adjPoint);

                                // south
                                adjPoint = new Point(tile.X, tile.Y + 1);
                                if (room[adjPoint.X, adjPoint.Y] == TileType.Floor && !toBeFilled.Contains(adjPoint))
                                    toBeFilled.Push(adjPoint);

                                // west
                                adjPoint = new Point(tile.X - 1, tile.Y);
                                if (room[adjPoint.X, adjPoint.Y] == TileType.Floor && !toBeFilled.Contains(adjPoint))
                                    toBeFilled.Push(adjPoint);

                                // east
                                adjPoint = new Point(tile.X + 1, tile.Y);
                                if (room[adjPoint.X, adjPoint.Y] == TileType.Floor && !toBeFilled.Contains(adjPoint))
                                    toBeFilled.Push(adjPoint);
                            }
                        }

                        if (newRegion.Count > largestRegion.Count)
                            largestRegion = newRegion;
                    }
                }
            }

            for (int i = 0; i < largestRegion.Count; i++)
                room[largestRegion[i].X, largestRegion[i].Y] = TileType.Floor;

            return largestRegion.Count;
        }

        Rectangle GetRoomFloorSize(TileType[,] room)
        {
            int roomWidth = room.GetLength(0);
            int roomHeight = room.GetLength(1);
            bool[] colFloor = new bool[roomWidth]; // which columns (same X) have tiles (in any row Y)
            bool[] rowFloor = new bool[roomHeight]; // which rows (same Y) have tiles (in any column X)
            for (int x = 1; x < roomWidth - 1; x++)
            {
                for (int y = 1; y < roomHeight - 1; y++)
                {
                    if (room[x, y] == TileType.Floor)
                    {
                        colFloor[x] = true;
                        rowFloor[y] = true;
                    }
                }
            }

            int startX = 0;
            int endX = 0;
            int startY = 0;
            int endY = 0;
            for (int i = 0; i < roomWidth; i++)
            {
                if (colFloor[i] && startX == 0)
                {
                    startX = i;
                }
                else if (!colFloor[i] && startX != 0)
                {
                    endX = i;
                    break;
                }
            }
            for (int i = 0; i < roomHeight; i++)
            {
                if (rowFloor[i] && startY == 0)
                {
                    startY = i;
                }
                else if (!rowFloor[i] && startY != 0)
                {
                    endY = i;
                    break;
                }
            }

            return new Rectangle(startX, startY, endX - startX, endY - startY);
        }

        int CountAdjacentWalls(int x, int y, TileType[,] room)
        {
            int count = 0;
            for (int search_x = x - 1; search_x <= x + 1; search_x++)
            {
                for (int search_y = y - 1; search_y <= y + 1; search_y++)
                {
                    if (search_x == x && search_y == y)
                        continue;
                    if (room[search_x, search_y] == TileType.Wall)
                        count += 1;
                }
            }
            return count;
        }

        static Vector2 RandomPointInCircle(Random random, float radius)
        {
            double theta = 2 * Math.PI * random.NextDouble();
            double u = random.NextDouble() + random.NextDouble();
            double r = (u > 1) ? 2 - u : u;
            return new Vector2((float)(radius * r * Math.Cos(theta)), (float)(radius * r * Math.Sin(theta)));
        }

    }
}
