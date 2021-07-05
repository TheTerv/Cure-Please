using System;

namespace CurePlease.Model
{
    public class Coordinates
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Coordinates() { }

        public Coordinates(float x, float y, float z)
        {
            UpdateCoordinates(x, y, z);
        }

        public void UpdateCoordinates(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double GetDistanceFrom(float x, float y, float z)
        {
            float genX = X - x;
            float genY = Y - y;
            float genZ = Z - z;

            return Math.Sqrt(genX * genX + genY * genY + genZ * genZ);
        }
    }
}
