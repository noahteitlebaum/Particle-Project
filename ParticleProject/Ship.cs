//Author: Noah Teitlebaum
//File Name: Ship.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building an emitter class that launches a ship which launches more particles in various ways

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
    class Ship : Emitter
    {
        //Store the max amount of new particles the ship emitter can own
        private const int NEW_MAX_PARATICLES = 100;

        //Store the data that makes up an explode particle
        int numParticlesExplode;
        private Texture2D particleImgExplode;
        private float[] scaleRangeExplode = new float[2];
        private int[] lifeRangeExplode = new int[2];
        private int[] angleRangeExplode = new int[2];
        private int[] speedRangeExplode = new int[2];
        private Vector2 forcesExplode;
        private float reboundScalerExplode;
        private Color colourExplode;
        private bool envCollisionsExplode;
        private bool fadeExplode;

        //Store the ship particle
        private Particle shipParticle;

        /// <summary>
        /// Ship is a type of emitter that launches and explodes particles on its location, while behaving like its own particle
        /// </summary>
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
        /// <param name="numParticlesExplode">Number of explode particles launched by the emitter</param>
        /// <param name="particleImgExplode">Explode particle image</param>
        /// <param name="scaleMinExplode">Minimum explode particle scale</param>
        /// <param name="scaleMaxExplode">Maximum explode particle scale</param>
        /// <param name="lifeMinExplode">Minimum time an explode particle lives for</param>
        /// <param name="lifeMaxExplode">Maximum time an explode particle lives for</param>
        /// <param name="angleMinExplode">Minimum angle range for an explode particle</param>
        /// <param name="angleMaxExplode">Maximum angle range for an explode particle</param>
        /// <param name="speedMinExplode">Minimum speed of the explode particle</param>
        /// <param name="speedMaxExplode">Maximum speed of the explode particle</param>
        /// <param name="forcesExplode">The horizontal and vertical forces that act on the explode particle’s velocity</param>
        /// <param name="reboundScalerExplode">A value to multiply the explode particle velocity by on collisions to account for lost energy during impact</param>
        /// <param name="colourExplode">Explode particle colour</param>
        /// <param name="envCollisionsExplode">If true, collisions will be detected between the explode particle and all platforms</param>
        /// <param name="fadeExplode">If true, the opacity of the explode particle will fade relative to its lifespan remaining</param>
        /// <param name="posShip">Location of where the ship first spawns</param>
        /// <param name="particleImgShip">Ship particle image</param>
        /// <param name="scaleShip">Magnitude of the ship particle</param>
        /// <param name="lifeShip">How long the ship lives for</param>
        /// <param name="angleShip">The direction to where the ship will go when first spawned</param>
        /// <param name="speedShip">How fast the ship will be moving</param>
        /// <param name="forcesShip">Ship particle colour</param>
        /// <param name="reboundScalerShip">A value to multiply the ship particle velocity by on collisions to account for lost energy during impact</param>
        /// <param name="colourShip">Ship particle colour</param>
        /// <param name="envCollisionsShip">If true, collisions will be detected between the ship particle and all platforms</param>
        /// <param name="fadeShip">If true, the opacity of the ship particle will fade relative to its lifespan remaining</param>
        public Ship(int numParticles, int launchTimeMin, int launchTimeMax, Texture2D particleImg,  float scaleMin, float scaleMax, int lifeMin, int lifeMax, int angleMin, int angleMax,
                    int speedMin, int speedMax, Vector2 forces, float reboundScaler, Color colour, bool envCollisions, bool fade,
                    int numParticlesExplode, Texture2D particleImgExplode, float scaleMinExplode, float scaleMaxExplode, int lifeMinExplode, int lifeMaxExplode, int angleMinExplode, int angleMaxExplode, 
                    int speedMinExplode, int speedMaxExplode, Vector2 forcesExplode, float reboundScalerExplode, Color colourExplode, bool envCollisionsExplode, bool fadeExplode, 
                    Vector2 posShip, Texture2D particleImgShip, float scaleShip, int lifeShip, int angleShip, int speedShip, Vector2 forcesShip, float reboundScalerShip, Color colourShip, bool envCollisionsShip, bool fadeShip) : 
                    base(null, 0f, new Vector2(0,0), numParticles, Math.Abs(launchTimeMin), Math.Abs(launchTimeMax), particleImg, scaleMin, scaleMax, 
                    lifeMin, lifeMax, angleMin, angleMax, speedMin, speedMax, forces, reboundScaler, colour, envCollisions, fade)
        {
            //Load the data that makes up an explode particle
            this.numParticlesExplode = numParticlesExplode;
            this.particleImgExplode = particleImgExplode;
            this.scaleRangeExplode = new float[] { scaleMinExplode, scaleMaxExplode };
            this.lifeRangeExplode = new int[] { lifeMinExplode, lifeMaxExplode };
            this.angleRangeExplode = new int[] { angleMinExplode, angleMaxExplode };
            this.speedRangeExplode = new int[] { speedMinExplode, speedMaxExplode };
            this.forcesExplode = forcesExplode;
            this.reboundScalerExplode = reboundScalerExplode;
            this.colourExplode = colourExplode;
            this.envCollisionsExplode = envCollisionsExplode;
            this.fadeExplode = fadeExplode;

            //Load and launch the ship particle
            this.shipParticle = new Particle(particleImgShip, scaleShip, lifeShip, angleShip, speedShip, forcesShip, reboundScalerShip, colourShip, envCollisionsShip, fadeShip);
            this.shipParticle.Launch(posShip);
        }

        //Pre: None
        //Post: None
        //Description: Launch all of the emitters particles at once
        protected override void LaunchAll()
        {
            //Restrict the amount of particles in a rangle between 1 and max particles
            numParticlesExplode = MathHelper.Clamp(numParticlesExplode, 1, NEW_MAX_PARATICLES);

            //Loop through each explode particle
            for (int i = 0; i < numParticlesExplode; i++)
            {
                //Add and launch each explode particle
                particles.Add(SpawnExplodeParticle());
                particles[i].Launch(GetPosition());
            }
        }

        //Pre: None
        //Post: Return the resulting particle as a new Particle
        //Description: Spawn a new particle
        private Particle SpawnExplodeParticle()
        {
            //Return a new explode particle
            return new Particle(particleImgExplode, GetFloatInRange(scaleRangeExplode[MIN], scaleRangeExplode[MAX]), GetIntInRange(lifeRangeExplode[MIN], lifeRangeExplode[MAX]),
                                GetIntInRange(angleRangeExplode[MIN], angleRangeExplode[MAX]), GetIntInRange(speedRangeExplode[MIN], speedRangeExplode[MAX]), forcesExplode,
                                reboundScalerExplode, colourExplode, envCollisionsExplode, fadeExplode);
        }

        //Pre: The time of the program, and a collection of all the platforms
        //Post: None
        //Description: Update the ship emitter
        public override void Update(GameTime gameTime, List<Platform> platforms)
        {
            //Verify the emitter is running
            if (running)
            {
                //Set the position of the particles onto the ship emitter
                SetPosition(shipParticle.GetPosition().X, shipParticle.GetPosition().Y);

                //Check if the ship emitter's state is active
                if (GetState() == ACTIVE)
                {
                    //Update the launch timer
                    launchTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
                }

                //Check if the launch timer is finished
                if (launchTimer.IsFinished())
                {
                    //Check if explosive is active or not
                    if (explode)
                    {
                        //Explode the ships particles
                        LaunchAll();
                        ToggleShipExplosivness();
                    }
                    else if (numParticles == INFINITE || numLaunched < numParticles)
                    {
                        //Launch each particle one at a time
                        Launch();
                    }
                }

                //Check if the ship emitter has launched all of its particles or if it faded from the screen
                if (numLaunched >= numParticles && numParticles != INFINITE || shipParticle.GetState() == Particle.DEAD)
                {
                    //Set the ship emitter's state to dead
                    state = DEAD;

                    //Check if the ship emitter owns no more particles
                    if (particles.Count <= 0)
                    {
                        //Set the ship emitter's state to done
                        state = DONE;
                    }
                }

                //Update the ship particle
                shipParticle.Update(gameTime, platforms);

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
        //Description: Draw the particles
        public override void Draw(SpriteBatch spriteBatch)
        {
            //Loop through each particle
            for (int i = 0; i < particles.Count; i++)
            {
                //Draw all particles
                particles[i].Draw(spriteBatch);
            }

            //Draw the ship particle
            shipParticle.Draw(spriteBatch);
        }
    }
}