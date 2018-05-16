using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Asteroid.Objects;

namespace Asteroid.Shapes
{
    class Plot
    {
        public PointCollection Points { get; set; }
      
        public Plot(PlayerShip p, Point center)
        {
            double x = center.X;
            double y = center.Y;
            Points = new PointCollection()
            {
                new Point(x-10,y+10),  //x-5, y-5
                new Point(x,y-20),
                new Point(x+10,y+10),  //x+5, y-5
                new Point(x+8,y+5),
                new Point(x-8,y+5),    //x, y+5
                new Point(x-10,y+10)
            };
        }

        public Plot(string AsteroidType, Point center)
        {
            double x = center.X;
            double y = center.Y;
            
            switch(AsteroidType)
            {
                case "ExtraLarge0":
                    Points = new PointCollection()
                    {
                        new Point(x-140,y), new Point(x-100,y-80), new Point(x-100,y-120),
                        new Point(x,y-140), new Point(x+60,y-120), new Point(x+120,y-100),
                        new Point(x+140,y), new Point(x+100,y+80), new Point(x+100,y+120),
                        new Point(x,y+140), new Point(x-100,y+120), new Point(x-140,y)
                    };
                    break;

                case "ExtraLarge1":
                    Points = new PointCollection()
                    {
                        new Point(x-140,y), new Point(x-100,y-60), new Point(x-120,y-100),
                        new Point(x,y-140), new Point(x+120,y-120), new Point(x+140,y),
                        new Point(x+100,y+120), new Point(x,y+140), new Point(x-120,y+120),
                        new Point(x-120,y+60), new Point(x-140,y)
                    };
                    break;

                case "ExtraLarge2":
                    Points = new PointCollection()
                    {
                        new Point(x+140,y), new Point(x+140,y+80), new Point(x+100,y+120),
                        new Point(x,y+140), new Point(x-120,y+120), new Point(x-140,y+60),
                        new Point(x-140,y-80), new Point(x-100,y-120), new Point(x,y-140),
                        new Point(x+80,y-80), new Point(x+140,y)
                    };
                    break;

                case "Large0":
                    Points = new PointCollection()
                    {
                        new Point(x-100,y), new Point(x-60,y-40), new Point(x-60,y-80),
                        new Point(x,y-100), new Point(x+20,y-80), new Point(x+80,y-60),
                        new Point(x+100,y), new Point(x+60,y+40), new Point(x+60,y+80),
                        new Point(x,y+100), new Point(x-60,y+80), new Point(x-100,y)
                    };
                    break;
                case "Large1":
                    Points = new PointCollection()
                    {
                        new Point(x-100,y), new Point(x-60,y-20), new Point(x-80,y-60),
                        new Point(x,y-100), new Point(x+80,y-80), new Point(x+100,y),
                        new Point(x+60,y+80), new Point(x,y+100), new Point(x-80,y+80),
                        new Point(x-80,y+20), new Point(x-100,y)
                    };
                    break;
                case "Large2":
                    Points = new PointCollection()
                    {
                        new Point(x+100,y), new Point(x+100,y+40), new Point(x+60,y+80),
                        new Point(x,y+100), new Point(x-80,y+80), new Point(x-100,y+20),
                        new Point(x-100,y-40), new Point(x-60,y-80), new Point(x,y-100),
                        new Point(x+40,y-40), new Point(x+100,y)
                    };
                    break;
                case "Medium0":
                    Points = new PointCollection()
                    {
                        new Point(x-40,y), new Point(x-20,y-20), new Point(x-20,y-40),
                        new Point(x+20,y-20), new Point(x+40,y+20), new Point(x,y+40),
                        new Point(x-40,y)
                    };
                    break;

                case "Medium1":
                    Points = new PointCollection()
                    {
                        new Point(x+40,y), new Point(x,y+40), new Point(x-40,y+20),
                        new Point(x-20,y-20), new Point(x,y-20), new Point(x,y-40),
                        new Point(x+40,y)
                    };
                    break;

                case "Medium2":
                    Points = new PointCollection()
                    {
                        new Point(x+40,y), new Point(x+20,y+40), new Point(x-20,y+40),
                        new Point(x-40,y-20), new Point(x+20,y-40), new Point(x+40,y)
                    };
                    break;

                case "Small0":
                    Points = new PointCollection()
                    {
                        new Point(x-10,y), new Point(x-10,y-10), new Point(x,y-20),
                        new Point(x+20,y-10), new Point(x+10,y+10), new Point(x,y+20),
                        new Point(x-20,y+10), new Point(x-10,y)
                    };
                    break;

                case "Small1":
                    Points = new PointCollection()
                    {
                        new Point(x-20,y), new Point(x-20,y-10), new Point(x,y-20),
                        new Point(x+20,y), new Point(x+10,y+20), new Point(x-10,y+10),
                        new Point(x-20,y)
                    };
                    break;

                case "Small2":
                    Points = new PointCollection()
                    {
                        new Point(x-20,y), new Point(x,y-20), new Point(x+20,y),
                        new Point(x+10,y+10), new Point(x+10,y+20), new Point(x-10,y+10),
                        new Point(x-20,y)
                    };
                    break;

                default: goto case "ExtraLarge0";
            }
        }

        public Plot(SmallSaucer s, Point center)
        {
            double x = center.X;
            double y = center.Y;

            Points = new PointCollection()
            {
                new Point(x+30,y), new Point(x-30,y),
                new Point(x-20,y+10), new Point(x+20,y+10),
                new Point(x+30,y), new Point(x+20,y-10),
                new Point(x-10,y-10), new Point(x-5,y-20),
                new Point(x+5,y-20), new Point(x+10,y-10),
                new Point(x-20,y-10), new Point(x-30,y)
            };
        }
        
        public Plot(MotherShip m, Point center)
        {
            double x = center.X;
            double y = center.Y;

            Points = new PointCollection()
            {
                //top section 
                new Point(x,y-15), new Point(x+2.5,y-20), new Point(x,y-20),
                new Point(x-5,y-10), new Point(x-10,y-10), new Point(x-5,y-20),
                new Point(x-15,y-20), new Point(x-10,y-30), new Point(x+10,y-30),
                new Point(x+15,y-20), new Point(x+20,y-20), new Point(x-20,y-20),
                new Point(x-20,y-10), new Point(x+20,y-10), new Point(x+20,y-20),
                new Point(x+20,y-10), //bottom right corner of viewing center

                new Point(x+40,y), new Point(x-40,y), new Point(x-40,y+10),
                new Point(x+40,y+10), new Point(x+40,y), new Point(x+40,y+10),
                new Point(x+30,y+20), new Point(x-30,y+20), new Point(x+30,y+20),
                new Point(x+35,y+30), new Point(x+5,y+60), new Point(x+25,y+30),
                new Point(x+20,y+20), //top left point of right claw

                new Point(x+10,y+20), new Point(x+5,y+30), new Point(x-5,y+30),
                new Point(x+5,y+30), new Point(x,y+57.5), new Point(x-5,y+30),
                new Point(x-10,y+20), new Point(x-20,y+20), new Point(x-25,y+30),
                new Point(x-5,y+60), new Point(x-35,y+30), new Point(x-30,y+20),
                new Point(x-40,y+10), new Point(x-40,y), new Point(x-20,y-10)  
            };
        }
    }
}
