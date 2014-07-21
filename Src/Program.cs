using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using Tao.FreeGlut;

namespace kVoxEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(1280, 768);
            Glut.glutCreateWindow("kVoxGame");

            Glut.glutMainLoop();
        }
    }
}
