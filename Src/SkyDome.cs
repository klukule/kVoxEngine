using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace kVoxEngine
{
    class SkyDome
    {
        /// <summary>
        /// Builds a simple dome suitable for creating a smooth gradient to the horizon.
        /// </summary>
        /// <param name="program">The shader program that will be used with this dome.</param>
        /// <param name="segments">The number of segments in the dome.</param>
        /// <returns>A VAO that contains the vertices, uv co-ordinates and elements to draw the dome.</returns>
        public static VAO GradientDome(ShaderProgram program, int segments)
        {
            // allocate our vertex, uv and element arrays
            Vector3[] vertices = new Vector3[segments * (segments - 1) + 2];
            Vector2[] uvs = new Vector2[segments * (segments - 1) + 2];
            int[] elements = new int[2 * segments * (segments - 1) * 3];
            
            double deltaLatitude = Math.PI / segments;
            double deltaLongitude = Math.PI * 2.0 / segments;
            int index = 0;

            // create the rings of the dome using polar coordinates
            for (int i = 1; i < segments; i++)
            {
                double r0 = Math.Sin(i * deltaLatitude);
                double y0 = Math.Cos(i * deltaLatitude);

                for (int j = 0; j < segments; j++)
                {
                    double x0 = r0 * Math.Sin(j * deltaLongitude);
                    double z0 = r0 * Math.Cos(j * deltaLongitude);

                    vertices[index] = new Vector3(x0, y0, z0);
                    uvs[index++] = new Vector2(0, 1.0f - (float)y0);
                }
            }

            // create the top of the dome
            vertices[index] = new Vector3(0, 1, 0);
            uvs[index++] = new Vector2(0, 0);

            // create the bottom of the dome
            vertices[index] = new Vector3(0, -1, 0);
            uvs[index] = new Vector2(0, 2);

            // create the faces of the rings
            index = 0;
            for (int i = 0; i < segments - 2; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    elements[index++] = segments * i + j;
                    elements[index++] = segments * i + (j + 1) % segments;
                    elements[index++] = segments * (i + 1) + (j + 1) % segments;
                    elements[index++] = segments * i + j;
                    elements[index++] = segments * (i + 1) + (j + 1) % segments;
                    elements[index++] = segments * (i + 1) + j;
                }
            }

            // create the faces of the top of the dome
            for (int i = 0; i < segments; i++)
            {
                elements[index++] = segments * (segments - 1);
                elements[index++] = (i + 1) % segments;
                elements[index++] = i;
            }

            // create the faces of the bottom of the dome
            for (int i = 0; i < segments; i++)
            {
                elements[index++] = segments * (segments - 1) + 1;
                elements[index++] = segments * (segments - 2) + i;
                elements[index++] = segments * (segments - 2) + (i + 1) % segments;
            }

            Vector3[] normals = Geometry.CalculateNormals(vertices, elements);
            return new VAO(program, new VBO<Vector3>(vertices), new VBO<Vector3>(normals), new VBO<Vector2>(uvs), new VBO<int>(elements));
        }
    }
}
