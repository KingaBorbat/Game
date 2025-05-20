using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static System.Formats.Asn1.AsnWriter;

namespace Game
{
    internal class MapObjectRandomizer
    {
        public static List<int> edgeTreeIndices = new();
        public static List<Matrix4X4<float>> edgeTreesModelMatrices = new();
        public static List<int> treeIndices = new();
        private static List<Vector2D<float>> treeCoordinates = new();
        public static List<Matrix4X4<float>> treesModelMatrices = new();
        public static List<Matrix4X4<float>> plantsModelMatrices = new();
        public static List<Matrix4X4<float>> rocksModelMatrices = new();
        public static List<Vector2D<float>> obstacleCoordinates = new();
        public static List<Vector2D<float>> mushroomPositions = new();
        public static List<Vector2D<float>> glowwormPositions = new();
        private static int limit = 170;
        private static float minDistance = 20;

        private static readonly Random rand = new();

        internal static void GenerateEdgeTrees()
        {
            int width = 190;
            for (int x = -width; x <= width; x += 10)
            {
                int index = rand.Next(0, 9);
                float rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                var scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                var trans = Matrix4X4.CreateTranslation(new Vector3D<float>((float)x, 0, (float)width));
                var rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                var modelMatrix = scale * rotY * trans;
                edgeTreesModelMatrices.Add(modelMatrix);
                edgeTreeIndices.Add(index);

                index = rand.Next(0, 9);
                rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                trans = Matrix4X4.CreateTranslation(new Vector3D<float>((float)x, 0, -(float)width));
                rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                modelMatrix = scale * rotY * trans;
                edgeTreesModelMatrices.Add(modelMatrix);
                edgeTreeIndices.Add(index);

                index = rand.Next(0, 9);
                rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                trans = Matrix4X4.CreateTranslation(new Vector3D<float>((float)width, 0, (float)x));
                rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                modelMatrix = scale * rotY * trans;
                edgeTreesModelMatrices.Add(modelMatrix);
                edgeTreeIndices.Add(index);

                index = rand.Next(0, 9);
                rotate = rand.Next(-19, 19);
                while (rotate == 0)
                {
                    rotate = rand.Next(-19, 19);
                }
                scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                trans = Matrix4X4.CreateTranslation(new Vector3D<float>(-(float)width, 0, (float)x));
                rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);

                modelMatrix = scale * rotY * trans;
                edgeTreesModelMatrices.Add(modelMatrix);
                edgeTreeIndices.Add(index);


            }
        }

        internal static void GenerateTrees()
        {
            int n = 20;
            int i = 0;
            List<Vector2D<float>> generatedCoordinates = new();

            while(i < n)
            {
                float x = rand.Next(-limit, limit+1);
                float z = rand.Next(-limit, limit+1);
                var coord = new Vector2D<float>(x, z);

                bool invalid = obstacleCoordinates.Any(c => Vector2D.Distance(coord, c) < minDistance);

                if(!invalid)
                {
                    generatedCoordinates.Add(coord);
                    obstacleCoordinates.Add(coord);
                    i++;
                }
            }

            foreach (Vector2D<float> coord in generatedCoordinates) {
                var index = rand.Next(0, 5);
                var rotate = rand.Next(-19, 19);

                var scale = Matrix4X4.CreateScale(1f, 1f, 1f);
                var trans = Matrix4X4.CreateTranslation(new Vector3D<float>(coord.X, 0, coord.Y));
                var rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);
                var modelMatrix = scale * rotY * trans;
                treesModelMatrices.Add(modelMatrix);
                treeIndices.Add(index);
            }
            treeCoordinates = generatedCoordinates;
        }

        internal static void GeneratePlants()
        {
            int n = 100;
            int i = 0;
            List<Vector2D<float>> generatedCoordinates = new();

            while (i < n)
            {
                float x = rand.Next(-limit, limit + 1);
                float z = rand.Next(-limit, limit + 1);
                var coord = new Vector2D<float>(x, z);

                bool invalid = obstacleCoordinates.Any(c => Vector2D.Distance(coord, c) < minDistance);

                if (!invalid)
                {
                    generatedCoordinates.Add(coord);
                    obstacleCoordinates.Add(coord);
                    i++;
                }
            }
            float scaleValue = 5;
            foreach (Vector2D<float> coord in generatedCoordinates)
            {
                var index = rand.Next(0, 5);
                var rotate = rand.Next(-19, 19);

                var scale = Matrix4X4.CreateScale(scaleValue);
                var trans = Matrix4X4.CreateTranslation(new Vector3D<float>(coord.X, 0, coord.Y));
                var rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);
                var modelMatrix = scale * rotY * trans;
                plantsModelMatrices.Add(modelMatrix);
            }
        }

        internal static void GenerateRocks()
        {
            int n = 30;
            int i = 0;
            List<Vector2D<float>> generatedCoordinates = new();

            while (i < n)
            {
                float x = rand.Next(-limit, limit + 1);
                float z = rand.Next(-limit, limit + 1);
                var coord = new Vector2D<float>(x, z);

                bool invalid = obstacleCoordinates.Any(c => Vector2D.Distance(coord, c) < minDistance);

                if (!invalid)
                {
                    generatedCoordinates.Add(coord);
                    obstacleCoordinates.Add(coord);
                    i++;
                }
            }
            foreach (Vector2D<float> coord in generatedCoordinates)
            {
                var rotate = rand.Next(-19, 19);
                float scaleValue = 2f;
                var scale = Matrix4X4.CreateScale(scaleValue);
                var trans = Matrix4X4.CreateTranslation(new Vector3D<float>(coord.X, 0, coord.Y));
                var rotY = Matrix4X4.CreateRotationY((float)Math.PI / rotate);
                var modelMatrix = scale * rotY * trans;
                rocksModelMatrices.Add(modelMatrix);
            }
        }

        internal static void GenerateMushrooms()
        {
            int n = 15;
            int i = 0;
            List<Vector2D<float>> generatedCoordinates = new();

            while (i < n)
            {
                float x = rand.Next(-limit, limit + 1);
                float z = rand.Next(-limit, limit + 1);
                var coord = new Vector2D<float>(x, z);

                bool invalid = obstacleCoordinates.Any(c => Vector2D.Distance(coord, c) < minDistance);

                if (!invalid)
                {
                    generatedCoordinates.Add(coord);
                    obstacleCoordinates.Add(coord);
                    i++;
                }
            }

            foreach (Vector2D<float> coord in generatedCoordinates)
            {
                mushroomPositions.Add(coord);
            }
        }

        internal static void GenerateGlowworms()
        {
            int n = 100;
            int i = 0;
            List<Vector2D<float>> generatedCoordinates = new();

            while (i < n)
            {
                float x = rand.Next(-limit, limit + 1);
                float z = rand.Next(-limit, limit + 1);
                var coord = new Vector2D<float>(x, z);

                bool invalid = treeCoordinates.Any(c => Vector2D.Distance(coord, c) < minDistance) && generatedCoordinates.Any(c => Vector2D.Distance(coord, c) < minDistance);

                if (!invalid)
                {
                    generatedCoordinates.Add(coord);
                    i++;
                }
            }

            foreach (Vector2D<float> coord in generatedCoordinates)
            {
                glowwormPositions.Add(coord);
            }
        }
    }
}