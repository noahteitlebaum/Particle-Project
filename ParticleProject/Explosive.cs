//Author: Noah Teitlebaum
//File Name: Explosive.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building an emitter class that explodes all its particles on command

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
    class Explosive : Emitter
    {
        //Store the explosive's particle scale values
        private static float SCALE_MIN = 0.1f;
        private static float SCALE_MAX = 0.3f;

        //Store the explosive's particle life span values
        private static int LIFE_MIN = 1500;
        private static int LIFE_MAX = 3500;

        //Store the explosive's particle speed values
        private static int SPEED_MIN = 0;
        private static int SPEED_MAX = 850;

        //Store the explosive's launch time to instant
        private static int NO_TIME = -1;

        /// <summary>
        /// Explosive is an emitter with no image and always launches all of its particles at once
        /// </summary>
        /// <param name="img">Particle image</param>
        /// <param name="pos">Emitter midpoint position</param>
        /// <param name="numParticles">Number of particles launched by the emitter</param>
        public Explosive(Texture2D img, Vector2 pos, int numParticles) : base(null, 0f, pos, numParticles, NO_TIME, NO_TIME, img, SCALE_MIN, SCALE_MAX, LIFE_MIN, LIFE_MAX, 0, 360, 
                                                                              SPEED_MIN, SPEED_MAX, Game1.gravity, Particle.BOWLING_BALL, Color.White, true, true)
        {
            //Load the data that makes up an explosive emitter
            this.img = img;
            this.pos = pos;
            this.numParticles = numParticles;
        }

        //Pre: None
        //Post: None
        //Description: Launch all of the emitters particles at once
        protected override void LaunchAll()
        {
            //Restrict the amount of particles in a rangle between 1 and max particles
            numParticles = MathHelper.Clamp(numParticles, 1, MAX_PARTICLES);

            //Loop through each particle
            for (int i = 0; i < numParticles; i++)
            {
                //Add and launch each particle
                particles.Add(SpawnParticle());
                particles[i].Launch(GetPosition());
            }
        }

        //Pre: The time of the program, and a collection of all the platforms
        //Post: None
        //Description: Update the explosive emitter
        public override void Update(GameTime gameTime, List<Platform> platforms)
        {
            //Check if the emitter's state is active or if there are no more particles
            if (GetState() == ACTIVE)
            {
                //Launch all the particles and set the emitter's state to dead
                LaunchAll();
                state = DEAD;
            }
            else if (particles.Count <= 0)
            {
                //Set the emitter's state to done
                state = DONE;
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
        }
    }
}