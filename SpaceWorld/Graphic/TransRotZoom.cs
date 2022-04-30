using Emgu.CV;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Graphic
{

    public struct trsc
    {
        public Point3d_GL transl;
        public Point3d_GL rotate;
        public float scale;

        public trsc(Point3d_GL _transl, Point3d_GL _rotate, float _scale)
        {
            transl = _transl.Clone();
            rotate = _rotate.Clone();
            scale = _scale;
        }
        public trsc(double x, double y, double z, double rx, double ry, double rz, float _scale)
        {
            transl = new Point3d_GL(x, y, z);
            rotate = new Point3d_GL(rx, ry, rz);
            scale = _scale;
        }

        public Matrix4x4f getModelMatrix(Vertex3f target)
        {
           return  Matrix4x4f.Translated((float)transl.x-target.x, (float)transl.y - target.y, (float)transl.z - target.z) *
                Matrix4x4f.RotatedX((float)rotate.x) *
                Matrix4x4f.RotatedY((float)rotate.y) *
                Matrix4x4f.RotatedZ((float)rotate.z) *
                Matrix4x4f.Scaled(scale, scale, scale);
        }

        public Matrix4x4f getRotateMatrix()
        {
            return Matrix4x4f.RotatedX((float)rotate.x) *
                 Matrix4x4f.RotatedY((float)rotate.y) *
                 Matrix4x4f.RotatedZ((float)rotate.z);
                
        }


    }

    public class TransRotZoom
    {
        public enum TRZtype { Master, Slave, Const }
        public double zoom;
        public double xRot;
        public double yRot;
        public double zRot;
        public TRZtype type;
        public viewType viewType_;
        public int id;
        public int id_m;
        public bool visible;
        public Rectangle rect;
        public DateTime dateTime;
        public TransRotZoom consttransf;
        public Vertex3f target;
        public Vertex3f pos;
        //public Vertex3f localpos;

        public TransRotZoom(Rectangle _rect, int _id)
        {
            zoom = -1e-1;
            xRot = 0;
            yRot = 0;
            zRot = 0;
            rect = _rect;
            id = _id;
            type = TRZtype.Master;
            viewType_ = viewType.Perspective;
            visible = false;
            target = new Vertex3f(1.001f, 0, 0);
            pos = new Vertex3f(0, 0, -1e-1f);
            //localpos = new Vertex3f(0, 0, -5);
        }

        public TransRotZoom(Rectangle _rect, int _id, Vertex3f rotVer, Vertex3f transVer, int _idMast)
        {
            zoom = 0.1;
            xRot = 0;
            yRot = 0;
            zRot = 0;

            pos = new Vertex3f(0, 0, 4);
            rect = _rect;
            id = _id;
            id_m = _idMast;
            type = TRZtype.Slave;
            consttransf = new TransRotZoom(rotVer, transVer);
            viewType_ = viewType.Perspective;
            visible = false;
            
        }
        public TransRotZoom(Vertex3f rotVer, Vertex3f transVer)
        {
            xRot = rotVer.x;
            yRot = rotVer.y;
            zRot = rotVer.z;
            pos.x = transVer.x;
            pos.y = transVer.y;
            pos.z = transVer.z;
            type = TRZtype.Const;
            viewType_ = viewType.Perspective;
            visible = false;
        }

        public TransRotZoom getInfo(TransRotZoom[] transRotZooms)
        {
            switch (type)
            {
                case TRZtype.Master:
                    return this;

                case TRZtype.Slave:
                    var trz_m = transRotZooms[id_m];
                    var trz_info = new TransRotZoom();
                    trz_info.zoom = trz_m.zoom;
                    trz_info.pos.x = trz_m.pos.x + consttransf.pos.x;
                    trz_info.pos.y = trz_m.pos.y + consttransf.pos.y;
                    trz_info.pos.z = trz_m.pos.z + consttransf.pos.z;
                    trz_info.xRot = trz_m.xRot + consttransf.xRot;
                    trz_info.yRot = trz_m.yRot + consttransf.yRot;
                    trz_info.zRot = trz_m.zRot + consttransf.zRot;
                    trz_info.viewType_ = trz_m.viewType_;
                    trz_info.rect = rect;
                    trz_info.visible = trz_m.visible;

                    return trz_info;
                default:
                    return null;
            }
        }
        public TransRotZoom(TransRotZoom _trz)
        {
            zoom = _trz.zoom;
            xRot = _trz.xRot;
            yRot = _trz.yRot;
            zRot = _trz.zRot;
            pos.x = _trz.pos.x;
            pos.y = _trz.pos.y;
            pos.z = _trz.pos.z;
            rect = _trz.rect;
            id = _trz.id;
            type = _trz.type;
        }
        public TransRotZoom()
        {

        }

        public TransRotZoom(string data)
        {
            var dt = data.Split();
            if (dt.Length < 7)
            {

            }
            xRot = Convert.ToDouble(dt[0]);
            yRot = Convert.ToDouble(dt[1]);
            zRot = Convert.ToDouble(dt[2]);
            pos.x = Convert.ToSingle(dt[3]);
            pos.y = Convert.ToSingle(dt[4]);
            pos.z = Convert.ToSingle(dt[5]);
            zoom = Convert.ToDouble(dt[6]);
        }
        public TransRotZoom(double _xRot, double _yRot, double _zRot,
            float _off_x, float _off_y, float _off_z, double _zoom)
        {
            xRot = _xRot;
            yRot = _yRot;
            zRot = _zRot;
            pos.x = _off_x;
            pos.y = _off_y;
            pos.z = _off_z;
            zoom = _zoom;
        }
        public void setTrz(double _xRot, double _yRot, double _zRot,
            float _off_x, float _off_y, float _off_z, double _zoom)
        {
            xRot = _xRot;
            yRot = _yRot;
            zRot = _zRot;
            pos.x = _off_x;
            pos.y = _off_y;
            pos.z = _off_z;
            zoom = _zoom;
        }
        public TransRotZoom minusDelta(TransRotZoom trz)
        {
            var _xRot = xRot - trz.xRot;
            var _yRot = yRot - trz.yRot;
            var _zRot = zRot - trz.zRot;
            var _off_x = pos.x - trz.pos.x;
            var _off_y = pos.y - trz.pos.y;
            var _off_z = pos.z - trz.pos.z;
            var _zoom = zoom - trz.zoom;
            return new TransRotZoom(_xRot, _yRot, _zRot, _off_x, _off_y, _off_z, _zoom);
        }
        public void setTrz(TransRotZoom trz)
        {
            xRot = trz.xRot;
            yRot = trz.yRot;
            zRot = trz.zRot;
            pos.x = trz.pos.x;
            pos.y = trz.pos.y;
            pos.z = trz.pos.z;
            zoom = trz.zoom;
        }
        public override string ToString()
        {
            return xRot + " " + yRot + " " + zRot + " "
                + pos.x + " " + pos.y + " " + pos.z + " "
                + zoom + " " + viewType_ + " ";
        }

        public static TransRotZoom operator -(TransRotZoom trz1, TransRotZoom trz2)
        {
            return new TransRotZoom(
                trz1.xRot - trz2.xRot,
                 trz1.yRot - trz2.yRot,
                  trz1.zRot - trz2.zRot,
                   trz1.pos.x - trz2.pos.x,
                   trz1.pos.y - trz2.pos.y,
                   trz1.pos.z - trz2.pos.z,
                   trz1.zoom - trz2.zoom
                  );
        }
        public void setxRot(double value)
        {
            xRot = value;
        }
        public void setyRot(double value)
        {
            yRot = value;
        }
        public void setzRot(double value)
        {
            zRot = value;
        }
        public void setRot(double valuex, double valuey, double valuez)
        {
            xRot = valuex;
            yRot = valuey;
            zRot = valuez;
        }

        Vertex3f cross(Vertex3f U, Vertex3f V)
        {
            return new Vertex3f(
                U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x
                );
        }
        public Matrix4x4f[] getVPmatrix()
        {

            if (viewType_ == viewType.Perspective)
            {
                var _Pm = Matrix4x4f.Perspective(53f, (float)rect.Width / rect.Height, -(float)zoom*0.1f, -(float)zoom * 1e4f);
                var _Vm = Matrix4x4f.Translated(0, 0, (float)zoom) *
                    Matrix4x4f.RotatedX((float)xRot) *
                    Matrix4x4f.RotatedY((float)yRot) *
                    Matrix4x4f.RotatedZ((float)zRot);// *Matrix4x4f.Translated(-target.x, -target.y, -target.z);

               /* var camDir = (pos - target).Normalized;
                var vecu = new Vertex3f(0, 0, -1);
                var rcam = cross(vecu, camDir).Normalized;
                var ucam = cross(camDir, rcam).Normalized;
                var loc4 = Matrix4x4f.RotatedX((float)xRot) *
                    Matrix4x4f.RotatedY((float)yRot) *
                    Matrix4x4f.RotatedZ((float)zRot)*  new Vertex4f((float)zoom, 0, 0,1);
                localpos = new Vertex3f(loc4.x, loc4.y, loc4.z);
                pos = target+localpos; 
                _Vm = Matrix4x4f.LookAt(pos, target, ucam);*/
                //var _PVm
                return new Matrix4x4f[] { _Pm, _Vm, _Pm * _Vm };
            }
            else if (viewType_ == viewType.Ortho)
            {
                var _Pm = Matrix4x4f.Ortho(-1, 1, -1, 1, 0.00001f, 10000f);
                var _Vm = Matrix4x4f.Translated(pos.x, -pos.y, (float)zoom * pos.z) *
                    Matrix4x4f.RotatedX((float)xRot) *
                    Matrix4x4f.RotatedY((float)yRot) *
                    Matrix4x4f.RotatedZ((float)zRot);
                return new Matrix4x4f[] { _Pm, _Vm, _Pm * _Vm };
            }
            else
            {
                return new Matrix4x4f[] { Matrix4x4f.Identity, Matrix4x4f.Identity, Matrix4x4f.Identity };
            }

        }
    }

}
