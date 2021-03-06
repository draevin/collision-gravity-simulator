﻿namespace GameLogic
{
    public class Body : BallV
    {
        public float localGravity;
        public int aoeRadius;

        public Body(int xP, int yP, int xV, int yV, int r) : base(xP, yP, xV, yV, r)
        {
            position = new Vec2(xP, yP);
            velocity = new Vec2(xV, yV);
            radius = r;
            mass = radius;
            aoeRadius = radius * 10;
            localGravity = (float)mass/300.0f;
        }


    }
}
