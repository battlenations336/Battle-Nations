using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNR
{
    public class Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle()
        {

        }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Point _point)
        {
            if (_point.X < X || _point.X > (Width - 1))
                return (false);
            if (_point.Y < Y || _point.Y > (Height - 1))
                return (false);
            return (true);
        }
    }

    public class RectangleD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public RectangleD()
        {

        }

        public RectangleD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Point _point)
        {
            if (_point.X < X || _point.X > (Width - 1))
                return (false);
            if (_point.Y < Y || _point.Y > (Height - 1))
                return (false);
            return (true);
        }
    }
}
