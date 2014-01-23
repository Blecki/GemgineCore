using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public interface IScreen
    {
        Input Input { get; set; }
        Main Main { get; set; }
        void Begin();
        void End();
        void Update(float elapsedSeconds);
        void Draw(float elapsedSeconds);
    }
}
