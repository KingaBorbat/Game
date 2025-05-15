using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using StbImageSharp;

namespace Game
{
    internal class ObjectResourceReader
    {
        public static unsafe GlObject CreateObjWithColor(GL Gl, float[] faceColor, string objResource, string textureResource)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<int[]> objFaces;
            List<float[]> objNormals;
            List<int[]> normalIndices;
            List<float[]> objTextures;
            List<int[]> textureIndices;

            ReadObjData(out objVertices, out objFaces, out objNormals, out normalIndices, out objTextures, out textureIndices, objResource);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, objNormals, normalIndices, objTextures, textureIndices, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices, textureResource);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices, string textureResource)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint offsetTexture = offsetNormal + (3 * sizeof(float));
            uint vertexSize = offsetTexture + (2 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();

            uint texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2D, texture);

            // load and apply texture
            var skyboxImageResult = ReadTextureImage(textureResource);
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
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, texture, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(
            float[] faceColor,
            List<float[]> objVertices,
            List<int[]> objFaces,
            List<float[]> objNormals,
            List<int[]> normalIndices,
            List<float[]> objTextures,
            List<int[]> texturesIndices,
            List<float> glVertices,
            List<float> glColors,
            List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();
            var vertexNormals = new Dictionary<int, Vector3D<float>>();
            var normalsProvided = (objNormals.Count > 0) ? true : false;
            var texturesProvided = (objTextures.Count > 0) ? true : false;
            Vector3D<float> normal = default;
            for (int index = 0; index < objFaces.Count; index++)
            {
                var objFace = objFaces[index];
                
                if (!normalsProvided)
                {
                    var aObjVertex = objVertices[objFace[0] - 1];
                    var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                    var bObjVertex = objVertices[objFace[1] - 1];
                    var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                    var cObjVertex = objVertices[objFace[2] - 1];
                    var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);

                    normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));
                }

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i] - 1];

                    // create gl description of vertex
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);

                    // if the normals were given
                    if (normalsProvided)
                    {
                        var objFaceNormals = normalIndices[index];
                        var normalIndex = objFaceNormals[i] - 1;
                        glVertex.Add(objNormals[normalIndex][0]);
                        glVertex.Add(objNormals[normalIndex][1]);
                        glVertex.Add(objNormals[normalIndex][2]);
                    }
                    // else use the calculated ones
                    else
                    {
                        glVertex.Add(normal.X);
                        glVertex.Add(normal.Y);
                        glVertex.Add(normal.Z);
                    }

                    if (texturesProvided) {
                        var objFaceTextures = texturesIndices[index];
                        var texIndex = objFaceTextures[i] - 1;
                        glVertex.Add(objTextures[texIndex][0]);
                        glVertex.Add(objTextures[texIndex][1]);
                    }

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }


        private static unsafe void ReadObjData(
            out List<float[]> objVertices,
            out List<int[]> objFaces,
            out List<float[]> objNormals,
            out List<int[]> normalIndices,
            out List<float[]> objTextures,
            out List<int[]> textureIndices,
            string objresource)
        {
            objVertices = new List<float[]>();
            objFaces = new List<int[]>();
            objNormals = new List<float[]>();
            objTextures = new List<float[]>();
            normalIndices = new List<int[]>();
            textureIndices = new List<int[]>();

            using (Stream objStream = typeof(ObjectResourceReader).Assembly.GetManifestResourceStream("Game.Resources.Objects." + objresource))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();
                    if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("#"))
                        continue;

                    var lineClassifier = line.Substring(0, line.IndexOf(' '));
                    var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');
                    switch (lineClassifier)
                    {
                        case "v":
                            float[] vertex = new float[3];
                            for (int i = 0; i < 3; ++i)
                                vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            objVertices.Add(vertex);
                            break;
                        case "vn":
                            float[] normal = new float[3];
                            for (int i = 0; i < 3; ++i)
                            {
                                normal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            }
                            objNormals.Add(normal);
                            break;
                        case "vt":
                            float[] texture = new float[2];
                            
                            for (int i = 0; i < 2; ++i)
                            {
                                texture[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                            }
                            texture[1] = 1f - texture[1];
                            
                            objTextures.Add(texture);
                            break;
                        case "f":
                            List<int> faceVertices = new List<int>();
                            List<int> normIndices = new List<int>();
                            List<int> textIndices = new List<int>();
                            foreach (var item in lineData)
                            {
                                var parts = item.Split('/');
                                faceVertices.Add(int.Parse(parts[0]));
                                if(parts.Length == 2)
                                {
                                    textIndices.Add(int.Parse(parts[1]));
                                }
                                else
                                {
                                    normIndices.Add(int.Parse(parts[2]));
                                    if (parts[1] != "")
                                    {
                                        textIndices.Add(int.Parse(parts[1]));
                                    }
                                }
                            }

                            if (faceVertices.Count == 3)
                            {
                                objFaces.Add(new int[] { faceVertices[0], faceVertices[1], faceVertices[2] });  
                                if(normIndices.Count > 0)
                                {
                                    normalIndices.Add(new int[] { normIndices[0], normIndices[1], normIndices[2] });
                                }
                                if(textIndices.Count > 0)
                                {
                                    textureIndices.Add(new int[] { textIndices[0], textIndices[1], textIndices[2] });
                                }
                            }
                            else if (faceVertices.Count == 4)
                            {
                                objFaces.Add(new int[] { faceVertices[0], faceVertices[1], faceVertices[2] });
                                objFaces.Add(new int[] { faceVertices[0], faceVertices[2], faceVertices[3] });
                                if (normIndices.Count > 0)
                                {
                                    normalIndices.Add(new int[] { normIndices[0], normIndices[1], normIndices[2] });
                                    normalIndices.Add(new int[] { normIndices[0], normIndices[2], normIndices[3] });
                                }
                                if (textIndices.Count > 0)
                                {
                                    textureIndices.Add(new int[] { textIndices[0], textIndices[1], textIndices[2] });
                                    textureIndices.Add(new int[] { textIndices[0], textIndices[2], textIndices[3] });
                                }
                            }
                            
                            break;
                      
                    }
                }
            }
        }

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
