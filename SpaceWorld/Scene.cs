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
        static  int count = 300 ;
        int[] obs = new int[count];
        int obs_inst = 0;
        float[] _mass1;
        public Scene()
        {
            InitializeComponent();
            glControl1.MouseWheel += GlControl1_MouseWheel;
            PreInitializeScene();
        }

        public void PreInitializeScene()
        {
            
            var pos3a = new float[] { 0 ,0, 0f, 1.0f, 0, 0 };
            var vel3a = new float[] { 0, 0, 0, 0, 2E-7f, 0 };
            var mass1a = new float[] { 3.3E+5f, 0.995f };


             var pos3 = new float[obs.Length * 3];
             var vel3 = new float[obs.Length * 3];
             var mass1 = new float[obs.Length ];
            Random random = new Random();
            for (int i = 0; i < obs.Length; i++)
             {
                 var pos = 2e-4* random.Next(-10000, 10000);
                 pos3[3 * i] = (float) pos;

                 pos = 2e-4 * random.Next(-10000, 10000);
                 pos3[3 * i+1] = (float)pos;

                 pos = 2e-4 * random.Next(-10000, 10000);
                 pos3[3 * i+2] = (float)pos;



                 var vel = 1e-9 * random.Next(-100, 100);
                 vel3[3 * i] = (float)vel;

                 vel = 1e-9 * random.Next(-100, 100);
                 vel3[3 * i+1] = (float)vel;

                 vel = 1e-9 * random.Next(-100, 100);
                 vel3[3 * i+2] = (float)vel;

                 var mass =1e-5 * random.Next(1, 1000);
                 mass1[i] = (float)mass;

             }

           /*for(int i=0; i<2;i++)
            {
                pos3[3*i] = pos3a[3 * i];
                pos3[3 * i +1] = pos3a[3 * i +1];
                pos3[3 * i +2] = pos3a[3 * i +2];

                vel3[3 * i] = vel3a[3 * i];
                vel3[3 * i + 1] = vel3a[3 * i + 1];
                vel3[3 * i + 2] = vel3a[3 * i + 2];

                mass1[i] = mass1a[i];
            }*/
            List<float[]> gravData = new List<float[]>();

            gravData.Add(pos3);
            gravData.Add(vel3);
            gravData.Add(mass1);
            Console.WriteLine(gravData.Count);
            GL1.dataComputeShader = gravData;
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(-1, 0, 0), new Point3d_GL(0, -1, 0), new Point3d_GL(0, 0, -1));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(1, 0, 0), new Point3d_GL(0, 1, 0), new Point3d_GL(0, 0, 1));
        }

        public void InitializeScene()
        {
            Random random = new Random();
            var p1 = new NativeObj();
            var cube = new Model3d(@"модели\cube30.STL");
            //var izr = new Model3d(@"модели\izr1.STL");
            var sphere = new Model3d(@"модели\Шар.STL");
            sphere= sphere.InvertNormals();
            obs_inst = GL1.addSTL(sphere.mesh, PrimitiveType.Triangles, new Point3d_GL(0, 0, 0), new Point3d_GL(0, 0, 0),0.001f, count);
            for (int i = 0; i < obs.Length; i++)
            {
               float scale = 0.0001f;
              /* if (i == 0 || i == 1)
                {
                    scale = 0.001f;
                }*/
                GL1.buffersGl.setScale(obs_inst,i, scale);

            }
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
                        GL1.buffersGl.setTransfObj(obs_inst, i, new Point3d_GL(GL1.resultComputeShader[3 * i], GL1.resultComputeShader[3 * i + 1], GL1.resultComputeShader[3 * i + 2]), new Point3d_GL(0, 0, 0));
                    }
                }
            }
            //GL1.printDebug(richTextBox1);
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
