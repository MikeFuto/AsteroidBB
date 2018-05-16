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
    class ExplodeEffect : Effect
    {
        public double Speed { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public ExplodeEffect(Point center, Point trajectory, int lifetime, double speed, Polyline line) : base(line, lifetime)
        {
            Speed = speed;
            OffsetX = (trajectory.X - center.X) * Speed;
            OffsetY = (trajectory.Y - center.Y) * Speed;
        }

        public ExplodeEffect(Color c, double offsetX, double offsetY, int lifetime, int speed, Point p1, Point p2) : base(p1,p2,lifetime,c)
        {
            OffsetX = offsetX;
            OffsetY = OffsetY;
            Speed = speed;
        }

        public override bool Display()
        {
            if (Lifetime <= 0)
                return false;
            var translation = new TranslateTransform(OffsetX, OffsetY);
            Polyline pl = (Polyline)Geometry;
            PointCollection p = new PointCollection();
            foreach (Point vert in pl.Points)
            {
                p.Add(translation.Transform(vert));
            }
            pl.Points = p;
            Geometry = pl;
            Lifetime--;
            return true;
        }

    }

}
