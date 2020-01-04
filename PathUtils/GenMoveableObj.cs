using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Veda.PathFinder.PathUtils;
using com.Veda.PathFinder.Core;
using System.Windows;

namespace com.Veda.PathFinder.PathUtils
{
   public partial class GenMoveableObj : GenMapObject
   {
      public const float UNABLE_TO_MOVE_NUM = float.MaxValue;

      //public readonly RotateUtils Rotates = new RotateUtils();
      private float uiLocX;
      private float uiLocY;
      private float Speed = 2f;
      private DateTime lastMoveTime = DateTime.Now;
      private PointLocation MoveToLoc;
      private float LastMoveOverTime = 0;
      //public CirDraw VisualCir;
      public MoveEngineObjGroup MoveGroup;
      public event Action<PointLocation> MovingToEvent;

      public float GetUILocX() { return uiLocX; }
      public float GetUILocY() { return uiLocY; }
      public bool CanMoveToSqure(PointLocation to)
      {
         if (Math.Abs(uiLocX - to.X) < 1.1 && Math.Abs(uiLocY - to.Y) < 1.1) return true;
         return false;
      }

      public float GetSpeed() { return Speed; }

      public void MoveObj()
      {
         PointLocation MoveToLocFixed = this.MoveToLoc;
         if (MoveToLocFixed == null) return;
         float xDiff = MoveToLocFixed.X - uiLocX;
         float yDiff = MoveToLocFixed.Y - uiLocY;
         //Console.Write("to loc = " + MoveToLocFixed + " diff=" + xDiff.ToString("0.0") + "," + yDiff.ToString("0.0"));
         float timeDiff = (float)DateTime.Now.Subtract(lastMoveTime).TotalSeconds;
         lastMoveTime = DateTime.Now;
         float absXdiff = Math.Abs(xDiff);
         float absYdiff = Math.Abs(yDiff);
         if ((absXdiff > 0.1) && (absYdiff > 0.1))
         {
            float realSpeed = Speed / 1.4142f;
            float dist = realSpeed * timeDiff;
            if (dist > absXdiff)
            {
               LastMoveOverTime = (float)((dist - absXdiff) / realSpeed);
               uiLocX += xDiff;
            }
            else
            {
               uiLocX += Math.Sign(xDiff) * dist;
            }
            if (dist > absYdiff)
            {
               float yover = (float)((dist - absYdiff) / realSpeed);
               if (LastMoveOverTime < yover) LastMoveOverTime = yover;
               uiLocY += yDiff;
            }
            else
            {
               uiLocY += Math.Sign(yDiff) * dist;
            }
            //Rotates.SetRelativePosition(pt);
         }
         else if (absXdiff > 0.01)
         {
            float dist = Speed * timeDiff;
            if (dist > absXdiff)
            {
               LastMoveOverTime = ((dist - absXdiff) / Speed);
               //#if DEBUG
               //             Console.WriteLine("last over= " + LastMoveOverTime + " dist=" + dist + " absXdiff=" + absXdiff);
               //#endif
               dist = absXdiff;
            }
            //Point pt = Rotates.GetRelativePosition();
            uiLocX += Math.Sign(xDiff) * dist;
            //Rotates.SetRelativePosition(pt);
         }
         else if (absYdiff > 0.01)
         {
            float dist = Speed * timeDiff;
            if (dist > absYdiff)
            {
               LastMoveOverTime = ((dist - absYdiff) / Speed);
               dist = absYdiff;
            }
            //Console.WriteLine("DEBUG engine timediff=" + timeDiff + " lastMoveOvertime=" + LastMoveOverTime + " dist=" + dist);
            //Point pt = Rotates.GetRelativePosition();
            uiLocY += Math.Sign(yDiff) * dist;
            //Rotates.SetRelativePosition(pt);
         }
      }
      public void MovingTo(PointLocation toPt)
      {
         //#if DEBUG
         //       Console.WriteLine("debug moving to " + toPt);
         //#endif
         MoveToLoc = toPt;
         if (LastMoveOverTime > 0)
         {
            lastMoveTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, (int)LastMoveOverTime, (int)(LastMoveOverTime * 1000)));
            //#if DEBUG
            //          Console.WriteLine("debug lastMoveOverTime=" + LastMoveOverTime + " diff=" + DateTime.Now.Subtract(lastMoveTime).TotalSeconds);
            //#endif
            LastMoveOverTime = 0;
         }
         else
         {
            lastMoveTime = DateTime.Now;
         }
         if (MovingToEvent != null) MovingToEvent(toPt);
      }

      public override int GetFactionGroup()
      {
         return GameObj.FactionGroup;
      }
      public GenMoveableObj(IMoveableGameObj obj, MapObjectShape shape, PointLocation loc)
         : base(obj.Controler.GetMoveEngine().GetMap(), shape, loc)
      {
         //VisualCir = new CirDraw(VisualCheckFunc);         
         GameObj = obj;
         uiLocX = loc.X;
         uiLocY = loc.Y;
         //visualMap = GameObj.Controler.GetMoveEngine().GetMap();
      }

      //private PathMap visualMap;
      //private void VisualCheckFunc(int x, int y)
      //{
      //   visualMap.MapGrid[x, y].FactionVisiblity |= GameObj.Faction;
      //}
      public IMoveableGameObj GameObj { get; private set; }
      public override float Life { get { return GameObj.Life; } }
      private float AttackPower { get { return GameObj.AttackPower; } }
      //public PointLocation CurrentGoToLocation;
      public bool IsSameFaction(int fac)
      {
         //TODO: use a map instead.
         return GameObj.Controler.IsFactionAlly(GameObj.Faction, fac);
      }
      public float CheckResistance(GenMapObject obj)
      {
         if (obj == null)
         {
            return 0;
         }
         bool isSameFac = obj.GetFactionGroup() == GameObj.FactionGroup;
         if (obj.UnitType == UnitCatagories.Building)
         {
            if (isSameFac)
               return UNABLE_TO_MOVE_NUM;
            if (AttackPower == 0) return UNABLE_TO_MOVE_NUM;
            return obj.Life / AttackPower;
         }
         else
         {
            if (isSameFac)
            {
               //if (bsObj.GameObj.CurrentGoToLocation == null) return SettingSameFactUnitPassCost;
               //if (bsObj.GameObj.CurrentGoToLocation.IsSameLoc(GameObj.CurrentGoToLocation)) return 0;
               GenMoveableObj bsObj = obj as GenMoveableObj;
               MoveEngineObjGroup mgrp = bsObj.MoveGroup;
               if (mgrp != null && mgrp.MoveGroupId == MoveGroup.MoveGroupId) return MoveSettings.SettingSameFactSameMoveToUnitPassCost;
               return MoveSettings.SettingSameFactUnitPassCost;
            }
            else
            {
               if (AttackPower == 0) return UNABLE_TO_MOVE_NUM;
               return obj.Life / AttackPower;
            }
         }


      }
   }
}
