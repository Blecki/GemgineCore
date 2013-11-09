using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class OctCell
    {
        public BoundingBox box;
        public OctCell[] children;
        public List<OctNode> contents;

        public bool Leaf { get { return children == null; } }

        public OctCell(BoundingBox box) { this.box = box; }
    }

    public class OctTree
    {
        public static void VisitTree(OctCell root, BoundingBox box, Action<OctCell> callback)
        {
            if (box.Intersects(root.box))
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.children) VisitTree(child, box, callback);
            }
        }

        public static void VisitTree(OctCell root, BoundingSphere bounds, Action<OctCell> callback)
        {
            if (bounds.Intersects(root.box))
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.children) VisitTree(child, bounds, callback);
            }
        }

        public static void VisitTree(OctCell root, BoundingFrustum frustum, Action<OctCell> callback)
        {
            if (frustum.Intersects(root.box))
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.children) VisitTree(child, frustum, callback);
            }
        }

        public static void VisitTree(OctCell root, Ray ray, Action<OctCell> callback)
        {
            if (ray.Intersects(root.box).HasValue)
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.children) VisitTree(child, ray, callback);
            }
        }

        private static BoundingBox makeBox(float x, float y, float z, float w, float h, float d)
        {
            return new BoundingBox(new Vector3(x,y,z), new Vector3(x + w, y + h, z + d));
        }

        private static OctCell[] splitCell(OctCell root)
        {
            var into = new OctCell[8];
            var dims = (root.box.Max - root.box.Min) / 2;
            var min = root.box.Min;
            into[0] = new OctCell(makeBox(min.X, min.Y, min.Z, dims.X, dims.Y, dims.Z));
            into[1] = new OctCell(makeBox(min.X + dims.X, min.Y, min.Z, dims.X, dims.Y, dims.Z));
            into[2] = new OctCell(makeBox(min.X, min.Y + dims.Y, min.Z, dims.X, dims.Y, dims.Z));
            into[3] = new OctCell(makeBox(min.X + dims.X, min.Y + dims.Y, min.Z, dims.X, dims.Y, dims.Z));

            into[4] = new OctCell(makeBox(min.X, min.Y, min.Z + dims.Z, dims.X, dims.Y, dims.Z));
            into[5] = new OctCell(makeBox(min.X + dims.X, min.Y, min.Z + dims.Z, dims.X, dims.Y, dims.Z));
            into[6] = new OctCell(makeBox(min.X, min.Y + dims.Y, min.Z + dims.Z, dims.X, dims.Y, dims.Z));
            into[7] = new OctCell(makeBox(min.X + dims.X, min.Y + dims.Y, min.Z + dims.Z, dims.X, dims.Y, dims.Z));

            return into;
        }

        public static void BuildTree(OctCell root, BoundingBox box, Action<OctCell> forLeaves, float leafSize)
        {
            if (box.Intersects(root.box))
            {
                if ((root.box.Max.X - root.box.Min.X) <= leafSize)
                {
                    if (root.contents == null) root.contents = new List<OctNode>();
                    forLeaves(root);
                }
                else
                {
                    if (root.children == null) root.children = splitCell(root);
                    foreach (var child in root.children) BuildTree(child, box, forLeaves, leafSize);
                }
            }
        }

        public static void BuildTree(OctCell root, BoundingSphere bounds, Action<OctCell> forLeaves, float leafSize)
        {
            if (bounds.Intersects(root.box))
            {
                if ((root.box.Max.X - root.box.Min.X) <= leafSize)
                {
                    if (root.contents == null) root.contents = new List<OctNode>();
                    forLeaves(root);
                }
                else
                {
                    if (root.children == null) root.children = splitCell(root);
                    foreach (var child in root.children) BuildTree(child, bounds, forLeaves, leafSize);
                }
            }
        }

        public static void InsertNode(OctCell root, OctNode node, float leafSize)
        {
            BuildTree(root, node.bounds, (cell) => { cell.contents.Add(node); }, leafSize);
        }

        public static void RemoveNode(OctCell root, OctNode node)
        {
            VisitTree(root, node.bounds, (cell) => { if (cell.contents != null) cell.contents.Remove(node); });
        }


    }
}
