using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.Veda.PathFinder.PathUtils
{

   public enum UnitCatagories
   {
      Unit,
      Building,
   }

   public interface IMoveableGameObj
   {
      IFactionDB Controler { get; }
      int Faction { get; }
      int FactionGroup { get; }
      float Life { get; }
      float AttackPower { get; }
      int ShapeId();
      //PointLocation CurrentGoToLocation { get; set; }
   }

  public abstract class GenMapObject 
  {
     private VisualCircle VisualCir;
     protected PathMap visualMap;
     public Action<MapLocation> ExtraVisualProcessingFunc;
    public GenMapObject(PathMap map, MapObjectShape shape, PointLocation loc)
    {
       objLoc = loc;
       visualMap = map;
      _shape = shape;
      //objOff = off;
      _occupiedLocs = new MapLocation[shape.ShapeWidth * shape.Shape.Length];
      VisualCir = new VisualCircle(VisualUpdateFunc);
    }

    private void VisualUpdateFunc(int x, int y)
    {
       if (x < 0 || y < 0) return;
       if (x >= visualMap.MapWidth) return;
       if (y >= visualMap.MapHeight) return;
       MapLocation loc = visualMap.MapGrid[x, y];
       if (inSpotPhrase)
          loc.FactionGroupVisiblity |= GetFactionGroup();
       if (ExtraVisualProcessingFunc != null) ExtraVisualProcessingFunc(loc);
    }

    private bool inSpotPhrase;
    public void UpdateMapVisual()
    {
       inSpotPhrase = true;
       for (int r = 0; r < VisualRange; r++)
       {
          VisualCir.circle(objLoc.X, objLoc.Y, r);
       }
       inSpotPhrase = false;
       for (int r = VisualRange; r < WeaponRange; r++) VisualCir.circle(objLoc.X, objLoc.Y, r);
    }

    public int VisualRange;
    public int WeaponRange;
    public UnitCatagories UnitType;
    //public int Faction;
    public abstract float Life { get; }
    private MapObjectShape _shape;
    private PointLocation objLoc;
    //private PointLocation objOff;
    protected MapLocation[] _occupiedLocs;
    //where to move to for the engine to calculate
    //public MapLocation MoveToLoc { get; set; }
    public PointLocation GetObjectLocation()
    {
      return objLoc;
    }
    public void SetObjectLocation(PointLocation loc)
    {
       objLoc = loc;
    }

    //public PointLocation GetObjectShapeOffset()
    //{
    //  return objOff;
    //}

    public MapObjectShape GetObjectShape()
    {
      return _shape;
    }

    public MapLocation[] OccupiedLocations()
    {
      return _occupiedLocs;
    }

    //public abstract double CheckResistance(GenMapObject obj);
    //public abstract void MovingTo(PointLocation pt);
    public abstract int GetFactionGroup();


     /// <summary>
     /// Check if the place is hill or water.
     /// </summary>
     /// <param name="loc"></param>
     /// <returns></returns>
    public virtual bool CheckPlaceAbility(MapLocation loc)
    {
      if (loc.MapType == MapGeoTypes.MapGeoTypeClif
        || loc.MapType == MapGeoTypes.MapGeoTypeWater)
      {
        return false;
      }
      return true;
    }
  }
}
