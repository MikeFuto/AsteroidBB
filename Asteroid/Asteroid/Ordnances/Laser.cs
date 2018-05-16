using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Asteroid.Effects;

namespace Asteroid.Ordnances
{
    public class Laser : Ordnance
    {
        public double Speed { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public bool AsteroidCollision { get; set; }
        public bool PlayerShipCollision { get; set; }
        public bool SaucerCollision { get; set; }

        public Laser(Color color, Point start, Point end, double speed, int lifetime) : base(start, end, lifetime, color)
        {
            Speed = speed;
            Geometry = GenerateGeometry(color, start);
            OffsetX = (Trajectory.X - Center.X) * Speed;
            OffsetY = (Trajectory.Y - Center.Y) * Speed;
        }

        private Polyline GenerateGeometry(Color color, Point center)
        {
            return new Polyline
            {
                Effect = new FX(color).Glow,
                Stroke = Brushes.White,
                Fill = Brushes.Transparent,
                StrokeThickness = 4,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Points = new PointCollection()
                {
                    new Point(Center.X, Center.Y),
                    new Point(Trajectory.X, Trajectory.Y),
                }
            };
        }

        public override bool Move()
        {
            if (Lifetime <= 0)
                return false;
            Lifetime--;
            return true;
        }

        public override bool UsesPointCollision()
        {
            return true;
        }

        public override Point CollisionPoint()
        {
            return Geometry.Points[1];
        }
    }
}
