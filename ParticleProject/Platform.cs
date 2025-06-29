//Author: Noah Teitlebaum
//File Name: Platform.cs
//Project Name: PASS2
//Creation Date: Oct. 16, 2023
//Modified Date: Oct. 30, 2023
//Description: Building an object that acts as a barrier for particles

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
    class Platform
    {
        //Store the data that makes up a platform
        private Texture2D img;
        private Rectangle[] blockRecs;

        //Store the platform's bounding box
        private Rectangle rec;

        /// <summary>
        /// A platform is a collection of blocks that are combined into one object with a single rectangle to allow for cheap collision detection
        /// </summary>
        /// <param name="blockImg">The image repeated along the length of the platform</param>
        /// <param name="numBricks">The number of blocks to be aligned edge to edge</param>
        /// <param name="scale">The amount each block image is scaled by before aligned together</param>
        /// <param name="x">The X position of the top left corner of the first block in the platform</param>
        /// <param name="y">The Y position of the top left corner of the first block in the platform</param>
        /// <param name="isHorizontal">If true, the blocks will be aligned side by side, otherwise they will be aligned top to bottom</param>
        public Platform(Texture2D blockImg, int numBricks, float scale, int x, int y, bool isHorizontal)
        {
            //Load the data that makes up a platform
            img = blockImg;
            blockRecs = new Rectangle[Math.Max(1,numBricks)];

            //Load the dimensions of the platform
            int width = (int)(img.Width * scale);
            int height = (int)(img.Height * scale);

            //Loop through each number of bricks
            for (int i = 0; i < numBricks; i++)
            {
                blockRecs[i] = new Rectangle(x + (width * i * (isHorizontal ? 1 : 0)),  //Add on to x for each block if it is horizontal
                                             y + (height * i * (isHorizontal ? 0 : 1)), //Add on to y for each block if it is not horizontal
                                             width, height);
            }

            //Load the platform's bounding box
            rec = new Rectangle(x, y, 
                                blockRecs[numBricks - 1].Right - blockRecs[0].Left,   //width = Right side of last block - Left side of first block
                                blockRecs[numBricks - 1].Bottom - blockRecs[0].Top);  //height = Bottom side of last block - Top side of first block
        }

        //Pre: None
        //Post: Returns the resulting platform bounding box
        //Description: Retreive the platform's bounding box
        public Rectangle GetBoundingBox()
        {
            //Return the platform's bounding box
            return rec;
        }

        //Pre: Draws the sprites onto the screen
        //Post: None
        //Description: Draw the platforms
        public void Draw(SpriteBatch spriteBatch)
        {
            //Loop through each block's bounding boxes
            for (int i = 0; i < blockRecs.Length; i++)
            {
                //Draw the platform
                spriteBatch.Draw(img, blockRecs[i], Color.White);
            }
        }
    }
}