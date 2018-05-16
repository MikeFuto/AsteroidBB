using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Asteroid.Effects;
using Asteroid.Ordnances;

namespace Asteroid.Objects
{
    public abstract class GameObject
    {
        public Point Center { get; set; }
        public Point Trajectory { get; set; }
        public bool RemoveFromGame { get; set; }
        public int GridSquare { get; set; }
        public List<Ordnance> Ordnances { get; set; }
        public List<Effect> Effects { get; set; }
        public Polyline Geometry { get; set; }
        public bool IsDestroyed { get; set; }
        public int PointValue { get; set; }
        public double DifficultyModifier { get; set; }
        public Color DestructionColor { get; set; }
        public Brush DestructionBrush { get; set; }

        public GameObject(Point center, int pointValue, double difficulty, List<Ordnance> ordnances, List<Effect> effects)
        {
            Center = center;
            DifficultyModifier = difficulty;
            PointValue = pointValue;
            Ordnances = ordnances;
            Effects = effects;
        }

        public abstract void Move();
        public abstract void TeleportPoints(Point teleportOffset);
        public abstract void Destroy();
        public int GetPointValue()
        {
            return PointValue;
        }
    }
}
