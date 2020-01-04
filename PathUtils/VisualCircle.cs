using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Veda.PathFinder.PathUtils
{
   public class VisualCircle
   {
      private Action<int, int> ActionOnPoint;
      public VisualCircle(Action<int, int> act)
      {
         ActionOnPoint = act;
      }

      public void circle(int cx, int cy, int radius)
      {
         if (radius == 0)
         {
            ActionOnPoint(cx, cy);
            return;
         }
         int error = -radius;
         int x = radius;
         int y = 0;
         
         while (x >= y)
         {
            plot8(cx, cy, x, y);

            error += y;
            ++y;
            error += y;

            if (error >= 0)
            {
               plot8(cx, cy, x, y);
               --x;               
               error -= x;
               error -= x;
            }
         }
      }

      private void plot8(int cx, int cy, int x, int y)
      {
         plot4(cx, cy, x, y);
         if (x != y) plot4(cx, cy, y, x);
      }

      private void plot4(int cx, int cy, int x, int y)
      {
         ActionOnPoint(cx + x, cy + y);
         if (x != 0) ActionOnPoint(cx - x, cy + y);
         if (y != 0) ActionOnPoint(cx + x, cy - y);
         if (x != 0 && y != 0)ActionOnPoint(cx - x, cy - y);
      }

   }
}
