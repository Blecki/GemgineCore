using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem.Render.OctTree
{
    public class OctTree
    {
        public static void VisitTree(OctCell root, BoundingBox box, Action<OctCell> callback)
        {
            if (box.Intersects(root.Bounds))
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.Children) VisitTree(child, box, callback);
            }
        }

        public static void VisitTree(OctCell root, BoundingSphere bounds, Action<OctCell> callback)
        {
            if (bounds.Intersects(root.Bounds))
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.Children) VisitTree(child, bounds, callback);
            }
        }

        public static void VisitTree(OctCell root, BoundingFrustum frustum, Action<OctCell> callback)
        {
            if (frustum.Intersects(root.Bounds))
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.Children) VisitTree(child, frustum, callback);
            }
        }

        public static void VisitTree(OctCell root, Ray ray, Action<OctCell> callback)
        {
            if (ray.Intersects(root.Bounds).HasValue)
            {
                if (root.Leaf) callback(root);
                else foreach (var child in root.Children) VisitTree(child, ray, callback);
            }
        }

        private static BoundingBox makeBox(float x, float y, float z, float w, float h, float d)
        {
            return new BoundingBox(new Vector3(x,y,z), new Vector3(x + w, y + h, z + d));
        }

        private static OctCell[] splitCell(OctCell root)
        {
            var into = new OctCell[8];
            var dims = (root.Bounds.Max - root.Bounds.Min) / 2;
            var min = root.Bounds.Min;
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
            if (box.Intersects(root.Bounds))
            {
                if ((root.Bounds.Max.X - root.Bounds.Min.X) <= leafSize)
                {
                    if (root.Contents == null) root.Contents = new List<IOctNode>();
                    forLeaves(root);
                }
                else
                {
                    if (root.Children == null) root.Children = splitCell(root);
                    foreach (var child in root.Children) BuildTree(child, box, forLeaves, leafSize);
                }
            }
        }

        public static void BuildTree(OctCell root, BoundingSphere bounds, Action<OctCell> forLeaves, float leafSize)
        {
            if (bounds.Intersects(root.Bounds))
            {
                if ((root.Bounds.Max.X - root.Bounds.Min.X) <= leafSize)
                {
                    if (root.Contents == null) root.Contents = new List<IOctNode>();
                    forLeaves(root);
                }
                else
                {
                    if (root.Children == null) root.Children = splitCell(root);
                    foreach (var child in root.Children) BuildTree(child, bounds, forLeaves, leafSize);
                }
            }
        }

        public static void InsertNode(OctCell root, IOctNode node, float leafSize)
        {
            BuildTree(root, node.Bounds, (cell) => { cell.Contents.Add(node); }, leafSize);
        }

        public static void RemoveNode(OctCell root, IOctNode node)
        {
            VisitTree(root, node.Bounds, (cell) => { if (cell.Contents != null) cell.Contents.Remove(node); });
        }


    }
}
