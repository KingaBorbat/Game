using ImGuiNET;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using ErrorCode = Silk.NET.OpenGL.ErrorCode;

namespace Game
{
    internal class Program
    {
        private static CameraDescriptor camera;

        private static Animation animation = new Animation();

        private static ImGuiController controller;

        private static GL Gl;

        private static IWindow window;

        private static IInputContext inputContext;

        private static uint program;

        private static HashSet<Key> pressedKeys = new HashSet<Key>();

        private static Skybox skybox;
        private static Map map;
        private static List<GlObject> trees = new();
        private static List<string> treeNames = new();
        private static GlObject rock;
        private static GlObject agave;
        private static GlObject mushroom;
        private static GlObject glowworm;
        private static Character character;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "uLightColor";
        private const string LightPositionVariableName = "uLightPos";
        private const string ViewPositionVariableName = "uViewPos";

        private const string AmbientStrength = "uAmbientStrength";
        private const string DiffuseStrength = "uDiffuseStrength";
        private const string SpecularStrength = "uSpecularStrength";

        private const string ShinenessVariableName = "uShininess";
        private const string EmissiveVariableName = "uEmissiveColor";
        private const string UseEmissiveVariableName = "uUseEmissive";
        private const string UseTextureVariableName = "uUseTexture";

        private static float Shininess = 50;
        public static float characterRadius = 0.5f;
        public static float collectibleRadius = 1f;
        public static int nrMushrooms = 20;
        public static bool gameOver = false;

        public static bool firstPerson = false;
        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Jungle Game";
            windowOptions.Size = new Vector2D<int>(1920, 1080);

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
                keyboard.KeyUp += Keyboard_KeyUp;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            window.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.White);
            character = new Character(Gl);
            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        
        private static void Window_Update(double deltaTime)
        {
            CheckMushroomCollision();
            if (nrMushrooms == 0)
            {
                gameOver = true;
            }
            camera.UpdatePosition();
            animation.AdvanceTime(deltaTime);
            Keyboard_Reaction();
            controller.Update((float)deltaTime);

        }
        private static void Window_Render(double obj)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();
            SetUnifrom3(LightColorVariableName, new Vector3D<float>(0.3f, 0.4f, 0.7f));
            SetUnifrom3(LightPositionVariableName, new Vector3D<float>(0f, 50f, 0f));
            SetUnifrom3(ViewPositionVariableName, new Vector3D<float>(camera.Position.X, camera.Position.Y, camera.Position.Z));
            SetUnifrom3(AmbientStrength, new Vector3D<float>(0.6f, 0.4f, 0.2f));
            SetUnifrom3(DiffuseStrength, new Vector3D<float>(0.5f, 0.3f, 0.1f));
            SetUnifrom3(SpecularStrength, new Vector3D<float>(0.2f, 0.1f, 0.05f));
            SetUnifrom3(LightColorVariableName, new Vector3D<float>(0.3f, 0.4f, 0.7f));
            SetUniform1(ShinenessVariableName, Shininess);
            DrawObjects();

            ImGuiNET.ImGui.Begin("Camera View");
            if (ImGui.RadioButton("First Person", firstPerson == false))
            {
                firstPerson = true;
                camera.SetView(firstPerson);
            }

            if (ImGui.RadioButton("Third Person", firstPerson == true))
            {
                firstPerson = false;
                camera.SetView(firstPerson);
            }
            ImGuiNET.ImGui.End();

