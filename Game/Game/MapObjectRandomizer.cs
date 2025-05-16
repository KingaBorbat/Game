using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using static System.Formats.Asn1.AsnWriter;

namespace Game
{
    internal class MapObjectRandomizer
    {
        public static List<int> treeIndices = new();
        public static List<Matrix4X4<float>> modelMatrices = new();

        internal static void Generate()
        {
            Random rand = new Random();
            for (int x = -90; x <= 90; x += 15)
            {
                int index = rand.Next(0, 9);
                float rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                var scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                var trans = Matrix4X4.CreateTranslation(new Vector3D<float>((float)x, 0, 90f));
                var rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                var modelMatrix = scale * rotY * trans;
                modelMatrices.Add(modelMatrix);
                treeIndices.Add(index);

                index = rand.Next(0, 9);
                rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                trans = Matrix4X4.CreateTranslation(new Vector3D<float>((float)x, 0, -90f));
                rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                modelMatrix = scale * rotY * trans;
                modelMatrices.Add(modelMatrix);
                treeIndices.Add(index);

                index = rand.Next(0, 9);
                rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                trans = Matrix4X4.CreateTranslation(new Vector3D<float>(90f, 0, (float)x));
                rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                modelMatrix = scale * rotY * trans;
                modelMatrices.Add(modelMatrix);
                treeIndices.Add(index);

                index = rand.Next(0, 9);
                rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                trans = Matrix4X4.CreateTranslation(new Vector3D<float>(-90f, 0, (float)x));
                rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                modelMatrix = scale * rotY * trans;
                modelMatrices.Add(modelMatrix);
                treeIndices.Add(index);


            }
        }
    }
}