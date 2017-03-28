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

    public class D3DObject {

        private List<Vertex> vertexs = new List<Vertex>();
        private List<Face> faces = new List<Face>();

        public D3DObject(List<Vertex> vex, List<Face> faces) {
            this.vertexs = vex;
            this.faces = faces;
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

        // 3ds chunk IDs
        public class Chunks {
            static public readonly UInt16 Main = 0x4d4d;
            static public readonly UInt16 Version = 0x0002;
            static public readonly UInt16 Object = 0x4000;
            static public readonly UInt16 Editor = 0x3d3d;
            static public readonly UInt16 Mesh = 0x4100;
            static public readonly UInt16 VertexList = 0x4110;
            static public readonly UInt16 Face = 0x4120;

        }

        private long findChunkPosition(BinaryReader br, long pos, UInt16 chunks) {

            var curretPos = br.BaseStream.Position;
            br.BaseStream.Seek(pos, SeekOrigin.Begin);
            long targetPos = 0;

            UInt16 chunkID = br.ReadUInt16();
            UInt32 chunkSize = br.ReadUInt32();

            while (chunkID != chunks) {
                br.BaseStream.Seek(chunkSize - 6, SeekOrigin.Current);
                chunkID = br.ReadUInt16();
                chunkSize = br.ReadUInt32();
            }
            targetPos = br.BaseStream.Position - 6;
            br.BaseStream.Seek(curretPos, SeekOrigin.Begin);

            return targetPos;
        }

        private List<Vertex> ReadVertexs(BinaryReader binaryFile) {
            UInt16 vertexsCount = binaryFile.ReadUInt16();

            List<Vertex> vertexs = new List<Vertex>();
            for (UInt16 i = 0; i < vertexsCount; ++i) {
                float x = (float)binaryFile.ReadInt32();
                float y = (float)binaryFile.ReadInt32();
                float z = (float)binaryFile.ReadInt32();
                vertexs.Add(new Vertex(x, y, z));
            }
            return vertexs;
        }

        public List<D3DObject> Read(string filename) {
            FileStream file = new FileStream(filename, FileMode.Open);
            BinaryReader binaryFile = new BinaryReader(file);

            // read the first chunk, must be a main chunk
            UInt16 root = binaryFile.ReadUInt16();
            if (root != Chunks.Main) {
                throw new FileFormatException("not a 3ds file");
            }
            UInt32 fileSize = binaryFile.ReadUInt32();

            // caution: for some reason it may throw EndOfStream or others IO exceptions
            // it's batter to pay more attention on error check
            long pos = findChunkPosition(binaryFile, binaryFile.BaseStream.Position, Chunks.Editor);
            // chunk's header always has a id block and a next pointer block
            // it occupy 2 and 4 bytes, 6 bytes in total.
            // so the first sub chunk's header start after 6 bytes of current file position.
            binaryFile.ReadUInt16();
            UInt32 editorSize = binaryFile.ReadUInt32();
            long editorEndPos = pos + editorSize;
            pos += 6;

            List<D3DObject> objs = new List<D3DObject>();
            long objectEntryPos = pos;
            while (objectEntryPos < editorEndPos) {
                pos = objectEntryPos;
	            pos = findChunkPosition(binaryFile, pos, Chunks.Object);

                objectEntryPos = pos;
	            // into object's sub chunks
	            binaryFile.BaseStream.Seek(pos + 6, SeekOrigin.Begin);
	
	            // first element is object's name, which is a c-style string
	            List<byte> nameByte = new List<byte>();
	            byte c = binaryFile.ReadByte();
	            while (c != 0) {
	                nameByte.Add(c);
	                c = binaryFile.ReadByte();
	            }
                string name = Encoding.UTF8.GetString(nameByte.ToArray());


	            pos = findChunkPosition(binaryFile, binaryFile.BaseStream.Position, Chunks.Mesh);


	            pos = findChunkPosition(binaryFile, pos + 6, Chunks.VertexList);
	            binaryFile.BaseStream.Seek(pos + 6, SeekOrigin.Begin);

                // read in vertexs
                List<Vertex> vertexs = ReadVertexs(binaryFile);

                pos = findChunkPosition(binaryFile, binaryFile.BaseStream.Position, Chunks.Face);
	            binaryFile.BaseStream.Seek(pos + 6, SeekOrigin.Begin);
	
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
	
	            D3DObject obj = new D3DObject(vertexs, faces);
                objs.Add(obj);
            }

            // clear up
            binaryFile.Close();
            file.Close();
            return objs;
        }

    }
}
