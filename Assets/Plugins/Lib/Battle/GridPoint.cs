using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNR
{
    public class GridPoint
    {
        public static int GRID_X = 1; //100;
        public static int GRID_Y = 1; //50;

        public int x, y;

        public GridPoint(int xg, int yg)
        {
            x = -GRID_X * (xg + yg);
            y = GRID_Y * (yg - xg);
            if (yg < 0)
            {
                x -= GRID_X / 4;
                y += (GRID_Y + 2) / 4;
            }
            else if (yg > 0)
            {
                x += GRID_X / 4;
                y -= GRID_Y / 4;
            }
        }

        public GridPoint(GridPoint p, int x, int y)
        {
            this.x = p.x - GRID_X * (x + y);
            this.y = p.y + GRID_Y * (y - x);
        }

        public GridPoint translate(int x, int y)
        {
            return new GridPoint(this, x, y);
        }
    }
}
