﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyWind.DataStructs.Point;

namespace FlyWind.DataStructs {
    class Vertex : Vector3{

        List<Vertex> contract;

        public Vertex(double x = 0f, double y = 0f, double z = 0f) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
