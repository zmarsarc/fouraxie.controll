﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyWind.DataStructs.Point;

namespace FlyWind.DataStructs {
    class VertexsNet {

        private List<Vertex> nodes = new List<Vertex>();

        public List<Vertex> getNet() {
            return nodes;
        }
    }
}
