using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Asteroid.Effects;
using Asteroid.Ordnances;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace Asteroid.Objects
{
    class Wall : GameObject
    {
        public Wall(Brush stroke, Color glow, int thickness, Point center, PointCollection points, int pointValue, double difficulty, List<Ordnance> ordnances, List<Effect> effects) : base(center, pointValue, difficulty, ordnances, effects)
        {
            Geometry = new Polyline
            {
                Points = points,
                Stroke = stroke,
                Fill = Brushes.Transparent,
                Effect = new FX(glow).Glow,
                StrokeThickness = thickness,
            };
        }

        public override void Destroy()
        {
            //undestroyable
        }

        public override void Move()
        {
            //non-moving
        }

        public override void TeleportPoints(Point teleportOffset)
        {
            //doesn't teleport
        }
    }
}
