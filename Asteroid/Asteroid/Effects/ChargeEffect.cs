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
    class ChargeEffect : Effect
    {
        private Point Center;
        private double OffsetX { get; set; }
        private double OffsetY { get; set; }

        public ChargeEffect(Point center, Brush stroke, Brush fill, Color c, int lifetime, double offsetX, double offsetY) : base(lifetime)
        {
            Center = center;
            OffsetX = offsetX;
            OffsetY = offsetY;

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

            Transform translate = new TranslateTransform(OffsetX, OffsetY);
            Polyline pl = (Polyline)Geometry;
            PointCollection p = new PointCollection();
            foreach (Point vert in pl.Points)
                p.Add(translate.Transform(vert));
            pl.Points = p;

            Transform scale = new ScaleTransform(.998, .998, Center.X, Center.Y);
            p = new PointCollection();
            foreach (Point vert in pl.Points)
                p.Add(scale.Transform(vert));
            pl.Points = p;

            OffsetX -= .002;

            Lifetime--;
            return true;
        }
    }
}
