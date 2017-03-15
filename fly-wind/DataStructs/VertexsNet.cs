using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fly_wind.DataStructs {
    class VertexsNet {

        private HashSet<Vertex> nodes = new HashSet<Vertex>();

        public HashSet<Vertex> getNet() {
            return nodes;
        }
    }
}
