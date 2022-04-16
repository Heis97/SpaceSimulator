using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graphic;
using Objects;
using OpenGL;
using Model;
using Geometry;


namespace SpaceWorld
{
    public partial class Scene : Form
    {
        private GraphicGL GL1 = new GraphicGL();
        int[] obs = new int[500];
        
        public Scene()
        {
            InitializeComponent();
            glControl1.MouseWheel += GlControl1_MouseWheel;
            
        }

        private void GlControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            GL1.Form1_mousewheel(sender,e);
        }

        public void InitializeScene()
        {
            var p1 = new NativeObj();
            var cube = new Model3d(@"модели\cube30.STL");
            var sphere = new Model3d(@"модели\Шар.STL");
            //GL1.addSTL(cube.mesh, PrimitiveType.Triangles, new Point3d_GL(30,0,0), new Point3d_GL(0, 0, 30));
            //GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(100, 0, 0), new Point3d_GL(0, 100, 0), new Point3d_GL(0, 0, 100));
        
        }

        #region gl_control
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextCreated(sender, e);
            var send = (Control)sender;
            var w = send.Width;
            var h = send.Height;
            Console.WriteLine(w + " " + h);
            GL1.addMonitor(new Rectangle(0, 0, w, h), 0);
            InitializeScene();
            GL1.SortObj();
            //GL1.printDebug(richTextBox1);
            // pictureBox1.Image = GL1.bmp;
        }

        private void glControl1_Render(object sender, GlControlEventArgs e)
        {
            GL1.glControl_Render(sender, e);

        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            GL1.glControl_MouseDown(sender, e);
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {

            GL1.glControl_MouseMove(sender, e);

        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {

            var g = glControl1.CreateGraphics();
            Pen pen1 = new Pen(Color.Black);
            pen1.Width = 2;

            //g.Clear(Color.White);
            g.DrawRectangle(pen1, 0, 0, 200, 100);

            glControl1.Update();
        }
        private void glControl1_ContextDestroying(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextDestroying(sender, e);
        }
        #endregion
    }
}
