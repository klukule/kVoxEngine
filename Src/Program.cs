using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using Tao.FreeGlut;
using SimplexNoise;

namespace kVoxEngine
{
    class Program
    {
        private static List<VAO> terrain;
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

            //Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            // create the shader program
            program = new ShaderProgram(vertexShaderSource, fragmentShaderSource);

            // set the color to green
            program["color"].SetValue(new Vector3(0, 1, 0));

            camera = new Camera(new Vector3(0, 0, 10), Quaternion.Identity);
            camera.SetDirection(new Vector3(0, 0, -1));

            // set up some defaults for the shader program project and modelview matrices
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateRotation(new Vector3(0, 0, 0), 0.0f));

            //create terrain
            terrain = new List<VAO>();
            for (int x = -50; x < 50; x++)
            {
                for (int y = -50; y < 50; y++)
                {
                    float h = Noise.Generate((x + 50) / 32f, (y + 50) / 45f);
                    h = (float)Math.Round(h * 8);

                    terrain.Add(OpenGL.Geometry.CreateCube(program, new Vector3(x, h, y), new Vector3(x + 1, h + 1, y + 1)));
                }
            }

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

            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);
            program["view_matrix"].SetValue(camera.ViewMatrix);
            vk.RenderWithVAOSimple(program);
            
            Glut.glutSwapBuffers();
        }

        public static string fragmentShaderSource = @"
#version 130

varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;
void main(void)
{
  float diffuse_value = max(dot(vertex_normal, vertex_light_position), 0.0);
  gl_FragColor = vec4(vertex_color, 1.0) * max(0.7, diffuse_value);
}";

        public static string vertexShaderSource = @"
uniform mat4 projection_matrix;
uniform mat4 model_matrix;
uniform mat4 view_matrix;
uniform vec3 color;
 
attribute vec3 in_position;
attribute vec3 in_normal;
 
varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;
 
void main(void)
{
  vertex_normal = normalize((model_matrix * vec4(in_normal, 0)).xyz);
  vertex_light_position = normalize(vec3(0.5, 0.3, 0.2));
  vertex_color = color;
 
  gl_Position = projection_matrix * view_matrix * model_matrix * vec4(in_position, 1);
}";
    }
}
