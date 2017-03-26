using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyWind.DataStructs.Point;

namespace FlyWind.DataStructs.Point {
    class Navigation : Vector3 {
        
        public Navigation(double x = 0f, double y = 0f, double z =0f) : base(x, y, z) { }
        public Navigation(IVector3 v) : base(v) { }

        public Vector3 position() {
            return new Vector3(x, y, z);
        }

        public Navigation moveTo(Vector3 direction, double range) {
            return new Navigation(position() + direction * range);
        }
    }
}
