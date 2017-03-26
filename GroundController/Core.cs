using System;
using System.Collections.Generic;
using System.IO;
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

    public class Face {
        public uint first;
        public uint second;
        public uint third;

        public List<bool> visibility = new List<bool>();

        public Face(uint first, uint second, uint third) {
            this.first = first;
            this.second = second;
            this.third = third;
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

    public class InputAdapter {

        public InputAdapter() { }

        // from .3ds file format

        public const UInt16 MainChunk = 0x4d4d;
        public const UInt16 Version = 0x0002;
        public const UInt16 Editor = 0x3d3d;
        public const UInt16 ObjectChunk = 0x4000;
        public const UInt16 Mash = 0x4100;
        public const UInt16 VertexList = 0x4110;
        public const UInt16 FacesDescription = 0x4120;

        public void Open(string filename) {
            FileStream file = new FileStream(filename, FileMode.Open);
            BinaryReader binaryFile = new BinaryReader(file);

            // read the first chunk, must be a main chunk
            UInt16 root = binaryFile.ReadUInt16();
            if (root != MainChunk) {
                throw new FileFormatException("not a 3ds file");
            }
            UInt32 fileSize = binaryFile.ReadUInt32();

            // relocate to editor chunk
            UInt16 chunkID = binaryFile.ReadUInt16();
            UInt32 chunkSize = binaryFile.ReadUInt32();

            while (chunkID != Editor) {
                binaryFile.BaseStream.Seek(chunkSize - 6, SeekOrigin.Current);
                chunkID = binaryFile.ReadUInt16();
                chunkSize = binaryFile.ReadUInt32();
            }

            // into editor's sub chunks
            chunkID = binaryFile.ReadUInt16();
            chunkSize = binaryFile.ReadUInt32();

            // relocate to object chunk
            while (chunkID != ObjectChunk) {
                binaryFile.BaseStream.Seek(chunkSize - 6, SeekOrigin.Current);
                chunkID = binaryFile.ReadUInt16();
                chunkSize = binaryFile.ReadUInt32();
            }

            // into object's sub chunks
            // first element is object's name, which is a c-style string
            List<byte> nameByte = new List<byte>();
            byte c = binaryFile.ReadByte();
            while (c != 0) {
                nameByte.Add(c);
                c = binaryFile.ReadByte();
            }
            string name = Encoding.UTF8.GetString(nameByte.ToArray());

            // relocate to mash
            chunkID = binaryFile.ReadUInt16();
            chunkSize = binaryFile.ReadUInt32();
            while (chunkID != Mash) {
                binaryFile.BaseStream.Seek(chunkSize - 6, SeekOrigin.Current);
                chunkID = binaryFile.ReadUInt16();
                chunkSize = binaryFile.ReadUInt32();
            }

            // now process all mash information
            // get all vertexs first

            chunkID = binaryFile.ReadUInt16();
            chunkSize = binaryFile.ReadUInt32();
            while (chunkID != VertexList) {
                binaryFile.BaseStream.Seek(chunkSize - 6, SeekOrigin.Current);
                chunkID = binaryFile.ReadUInt16();
                chunkSize = binaryFile.ReadUInt32();
            }

            // read in vertexs
            UInt16 vertexsCount = binaryFile.ReadUInt16();

            List<Vertex> vertexs = new List<Vertex>();
            for (UInt16 i = 0; i < vertexsCount; ++i) {
                float x = (float)binaryFile.ReadInt32();
                float y = (float)binaryFile.ReadInt32();
                float z = (float)binaryFile.ReadInt32();
                vertexs.Add(new Vertex(x, y, z));
            }

            // relocate to face list
            chunkID = binaryFile.ReadUInt16();
            chunkSize = binaryFile.ReadUInt32();
            while (chunkID != FacesDescription) {
                binaryFile.BaseStream.Seek(chunkSize - 6, SeekOrigin.Current);
                chunkID = binaryFile.ReadUInt16();
                chunkSize = binaryFile.ReadUInt32();
            }

            // read in faces
            List<Face> faces = new List<Face>();
            UInt16 polygonsCount = binaryFile.ReadUInt16();
            for (UInt16 i = 0; i < polygonsCount; ++i) {
                UInt16 first = binaryFile.ReadUInt16();
                UInt16 second = binaryFile.ReadUInt16();
                UInt16 third = binaryFile.ReadUInt16();
                UInt16 info = binaryFile.ReadUInt16();

                Face f = new Face(first, second, third);
                f.visibility.Add((info & (UInt16)0x0001) != 0 ? true : false); // set visibility of ac, first -> third
                f.visibility.Add((info & (UInt16)0x0002) != 0 ? true : false); // set visibility of cb, third -> second
                f.visibility.Add((info & (UInt16)0x0004) != 0 ? true : false); // set visibility of ba, second -> first
            }

            // clear up
            binaryFile.Close();
            file.Close();
        }
    }
}
