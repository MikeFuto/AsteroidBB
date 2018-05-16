using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Asteroid.Shapes
{
    class Draw
    {
        public Polyline Lines { get; set; }

        public Draw(Brush stroke, Brush fill, int thickness)
        {
            Lines = new Polyline()
            {
                Stroke = stroke,
                Fill = fill,
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round

            };
        }
    }
}
