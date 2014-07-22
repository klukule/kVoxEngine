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
    public class VoxelChunk
    {
        /// <summary>
        /// A voxel chunk contains a 32x32x32 set of voxels, where
        /// a voxel is 'empty' when == 0, and contains some color when != 0
        /// </summary>
        private byte[] voxelData;

        private int offsetX;
        private int offsetZ;

        /// <summary>
        /// The translation of this VoxelChunk to bring it into world co-ordinates.
        /// </summary>
        public Matrix4 ModelMatrix { get; set; }

        public AxisAlignedBoundingBox BoundingBox { get; private set; }

        public VoxelChunk(int oX,int oZ)
        {
            offsetX = oX;
            offsetZ = oZ;
            voxelData = new byte[32 * 32 * 32];

            // for now, lets just fill this with voxels
            for (int i = 0; i < voxelData.Length; i++) voxelData[i] = 1;

            this.ModelMatrix = Matrix4.Identity;

            this.BoundingBox = new AxisAlignedBoundingBox(new Vector3(0 + offsetX, 0, 0 + offsetZ), new Vector3(32 + offsetX, 0, 32 + offsetZ));
        }
        private VAO chunkVAO;
        private void AddCube(Vector3 min, Vector3 max, List<Vector3> vertices, List<int> elements)
        {
            int offset = vertices.Count;

            vertices.AddRange(new Vector3[] {
        new Vector3(min.x, min.y, max.z),
        new Vector3(max.x, min.y, max.z),
        new Vector3(min.x, max.y, max.z),
        new Vector3(max.x, max.y, max.z),
        new Vector3(max.x, min.y, min.z),
        new Vector3(max.x, max.y, min.z),
        new Vector3(min.x, max.y, min.z),
        new Vector3(min.x, min.y, min.z)
    });

            elements.AddRange(new int[] { 
        0 + offset, 1 + offset, 2 + offset, 1 + offset, 3 + offset, 2 + offset,
        1 + offset, 4 + offset, 3 + offset, 4 + offset, 5 + offset, 3 + offset,
        4 + offset, 7 + offset, 5 + offset, 7 + offset, 6 + offset, 5 + offset,
        7 + offset, 0 + offset, 6 + offset, 0 + offset, 2 + offset, 6 + offset,
        7 + offset, 4 + offset, 0 + offset, 4 + offset, 1 + offset, 0 + offset,
        2 + offset, 3 + offset, 6 + offset, 3 + offset, 5 + offset, 6 + offset
    });
        }
        /*private void AddFace(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> elements)
        {
            int offset = vertices.Count;

            vertices.AddRange(new Vector3[] {
        new Vector3(v1.x, v1.y, v1.z),
        new Vector3(v2.x, v2.y, v2.z),
        new Vector3(v4.x, v4.y, v4.z),
        new Vector3(v3.x, v3.y, v3.z)
        
    });

            elements.AddRange(new int[] { 
        0 + offset, 1 + offset, 2 + offset, 3 + offset,
    });
        }*/

        public void RenderWithVAOSimple(ShaderProgram program)
        {
            if (chunkVAO == null)
            {
                List<Vector3> vertices = new List<Vector3>();
                List<int> elements = new List<int>();
                for (int x = 0 + offsetX; x < 32 + offsetX; x++)
                {
                    for (int y = 0 + offsetZ; y < 32 + offsetZ; y++)
                    {
                        float h = Noise.Generate((x + 32) / 32f, (y + 32) / 45f);
                        h = (float)Math.Round(h * 8);
                        if(h < 0)
                        {
                            h = -h;
                        }
                        //for(float test = -10; test <= h; h--)
                        AddCube(new Vector3(x, 0, y), new Vector3(x + 1, h + 1, y + 1), vertices, elements);
                        
                    }
                }

                Vector3[] vertex = vertices.ToArray();
                int[] element = elements.ToArray();
                Vector3[] normals = OpenGL.Geometry.CalculateNormals(vertex, element);

                chunkVAO = new VAO(program, new VBO<Vector3>(vertex), new VBO<Vector3>(normals), new VBO<int>(element, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticRead));
            }

            program["model_matrix"].SetValue(ModelMatrix);
            
            chunkVAO.Draw();
        }
        /*public void RenderWithVAOGreedy(ShaderProgram program, bool draw = true)
        {
            // This greedy algorithm is converted from PHP to C# from this article:
            // http://0fps.wordpress.com/2012/06/30/meshing-in-a-minecraft-game/
            //
            // The original source code can be found here:
            // https://github.com/mikolalysenko/mikolalysenko.github.com/blob/gh-pages/MinecraftMeshes/js/greedy.js
            if (chunkVAO == null)
            {
                List<Vector3> vertices = new List<Vector3>();
                List<int> elements = new List<int>();

                for (int d = 0; d < 3; d++)
                {
                    int i, j, k, l, w, h, u = (d + 1) % 3, v = (d + 2) % 3;
                    int[] x = new int[3];
                    int[] q = new int[3];
                    bool[] mask = new bool[32 * 32];

                    q[d] = 1;

                    for (x[d] = -1; x[d] < 32; )
                    {
                        // Compute the mask
                        int n = 0;
                        for (x[v] = 0; x[v] < 32; ++x[v])
                        {
                            for (x[u] = 0; x[u] < 32; ++x[u])
                            {
                                mask[n++] = (0 <= x[d] ? data(x[0], x[1], x[2]) : false) !=
                                    (x[d] < 32 - 1 ? data(x[0] + q[0], x[1] + q[1], x[2] + q[2]) : false);
                            }
                        }

                        // Increment x[d]
                        ++x[d];

                        // Generate mesh for mask using lexicographic ordering
                        n = 0;
                        for (j = 0; j < 32; ++j)
                        {
                            for (i = 0; i < 32; )
                            {
                                if (mask[n])
                                {
                                    // Compute width
                                    for (w = 1; i + w < 32 && mask[n + w]; ++w) ;

                                    // Compute height (this is slightly awkward
                                    var done = false;
                                    for (h = 1; j + h < 32; ++h)
                                    {
                                        for (k = 0; k < w; ++k)
                                        {
                                            if (!mask[n + k + h * 32])
                                            {
                                                done = true;
                                                break;
                                            }
                                        }
                                        if (done) break;
                                    }

                                    // Add quad
                                    x[u] = i; x[v] = j;
                                    int[] du = new int[3];
                                    int[] dv = new int[3];
                                    du[u] = w;
                                    dv[v] = h;

                                    AddFace(new Vector3(x[0], x[1], x[2]),
                                            new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]),
                                            new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                            new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]), vertices, elements);

                                    // Zero-out mask
                                    for (l = 0; l < h; ++l)
                                    {
                                        for (k = 0; k < w; ++k)
                                        {
                                            mask[n + k + l * 32] = false;
                                        }
                                    }

                                    // Increment counters and continue
                                    i += w; n += w;
                                }
                                else
                                {
                                    ++i; ++n;
                                }
                            }
                        }
                    }
                }

                Vector3[] vertex = vertices.ToArray();
                int[] element = elements.ToArray();
                Vector3[] normals = OpenGL.Geometry.CalculateNormals(vertex, element);

                chunkVAO = new VAO(program, new VBO<Vector3>(vertex), new VBO<Vector3>(normals), new VBO<int>(element, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticRead));
            }

            if (draw)
            {
                program["model_matrix"].SetValue(ModelMatrix);

                chunkVAO.Draw();
            }
        }

        private bool data(int x, int y, int z)
        {
            return voxelData[x + 32 * y + 32 * 32 * z] != 0;
        }*/
    }
}