            // if the player collected all mushrooms on the map, show a textbox and close the application if the player clicks "Exit"
            if (gameOver)
            {
                ImGuiNET.ImGui.Begin("You Win!", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);

                ImGuiNET.ImGui.Text("Congratulations! You collected all mushrooms.");

                if (ImGuiNET.ImGui.Button("Exit"))
                {
                    Environment.Exit(0);
                }

                ImGuiNET.ImGui.End();
            }
            controller.Render();
        }

        private static void Window_Closing()
        {
            ReleaseObjects();
        }
        // check if the character collides with a mushroom, decrease the number of mushrooms
        private static void CheckMushroomCollision()
        {
            foreach (Vector2D<float> coord in MapObjectRandomizer.mushroomPositions.ToList())
            {
                Vector3D<float> mushroomPos = new Vector3D<float>(coord.X, 0f, coord.Y);

                float distance = (character.position - mushroomPos).Length;

                if (distance < characterRadius + collectibleRadius)
                {
                    MapObjectRandomizer.mushroomPositions.Remove(coord);
                    nrMushrooms--;
                }
            }
        }

        private static void ReleaseObjects()
        {
            skybox.ReleaseGlObject();
            map.ReleaseGlObject();
            rock.ReleaseGlObject();
            for (int i = 0; i < trees.Count; i++) trees[i].ReleaseGlObject();
            agave.ReleaseGlObject();
            character.obj.ReleaseGlObject();
            mushroom.ReleaseGlObject();
        }

        // set up objects: read obj files and textures, generate random coordinates for obstacles, mushrooms and glowworms
        private static void SetUpObjects()
        {
            skybox = Skybox.CreateSkybox(Gl, "");
            map = Map.CreateMap(Gl, "ground4.png");
            rock = ObjectResourceReader.CreateObjWithtexture(Gl, "Stone.obj", "StoneAlbedo.png");
            agave = ObjectResourceReader.CreateObjWithtexture(Gl, "agave.obj", "agaveTexture.png");
            mushroom = ObjectResourceReader.CreateObjWithColor(Gl, new float[] { 1f, 1f, 0f, 1f }, "mushroom.obj");
            glowworm = ObjectResourceReader.CreateObjWithColor(Gl, new float[] { 1f, 1f, 0f, 1f }, "sphere.obj");
            character.InitializeCharacter();
            camera = new CameraDescriptor(character);
            for (int i = 0; i < 9; i++)
            {
                int x = i + 1;
                string name = "Simple_Tree_0" + x + ".obj";
                GlObject t = ObjectResourceReader.CreateObjWithtexture(Gl, name, "Lp_tree_bake_DefaultMaterial_BaseColor.png");
                trees.Add(t);
            }
            MapObjectRandomizer.GenerateEdgeTrees();
            MapObjectRandomizer.GenerateTrees();
            MapObjectRandomizer.GeneratePlants();
            MapObjectRandomizer.GenerateRocks();
            MapObjectRandomizer.GenerateMushrooms(nrMushrooms);
            MapObjectRandomizer.GenerateGlowworms();
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
        
        // keyboard interactions
        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {

            pressedKeys.Add(key);

            if (key == Key.C)
                camera.ToggleView();
        }
        private static void Keyboard_KeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            pressedKeys.Remove(key);
        }
        private static void Keyboard_Reaction()
        {
            if (pressedKeys.Contains(Key.W))
            {
                character.MoveForward();
            }
            if (pressedKeys.Contains(Key.S))
            {
                character.MoveBackward();
            }
            if (pressedKeys.Contains(Key.A))
            {
                character.RotateLeft();

            }
            if (pressedKeys.Contains(Key.D))
            {
                character.RotateRight();
            }
            if (pressedKeys.Count > 0)
            {
                camera.UpdatePosition();
            }
        }

        // draw all objects on the map
        public static void DrawObjects()
        {
            // set uUseEmissive and uUseTexture for textured objects
            SetUniform1(UseEmissiveVariableName, 0);
            SetUniform1(UseTextureVariableName, 1);

            // draw skybox
            Matrix4X4<float> scale = Matrix4X4.CreateScale(400f);
            var trans = Matrix4X4.CreateTranslation(0f, 30f, 0f);
            Matrix4X4<float> modelMatrix = scale * trans;
            DrawObjectWithTexture(skybox, modelMatrix);

            // draw map
            scale = Matrix4X4.CreateScale(200f, 1f, 200f);
            modelMatrix = scale;
            DrawObjectWithTexture(map, modelMatrix);

            // draw character
            Vector3D<float> pos = character.position;
            trans = Matrix4X4.CreateTranslation(pos.X, pos.Y, pos.Z);
            var rotY = Matrix4X4.CreateRotationY(character.rotationY);
            scale = Matrix4X4.CreateScale(2f);
            DrawObjectWithTexture(character.obj, scale * rotY * trans);

            // draw edge trees
            for (int i = 0; i < MapObjectRandomizer.edgeTreesModelMatrices.Count; i++)
            {
                DrawObjectWithTexture(trees[MapObjectRandomizer.edgeTreeIndices[i]], MapObjectRandomizer.edgeTreesModelMatrices[i]);
            }
            // draw obstacles: trees, agaves and rocks
            for (int i = 0; i < MapObjectRandomizer.treesModelMatrices.Count; i++)
            {
                DrawObjectWithTexture(trees[MapObjectRandomizer.treeIndices[i]], MapObjectRandomizer.treesModelMatrices[i]);
            }
            foreach (Matrix4X4<float> modelMatr in MapObjectRandomizer.plantsModelMatrices)
            {
                DrawObjectWithTexture(agave, modelMatr);
            }
            foreach (Matrix4X4<float> modelMatr in MapObjectRandomizer.rocksModelMatrices)
            {
                DrawObjectWithTexture(rock, modelMatr);
            }

            // draw glowing mushrooms
            SetUniform1(UseTextureVariableName, 0);
            SetUniform1(UseEmissiveVariableName, 1);
            SetUnifrom3(EmissiveVariableName, new Vector3D<float>(0.5f, 0.5f, 0f));
            foreach (Vector2D<float> coord in MapObjectRandomizer.mushroomPositions)
            {
                Matrix4X4<float> rotate = Matrix4X4.CreateRotationX(-(float)Math.PI / 2);
                scale = Matrix4X4.CreateScale(0.1f);
                trans = Matrix4X4.CreateTranslation(coord.X, 0f, coord.Y);
                DrawObjectWithColor(mushroom, scale * rotate * trans);
            }

            // draw moving glowworms
            float orbitRadius = 5f;
            scale = Matrix4X4.CreateScale(0.001f);
            for (int i = 0; i < MapObjectRandomizer.glowwormPositions.Count; i += 3)
            {
                float offset = i * i;
                var coord = MapObjectRandomizer.glowwormPositions[i];
                var rot = Matrix4X4.CreateRotationX((float)animation.GlobalXAngle + offset);
                rotY = Matrix4X4.CreateRotationY((float)animation.GlobalYAngle + offset);
                var rotZ = Matrix4X4.CreateRotationZ((float)animation.GlobalZAngle + offset);
                var trans2 = Matrix4X4.CreateTranslation(coord.X, 5f, coord.Y);
                Matrix4X4<float> orbitOffset = Matrix4X4.CreateTranslation(0f, 0f, orbitRadius);
                DrawObjectWithColor(glowworm, scale * orbitOffset * rot * rotY * rotZ * trans2);
            }
        }

        // draw object with texture
        private static unsafe void DrawObjectWithTexture(GlObject obj, Matrix4X4<float> modelMatrix)
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
        // draw object with color
        private static unsafe void DrawObjectWithColor(GlObject obj, Matrix4X4<float> modelMatrix)
        {
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(obj.Vao);
            Gl.DrawElements(GLEnum.Triangles, obj.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
            CheckError();
        }

        // read given shader
        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("Game.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static unsafe void SetUnifrom3(string VariableName, Vector3D<float> value)
        {
            int location = Gl.GetUniformLocation(program, VariableName);

            if (location == -1)
            {
                throw new Exception($"{VariableName} uniform not found on shader.");
            }
            Gl.Uniform3(location, value.X, value.Y, value.Z);
            CheckError();
        }

        private static void SetUniform1(string VariableName, float value) {
            int location = Gl.GetUniformLocation(program, VariableName);

            if (location == -1)
            {
                throw new Exception($"{VariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, value);
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
            float aspectRatio = (float)window.Size.X / window.Size.Y;
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, aspectRatio, 0.1f, 1000);
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