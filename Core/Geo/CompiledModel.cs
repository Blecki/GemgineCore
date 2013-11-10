using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class CompiledModel: IDisposable
    {
        public VertexBuffer verticies;
        public IndexBuffer indicies;
        public int primitiveCount;

        public void Dispose()
        {
            verticies.Dispose();
            indicies.Dispose();
        }

        public static CompiledModel CompileModel(Mesh model, GraphicsDevice device)
        {
            CompiledModel result = new CompiledModel();
            result.indicies = new IndexBuffer(device, typeof(Int16), model.indicies.Length, BufferUsage.WriteOnly);
            result.indicies.SetData(model.indicies);
            result.verticies = new VertexBuffer(device, typeof(VertexPositionNormalTexture), model.verticies.Length, BufferUsage.WriteOnly);
            result.verticies.SetData(model.verticies);

            result.primitiveCount = model.indicies.Length / 3;

            return result;
        }

    }
}
