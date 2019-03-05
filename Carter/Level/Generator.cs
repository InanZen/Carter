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

            public void AddRoom(Room room, Point position)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    for (int y = 0; y < room.Height; y++)
                    {
                        if (room[x, y] == TileType.Floor)
                            _tiles[position.X + x, position.Y + y] = TileType.Floor;
                    }
                }
                room.Position = position;
                room.Index = _rooms.Count;
                _rooms.Add(room);
            }
            public void AddRoom(Room roomTo, Point position, Room roomFrom, Point tileFrom, Point directionTo)
            {
                AddRoom(roomTo, position);
                CreateTunnel(roomFrom, roomTo, tileFrom, directionTo);
            }

            void CreateTunnel(Room a, Room b, Point start, Point direction)
            {
                // find floor of room A
                while (_tiles[start.X, start.Y] != TileType.Floor)
                {
                    start -= direction;
                }
                start += direction; // 1st wall next to the room floor
                a.Connections.Add(b, start);

                // make tunnel to floor of room B
                while (_tiles[start.X, start.Y] != TileType.Floor)
                {
                    _tiles[start.X, start.Y] = TileType.Floor;
                    start += direction;
                }
                start -= direction;
                b.Connections.Add(a, start);
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
       /* public struct RoomConnection
        {
            public Point Entrance;
            public List<Room> Connections;
            public RoomConnection(Point entrance, Room connection)
            {
                Entrance = entrance;
                Connections = new List<Room>() { connection };
            }
        }
        */
        public class Room
        {
            readonly TileType[,] _tiles;
            readonly int _width;
            readonly int _height;
            //readonly List<Point> _entrances;
            Dictionary<Room, Point> _connections;
            Point _position;
            //Rectangle2D _posRect;
            Rectangle _roomRect;
            
            public TileType[,] Tiles { get { return _tiles; } }
            public TileType this[int x, int y] { get { return _tiles[x, y]; } }
            public int Width { get { return _width; } }
            public int Height { get { return _height; } }
            public Dictionary<Room, Point>.ValueCollection Entrances { get { return _connections.Values; } }
            public Dictionary<Room, Point>.KeyCollection ConnectingRooms { get { return _connections.Keys; } }
            public Dictionary<Room, Point> Connections { get { return _connections; } }

            public int Index { get; set; }
            public Point Position
            {
                get { return _position; }
                set
                {
                    _position = value;
                    _roomRect.Location = _position;
                    //_posRect = new Rectangle2D(_position.X, _position.Y, _width, _height);
                }
            }
            public Rectangle Rectangle { get { return _roomRect; } }

            //QuadtreeNode IQuadtreeStorable.TreeNode { get; set; }

            public Room(TileType[,] tiles)
            {
                _tiles = tiles;
                _width = tiles.GetLength(0);
                _height = tiles.GetLength(1);
                _connections = new Dictionary<Room, Point>();
                _roomRect = new Rectangle(0, 0, _width, _height);
            }

            public bool ConnectsTo(Room room)
            {
                return _connections.ContainsKey(room);
            }

            /// <summary>
            /// Number of rooms between this room and the target room
            /// </summary>
            /// <param name="room">the target room</param>
            public int DistanceTo(Room room)
            {
                if (room == this)
                    return 0;
                return NumHops(room, this);
            }


            static int NumHops(Room toFind, Room current)
            {
                List<Room> checkedRooms = new List<Room>();
                Queue<Tuple<Room, int>> toCheck = new Queue<Tuple<Room, int>>();
                toCheck.Enqueue(new Tuple<Room, int>(current, 1));

                while (toCheck.Count > 0)
                {
                    var q = toCheck.Dequeue();
                    Room checking = q.Item1;
                    int distance = q.Item2;
                    if (checkedRooms.Contains(checking))
                        continue;

                    if (checking.ConnectsTo(toFind))
                        return distance;
                    checkedRooms.Add(checking);

                    foreach (var r in checking.ConnectingRooms)
                    {
                        if (r == checking)
                            continue;
                        toCheck.Enqueue(new Tuple<Room, int>(r, distance + 1));
                    }
                }

                return -1; // shouldnt happen if all rooms are connected to eachother
            }



            /*bool IQuadtreeStorable.IsContained(Rectangle2D container)
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
            }*/
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




#if DEBUG
        public Level LVL { get; private set; }
        public Room PlacingRoom { get; private set; }
        public bool Faulty { get; set; }
        public bool Visualise { get; set; }
        public bool Paused { get; set; }
        public bool Updated { get; set; }
        void Pause(int ms = 250)
        {
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
#endif

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


        public Level GenerateLevel(int width, int height)
        {
            Level level = new Level(width, height);
#if DEBUG
            LVL = level;
#endif

            // center room
            Room room = GenerateRoom();
            level.AddRoom(room, new Point(width / 2 - room.Width / 2, height / 2 - room.Height / 2));
#if DEBUG
            Pause();
#endif

            // other rooms
            for (int i = 0; i < buildRoomAttempts; i++)
            {
                room = GenerateRoom();
#if DEBUG
                PlacingRoom = room;
#endif
                if (PlaceRoom(room, level))
                {
#if DEBUG
                    Pause();
#endif
                    if (level.Rooms.Count >= maxRoomCount)
                        break;
                }
            }

            // Add shortcuts
            if (includeShortcuts)
                AddShortcuts(level);

            Pause();
            return level;
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

        bool PlaceRoom(Room room, Level level)
        {
            // compile a list of wall tiles that have a wall on one side and a floor on the other 
            List<Tuple<Room, Point, Direction>> wallTileCandidates = new List<Tuple<Room, Point, Direction>>();
            Direction[] dirTable = new Direction[] { Direction.North, Direction.East, Direction.South, Direction.West };
            foreach (Room r in level.Rooms)
            {
                for (int x = 0; x < r.Width; x++)
                {
                    for (int y = 0; y < r.Height; y++)
                    {
                        if (r[x, y] == TileType.Wall)
                        {
                            Direction dir = Direction.Zero;
                            Point position = new Point(r.Position.X + x, r.Position.Y + y);
                            if (position.X <= 0 || position.X >= level.Width - 1 || position.Y <= 0 || position.Y >= level.Height - 1)
                                continue;
                            for (int d = 0; d < 4; d++)
                            {
                                Point searchDir = Directions[d];
                                //if (level[position.X + searchDir.X, position.Y + searchDir.Y] == TileType.Wall && level[position.X - searchDir.X, position.Y - searchDir.Y] == TileType.Floor)
                                if (x - searchDir.X >= 0 && x - searchDir.X < r.Width && y - searchDir.Y >= 0 && y - searchDir.Y < r.Height
                                    && r[x - searchDir.X, y - searchDir.Y] == TileType.Floor
                                    && level[position.X + searchDir.X, position.Y + searchDir.Y] == TileType.Wall)
                                {
                                    if (dir != Direction.Zero) // corner wall with multiple directions, not good candidate
                                    {
                                        dir = Direction.Zero;
                                        break;
                                    }
                                    dir = dirTable[d];
                                }
                            }
                            if (dir != Direction.Zero)
                                wallTileCandidates.Add(new Tuple<Room, Point, Direction>(r, position, dir));
                        }
                    }
                }
            }

            /*for (int x = 1; x < level.Width - 1; x++)
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
            }*/

            int roomX = 0;
            int roomY = 0;
            for (int i = 0; i < placeRoomAttempts; i++)
            {
                // get random wall tile and its direction
                Tuple<Room, Point, Direction> wallTileC = wallTileCandidates[random.Next(0, wallTileCandidates.Count)];
                Point wallTile = wallTileC.Item2;
                Point direction = DirectionToVector[wallTileC.Item3];
#if DEBUG
                 _highlighted = new List<Point>() { wallTile };
                 Pause();
#endif

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
#if DEBUG
                 _highlighted.Add(new Point(roomX, roomY));
                  Pause();
#endif

                // verify that its not overlaping with other rooms or over the edge of the level
                if (VerifyRoomPosition(room, level, roomX, roomY))
                {
                    // add room to the level
                    level.AddRoom(room, new Point(roomX, roomY), wallTileC.Item1, wallTile, direction);
                    return true;
                }
            }
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
            int maxTunnelLength = 10;
            int maxPathLength = 50;
            int minRoomsBetween = 3;
            BitArray walls = new BitArray(level.Width * level.Height);
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    walls[x + y * level.Width] = (level[x, y] == TileType.Wall);
                }
            }

            List<Room> roomsToCheck = level.Rooms.ToList();
            for (int i = roomsToCheck.Count - 1; i >= 0; i--)
            {
                Room room = roomsToCheck[i];
                for (int j = roomsToCheck.Count - 2; j >= 0; j--)
                {
                    Room otherRoom = roomsToCheck[j];
                    if (room.ConnectsTo(otherRoom))
                        continue;
                    if (room.DistanceTo(otherRoom) <= minRoomsBetween)
                        continue;

                    int distLeft = room.Position.X - (otherRoom.Position.X + otherRoom.Width); // number of tiles between left wall of room and right wall of other room
                    int distRight = otherRoom.Position.X - (room.Position.X + room.Width); // number of tiles between right wall of room and left wall of other room

                    int distTop = room.Position.Y - (otherRoom.Position.Y + otherRoom.Height); // number of tiles between top wall of room and bottom wall of other room
                    int distBottom = otherRoom.Position.Y - (room.Position.Y + room.Height); // number of tiles between bottom wall of room and top wall of other room
                    
                    bool alignedYaxis = false;

                    if (distLeft <= -3 && distRight <= -3) // overlap of at least 3 tiles (2 walls 1 floor) on the X axis (other room above or below room)
                    {
                        if (distTop > maxTunnelLength - 2 || distBottom > maxTunnelLength - 2) // too far up or down
                            continue;

                        if (distTop <= -3 && distBottom <= -3) // also overlaps on Y axis - use different method
                        {
                            Point center1 = room.Position + new Point(room.Width / 2, room.Height / 2);
                            Point center2 = otherRoom.Position + new Point(otherRoom.Width / 2, otherRoom.Height / 2);
                            Point c1toc2 = center2 - center1;
                            if (Math.Abs(c1toc2.X) > Math.Abs(c1toc2.Y)) // other room is to the left/right of this room
                            {
                                alignedYaxis = true;
                            }
                        }
                    }
                    else if (distTop <= -3 && distBottom <= -3) // overlap of at least 3 tiles (2 walls 1 floor) on the Y axis (other room to the left or to the right of room)
                    {
                        if (distLeft > maxTunnelLength - 2 || distRight > maxTunnelLength - 2) // too far to the left or right
                            continue;

                        alignedYaxis = true;
                    }
                    else // other room not aligned on any axis
                        continue;


                    List<Point> startPoints;
                    Point tunnelDirection;

                    if (alignedYaxis) 
                    {
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
                        startPoints = new List<Point>(bottomY - topY);
                        for (int sp = topY; sp <= bottomY; sp++)
                        {
                            startPoints.Add(new Point(x, sp));
                        }
                    }
                    else
                    {
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

                        startPoints = new List<Point>(rightX - leftX);
                        for (int sp = leftX; sp <= rightX; sp++)
                        {
                            startPoints.Add(new Point(sp, y));
                        }
                    }

                    Console.WriteLine("StartPoints between {0} and {1} count: {2}", room.Index, otherRoom.Index, startPoints.Count);

                    Point tunnelStart, tunnelEnd;
                    while (startPoints.Count > 0)
                    {
                        int spIndex = random.Next(0, startPoints.Count);
                        tunnelStart = startPoints[spIndex];
                        if (IsTunnelViable(level, room, otherRoom, tunnelStart, tunnelDirection, out tunnelStart, out tunnelEnd, out int tunnelLength) && tunnelLength <= maxTunnelLength)
                        {
#if DEBUG
                            _highlighted = new List<Point>() { tunnelStart, tunnelEnd };
                            Pause(500);
#endif

                            // check for connection
                            // start and end should be walls - set them ass non-collision for pathifinding

                           /* Console.WriteLine("Room {0} to {1}", room.Index, otherRoom.Index);
                            Console.WriteLine("start:{0}  end:{1}", tunnelStart, tunnelEnd);
                            Console.WriteLine("walls[start]:{0}  walls[end]:{1}", walls[tunnelStart.X + tunnelStart.Y * level.Width], walls[tunnelEnd.X + tunnelEnd.Y * level.Width]);
*/
                            int wallStartIndex = tunnelStart.X + tunnelStart.Y * level.Width;
                            int wallEndIndex = tunnelEnd.X + tunnelEnd.Y * level.Width;
                            bool wallStartOrig = walls[wallStartIndex];
                            bool wallEndOrig = walls[wallEndIndex];
                            walls[wallStartIndex] = false;
                            walls[wallEndIndex] = false;
                            var path = Pathfinding.AStar.GetAPath(tunnelStart, tunnelEnd, walls, level.Width, maxPathLength);
                            walls[wallStartIndex] = wallStartOrig;
                            walls[wallEndIndex] = wallEndOrig;
                            //Console.WriteLine(" after walls[start]:{0}  walls[end]:{1}", walls[tunnelStart.X + tunnelStart.Y * level.Width], walls[tunnelEnd.X + tunnelEnd.Y * level.Width]);

#if DEBUG
                            if (path.Count > 0)
                            {
                                _highlighted = path.ToList();
                                Pause(1000);
                            }
                            _highlighted.Clear();
#endif

                            if (path.Count == 0)
                            {
                                room.Connections.Add(otherRoom, tunnelStart);
                                otherRoom.Connections.Add(room, tunnelEnd);
                                // carve tunnel
                                while (!tunnelStart.Equals(tunnelEnd + tunnelDirection))
                                {
                                    level[tunnelStart.X, tunnelStart.Y] = TileType.Floor;
                                    walls[tunnelStart.X + tunnelStart.Y * level.Width] = false;

                                    tunnelStart += tunnelDirection;
                                }
                            }

                            break;
                        }
                        startPoints.RemoveAt(spIndex);
                    }
                }
                roomsToCheck.RemoveAt(i);
            }
        }

        bool IsTunnelViable(Level level, Room roomA, Room roomB, Point searchStart, Point tunnelDirection, out Point tunnelStart, out Point tunnelEnd, out int tunnelLength)
        {
            //Console.WriteLine("Tunnel: {0} to {1}.", roomA.Index, roomB.Index);
            tunnelStart = searchStart;
            tunnelEnd = searchStart;
            tunnelLength = 0;

            // find entrance in room A
#if DEBUG
            _highlighted = new List<Point>() { tunnelStart };
            Pause(100);
#endif
            Point perpendicular = new Point(-tunnelDirection.Y, tunnelDirection.X);
            Point roomCoords = tunnelStart - roomA.Position;
            while (roomA[roomCoords.X - tunnelDirection.X, roomCoords.Y - tunnelDirection.Y] != TileType.Floor // "forward" is wall
                && roomA[roomCoords.X + perpendicular.X, roomCoords.Y + perpendicular.Y] != TileType.Floor // left is wall
                && roomA[roomCoords.X - perpendicular.X, roomCoords.Y - perpendicular.Y] != TileType.Floor) // right is wall
            {
                tunnelStart -= tunnelDirection;
                roomCoords -= tunnelDirection;
#if DEBUG
                _highlighted = new List<Point>() { tunnelStart };
                Pause(100);
#endif
            }
#if DEBUG
            _highlighted = new List<Point>() { tunnelStart };
            //Console.WriteLine(" -start: {0}", tunnelStart);
            Pause(100);
#endif

            // find edge of room B
            tunnelEnd = tunnelStart;
            roomCoords = tunnelEnd - roomB.Position;
            while (roomCoords.X < 0 || roomCoords.X >= roomB.Width || roomCoords.Y < 0 || roomCoords.Y >= roomB.Height)
            {
                roomCoords += tunnelDirection;
                tunnelEnd += tunnelDirection;
                tunnelLength++;
#if DEBUG
                _highlighted.Add(roomCoords + roomB.Position);
                Pause(100);
#endif

                if (level.Tiles[tunnelEnd.X + perpendicular.X, tunnelEnd.Y + perpendicular.Y] == TileType.Floor
                    || level.Tiles[tunnelEnd.X - perpendicular.X, tunnelEnd.Y - perpendicular.Y] == TileType.Floor)
                {
                    Pause(500);
                    return false;
                }

            }

            // find entrance in room B
            while (roomB[roomCoords.X + tunnelDirection.X, roomCoords.Y + tunnelDirection.Y] != TileType.Floor // forward is wall
                && roomB[roomCoords.X + perpendicular.X, roomCoords.Y + perpendicular.Y] != TileType.Floor // left is wall
                && roomB[roomCoords.X - perpendicular.X, roomCoords.Y - perpendicular.Y] != TileType.Floor) // right is wall
            {
               /* if (level.Tiles[tunnelEnd.X + perpendicular.X, tunnelEnd.Y + perpendicular.Y] == TileType.Floor
                    || level.Tiles[tunnelEnd.X - perpendicular.X, tunnelEnd.Y - perpendicular.Y] == TileType.Floor)
                {
                    Pause(1000);
                    return false;
                }*/

                tunnelEnd += tunnelDirection;
                roomCoords += tunnelDirection;
                tunnelLength++;

                // Check if somehow the coords are out of bounds
                if (tunnelEnd.X <= 0 || tunnelEnd.X >= level.Width - 1 || tunnelEnd.Y <= 0 || tunnelEnd.Y >= level.Height - 1
                    || roomCoords.X < 0 || roomCoords.X >= roomB.Width || roomCoords.Y < 0 || roomCoords.Y >= roomB.Height)
                {
                    return false;
                }
#if DEBUG
                _highlighted.Add(tunnelEnd);
                Pause(100);
#endif
            }


#if DEBUG
            _highlighted = new List<Point>() { tunnelStart, tunnelEnd };
            //Console.WriteLine(" -end: {0}", tunnelEnd);
            Pause(1000);
#endif

            return true;
        }

       /* void CreateTunnel(Level level, Room a, Room b, Point start, Point direction)
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
        }*/


        Room GenerateRoom()
        {
            // TODO
            // - add room type - stepped square/cross
            double chance = random.NextDouble();
            //if (chance < crossRoomChance)
            //    return GenerateRoomCross();
            if (chance < squareRoomChance)
                return GenerateRoomSquare();

            return GenerateRoomCelular();
        }

        Room GenerateRoomSquare()
        { 
            // TODO
            // - adjust chance of elongated squares
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
