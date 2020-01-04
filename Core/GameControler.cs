using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Veda.PathFinder.PathUtils;

namespace com.Veda.PathFinder.Core
{
  public class GameControler : IFactionDB
  {     
    private Dictionary<int, int> FactionToAllyGroupMap = new Dictionary<int, int>();
    public void SetFactionGroup(int faction, int Group)
    {
       FactionToAllyGroupMap.Add(faction, Group);
    }
    public int GetFactionGroup(int faction)
    {
       return FactionToAllyGroupMap[faction];
    }
    public bool IsFactionAlly(int fact1, int fact2)
    {
      return FactionToAllyGroupMap[fact1] == FactionToAllyGroupMap[fact2];
    }

    private readonly MoveEngine Engine;
    public MoveEngine GetMoveEngine() { return Engine; }
    public GameControler(PathMap map)
    {
       Engine = new MoveEngine(map);
    }
    public void StopEngine()
    {
       Engine.StopEngine();
    }
  }
}
