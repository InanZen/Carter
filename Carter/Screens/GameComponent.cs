using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Carter.Level;
using InanZEngine;
using System.Threading;
namespace Carter.Screens
{
    class GameComponent : DrawableGameComponent
    {
        ContentManager _content;
        SpriteBatch _spriteBatch;
        SpriteFont _spriteFont;

        Generator _lvlgen;
        Level.Area _area;
        Camera2D _camera;
        KeyboardState _oldKeyState;
        int lvlW = 64;
        int lvlH = 64;
        int seed = 1694537278;
        int maxRooms = 10;
        bool shortcuts = true;
        Random random = new Random();
        Thread genThread;
        Texture2D markers;
        Texture2D whiteSquare;


        public GameComponent(Game game) : base(game)
        {
            _content = new ContentManager(game.Services, "Content");
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = _content.Load<SpriteFont>("PixelOperator");
            markers = _content.Load<Texture2D>("markers");
            whiteSquare = _content.Load<Texture2D>("white");
            

            _camera = new Camera2D(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
            _camera.TranslateTo(new Vector2(lvlW * 16 * 0.5f, lvlH * 16 * 0.5f));

            var tilesTexture = _content.Load<Texture2D>("tiles");
            LevelSprites.Initialise(tilesTexture);

            _lvlgen = new Generator(seed) { includeShortcuts = shortcuts, maxRoomCount = maxRooms, Visualise = false };
            genThread = new Thread(GenerateLevel);
            genThread.Start();

            base.LoadContent();
        }
        void GenerateLevel()
        {
            _lvlgen.SetSeed(seed);
            _lvlgen.GenerateLevel(lvlW, lvlH);

            /* int count = 0;
             while (!_lvlgen.Faulty && count < 100000)
              {
                  _lvlgen.GenerateLevel(lvlW, lvlH);
                  count++;
                  if (count % 10 == 0)
                      Console.WriteLine("count: {0}", count);
              }*/
        }


        protected override void UnloadContent()
        {
            _content.Unload();

            base.UnloadContent();
        }
        protected override void Dispose(bool disposing)
        {
            if (genThread.ThreadState != ThreadState.Stopped)
                genThread.Abort();

            base.Dispose(disposing);
        }

        public override void Update(GameTime gameTime)
        {
            float dT = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyState = Keyboard.GetState();

            Vector2 cam_MoveVector = Vector2.Zero;
            if (keyState.IsKeyDown(Keys.W))
                cam_MoveVector.Y = -1;
            if (keyState.IsKeyDown(Keys.S))
                cam_MoveVector.Y = 1;
            if (keyState.IsKeyDown(Keys.A))
                cam_MoveVector.X = -1;
            if (keyState.IsKeyDown(Keys.D))
                cam_MoveVector.X = 1;

            // Set Camera movements
            if (cam_MoveVector != Vector2.Zero)
            {
                _camera.Translate(cam_MoveVector* 10);
            }

            if (keyState.IsKeyDown(Keys.R) && _oldKeyState.IsKeyUp(Keys.R))
            {
                seed = random.Next();
                Console.WriteLine("seed: {0}", seed);

                if (genThread.ThreadState != ThreadState.Stopped)
                    genThread.Abort();
                genThread = new Thread(GenerateLevel);
                genThread.Start();
               // _lvlgen.GenerateLevel(lvlW, lvlH);
                //_area = new Area(_lvlgen.LVL.GenerateTiles());
            }
            if (keyState.IsKeyDown(Keys.T) && _oldKeyState.IsKeyUp(Keys.T))
            {
                _lvlgen.includeShortcuts = !_lvlgen.includeShortcuts;

                if (genThread.ThreadState != ThreadState.Stopped)
                    genThread.Abort();
                genThread = new Thread(GenerateLevel);
                genThread.Start();
                //_lvlgen.GenerateLevel(lvlW, lvlH);
                //_area = new Area(_lvlgen.LVL.GenerateTiles());
            }

            if (keyState.IsKeyDown(Keys.V) && _oldKeyState.IsKeyUp(Keys.V))
            {
                _lvlgen.Visualise = !_lvlgen.Visualise;

                if (genThread.ThreadState == ThreadState.Stopped)
                {                    
                    genThread = new Thread(GenerateLevel);
                    genThread.Start();
                }
            }
            if (keyState.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
            {
                _lvlgen.Paused = !_lvlgen.Paused;
            }
            if (_lvlgen.Updated)
            {
                _lvlgen.Updated = false;
                if (_lvlgen.LVL != null)
                {
                    _area = new Area(_lvlgen.LVL.GenerateTiles());
                }
            }


            if (keyState.IsKeyDown(Keys.C) && _oldKeyState.IsKeyUp(Keys.C))
            {
                var rooms = _lvlgen.LVL.Rooms;
                for (int i = 0; i < rooms.Count; i++)
                {
                    Console.WriteLine("Connections of room {0}:", rooms[i].Index);
                    foreach (var c in rooms[i].Connections)
                    {
                        Console.WriteLine("  - to room {0} from point {1}", c.Key.Index, c.Value);
                    }

                }
                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = 0; j < rooms.Count; j++)
                    {
                        Console.WriteLine("Number of rooms between {0} and {1} is {2}", rooms[i].Index, rooms[j].Index, rooms[i].DistanceTo(rooms[j]));
                    }
                }
            }


                _oldKeyState = keyState;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            int tileSize = 16;
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _camera.Transform);
            for (int x = 0; x < _area.Width; x++)
            {
                for (int y = 0; y < _area.Height; y++)
                {
                    if (_area[x, y].Visible)
                    {
                        Rectangle destination = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                        _spriteBatch.Draw(LevelSprites.SpriteSheet.Texture, destination, _area[x, y].SpriteDefinition.Source, Color.White);
                    }
                }
            }
            if (_lvlgen.LVL != null)
            {
                Color[] colors = new Color[] { new Color(255, 0, 255, 50), new Color(0, 255, 255, 50), new Color(255, 255, 0, 50) };

                for (int i = 0; i < _lvlgen.LVL.Rooms.Count; i++)
                {
                    var room = _lvlgen.LVL.Rooms[i];
                    _spriteBatch.DrawString(_spriteFont, string.Format("{0}", room.Index), (room.Position + new Point(room.Width / 2, room.Height / 2)).ToVector2() * new Vector2(tileSize, tileSize), Color.Black);
                }

                for (int i = 0; i < _lvlgen.HighlightedTiles.Count; i++)
                {
                    var tile = _lvlgen.HighlightedTiles[i];
                    Rectangle destination = new Rectangle(tile.X * tileSize, tile.Y * tileSize, tileSize, tileSize);
                    //_spriteBatch.Draw(markers, destination, new Rectangle(32, (i % 4) * 32, 32, 32), c);
                    _spriteBatch.Draw(whiteSquare, destination, colors[i % 3]);
                }
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
