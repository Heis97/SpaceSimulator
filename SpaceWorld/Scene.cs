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
using System.Threading;

namespace SpaceWorld
{
    public partial class Scene : Form
    {
        private GraphicGL GL1 = new GraphicGL();
        static  int count = 100;
        static float PI = 3.1415926535f;
        List<ObjectMassGL> objs;
        double fps = 0;
        Model3d[] models;
        float w = 1900;
        System.Threading.Timer timer;
        public Scene()
        {
            InitializeComponent();
            glControl1.MouseWheel += GlControl1_MouseWheel;
            PreInitializeScene();
        }

        public void PreInitializeScene()
        {
            models = new Model3d[] 
            {
                new Model3d(@"модели\Шар1.STL") ,
                new Model3d(@"модели\izr1.STL"),
                new Model3d(@"модели\cube1.obj"),
                new Model3d(@"модели\cube_scene.stl"), };
            objs = new List<ObjectMassGL>();
            objs.Add(new ObjectMassGL(0 , //sun
                3.3E+5f, kmToAe(7e6f),2* kmToAe(7e6f), 
                new Vertex3f(0, 0, 0),
                new Vertex3f(0, 0, 0),
                new Vertex3f(0, 0, 0),
                new Vertex3f(0, 0, 0)));
            objs.Add(new ObjectMassGL(0, //earth
                0.995f, 1 * kmToAe(1.27e4f),1* kmToAe(1.27e4f),
                new Vertex3f(1.0f, 0, 0),
                new Vertex3f(0, 2E-7f, 0),
                new Vertex3f(0, 0, 0),
                new Vertex3f(0, 0, 0)));
            objs.Add(new ObjectMassGL(0 ,//moon
                kgToMe(7.3477e22f), kmToAe(7e3f),1* kmToAe(7e3f),
                new Vertex3f(1f + kmToAe(3.8e5f), 0, 0),
                new Vertex3f(0, 2E-7f + kmToAe(1f), 0),
                new Vertex3f(20, 20, 0),
                new Vertex3f(0, 0, 0)));

            objs.Add(new ObjectMassGL(1,//izr
                kgToMe(1e10f), kmToAe(1e-3f), 1600f* kmToAe(1e-3f) ,
                new Vertex3f(1.001f, 0, 0),
                new Vertex3f(0, 0, 0),
                new Vertex3f(-PI/2, 0, PI),
                new Vertex3f(0, 0, 0)));


            objs.Add(new ObjectMassGL(1,//izr
                kgToMe(1e10f), kmToAe(1e-3f), 1600f* kmToAe(1e-3f),
                new Vertex3f(1.001f, kmToAe(1), 0),
                new Vertex3f(0, 0, 0),
                new Vertex3f(-PI / 2, 0, PI),
                new Vertex3f(0, 0, 0)));
            Random random = new Random();
          /*  for (int i = 0; i < count/2; i++)
             {
                 var posx = 2e-5f* random.Next(-10000, 10000)+1f;
                var posy = 2e-5f * random.Next(-10000, 10000);
                var posz = 2e-5f * random.Next(-10000, 10000);

                var velx = 1e-9f * random.Next(-100, 100);
                var vely = 1e-9f * random.Next(-100, 100);
                var velz = 1e-9f * random.Next(-100, 100);

                 var mass =1e-5f * random.Next(1, 1000);
                objs.Add(new ObjectMassGL(2,mass, kmToAe(1e6f),1 * kmToAe(1e6f), new Vertex3f(posx, posy, posz), new Vertex3f(velx, vely, velz), new Vertex3f(90, 0, 0), new Vertex3f(0, 0, 0)));
            }

            for (int i = 0; i < count/2; i++)
            {
                var posx = 2e-5f * random.Next(-10000, 10000);
                var posy = 2e-5f * random.Next(-10000, 10000);
                var posz = 2e-5f * random.Next(-10000, 10000);

                var velx = 1e-9f * random.Next(-100, 100);
                var vely = 1e-9f * random.Next(-100, 100);
                var velz = 1e-9f * random.Next(-100, 100);

                var mass = 1e-5f * random.Next(1, 1000);
                objs.Add(new ObjectMassGL(0, mass, kmToAe(1e6f),2 * kmToAe(1e6f), new Vertex3f(posx, posy, posz), new Vertex3f(velx, vely, velz), new Vertex3f(0, 90, 0), new Vertex3f(0, 0, 0)));
            }*/
            GL1.dataComputeShader = objs.ToArray();

            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(-0.1, 0, 0), new Point3d_GL(0, -0.1, 0), new Point3d_GL(0, 0, -0.1));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(0.1, 0, 0), new Point3d_GL(0, 0.1, 0), new Point3d_GL(0, 0, 0.1));
        }

        #region gl_control
        void calcFps(object ob)
        {
            fps = 2*GL1.rendercout;
            GL1.rendercout = 0;
            //Console.WriteLine(fps);
        }
        private void glControl1_Render(object sender, GlControlEventArgs e)
        {
            GL1.glControl_Render(sender, e);
            label_fps.Text= ( fps).ToString();
        }


        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextCreated(sender, e);
            TimerCallback tm = new TimerCallback(calcFps);
            timer = new System.Threading.Timer(tm, 0, 0, 500);
            
            var send = (Control)sender;
            var w = send.Width;
            var h = send.Height;
            Console.WriteLine(w + " " + h);
            GL1.addMonitor(new Rectangle(0, 0, w, h), 0);
            GL1.loadObjs(objs.ToArray(), models);
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
