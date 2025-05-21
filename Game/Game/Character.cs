using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Game
{
    internal class Character
    {
        public GlObject obj;
        public Vector3D<float> position; 
        public float rotationY;
        private GL gl;
        private Vector3D<float> forward;
        private Vector3D<float> right;
        private float moveSpeed = 0.1f;
        private float rotationValue = (float)Math.PI / 180;

        public Character(GL gl)
        {
            this.gl = gl;
        }
        public void InitializeCharacter()
        {
            obj = ObjectResourceReader.CreateObjWithtexture(gl, "lizard2.obj", "lizard.jpg");
            position = new Vector3D<float>(0f, 1f, 0f);
            rotationY =0f;
        }

        public void MoveForward()
        {
            CalculateVectors();
            Vector3D<float> nextPos = position + (right * moveSpeed);
            if (!MapObjectRandomizer.CheckCollision(nextPos)) position = nextPos;
        }

        public void MoveBackward() {
            CalculateVectors();
            Vector3D<float> nextPos = position - (right * moveSpeed);
            if (!MapObjectRandomizer.CheckCollision(nextPos)) position = nextPos;
        }

        public void RotateRight()
        {
            rotationY -= rotationValue;
        }

        public void RotateLeft() {
            rotationY += rotationValue;
        }

        private void CalculateVectors()
        {
            forward = new Vector3D<float>(
                (float)Math.Sin(rotationY),
                0,
                (float)Math.Cos(rotationY)

            );

            right = new Vector3D<float>(
                (float)Math.Cos(rotationY),
                0,
                -(float)Math.Sin(rotationY)
            );
        }
    }
}
