using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Game
{
    internal class Skybox : GlObject
    {
        public uint? Texture {  get; set; }
        private Skybox(uint vao, uint vertices, uint colors, uint indeces, uint indexArrayLength, GL gl, uint texture = 0)
            : base(vao, vertices, colors, indeces, indexArrayLength, gl)
        {
            Texture = texture;
        }
        
        // skybox creation
        public unsafe static Skybox CreateSkybox(GL Gl, string textureResource)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // define vertex positions
            float[] vertexArray = new float[] {
                // top 
                -1f, 1f, 1f, 0f, -1f, 0f, 1f/4f, 0f/3f,
                1f, 1f, 1f, 0f, -1f, 0f, 2f/4f, 0f/3f,
                1f, 1f, -1f, 0f, -1f, 0f, 2f/4f, 1f/3f,
                -1f, 1f, -1f, 0f, -1f, 0f, 1f/4f, 1f/3f,

                // front 
                -1f, 1f, 1f, 0f, 0f, -1f, 1, 1f/3f,
                -1f, -1f, 1f, 0f, 0f, -1f, 4f/4f, 2f/3f,
                1f, -1f, 1f, 0f, 0f, -1f, 3f/4f, 2f/3f,
                1f, 1f, 1f, 0f, 0f, -1f,  3f/4f, 1f/3f,

                // left 
                -1f, 1f, 1f, 1f, 0f, 0f, 0, 1f/3f,
                -1f, 1f, -1f, 1f, 0f, 0f,1f/4f, 1f/3f,
                -1f, -1f, -1f, 1f, 0f, 0f, 1f/4f, 2f/3f,
                -1f, -1f, 1f, 1f, 0f, 0f, 0f/4f, 2f/3f,

                // bottom 
                -1f, -1f, 1f, 0f, 1f, 0f, 1f/4f, 1f,
                1f, -1f, 1f,0f, 1f, 0f, 2f/4f, 1f,
                1f, -1f, -1f,0f, 1f, 0f, 2f/4f, 2f/3f,
                -1f, -1f, -1f,0f, 1f, 0f, 1f/4f, 2f/3f,

                // back 
                1f, 1f, -1f, 0f, 0f, 1f, 2f/4f, 1f/3f,
                -1f, 1f, -1f, 0f, 0f, 1f, 1f/4f, 1f/3f,
                -1f, -1f, -1f,0f, 0f, 1f, 1f/4f, 2f/3f,
                1f, -1f, -1f,0f, 0f, 1f, 2f/4f, 2f/3f,

                // right
                1f, 1f, 1f, -1f, 0f, 0f, 3f/4f, 1f/3f,
                1f, 1f, -1f,-1f, 0f, 0f, 2f/4f, 1f/3f,
                1f, -1f, -1f, -1f, 0f, 0f, 2f/4f, 2f/3f,
                1f, -1f, 1f, -1f, 0f, 0f, 3f/4f, 2f/3f,
            };

            // define index array for triangles
            uint[] indexArray = new uint[] {
                0, 2, 1,
                0, 3, 2,

                4, 6, 5,
                4, 7, 6,

                8, 10, 9,
                10, 8, 11,

                12, 13, 14,
                12, 14, 15,

                17, 19, 16,
                17, 18, 19,

                20, 21, 22,
                20, 22, 23
            };

            // set buffer offsets
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            
            // activate and bind texture
            uint texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, texture);

            // load and apply texture
            var skyboxImageResult = ReadTextureImage("skybox.png");
            var textureBytes = (ReadOnlySpan<byte>)skyboxImageResult.Data.AsSpan();
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)skyboxImageResult.Width,
                (uint)skyboxImageResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Gl.BindTexture(TextureTarget.Texture2D, 0);

            Gl.EnableVertexAttribArray(3);
            Gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetTexture);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)indexArray.Length;

            return new Skybox(vao, vertices, colors, indices, indexArrayLength, Gl, texture);
        }

        // method to load texture image from embedde resources
        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(Skybox).Assembly.GetManifestResourceStream("Game.Resources.Textures." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }

    }
}
