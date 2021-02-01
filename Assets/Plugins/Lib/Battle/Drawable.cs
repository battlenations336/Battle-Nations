
using UnityEngine;

namespace BNR
{
    public interface Drawable
    {
        void drawFrame(int frame, GameObject g);

        double getSortPosition();

        RectangleD getBounds();

        RectangleD getBounds(int frame);

        int getEndFrame();

        AGAbility.TargetSquare[] getTargetSquares();
    }
}
