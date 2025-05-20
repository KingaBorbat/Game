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
        private Character _character;

        private const float DistanceBehind = 6f;
        private const float HeightAbove = 1.5f;

        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Target => _character.position;
        public Vector3D<float> Up { get; } = Vector3D<float>.UnitY;

        public CameraDescriptor(Character character)
        {
            _character = character;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            float behindX = _character.position.X - DistanceBehind * (float)Math.Cos(-_character.rotationY);
            float behindZ = _character.position.Z - DistanceBehind * (float)Math.Sin(-_character.rotationY);

            Position = new Vector3D<float>(
                behindX,
                _character.position.Y + HeightAbove,
                behindZ
            );
        }
    }
}
