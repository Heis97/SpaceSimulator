using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Objects
{
    public  class Space
    {
        const double G = 6.6743E-11;

        List<NativeObj> NativeObjs = new List<NativeObj>();


        static void computeGravit(NativeObj NativeObj1, NativeObj NativeObj2)
        {
            var vec = NativeObj1.position - NativeObj2.position;
            var vec1 = vec.normalize();
            var vec2 = -vec1;
            double dist = vec.magnitude();
            double F = G * (NativeObj1.mass * NativeObj2.mass) / (dist * dist);
            var gravF1 = F * vec1;
            var gravF2 = F * vec2;
            //return new Point3d_GL();
        }

        
    }
}
