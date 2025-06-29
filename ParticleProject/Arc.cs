 //Author: Noah Teitlebaum
//File Name: Arc.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building an emitter class that launches its particles inside a given arc area

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
    class Arc : Emitter
    {
        //Store the radius max value
        private static int DIMESNSIONS_MAX = 200;

        //Store the data that makes up an arc emitter
        private int radius;
        private float fov;
        private Color arcColour;

        //Store the data to draw the arc
        private GraphicsDevice gd;
        private GameArc gameArc;

        /// <summary>
        /// Arc acts like a normal Emitter, but it launches from a random position within a given arc
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
        /// <param name="radius">Radius of the arc emitter</param>
        /// <param name="fov">The angle of the arc</param>
        /// <param name="arcColour">Colour of the arc emitter</param>
        /// <param name="gd">Creates the area of the arc emitter to draw</param>
        public Arc(Texture2D img, float scale, Vector2 pos, int numParticles, int launchTimeMin, int launchTimeMax,
                Texture2D particleImg, float scaleMin, float scaleMax, int lifeMin, int lifeMax, int angleMin, int angleMax,
                int speedMin, int speedMax, Vector2 forces, float reboundScaler, Color colour, bool envCollisions, bool fade,
                int radius, float fov, Color arcColour, GraphicsDevice gd) : base(img, scale, pos, numParticles, launchTimeMin, launchTimeMax,
                particleImg, scaleMin, scaleMax, lifeMin, lifeMax, angleMin, angleMax, speedMin, speedMax, forces, reboundScaler,
                colour, envCollisions, fade)
        {
            //Load the data that makes up and draws an arc emitter
            this.radius = MathHelper.Clamp(radius, 1, DIMESNSIONS_MAX);
            this.fov = fov;
            this.arcColour = arcColour;
            this.gd = gd;
        }

        //Pre: A positions X and Y values
        //Post: None
        //Description: Set the emitters bounding box based on the position
        public override void SetPosition(float x, float y)
        {
            //Set the emitter's bounding box based off of the position
            rec.X = (int)x;
            rec.Y = (int)y;

            //Create the arc emitter's launch area to be drawn
            gameArc = new GameArc(gd, rec.X, rec.Y, radius, rec.X + 1, rec.Y, fov);
        }

        //Pre: None
        //Post: Return a resulting position inside the arcs launch area
        //Description: Calculate a random position inside the arcs launch area
        private Vector2 GetLaunchPos()
        {
            //Randomize a new radius and angle inside the arcs area
            double newRadius = GetFloatInRange(0, radius);
            double theta = MathHelper.ToRadians(GetFloatInRange(-fov / 2, fov / 2));

            //Calculate the newly randomized position based of the new radius and angle
            int x = (int)(rec.X + newRadius * Math.Cos(theta));
            int y = (int)(rec.Y - newRadius * Math.Sin(theta));

            //Return the newly randomized position
            return new Vector2(x, y);
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
                //Add, launch and increment each particle
                particles.Add(SpawnParticle());
                particles[i].Launch(GetLaunchPos());
                numLaunched++;
            }

            //Deactivate the launch timer
            launchTimer.Deactivate();
        }

        //Pre: None
        //Post: None
        //Description: Launch the emitters particles spaced out betwen the launch timer
        protected override void Launch()
        {
            //Add a new particle
            particles.Add(SpawnParticle());

            //Loop through each particle
            for (int i = 0; i < particles.Count; i++)
            {
                //Launch each particle
                particles[i].Launch(GetLaunchPos());
            }

            //Increment the number of particles launched and reset the timer with a new random value
            numLaunched++;
            launchTimer.ResetTimer(true, GetIntInRange(launchTimeRange[MIN], launchTimeRange[MAX]));
        }

        //Pre: Draws the sprites onto the screen
        //Post: None
        //Description: Draw the launch area, particles, and emitter
        public override void Draw(SpriteBatch spriteBatch)
        {
            //Check if the launcher visibility values state is true
            if (showLauncher)
            {
                //Draw the arc emitter's launch area
                gameArc.Draw(spriteBatch, arcColour * 0.5f);
            }

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
    }
}