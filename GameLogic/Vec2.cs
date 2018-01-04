using System;

namespace GameLogic
{
    public class Vec2
    {
        public float x;
        public float y;

        public Vec2()
        {
            x = 0;
            y = 0;
        }

        public Vec2(float X, float Y)
        {
            x = X;
            y = Y;
        }

        public float Dot(Vec2 v2)
        {
            float result = this.x * v2.x + this.y * v2.y;
            return result;
        }

        public float GetLength()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public float GetDistance(Vec2 v2)
        {
            return (float)Math.Sqrt((v2.x - this.x) * (v2.x - this.x) + (v2.y - this.y) * (v2.y - this.y));
        }

        public Vec2 Add (Vec2 v2)
        {
            Vec2 result = new Vec2();
            result.x = this.x + v2.x;
            result.y = this.y + v2.y;
            return result;
        }

        public Vec2 Subtract(Vec2 v2)
        {
            Vec2 result = new Vec2();
            result.x = this.x - v2.x;
            result.y = this.y - v2.y;
            return result;
        }

        public Vec2 Multiply(float scalefactor)
        {
            Vec2 result = new Vec2();
            result.x = this.x * scalefactor;
            result.y = this.y * scalefactor;
            return result;
        }

        public Vec2 Normalize()
        {
            float len = this.GetLength();
            if (len != 0.0f)
            {
                this.x = (this.x / len); 
                this.y = (this.y / len);
            }
            else
            {
                this.x = 0.0f;
                this.y = 0.0f;
            }

            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Vec2 v2 = (Vec2)obj;
            return (this.x == v2.x && this.y == v2.y);
        }

        public override int GetHashCode()
        {
            int result = 17;

            int xCode = BitConverter.ToInt32(BitConverter.GetBytes(this.x),0);
            result = 17 * result + xCode;

            int yCode = BitConverter.ToInt32(BitConverter.GetBytes(this.y), 0);
            result = 17 * result + yCode;

            return result;
        }
    }
}
