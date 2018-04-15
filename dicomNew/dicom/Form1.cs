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
using System.Drawing.Imaging;


namespace dicom
{
    public partial class Form1 : Form
    {
        public Form1() {
            OpenTK.Toolkit.Init();
            InitializeComponent();
        }
        
        GLgraphics glgraphics = new GLgraphics();

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glgraphics.Update();
            glControl1.SwapBuffers();
        }
        private void glControl1_Load(object sender, EventArgs e)
        {
            glgraphics.Setup(glControl1.Width, glControl1.Height);
            glgraphics.Update();
            glControl1.SwapBuffers();
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            glgraphics.Update();
            glControl1.SwapBuffers();
        }
    }
}
