using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Veda.PathFinder.PathUtils;
using com.Veda.PathFinder.Core;
using System.IO;

namespace com.Veda.PathFinder.PathUtils
{
   public partial interface IFactionDB
   {
      MoveEngine GetMoveEngine();
   }
}
namespace com.Veda.PathFinder.Units
{
   public class GameUnitTags
   {
      public const string TAG_TANK = "TNK4";
      public const string TAG_ZLOT = "ZLT1";
   }
   public class BaseGameObj : IMoveableGameObj
   {
      public int DisplayOffsetX;
      public int DisplayOffsetY;
      public IMoveableGameObj NearstTarget;
      public readonly string SaveTag;
      public IFactionDB Controler { get; private set; }
      public int Faction { get; private set; }
      public float Life { get; set; }
      public float AttackPower { get; protected set; }
      public GenMoveableObj MapObj { get; private set; }
      public int ShapeId() { return MapObj.GetObjectShape().ShapeId; }
      public int FactionGroup { get; private set; }
      public BaseGameObj(IFactionDB controler, int faction,PointLocation pt, MapObjectShape shape, string savetag)
      {
         SaveTag = savetag;
         Controler = controler;
         Faction = faction;
         MapObj = new GenMoveableObj(this, shape, pt);
         MapObj.ExtraVisualProcessingFunc = VisualUpdateFunc;    
         FactionGroup = Controler.GetFactionGroup(Faction);
         DisplayOffsetX = MoveSettings.DisplaySqureSize * shape.ShapeWidth / 2;
         DisplayOffsetY = MoveSettings.DisplaySqureSize * shape.ShapeHeight / 2;
         //MapObj.Rotates.SetRelativePosition(new System.Windows.Point((int)(pt.X * MoveSettings.DisplaySqureSize + MapObj.DisplayOffsetX),
         //   (int)(pt.Y * MoveSettings.DisplaySqureSize + MapObj.DisplayOffsetY)));
      }

      private void VisualUpdateFunc(MapLocation loc)
      {
         if (NearstTarget == null)
         {
            GenMoveableObj mvobj = loc.CurObject;
            //warning: mvobj might have moved
            if (mvobj != null)
            {
               if (mvobj.GetFactionGroup() != FactionGroup)
               {
                  NearstTarget = mvobj.GameObj;
               }
            }
         }
      }
      
      public bool PlaceObjOnMap()
      {
         return Controler.GetMoveEngine().AddMoveableObj(MapObj);
      }
      public void Save(BinaryWriter bw)
      {
         bw.Write(SaveTag);
         bw.Write(this.MapObj.GetObjectLocation().X);
         bw.Write(this.MapObj.GetObjectLocation().Y);
         bw.Write(Faction);
         bw.Write(Life);
      }
   }
   public class Gen1by1MoveableGameObj : BaseGameObj
   {
      private readonly static MapObjectShape _oneShape = new MapObjectShape(1, 1);      
      public Gen1by1MoveableGameObj(IFactionDB controler, int faction, PointLocation pt, string saveTag)
         : base(controler, faction, pt, _oneShape, saveTag)
      {         
      }
   }
   public class Gen2by2MoveableGameObj : BaseGameObj
   {
      private readonly static MapObjectShape _twoShape = new MapObjectShape(2, 2);      
      public Gen2by2MoveableGameObj(IFactionDB controler, int faction, PointLocation pt, string saveTag)
         : base(controler, faction, pt, _twoShape, saveTag)
      {         
      }
   }
   public class Tank : Gen2by2MoveableGameObj
   {
      public Tank(GameControler controler, int faction, PointLocation pt)
         : base(controler, faction, pt, GameUnitTags.TAG_TANK)
      {
         this.MapObj.VisualRange = 5;
         this.MapObj.WeaponRange = 6;
         Life = MaxLife = 100f;
         AttackPower = 10;         
      }
      
#if DEBUG
      public string DebugName;
      public override string ToString()
      {
         return "Tank " + DebugName + " pos " + MapObj.GetObjectLocation().X + "," + MapObj.GetObjectLocation().Y;
      }
#endif
      public float MaxLife { get; private set; }
   }
   public class zlot : Gen1by1MoveableGameObj
   {
      public zlot(IFactionDB controler, int faction, PointLocation pt)
         : base(controler, faction, pt, GameUnitTags.TAG_ZLOT)
      {
         this.MapObj.WeaponRange = 1;
         this.MapObj.VisualRange = 5;

         Life = MaxLife = 100f;
         AttackPower = 2;
      }
#if DEBUG
      public string DebugName;
      public override string ToString()
      {
         return "zlot " + DebugName + " pos " + MapObj.GetObjectLocation().X + "," + MapObj.GetObjectLocation().Y;
      }
#endif
      public float MaxLife { get; private set; }      
   }
}
