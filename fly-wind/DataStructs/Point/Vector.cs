using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fly_wind.DataStructs.Point {
    interface IVector3 {
        double x { get; set; }
        double y { get; set; }
        double z { get; set; }
        double distanceFrom(double x, double y, double z);
        double distanceFrom(IVector3 vec3);
    }

    interface IVector2 {
        double x { get; set; }
        double y { get; set; }
        double distanceFrom(double x, double y);
        double distanceFrom(IVector2 vec2);
    }
}
