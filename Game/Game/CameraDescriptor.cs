using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace Game
{
    internal class CameraDescriptor
    {
        public Vector3D<float> Position { get; private set; } = new(0, 10, 0);
        public double HorizontalAngle { get; private set; } = 0.0;
        public double VerticalAngle { get; private set; } = -Math.PI / 2;

        private const float MoveSpeed = 0.5f;
        private const double AngleStep = Math.PI / 180 * 5;

        public Vector3D<float> Forward => GetPointsFromAngles(HorizontalAngle, VerticalAngle);
        public Vector3D<float> Right => Vector3D.Normalize(GetPointsFromAngles(HorizontalAngle, VerticalAngle + Math.PI / 2));
        public Vector3D<float> Up => Vector3D.Normalize(GetPointsFromAngles(HorizontalAngle + Math.PI / 2, VerticalAngle));

        public Vector3D<float> Target => Position + Forward;

        public void MoveForward()
        {
            Position += Forward * MoveSpeed;
        }

        public void MoveBackward()
        {
            Position -= Forward * MoveSpeed;
        }

        public void MoveUp()
        {
            Position += Up * MoveSpeed;
        }


        public void MoveDown()
        {
            Position -= Up * MoveSpeed;
        }

        public void RotateUp()
        {
            HorizontalAngle += AngleStep;
        }

        public void RotateDown()
        {
            HorizontalAngle -= AngleStep;
        }

        public void RotateLeft()
        {
            VerticalAngle -= AngleStep;
        }

        public void RotateRight()
        {
            VerticalAngle += AngleStep;
        }

        private Vector3D<float> GetPointsFromAngles(double Pitch, double Yaw)
        {
            var x = Math.Cos(Pitch) * Math.Cos(Yaw);
            var y = Math.Sin(Pitch);
            var z = Math.Cos(Pitch) * Math.Sin(Yaw);

            return Vector3D.Normalize(new Vector3D<float>((float)x, (float)y, (float)z));
        }


    }
}
