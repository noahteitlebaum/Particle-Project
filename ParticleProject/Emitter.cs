//Author: Noah Teitlebaum
//File Name: Emitter.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building an entity that creates, owns, maintains and launches particles

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using GameUtility;

namespace ParticleProject
{
    class Emitter
    {
        //Store the min and max array values
        protected const int MIN = 0;
        protected const int MAX = 1;

        //Store the different types of emitter states
        public const int INACTIVE = 0;
        public const int ACTIVE = 1;
        public const int DEAD = 2;
        public const int DONE = 3;

        //Store the infinite emitter constant and the max amount of particles an emitter can own
        public const int INFINITE = -1;
        public const int MAX_PARTICLES = 5000;

        //Store the data that makes up an emitter
        protected List<Particle> particles = new List<Particle>();
        protected Texture2D img;
        protected float scale;
        protected Vector2 pos;
        protected Rectangle rec;
        protected int numParticles;
        protected int state;
        protected int numLaunched;

        //Store the launch timer and its ranges
        protected int[] launchTimeRange = new int[2];
        protected Timer launchTimer;

        //Store the data that makes up a particle
        protected Texture2D particleImg;
        protected float[] scaleRange = new float[2];
        protected int[] lifeRange = new int[2];
        protected int[] angleRange = new int[2];
        protected int[] speedRange = new int[2];
        protected Vector2 forces;
        protected float reboundScaler;
        protected Color colour;
        protected bool envCollisions;
        protected bool fade;

        //Store the toggle values and if the emitter is an explosive
        protected bool running = true;
        protected bool showLauncher = false;
        protected bool explode = false;

        /// <summary>
        /// This is the general term for the entity that creates, owns, maintains and launches particles
        /// </summary>
        /// <param name="img">Emitter image</param>
        /// <param name="scale">Emitter scale</param>
        /// <param name="pos">Emitter midpoint position</param>
        /// <param name="numParticles">Number of particles launched by the emitter</param>
        /// <param name="launchTimeMin">Minimum launch time between particles</param>
        /// <param name="launchTimeMax">Maximum launch time between particles</param>
        /// <param name="particleImg">Particle image</param>
        /// <param name="scaleMin">Minimum particle scale</param>
        /// <param name="scaleMax">Maximum particle scale</param>
        /// <param name="lifeMin">Minimum time a particle lives for</param>
        /// <param name="lifeMax">Maximum time a particle lives for</param>
        /// <param name="angleMin">Minimum particle angle range</param>
        /// <param name="angleMax">Maximum particle angle range</param>
        /// <param name="speedMin">Minimum speed of the particle</param>
        /// <param name="speedMax">Maximum speed of the particle</param>
        /// <param name="forces">The horizontal and vertical forces that act on the particle’s velocity</param>
        /// <param name="reboundScaler">A value to multiply the particle velocity by on collisions to account for lost energy during impact</param>
        /// <param name="colour">Particle colour</param>
        /// <param name="envCollisions">If true, collisions will be detected between the particle and all platforms</param>
        /// <param name="fade">If true, the opacity of the particle will fade relative to its lifespan remaining</param>
        public Emitter(Texture2D img, float scale, Vector2 pos, int numParticles, int launchTimeMin, int launchTimeMax, 
                       Texture2D particleImg, float scaleMin, float scaleMax, int lifeMin, int lifeMax, int angleMin, int angleMax,
                       int speedMin, int speedMax, Vector2 forces, float reboundScaler, Color colour, bool envCollisions, bool fade)
        {
            //Load the data that makes up an emitter
            this.img = img;
            this.scale = scale;
            this.pos = pos;
            this.numParticles = numParticles;
            this.state = INACTIVE;

            //Verify if the emitter image can be drawn
            if (IsDrawn())
            {
                //Load the emitter's bounding box
                this.rec = new Rectangle(0, 0, (int)(img.Width * scale), (int)(img.Height * scale));
            }

            //Load the launch timer ranges
            this.launchTimeRange = new int[] { launchTimeMin, launchTimeMax };

            //Check if any lauch time ranges are below zero
            if (launchTimeRange[MIN] < 0 || launchTimeRange[MAX] < 0)
            {
                //Set the emitter to be explosive and create an instant launch timer
                this.explode = true;
                this.launchTimer = new Timer(1, false);
            }
            else
            {
                //Set the launch timer to a random value
                this.launchTimer = new Timer(GetIntInRange(launchTimeRange[MIN], launchTimeRange[MAX]), false);
            }

            //Load the data that makes up a particle
            this.particleImg = particleImg;
            this.scaleRange = new float[] { scaleMin, scaleMax };
            this.lifeRange = new int[] { lifeMin, lifeMax };
            this.angleRange = new int[] { angleMin, angleMax };
            this.speedRange = new int[] { speedMin, speedMax };
            this.forces = forces;
            this.reboundScaler = reboundScaler;
            this.colour = colour;
            this.envCollisions = envCollisions;
            this.fade = fade;
        }

        //Pre: None
        //Post: Return the resulting center position as a Vector2
        //Description: Retrieve the emitter's center position
        public Vector2 GetPosition()
        {
            //Return the emitters position
            return pos;
        }

        //Pre: A positions X and Y values
        //Post: None
        //Description: Set the emitter's bounding box based on the center position
        public virtual void SetPosition(float x, float y)
        {
            //Set the emitter's bounding box based off of the center position
            rec.X = (int)x - rec.Width / 2 - 1;
            rec.Y = (int)y - rec.Height / 2 - 1;

            //Verify if the image can not be drawn
            if (!IsDrawn())
            {
                //Set the emitter's position to its bounding box
                pos.X = rec.X;
                pos.Y = rec.Y;
            }
        }

