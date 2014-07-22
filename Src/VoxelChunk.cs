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
                        for(float test = -10; test <= h; h--)
                        AddCube(new Vector3(x, h, y), new Vector3(x + 1, h + 1, y + 1), vertices, elements);
                        
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
    }
}
