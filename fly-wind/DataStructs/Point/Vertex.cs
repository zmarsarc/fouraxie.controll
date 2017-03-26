using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyWind.DataStructs.Point;

namespace FlyWind.DataStructs.Point {
    class Vertex : Vector3{

        private List<Vertex> contract;

        public Vertex(double x = 0f, double y = 0f, double z = 0f) : base(x, y, z) { }
        public Vertex(IVector3 v) : base(v) { }

        public List<Vertex> contracts() {
            return contract;
        }
    }
}
