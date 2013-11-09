using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
	public class MatrixStack
	{
        private Stack<Matrix> _MatrixStack = new Stack<Matrix>();

        public MatrixStack()
        {
            _MatrixStack.Push(Matrix.Identity);
        }

        public void PushMatrix(Matrix M)
        {
            _MatrixStack.Push(M * _MatrixStack.Peek());
        }

        public void PushCleanMatrix(Matrix M)
        {
            _MatrixStack.Push(M);
        }

        public void ReplaceTop(Matrix M)
        {
            _MatrixStack.Pop();
            _MatrixStack.Push(M);
        }

        public void PopMatrix()
        {
            if (_MatrixStack.Count <= 1) throw new InvalidOperationException();
            _MatrixStack.Pop();
        }

        public Matrix TopMatrix { get { return _MatrixStack.Peek(); } }

        public void Clean()
        {
            while (_MatrixStack.Count > 1) _MatrixStack.Pop();
        }

    }
}
