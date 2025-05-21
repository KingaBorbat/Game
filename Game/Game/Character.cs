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


    }
}
