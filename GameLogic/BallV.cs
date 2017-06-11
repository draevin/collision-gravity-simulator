using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GameLogic;

namespace GameLogic
{
    public class BallV
    {
        public int radius;
        public Vec2 position;
        public Vec2 velocity;
        public int mass;

        public static int rightBound, botBound;
        
        public BallV(int xP, int yP, int xV, int yV, int r)
        {
            position = new Vec2(xP, yP);
            velocity = new Vec2(xV, yV);
            radius = r;
            mass = radius;
        }

        public BallV(int xP, int yP, Vec2 vel, int r)
        {
            position = new Vec2(xP, yP);
            velocity = vel;
            radius = r;
            mass = radius;
        }

        public void Step()
        {
            position.x += velocity.x;
            position.y += velocity.y;

            WallCollisions();
        }

        public void TorusStep()
        {
            position.x += velocity.x;
            position.y += velocity.y;

            TorusWall();
        }

        public void TorusWall()
        {
            if (position.y - radius < 0 - (2 * radius))
            {
                position.y = botBound + radius;
            } 
            else if (position.y + radius > botBound + 2 * radius)
            {
                position.y = 0 - radius;
            }
            
            if (position.x - radius < (0 - 2 * radius))
            {
                position.x = rightBound + radius;
            }
            else if (position.x + radius > rightBound + 2 * radius)
            {
                position.x = 0 - radius;
            }
        }

        public void WallCollisions()
        {
            if (position.y - radius < 0)
            {
                position.y = radius;
                velocity.y = (int)-(velocity.y*.8);
            }
            else if (position.y + radius > botBound)
            {
                position.y = botBound - radius;
                velocity.y = (int)(-(velocity.y)*.8);
            }

            if (position.x - radius < 0)
            {
                position.x = radius;
                velocity.x = (int)(-(velocity.x)*.8);
            }
            else if (position.x + radius > rightBound)
            {
                position.x = rightBound - radius;
                velocity.x = (int)(-(velocity.x)*.8);
            }
        }

        //old collision checking
        public bool BallsColliding(BallV b)
        {
            float xd = position.x - b.position.x;
            float yd = position.y - b.position.y;

            float sqrDist = (xd * xd) + (yd * yd);

            int sumRad = radius + b.radius;
            int sqrRad = sumRad * sumRad;

            if (sqrDist <= sqrRad)
                return true;

            return false;
        }

        public bool UnderEffect(Body b)
        {
            float xd = position.x - b.position.x;
            float yd = position.y - b.position.y;

            float sqrDist = (xd * xd) + (yd * yd);

            int sumRad = radius + b.aoeRadius;
            int sqrRad = sumRad * sumRad;

            if (sqrDist <= sqrRad)
                return true;

            return false;
        }

        public void gravitate(Body b)
        {
            Vec2 delta = (this.position.Subtract(b.position));
            delta.Normalize();
            delta = delta.Multiply((float)b.localGravity);

            this.velocity = this.velocity.Subtract(delta);
        }

        public void courseCollision(BallV b)
        {            
            //new collision checking - rolled in for efficiency
            Vec2 delta = (this.position.Subtract(b.position));
            float r = this.radius + b.radius;
            float dist2 = delta.Dot(delta);

            //if (dist2 > r * r) return;

            float d = delta.GetLength();

            //mtd - minimum translation distance to avoid sticking
            Vec2 mtd;
            if (d != 0.0f)
            {
                mtd = delta.Multiply(((this.radius + b.radius) - d) / d);
            }
            else //special, balls are exact overlap
            {
                d = this.radius + b.radius - 1.0f;
                delta = new Vec2(b.radius + this.radius, 0.0f);

                mtd = delta.Multiply(((this.radius + b.radius) - d) / d) ;
            }
            
            //inverse mass quantities
            float im1 = 1.0f / this.mass;
            float im2 = 1.0f / b.mass;

            //push-pull apart
            this.position = this.position.Add(mtd.Multiply(im1 / (im1 + im2)));
            b.position = b.position.Subtract(mtd.Multiply(im2 / (im1 + im2)));

            //impact speed
            Vec2 v = (this.velocity.Subtract(b.velocity));
            float vn = v.Dot(mtd.Normalize());

            //intersecting but already receding
            if (vn > 0.0f) return;

            //impulse on collision
            float i = (-(1.0f + 0.80f) * vn) / (im1 + im2);
            Vec2 impulse = mtd.Multiply(i);

            //momentum change
            this.velocity = this.velocity.Add(impulse.Multiply(im1));
            b.velocity = b.velocity.Subtract(impulse.Multiply(im2));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            BallV b = (BallV)obj;
            return (position == b.position && velocity == b.velocity);
        }
    }
}
