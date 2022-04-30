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
        public Vertex3f pos;
        Vertex3f vel;
        static int datalen = 8;
        public ObjectMassGL(float _mass, float _size, Vertex3f _pos, Vertex3f _vel)
        {
            mass = _mass;
            size = _size;
            pos = _pos;
            vel = _vel;
        }

        public float[] getData()
        {
            return new float[] { pos.x, pos.y, pos.z, mass, vel.x, vel.y, vel.z, size };
        }
        public ObjectMassGL setData(float[] data)
        {
            pos.x = data[0];
            pos.y = data[1];
            pos.z = data[2];
            mass = data[3];
            vel.x = data[4];
            vel.y = data[5];
            vel.z = data[6];
            size = data[7];
            return this;
        }
        static public int getLength()
        {
            return datalen;
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
