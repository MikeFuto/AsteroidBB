using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Asteroid.Effects;

namespace Asteroid.Ordnances
{
    public class Bullet : Ordnance
    {
        public double Speed { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public bool AsteroidCollision { get; set; }
        public bool PlayerShipCollision { get; set; }
        public bool SaucerCollision { get; set; }

        public Bullet(Color color, Point center, Point trajectory, double speed, int lifetime) : base(center, trajectory, lifetime, color)
        {
            Speed = speed;
            Geometry = GenerateGeometry(color, center);
            OffsetX = (Trajectory.X - Center.X) * Speed;
            OffsetY = (Trajectory.Y - Center.Y) * Speed;
        }

        private Polyline GenerateGeometry(Color color, Point center)
        {
            return new Polyline
            {
                Effect = new FX(color).Glow,
                Stroke = Brushes.White,
                Fill = Brushes.White,
                StrokeThickness = 5,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Points = new PointCollection()
                {
                    new Point(Center.X, Center.Y),
                    new Point(Center.X - .5, Center.Y - .5),
                }
            };
        }

        public override bool UsesPointCollision()
        {
            return true;
        }

        public override bool Move()
        {
            if (Lifetime <= 0)
                return false;

            var translation = new TranslateTransform(OffsetX, OffsetY);
            PointCollection p = new PointCollection();
            foreach(Point vert in Geometry.Points)
            {
                p.Add(translation.Transform(vert));
            }
            Geometry.Points = p;
            Lifetime--;
            return true;
        }

        public override Point CollisionPoint()
        {
            return Geometry.Points[0];
        }
    }
}
