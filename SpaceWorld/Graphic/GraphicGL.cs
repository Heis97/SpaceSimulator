using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;
using Geometry;

namespace Graphic
{   
    public enum modeGL { Paint, View}
    public enum viewType { Perspective, Ortho }
    public class TextureGL
    {
        public uint id;
        public int binding;
        public int w, h,ch;
        public PixelFormat pixelFormat;
        InternalFormat internalFormat;
        public float[] data;
        public TextureGL(int _binding, int _w, int _h = 1, PixelFormat _pixelFormat = PixelFormat.Red, float[] _data = null)
        {
            Console.WriteLine("genTexture");    
            data = (float[])_data.Clone();
            var buff = genTexture(_binding, _w, _h, _pixelFormat,data);
            id = buff;
            binding = _binding;
            w = _w;
            h = _h;
            pixelFormat = _pixelFormat;
            Console.WriteLine(w + " " + h + " " + ch+" "+ pixelFormat);
        }
        public float[] getData()
        {
            //Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, id);
            float[] dataf = new float[w * h ];
            Gl.GetTexImage(TextureTarget.Texture2d, 0, pixelFormat, PixelType.Float, dataf);
            //Console.WriteLine(w+" "+h+" "+ch+" "+ dataf.Length);
            return dataf;
        }
        public void setData(float[] data)
        {
            //Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, id);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, w, h, 0,pixelFormat, PixelType.Float, data);
        }
        uint genTexture(int binding, int w, int h = 1, PixelFormat pixelFormat = PixelFormat.Red, float[] data = null)
        {
            if (pixelFormat == PixelFormat.Red)
            {
                ch = 1;
            }
            else if (pixelFormat == PixelFormat.Rg)
            {
                ch = 2;
            }
            else if (pixelFormat == PixelFormat.Rgb)
            {
                ch = 3;
            }
            else if (pixelFormat == PixelFormat.Rgba)
            {
                ch = 4;
            }
            else
            {
                ch = 1;
            }
            var buff_texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
            internalFormat = InternalFormat.R32f;
            if(pixelFormat == PixelFormat.Rgb)
            {
                internalFormat = InternalFormat.Rgba32f;
            }
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, w/ch, h, 0, pixelFormat, PixelType.Float, data);
            Gl.BindImageTexture((uint)binding, buff_texture, 0, false, 0, BufferAccess.ReadWrite, internalFormat);
            return buff_texture;
        }

