using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Graphic
{

    public struct openGlobj
    {
        public enum AnimType { Static, Dynamic }
        public float[] vertex_buffer_data;
        public float[] color_buffer_data;
        public float[] normal_buffer_data;
        public float[] texture_buffer_data;
        public int vert_len;
        public PrimitiveType tp;
        public AnimType animType;
        public int id;
        public bool visible;
        uint buff_array;
        uint buff_array_orb;
        public trsc[] trsc;
        public int count;
        public int orb_count;
        uint[] buffs_orb;


        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, float[] t_buf, PrimitiveType type, int _id= -1,int _count=1)
        {
            vertex_buffer_data = new float[v_buf.Length];
            normal_buffer_data = new float[n_buf.Length];
            orb_count = 4;
            buffs_orb = new uint[orb_count];

            trsc = new trsc[_count];
            count = _count;
            for(int i=0; i<_count;i++)
            {
                trsc[i] = new trsc(0, 0, 0, 0, 0, 0, 1);
            }

            buff_array = 0; buff_array_orb = 0;
            if (t_buf == null)
            {
                texture_buffer_data = new float[v_buf.Length];
            }
            else
            {
                texture_buffer_data = new float[t_buf.Length];
                t_buf.CopyTo(texture_buffer_data, 0);
            }

            if (c_buf == null)
            {
                color_buffer_data = new float[v_buf.Length];
                
            }
            else
            {
                color_buffer_data = new float[c_buf.Length];
                c_buf.CopyTo(color_buffer_data, 0);
            }
            vert_len =(int) v_buf.Length / 3;
            v_buf.CopyTo(vertex_buffer_data, 0);       
            n_buf.CopyTo(normal_buffer_data, 0);
            
            tp = type;
            visible = true;
            id = _id;
            if ( _id == -1)
            {
                animType = AnimType.Static;
            }
            else
            {
                animType = AnimType.Dynamic;
            }
        }

        public openGlobj setBuffersObj()
        {
            buff_array = Gl.GenVertexArray();
            Gl.BindVertexArray(buff_array);
            setBuffer(vertex_buffer_data, 0, 3);
            setBuffer(normal_buffer_data, 1, 3);
            setBuffer(color_buffer_data, 2, 3);
            setBuffer(texture_buffer_data, 3, 2);

            setBuffersOrbite();
            return this;
        }
        public openGlobj setBuffersOrbite()
        {
            buff_array_orb = Gl.GenVertexArray();
            Gl.BindVertexArray(buff_array_orb);

            var initDataOrb = orbDataFromTrsc();
            for(int i=0; i<orb_count; i++)
            {
                buffs_orb[i]= setBuffer(initDataOrb, (uint)i, 3);
            }

            return this;
        }

        public void updateOrbData()
        {
            Gl.BindVertexArray(buff_array_orb);
            float[] data = orbDataFromTrsc();

            for (int i = 0; i < orb_count-1; i++)
            {
                setVertexAttrib(buffs_orb[i], (uint)i+1, 3);
            }
            buffs_orb[orb_count - 1] = setBuffer(data, 0, 3);
        }
        public void updateOrbDataLong()
        {
            Gl.BindVertexArray(buff_array_orb);
            float[] data = orbDataFromTrsc();
            for (int i = 0; i < orb_count - 1; i++)
            {
                setVertexAttrib(buffs_orb[i], (uint)i + 1, 3);
            }
            buffs_orb[orb_count - 1] = setBuffer(data, (uint)(orb_count - 1), 3);
        }
        float[] orbDataFromTrsc()
        {
            var data = new float[trsc.Length * 3];
            for(int i=0; i<trsc.Length; i++)
            {
                data[3 * i] = (float)trsc[i].transl.x;
                data[3 * i+1] = (float)trsc[i].transl.y;
                data[3 * i+2] = (float)trsc[i].transl.z;
            }
            return data;
        }
        public void useBuffers()
        {
            Gl.BindVertexArray(buff_array);
        }
        public void useOrbit()
        {
            Gl.BindVertexArray(buff_array_orb);
        }
        public void loadModels()
        {
            bindBufferInstanceMatr(modelData(), 4);
        }
        uint setBuffer(float[] data, uint lvl, int strip)
        {
            var buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
            Gl.EnableVertexAttribArray(lvl);
            Gl.VertexAttribPointer(lvl, strip, VertexAttribType.Float, false, 0, (IntPtr)0);
            return buff;
        }
        void setVertexAttrib( uint buff, uint lvl, int strip)
        {
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.EnableVertexAttribArray(lvl);
            Gl.VertexAttribPointer(lvl, strip, VertexAttribType.Float, false, 0, (IntPtr)0);
        }
        void setBufferData(uint buff, float[] data)
        {
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
        }
        Matrix4x4f[] modelData()
        {
            var matrs = new Matrix4x4f[trsc.Length];
            for(int i=0; i<trsc.Length;i++)
            {
                matrs[i] = trsc[i].getModelMatrix();
            }
            return matrs;
        }
        uint bindBufferInstanceMatr(Matrix4x4f[] data, uint lvl)
        {
            var buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4*16 * data.Length), data, BufferUsage.DynamicDraw);

            Gl.EnableVertexAttribArray(lvl);
            Gl.VertexAttribPointer(lvl, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)0);

            Gl.EnableVertexAttribArray(lvl + 1);
            Gl.VertexAttribPointer(lvl + 1, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)(4 * 4));

            Gl.EnableVertexAttribArray(lvl + 2);
            Gl.VertexAttribPointer(lvl + 2, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)(4 * 8));

            Gl.EnableVertexAttribArray(lvl + 3);
            Gl.VertexAttribPointer(lvl + 3, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)(4 * 12));

            Gl.VertexAttribDivisor(lvl, 1);
            Gl.VertexAttribDivisor(lvl+1, 1);
            Gl.VertexAttribDivisor(lvl+2, 1);
            Gl.VertexAttribDivisor(lvl+3, 1);
        
            return buff;
        }


        #region setters
        public openGlobj setScale(int i,float _scale)
        {
            trsc[i].scale = _scale;
            return this;
        }

        public openGlobj setTransf(int i, trsc _trsc)
        {
            trsc[i] = _trsc;
            return this;
        }

        public openGlobj setTransf(int i, Point3d_GL transl, Point3d_GL rotate)
        {
            trsc[i] = new trsc(transl, rotate, trsc[i].scale);
            return this;
        }
        public openGlobj setX(int i, double x)
        {
            trsc[i].transl.x = x;
            return this;
        }
        public openGlobj setY(int i, double y)
        {
            trsc[i].transl.y = y;
            return this;
        }
        public openGlobj setZ(int i, double z)
        {
            trsc[i].transl.z = z;
            return this;
        }
        public openGlobj setRotX(int i, double x)
        {
            trsc[i].rotate.x = x;
            return this;
        }
        public openGlobj setRotY(int i, double y)
        {
            trsc[i].rotate.y = y;
            return this;
        }
        public openGlobj setRotZ(int i, double z)
        {
            trsc[i].rotate.z = z;
            return this;
        }
        #endregion
    }

}
