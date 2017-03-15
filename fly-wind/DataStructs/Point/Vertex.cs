using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fly_wind.DataStructs {
    class Vertex {

        public double x;
        public double y;
        public double  z;

        List<Vertex> contract;

        public Vertex() {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;

            contract = new List<Vertex>();
        }
    }
}
