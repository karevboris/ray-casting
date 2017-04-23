using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvilDicom.Components;
using EvilDicom.Image;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace dicom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        GLgraphics glgraphics = new GLgraphics();
        public float[,,] density;
        public float[] d;
        public Data arr; 
        int load = 0;
        int load1 = 0;
        int row, col, dep;
        unsafe private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect=true;
            int count = 0;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                count=dialog.FileNames.Length;
                var imgFile = new ImageMatrix(dialog.FileNames[0]);
                row = imgFile.Properties.Rows;
                col = imgFile.Properties.Columns;
                dep = count;
                density = new float [row,col,dep];
                d = new float[row * col * dep];
                for (int z = 0; z < count; z++)
                {
                    imgFile = new ImageMatrix(dialog.FileNames[z]);
                    float max = imgFile.Image.Max();
                    float min = imgFile.Image.Min();
                    for (int i = 0; i < imgFile.Properties.Rows; i++)
                        for (int j = 0; j < imgFile.Properties.Columns; j++)
                        {
                            density[i, j, z] = imgFile.Image[i * imgFile.Properties.Rows + j]/10000;
                            //density[i, j, z] = imgFile.Image[i * imgFile.Properties.Rows + j] - min;
                            //density[i, j, z] /= (max - min) / 255f;
                            d[(imgFile.Properties.Rows-1-i) * imgFile.Properties.Rows + j + z * imgFile.Properties.Rows * imgFile.Properties.Columns] = density[i, j, z];
                        }
                }
           
                /*GL.TexImage3D(TextureTarget.Texture3D, 0, 
                    PixelInternalFormat.Rgba, imgFile.Properties.Rows, imgFile.Properties.Columns, count, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.Float, density);*/
              }
            load = 1;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {

            glgraphics.Update(d, row, col, dep);
            //glgraphics.Update(arr.data, arr.H, arr.W, arr.D);
        }
        private void glControl1_Load(object sender, EventArgs e)
        {
            //Application.Idle += Application_Idle;
            glgraphics.Setup(glControl1.Width, glControl1.Height);
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            /*float widthCoef = (e.X - glControl1.Width * 0.5f) / (float)glControl1.Width;
            float heightCoef = (-e.Y + glControl1.Height * 0.5f) / (float)glControl1.Height;
            glgraphics.latitude = heightCoef * 180;
            glgraphics.longitude = widthCoef * 360;*/
            if (load == 1)
            {
                glgraphics.Update(d, row, col, dep);
                //glgraphics.Update(arr.data, arr.H, arr.W, arr.D);
                glControl1.SwapBuffers();
            }
        }
        /*void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle) glControl1.Refresh();
        }*/

        private void glControl1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            /*if (load == 1)
            {
                if (e.KeyChar == (char)Keys.W)
                {
                    glgraphics.radius -= 1;
                    glgraphics.Update(density, glControl1.Width, glControl1.Height);
                    glControl1.SwapBuffers();
                }
                if (e.KeyChar == (char)Keys.S)
                {
                    glgraphics.radius += 1;
                    glgraphics.Update(density, glControl1.Width, glControl1.Height);
                    glControl1.SwapBuffers();
                }
                if (e.KeyChar == (char)Keys.A)
                {
                    glgraphics.latitude += 1;
                    glgraphics.Update(density, glControl1.Width, glControl1.Height);
                    glControl1.SwapBuffers();
                }
                if (e.KeyChar == (char)Keys.D)
                {
                    glgraphics.latitude -= 1;
                    glgraphics.Update(density, glControl1.Width, glControl1.Height);
                    glControl1.SwapBuffers();
                }
                if (e.KeyChar == (char)Keys.Q)
                {
                    glgraphics.longitude -= 1;
                    glgraphics.Update(density, glControl1.Width, glControl1.Height);
                    glControl1.SwapBuffers();
                }
                if (e.KeyChar == (char)Keys.E)
                {
                    glgraphics.longitude += 1;
                    glgraphics.Update(density, glControl1.Width, glControl1.Height);
                    glControl1.SwapBuffers();
                }
            }*/
        }

        private void открытьBinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                short[] tmp=new short[0];
                arr=new Data(tmp,0,0,0,0,0,0);
                arr.readBinFile(dialog.FileName);
                Console.WriteLine(arr.H);
                Console.WriteLine(arr.sX);
                Console.WriteLine(arr.W);
                Console.WriteLine(arr.D);
                load = 1;
            }
        }

    }
}
