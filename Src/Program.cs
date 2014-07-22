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
        private static ShaderProgram program;
        private static bool left, right, up, down, space;
        private static System.Diagnostics.Stopwatch watch;
        private static VoxelChunk vk;
        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("kVoxGame");
            //Callbacks
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            //Mouse
            Glut.glutMouseFunc(OnMouse);
            Glut.glutMotionFunc(OnMove);
            
            //Keyboard
            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);

            //Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // create the shader program for "terrain"
            program = new ShaderProgram(vertexShaderSource, fragmentShaderSource);
            program["color"].SetValue(new Vector3(0, 1, 0));
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateRotation(new Vector3(0, 0, 0), 0.0f));

            //Setup camera
            camera = new Camera(new Vector3(0, 0, 10), Quaternion.Identity);
            camera.SetDirection(new Vector3(0, 0, -1));

            watch = System.Diagnostics.Stopwatch.StartNew();            
            vk = new VoxelChunk();
            Glut.glutMainLoop();
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
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();
            if (down) camera.MoveRelative(Vector3.UnitZ * deltaTime * 5);
            if (up) camera.MoveRelative(-Vector3.UnitZ * deltaTime * 5);
            if (left) camera.MoveRelative(-Vector3.UnitX * deltaTime * 5);
            if (right) camera.MoveRelative(Vector3.UnitX * deltaTime * 5);
            if (space) camera.MoveRelative(Vector3.Up * deltaTime * 3);
            float fps = 1 / deltaTime;
            Glut.glutSetWindowTitle(((int)fps).ToString());
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);
            program["view_matrix"].SetValue(camera.ViewMatrix);
            vk.RenderWithVAOSimple(program);
            Glut.glutSwapBuffers();
        }

        public static string fragmentShaderSource = @"" + File.ReadAllText(@"" + Directory.GetCurrentDirectory() + "/shaders/main.frag").ToString();
        public static string vertexShaderSource = @"" + File.ReadAllText(@"" + Directory.GetCurrentDirectory() + "/shaders/main.vert").ToString();
    }
}
