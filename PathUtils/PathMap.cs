using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Veda.PathFinder.PathUtils
{
  public class PointLocation {
    public readonly int X;
    public readonly int Y;
    public PointLocation(int x, int y)
    {
       X = x;
       Y = y;
    }
    public bool IsSameLoc(PointLocation loc)
    {
      if (loc == null) return false;
      return loc.X == X && loc.Y == Y;
    }
#if DEBUG
    public override string ToString()
    {
      return "Pt loc (" + X + "," + Y+")";
    }
#endif
  }
  public enum MapGeoTypes
  {
    MapGeoTypeLand,
    MapGeoTypeWater,
    MapGeoTypeClif,
  }

  //public enum MapOverlapResult
  //{
  //  NotOverlap,
  //  Overlap,
  //  DontCare,
  //}

  public class MapObjectShape
  {
    public const int MaxMapShapeWidth = 3;
    public const int MaxMapShapeHeight = 3;
    public byte[] Shape;
    public readonly int ShapeWidth;
    public readonly int ShapeHeight;
    public readonly int ShapeId;
    public MapObjectShape(int w, int h)
    {
      Shape = new byte[h];
      ShapeWidth = w;
      ShapeHeight = h;
      ShapeId = w * 10 + h;
      for (int i = 0; i < h; i++)
      {
        Shape[i] = (byte)((1 << w) - 1);
      }
    }
  }
  //public interface MapObject
  //{
  //  int GetFactionGroup();
  //  PointLocation GetObjectLocation();
  //  //PointLocation GetObjectShapeOffset();
  //  MapLocation[] OccupiedLocations();
  //  MapObjectShape GetObjectShape();
  //  //bool CheckMovability(MapLocation loc);
  //  /// <summary>
  //  /// Check how much resistance it is to push through this object
  //  /// </summary>
  //  /// <param name="obj"></param>
  //  /// <returns></returns>
  //  double CheckResistance(MapObject obj);
  //  //where to move to for the engine to calculate
  //  //MapLocation MoveToLoc { get; set; }
  //  //MapOverlapResult CheckOverLap(MapLocation[,] grid);
  //  //void MovingTo(PointLocation pt);
  //}


  public class MapLocation
  {
    public MapLocation(int x, int y)
    {
      MapLoc = new PointLocation(x,y);      
    }
    public readonly PointLocation MapLoc;
    public GenMapObject FixedObj;
    public GenMoveableObj CurObject;
    public List<GenMapObject> FloatingObjects;
    public MapGeoTypes MapType;
    public int FactionGroupVisiblity;
#if DEBUG
    public override string ToString()
    {
      return "Maploc (" + MapLoc.X + "," + MapLoc.Y + ")";
    }
#endif
  }

  public class MapRoutCalculateUnit
  {
    public enum MapRoutUnitStatus
    {
      Available = 0,
      Open,
      Closed,
    }
    public enum MapRoutUnitResultTag
    {
      UnableToReachTarget,
      ReachedTarget,
      ChainLimitReached,
    }
    public MapLocation Location;
    public float CurrentCost;
    
    public MapRoutCalculateUnit calcFromLocation;
    public MapRoutUnitResultTag ResultTag;
    public int PassId;
    public MapRoutUnitStatus UnitStatus;
    public float CellCost; //in case there are destroyable walls or units.
    public void Init(int pid)
    {
      if (PassId != pid)
      {
        //new
        UnitStatus = MapRoutUnitStatus.Available;
        PassId = pid;
        calcFromLocation = null;        
      }
    }

    public void SetFrom(MapRoutCalculateUnit fromUnit, LinkedList<MapRoutCalculateUnit> OpenList, float weight)
    {
      if (UnitStatus == MapRoutUnitStatus.Closed) return;
      float newcost = fromUnit.CurrentCost + weight;
      if (UnitStatus == MapRoutUnitStatus.Available)
      {
         UnitStatus = MapRoutUnitStatus.Open;
         CurrentCost = newcost;
         OpenList.AddLast(this);
      }
      else
      {
         if (CurrentCost < newcost)
         {
            return;
         }
         CurrentCost = newcost;
      }
      
      calcFromLocation = fromUnit;
    }
    
#if DEBUG
    public override string ToString()
    {
      return "MapRoutCalculateUnit " + Location.MapLoc + " cur=" + CurrentCost;
    }
#endif
  
  }
  public class PathMap
  {
     /// <summary>
     /// obj, loc, X,Y, allow custom check of map.
     /// </summary>
     public Func<GenMapObject, MapLocation, int, int, float> CustPointCheck;
    public bool Prepared = false;
    private object _mapLock;
    public PathMap(MapLocation[,] grid, object lck)
    {
      if (lck == null) _mapLock = new object();
      else _mapLock = lck;
      MapGrid = grid;
      MapWidth = grid.GetLength(0);
      MapHeight = grid.GetLength(1);
      initRouts();
    }
    private void initRouts()
    {
      CalcGrid = new MapRoutCalculateUnit[MapWidth, MapHeight];
      for (int w = 0; w < MapWidth; w++)
      {
        for (int h = 0; h < MapHeight; h++)
        {
          CalcGrid[w, h] = new MapRoutCalculateUnit
          {
            Location = MapGrid[w, h]
          };
        }
      }
    }
    public PathMap MakeCloneForThread()
    {
      return new PathMap(MapGrid, _mapLock);
    }
    private MapRoutCalculateUnit[,] CalcGrid;
    public MapLocation[,] MapGrid;
    public void RemoveObjectFromMap(GenMapObject obj)
    {
      lock (_mapLock)
      {
        RemoveObjectFromMapInternalNoLock(obj);
      }
    }
    private void RemoveObjectFromMapInternalNoLock(GenMapObject obj)
    {
      foreach (var occ in obj.OccupiedLocations())
      {
        occ.CurObject = null;
        occ.FixedObj = null;
      }
    }
    public bool PlaceObjectOnMap(GenMapObject obj, PointLocation moveTo)
    {
      PointLocation loc = moveTo;
      PointLocation oldObjLoc = null;
      GenMoveableObj isMoveable = obj as GenMoveableObj;
      lock (_mapLock)
      {
         oldObjLoc = obj.GetObjectLocation();
        if (moveTo == null)
        {
           loc = oldObjLoc;
        }

        //PointLocation off = obj.GetObjectShapeOffset();
        MapObjectShape shape = obj.GetObjectShape();

        int shapew = shape.ShapeWidth + loc.X;
        int shapeh = shape.Shape.Length + loc.Y;

        MapLocation[] occLocs = obj.OccupiedLocations();
        for (int pass = 0; pass < 2; pass++)
        {
          if (moveTo != null)
          {
            if (pass == 1)
            {
               RemoveObjectFromMapInternalNoLock(obj);
              obj.SetObjectLocation(moveTo);
            }
          }
          int occAt = 0;
          for (int y = loc.Y; y < shapeh; y++)
          {
            for (int x = loc.X; x < shapew; x++)
            {

              if (pass == 0)
              {
                if (y < 0 || y >= MapHeight) return false;
                if (x < 0 || x >= MapWidth) return false;
                MapLocation mapxy = MapGrid[x, y];
                if (!obj.CheckPlaceAbility(mapxy)) return false;
                if (mapxy.CurObject != null && mapxy.CurObject != obj)
                {
                  return false;
                }
              }
              else
              {
                MapLocation mapxy = MapGrid[x, y];
                mapxy.CurObject = isMoveable;
                if (isMoveable == null) mapxy.FixedObj = obj;
                occLocs[occAt++] = mapxy;
              }
            }
          }
        }        
      }
      if (isMoveable != null)
      {
         isMoveable.MovingTo(loc);
      }
      return true;
    }
    public virtual float CheckResistance(GenMoveableObj obj, PointLocation objLoc)
    {
      if (obj == null) return 0;
      //MapOverlapResult objCheck = obj.CheckOverLap(MapGrid);
      //switch (objCheck)
      //{
      //  case MapOverlapResult.Overlap: return true;
      //  case MapOverlapResult.NotOverlap: return false;
      //}

      //PointLocation objLoc = obj.GetObjectLocation();
      MapObjectShape objShape = obj.GetObjectShape();
      //PointLocation objOff = obj.GetObjectShapeOffset();
      int startLocX = objLoc.X;

      //int MapWidth = MapGrid.GetLength(0);
      //int MapHeight = MapGrid.GetLength(1);
      if (startLocX < 0) return GenMoveableObj.UNABLE_TO_MOVE_NUM;
      if (startLocX >= MapWidth) return GenMoveableObj.UNABLE_TO_MOVE_NUM;
      int startLocY = objLoc.Y;
      if (startLocY < 0) return GenMoveableObj.UNABLE_TO_MOVE_NUM;
      if (startLocY >= MapHeight) return GenMoveableObj.UNABLE_TO_MOVE_NUM;
      float result = 0;
      for (int h = 0; h < objShape.ShapeHeight; h++)
      {
        for (int w = 0; w < objShape.ShapeWidth; w++)
        {
          if ( (objShape.Shape[h] & (1<<w)) != 0)
          {
            int checkX = startLocX + w;
            int checkY = startLocY + h;
            if (checkX >= MapWidth) return GenMoveableObj.UNABLE_TO_MOVE_NUM;
            if (checkY >= MapHeight) return GenMoveableObj.UNABLE_TO_MOVE_NUM;
            MapLocation loc = MapGrid[checkX, checkY];
            switch (loc.MapType)
            {
               case MapGeoTypes.MapGeoTypeClif: return GenMoveableObj.UNABLE_TO_MOVE_NUM;
               case MapGeoTypes.MapGeoTypeWater: return GenMoveableObj.UNABLE_TO_MOVE_NUM;
              case MapGeoTypes.MapGeoTypeLand:
                if (loc.CurObject == null || loc.CurObject == obj) continue;
                if (CustPointCheck != null)
                {
                   float res = CustPointCheck(obj, loc, checkX, checkY);
                   if (res != GenMoveableObj.UNABLE_TO_MOVE_NUM) result += res; else return GenMoveableObj.UNABLE_TO_MOVE_NUM;
                }
                else
                {
                   if (loc.CurObject != null)
                   {
                      float res = obj.CheckResistance(loc.CurObject);
                      if (res != GenMoveableObj.UNABLE_TO_MOVE_NUM) result += res; else return GenMoveableObj.UNABLE_TO_MOVE_NUM;
                   }
                   if (loc.FixedObj != null)
                   {
                      float res = obj.CheckResistance(loc.FixedObj);
                      if (res != GenMoveableObj.UNABLE_TO_MOVE_NUM) result += res; else return res;
                   }
                }
                break;
            }
          }
        }
      }
      return result;
    }
    private int CurPassId;

    public readonly int MapWidth;
    public readonly int MapHeight;
    public class MapPosCheckWeight
    {
      public readonly int X;
      public readonly int Y;
      public readonly float weight;
      public MapPosCheckWeight(int x, int y, float w)
      {
         X = x;
         Y = y;
         weight = w;
      }
    }
    public static readonly MapPosCheckWeight[] posToCheck = new MapPosCheckWeight[]{
        new MapPosCheckWeight( -1,  -1, 1.41f),
        new MapPosCheckWeight(  0,  -1, 1),
        new MapPosCheckWeight(  1,  -1, 1.41f),
        new MapPosCheckWeight( -1,   0, 1),
        new MapPosCheckWeight(  1,   0, 1),
        new MapPosCheckWeight( -1,   1, 1.41f),
        new MapPosCheckWeight(  0,   1, 1),
        new MapPosCheckWeight(  1,   1, 1.41f),
      };
    
    public MapLocation GetMapLocation(PointLocation pt)
    {
      return MapGrid[pt.X, pt.Y];
    }
    public MapRoutCalculateUnit RetrivePathAfterPrep(int x, int y)
    {
       if (x < 0 || y < 0) return null;
       if (x >= MapWidth || y >= MapHeight) return null;
       MapRoutCalculateUnit munit = CalcGrid[x,y];
       munit.Init(CurPassId);
       return munit;
    }
    public void PreparePathMap(MapLocation toLoc, GenMoveableObj CurObj)
    {
      CurPassId++;
      MapRoutCalculateUnit fromUnit = CalcGrid[toLoc.MapLoc.X, toLoc.MapLoc.Y];      

      fromUnit.ResultTag = MapRoutCalculateUnit.MapRoutUnitResultTag.UnableToReachTarget;
      fromUnit.UnitStatus = MapRoutCalculateUnit.MapRoutUnitStatus.Open;
      //fromUnit.EstimatedCost = fromUnit.EstimateCost(to);
      fromUnit.CurrentCost = 0;
      fromUnit.calcFromLocation = null;
      fromUnit.PassId = CurPassId;      

      LinkedList<MapRoutCalculateUnit> OpenList = new LinkedList<MapRoutCalculateUnit>();      
      OpenList.AddFirst(fromUnit);
      //MapObject CurObj = from.CurObject;
      while (OpenList.Count > 0)
      {
        fromUnit = OpenList.First();
        OpenList.RemoveFirst();
        fromUnit.UnitStatus = MapRoutCalculateUnit.MapRoutUnitStatus.Closed;
        
        PointLocation fromLoc = fromUnit.Location.MapLoc;
        
        foreach (MapPosCheckWeight mvpos in posToCheck)
        {
          int xpos = fromLoc.X + mvpos.X;
          if (xpos >= 0 && xpos < MapWidth)
          {
            int ypos = fromLoc.Y + mvpos.Y;
            if (ypos >= 0 && ypos < MapHeight)
            {
              MapRoutCalculateUnit mapUnit = CalcGrid[xpos, ypos];
              mapUnit.Init(CurPassId);
              if (mapUnit.UnitStatus == MapRoutCalculateUnit.MapRoutUnitStatus.Closed) continue;
              switch (mapUnit.Location.MapType)
              {
                case MapGeoTypes.MapGeoTypeClif:
                case MapGeoTypes.MapGeoTypeWater:
                  mapUnit.UnitStatus = MapRoutCalculateUnit.MapRoutUnitStatus.Closed;
                  continue;
              }
              if (mapUnit.UnitStatus == MapRoutCalculateUnit.MapRoutUnitStatus.Available)
              {
                 mapUnit.CellCost = CheckResistance(CurObj, mapUnit.Location.MapLoc);
                 if (mapUnit.CellCost == GenMoveableObj.UNABLE_TO_MOVE_NUM)
                 {
                   mapUnit.UnitStatus = MapRoutCalculateUnit.MapRoutUnitStatus.Closed;
                   continue;
                 }
              }
              
              mapUnit.SetFrom(fromUnit, OpenList, mvpos.weight + mapUnit.CellCost);              
            }
          }
        }
      }
      Prepared = true;
    }

    public static IEnumerable<MapRoutCalculateUnit> GetRoute(MapRoutCalculateUnit ut)
    {
      List<MapRoutCalculateUnit> lst = new List<MapRoutCalculateUnit>();
      while (ut != null)
      {
        lst.Add(ut);
        ut = ut.calcFromLocation;
      }
      if (lst.Count > 0) lst.RemoveAt(lst.Count - 1);
      lst.Reverse();
      return lst;
    }
  }
}
