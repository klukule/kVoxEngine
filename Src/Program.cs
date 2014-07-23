using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenGL;
using Tao.FreeGlut;
using SimplexNoise;

namespace kVoxEngine
{
    class Program
    {
        private static int width = 1280, height = 720;
        private static Camera camera;
        private static ShaderProgram terrainSP;
        private static ShaderProgram skyboxSP;
        private static bool left, right, up, down, space;
        private static System.Diagnostics.Stopwatch watch;
        private static System.Diagnostics.Stopwatch timefromstart;
        private static Frustum frustum;
        private static Matrix4 projectionMatrix;
        private static VoxelChunk[] chunks;
        private static Texture skydomeTexture;
        static void Main(string[] args)
        {
            timefromstart = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Initializing GLUT - {0}s", getTimeFromStart().ToString());
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("kVoxGame");
            Console.WriteLine("GLUT window created - {0}s", getTimeFromStart().ToString());

            //Callbacks
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            //Mouse
            Glut.glutMouseFunc(OnMouse);
            Glut.glutMotionFunc(OnMove);
            
            //Keyboard
            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Console.WriteLine("GLUT callbacks binded - {0}s", getTimeFromStart().ToString());

            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.Enable(EnableCap.Multisample);
            Gl.Enable(EnableCap.SampleAlphaToCoverage);
            Gl.Enable(EnableCap.FragmentLightingSgix);
            Gl.Enable(EnableCap.CullFace);
            

            Console.WriteLine("GL Enables enabled :D - {0}s", getTimeFromStart().ToString());

            skydomeTexture = new Texture("skydome.jpg");

            //Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            // create main shader program
            terrainSP = new ShaderProgram(terrainVS, terrainFS);
            terrainSP.Use();
            terrainSP["color"].SetValue(new Vector3(0, 0.8, 0));
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f);
            terrainSP["projection_matrix"].SetValue(projectionMatrix);
            terrainSP["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateRotation(new Vector3(0, 0, 0), 0.0f));
            Console.WriteLine("Shader program (main) compiled and data injected - {0}s", getTimeFromStart().ToString());

            // create main skybox program
            skyboxSP = new ShaderProgram(skyboxVS, skyboxFS);
            skyboxSP.Use();
            skyboxSP["color"].SetValue(new Vector3(0.2, 1.0, 1.0));
            skyboxSP["resolution"].SetValue(new Vector2(width, height));
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f);
            skyboxSP["projection_matrix"].SetValue(projectionMatrix);
            skyboxSP["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateRotation(new Vector3(0, 0, 0), 0.0f));
            Console.WriteLine("Shader program (skybox) compiled and data injected - {0}s", getTimeFromStart().ToString());






            //Setup camera
            camera = new Camera(new Vector3(0, 0, 10), Quaternion.Identity);
            camera.SetDirection(new Vector3(0, 0, -1));

            Console.WriteLine("Cam setup complete - {0}s", getTimeFromStart().ToString());


            watch = System.Diagnostics.Stopwatch.StartNew();
            chunks = new VoxelChunk[4];
            //instance few voxel chunks
            chunks[0] = new VoxelChunk(0,0);
            chunks[1] = new VoxelChunk(0, 32);
            chunks[2] = new VoxelChunk(32, 0);
            chunks[3] = new VoxelChunk(32, 32);
            Console.WriteLine("Chunks added to array - {0}s", getTimeFromStart().ToString());

            //init frustum
            frustum = new Frustum();
            frustum.UpdateFrustum(projectionMatrix, camera.ViewMatrix);
            Console.WriteLine("starting main GLUT loop - {0}s", getTimeFromStart().ToString());

            Glut.glutMainLoop();
        }
        private static float getTimeFromStart()
        {
            float timefromstartf = (float)timefromstart.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            return timefromstartf;
        }
        private static bool mouseDown = false;
        private static int downX, downY;
        private static int prevX, prevY;

