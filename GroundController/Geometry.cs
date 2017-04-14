using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GroundController {
    class Geometry {
    }

    class LineStrip {

        private List<Vertex> vertexs;

        public void Extend(List<Vertex> vex) {
            vertexs.AddRange(vex);
        }

        public void Add(Vertex vex) {
            vertexs.Add(vex);
        }

        public void Draw() {
            
        }
    }
}
