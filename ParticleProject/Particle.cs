//Author: Noah Teitlebaum
//File Name: Particle.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building an entity with various behaviours that launches from an emitter

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
    class Particle
    {
        //Store the different types of particle states
        public const int INACTIVE = 0;
        public const int ACTIVE = 1;
        public const int DEAD = 2;

        //Store the different type of particle bounciness
        public const float RUBBER_BALL = 0.8f;
        public const float BOWLING_BALL = 0.3f;
        public const float SPLAT_BALL = 0.1f;
        public const float NO_CHANGE = 1f;

        //Store the data that makes up a particle
        private Texture2D img;
        private Rectangle rec;
        private Vector2 pos;
        private float scale = 1f;
        private int lifeSpan;
        private Timer lifeTimer;
        private float angle;
        private Vector2 vel;
        private Vector2 forces;
        private bool fade;
        private float reboundScaler;
        private Color colour;
        private bool envCollisions;
        private int state;
        private float opacity;
        
        //Store the tolerence where the particle has practically stopped
        private float speedTolerance = 0.005f;

        /// <summary>
        /// A Particle is a tiny entity launched from an Emitter
        /// </summary>
        /// <param name="img">Particle image</param>
        /// <param name="scale">Scale on how large a particle is</param>
        /// <param name="lifeSpan">How long a particle lives for</param>
        /// <param name="angle">The angle the particle is launched at</param>
        /// <param name="speed">Speed the particle is launched at</param>
        /// <param name="forces">The horizontal and vertical forces that act on the particle’s velocity</param>
        /// <param name="reboundScaler">A value to multiply the velocity by on collisions to account for lost energy during impact</param>
        /// <param name="colour">Particle colour</param>
        /// <param name="envCollisions">If true, collisions will be detected between the particle and all platforms</param>
        /// <param name="fade">If true, the opacity of the particle will fade relative to its lifespan remaining</param>
        public Particle(Texture2D img, float scale, int lifeSpan, int angle, float speed, Vector2 forces, float reboundScaler, Color colour, bool envCollisions, bool fade)
        {
            //Load the data that makes up a particle
            this.img = img;
            this.scale = scale;
            this.rec = new Rectangle(0, 0, (int)(img.Width * scale), (int)(img.Height * scale));
            this.lifeSpan = lifeSpan;
            this.lifeTimer = new Timer(lifeSpan, false);
            this.angle = MathHelper.ToRadians(angle);
            this.vel = new Vector2((float)(speed * Math.Cos(this.angle)), -(float)(speed * Math.Sin(this.angle)));
            this.forces = forces;
            this.fade = fade;
            this.reboundScaler = reboundScaler;
            this.colour = colour;
            this.envCollisions = envCollisions;

            //Load the state and opacity of the particle
            this.state = INACTIVE;
            this.opacity = 1f;
        }

        //Pre: None
        //Post: Return the resulting particle's position as a Vector2
        //Description: Retrieve the particle's position
        public Vector2 GetPosition()
        {
            //Return the particle's position
            return pos;
        }

        //Pre: The time of the program, and a collection of all the platforms
        //Post: None
        //Description: Update the particles
        public void Update(GameTime gameTime, List<Platform> platforms)
        {
            //Verifiy if the state is active
            if (GetState() == ACTIVE)
            {
                //Update the life timer
                lifeTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

                //Check to see if the life timer is active or not
                if (lifeTimer.IsActive())
                {
                    //Check if fade was opted for true or false
                    if (fade)
                    {
                        //Make the particle slowly disappear based off the life timer and life span
                        opacity = lifeTimer.GetTimeRemainingInt() / (float)lifeSpan;
                    }

                    //Add the forces onto the velocity and translate the particle
                    vel += forces;
                    Translate(vel * (float)gameTime.ElapsedGameTime.TotalSeconds);

                    //Check if particle collisions was opted for true or false
                    if (envCollisions && platforms != null)
                    {
                        //Loop through each platform
                        for (int i = 0; i < platforms.Count; i++)
                        {
                            //Verify if the particle has intersected any platform
                            if (rec.Intersects(platforms[i].GetBoundingBox()))
                            {
                                //Check which particle midpoint collidied with the platform
                                if (platforms[i].GetBoundingBox().Contains(rec.Center.X, rec.Bottom))
                                {
                                    //Set the particle’s position to align with the platform’s top side and reverse the velocity’s Y component to bounce
                                    pos.Y = platforms[i].GetBoundingBox().Top - rec.Height / 2;
                                    vel.Y *= -reboundScaler;
                                }
                                else if (platforms[i].GetBoundingBox().Contains(rec.Center.X, rec.Top))
                                {
                                    //Set the particle’s position to align with the platform’s bottom side and reverse the velocity’s Y component to bounce
                                    pos.Y = platforms[i].GetBoundingBox().Bottom + rec.Height / 2;
                                    vel.Y *= -reboundScaler;
                                }
                                else if (platforms[i].GetBoundingBox().Contains(rec.Right, rec.Center.Y))
                                {
                                    //Set the particle’s position to align with the platform’s left side and reverse the velocity’s X component to bounce
                                    pos.X = platforms[i].GetBoundingBox().Left - rec.Width / 2;
                                    vel.X *= -reboundScaler;
                                }
                                else if (platforms[i].GetBoundingBox().Contains(rec.Left, rec.Center.Y))
                                {
                                    //Set the particle’s position to align with the platform’s right side and reverse the velocity’s X component to bounce
                                    pos.X = platforms[i].GetBoundingBox().Right + rec.Width / 2;
                                    vel.X *= -reboundScaler;
                                }
                            }
                        }
                    }

                    //Stop the particle when it's velocity goes below the tolerance
                    if (Math.Abs(vel.X) < speedTolerance && Math.Abs(vel.Y) < speedTolerance)
                    {
                        //Set the particle's velocity and forces to zero
                        vel = Vector2.Zero;
                        forces = Vector2.Zero;
                    }
                }
                else if (lifeTimer.IsFinished())
                {
                    //Set the particle state to dead
                    state = DEAD;
                }
            }
        }

        //Pre: Draws the sprites onto the screen
        //Post: None
        //Description: Draw the particles
        public void Draw(SpriteBatch spriteBatch)
        {
            //Verify the particle can be drawn
            if (GetState() == ACTIVE && opacity > 0)
            {
                //Draw the particle
                spriteBatch.Draw(img, rec, colour * opacity);
            }
        }

        //Pre: The change in velocity based off the time
        //Post: None
        //Description: Translate the particles
        private void Translate(Vector2 deltaVel)
        {
            //Update the particle's bounding box based off its position and current velocity
            pos += deltaVel;
            rec.X = (int)pos.X - rec.Width / 2;
            rec.Y = (int)pos.Y - rec.Height / 2;
        }

        //Pre: Center position of the emitter
        //Post: None
        //Description: Set the starting position of the particles
        private void SetPosition(Vector2 pos)
        {
            //Set the particle's bounding box based off its starting position
            this.pos = pos;
            rec.X = (int)this.pos.X - rec.Width / 2;
            rec.Y = (int)this.pos.Y - rec.Height / 2;
        }

        //Pre: Center position of the emitter
        //Post: None
        //Description: Launch the particles
        public void Launch(Vector2 startPos)
        {
            //Verify the particle is inactive
            if (GetState() == INACTIVE)
            {
                //Activate the particle's position and life timer
                state = ACTIVE;
                SetPosition(startPos);
                lifeTimer.Activate();
            }
        }

        //Pre: None
        //Post: Return the resulting particle state as an int
        //Description: Retrieve the particle state
        public int GetState()
        {
            //Return the particle's state
            return state;
        }
    }
}