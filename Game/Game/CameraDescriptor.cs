using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

// camera that follows the character in third person or first person
namespace Game
{
    internal class CameraDescriptor
    {
        private Character character;
        private bool isFirstPerson = false;
        private const float DistanceBehind = 6f;
        private const float HeightAbove = 1.5f;
        private const float EyeHeight = 1.7f;

        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Target { get; private set; }
        public Vector3D<float> Up { get; } = Vector3D<float>.UnitY;

        public CameraDescriptor(Character character)
        {
            this.character = character;
            UpdatePosition();
        }

        public void ToggleView()
        {
            isFirstPerson = !isFirstPerson;
            UpdatePosition();
        }

        public void SetView(bool cameraView)
        {
            isFirstPerson = cameraView;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if (isFirstPerson)
            {
                Position = new Vector3D<float>(
                    character.position.X,
                    character.position.Y + EyeHeight,
                    character.position.Z
                );

                Target = Position + new Vector3D<float>(
                    (float)Math.Cos(character.rotationY), 
                    0,
                    -(float)Math.Sin(character.rotationY)
                );
            }
            else
            {
                float behindX = character.position.X - DistanceBehind * (float)Math.Cos(-character.rotationY);
                float behindZ = character.position.Z - DistanceBehind * (float)Math.Sin(-character.rotationY);

                Position = new Vector3D<float>(
                    behindX,
                    character.position.Y + HeightAbove,
                    behindZ
                );
                Target = character.position;
            }
            
        }
    }
}
