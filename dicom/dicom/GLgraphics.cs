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
        Vector3 cameraPosition = new Vector3(2, 0, 0);
        Vector3 cameraDirecton = new Vector3(1, 0, 0);
        Vector3 cameraUp = new Vector3(0, 0, 1);
        Vector3 cameraRight = new Vector3(0, 1.2f, 0);

        //public float latitude = 47.98f;
        //public float longitude = 60.41f;
        //public float radius = 5.385f;

        public List<int> texturesIDs = new List<int>();

        public float time = 0;

        public string Setup(int width, int height)
        {
            GL.ClearColor(Color.DarkGray);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.DepthTest);
            Matrix4 perspectiveMat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, width / (float)height, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspectiveMat);

            return InitShaders();
        }

        public void Update(float[] den, int x, int y, int z)
        {
            //cameraPosition = new Vector3(
                        //(float)(radius * Math.Cos(Math.PI / 180.0f * latitude) * Math.Cos(Math.PI / 180.0f * longitude)),
                        //(float)(radius * Math.Cos(Math.PI / 180.0f * latitude) * Math.Sin(Math.PI / 180.0f * longitude)),
                        //(float)(radius * Math.Sin(Math.PI / 180.0f * latitude)));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 viewMat = Matrix4.LookAt(cameraPosition, cameraDirecton, cameraUp);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref viewMat);
            Render(den, y, x, z);
        }

        unsafe public void Render(float [] den, int x, int y, int z)
        {
            
            /*GL.ActiveTexture(TextureUnit.Texture0);
            uint texture;
            GL.Enable(EnableCap.Texture3DExt);
            GL.GenTextures(1, &texture);
            GL.BindTexture(TextureTarget.Texture3D, texture);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            GL.TexImage3D(TextureTarget.Texture3D, 0, OpenTK.Graphics.OpenGL.PixelInternalFormat.Alpha, x, y, z,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, den);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "tex"),0);*/

            /*float[] tmp = new float[x * y];
            for (int i = 0; i < x * y; i++)
                tmp[i] = 0.95f;

                    GL.ActiveTexture(TextureUnit.Texture0);
            uint texture;
            GL.Enable(EnableCap.Texture2D);
            GL.GenTextures(1, &texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            GL.TexImage2D(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelInternalFormat.Alpha, x, y,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, tmp);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "tex"), 0);*/
            
            /*GL.UseProgram(IlluminationProgramID);GL.Enable(EnableCap.Texture2D);
            float[] tmp = new float[x * y];
            for (int i = 0; i < x*y / 2; i++) { tmp[i] = 0.4f; tmp[x*y - i - 1] = 0.8f; }

            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texID);
            GL.TexImage2D(TextureTarget.Texture2D, 0,
            PixelInternalFormat.Alpha16, x, y, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, tmp);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "tex"), imageTextureID);*/

            GL.UseProgram(IlluminationProgramID); GL.Enable(EnableCap.Texture3DExt);
            /*float[] tmp = new float[x * y];
            for (int i = 0; i < y; i++)
                for (int j = 0; j < x; j++) tmp[i * x + j] = den[i, j, 0];*/

            int texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture3D, texID);
            GL.TexImage3D(TextureTarget.Texture3D, 0,
            PixelInternalFormat.Alpha16, x, y, z, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, den);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "tex"), imageTextureID);
            
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camPos"), cameraPosition);
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camDir"), cameraDirecton);
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camUp"), cameraUp);
            GL.Uniform3(GL.GetUniformLocation(IlluminationProgramID, "camRight"), cameraRight);
            GL.Uniform1(GL.GetUniformLocation(IlluminationProgramID, "time"), time);
            GL.Color3(Color.Blue);
            drawScreen();
            time += 0.01f;
            //GL.UseProgram(0);
        }

        private void drawScreen()
        {
            GL.Enable(EnableCap.Texture2D);
            //GL.BindTexture(TextureTarget.Texture2D, texturesIDs[0]);
            GL.Begin(PrimitiveType.Quads);        
            GL.TexCoord2(0.0, 0.0);
            GL.Vertex3(0f, -1.2f, -1.0f);          
            GL.TexCoord2(0.0, 1.0);
            GL.Vertex3(0f, -1.2f, 1.0f);           
            GL.TexCoord2(1.0, 1.0);
            GL.Vertex3(0f, 1.2f, 1.0f);           
            GL.TexCoord2(1.0, 0.0);
            GL.Vertex3(0f, 1.2f, -1.0f);
            GL.End();
            
            GL.Disable(EnableCap.Texture2D);
        }


        public int LoadTexture(String filePath)
        {
            try
            {
                Bitmap image = new Bitmap(filePath);

                float[] tmp = new float[image.Width* image.Height];
                for (int i = 0; i < image.Width * image.Height / 2; i++) { tmp[i] = 0.4f; tmp[image.Width * image.Height - i - 1] = 0.8f; }

                int texID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texID);
                BitmapData data = image.LockBits(
                new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Alpha16, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, tmp);
                image.UnlockBits(data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                return texID;
            }
            catch (System.IO.FileNotFoundException е)
            {
                return -1;
            }
        }

        private void drawTestQuad()
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.Blue);
            GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.Color3(Color.Red);
            GL.Vertex3(-1.0f, 1.0f, -1.0f);
            GL.Color3(Color.White);
            GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.Color3(Color.Green);
            GL.Vertex3(1.0f, -1.0f, -1.0f);
            GL.End();
        }

        private void DrawSphere(double r, int nx, int ny)
        {
            int ix, iy;
            double x, y, z;
            for (iy = 0; iy < ny; ++iy)
            {
                GL.Begin(PrimitiveType.QuadStrip);
                for (ix = 0; ix <= nx; ++ix)
                {
                    x = r * Math.Sin(iy * Math.PI / ny) * Math.Cos(2 * ix * Math.PI / nx);
                    y = r * Math.Sin(iy * Math.PI / ny) * Math.Sin(2 * ix * Math.PI / nx);
                    z = r * Math.Cos(iy * Math.PI / ny);
                    GL.Normal3(x, y, z);
                    GL.Vertex3(x, y, z);

                    x = r * Math.Sin((iy + 1) * Math.PI / ny) * Math.Cos(2 * ix * Math.PI / nx);
                    y = r * Math.Sin((iy + 1) * Math.PI / ny) * Math.Sin(2 * ix * Math.PI / nx);
                    z = r * Math.Cos((iy + 1) * Math.PI / ny);
                    GL.Normal3(x, y, z);
                    GL.Vertex3(x, y, z);
                }
                GL.End();
            }
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




        int IlluminationProgramID;
        int IlluminationVertexShader;
        int IlluminationFragmentShader;
        int uniformRenderTexture;
        int imageTextureID;

        private string InitShaders()
        {
            IlluminationProgramID = GL.CreateProgram();
            loadShader("SepiaShader.vs.txt", ShaderType.VertexShader, IlluminationProgramID, out IlluminationVertexShader);
            loadShader("SepiaShader1.fs.txt", ShaderType.FragmentShader, IlluminationProgramID, out IlluminationFragmentShader);
            GL.LinkProgram(IlluminationProgramID);
            Console.WriteLine(GL.GetProgramInfoLog(IlluminationProgramID));
            
            /*GL.Enable(EnableCap.Texture2D);
            imageTextureID = LoadTexture("logo.png");
            uniformRenderTexture = GL.GetUniformLocation(IlluminationProgramID, "tex");
            GL.Uniform1(uniformRenderTexture, imageTextureID);*/
            return GL.GetShaderInfoLog(IlluminationVertexShader) + GL.GetShaderInfoLog(IlluminationFragmentShader) + GL.GetProgramInfoLog(IlluminationProgramID);
        }


        //       //        //  2   //    //

        //string glVersion = GL.GetString(StringName.Version);
        //string glslVersion = GL.GetString(StringName.ShadingLanguageVersion);
        /*
        int BasicProgramID;
        int BasicVertexShader;
        int BasicFragmentShader;

        int vaoHandle;

        private void InitShaders2()
        {
            BasicProgramID = GL.CreateProgram();
            loadShader("illumination.vs.txt", ShaderType.VertexShader, BasicProgramID,
                       out BasicVertexShader);
            loadShader("illumination.fs.txt", ShaderType.FragmentShader, BasicProgramID,
                        out BasicFragmentShader);
            GL.LinkProgram(BasicProgramID);
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));

            float[] positionData = { -0.8f, -0.8f, 0.0f, 0.8f, -0.8f, 0.0f, 0.0f, 0.8f, 0.0f };
            float[] colorData = { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f };
            int[] vboHandlers = new int[2];
            GL.GenBuffers(2, vboHandlers);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer,
                  (IntPtr)(sizeof(float) * positionData.Length),
                  positionData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[1]);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          (IntPtr)(sizeof(float) * colorData.Length),
                          colorData, BufferUsageHint.StaticDraw);
            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[0]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandlers[1]);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        }*/


    }
}