        private void useTexture()
        {
            Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, id);
        }
    }
    public  class IDs
    {
        public uint buff_tex;
        public uint buff_tex1;
        public uint programID_ps;
        public uint programID_trs;
        public uint programID_lns;
        public uint programID_comp;
        public uint vert;
        public int[] LocationMVPs = new int[4];
        public int[] LocationMs = new int[4];
        public int[] LocationVs = new int[4];
        public int[] LocationPs = new int[4];
        public int LightID;
        public int textureVisID;
        public int LightPowerID;
        public int MaterialDiffuseID;
        public int MaterialAmbientID;
        public int currentMonitor = 1;
        public int MaterialSpecularID;
        public int TextureID;
        public int MouseLocID;
        public int MouseLocGLID;
        public int translMeshID;
    }
    public class GraphicGL
    {
        #region vars
        static float PI = 3.1415926535f;
        public int startGen = 0;
        public int saveImagesLen = 0;
        public int renderdelim = 1500;
        public int rendercout = 0;
        public viewType typeProj = viewType.Perspective;
        Size sizeControl;
        Point lastPos;

        int currentMonitor = 1;

        public int textureVis = 0;
        float LightPower = 500000.0f;
        Label Label_cor;
        Label Label_cor_cur;
        Label Label_trz_cur;
        RichTextBox debug_box;
        Matrix4x4f Pm;
        Matrix4x4f Vm;
        public BuffersGl buffersGl = new BuffersGl();
        Matrix4x4f Mm;
        Matrix4x4f MVP;
        public Matrix4x4f[] MVPs;
        public Matrix4x4f[] Ms;
        public Matrix4x4f[] Vs;
        public Matrix4x4f[] Ps;
        public Vertex2f MouseLoc;
        public Vertex2f MouseLocGL;
        Vertex3f translMesh = new Vertex3f(0.0f, 0.0f, 0.0f);
        Vertex3f lightPos = new Vertex3f(0.0f, 0.0f, 50.0f);
        Vertex3f MaterialDiffuse = new Vertex3f(0.5f, 0.5f, 0.5f);
        Vertex3f MaterialAmbient = new Vertex3f(0.2f, 0.2f, 0.2f);
        Vertex3f MaterialSpecular = new Vertex3f(0.1f, 0.1f, 0.1f);
        
        public modeGL modeGL = modeGL.View;
        List<Point3d_GL> pointsPaint = new List<Point3d_GL>();
        Point3d_GL curPointPaint = new Point3d_GL(0, 0, 0);
        public List<TransRotZoom> transRotZooms = new List<TransRotZoom>();
        
        public List<TransRotZoom[]> trzForSave;
        public int[] monitorsForGenerate;
        public string pathForSave;
        public ImageBox[] imageBoxesForSave;
        public Size size = new Size(1,1);
        Mat pict = new Mat();
        byte[] textureB;
        Size textureSize;
        public Bitmap bmp;
        IDs gluints = new IDs();

        TextureGL posData, velData, massData;
        public List<float[]> dataComputeShader = new List<float[]>();
        bool initComputeShader = false;
        public float[] resultComputeShader;

        #endregion 

        public void glControl_Render(object sender, GlControlEventArgs e)
        {
            MVPs = new Matrix4x4f[4];
            Ms = new Matrix4x4f[4];
            Vs = new Matrix4x4f[4];
            Ps = new Matrix4x4f[4];
            var txt = "";
            for (int i = 0; i < transRotZooms.Count; i++)
            {
                Gl.ViewportIndexed((uint)i,
                    transRotZooms[i].rect.X,
                    transRotZooms[i].rect.Y,
                    transRotZooms[i].rect.Width,
                    transRotZooms[i].rect.Height);
                var retM = compMVPmatrix(transRotZooms[i]);               
                MVPs[i] = retM[3];
                Ms[i] = retM[2];
                Vs[i] = retM[1];
                Ps[i] = retM[0];

                txt += "TRZ " + i + ": "+transRotZooms[i].getInfo(transRotZooms.ToArray()).ToString()+"\n";
            }
            if(Label_trz_cur!=null)
            {
                Label_trz_cur.Text = txt;
            }
           
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (buffersGl.objs_static!=null)
            {
                if (buffersGl.objs_static.Count!=0)
                {
                    foreach(var opglObj in buffersGl.objs_static)
                    {
                        renderGlobj(opglObj);   
                    }
                }
            }

            if (buffersGl.objs_dynamic != null)
            {
                if (buffersGl.objs_dynamic.Count != 0)
                {
                    foreach (var opglObj in buffersGl.objs_dynamic)
                    {
                        renderGlobj(opglObj);
                    }
                }
            }

            rendercout++;
            if(rendercout%renderdelim==0)
            {
                rendercout = 0;
            }
            gpuCompute();
        }

        

        void bufferToCompute(float[] data, int locat)
        {
            var dat_buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, dat_buff);
            Gl.BufferData(BufferTarget.ShaderStorageBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
            Gl.BindBufferBase(BufferTarget.ShaderStorageBuffer, (uint)locat, dat_buff);
            // Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }
        void renderGlobj(openGlobj opgl_obj)
        {

            if(opgl_obj.visible)
            {
                try
                {
                    uint prog = 0;
                    if (opgl_obj.tp == PrimitiveType.Points)
                    {
                        prog = gluints.programID_ps;
                    }
                    else if (opgl_obj.tp == PrimitiveType.Triangles)
                    {
                        prog = gluints.programID_trs;
                    }
                    else if (opgl_obj.tp == PrimitiveType.Lines)
                    {
                        prog = gluints.programID_lns;
                    }
                    load_vars_gl(prog,opgl_obj);
                    use_buff_gl(opgl_obj);
                    Gl.DrawArrays(opgl_obj.tp, 0, opgl_obj.vert_len);
                    disableVert(opgl_obj);
                }
                catch
                {
                }
            }
            
        }

        public void glControl_ContextDestroying(object sender, GlControlEventArgs e)
        {
            foreach (var opglObj in buffersGl.objs_static)
            {
                Gl.DeleteBuffers(opglObj.buff_color);
                Gl.DeleteBuffers(opglObj.buff_pos);
                Gl.DeleteBuffers(opglObj.buff_normal);
                Gl.DeleteBuffers(opglObj.buff_UV);
            }
            foreach (var opglObj in buffersGl.objs_dynamic)
            {
                Gl.DeleteBuffers(opglObj.buff_color);
                Gl.DeleteBuffers(opglObj.buff_pos);
                Gl.DeleteBuffers(opglObj.buff_normal);
                Gl.DeleteBuffers(opglObj.buff_UV);
            }
        }

        public void glControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            sizeControl = ((Control)sender).Size;
            Gl.Initialize();
            Gl.Enable(EnableCap.Multisample);
            Gl.ClearColor(0.9f, 0.9f, 0.95f, 0.0f);
            Gl.PointSize(2f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var ComputeSourceGL = assembCode(new string[] { @"Graphic\Shaders\Comp\CompSh_N2_gravitation.glsl" });
            var VertexSourceGL = assembCode(new string[] { @"Graphic\Shaders\Vert\VertexSh.txt" });
            var FragmentSourceGL = assembCode(new string[] { @"Graphic\Shaders\Frag\FragmSh.txt" });
            var GeometryShaderPointsGL = assembCode(new string[] { @"Graphic\Shaders\Geom_R\GeomShP_head.txt", @"Graphic\Shaders\Geom_R\GeomSh_body.txt" });
            var GeometryShaderLinesGL = assembCode(new string[] { @"Graphic\Shaders\Geom_R\GeomShL_head.txt", @"Graphic\Shaders\Geom_R\GeomSh_body.txt" });
            var GeometryShaderTrianglesGL = assembCode(new string[] { @"Graphic\Shaders\Geom_R\GeomShT_head.txt", @"Graphic\Shaders\Geom_R\GeomSh_body.txt" });


            gluints.programID_lns = createShader(VertexSourceGL, GeometryShaderLinesGL, FragmentSourceGL);
            gluints.programID_ps = createShader(VertexSourceGL, GeometryShaderPointsGL, FragmentSourceGL);
            gluints.programID_trs = createShader(VertexSourceGL, GeometryShaderTrianglesGL , FragmentSourceGL);
            gluints.programID_comp = createShaderCompute(ComputeSourceGL);
            init_vars_gl(gluints.programID_lns);
            init_vars_gl(gluints.programID_ps);
            init_vars_gl(gluints.programID_trs);

            initComputeShader = init_textures(dataComputeShader);

            // Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.DepthTest);
            //pict = new Mat("cube2.png");
            //pict = new Mat("Model.png");
            //CvInvoke.Resize(pict, pict, new Size(3800,3800));

            // pict.Save("Model_small.png");
            // textureB = textureLoad(pict);
            //bmp = byteToBitmap(textureB, textureSize);

            //bindTexture(textureB);
        }

    

        #region addBuffer
        private void load_buffs_gl(List<openGlobj> opgl_obj)
        {
            for(int i = 0; i < opgl_obj.Count; i++)
            {
                opgl_obj[i] = load_buff_gl(opgl_obj[i]);
            }
        }

        private openGlobj load_buff_gl(openGlobj opgl_obj)
        {
            var buff_pos = bindBuffer(opgl_obj.vertex_buffer_data);
            var buff_normal = bindBuffer(opgl_obj.normal_buffer_data);
            var buff_color = bindBuffer(opgl_obj.color_buffer_data);
            var buff_UV = bindBuffer(opgl_obj.texture_buffer_data);
            return opgl_obj.setBuffers(buff_pos, buff_normal, buff_color, buff_UV);           
        }
        private void use_buff_gl(openGlobj opgl_obj)
        {
            useBuffer(opgl_obj.buff_pos,0, 3);
            useBuffer(opgl_obj.buff_normal, 1, 3);
            useBuffer(opgl_obj.buff_color, 2, 3);
            useBuffer(opgl_obj.buff_UV, 3, 2);
        }

        private void disableVert(openGlobj openGlobj)
        {
            Gl.DisableVertexAttribArray(openGlobj.buff_pos);
            Gl.DisableVertexAttribArray(openGlobj.buff_normal);
            Gl.DisableVertexAttribArray(openGlobj.buff_color);
            Gl.DisableVertexAttribArray(openGlobj.buff_UV);
        }
        private uint bindBuffer(float[] data)
        {
            var buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
            return buff;
        }
        private void useBuffer(uint buff, uint lvl, int strip)
        {
            Gl.EnableVertexAttribArray(lvl);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.VertexAttribPointer(lvl, strip, VertexAttribType.Float, false, 0, IntPtr.Zero);
        }

        public void SortObj()
        {
            buffersGl.sortObj();
            if (buffersGl.objs_static != null)
            {
                if (buffersGl.objs_static.Count != 0)
                {
                    load_buffs_gl(buffersGl.objs_static);
                }
            }
        }
        #endregion

        private bool init_textures(List<float[]> data)
        {
           // Console.WriteLine(data.Count);
            if (data.Count<3)
            {
                return false;
            }
            float[] pos3 = data[0];
            float[] vel3 = data[1];
            float[] mass1 = data[2];
            posData = new TextureGL(0, pos3.Length, 1, PixelFormat.Rgb, pos3);
            velData = new TextureGL(2, vel3.Length, 1, PixelFormat.Rgb, vel3);
            massData = new TextureGL(3, mass1.Length, 1, PixelFormat.Red, mass1);
            Console.WriteLine(posData.w + " " + posData.h + " " + posData.ch + " " + posData.data.Length);
            return true;
        }
        private void init_vars_gl(uint prog)
        {
            Gl.UseProgram(prog);

            for (int i = 0; i < 4; i++)
            {
                gluints.LocationMVPs[i] = Gl.GetUniformLocation(prog, "MVPs[" + i + "]");
                gluints.LocationMs[i] = Gl.GetUniformLocation(prog, "Ms[" + i + "]");
                gluints.LocationVs[i] = Gl.GetUniformLocation(prog, "Vs[" + i + "]");
                gluints.LocationPs[i] = Gl.GetUniformLocation(prog, "Ps[" + i + "]");
            }
            gluints.TextureID  = Gl.GetUniformLocation(prog, "textureSample");
            gluints.MaterialDiffuseID = Gl.GetUniformLocation(prog, "MaterialDiffuse");
            gluints.MaterialAmbientID = Gl.GetUniformLocation(prog, "MaterialAmbient");
            gluints.MaterialSpecularID = Gl.GetUniformLocation(prog, "MaterialSpecular");
            gluints.LightID = Gl.GetUniformLocation(prog, "LightPosition_world");
            gluints.LightPowerID = Gl.GetUniformLocation(prog, "lightPower");
            gluints.textureVisID = Gl.GetUniformLocation(prog, "textureVis");
            gluints.MouseLocID = Gl.GetUniformLocation(prog, "MouseLoc");
            gluints.MouseLocGLID = Gl.GetUniformLocation(prog, "MouseLocGL");

        }
        private void load_vars_gl(uint prog, openGlobj openGlobj)
        {

            Gl.UseProgram(prog);

            var ModelMatr =new Matrix4x4f(( Transmatr((float)openGlobj.transl.x, (float)openGlobj.transl.y, (float)openGlobj.transl.z)
                * RotXmatr(openGlobj.rotate.x) * RotYmatr(openGlobj.rotate.y) * RotZmatr(openGlobj.rotate.z)* Scalematr(openGlobj.scale)).data);
           // Console.WriteLine(ModelMatr);

            for (int i = 0; i < 4; i++)
            {

                Gl.UniformMatrix4f(gluints.LocationMVPs[i], 1, false, MVPs[i]);
                Gl.UniformMatrix4f(gluints.LocationMs[i], 1, false, ModelMatr);
                Gl.UniformMatrix4f(gluints.LocationVs[i], 1, false, Vs[i]);
                Gl.UniformMatrix4f(gluints.LocationPs[i], 1, false, Ps[i]);
            }

            
           // Gl.UniformMatrix4f(LocationMVP, 1, false, MVP);
           // Gl.UniformMatrix4f(LocationM, 1, false, Mm);
           // Gl.UniformMatrix4f(LocationV, 1, false, Vm);

            Gl.Uniform3f(gluints.MaterialDiffuseID, 1, MaterialDiffuse);
            Gl.Uniform3f(gluints.MaterialAmbientID, 1, MaterialAmbient);
            Gl.Uniform3f(gluints.MaterialSpecularID, 1, MaterialSpecular);
            Gl.Uniform3f(gluints.LightID, 1, lightPos);
            Gl.Uniform1f(gluints.LightPowerID, 1, LightPower);
            Gl.Uniform2f(gluints.MouseLocID, 1, MouseLoc);
            Gl.Uniform2f(gluints.MouseLocGLID, 1, MouseLocGL);
        
        }



        #region texture
        void gpuCompute()
        {
            if (initComputeShader)
            {
                Gl.UseProgram(gluints.programID_comp);
                //use texture
                //Console.WriteLine(massData.data.Length);
                Gl.DispatchCompute((uint)massData.data.Length, 1, 1);
                Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
                var result = posData.getData();
               // var resultVel = velData.getData();
                resultComputeShader = result;
                //Console.WriteLine(toStringBuf(result, 3, "pos "));
                // Console.WriteLine(toStringBuf(resultVel, 3, "vel "));
            }
        }
        byte[] textureLoad(Mat mat)
        {

            var bytearr = (byte[,,])mat.GetData();
            var bytetext = new byte[bytearr.GetLength(0) * bytearr.GetLength(1) * bytearr.GetLength(2)];
            Console.WriteLine(bytearr.GetLength(0));
            Console.WriteLine(bytearr.GetLength(1));
            Console.WriteLine(bytearr.GetLength(0) * bytearr.GetLength(1) * 3);
            Console.WriteLine("___");
            int ind = 0;

            for (int i = 0; i < bytearr.GetLength(0); i++)
            {
                for (int j = 0; j < bytearr.GetLength(1); j++)
                {
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 0]; ind++;
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 1]; ind++;
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 2]; ind++;
                }
            }
            Console.WriteLine(ind);
            textureSize = new Size(mat.Width, mat.Height);
            return bytetext;
        }
        Bitmap byteToBitmap(byte[] arr, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            for (int i = 0; i < size.Width; i++)
            {
                for (int j = 0; j < size.Height; j++)
                {
                    // Console.WriteLine(3 * (j * size.Width + j));
                    var color = Color.FromArgb(
                        arr[3 * (j * size.Width + i)],
                        arr[3 * (j * size.Width + i) + 1],
                        arr[3 * (j * size.Width + i) + 2]
                        );
                    bmp.SetPixel(i, j, color);
                }
            }
            return bmp;
        }
        private uint bindTexture(byte[] arrB)
        {
            var buff_texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);
            // Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.REPEAT);

            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, textureSize.Width, textureSize.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, arrB);
            Gl.GenerateMipmap(TextureTarget.Texture2d);
            return buff_texture;
        }

        #endregion

        #region util

        public void SaveToFolder(string folder,int id)
        {
            var bitmap = matFromMonitor(id);
            var invVm = Vs[id].Inverse;
            var trz_in = transRotZooms[selectTRZ_id(id)];
            var trz = trz_in.getInfo(transRotZooms.ToArray());
            //Gl.depth
            var path_gl = Path.Combine(folder, "monitor_" + id.ToString());
            Directory.CreateDirectory(path_gl);
            bitmap.Save(path_gl + "/" + trz.ToString() + ".png");
        }

        public Bitmap bitmapFromMonitor(int id)
        {
            var recTRZ = (transRotZooms[selectTRZ_id(id)]).rect;
            var lockMode = System.Drawing.Imaging.ImageLockMode.WriteOnly;
            var format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            var bitmap = new Bitmap(recTRZ.Width, recTRZ.Height, format);

            var bitmapRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(bitmapRectangle, lockMode, format);
            Gl.ReadPixels(recTRZ.X, recTRZ.Y, recTRZ.Width, recTRZ.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            bitmap.UnlockBits(bmpData);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
           
            return bitmap;

        }

        public Mat matFromMonitor(int id)
        {
            //Console.WriteLine("selectTRZ_id(id)" + selectTRZ_id(id));
            var selecTrz = selectTRZ_id(id);
            if(selecTrz<0)
            {
                return null;
            }
            var trz = transRotZooms[selectTRZ_id(id)];
            var recTRZ = trz.rect;
            var data = new Mat(recTRZ.Width, recTRZ.Height, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            //Console.WriteLine("recTRZ.Width " + recTRZ.X + " " + recTRZ.Y + " " + recTRZ.Width + " " + recTRZ.Height);
           // Console.WriteLine(data.DataPointer);
           // Console.WriteLine(trz);
            Gl.ReadPixels(recTRZ.X, recTRZ.Y, recTRZ.Width, recTRZ.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.DataPointer);
            //CvInvoke.Rotate(data, data, Emgu.CV.CvEnum.RotateFlags.Rotate180);
            //CvInvoke.Flip(data, data, Emgu.CV.CvEnum.FlipType.Vertical);
            return data;
        }
        
        Geometry.PointF toGLcord(Geometry.PointF pf)
        {
            var sizeView = transRotZooms[0].rect;

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = -((sizeView.Width / 2) * pf.Y) + sizeView.Height / 2;
            return new Geometry.PointF(x, y);
        }
        Geometry.PointF toTRZcord(Geometry.PointF pf)
        {
            var sizeView = transRotZooms[0].rect;

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = (sizeView.Width / 2) * pf.Y + sizeView.Height / 2;
            return new Geometry.PointF(x, y);
        }
        Geometry.PointF toTRZcord_photo(Geometry.PointF pf,Size size_trz)
        {
            var sizeView = new Rectangle(new Point(0, 0), size_trz);

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = (sizeView.Width / 2) * pf.Y + sizeView.Height / 2;
            return new Geometry.PointF(x, y);
        }
        public void swapTRZ(int ind1, int ind2)
        {
            var lamb1 = transRotZooms[ind1].rect;
            var lamb2 = transRotZooms[ind2].rect;
            var trz1 = transRotZooms[ind1];
            var trz2 = transRotZooms[ind2];
            trz1.rect = lamb2;
            trz2.rect = lamb1;
            transRotZooms[ind1] = trz1;
            transRotZooms[ind2] = trz2;
        }
        int selectTRZ_id(int id)
        {
            int ind = 0;
            foreach (var trz in transRotZooms)
            {
                if (trz.id == id)
                {
                    return ind;
                }
                ind++;
            }
            return -1;
        }
         public Matrix4x4f[] compMVP_photo(string data)
        {
            var trz = new TransRotZoom(data);
            var zoom = trz.zoom;
            var off_x = trz.off_x;
            var off_y = trz.off_y;
            var off_z = trz.off_z;
            var xRot = trz.xRot;
            var yRot = trz.yRot;
            var zRot = trz.zRot;
            
            var _Pm = ProjmatrF(53f);
            var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
            var Pm_ = new Matrix4x4f((_Pm).data);
            var Vm_ = new Matrix4x4f((_Vm).data);

            var Mm_ = Matrix4x4f.Identity;
            var MVP_ = new Matrix4x4f((_Pm * _Vm).data);

            return new Matrix4x4f[] { Pm_, Vm_, Mm_, MVP_ };
        }

        public Matrix4x4f[] compMVPmatrix(TransRotZoom trz_in)
        {
            var trz = trz_in.getInfo(transRotZooms.ToArray());
            
            var zoom = trz.zoom;
            var off_x = trz.off_x;
            var off_y = trz.off_y;
            var off_z = trz.off_z;
            var xRot = trz.xRot;
            var yRot = trz.yRot;
            var zRot = trz.zRot;
            if (trz.viewType_ == viewType.Perspective)
            {
                var _Pm = ProjmatrF(53f,(float)trz.rect.Width/ trz.rect.Height);              
                var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
                Pm = new Matrix4x4f((_Pm).data);
                Vm = new Matrix4x4f((_Vm).data);
                Mm = Matrix4x4f.Identity;
                MVP = new Matrix4x4f((_Pm * _Vm).data);
            }
            else if (trz.viewType_ == viewType.Ortho)
            {
                var _Pm = OrthoF();
                var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
                Pm = new Matrix4x4f((_Pm).data);
                Vm = new Matrix4x4f((_Vm).data);
                Mm = Matrix4x4f.Identity;
                MVP = new Matrix4x4f((_Pm * _Vm).data);
            }
            

            return new Matrix4x4f[] { Pm, Vm, Mm, MVP };
        }
        static public float toRad(float degrees)
        {
            return degrees * PI / 180;
        }
        static public float cos(double alpha)
        {
            return (float)Math.Cos(toRad((float)alpha));
        }
        static public float sin(double alpha)
        {
            return (float)Math.Sin(toRad((float)alpha));
        }
        static public Matr4x4f Transmatr(float x = 0, float y = 0, float z = 0)
        {
            var data = new float[] {
                 1, 0, 0, x ,
                 0, 1, 0, y,
                 0, 0, 1, z ,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f ProjmatrF(float fx = 1, float aspec = 1, float n =0.1f ,float f = 300000f)
        {
            var fy = fx / aspec;
            var a = (f + n) / (f - n);
            var b = (-2*f * n) / (f - n);
            var cx = 1 / (float)Math.Tan(toRad(fx) / 2);
            var cy = 1 / (float)Math.Tan(toRad(fy) / 2);
            var data = new float[] {
                 cx, 0, 0, 0 ,
                 0, cy, 0, 0 ,
                 0, 0, a,  b,
                 0, 0, 1, 0  };
            return new Matr4x4f(data);
        }

        static public Matr4x4f OrthoF(float fx = 1, float aspec = 1, float n = 0.1f, float f = 30000f)
        {
            var fy = fx / aspec;
            var a = (f + n) / (f - n);
            var b = (-2 * f * n) / (f - n);
            var cx = 1 / (float)Math.Tan(toRad(fx) / 2);
            var cy = 1 / (float)Math.Tan(toRad(fy) / 2);
            var data = new float[] {
                 1, 0, 0, 0 ,
                 0, 1, 0, 0 ,
                 0, 0, 1,  0,
                 0, 0, 0, 1 };
            return new Matr4x4f(data);
        }

        static public Matr4x4f Projmatr(float f = 1)
        {

            var data = new float[] {
                 f, 0, 0, 0 ,
                 0, f, 0, 0 ,
                 0, 0, 1.001f,  0.04f,
                 0, 0, 100, 0  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f RotZmatr(double alpha)
        {
            var data =  new float[] {
                 cos(alpha), -sin(alpha), 0,0 ,
                 sin(alpha), cos(alpha), 0, 0 ,
                 0, 0, 1, 0 ,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f RotYmatr(double alpha)
        {
            var data = new float[] {
                 cos(alpha),0, sin(alpha), 0,
                 0,1,0 , 0,
                 -sin(alpha), 0, cos(alpha), 0 ,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f RotXmatr(double alpha)
        {
            var data = new float[] {
                1,0,0,0,
                0, cos(alpha), -sin(alpha), 0,
                0, sin(alpha), cos(alpha), 0, 
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f Scalematr(double scale)
        {
            float scalef = (float)scale; 
            var data = new float[] {
                scalef,0,0,0,
                0, scalef, 0, 0,
                0, 0, scalef, 0,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }

        /// <summary>
        /// 3dGL->2dIm
        /// </summary>
        /// <param name="point"></param>
        /// <param name="mvp"></param>
        /// <returns></returns>
        public Geometry.PointF calcPixel(Vertex4f point, int id)
        {
            var p2 =new  Matr4x4f( MVPs[id]) * new Vert4f(point);
            p2.Norm();
            var p3 = toTRZcord(new Geometry.PointF(p2.data[0], p2.data[1]));
            
            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

         public Geometry.PointF calcPixel_photo(Vertex4f point, string data,Size size_trz)
        {
            var p2 = new Matr4x4f(compMVP_photo(data)[3]) * new Vert4f(point);
            p2.Norm();
            var p3 = toTRZcord_photo(new Geometry.PointF(p2.data[0], p2.data[1]), size_trz);

            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        public Geometry.PointF calcPixelInv(Vertex4f point, Matrix4x4f mvp)
        {
            var p2 = mvp.Inverse * point;
            var p4 = p2 / p2.w;
            var p3 = toGLcord(new Geometry.PointF(p4.x, p4.y));
            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        public void printDebug(RichTextBox box)
        {
            string txt = "";
            txt += "\n______DYNAMIC_________\n";
            foreach (var ob in buffersGl.objs_dynamic)
            {
                txt += toStringBuf(ob.vertex_buffer_data, 3, "vert");
                txt += toStringBuf(ob.color_buffer_data, 3, "color");
                txt += toStringBuf(ob.normal_buffer_data, 3, "normal");
                txt += toStringBuf(ob.texture_buffer_data, 2, "textUV");
                txt += "\n________________________\n";
            }
            txt += "\n______STATIC_________\n";
            foreach (var ob in buffersGl.objs_static)
            {
                txt += toStringBuf(ob.vertex_buffer_data, 3, "vert");
                txt += toStringBuf(ob.color_buffer_data, 3, "color");
                txt += toStringBuf(ob.normal_buffer_data, 3, "normal");
                txt += toStringBuf(ob.texture_buffer_data, 2, "textUV");
                txt += "\n________________________\n";
            }
            box.Text = txt;
        }

        string toStringBuf(float[] buff, int strip,string name)
        {
            if (buff == null)
                return name + " null ";
            string txt = name +" "+buff.Length;
            for (int i = 0; i < buff.Length / strip; i++)
            {
                txt += "         | ";
                for(int j=0; j<strip; j++)
                {
                    txt += buff[i * strip+j].ToString() + ", ";
                }
            }
            txt += " |\n---------------------\n";
            return txt;
       
        }
        #endregion

       
        #region mouse
        public void add_Label(Label label_list, Label label_cur, Label label_trz)
        {
            Label_trz_cur = label_trz;
            Label_cor = label_list;
            Label_cor_cur = label_cur;
            if (Label_cor == null || Label_cor_cur==null || Label_trz_cur==null) 
            {
                Console.WriteLine("null_start");
            }
            
        }
        public void add_TextBox(RichTextBox richTextBox)
        {
            debug_box = richTextBox;
        }
        public void addMonitor(Rectangle rect,int id)
        {
            transRotZooms.Add(new TransRotZoom(rect,id));
        }
        public void addMonitor(Rectangle rect, int id, Vertex3d rotVer, Vertex3d transVer, int _idMast)
        {
            transRotZooms.Add(new TransRotZoom(rect, id, rotVer, transVer, _idMast));
        }
        int selectTRZ(MouseEventArgs e)
        {
            int ind = 0;
            foreach(var trz in transRotZooms)
            {
                var recGL = new Rectangle(trz.rect.X, sizeControl.Height - trz.rect.Y - trz.rect.Height, trz.rect.Width, sizeControl.Height - trz.rect.Y);
                if(recGL.Contains(e.Location))
                {
                    return ind;
                }
                ind++;
            }
            return -1;
        }


        public void glControl_MouseDown(object sender, MouseEventArgs e)
        {           
            switch(modeGL)
            {
                case modeGL.View:
                    lastPos = e.Location;
                    break;
                case modeGL.Paint:
                    if (e.Button == MouseButtons.Left)
                    {
                        pointsPaint.Add(curPointPaint);
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        pointsPaint.Clear();
                    }
                    break;
            }
        }
        public void glControl_MouseMove(object sender, MouseEventArgs e)
        {

            var cont = (Control)sender;
            MouseLocGL = new Vertex2f((float)e.X / (0.5f * (float)cont.Width) - 1f, -((float)e.Y / (0.5f * (float)cont.Height) - 1f));
            int sel_trz = selectTRZ(e);
            if(sel_trz < 0)
            {
                return;
            }
            var trz = transRotZooms[sel_trz];
            if(Label_cor_cur!=null)
            {
                Label_cor_cur.Text = e.X + " " + e.Y;
            }
            
            int w = trz.rect.Width;
            int h = trz.rect.Height;
            switch (modeGL)
            {
                case modeGL.View:
                    
                    var dx = e.X - lastPos.X;
                    var dy = e.Y - lastPos.Y;
                    double dyx = lastPos.Y - w / 2;
                    double dxy = lastPos.X - h / 2;
                    var delim = (Math.Sqrt(dy * dy + dx * dx) * Math.Sqrt(dxy * dxy + dyx * dyx));
                    double dz = 0;
                    if (delim != 0)
                    {
                        dz = (dy * dxy + dx * dyx) / delim;

                    }
                    else
                    {
                        dz = 0;
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        trz.xRot += dy;
                        trz.yRot -= dx;
                        trz.zRot += dz;
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        trz.off_x += Convert.ToDouble(dx);
                        trz.off_y += Convert.ToDouble(dy);
                    }
                    lastPos = e.Location;
                    break;
                case modeGL.Paint:
                    var p_XY = new Point3d_GL((double)e.Location.X/ (0.5*(double)w), (double)e.Location.Y/(0.5* (double)h), 0);
                   // var p_YZ = new Point3d_GL(0,(double)e.Location.X / (0.5 * (double)w), (double)e.Location.Y / (0.5 * (double)h));
                   // var p_ZX = new Point3d_GL((double)e.Location.X / (0.5 * (double)w),0, (double)e.Location.Y / (0.5 * (double)h));
                    try
                    {
                        var invM = Pm.Inverse;
                        //var invM = MVPs[0].Inverse;
                        if (Label_cor != null)
                        {
                            curPointPaint = invM * p_XY;
                            Label_cor.Text = curPointPaint.ToString() + "\n" + "\n";//;// + (invM * p_YZ).ToString() + "\n" + (invM * p_ZX).ToString();
                            if(pointsPaint.Count!=0)
                            {
                                foreach(var p in pointsPaint)
                                {
                                    Label_cor.Text += p.ToString() + "\n";
                                }
                            }

                            if (pointsPaint.Count > 1)
                            {
                                var dis = (pointsPaint[pointsPaint.Count - 1] - pointsPaint[pointsPaint.Count - 2]).magnitude();
                                Label_cor.Text +="dist = "+ Math.Round(dis,4).ToString() + "\n";
                            }
                        }
                    }
                    catch
                    {

                    }
                    
                    break;
            }
            transRotZooms[sel_trz] = trz;
        }
        public void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("P m = " + Pm);
            //Console.WriteLine("V m = " + Vm);
            // var invVm = Vm.Inverse;
            //Console.WriteLine("invV m = " + invVm);
            int sel_trz = selectTRZ(e);
            if (sel_trz < 0)
            {
                return;
            }
            var trz = transRotZooms[sel_trz];
            var angle = e.Delta;
            if (angle > 0)
            {
                if (trz.zoom < 0.0002)
                {
                }
                else
                {
                    trz.zoom = 0.7 * trz.zoom;
                    trz.zoom = Math.Round(trz.zoom, 4);
                }
            }
            else
            {
                trz.zoom = 1.3 * trz.zoom;
                trz.zoom = Math.Round(trz.zoom, 4);
            }
            transRotZooms[sel_trz] = trz;
        }


        public void lightPowerScroll(int value)
        {
            var f = (float)value*100;
            LightPower = f;
        }
        public void diffuseScroll(int value)
        {
            var f = (float)value / 10;
            MaterialDiffuse.x = f;
            MaterialDiffuse.y = f;
            MaterialDiffuse.z = f;
        }
        public void ambientScroll(int value)
        {
            var f = (float)value / 10;
            MaterialAmbient.x = f;
            MaterialAmbient.y = f;
            MaterialAmbient.z = f;
        }
        public void specularScroll(int value)
        {

            var f = (float)value / 10;
            MaterialSpecular.x = f;
            MaterialSpecular.y = f;
            MaterialSpecular.z = f;
        }
        
        public void lightXscroll(int value)
        {
            lightPos.x = (float)value * 10;
        }
        public void lightYscroll(int value)
        {
            lightPos.y = (float)value * 10;

        }
        public void lightZscroll(int value)
        {
            lightPos.z = (float)value * 10;
        }
        public void orientXscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setxRot(value);
            transRotZooms[currentMonitor] = trz;
        }
        public void orientYscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setyRot(value);
            transRotZooms[currentMonitor] = trz;
        }
        public void orientZscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setzRot(value);
            transRotZooms[currentMonitor] = trz;
        }

        public void planeXY()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(0, 0, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void planeYZ()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(0, 90, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void planeZX()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(90, 0, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void changeViewType(int ind)
        {
            if (ind >= 0 && ind<transRotZooms.Count)
            {
                var trz = transRotZooms[ind];
                if(trz.viewType_==viewType.Ortho)
                {
                    trz.viewType_ = viewType.Perspective;
                    transRotZooms[ind] = trz;
                }
                else if (trz.viewType_ == viewType.Perspective)
                {
                    trz.viewType_ = viewType.Ortho;
                    transRotZooms[ind] = trz;
                }

            }
        }

        public void changeVisible(int ind)
        {
            if (ind >= 0 && ind < transRotZooms.Count)
            {
                var trz = transRotZooms[ind];
                if (trz.visible == true)
                {
                    trz.visible = false;
                    transRotZooms[ind] = trz;
                }
                else if (trz.visible == false)
                {
                    trz.visible = true;
                    transRotZooms[ind] = trz;
                }

            }
        }
        public void setMode(modeGL mode)
        {
            modeGL = mode;
        }
        #endregion
        #region mesh
 
        
        float[] toFloat(Point3d_GL[] points)
        {
            var fl = new float[points.Length * 3];
            for(int i=0; i< points.Length; i++)
            {
                fl[3 * i] = (float)points[i].x;
                fl[3 * i+1] = (float)points[i].y;
                fl[3 * i+2] = (float)points[i].z;
            }
            return fl;
        }
        float[] toFloat(Vertex4f[] points)
        {
            var fl = new float[points.Length * 3];
            for (int i = 0; i < points.Length; i++)
            {
                fl[3 * i] = points[i].x;
                fl[3 * i + 1] = points[i].y;
                fl[3 * i + 2] = points[i].z;
            }
            return fl;
        }

        void add_buff_gl_obj(float[] data_v, float[] data_t, float[] data_n, PrimitiveType tp)
        {
            buffersGl.add_obj(new openGlobj(data_v, null, data_n, data_t, tp));
        }
        public void add_buff_gl(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp)
        {            
            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n,null,  tp));
        }
        public int addSTL(float[] data_v, PrimitiveType tp, Point3d_GL trans, Point3d_GL rotate, double scale = 1)
        {
            var data_n = computeNormals(data_v);
            var glObj = new openGlobj(data_v, null, data_n, null, tp,1);
            glObj.scale = scale;
            glObj.transl = trans;
            glObj.rotate = rotate;       
            return buffersGl.add_obj(load_buff_gl(glObj));
        }
       void add_buff_gl_id(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp,int id)
        {
            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n, null,tp,id));
        }

        void remove_buff_gl_id(int id)
        {
            buffersGl.removeObj(id);
        }
        public void addFrame(Point3d_GL pos, Point3d_GL x, Point3d_GL y, Point3d_GL z)
        {
            addLineMesh(new Point3d_GL[] { pos, x }, 1.0f, 0, 0);
            addLineMesh(new Point3d_GL[] { pos, y }, 0, 1.0f, 0);
            addLineMesh(new Point3d_GL[] { pos, z }, 0, 0, 1.0f);
        }

      
        public void addGLMesh(float[] _mesh, PrimitiveType primitiveType, float x = 0, float y = 0, float z = 0, float r = 0.1f, float g = 0.1f, float b = 0.1f, float scale = 1f)
        {
            // addMesh(cube_buf, PrimitiveType.Points);
            if (x == 0 && y == 0 && z == 0)
            {
                addMesh(_mesh, primitiveType, r, g, b);
            }
            else
            {
                addMesh(translateMesh(scaleMesh(_mesh, scale), x, y, z), primitiveType, r, g, b);
            }

        }
        public float[] translateMesh(float[] _mesh, float x=0, float y=0, float z=0)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] = _mesh[i] + x;
                mesh[i + 1] = _mesh[i + 1] + y;
                mesh[i + 2] = _mesh[i + 2] + z;
            }
            return mesh;
        }
        public float[] scaleMesh(float[] _mesh, float k, float kx = 1.0f, float ky = 1.0f, float kz = 1.0f)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] = _mesh[i] * k * kx;
                mesh[i + 1] = _mesh[i + 1] * k * ky;
                mesh[i + 2] = _mesh[i + 2] * k * kz;
            }
            return mesh;
        }
        void addPointMesh(Point3d_GL[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Points, r, g, b);
        }
        void addLineFanMesh(float[] startpoint, float[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new float[points.Length * 2];
            var j = 0;
            for(int i=0; i<points.Length-3;i+=3)
            {
                mesh[j] = startpoint[0]; j++;
                mesh[j] = startpoint[1]; j++;
                mesh[j] = startpoint[2]; j++;
                mesh[j] = points[i]; j++;
                mesh[j] = points[i+1]; j++;
                mesh[j] = points[i+2]; j++;
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, r, g, b);
        }
        public void addLineMesh(Point3d_GL[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, r, g, b);
        }
        void addLineMesh(Vertex4f[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new float[points.Length * 3];
            int ind = 0;
            foreach (var p in points)
            {
                mesh[ind] = p.x; ind++;
                mesh[ind] = p.y; ind++;
                mesh[ind] = p.z; ind++;
            }
            addMeshWithoutNorm(mesh, PrimitiveType.Lines, r, g, b);
        }
        public void addMeshWithoutNorm(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = r;
                color_buffer_data[i + 1] = g;
                color_buffer_data[i + 2] = b;

                normal_buffer_data[i] = 0.1f;
                normal_buffer_data[i + 1] = 0.1f;
                normal_buffer_data[i + 2] = 0.1f;
            }
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }
        void addMeshColor(float[] gl_vertex_buffer_data, float[] gl_color_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1, p2, p3, U, V, Norm1, Norm;
            for (int i = 0; i < normal_buffer_data.Length; i += 9)
            {
                p1 = new Point3d_GL(gl_vertex_buffer_data[i], gl_vertex_buffer_data[i + 1], gl_vertex_buffer_data[i + 2]);
                p2 = new Point3d_GL(gl_vertex_buffer_data[i + 3], gl_vertex_buffer_data[i + 4], gl_vertex_buffer_data[i + 5]);
                p3 = new Point3d_GL(gl_vertex_buffer_data[i + 6], gl_vertex_buffer_data[i + 7], gl_vertex_buffer_data[i + 8]);
                U = p1 - p2;
                V = p1 - p3;
                Norm = new Point3d_GL(
                    U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x);
                Norm1 = Norm.normalize();
                normal_buffer_data[i] = (float)Norm1.x;
                normal_buffer_data[i + 1] = (float)Norm1.y;
                normal_buffer_data[i + 2] = (float)Norm1.z;

                normal_buffer_data[i + 3] = (float)Norm1.x;
                normal_buffer_data[i + 4] = (float)Norm1.y;
                normal_buffer_data[i + 5] = (float)Norm1.z;

                normal_buffer_data[i + 6] = (float)Norm1.x;
                normal_buffer_data[i + 7] = (float)Norm1.y;
                normal_buffer_data[i + 8] = (float)Norm1.z;
            }
            // Console.WriteLine("vert len " + gl_vertex_buffer_data.Length);
            add_buff_gl(gl_vertex_buffer_data, gl_color_buffer_data, normal_buffer_data, primitiveType);
        }
        public void addMesh(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = computeNormals(gl_vertex_buffer_data);
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = r;
                color_buffer_data[i + 1] = g;
                color_buffer_data[i + 2] = b;
            }
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }

        float[] computeNormals(float[] gl_vertex_buffer_data)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1, p2, p3, U, V, Norm1, Norm;
            for (int i = 0; i < normal_buffer_data.Length; i += 9)
            {
                p1 = new Point3d_GL(gl_vertex_buffer_data[i], gl_vertex_buffer_data[i + 1], gl_vertex_buffer_data[i + 2]);
                p2 = new Point3d_GL(gl_vertex_buffer_data[i + 3], gl_vertex_buffer_data[i + 4], gl_vertex_buffer_data[i + 5]);
                p3 = new Point3d_GL(gl_vertex_buffer_data[i + 6], gl_vertex_buffer_data[i + 7], gl_vertex_buffer_data[i + 8]);
                U = p1 - p2;
                V = p1 - p3;
                Norm = new Point3d_GL(
                    U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x);
                Norm1 = Norm.normalize();
                normal_buffer_data[i] = (float)Norm1.x;
                normal_buffer_data[i + 1] = (float)Norm1.y;
                normal_buffer_data[i + 2] = (float)Norm1.z;

                normal_buffer_data[i + 3] = (float)Norm1.x;
                normal_buffer_data[i + 4] = (float)Norm1.y;
                normal_buffer_data[i + 5] = (float)Norm1.z;

                normal_buffer_data[i + 6] = (float)Norm1.x;
                normal_buffer_data[i + 7] = (float)Norm1.y;
                normal_buffer_data[i + 8] = (float)Norm1.z;
            }
            return normal_buffer_data;
        }


        #endregion


        #region shader
        string[] assembCode(string[] paths)
        {
            var text = "";
            foreach (var path in paths)
                text += File.ReadAllText(path);
            return new string[] { text };
        }
        void debugShaderComp(uint ShaderName)
        {
            int compiled;

            Gl.GetShader(ShaderName, ShaderParameterName.CompileStatus, out compiled);
            if (compiled != 0)
            {
                Console.WriteLine("SHADER COMPILE");
                return;
            }


            // Throw exception on compilation errors
            const int logMaxLength = 1024;

            StringBuilder infolog = new StringBuilder(logMaxLength);
            int infologLength;

            Gl.GetShaderInfoLog(ShaderName, logMaxLength, out infologLength, infolog);

            throw new InvalidOperationException($"unable to compile shader: {infolog}");
        }
        private uint compileShader(string[] shSource, ShaderType shaderType)
        {
            uint ShaderID = Gl.CreateShader(shaderType);
            Gl.ShaderSource(ShaderID, shSource);
            Gl.CompileShader(ShaderID);
            debugShaderComp(ShaderID);
            return ShaderID;
        }
        private uint createShader(string[] VertexSourceGL, string[] GeometryShaderGL, string[] FragmentSourceGL)
        {
            bool geom = false;
            uint GeometryShaderID = 0;
            if (GeometryShaderGL!=null)
            {
                geom = true;
            }
            var VertexShaderID = compileShader(VertexSourceGL, ShaderType.VertexShader);
            var FragmentShaderID = compileShader(FragmentSourceGL, ShaderType.FragmentShader);
            if (geom)
            {
                GeometryShaderID = compileShader(GeometryShaderGL, ShaderType.GeometryShader);
            }

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, VertexShaderID);
            Gl.AttachShader(ProgrammID, FragmentShaderID);
            if(geom)
            {
                Gl.AttachShader(ProgrammID, GeometryShaderID);
            }
            Gl.LinkProgram(ProgrammID);

            int linked;

            Gl.GetProgram(ProgrammID, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetProgramInfoLog(ProgrammID, 1024, out infologLength, infolog);

                throw new InvalidOperationException($"unable to link program: {infolog}");
            }

            Gl.DeleteShader(VertexShaderID);
            Gl.DeleteShader(FragmentShaderID);
            if(geom)
            {
                Gl.DeleteShader(GeometryShaderID);
            }
            return ProgrammID;
        }
        private uint createShaderCompute(string[] ComputeSourceGL)
        {

            var ComputeShaderID = compileShader(ComputeSourceGL, ShaderType.ComputeShader);

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, ComputeShaderID);
            Gl.LinkProgram(ProgrammID);

            int linked;

            Gl.GetProgram(ProgrammID, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetProgramInfoLog(ProgrammID, 1024, out infologLength, infolog);

                throw new InvalidOperationException($"unable to link program: {infolog}");
            }

            Gl.DeleteShader(ComputeShaderID);
            return ProgrammID;
        }

        #endregion
    }

    /*
     * float[] compute(float[] data)
        {
            
            Gl.UseProgram(gluints.programID_comp);
            useTexture(gluints.buff_tex,0);
            Gl.DispatchCompute(16, 1, 1);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            useTexture(gluints.buff_tex1, 1);
            float[] pixels = new float[16];
            Gl.GetTexImage(TextureTarget.Texture2d, 0, PixelFormat.Red, PixelType.Float, pixels);
            //useTexture(buff_tex, 0);
            //bufferToComputeTex(pixels);
            printBuff(pixels);
            return pixels;
        }
     * 
     * private  string[] _VertexSourceGL = {
            "#version 460 core\n",


            "in vec3 _vertexPosition_modelspace;\n",
            "in vec3 _vertexNormal_modelspace;\n",
            "in vec3 _vertexColor;\n",

            "out VS_GS_INTERFACE\n",
            "{\n",
            "vec3 vertexPosition_modelspace;\n",
            "vec3 vertexNormal_modelspace;\n",
            "vec3 vertexColor;\n",
            "} vs_out;\n",

            "void main() {\n",
            "   gl_Position = vec4(_vertexPosition_modelspace, 1.0);\n",
            "	vs_out.vertexPosition_modelspace = _vertexPosition_modelspace;\n",
            "	vs_out.vertexNormal_modelspace = _vertexNormal_modelspace;\n",
            "	vs_out.vertexColor = _vertexColor;\n",
            "}\n"
        };
        private  string[] _GeometryShaderBody = {
            "uniform mat4 MVP;\n",
            "uniform mat4 M;\n",
            "uniform mat4 V;\n",
            "uniform vec3 LightPosition_worldspace;\n",

            "in VS_GS_INTERFACE\n",
            "{\n",
            "vec3 vertexPosition_modelspace;\n",
            "vec3 vertexNormal_modelspace;\n",
            "vec3 vertexColor;\n",
              "}vs_out[];\n",

            "out GS_FS_INTERFACE\n",
            "{\n",
            "vec3 Position_worldspace;\n",
            "vec3 Color;\n",
            "vec3 Normal_cameraspace;\n",
            "vec3 EyeDirection_cameraspace;\n",
            "vec3 LightDirection_cameraspace;\n",
            "} fs_in;\n",

            "uniform mat4 MVPs[4];\n",
            "uniform mat4 Ms[4];\n",
            "uniform mat4 Vs[4];\n",



            "void main() {\n",

            "   for (int i = 0; i < gl_in.length(); i++){ \n",
            "	    gl_ViewportIndex = gl_InvocationID;\n",

            "       gl_Position = MVPs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_modelspace, 1.0);\n",

            "	    fs_in.Position_worldspace = (M * vec4(vs_out[i].vertexPosition_modelspace,1)).xyz;\n",

            "	    vec3 vertexPosition_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexPosition_modelspace,1)).xyz;\n",

            "	    float[16] mx = float[](0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);\n",

            "	    fs_in.EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;\n",
            "	    vec3 LightPosition_cameraspace = ( Vs[gl_InvocationID] * vec4(LightPosition_worldspace,1)).xyz;\n",
            "	    fs_in.LightDirection_cameraspace = LightPosition_cameraspace + fs_in.EyeDirection_cameraspace;\n",
            "	    fs_in.Normal_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexNormal_modelspace,0)).xyz;\n",
            "	    fs_in.Color = vs_out[i].vertexColor;\n",

            "	    EmitVertex();}\n",


            "}\n"
        };

        private string[] _GeometryShaderLinesGL = {
            "#version 460 core\n",

            "layout (lines, invocations = 4) in;\n",
            "layout (line_strip, max_vertices = 2) out;\n", 
        };

        private  string[] _GeometryShaderPointsGL = {
            "#version 460 core\n",

            "layout (points, invocations = 4) in;\n",
            "layout (points, max_vertices = 1) out;\n", 
        };

        private string[] _GeometryShaderTrianglesGL = {
            "#version 460 core\n",

            "layout (triangles, invocations = 4) in;\n",
            "layout (triangle_strip, max_vertices = 3) out;\n",

        };

        private string[] _FragmentSourceGL = {
            "#version 460 core\n",
            "uniform vec3 LightPosition_worldspace;\n",
            "uniform vec3 MaterialDiffuse;\n",
            "uniform vec3 MaterialAmbient;\n",
            "uniform vec3 MaterialSpecular;\n",
            "uniform float lightPower;\n",


            "in GS_FS_INTERFACE\n",
            "{\n",
            "vec3 Position_worldspace;\n",
            "vec3 Color;\n",
            "vec3 Normal_cameraspace;\n",
            "vec3 EyeDirection_cameraspace;\n",
            "vec3 LightDirection_cameraspace;\n",
              "}fs_in;\n",


            //"in int  gl_ViewportIndex;\n",

            "out vec4 color;\n",
            "void main() {\n",
            "	vec3 LightColor = vec3(1,1,1);\n",
            "	float LightPower = lightPower;\n",
            "	vec3 MaterialDiffuseColor = MaterialDiffuse;\n",
            "	vec3 MaterialAmbientColor = MaterialAmbient;\n",
            "	vec3 MaterialSpecularColor = MaterialSpecular;\n",
            "	float distance = length( LightPosition_worldspace - fs_in.Position_worldspace );\n",
            "	vec3 n = normalize( fs_in.Normal_cameraspace );\n",
            "	vec3 l = normalize( fs_in.LightDirection_cameraspace );\n",
            "	float cosTheta = clamp( dot( n,l ), 0,1 );\n",
            "	vec3 E = normalize(fs_in.EyeDirection_cameraspace);\n",
            "	vec3 R = reflect(-l,n);\n",
            "	float cosAlpha = clamp( dot( E,R ), 0,1 );\n",
             "	MaterialDiffuseColor = fs_in.Color;\n",
            "	color.xyz = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);\n",
            "	color.w = 1.0;\n",
          //  "	if(gl_ViewportIndex==0)\n",
           // "	{color.xyz =  vec3(1,0,0);}\n",
          //  "	if(gl_ViewportIndex==1)\n",
          //  "	{color.xyz =  vec3(0,1,0);}\n",
            //"	if(gl_ViewportIndex);\n",
           // "	color.xyz = Color;\n",
          //  "	float color_grey = (color_true.x+color_true.y+color_true.z)/3;\n",
           // "	color = vec3(color_grey,color_grey,color_grey);\n",
            "}\n"
        };


        private string[] _FragmentSourceGL_ = {
            "#version 460 core\n",
            "uniform vec3 LightPosition_worldspace;\n",
            "uniform vec3 MaterialDiffuse;\n",
            "uniform vec3 MaterialAmbient;\n",
            "uniform vec3 MaterialSpecular;\n",
            "uniform float lightPower;\n",
            "in vec3 Color;\n",
            "in vec3 Position_worldspace;\n",
            "in vec3 Normal_cameraspace;\n",
            "in vec3 EyeDirection_cameraspace;\n",
            "in vec3 LightDirection_cameraspace;\n",
            "out vec4 color;\n",
            "void main() {\n",
            "	vec3 LightColor = vec3(1,1,1);\n",
            "	float LightPower = lightPower;\n",
            "	vec3 MaterialDiffuseColor = MaterialDiffuse;\n",
            "	vec3 MaterialAmbientColor = MaterialAmbient;\n",
            "	vec3 MaterialSpecularColor = MaterialSpecular;\n",
            "	float distance = length( LightPosition_worldspace - Position_worldspace );\n",
            "	vec3 n = normalize( Normal_cameraspace );\n",
            "	vec3 l = normalize( LightDirection_cameraspace );\n",
            "	float cosTheta = clamp( dot( n,l ), 0,1 );\n",
            "	vec3 E = normalize(EyeDirection_cameraspace);\n",
            "	vec3 R = reflect(-l,n);\n",
            "	float cosAlpha = clamp( dot( E,R ), 0,1 );\n",
             "	MaterialDiffuseColor = Color;\n",
            "	color.xyz = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);\n",
            "	color.w = 1.0;\n",
           // "	color.xyz = Color;\n",
          //  "	float color_grey = (color_true.x+color_true.y+color_true.z)/3;\n",
           // "	color = vec3(color_grey,color_grey,color_grey);\n",
            "}\n"
        };
        private string[] _GeometryShaderGL_ = {
            "#version 460 core\n",
            //"uniform mat4 MVP;\n",
            //"uniform mat4 M;\n",
            //"uniform mat4 V;\n",
            //"uniform vec3 LightPosition_worldspace;\n",

           // "in vec3 vertexPosition_modelspace;\n",
           // "in vec3 vertexNormal_modelspace;\n",
           // "in vec3 vertexColor;\n",


           // "out vec3 Color;\n",
           // "out vec3 Position_worldspace;\n",
           // "out vec3 Normal_cameraspace;\n",
           // "out vec3 EyeDirection_cameraspace;\n",
           // "out vec3 LightDirection_cameraspace;\n",
            "void main() {\n",
            "   for (int i = 0; i < gl_in.length(); i++)\n",
            "	gl_ViewportIndex = gl_InvocationID;\n",
            "	gs_color = colors[gl_InvocationID];\n",
            "	gs_normal = (model_matrix[gl_InvocationID] * vec4(vs_normal[i], 0.0)).xyz;\n",
            "	gl_Position = projection_matrix * (model_matrix[gl_InvocationID] * gl_in[i].gl_Position);\n",
            "	EmitVertex();\n",
           // "	Normal_cameraspace = ( V * M * vec4(vertexNormal_modelspace,0)).xyz;\n",
           // "	Color = vertexColor;\n",
            "}\n"
        };
     * 
     */
}
