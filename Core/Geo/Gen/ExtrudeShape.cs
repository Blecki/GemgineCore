//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;

//namespace GeometryGeneration
//{

//    public partial class MeshPart
//    {
//        public static void CopyIndicies(short[] into, int at, short[] source)
//        {
//            for (int i = 0; i < source.Length; ++i)
//                into[at + i] = source[i];
//        }

//        public static MeshPart ExtrudeShape(Vertex[] shape, Matrix transform)
//        {
//            var result = new MeshPart();
//            result.verticies = new Vertex[shape.Length * 2];
//            for (int i = 0; i < shape.Length; ++i)
//            {
//                result.verticies[i] = shape[i];
//                result.verticies[i + shape.Length] = shape[i];
//                result.verticies[i + shape.Length].Position = Vector3.Transform(shape[i].Position, transform);
//            }

//            result.indicies = new short[shape.Length * 6];

//            for (short i = 0; i < shape.Length; ++i)
//            {
//                var next = (short)(i + 1);
//                if (next >= shape.Length) next = 0;
//                CopyIndicies(result.indicies, i * 6,
//                    new short[] { i, next, (short)(i + shape.Length), next, (short)(next + shape.Length), (short)(i + shape.Length) });
//            }

//            return result;                
//        }

//        public MeshPart ExtrudeShape(int loopStart, int loopCount, Matrix transform)
//        {
//            if (loopStart < 0) loopStart = verticies.Length + loopStart;
//            var verts = new Vertex[loopCount + verticies.Length];
//            for (int i = 0; i < verticies.Length; ++i) verts[i] = verticies[i];
//            for (int i = 0; i < loopCount; ++i)
//            {
//                verts[verticies.Length + i] = verticies[loopStart + i];
//                verts[verticies.Length + i].Position = Vector3.Transform(verticies[loopStart + i].Position, transform);
//            }

//            var indi = new short[indicies.Length + loopCount * 6];
//            CopyIndicies(indi, 0, indicies);
//            for (short i = 0; i < loopCount; ++i)
//            {
//                var next = (short)(i + 1);
//                if (next >= loopCount) next = 0;
//                CopyIndicies(indi, indicies.Length + (i * 6),
//                    new short[] { (short)(loopStart + i), (short)(loopStart + next), (short)(verticies.Length + i),
//                        (short)(loopStart + next), (short)(verticies.Length + next), (short)(verticies.Length + i) });
//            }

//            this.verticies = verts;
//            this.indicies = indi;

//            return this;
//        }

//        public MeshPart ExtrudeN(int loopStart, int loopCount, Matrix transform, int times)
//        {
//            for (int i = 0; i < times; ++i)
//            {
//                ExtrudeShape(loopStart, loopCount, transform);
//                loopStart = -loopCount;
//            }
//            return this;
//        }
//    }
//}