using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;
using OpenGL;

namespace Objects
{
    public struct ObjectMassGL
    {
        float mass;
        public float size;
        public float true_size;
        public Vertex3f pos;
        Vertex3f vel;
        public Vertex3f posrot;
        Vertex3f velrot;
        public int mesh_number;
       // public int ind_center_obj;
        static int datalen = 44;
        Vertex4f root_inf1;
        Vertex4f root_inf2;
        Vertex4f root_inf3;
        public ObjectMassGL(int _mesh_number,float _mass, float _size, float _true_size, Vertex3f _pos, Vertex3f _vel, Vertex3f _posrot , Vertex3f _velrot, Vertex4f _root_inf1 = new Vertex4f(), Vertex4f _root_inf2 = new Vertex4f(), Vertex4f _root_inf3 = new Vertex4f())
        {
            mesh_number = _mesh_number;
            mass = _mass;
            size = _size;
            true_size = _true_size;
            pos = _pos;
            vel = _vel;
            posrot = _posrot;
            velrot = _velrot;
            //ind_center_obj = _ind_center_obj;
            root_inf1 = _root_inf1;
            root_inf2 = _root_inf2;
            root_inf3 = _root_inf3;

          
        }

        public float[] getData()
        {
            return new float[] { 
                pos.x, pos.y, pos.z, root_inf1.x,
                vel.x, vel.y, vel.z, size,
                posrot.x, posrot.y, posrot.z, true_size, //поворот
                velrot.x, velrot.y, velrot.z, 0, //поворот скорость,root is comp
                1, 0, 0, 0,//матрица
                0, 1, 0, 0,//4
                0, 0, 1, 0,//х
                0, 0, 0, 1,//4
                mass, root_inf1.y, root_inf1.z,root_inf1.w,//ind_loc_obj,root_count, ind0,ind1,
                root_inf2.x, root_inf2.y, root_inf2.z,root_inf2.w,//ind2,ind3,ind4,ind5,
                root_inf3.x, root_inf3.y, root_inf3.z,root_inf3.w,

                 };
        }
        public ObjectMassGL Clone()
        {
            return new ObjectMassGL(mesh_number, mass, size, true_size, pos, vel, posrot, velrot, root_inf1, root_inf2, root_inf3);
        }
        public ObjectMassGL setData(float[] data)
        {
            pos.x = data[0];
            pos.y = data[1];
            pos.z = data[2];
            mass = data[32];
            vel.x = data[4];
            vel.y = data[5];
            vel.z = data[6];
            size = data[7];

            posrot.x = data[8];
            posrot.y = data[9];
            posrot.z = data[10];
            true_size = data[11];
            velrot.x = data[12];
            velrot.y = data[13];
            velrot.z = data[14];

            root_inf1.x = data[3];

            return this;
        }
        static public int getLength()
        {
            return datalen;
        }

        public override string ToString()
        {
            return pos.ToString();
        }
    }
    public class ObjectMass
    {
        public double mass;
        double energy;
        public Point3d_GL position;
        Point3d_GL rotation;
        Point3d_GL translation_velocity;
        Point3d_GL rotation_velocity;
        Point3d_GL inertia_moment;
        Point3d_GL force_result;
        Point3d_GL moment_result;


    }

    public class NativeObj :ObjectMass
    {
        public NativeObj()
        {
           
        }
    }
}
