using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroid.Effects
{
    public class ThrustEffect : Effect
    {
        public ThrustEffect(Color playerColor, Point start, Point end, int lifetime) :
            base(new Polyline
            {
                Effect = new FX(playerColor).Glow,
                Stroke = new BrushConverter().ConvertFrom("#6FFFFFFF") as Brush,
                StrokeThickness = 5,
                Points = new PointCollection() { start, end }
            }, lifetime)
        { }

        public override bool Display()
        {
            if (Lifetime <= 0)
                return false;
            else
            {
                Lifetime--;
                return true;
            }
        }
    }
}