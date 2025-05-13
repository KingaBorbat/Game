using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace Game
{
    // map
    internal class Map : GlObject
    {
        public uint? Texture {  get; set; }
        private Map(uint vao, uint vertices, uint colors, uint indices, uint indexArrayLength, GL gl, uint texture = 0) :
            base(vao, vertices, colors, indices, indexArrayLength, gl)  
        { 
            Texture = texture;
        }

        // create map
        public static unsafe Map CreateMap(GL Gl, string textureResource)
        {
            // generate vao
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            // defien vertex positions
            float[] vertexArray = new float[]
            {
                -100f, 0f, 100f, 0f, 1f, 0f, 0f, 1f, 0f, 
                100f, 0f, 100f, 0f, 1f, 0f, 1f, 1f, 0f,
                100f, 0f, -100f, 0f, 1f, 0f, 1f, 0f, 0f,
                -100f, 0f, -100f, 0f, 1f, 0f, 0f, 0f, 0f
            };

            // define index array for triangles
            uint[] indexArray = new uint[] {
                0, 1, 2,
                0, 2, 3,
            };

            // set buffer offsets
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (3 * sizeof(float));

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
            var mapTextureResult = ReadTextureImage(textureResource);
            var textureBytes = (ReadOnlySpan<byte>)mapTextureResult.Data.AsSpan();
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)mapTextureResult.Width,
                (uint)mapTextureResult.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
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

            return new Map(vao, vertices, colors, indices, indexArrayLength, Gl, texture);
        }

        // method to load texture image from embedde resources
        private static unsafe ImageResult ReadTextureImage(string textureResource)
        {
            ImageResult result;
            using (Stream skyeboxStream
                = typeof(Map).Assembly.GetManifestResourceStream("Game.Resources." + textureResource))
                result = ImageResult.FromStream(skyeboxStream, ColorComponents.RedGreenBlueAlpha);

            return result;
        }
    }
}
