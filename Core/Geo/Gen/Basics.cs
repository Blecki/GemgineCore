using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Gen
    {
        public static void CopyIndicies(short[] into, int at, short[] source)
        {
            for (int i = 0; i < source.Length; ++i)
                into[at + i] = source[i];
        }

        public static Mesh Copy(Mesh mesh)
        {
            var result = new Mesh();

            result.verticies = new VertexPositionNormalTexture[mesh.verticies.Length];
            for (int i = 0; i < mesh.verticies.Length; ++i) result.verticies[i] = mesh.verticies[i];
            result.indicies = new short[mesh.indicies.Length];
            CopyIndicies(result.indicies, 0, mesh.indicies);
            return result;
        }

        public static void Transform(Mesh mesh, Matrix m, int start, int count)
        {
            if (start < 0) start = mesh.verticies.Length - start;
            for (int i = start; i < start + count; ++i)
                mesh.verticies[i].Position = Vector3.Transform(mesh.verticies[i].Position, m);

            Vector3 scale;
            Vector3 trans;
            Quaternion rot;
            m.Decompose(out scale, out rot, out trans);
            for (int i = start; i < start + count; ++i)
                mesh.verticies[i].Normal = Vector3.Transform(mesh.verticies[i].Normal, rot);
        }

        public static Mesh TransformCopy(Mesh mesh, Matrix m, int start, int count)
        {
            var result = Copy(mesh);
            Transform(result, m, start, count);
            return result;
        }

        public static void Transform(Mesh mesh, Matrix m)
        {
            Transform(mesh, m, 0, mesh.verticies.Length);
        }

        public static Mesh TransformCopy(Mesh mesh, Matrix m)
        {
            return TransformCopy(mesh, m, 0, mesh.verticies.Length);
        }

        public static void Morph(Mesh mesh, Func<Vector3, Vector3> func)
        {
            for (int i = 0; i < mesh.verticies.Length; ++i)
                mesh.verticies[i].Position = func(mesh.verticies[i].Position);
        }

        public static Mesh MorphCopy(Mesh mesh, Func<Vector3, Vector3> func)
        {
            var result = Copy(mesh);
            Morph(result, func);
            return result;
        }

        public static Vector3 CalculateNormal(Mesh part, int a, int b, int c)
        {
            return Vector3.Normalize(Vector3.Cross(part.GetVertex(b).Position - part.GetVertex(a).Position,
                     part.GetVertex(c).Position - part.GetVertex(a).Position));
        }

        public static VertexPositionNormalTexture[] GetVerticies(Mesh mesh, int startIndex, int Length)
        {
            var r = new VertexPositionNormalTexture[Length];
            for (int i = 0; i < Length; ++i) r[i] = mesh.verticies[i + startIndex];
            return r;
        }

        public static BoundingBox CalculateBoundingBox(Mesh mesh)
        {
            Vector3 minimum = mesh.verticies[0].Position;
            Vector3 maximum = mesh.verticies[0].Position;
            foreach (var vert in mesh.verticies)
            {
                if (vert.Position.X < minimum.X) minimum.X = vert.Position.X;
                if (vert.Position.Y < minimum.Y) minimum.Y = vert.Position.Y;
                if (vert.Position.Z < minimum.Z) minimum.Z = vert.Position.Z;
                if (vert.Position.X > maximum.X) maximum.X = vert.Position.X;
                if (vert.Position.Y > maximum.Y) maximum.Y = vert.Position.Y;
                if (vert.Position.Z > maximum.Z) maximum.Z = vert.Position.Z;
            }

            return new BoundingBox(minimum, maximum);
        }

        public static Mesh Merge(params Mesh[] parts)
        {
            var result = new Mesh();

            result.verticies = new VertexPositionNormalTexture[parts.Sum((p) => p.verticies.Length)];
            result.indicies = new short[parts.Sum((p) => p.indicies.Length)];

            int vCount = 0;
            int iCount = 0;
            foreach (var part in parts)
            {
                for (int i = 0; i < part.verticies.Length; ++i) result.verticies[i + vCount] = part.verticies[i];
                for (int i = 0; i < part.indicies.Length; ++i) result.indicies[i + iCount] = (short)(part.indicies[i] + vCount);
                vCount += part.verticies.Length;
                iCount += part.indicies.Length;
            }

            return result;
        }

    }
}