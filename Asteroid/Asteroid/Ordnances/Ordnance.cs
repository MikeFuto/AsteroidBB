using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroid.Ordnances
{
    public abstract class Ordnance
    {
        public Point Center { get; set; }
        public Point Trajectory { get; set; }
        public Polyline Geometry { get; set; }
        public Color OrdnanceColor { get; set; }
        public int Lifetime { get; set; }
        public int GridSquare { get; set; }

        public Ordnance(Point center, Point trajectory, int lifetime, Color color)
        {
            Center = center;
            Trajectory = trajectory;
            Lifetime = lifetime;
            OrdnanceColor = color;
        }

        public abstract bool UsesPointCollision();
        public abstract bool Move();
        public abstract Point CollisionPoint();
    }
}