        //Pre: None
        //Post: Return the resulting emitter state as an int
        //Description: Retrieve the emitter state
        public int GetState()
        {
            //Return the emitter's state
            return state;
        }

        //Pre: None
        //Post: None
        //Description: Launch all of the emitters particles at once
        protected virtual void LaunchAll()
        {
            //Restrict the amount of particles in a rangle between 1 and max particles
            numParticles = MathHelper.Clamp(numParticles, 1, MAX_PARTICLES);

            //Loop through each particle
            for (int i = 0; i < numParticles; i++)
            {
                //Add, launch and increment each particle
                particles.Add(SpawnParticle());
                particles[i].Launch(GetPosition());
                numLaunched++;
            }

            //Deactivate the launch timer
            launchTimer.Deactivate();
        }

        //Pre: None
        //Post: None
        //Description: Launch the emitters particles spaced out betwen the launch timer
        protected virtual void Launch()
        {
            //Add a new particle
            particles.Add(SpawnParticle());

            //Loop through each particle
            for (int i = 0; i < particles.Count; i++)
            {
                //Launch each particle
                particles[i].Launch(GetPosition());
            }

            //Increment the number of particles launched and reset the timer with a new random value
            numLaunched++;
            launchTimer.ResetTimer(true, GetIntInRange(launchTimeRange[MIN], launchTimeRange[MAX]));
        }

        //Pre: None
        //Post: Return the resulting particle as a new Particle
        //Description: Spawn a new particle
        protected Particle SpawnParticle()
        {
            //Return a new particle
            return new Particle(particleImg, GetFloatInRange(scaleRange[MIN], scaleRange[MAX]), GetIntInRange(lifeRange[MIN], lifeRange[MAX]), 
                                GetIntInRange(angleRange[MIN], angleRange[MAX]), GetIntInRange(speedRange[MIN], speedRange[MAX]), forces,
                                reboundScaler, colour, envCollisions, fade);
        }

        //Pre: Min and max values
        //Post: Return a resulting value between the min and max ranges as an int
        //Description: Randomize a new value between a min and max range
        protected int GetIntInRange(int min, int max)
        {
            //Return a random value
            return Game1.rng.Next(min, max + 1);
        }

        //Pre: Min and max values
        //Post: Return a resulting value between the min and max ranges as a float
        //Description: Randomize a new value between a min and max range
        protected float GetFloatInRange(float min, float max)
        {
            //Return a random value
            return (float)(Game1.rng.NextDouble() * (max - min) + min);
        }

        //Pre: The time of the program, and a collection of all the platforms
        //Post: None
        //Description: Update the base emitter
        public virtual void Update(GameTime gameTime, List<Platform> platforms)
        {
            //Verify the emitter is running
            if (running)
            {
                //Set the position of the emitter
                SetPosition(GetPosition().X, GetPosition().Y);

                //Check if the emitter's state is active
                if (GetState() == ACTIVE)
                {
                    //Update the launch timer
                    launchTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
                }

                //Check if the launch timer is finished
                if (launchTimer.IsFinished())
                {
                    //Check the type of emitter
                    if (explode)
                    {
                        //Launch all the particles
                        LaunchAll();
                    }
                    else if (numParticles == INFINITE || numLaunched < numParticles)
                    {
                        //Launch each particle one at a time
                        Launch();
                    }
                }

                //Check if the emitter has launched all of its particles
                if (numLaunched >= numParticles && numParticles != INFINITE)
                {
                    //Set the emitter's state to dead
                    state = DEAD;

                    //Check if the emitter owns no more particles
                    if (particles.Count <= 0)
                    {
                        //Set the emitter's state to done
                        state = DONE;
                    }
                }

                //Loop through each particle
                for (int i = 0; i < particles.Count; i++)
                {
                    //Update all particles
                    particles[i].Update(gameTime, platforms);

                    //Verify which particles are dead
                    if (particles[i].GetState() == Particle.DEAD)
                    {
                        //Remove the dead particle
                        particles.RemoveAt(i);
                    }
                }
            }
        }

        //Pre: Draws the sprites onto the screen
        //Post: None
        //Description: Draw the particles and base emitter
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            //Loop through each particle
            for (int i = 0; i < particles.Count; i++)
            {
                //Draw all particles
                particles[i].Draw(spriteBatch);
            }

            //Verify if the emitter image can be drawn
            if (IsDrawn())
            {
                //Draw the emitter
                spriteBatch.Draw(img, rec, Color.White);
            }
        }

        //Pre: None
        //Post: None
        //Description: Activate the emitter
        public void Activate()
        {
            //Set the emitter's state to active and active the timer
            state = ACTIVE;
            launchTimer.Activate();
        }

        //Pre: None
        //Post: None
        //Description: Toggle on/off the emitter
        public void ToggleOnOff()
        {
            //Toggle the running values state
            running = !running;
        }

        //Pre: None
        //Post: None
        //Description: Toggle the visibility for the child classes of the emitter
        public void ToggleLauncherVisibility()
        {
            //Toggle the launcher visibility values state
            showLauncher = !showLauncher;
        }

        //Pre: None
        //Post: None
        //Description: Toggle an explosive for the ship class
        public void ToggleShipExplosivness()
        {
            //Verify the emitter is running
            if (running)
            {
                //Toggle the explosive values state
                explode = !explode;
            }
        }

        //Pre: None
        //Post: Return a resulting value as true or false
        //Description: Verify if an emitter can be drawn
        protected bool IsDrawn()
        {
            //Check if the emitter has an image and has a positive scale value
            if (img != null && scale > 0)
            {
                //The emitter can be drawn
                return true;
            }

            //The emitter can't be drawn
            return false;
        }
    }
}