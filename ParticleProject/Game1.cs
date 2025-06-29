//Author: Noah Teitlebaum
//File Name: Game1.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building a system that manages, toggles, updates, and draws all emitters

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using GameUtility;

namespace ParticleProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Store the emitter names
        private const int MOUSE_EMITTER = 0;
        private const int WATER_LEFTWARD_EMITTER = 1;
        private const int WATER_UPWARD_EMITTER = 2;
        private const int WATER_RIGHTWARD_EMITTER = 3;
        private const int WATER_DOWNWARD_EMITTER = 4;
        private const int SMOKE_EMITTER = 5;

        //Store the space between the text locations
        private const int TEXT_LOC_SPACER = 40;

        //Store the RNG to be used by all classes
        public static Random rng = new Random();

        //Spritefonts for various forms of output
        private SpriteFont hudFont;
        private SpriteFont popUpFont;

        //Store the instruction texts and locations for toggling the emitters
        private Vector2[] instLocs = new Vector2[12];
        private string[] instTexts = new string[] { "1_KEY: Toggle Leftward Water Emitter", "2_KEY: Toggle Upward Water Emitter", "3_KEY: Toggle Rightward Water Emitter", "4_KEY: Toggle Downward Water Emitter",
                                                    "5_KEY: Toggle Smoke Emitter", "6_KEY: Toggle Circular Emitter", "7_KEY: Toggle Rectangular Emitter", "8_KEY: Toggle Arc Emitter", "9_KEY: Toggle Ship Emitter", 
                                                    "E_KEY: Explode Activated Ship Emitter", "SPACE_KEY: Toggle Launch Area Visibility", "LEFT_CLICK: Explode Particles" };

        //Store the background images
        private Texture2D bgImg;
        private Texture2D pausedBgImg;

        //Store the emitter image
        private Texture2D emitterImg;

        //Store the particle images
        private Texture2D[] smokeImgs = new Texture2D[2];
        private Texture2D earthImg;
        private Texture2D marsImg;
        private Texture2D saturnImg;
        private Texture2D uranusImg;
        private Texture2D asteroidImg;
        private Texture2D shipImg;
        private Texture2D sparkImg;
        private Texture2D bluePartImg;
        private Texture2D starImg;

        //Store the platform images
        private Texture2D explodeImg;
        private Texture2D brickImg;

        //Store the background rectangle
        private Rectangle bgRec;

        //Store input states
        private KeyboardState kb;
        private KeyboardState prevKb;
        private MouseState mouse;
        private MouseState prevMouse;

        //Store the screen dimensions
        private int screenWidth;
        private int screenHeight;

        //Store the game's paused state
        private bool paused = false;

        //Store the different forces, they can be directly added to combine them if needed
        public static Vector2 gravity = new Vector2(0, 1.62f);
        public static Vector2 wind = new Vector2(2f, 0);

        //Store the list of platforms
        private List<Platform> platforms = new List<Platform>();

        //Store the list of Emitters
        private List<Emitter> emitters = new List<Emitter>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            IsMouseVisible = true;

            //Setup the screen dimensions
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 740;
            graphics.ApplyChanges();
            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/HUDFont");
            popUpFont = Content.Load<SpriteFont>("Fonts/PopUpFont");

            //Loop through each text location
            for (int i = 0; i < instLocs.Length; i++)
            {
                //Load the text locations
                instLocs[i] = new Vector2((screenWidth - hudFont.MeasureString(instTexts[i]).X) / 2, TEXT_LOC_SPACER + TEXT_LOC_SPACER * i);
            }

            //Load the background, emitter, platform and particle images
            bgImg = Content.Load<Texture2D>("Images/Backgrounds/SpaceBackground");
            pausedBgImg = Content.Load<Texture2D>("Images/Backgrounds/PausedBg");
            emitterImg = Content.Load<Texture2D>("Images/Sprites/Box");
            smokeImgs[0] = Content.Load<Texture2D>("Images/Sprites/Smoke");
            smokeImgs[1] = Content.Load<Texture2D>("Images/Sprites/Smoke2");
            earthImg = Content.Load<Texture2D>("Images/Sprites/Earth");
            marsImg = Content.Load<Texture2D>("Images/Sprites/Mars");
            saturnImg = Content.Load<Texture2D>("Images/Sprites/Saturn");
            uranusImg = Content.Load<Texture2D>("Images/Sprites/Uranus");
            asteroidImg = Content.Load<Texture2D>("Images/Sprites/Asteroid");
            shipImg = Content.Load<Texture2D>("Images/Sprites/Ship");
            sparkImg = Content.Load<Texture2D>("Images/Sprites/Spark");
            bluePartImg = Content.Load<Texture2D>("Images/Sprites/BlueBall");
            starImg = Content.Load<Texture2D>("Images/Sprites/Star");
            brickImg = Content.Load<Texture2D>("Images/Sprites/MoonBrick");

            //Load the background rectangle
            bgRec = new Rectangle(0, 0, screenWidth, screenHeight);

            //Load the platforms for walls and floating platforms
            float brickScale = 0.25f;
            platforms.Add(new Platform(brickImg, 26, brickScale, 0, 0, false));                                                 //left wall
            platforms.Add(new Platform(brickImg, 26, brickScale, screenWidth - (int)(brickImg.Width * brickScale), 0, false));  //right wall
            platforms.Add(new Platform(brickImg, 10, 1f, 0, screenHeight - brickImg.Height, true));                             //floor

            platforms.Add(new Platform(brickImg, 4, 0.5f, 150, 500, true));                                                     //left platform
            platforms.Add(new Platform(brickImg, 2, 0.5f, 200, 400, false));

            platforms.Add(new Platform(brickImg, 1, 0.5f, 700, 590, false));                                                    //Hill
            platforms.Add(new Platform(brickImg, 2, 0.5f, 750, 540, false));
            platforms.Add(new Platform(brickImg, 3, 0.5f, 800, 490, false));
            platforms.Add(new Platform(brickImg, 2, 0.5f, 850, 540, false));
            platforms.Add(new Platform(brickImg, 1, 0.5f, 900, 590, false));

            platforms.Add(new Platform(brickImg, 10, 0.25f, 25, 108, true));                                                    //Water platform

            //Add the mouse following emitter
            emitters.Add(new Emitter(null, 0f, new Vector2(0, 0), Emitter.INFINITE, 50, 100, asteroidImg, 0.05f, 0.15f, 1000, 2000,
                         0, 360, 0, 100, gravity, Particle.BOWLING_BALL, Color.LightGray, false, true));

            //Add all of the water emitters in descending order of: leftward, upward, rightward, downward
            emitters.Add(new Emitter(emitterImg, 0.25f, new Vector2(38, 96), Emitter.INFINITE, 0, 10, bluePartImg, 0.1f, 0.25f, 3000, 4000,
                         330, 390, 250, 500, gravity, Particle.SPLAT_BALL, Color.White, true, true));
            emitters.Add(new Emitter(emitterImg, 0.25f, new Vector2(screenWidth / 2, 12), Emitter.INFINITE, 0, 10, bluePartImg, 0.1f, 0.25f, 3000, 4000,
                         240, 300, 250, 500, gravity, Particle.SPLAT_BALL, Color.White, true, true));
            emitters.Add(new Emitter(emitterImg, 0.25f, new Vector2(963, 100), Emitter.INFINITE, 0, 10, bluePartImg, 0.1f, 0.25f, 3000, 4000,
                         150, 210, 250, 500, gravity, Particle.SPLAT_BALL, Color.White, true, true));
            emitters.Add(new Emitter(emitterImg, 0.25f, new Vector2(825, 480), Emitter.INFINITE, 0, 10, bluePartImg, 0.1f, 0.25f, 3000, 4000,
                         60, 120, 250, 500, gravity, Particle.SPLAT_BALL, Color.White, true, true));

            //Add the smoke emitter
            emitters.Add(new Emitter(emitterImg, 0.25f, new Vector2(225, 388), Emitter.INFINITE, 0, 100, smokeImgs[1], 0.35f, 0.6f, 2500, 3500,
                         75, 105, 100, 100, new Vector2(0.5f, -1.5f), Particle.SPLAT_BALL, Color.White, true, true));

            //Add the circular emitters
            emitters.Add(new Circular(null, 0f, new Vector2(525, 485), Emitter.INFINITE, 300, 800, earthImg, 0.02f, 0.08f, 1000, 3500,
                                      0, 360, 0, 200, gravity, Particle.RUBBER_BALL, Color.White, true, true, 75, Color.MediumPurple, GraphicsDevice));

            //Add the rectangular emitters
            emitters.Add(new Rectangular(null, 0f, new Vector2(80, 540), Emitter.INFINITE, 400, 900, marsImg, 0.01f, 0.09f, 1000, 3500,
                                         0, 360, 0, 200, gravity, Particle.RUBBER_BALL, Color.White, true, true, 70, 180, Color.MediumPurple, GraphicsDevice));
            emitters.Add(new Rectangular(null, 0, new Vector2(475, 120), Emitter.INFINITE, 500, 1000, saturnImg, 0.07f, 0.11f, 1000, 3500,
                                         0, 360, 0, 200, gravity, Particle.RUBBER_BALL, Color.White, true, true, 350, 20, Color.MediumPurple, GraphicsDevice));

            //Add the arc emitters
            emitters.Add(new Arc(null, 0f, new Vector2(775, 300), Emitter.INFINITE, 600, 1100, uranusImg, 0.1f, 0.25f, 1000, 3500,
                                 0, 360, 0, 200, gravity, Particle.RUBBER_BALL, Color.White, true, true, 150, 60, Color.MediumPurple, GraphicsDevice));

            //Add the ship emitters
            emitters.Add(new Ship(Emitter.INFINITE, 25, 100, smokeImgs[1], 0.4f, 0.7f, 500, 1500, 0, 360, 0, 0, gravity, Particle.BOWLING_BALL, Color.OrangeRed, true, true, 
                                  90, sparkImg, 0.1f, 0.3f, 1500, 3000, 0, 360, 100, 200, gravity, Particle.BOWLING_BALL, Color.White, true, true, 
                                  new Vector2(screenWidth / 2, screenHeight / 2), shipImg, 1f, Emitter.INFINITE, 20, 250, gravity, Particle.NO_CHANGE, Color.White, true, true));
        }


        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            //Exit on the escape key
            if (kb.IsKeyDown(Keys.Escape)) Exit();

            //Update input devices
            prevKb = kb;
            prevMouse = mouse;
            kb = Keyboard.GetState();
            mouse = Mouse.GetState();

            //Toggle pause
            if (kb.IsKeyDown(Keys.P) && !prevKb.IsKeyDown(Keys.P))
            {
                //Inverse the pause states
                paused = !paused;
            }

            //Only update and get input when simulation is not paused
            if (!paused)
            {
                //Ignite Bomb at mouse click location
                if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
                {
                    //Set the collision to true
                    bool noCollision = true;

                    //Loop through each platform
                    for (int i = 0; i < platforms.Count; i++)
                    {
                        //Verify no platforms collide with the mouse's click location
                        if (platforms[i].GetBoundingBox().Contains(mouse.Position))
                        {
                            //Set the collision to false
                            noCollision = false;
                            break;
                        }
                    }

                    //No mouse collision with platforms was found, explosion is allowed
                    if (noCollision)
                    {
                        //Store the index of the explosion emitters
                        int explosionEmitters = emitters.Count;

                        //Add the explosion emitter
                        emitters.Add(new Explosive(starImg, new Vector2(mouse.X, mouse.Y), 1000));

                        //Loop through each explosive emitter
                        for (int i = explosionEmitters; i < emitters.Count; i++)
                        {
                            //Check if any explosive emitter is active
                            IsEmitterActive(i);
                        }
                    }
                }

                //Toggle the water emitters based on the key pressed
                if (kb.IsKeyDown(Keys.D1) && !prevKb.IsKeyDown(Keys.D1))
                {
                    //Check if the leftward spout is active
                    if (IsEmitterActive(WATER_LEFTWARD_EMITTER))
                    {
                        //Toggle on/off the leftward spout
                        emitters[WATER_LEFTWARD_EMITTER].ToggleOnOff();
                    }
                }
                if (kb.IsKeyDown(Keys.D2) && !prevKb.IsKeyDown(Keys.D2))
                {
                    //Check if the upward spout is active
                    if (IsEmitterActive(WATER_UPWARD_EMITTER))
                    {
                        //Toggle on/off the upward spout
                        emitters[WATER_UPWARD_EMITTER].ToggleOnOff();
                    }
                }
                if (kb.IsKeyDown(Keys.D3) && !prevKb.IsKeyDown(Keys.D3))
                {
                    //Check if the rightward spout is active
                    if (IsEmitterActive(WATER_RIGHTWARD_EMITTER))
                    {
                        //Toggle on/off the rightward spout
                        emitters[WATER_RIGHTWARD_EMITTER].ToggleOnOff();
                    }
                }
                if (kb.IsKeyDown(Keys.D4) && !prevKb.IsKeyDown(Keys.D4))
                {
                    //Check if the downward spout is active
                    if (IsEmitterActive(WATER_DOWNWARD_EMITTER))
                    {
                        //Toggle on/off the downward spout
                        emitters[WATER_DOWNWARD_EMITTER].ToggleOnOff();
                    }
                }

                //Toggle the smoke emitter
                if (kb.IsKeyDown(Keys.D5) && !prevKb.IsKeyDown(Keys.D5))
                {
                    //Check if the smoke emitter is active
                    if (IsEmitterActive(SMOKE_EMITTER))
                    {
                        //Toggle on/off the smoke emitter
                        emitters[SMOKE_EMITTER].ToggleOnOff();
                    }
                }

                //Toggle the Circular, Rectangular, Arc and Ship Emitters based on the key pressed
                if (kb.IsKeyDown(Keys.D6) && !prevKb.IsKeyDown(Keys.D6))
                {
                    //Loop through each circular emitter
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        //Verify which emitters are circular
                        if (emitters[i] is Circular)
                        {
                            //Check if any circular emitter is active
                            if (IsEmitterActive(i))
                            {
                                //Toggle on/off the circular emitters
                                emitters[i].ToggleOnOff();
                            }
                        }
                    }
                }
                if (kb.IsKeyDown(Keys.D7) && !prevKb.IsKeyDown(Keys.D7))
                {
                    //Loop through each rectangular emitter
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        //Verify which emitters are rectangular
                        if (emitters[i] is Rectangular)
                        {
                            //Check if any rectangular emitter is active
                            if (IsEmitterActive(i))
                            {
                                //Toggle on/off the rectangular emitters
                                emitters[i].ToggleOnOff();
                            }
                        }
                    }
                }
                if (kb.IsKeyDown(Keys.D8) && !prevKb.IsKeyDown(Keys.D8))
                {
                    //Loop through each arc emitter
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        //Verify which emitters are arcs
                        if (emitters[i] is Arc)
                        {
                            //Check if any arc emitter is active
                            if (IsEmitterActive(i))
                            {
                                //Toggle on/off the arc emitters
                                emitters[i].ToggleOnOff();
                            }
                        }
                    }
                }
                if (kb.IsKeyDown(Keys.D9) && !prevKb.IsKeyDown(Keys.D9))
                {
                    //Loop through each ship emitter
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        //Verify which emitters are ships
                        if (emitters[i] is Ship)
                        {
                            //Check if any ship emitter is active
                            if (IsEmitterActive(i))
                            {
                                //Toggle on/off the ship emitters
                                emitters[i].ToggleOnOff();
                            }
                        }
                    }
                }
                if (kb.IsKeyDown(Keys.E) && !prevKb.IsKeyDown(Keys.E))
                {
                    //Loop through each ship emitter
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        //Check if any ship emitter is active
                        if (emitters[i] is Ship && IsEmitterActive(i))
                        {
                            //Toggle on/off the ship explosivness
                            emitters[i].ToggleShipExplosivness();
                        }
                    }
                }

                //Verify if the space key was pressed
                if (kb.IsKeyDown(Keys.Space) && !prevKb.IsKeyDown(Keys.Space))
                {
                    //Loop through each emitter
                    for (int i = 0; i < emitters.Count; i++)
                    {
                        //Toggle on/off the launch area visibility
                        emitters[i].ToggleLauncherVisibility();
                    }
                }

                //Loop through each emitter
                for (int i = 0; i < emitters.Count; i++)
                {
                    //Update all emitters
                    emitters[i].Update(gameTime, platforms);

                    //Check if any emitter is done
                    if (emitters[i].GetState() == Emitter.DONE)
                    {
                        //Remove the emitter
                        emitters.RemoveAt(i);
                    }
                }

                //Activate and update the mouse following emitter's position
                emitters[MOUSE_EMITTER].SetPosition(mouse.X, mouse.Y);
                IsEmitterActive(MOUSE_EMITTER);           
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Begin drawing
            spriteBatch.Begin();

            //Draw the background
            spriteBatch.Draw(bgImg, bgRec, Color.White);

            //Loop through each emitter
            for (int i = 0; i < emitters.Count; i++)
            {
                //Draw all emitters
                emitters[i].Draw(spriteBatch);
            }

            //Loop through each platfrom
            for (int i = 0; i < platforms.Count; i++)
            {
                //Draw all platforms
                platforms[i].Draw(spriteBatch);
            }

            //Verify if the program is paused
            if (paused)
            {
                //Draw the paused background
                spriteBatch.Draw(pausedBgImg, bgRec, Color.DimGray * 0.8f);

                //Loop through each instruction text 
                for (int i = 0; i < instTexts.Length; i++)
                {
                    //Draw all of the instruction texts
                    spriteBatch.DrawString(hudFont, instTexts[i], instLocs[i], Color.Black);
                    spriteBatch.DrawString(hudFont, instTexts[i], new Vector2(instLocs[i].X + 2, instLocs[i].Y + 2), Color.Red);
                }
            }

            //End drawing
            spriteBatch.End();        

            base.Draw(gameTime);
        }

        //Pre: The index representing a certain emitter
        //Post: Return a resulting value as true or false
        //Description: Check if any emitter is active or not
        private bool IsEmitterActive(int index)
        {
            //Check if the emitter is inactive
            if (emitters[index].GetState() == Emitter.INACTIVE)
            {
                //Activate the emitter
                emitters[index].Activate();
                return false;
            }

            //The emitter is active
            return true;
        }
    }
}