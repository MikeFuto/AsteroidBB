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
    public class PlayerShip : GameObject
    {
        public Canvas Parent { get; set; }
        public Color PlayerColor { get; set; }
        public Brush PlayerStroke { get; set; }
        public Brush PlayerFill { get; set; }
        public Point ThrustStart { get; set; }
        public Point ThrustEnd { get; set; }
        public bool IsRotatingClockwise { get; set; }
        public bool IsRotatingCounterClockwise { get; set; }
        public bool IsApplyingThrust { get; set; }
        public bool IsInvincible { get; set; }
        public bool CanWarp { get; set; }
        public bool CanReappear { get; private set; }
        public bool CanFire { get; set; }
        public double Speed { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public int InvincibilityTimer { get; set; }
        public int DestroyedTimer { get; set; }
        public int FireCooldown { get; set; }
        public int WarpCooldown { get; private set; }
        public int Score { get; set; }
        public int Lives { get; set; }
        private int NextLife { get; set; }
        public int RoundModifier { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public Point RespawnLocation { get; set; }

        public PlayerShip(Brush stroke, Brush fill, Color glow, int thickness, double difficulty, int round, int lives, int health, Point center, Canvas parent) : base(center, 0, difficulty, new List<Ordnance>(), new List<Effect>())
        {
            Geometry = new Draw(stroke, fill, thickness).Lines;
            PlayerColor = glow;
            PlayerStroke = stroke;
            PlayerFill = fill;
            Geometry.Points = new Plot(this,center).Points;
            Geometry.Effect = new FX(glow).Glow;
            RespawnLocation = center;
            IsRotatingClockwise = false;
            IsRotatingCounterClockwise = false;
            IsApplyingThrust = false;
            IsInvincible = false;
            IsDestroyed = false;
            CanWarp = true;
            Trajectory = new Point(Center.X, Center.Y - .1);
            ThrustStart = new Point(Center.X - 5, Center.Y + 10);
            ThrustEnd = new Point(Center.X + 5, Center.Y + 10);
            Speed = 0;
            Score = 0;
            RoundModifier = round;
            Lives = lives;
            Health = health;
            MaxHealth = health;
            Parent = parent;
        }

        public void Thrust()
        {
            if(!IsDestroyed)
            {
                if(IsApplyingThrust)
                {
                    Speed = 1;
                    double addX = (Trajectory.X - Center.X) * Speed;
                    double addY = (Trajectory.Y - Center.Y) * Speed;

                    OffsetX += addX;
                    OffsetY += addY;

                    Effect t = new ThrustEffect(PlayerColor, ThrustStart, ThrustEnd, 5);
                    Effects.Add(t);
                    Parent.Children.Add(t.Geometry);
                }
            }
        }

        private bool RemoveFromDisplay(Effect e){ Parent.Children.Remove(e.Geometry); return true; }
        private bool RemoveFromDisplay(Ordnance o) { Parent.Children.Remove(o.Geometry); return true; }
        private void AddToDisplay(Effect e) { Parent.Children.Add(e.Geometry); }
        private void AddToDisplay(Ordnance o) { Parent.Children.Add(o.Geometry); }

        public override void Move()
        {
            Thrust();
            Rotate();
            TranslateShip();

            Effects.RemoveAll(effect => { return !effect.Display() ? RemoveFromDisplay(effect) : false; });
            Ordnances.RemoveAll(ord => { return !ord.Move() ? RemoveFromDisplay(ord) : false; });

            DestroyedTimeoutCooldown();
            InvincibilityCooldown();
            WeaponCooldown();
            WarpAbilityCooldown();
        }

        private void DestroyedTimeoutCooldown()
        {
            if (IsDestroyed)
            {
                if (DestroyedTimer <= 0)
                {
                    Respawn();
                }
                else
                    DestroyedTimer--;
            }
        }

        private void WarpAbilityCooldown()
        {
            if (!CanWarp)
                WarpCooldown--;
        }

        private void WeaponCooldown()
        {
            if (!CanFire)
                FireCooldown--;
        }

        private void InvincibilityCooldown()
        {
            if (IsInvincible)
            {
                if (InvincibilityTimer <= 0)
                {
                    IsInvincible = false;
                    Geometry.Stroke = PlayerStroke;
                }
                else if (InvincibilityTimer % 15 < 8)
                    Geometry.Stroke = Brushes.White;
                else
                    Geometry.Stroke = PlayerStroke;

                InvincibilityTimer--;
            }
        }

        private void TranslateShip()
        {
            //Grace Radius For Full Stop
            if (OffsetX < .01 && OffsetX > -.01) OffsetX = 0;
            if (OffsetY < .01 && OffsetY > -.01) OffsetY = 0;

            if (!IsDestroyed)
            {
                var translation = new TranslateTransform(OffsetX, OffsetY);
                PointCollection translated = new PointCollection();
                foreach (Point vert in Geometry.Points)
                    translated.Add(translation.Transform(vert));
                Geometry.Points = translated;
                Trajectory = translation.Transform(Trajectory);
                Center = translation.Transform(Center);
                ThrustStart = translation.Transform(ThrustStart);
                ThrustEnd = translation.Transform(ThrustEnd);
                if (OffsetX > 5) OffsetX = 5.0;
                if (OffsetX < -5) OffsetX = -5.0;
                if (OffsetY > 5) OffsetY = 5.0;
                if (OffsetY < -5) OffsetY = -5.0;
            }

            //Degrade Speed
            if (OffsetX != 0 && !IsApplyingThrust)
                OffsetX += OffsetX > 0 ? -.0005 : .0005;
            if (OffsetY != 0 && !IsApplyingThrust)
                OffsetY += OffsetY > 0 ? -.0005 : .0005;
        }

        public void AddPoints(int value)
        {
            Score += (int)(value * DifficultyModifier * RoundModifier);
            if (Score > 2000000000)
                Score = 2000000000;
            NextLife += (int)(value * DifficultyModifier * RoundModifier);
            if(NextLife >= 10000)
            {
                Lives++;
                NextLife = -10000 * (RoundModifier - 1);
            }
        }

        public override void TeleportPoints(Point teleportOffset)
        {
            var teleport = new TranslateTransform(teleportOffset.X, teleportOffset.Y);
            PointCollection p = new PointCollection();
            foreach (Point vert in Geometry.Points)
                p.Add(teleport.Transform(vert));
            Geometry.Points = p;
            Trajectory = teleport.Transform(Trajectory);
            Center = teleport.Transform(Center);
            ThrustStart = teleport.Transform(ThrustStart);
            ThrustEnd = teleport.Transform(ThrustEnd);
        }

        public void Fire()
        {
            if (FireCooldown <= 0)
                CanFire = true;
            if (!IsDestroyed && CanFire)
            {
                Ordnance o = new Bullet(PlayerColor, Center, Trajectory, 80, 200);
                Ordnances.Add(o);
                Parent.Children.Add(o.Geometry);
                CanFire = false;
                FireCooldown = (int)(15 * DifficultyModifier);
            }
        }

        public void Warp()
        {
            if (WarpCooldown <= 0)
                CanWarp = true;
            if(!IsDestroyed && CanWarp)
            {
                int seed = new Random().Next();
                Random r = new Random(seed);
                double x = r.Next((int)Parent.ActualWidth);
                double y = r.Next((int)Parent.ActualHeight);
                double reappearX = Center.X + x;
                double reappearY = Center.Y + y;
                Effect e1 = new WarpEffect(new Point(Center.X, Center.Y), Brushes.White, Brushes.Black, PlayerColor, 33);
                Effects.Add(e1);
                AddToDisplay(e1);
                if (reappearX > Parent.ActualWidth + 5) reappearX -= Parent.ActualWidth;
                if (reappearX < -5) reappearX += Parent.ActualWidth;
                if (reappearY > Parent.ActualHeight + 5) reappearY -= Parent.ActualHeight;
                if (reappearY < -5) reappearY += Parent.ActualHeight;


                Effect e2 = new WarpEffect(new Point(reappearX,reappearY), Brushes.White, Brushes.Black, PlayerColor, 33);
                Effects.Add(e2);
                AddToDisplay(e2);

                TeleportPoints(new Point(x, y));
                CanWarp = false;
                CanReappear = true;
                WarpCooldown = 200;
            }
        }

        public void Rotate()
        {
            if(!IsDestroyed)
            {
                if (IsRotatingClockwise || IsRotatingCounterClockwise)
                {
                    int degrees = 0;
                    if (IsRotatingClockwise)
                        degrees = 5;
                    else if (IsRotatingCounterClockwise)
                        degrees = -5;
                    var rotation = new RotateTransform(degrees, Center.X, Center.Y);

                    PointCollection rotated = new PointCollection();
                    foreach (Point vert in Geometry.Points)
                        rotated.Add(rotation.Transform(vert));
                    Geometry.Points = rotated;
                    Trajectory = rotation.Transform(Trajectory);
                    ThrustStart = rotation.Transform(ThrustStart);
                    ThrustEnd = rotation.Transform(ThrustEnd);
                }
            }
        }

        private Polyline generateExplosionGeometry(int i1, int i2)
        {
            return new Polyline()
            {
                Points = new PointCollection() { new Point(Geometry.Points[i1].X, Geometry.Points[i1].Y), new Point(Geometry.Points[i2].X, Geometry.Points[i2].Y) },
                Stroke = Brushes.White,
                StrokeThickness = 3,
                Effect = new FX(PlayerColor).Glow
            };
        }

        public override void Destroy()
        {
            Health--;
            if(Health <= 0)
            {
                IsDestroyed = true;
                IsInvincible = true;

                Polyline leftSegment = generateExplosionGeometry(0, 1);
                Polyline rightSegment = generateExplosionGeometry(1, 2);
                Polyline rearSegment = generateExplosionGeometry(3, 4);

                Point leftC = new Point(Center.X, Center.Y), leftT = new Point(Center.X - .1, Center.Y - .1);
                Point rightC = new Point(Center.X, Center.Y), rightT = new Point(Center.X + .1, Center.Y + .1);
                Point rearC = new Point(Center.X, Center.Y), rearT = new Point(Center.X, Center.Y + .1);

                Effect left = new ExplodeEffect(leftC, leftT, 100, 1, leftSegment);
                Effect right = new ExplodeEffect(rightC, rightT, 101, 1, rightSegment);
                Effect rear = new ExplodeEffect(rearC, rearT, 102, 1, rearSegment);
                Effects.Add(left);
                Effects.Add(right);
                Effects.Add(rear);

                AddToDisplay(left);
                AddToDisplay(right);
                AddToDisplay(rear);

                Geometry.Visibility = Visibility.Hidden;
                DestroyedTimer = 150;
            }
        }

        public void Respawn()
        {
            if(Lives > 0)
            {
                Health = MaxHealth;
                Lives--;
                OffsetX = 0;
                OffsetY = 0;
                Speed = 0;
                IsRotatingClockwise = false;
                IsRotatingCounterClockwise = false;
                IsApplyingThrust = false;
                CanWarp = true;
                IsDestroyed = false;

                Center = RespawnLocation;
                Trajectory = new Point(Center.X, Center.Y - .1);
                ThrustStart = new Point(Center.X - 5, Center.Y + 10);
                ThrustEnd = new Point(Center.X + 5, Center.Y + 10);
                Geometry.Points = new Plot(this, Center).Points;
                Geometry.Visibility = Visibility.Visible;

                IsInvincible = true;
                InvincibilityTimer = 200;
            }
        }

    }
}
