using InanZEngine.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Carter.Level
{
    static class LevelSprites
    {
        static readonly string[] directionNames = new string[]
        {
            "zero", // 0
            "north", // 1
            "east", // 2
            "northeast", // 3
            "south", // 4
            "northsouth", // 5
            "eastsouth", // 6 
            "northeastsouth", // 7
            "west", // 8
            "northwest", // 9
            "eastwest", // 10
            "northeastwest", // 11
            "southwest", // 12
            "northsouthwest", // 13
            "eastsouthwest", // 14
            "northeastsouthwest" // 15
        };
        static readonly Point[] offsets = new Point[]
        {
            new Point(16, 16), // "zero", // 0
            new Point(16, 0), // "north", // 1
            new Point(2*16, 16), // "east", // 2
            new Point(2*16, 0), // "northeast", // 3
            new Point(16, 2*16), // "south", // 4
            new Point(5*16, 16), // "northsouth", // 5
            new Point(2*16, 2*16), // "eastsouth", // 6 
            new Point(6*16, 16), // "northeastsouth", // 7
            new Point(0, 16), // "west", // 8
            new Point(0, 0), // "northwest", // 9
            new Point(3*16, 16), // "eastwest", // 10
            new Point(3*16, 0), // "northeastwest", // 11
            new Point(0, 2*16), // "southwest", // 12
            new Point(4*16, 16), // "northsouthwest", // 13
            new Point(3*16, 2*16), // "eastsouthwest", // 14
            new Point(5*16, 0) // "northeastsouthwest" // 15
        };
               
        static public DynamicSpriteSheet SpriteSheet { get; set; }
        static public Dictionary<string, SpriteDefinition[]> SpriteDefinitions { get; private set; }

        public static void Initialise(Texture2D texture)
        {
            GenerateDefinitions();
            List<SpriteDefinition> definitions = new List<SpriteDefinition>();
            foreach (var sd in SpriteDefinitions)
            {
                foreach (var d in sd.Value)
                    definitions.Add(d);
            }

            SpriteSheet = new DynamicSpriteSheet(texture, definitions.ToArray());
        }

        static void GenerateDefinitions()
        {
            SpriteDefinitions = new Dictionary<string, SpriteDefinition[]>();

            Tuple<string, Point>[] floors = new Tuple<string, Point>[]
            {
                new Tuple<string, Point>("floor_stone_lightgray", new Point(0, 3*16)),
                new Tuple<string, Point>("floor_stone_gray", new Point(0, 3*16 * 2)),
                new Tuple<string, Point>("floor_stone_darkgray", new Point(0, 3*16 * 3)),
                new Tuple<string, Point>("floor_wood_light", new Point(7*16, 3*16)),
                new Tuple<string, Point>("floor_wood_gray", new Point(7*16, 3*16 * 2)),
                new Tuple<string, Point>("floor_wood_dark", new Point(7*16, 3*16 * 3)),
                new Tuple<string, Point>("floor_grass", new Point(0, 3*16 * 4)),
                new Tuple<string, Point>("floor_rock", new Point(0, 3*16 * 5)),
                new Tuple<string, Point>("floor_dirt_light", new Point(7*16, 3*16 * 4)),
                new Tuple<string, Point>("floor_dirt_dark", new Point(7*16, 3*16 * 5))
            };

            foreach (var floor in floors)
            {
                SpriteDefinition[] definitions = new SpriteDefinition[16];
                for (int i = 0; i < definitions.Length; i++)
                {
                    definitions[i] = new SpriteDefinition(String.Format("{0}_{1}", floor.Item1, directionNames[i]), new Rectangle(floor.Item2.X + offsets[i].X, floor.Item2.Y + offsets[i].Y, 16, 16));
                }
                SpriteDefinitions.Add(floor.Item1, definitions);
            }
        }
    }
}