        private static void OnMouse(int button, int state, int x, int y)
        {
            if (button != Glut.GLUT_RIGHT_BUTTON) return;

            // this method gets called whenever a new mouse button event happens
            mouseDown = (state == Glut.GLUT_DOWN);

            // if the mouse has just been clicked then we hide the cursor and store the position
            if (mouseDown)
            {
                Glut.glutSetCursor(Glut.GLUT_CURSOR_NONE);
                prevX = downX = x;
                prevY = downY = y;
            }
            else // unhide the cursor if the mouse has just been released
            {
                Glut.glutSetCursor(Glut.GLUT_CURSOR_LEFT_ARROW);
                Glut.glutWarpPointer(downX, downY);
                
            }
        }
        private static void OnMove(int x, int y)
        {
            // if the mouse move event is caused by glutWarpPointer then do nothing
            if (x == prevX && y == prevY) return;

            // move the camera when the mouse is down
            if (mouseDown)
            {
                float yaw = (prevX - x) * 0.002f;
                camera.Yaw(yaw);

                float pitch = (prevY - y) * 0.002f;
                camera.Pitch(pitch);

                prevX = x;
                prevY = y;
            }

            if (x < 0) Glut.glutWarpPointer(prevX = width, y);
            else if (x > width) Glut.glutWarpPointer(prevX = 0, y);

            if (y < 0) Glut.glutWarpPointer(x, prevY = height);
            else if (y > height) Glut.glutWarpPointer(x, prevY = 0);
        }
        private static void OnKeyboardDown(byte key, int x, int y)
        {
            if (key == 'w') up = true;
            else if (key == 's') down = true;
            else if (key == 'd') right = true;
            else if (key == 'a') left = true;
            else if (key == ' ') space = true;
            else if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyboardUp(byte key, int x, int y)
        {
            if (key == 'w') up = false;
            else if (key == 's') down = false;
            else if (key == 'd') right = false;
            else if (key == 'a') left = false;
            else if (key == ' ') space = false;
        }
        public static void OnDisplay()
        {

        }

        private static void OnRenderFrame()
        {
            int vertCount = 0;
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();
            terrainSP["view_matrix"].SetValue(camera.ViewMatrix);
            skyboxSP["view_matrix"].SetValue(camera.ViewMatrix);
            frustum.UpdateFrustum(projectionMatrix, camera.ViewMatrix);
            if (down) camera.MoveRelative(Vector3.UnitZ * deltaTime * 5);
            if (up) camera.MoveRelative(-Vector3.UnitZ * deltaTime * 5);
            if (left) camera.MoveRelative(-Vector3.UnitX * deltaTime * 5);
            if (right) camera.MoveRelative(Vector3.UnitX * deltaTime * 5);
            if (space) camera.MoveRelative(Vector3.Up * deltaTime * 3);
            float fps = 1 / deltaTime;
            
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Skydome
            Gl.Disable(EnableCap.DepthTest);
            Gl.UseProgram(skyboxSP.ProgramID);
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(skydomeTexture);
            SkyDome.GradientDome(skyboxSP, 12).Draw();
            Gl.Enable(EnableCap.DepthTest);
            Gl.UseProgram(terrainSP);

            foreach (var chunk in chunks)
            {
                if (frustum.Intersects(chunk.BoundingBox))
                {
                    chunk.RenderWithVAOSimple(terrainSP);
                    vertCount += chunk.vertexCount();
                }
            }
            Glut.glutSetWindowTitle("kVoxGame - " + ((int)fps).ToString() + " fps " + vertCount.ToString() + " Verticles");
            Glut.glutSwapBuffers();
        }
        //Terrain shader
        public static string terrainFS = @"" + File.ReadAllText(@"" + Directory.GetCurrentDirectory() + "/shaders/main.frag").ToString();
        public static string terrainVS = @"" + File.ReadAllText(@"" + Directory.GetCurrentDirectory() + "/shaders/main.vert").ToString();
        //Skydome shader
        public static string skyboxFS = @"" + File.ReadAllText(@"" + Directory.GetCurrentDirectory() + "/shaders/skybox.frag").ToString();
        public static string skyboxVS = @"" + File.ReadAllText(@"" + Directory.GetCurrentDirectory() + "/shaders/skybox.vert").ToString();
    }
}
