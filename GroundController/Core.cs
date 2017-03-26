using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundController {

    public class Vertex {

        public double x;
        public double y;
        public double z;

        private List<Vertex> contracts = new List<Vertex>();


        public Vertex(double x, double y, double z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public List<Vertex> GetContracts() {
            return contracts;
        }

        public int AddContract(Vertex vertex) {
            contracts.Add(vertex);
            return contracts.Count;
        }
    }

    public class Graph {

        private List<Vertex> vertexs = new List<Vertex>();

        public Graph() { }

        public int Add(Vertex v) {
            vertexs.Add(v);
            return vertexs.Count;
        }

        public List<Vertex> GetVertexs() {
            return vertexs;
        }
    }

    public class Core {

        /// <summary>
        /// 给出一个点，在它的邻接点集合中找出一个最靠近目标的点。
        /// </summary>
        /// <param name="current">指定点</param>
        /// <param name="destination">目标点</param>
        /// <returns>最符合要求的点，可以是指定点自身</returns>
        public Vertex NextVertex(Vertex current, Vertex destination) {
            List<Vertex> contracts = current.GetContracts();
            Vertex bestMatch = current;

            double currentDistance = Math.Sqrt(
                Math.Pow(destination.x - current.x, 2f) +
                Math.Pow(destination.y - current.y, 2f) +
                Math.Pow(destination.z - current.z, 2f));

            foreach (Vertex ver in contracts) {
                double nextDistance = Math.Sqrt(
                    Math.Pow(destination.x - ver.x, 2f) +
                    Math.Pow(destination.y - ver.y, 2f) +
                    Math.Pow(destination.z - ver.z, 2f));
                if (nextDistance < currentDistance) {
                    bestMatch = ver;
                } 
            }

            return bestMatch;
        }

        /// <summary>
        /// 在给出的图中求出一条从起点到终点的点序列，序列中包含起点但不含终点。
        /// </summary>
        /// <param name="net">图</param>
        /// <param name="start">起点，必须在图中</param>
        /// <param name="end">终点</param>
        /// <returns>点序列，如果起点不在图中则返回null，否则返回一个不为空的序列</returns>
        public List<Vertex> GenerateWayPointSecquence(Graph net, Vertex start, Vertex end) {
            List<Vertex> wayPoints = new List<Vertex>();
            List<Vertex> vertexs = net.GetVertexs();

            if (null == vertexs.Find(x => x.GetHashCode() == start.GetHashCode())) {
                throw new ArgumentException("start point not in net");
            }

            Vertex current = start;
            while (current != end) {
                wayPoints.Add(current);
                Vertex next = NextVertex(current, end);
                if (current == next) {
                    break;
                }
                current = next;
            }

            return wayPoints;
        }

    }
}
