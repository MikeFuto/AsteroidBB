using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroid.Effects
{
    internal class WarpEffect : Effect
    {
        private Point Center;

        public WarpEffect(Point center, Brush stroke, Brush fill, Color c, int lifetime) : base(lifetime)
        {
            Center = center;
            double x = center.X, y = center.Y;
            Geometry = new Polyline
            {
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Stroke = stroke,
                Fill = fill,
                Effect = new FX(c).Glow,
                StrokeThickness = 2,
                Points = new PointCollection()
                {
                    new Point(x-20,y), new Point(x-15, y-10), new Point(x-10, y-15),
                    new Point(x,y-20), new Point(x+10, y-15), new Point(x+15, y-10),
                    new Point(x+20,y), new Point(x+15, y+10), new Point(x+10, y+15),
                    new Point(x,y+20), new Point(x-10, y+15), new Point(x-15, y+10),
                    new Point(x-20,y)
                }
            };
        }

        public override bool Display()
        {
            if (Lifetime <= 0)
                return false;
            Transform scale = new ScaleTransform(.91, .91, Center.X, Center.Y);
            Polyline pl = (Polyline)Geometry;
            PointCollection p = new PointCollection();
            foreach (Point vert in pl.Points)
                p.Add(scale.Transform(vert));
            pl.Points = p;
            Geometry = pl;
            Lifetime--;
            return true;
        }
    }
}