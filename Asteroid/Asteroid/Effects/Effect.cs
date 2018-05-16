using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroid.Effects
{
    public abstract class Effect
    {
        public Shape Geometry { get; set; }
        public int Lifetime { get; set; }

        public Effect(int lifetime)
        {
            Lifetime = lifetime;
        }

        public Effect(Polyline geometry, int lifetime)
        {
            Geometry = geometry;
            Lifetime = lifetime;
        }

        public Effect(Point p1, Point p2, int lifetime, Color c)
        {
            Geometry = new Polyline
            {
                Points = new PointCollection() { p1, p2 },
                Stroke = Brushes.White,
                StrokeThickness = 3,
                Effect = new FX(c).Glow,            
            };
        }

        public abstract bool Display();

    }
}
