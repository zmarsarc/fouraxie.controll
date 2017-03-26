using FlyWind.DataStructs;
using FlyWind.DataStructs.Point;
using System;
using System.Collections.Generic;

namespace FlyWind {
    class WayPointGenerater {
        
        public List<Navigation> generate(VertexsNet vn) {
            Vertex initPoint = findInitWayPoing(vn);
            return generateWayPoints(vn, initPoint);
        }

        /// <summary>
        /// 输入一个顶点网络图并指定起始点，生成导航点序列
        /// </summary>
        /// <param name="nm">顶点网络</param>
        /// <param name="initPoint">起始路点</param>
        /// <returns>包含导航点的 List</returns>
        private List<Navigation> generateWayPoints(VertexsNet vn, Vertex initPoint) {
            List<Navigation> wps = new List<Navigation>();
            


            return wps;
        }

        /// <summary>
        /// 输入一个顶点网络，确定一个起始路点
        /// </summary>
        /// <param name="vn">顶点网络</param>
        /// <returns>Vertex -> 起始路点</returns>
        private Vertex findInitWayPoing(VertexsNet vn) {
            // 策略是最靠近原点的第一个点的定位起始点
            Vertex ret = new Vertex(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            foreach (var nav in vn.getNet()) {
                ret = (ret.distanceFrom(Vector3.zero) < nav.distanceFrom(Vector3.zero) ? ret : nav);
            }
            return ret;
        }
    }
}
