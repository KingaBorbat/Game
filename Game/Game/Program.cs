using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Game
{
    internal class Program
    {
        private static CameraDescriptor camera = new();

        private static Skybox skybox;
        private static List<GlObject> trees = new();
        private static List<string> treeNames = new();
        private static GlObject rock;

        private static Map map;

        private static GL Gl;

        private static IWindow window;

        private static IInputContext inputContext;

        private static uint program;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        private static float Shininess = 50;
        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Jungle Game";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();

            window.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.White);
            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void Window_Render(double obj)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            DrawObjects();
        }
        private static void Window_Update(double obj)
        {


        }
        private static void Window_Closing()
        {
            skybox.ReleaseGlObject();
            map.ReleaseGlObject();
            rock.ReleaseGlObject();
            for (int i = 0; i < trees.Count; i++) trees[i].ReleaseGlObject();
        }

        // set up objects
        private static void SetUpObjects()
        {
            skybox = Skybox.CreateSkybox(Gl, "");
            map = Map.CreateMap(Gl, "ground4.png");
            rock = ObjectResourceReader.CreateObjWithColor(Gl, "Rock_1.OBJ", "rock.jpg");
            for (int i = 0; i < 9; i++)
            {
                int x = i + 1;
                string name = "Simple_Tree_0" + x + ".obj";
                GlObject t = ObjectResourceReader.CreateObjWithColor(Gl, name, "Lp_tree_bake_DefaultMaterial_BaseColor.png");
                trees.Add(t);
            }
            MapObjectRandomizer.GenerateEdgeTrees();
            MapObjectRandomizer.GenerateTrees();
        }

        // compile vertex and fragment shaders, link them into shader program
        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        // handle keyboard input
        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Up:
                    camera.MoveForward();
                    break;
                case Key.Down:
                    camera.MoveBackward();
                    break;
                case Key.Left:
                    camera.MoveUp();
                    break;
                case Key.Right:
                    camera.MoveDown();
                    break;
                case Key.A:
                    camera.RotateLeft();
                    break;
                case Key.D:
                    camera.RotateRight();
                    break;
                case Key.W:
                    camera.RotateUp();
                    break;
                case Key.S:
                    camera.RotateDown();
                    break;
            }
        }

        public static void DrawObjects()
        {
            Matrix4X4<float> scale = Matrix4X4.CreateScale(200f);
            var trans = Matrix4X4.CreateTranslation(0f, 30f, 0f);
            Matrix4X4<float> modelMatrix = scale * trans;
            DrawObject(skybox, modelMatrix);
            scale = Matrix4X4.CreateScale(200f, 1f, 200f);
            modelMatrix = scale;
            DrawObject(map, modelMatrix);

            for (int i = 0; i < MapObjectRandomizer.edgeTreesModelMatrices.Count; i++)
            {
                DrawObject(trees[MapObjectRandomizer.edgeTreeIndices[i]], MapObjectRandomizer.edgeTreesModelMatrices[i]);
            }

            for (int i = 0; i < MapObjectRandomizer.treesModelMatrices.Count; i++) {
                DrawObject(trees[MapObjectRandomizer.treeIndices[i]], MapObjectRandomizer.treesModelMatrices[i]);
            }
            scale = Matrix4X4.CreateScale(10f);
        }

        private static unsafe void DrawObject(GlObject obj, Matrix4X4<float> modelMatrix)
        {
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(obj.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, obj.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, obj.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        // read given shader
        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Game.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }


        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 0f, 50f, 10f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, camera.Position.X, camera.Position.Y, camera.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }


        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }
        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.Up);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}