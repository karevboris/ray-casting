using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace dicom
{
    class GLgraphics
    {
        Vector3 cameraPosition = new Vector3(0, 0, -1100);
        Vector3 cameraDirecton = new Vector3(0, 0, 1);
        Vector3 cameraUp = new Vector3(1, 0, 0);
        Vector3 cameraRight = new Vector3(0, 1.2f, 0);

        public float time = 0;
        public float min_level = 0, max_level = 0;
        public const int x=920, y=840, z=561, size = 256*3;

        byte[] voxels;
        byte[] pallete;
        float[] palleteColor;

        int IlluminationProgramID;
        int IlluminationVertexShader;
        int IlluminationFragmentShader;
        int imageTextureID=1;
        int imageTextureID2=2;

        public string Setup(int width, int height)
        {
            GL.ClearColor(Color.DarkGray);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.DepthTest);
            Matrix4 perspectiveMat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, width / (float)height, 1, 1500);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspectiveMat);

            voxels = new byte[x * y * z];
            pallete = new byte[size];

            using (FileStream fs = new FileStream("out.raw", FileMode.Open, FileAccess.Read))
            {
                fs.Read(pallete, 0, size);
                fs.Read(voxels, 0, x * y * z);
            }

            return InitShaders();
        }

        public void Update()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 viewMat = Matrix4.LookAt(cameraPosition, cameraDirecton, cameraUp);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref viewMat);
            Render();
        }

        unsafe public void Render()
        {
            GL.UseProgram(IlluminationProgramID);
            GL.Enable(EnableCap.Texture3DExt);
            
            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture3D, texID);
            GL.TexImage3D(TextureTarget.Texture3D, 0,
            PixelInternalFormat.Alpha8, x, y, z, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.UnsignedByte, voxels);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "tex"), imageTextureID);

            int texID2 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture1D, texID2);
            GL.TexImage1D(TextureTarget.Texture1D, 0,
            PixelInternalFormat.Alpha8, size, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.UnsignedByte, pallete);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture1D);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "pallete"), imageTextureID2);
            
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camPos"), cameraPosition);
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camDir"), cameraDirecton);
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camUp"), cameraUp);
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camRight"), cameraRight);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "time"), time);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "min_level"), min_level);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "max_level"), max_level);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "width"), x);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "height"), y);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "deep"), z);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "size"), size);
            GL.Color3(Color.Blue);
            drawScreen();
        }

        private void drawScreen()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);        
            GL.TexCoord2(0.0, 0.0);
            GL.Vertex3(-y/2, -x/2, 0f);          
            GL.TexCoord2(0.0, 1.0);
            GL.Vertex3(-y/2, x/2, 0f);           
            GL.TexCoord2(1.0, 1.0);
            GL.Vertex3(y/2, x/2, 0f);           
            GL.TexCoord2(1.0, 0.0);
            GL.Vertex3(y/2, -x/2, 0f);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private string InitShaders()
        {
            IlluminationProgramID = GL.CreateProgram();
            loadShader("VertexShader.vs.txt", ShaderType.VertexShader, IlluminationProgramID, out IlluminationVertexShader);
            loadShader("FragmentShader1.fs.txt", ShaderType.FragmentShader, IlluminationProgramID, out IlluminationFragmentShader);
            GL.LinkProgram(IlluminationProgramID);
            Console.WriteLine(GL.GetProgramInfoLog(IlluminationProgramID));
            return GL.GetShaderInfoLog(IlluminationVertexShader) + GL.GetShaderInfoLog(IlluminationFragmentShader) + GL.GetProgramInfoLog(IlluminationProgramID);
        }
    }
}
