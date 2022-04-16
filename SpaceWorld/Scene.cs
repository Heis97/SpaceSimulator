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
        int[] obs = new int[3000];
        float[] _mass1;
        public Scene()
        {
            InitializeComponent();
            glControl1.MouseWheel += GlControl1_MouseWheel;
            PreInitializeScene();
        }

        public void PreInitializeScene()
        {
            Random random = new Random();
            /*var pos3 = new float[] { 0.05f, 0, 0, -3.5f, 3.05f, 0 };
            var vel3 = new float[] { 0, 2E-5f, 0, 0, -0.1E-2f, 0 };
            var mass1 = new float[] { 5E14f, 5E10f };*/

            var pos3 = new float[obs.Length * 3];
            var vel3 = new float[obs.Length * 3];
            var mass1 = new float[obs.Length ];
            for (int i = 0; i < obs.Length; i++)
            {
                var pos = 0.01 * random.Next(-10000, 10000);
                pos3[3 * i] = (float) pos;

                pos = 0.01 * random.Next(-10000, 10000);
                pos3[3 * i+1] = (float)pos;

                pos = 0.01 * random.Next(-10000, 10000);
                pos3[3 * i+2] = (float)pos;



                var vel = 0.0001 * random.Next(-100, 100);
                vel3[3 * i] = (float)vel;

                vel = 0.0001 * random.Next(-100, 100);
                vel3[3 * i+1] = (float)vel;

                vel = 0.0001 * random.Next(-100, 100);
                vel3[3 * i+2] = (float)vel;

                var mass =2E11 * random.Next(10, 1000);
                mass1[i] = (float)mass;

            }
            _mass1 = mass1;
            List<float[]> gravData = new List<float[]>();

            gravData.Add(pos3);
            gravData.Add(vel3);
            gravData.Add(mass1);
            Console.WriteLine(gravData.Count);
            GL1.dataComputeShader = gravData;
        }

        public void InitializeScene()
        {
            Random random = new Random();
            var p1 = new NativeObj();
            var cube = new Model3d(@"модели\cube30.STL");
            var sphere = new Model3d(@"модели\Шар.STL");
            for(int i=0; i<obs.Length;i++)
            {
                obs[i] = GL1.addSTL(cube.mesh, PrimitiveType.Triangles, new Point3d_GL(0, 0, 0), new Point3d_GL(0, 0, 0), 1E-15*_mass1[i]);

            }
            
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(100, 0, 0), new Point3d_GL(0, 100, 0), new Point3d_GL(0, 0, 100));

        }

        #region gl_control
        private void glControl1_Render(object sender, GlControlEventArgs e)
        {
            GL1.glControl_Render(sender, e);
            if(GL1.resultComputeShader!=null)
            {
                if (GL1.resultComputeShader.Length> obs.Length*3-1)
                {
                    for (int i = 0; i < obs.Length; i++)
                    {
                        GL1.buffersGl.setTransfObj(obs[i], new Point3d_GL(GL1.resultComputeShader[3*i], GL1.resultComputeShader[3*i+1], GL1.resultComputeShader[3*i+2]), new Point3d_GL(0, 0, 0));
                    }
                }
            }
            

        }
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

        
        private void GlControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            GL1.Form1_mousewheel(sender, e);
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
