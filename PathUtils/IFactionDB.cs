using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Veda.PathFinder.PathUtils
{
  public partial interface IFactionDB
  {
    int GetFactionGroup(int faction);
    bool IsFactionAlly(int fact1, int fact2);
  }
}
