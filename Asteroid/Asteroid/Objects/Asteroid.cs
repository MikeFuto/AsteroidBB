using Asteroid.Effects;
using Asteroid.Ordnances;
using Asteroid.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroid.Objects
{
    public class Asteroid : GameObject
    {
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double Rotation { get; set; }
        public MainWindow Parent { get; set; }
        public string Size { get; set; }
        public Brush Stroke { get; set; }
        public Brush Fill { get; set; }
        public Color Glow { get; set; }
        public int Thickness { get; set; }
        public int DestroyedTimer { get; set; }

        public Asteroid(MainWindow parent, Brush stroke, Brush fill, Color glow, int pointValue, double difficulty, int thickness, Point center, string size, int type, int randomSeed) : base(center, pointValue, difficulty, new List<Ordnance>(), new List<Effect>())
        {
            Parent = parent;
            Size = size;
            Stroke = stroke;
            Fill = fill;
            Glow = glow;
            Thickness = thickness;

            Random generator = new Random(randomSeed);
            Point trajectory = new Point(Center.X + 1, Center.Y - 1);
            trajectory = new RotateTransform(generator.Next(360), Center.X, Center.Y).Transform(trajectory);
            OffsetX = (trajectory.X - Center.X) * generator.Next(4, 10) / 5;
            OffsetY = (trajectory.Y - Center.Y) * generator.Next(4, 10) / 5;
            int negative = generator.Next(1,3);
            Rotation = generator.Next(0,2);
            if (negative == 1)
                Rotation *= -1;
            Rotation = Rotation / generator.Next(1, 5);
            Geometry = new Polyline();
            Geometry.Stroke = stroke;
            Geometry.StrokeThickness = thickness;
            Geometry.Fill = fill;
            Geometry.StrokeEndLineCap = PenLineCap.Round;
            Geometry.StrokeStartLineCap = PenLineCap.Round;
            Geometry.Effect = new FX(glow).Glow;
            Geometry.Points = new Plot(size + type, Center).Points;
        }

        public override void Move()
        {
            if(!IsDestroyed)
            {
                RotateTransform rotated = new RotateTransform(Rotation, Center.X, Center.Y);
                PointCollection p = new PointCollection();
                foreach (Point vert in Geometry.Points)
                    p.Add(rotated.Transform(vert));
                Geometry.Points = p;

                TranslateTransform translated = new TranslateTransform(OffsetX, OffsetY);
                p = new PointCollection();
                foreach (Point vert in Geometry.Points)
                    p.Add(translated.Transform(vert));
                Geometry.Points = p;
                Center = translated.Transform(Center);
            }

            Effects.RemoveAll(effect => { return !effect.Display() ? RemoveFromDisplay(effect) : false; });
            DestroyedTimeoutCooldown();

        }

        private void DestroyedTimeoutCooldown()
        {
            if (IsDestroyed)
            {
                if (DestroyedTimer <= 0)
                {
                    RemoveFromGame = true;
                }
                else
                    DestroyedTimer--;
            }
        }

        private Polyline generateExplosionGeometry(int i1, int i2)
        {
            return new Polyline()
            {
                Points = new PointCollection() { new Point(Geometry.Points[i1].X, Geometry.Points[i1].Y), new Point(Geometry.Points[i2].X, Geometry.Points[i2].Y) },
                Stroke = DestructionBrush,
                StrokeThickness = .5,
                Effect = new FX(DestructionColor).Glow
            };
        }

        public override void TeleportPoints(Point teleportOffset)
        {
            var teleport = new TranslateTransform(teleportOffset.X, teleportOffset.Y);
            PointCollection p = new PointCollection();
            foreach (Point vert in Geometry.Points)
                p.Add(teleport.Transform(vert));
            Geometry.Points = p;
            Center = teleport.Transform(Center);    
        }

        public void Explode()
        {
            Random seed = new Random();
            switch(Size)
            {
                case "ExtraLarge":
                    {
                        Asteroid m0 = new Asteroid(Parent, Stroke, Fill, Glow, 50, DifficultyModifier, Thickness - 1, new Point(Center.X + 20, Center.Y - 20), "Large", 0, seed.Next());
                        Asteroid m1 = new Asteroid(Parent, Stroke, Fill, Glow, 50, DifficultyModifier, Thickness - 1, new Point(Center.X - 20, Center.Y + 40), "Large", 1, seed.Next(48000) * seed.Next(48000));
                        Asteroid m2 = new Asteroid(Parent, Stroke, Fill, Glow, 50, DifficultyModifier, Thickness - 1, new Point(Center.X + 40, Center.Y + 10), "Large", 2, seed.Next(2000000000) / seed.Next(1, 2000000000));
                        Parent.Objects.Add(m0);
                        Parent.Objects.Add(m1);
                        Parent.Objects.Add(m2);
                        Parent.canvas.Children.Add(m0.Geometry);
                        Parent.canvas.Children.Add(m1.Geometry);
                        Parent.canvas.Children.Add(m2.Geometry);
                    }
                    break;
                case "Large":
                    {
                        Asteroid m0 = new Asteroid(Parent, Stroke, Fill, Glow, 100, DifficultyModifier, Thickness - 1, new Point(Center.X + 20, Center.Y - 20), "Medium", 0, seed.Next());
                        Asteroid m1 = new Asteroid(Parent, Stroke, Fill, Glow, 100, DifficultyModifier, Thickness - 1, new Point(Center.X - 20, Center.Y + 40), "Medium", 1, seed.Next(48000) * seed.Next(48000));
                        Asteroid m2 = new Asteroid(Parent, Stroke, Fill, Glow, 100, DifficultyModifier, Thickness - 1, new Point(Center.X + 40, Center.Y + 10), "Medium", 2, seed.Next(2000000000) / seed.Next(1, 2000000000));
                        Parent.Objects.Add(m0);
                        Parent.Objects.Add(m1);
                        Parent.Objects.Add(m2);
                        Parent.canvas.Children.Add(m0.Geometry);
                        Parent.canvas.Children.Add(m1.Geometry);
                        Parent.canvas.Children.Add(m2.Geometry);
                    }
                    break;
                case "Medium":
                    {
                        Asteroid s0 = new Asteroid(Parent, Stroke, Fill, Glow, 200, DifficultyModifier, Thickness - 1, new Point(Center.X + 20, Center.Y - 20), "Small", 0, seed.Next());
                        Asteroid s1 = new Asteroid(Parent, Stroke, Fill, Glow, 200, DifficultyModifier, Thickness - 1, new Point(Center.X - 20, Center.Y + 40), "Small", 1, seed.Next(48000) * seed.Next(48000));
                        Asteroid s2 = new Asteroid(Parent, Stroke, Fill, Glow, 200, DifficultyModifier, Thickness - 1, new Point(Center.X + 40, Center.Y + 10), "Small", 2, seed.Next(2000000000) / seed.Next(1, 2000000000));
                        Parent.Objects.Add(s0);
                        Parent.Objects.Add(s1);
                        Parent.Objects.Add(s2);
                        Parent.canvas.Children.Add(s0.Geometry);
                        Parent.canvas.Children.Add(s1.Geometry);
                        Parent.canvas.Children.Add(s2.Geometry);
                    }
                    break;
                case "Small":
                    break;
                default:break;
            }
        }

        private bool RemoveFromDisplay(Effect e) { Parent.canvas.Children.Remove(e.Geometry); return true; }
        private bool RemoveFromDisplay(Ordnance o) { Parent.canvas.Children.Remove(o.Geometry); return true; }
        private void AddToDisplay(Effect e) { Parent.canvas.Children.Add(e.Geometry); }
        private void AddToDisplay(Ordnance o) { Parent.canvas.Children.Add(o.Geometry); }

        public override void Destroy()
        {
            IsDestroyed = true;
            for (int i = 0; i < Geometry.Points.Count - 1; i++)
            {
                Polyline segment = generateExplosionGeometry(i, i + 1);
                Point center = new Point(Center.X, Center.Y);
                Point trajectory = new Point(Geometry.Points[i].X, Geometry.Points[i].Y);
                Effect line = new ExplodeEffect(center, trajectory, 15 + i, .01, segment);
                Effects.Add(line);
                AddToDisplay(line);
            }
            Geometry.Visibility = Visibility.Hidden;
            DestroyedTimer = 200;
            Explode();
        }
    }
}
