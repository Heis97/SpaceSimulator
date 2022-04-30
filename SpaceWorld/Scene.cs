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
        static  int count = 20;
        int[] obs = new int[count];
        int obs_inst = 0;
        int obs_izr = 0;
        float[] _mass1;
        public Scene()
        {
            InitializeComponent();
            glControl1.MouseWheel += GlControl1_MouseWheel;
            PreInitializeScene();
        }

        public void PreInitializeScene()
        {
            var objs = new List<ObjectMassGL>();
            objs.Add(new ObjectMassGL(3.3E+5f, kmToAe(7e6f), new Vertex3f(0, 0, 0), new Vertex3f(0, 0, 0)));
            objs.Add(new ObjectMassGL(0.995f, kmToAe(1.27e4f), new Vertex3f(1.0f, 0, 0), new Vertex3f(0, 2E-7f, 0)));
            objs.Add(new ObjectMassGL(kgToMe(7.3477e22f), kmToAe(7e3f), new Vertex3f(1f + kmToAe(3.8e5f), 0, 0), new Vertex3f(0, 2E-7f + kmToAe(1f), 0)));
       

            Random random = new Random();
            for (int i = 0; i < obs.Length-3; i++)
             {
                 var posx = 2e-5f* random.Next(-10000, 10000);
                var posy = 2e-5f * random.Next(-10000, 10000);
                var posz = 2e-5f * random.Next(-10000, 10000);

                var velx = 1e-9f * random.Next(-100, 100);
                var vely = 1e-9f * random.Next(-100, 100);
                var velz = 1e-9f * random.Next(-100, 100);

                 var mass =1e-5f * random.Next(1, 1000);
                objs.Add(new ObjectMassGL(mass, kmToAe(1e2f), new Vertex3f(posx, posy, posz), new Vertex3f(velx, vely, velz)));
            }
            GL1.dataComputeShader = objs.ToArray();
            // 0 - pos xyz , mass
            // 1 - vel xyz , size
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(-1, 0, 0), new Point3d_GL(0, -1, 0), new Point3d_GL(0, 0, -1));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(1, 0, 0), new Point3d_GL(0, 1, 0), new Point3d_GL(0, 0, 1));
        }

        public void InitializeScene()
        {
            var cube = new Model3d(@"модели\cube1.obj");
            var izr = new Model3d(@"модели\izr1.STL");
            var sphere = new Model3d(@"модели\Шар1.STL");
            obs_inst = GL1.addSTL(cube.mesh, PrimitiveType.Triangles, new Point3d_GL(0, 0, 0), new Point3d_GL(0, 0, 0),0.001f, count);
            obs_izr = GL1.addSTL(izr.mesh, PrimitiveType.Triangles, new Point3d_GL(1.001f, 0, 0), new Point3d_GL(90, 260, 0), kmToAe(1e-3f), 1);
        }

        #region gl_control
        private void glControl1_Render(object sender, GlControlEventArgs e)
        {
            GL1.glControl_Render(sender, e);
            if(GL1.dataComputeShader!=null)
            {
                if (GL1.dataComputeShader.Length>= obs.Length)
                {
                    for (int i = 0; i < obs.Length; i++)
                    {
                        GL1.buffersGl.setTransfObj(obs_inst, i, new Point3d_GL(GL1.dataComputeShader[i].pos), new Point3d_GL(20, 20, 0));
                        GL1.buffersGl.setScale(obs_inst, i, GL1.dataComputeShader[i].size);
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

        float kmToAe(float km)
        {
            return km / 1.5e8f;
        }

        float kgToMe(float kg)
        {
            return kg / 5.9726e24f;
        }
    }
}
