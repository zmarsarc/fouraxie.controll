using Microsoft.VisualStudio.TestTools.UnitTesting;
using GroundController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundController.Tests {
    [TestClass()]
    public class CoreTests {
        [TestMethod()]
        public void nextVertexTest() {
            Vertex start = new Vertex(0f, 0f, 0f);
            Vertex end = new Vertex(10f, 10f, 10f);

            start.AddContract(new Vertex(-1f, -1f, -1f));
            start.AddContract(new Vertex(1f, 1f, 1f));

            Vertex bestMatch = new Vertex(2f, 2f, 2f);
            start.AddContract(bestMatch);

            Core core = new Core();

            Assert.AreEqual(bestMatch, core.NextVertex(start, end));
        }

        [TestMethod()]
        public void GenerateWayPointSecquenceTest() {
            Graph net = new Graph();
            List<Vertex> vertexs = new List<Vertex>();

            Vertex[,,] vertexList = new Vertex[10, 10, 10];
            for (int i = 0; i < 10; ++i) {
                for (int j = 0; j < 10; ++j) {
                    for (int k = 0; k < 10; ++k) {
                        vertexList[i, j, k] = new Vertex(i, j, k);
                        try {
                            vertexList[i, j, k].AddContract(vertexList[i - 1, j - 1, k - 1]);
                            vertexList[i - 1, j - 1, k - 1].AddContract(vertexList[i, j, k]);
                        } catch (IndexOutOfRangeException) {
                            ;
                        }
                        try {
                            vertexList[i, j, k].AddContract(vertexList[i, j - 1, k - 1]);
                            vertexList[i, j - 1, k - 1].AddContract(vertexList[i, j, k]);
                        }
                        catch (IndexOutOfRangeException) {
                            ;
                        }
                        try {
                            vertexList[i, j, k].AddContract(vertexList[i - 1, j, k - 1]);
                            vertexList[i - 1, j, k - 1].AddContract(vertexList[i, j, k]);
                        }
                        catch (IndexOutOfRangeException) {
                            ;
                        }
                        try {
                            vertexList[i, j, k].AddContract(vertexList[i - 1, j - 1, k]);
                            vertexList[i - 1, j - 1, k].AddContract(vertexList[i, j, k]);
                        }
                        catch (IndexOutOfRangeException) {
                            ;
                        }
                        net.Add(vertexList[i, j, k]);
                    }
                }
            }

            Vertex start = vertexList[0, 0, 0];
            Vertex end = vertexList[9, 9, 9];
            Core core = new Core();

            List<Vertex> wayPoints = core.GenerateWayPointSecquence(net, start, end);

            Assert.AreEqual(9, wayPoints.Count);
            for (int i = 0; i < 9; ++i) {
                Assert.AreEqual(wayPoints[i], vertexList[i, i, i]);
            }
        }
    }
}