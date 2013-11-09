using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace RealtimeCSG
{
	public sealed class CSGNode
	{
		public CSGNode(CSGNodeType branchOperator) 
        { 
            this.NodeType = branchOperator; 
            this.Left = null; this.Right = null;
            this.Translation = new Vector3(0, 0, 0);
            this.LocalTranslation = new Vector3(0, 0, 0);
        }

		public CSGNode(CSGNodeType branchOperator, CSGNode left, CSGNode right) 
        { 
            this.NodeType = branchOperator; 
            this.Left = left; 
            this.Right = right;
            this.Translation = new Vector3(0, 0, 0);
            this.LocalTranslation = new Vector3(0, 0, 0);
            left.Parent = this;
            right.Parent = this;
        }

		public CSGNode(IEnumerable<Plane> planes) 
        { 
            this.NodeType = CSGNodeType.Brush;
            Generator = new StaticBrushGenerator(planes.ToArray());
            this.Translation = new Vector3(0, 0, 0);
            this.LocalTranslation = new Vector3(0, 0, 0);
        }

        public CSGNode(BrushGenerator generator)
        {
            this.NodeType = CSGNodeType.Brush;
            Generator = generator;
            this.Translation = new Vector3(0, 0, 0);
            this.LocalTranslation = new Vector3(0, 0, 0);
        }

        public void DirtyTree()
        {
            cachedMesh = null;
            cachedRenderMesh = null;

            if (Left != null) Left.DirtyTree();
            if (Right != null) Right.DirtyTree();
        }

        public void DirtyUp()
        {
            cachedMesh = null;
            cachedRenderMesh = null;
            if (Parent != null) Parent.DirtyUp();
        }

        public void DirtyBoth()
        {
            DirtyTree();
            if (Parent != null) Parent.DirtyUp();
        }

		public readonly AABB Bounds = new AABB();
		public readonly CSGNodeType NodeType;

		public CSGNode Left;
		public CSGNode Right;
		public CSGNode Parent;

        public Vector3 LocalTranslation = new Vector3();
        public Vector3 Translation = new Vector3();
		//public Plane[] Planes;
        public BrushGenerator Generator;
        public Microsoft.Xna.Framework.Matrix WorldTransformation = Microsoft.Xna.Framework.Matrix.Identity;
        public Gem.Euler Transformation = new Gem.Euler();
        //public Microsoft.Xna.Framework.Matrix Transformation = Microsoft.Xna.Framework.Matrix.Identity;
        public Microsoft.Xna.Framework.Vector3 Color = Microsoft.Xna.Framework.Vector3.One;
        public Gem.Geo.TextureProjection TextureProjection = null;
        public Microsoft.Xna.Framework.Graphics.Texture2D Texture = null;
        public Gem.Mesh cachedRenderMesh = null;

        public CSGMesh cachedMesh = null;
	}
}
