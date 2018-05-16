using Asteroid.Effects;
using Asteroid.Ordnances;
using Asteroid.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroid.Objects
{
    public class SmallSaucer : GameObject
    {
        public Canvas Parent { get; set; }
        public PlayerShip Enemy { get; set; }
        public Color SaucerColor { get; set; }
        public bool CanFire { get; set; }
        public double Speed { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public int DestroyedTimer { get; set; }
        public int FireCooldown { get; set; }

        public SmallSaucer(Canvas parent, Brush stroke, Brush fill, Color glow, int thickness, int pointValue, double difficulty, Point center, double speed, PlayerShip player) : base(center, pointValue, difficulty, new List<Ordnance>(), new List<Effect>())
        {
            Enemy = player;
            Parent = parent;
            IsDestroyed = false;
            CanFire = true;
            Speed = speed;
            Geometry = new Draw(stroke, fill, thickness).Lines;
            Geometry.Points = new Plot(this, center).Points;
            Geometry.Effect = new FX(glow).Glow;
        }

        public override void Destroy()
        {
            IsDestroyed = true;
            for(int i = 0; i < Geometry.Points.Count - 2; i += 2)
            {
                Polyline segment = generateExplosionGeometry(i, i + 1);
                Point center = new Point(Center.X, Center.Y);
                Point trajectory = Geometry.Points[i];
                Effect line = new ExplodeEffect(center, trajectory, 50 + i, .01, segment);
                Effects.Add(line);
                AddToDisplay(line);
            }
            Geometry.Visibility = Visibility.Hidden;
            DestroyedTimer = 200;
        }

        private Polyline generateExplosionGeometry(int i1, int i2)
        {
            return new Polyline()
            {
                Points = new PointCollection() { new Point(Geometry.Points[i1].X, Geometry.Points[i1].Y), new Point(Geometry.Points[i2].X, Geometry.Points[i2].Y) },
                Stroke = Brushes.White,
                StrokeThickness = 3,
                Effect = new FX(SaucerColor).Glow
            };
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

        public override void Move()
        {
            TranslateShip();
            Fire();
            Ordnances.RemoveAll(ord => { return !ord.Move() ? RemoveFromDisplay(ord) : false; });
            Effects.RemoveAll(effect => { return !effect.Display() ? RemoveFromDisplay(effect) : false; });
            DestroyedTimeoutCooldown();
            WeaponCooldown();

        }

        public override void TeleportPoints(Point teleportOffset)
        {
            //Don't loop borders.
        }

        private bool RemoveFromDisplay(Effect e) { Parent.Children.Remove(e.Geometry); return true; }
        private bool RemoveFromDisplay(Ordnance o) { Parent.Children.Remove(o.Geometry); return true; }
        private void AddToDisplay(Effect e) { Parent.Children.Add(e.Geometry); }
        private void AddToDisplay(Ordnance o) { Parent.Children.Add(o.Geometry); }

        private void TranslateShip()
        { 
            if (!IsDestroyed)
            {
                if (Center.X > -50)
                {
                    OffsetX = -.1 * Speed;
                    OffsetY = 0;
                    var translation = new TranslateTransform(OffsetX, OffsetY);
                    PointCollection translated = new PointCollection();
                    foreach (Point vert in Geometry.Points)
                        translated.Add(translation.Transform(vert));
                    Geometry.Points = translated;
                    Center = translation.Transform(Center);
                }
                else
                    Destroy();
            }
        }

        private void Fire()
        {
            if (FireCooldown <= 0)
                CanFire = true;
            if (!IsDestroyed && CanFire)
            {
                Point bulletCenter = new Point(Center.X, Center.Y + 10);
                double rise = Enemy.Center.Y - Center.Y;
                double run = Enemy.Center.X - Center.X;
                double dist = Math.Sqrt(run * run + rise * rise);
                double scale = dist / .1;
                rise = rise / scale;
                run = run / scale;
                Point bulletTrajectory = new Point(Enemy.Center.X, Enemy.Center.Y);

                Ordnance o = new Bullet(SaucerColor, bulletCenter, bulletTrajectory, .01, 200);
                Ordnances.Add(o);
                AddToDisplay(o);

                CanFire = false;
                FireCooldown = (int)(60 / DifficultyModifier);
            }
        }

        private void WeaponCooldown()
        {
            if (!CanFire)
                FireCooldown--;
        }

    }
}
