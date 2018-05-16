using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;

namespace Asteroid.Effects
{
    public class FX
    {
        public DropShadowEffect Glow { get; set; }

        public FX(Color c)
        {
            Glow = new DropShadowEffect()
            {
                Color = c,
                ShadowDepth = 0,
                Direction = 0,
                BlurRadius = 10
            };
        }
    }
}
