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
    public class MotherShip : GameObject
    {
        public Canvas Parent { get; set; }
        public PlayerShip Enemy { get; set; }
        public Color SaucerColor { get; set; }
        public bool CanFireBullet { get; set; }
        public double Speed { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public int DestroyedTimer { get; set; }
        public int FireCooldown { get; set; }
        public int LaserCooldown { get; private set; }
        public bool CanFireLaser { get; private set; }
        public int HitsToDestroy { get; set; }

        public MotherShip(Canvas parent, int pointValue, double difficulty, int hitsToDestroy, Brush stroke, Brush fill, Color glow, int thickness, Point center, double speed, PlayerShip player) : base(center, pointValue, difficulty, new List<Ordnance>(), new List<Effect>())
        {
            Enemy = player;
            Parent = parent;
            HitsToDestroy = hitsToDestroy;
            IsDestroyed = false;
            CanFireBullet = true;
            Speed = speed;
            LaserCooldown = 900;
            Geometry = new Draw(stroke, fill, thickness).Lines;
            Geometry.Points = new Plot(this, center).Points;
            Geometry.Effect = new FX(glow).Glow;

            OffsetX = -.1 * Speed;
            OffsetY = 0;

            Effect e = new ChargeEffect(new Point(Center.X, Center.Y + 60), Brushes.Purple, Brushes.White, SaucerColor, 900, OffsetX, OffsetY);
            Effects.Add(e);
            AddToDisplay(e);
        }

        public override void Destroy()
        {
            HitsToDestroy--;
            if(HitsToDestroy <= 0)
            {
                IsDestroyed = true;
                foreach (Effect e in Effects)
                    RemoveFromDisplay(e);
                for (int i = 0; i < Geometry.Points.Count - 3; i += 3)
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
            BeamCooldown();
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
                CanFireBullet = true;
            if (!IsDestroyed && CanFireBullet)
            {
                Point bulletCenter = new Point(Center.X, Center.Y);
                Point bulletTrajectory = new Point(Enemy.Center.X, Enemy.Center.Y);

                Ordnance o = new Bullet(SaucerColor, bulletCenter, bulletTrajectory, .01, 200);
                Ordnances.Add(o);
                AddToDisplay(o);

                CanFireBullet = false;
                FireCooldown = (int)(60 / DifficultyModifier); 
            }
            FireLaser();
        }

        private void FireLaser()
        {
            if (LaserCooldown <= 0)
                CanFireLaser = true;
            if (!IsDestroyed && CanFireLaser)
            {
                Point laserStart = new Point(Center.X, Center.Y + 60);
                Point laserEnd = new Point(Enemy.Center.X, Enemy.Center.Y);

                Ordnance o = new Laser(SaucerColor, laserStart, laserEnd, .01, 50);
                Ordnances.Add(o);
                AddToDisplay(o);

                LaserCooldown = 900;
                CanFireLaser = false;
            }
      
        }

        private void WeaponCooldown()
        {
            if (!CanFireBullet)
                FireCooldown--;
        }

        private void BeamCooldown()
        {
            if (!CanFireLaser)
                LaserCooldown--;
        }
    }
}
