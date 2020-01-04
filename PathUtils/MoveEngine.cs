using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.Veda.PathFinder.PathUtils
{
   public class MoveEngine
   {
      public static int MoveEngineGroupRecalcTime = 8; //seconds;
      public static int MoveEngineSleepTime = 50;
      private object MoveEngineStepObj = new object();
      public void StepMoveEngine()
      {
         lock (MoveEngineStepObj)
         {
            Monitor.PulseAll(MoveEngineStepObj);
         }
      }
      
      public int MoveEngineThreadCount = 2;
      private List<GenMoveableObj> allObjs = new List<GenMoveableObj>();
      private object addRemoveLock = new object();
      private PathMap map;
      private bool running = false;
      private List<MoveEngineWorker> workers = new List<MoveEngineWorker>();
      private Thread engineThread;
      public MoveEngine(PathMap pm)
      {
         map = pm;
         StartEngine();
      }
      public void SetMap(PathMap mp)
      {
         StopEngine();
         map = mp;
         StartEngine();
      }
      public PathMap GetMap()
      {
         return map;
      }
      public bool AddMoveableObj(GenMoveableObj obj)
      {
         if (!map.PlaceObjectOnMap(obj, null)) return false;
         lock (addRemoveLock)
         {
            allObjs.Add(obj);
         }
         return true;
      }
      public void RemoveMoveableObj(GenMoveableObj obj)
      {
         lock (addRemoveLock)
         {
            allObjs.Remove(obj);            
         }
         map.RemoveObjectFromMap(obj);
      }
      private void StartEngine()
      {         
         if (running) throw new Exception("std check failed");
         workers.Clear();
         for (int i = 0; i < MoveEngineThreadCount; i++)
            workers.Add(new MoveEngineWorker(map.MakeCloneForThread()));
         workers.ForEach(w => w.Start());
         running = true;
         engineThread = new Thread(new ThreadStart(doWork));
         engineThread.Start();
      }
      public void StopEngine()
      {
         running = false;
         StepMoveEngine();
         engineThread.Join();
      }

      private void doWork()
      {
         DateTime lastFactionVisiblityTime = DateTime.Now;
         while (running)
         {
            DateTime start = DateTime.Now;
            lock (addRemoveLock)
            {
               int workerPos = 0;
               foreach (var obj in allObjs)
               {
                  //if (obj.MoveGroup != null)
                  {
                     workers[workerPos].MoveObjs.Add(obj);
                     workerPos++;
                     if (workerPos >= workers.Count) workerPos = 0;
                  }
               }
            }

            if (DateTime.Now.Subtract(lastFactionVisiblityTime).TotalSeconds > 2)
            {
               lastFactionVisiblityTime = DateTime.Now;
               for (int j = 0; j < map.MapHeight; j++)
               {
                  for (int i = 0; i < map.MapWidth; i++)
                  {
                     map.MapGrid[i, j].FactionGroupVisiblity = 0;
                  }
               }
            }

#if DEBUG
            Console.WriteLine("Start worker" + workers[0].MoveObjs.Count);
#endif
            workers.ForEach(wkr => wkr.SetMoveObjs());
            workers.ForEach(wkr => wkr.WaitForFinish());
            
            lock (MoveEngineStepObj)
            {
               double sleep = MoveEngineSleepTime - DateTime.Now.Subtract(start).TotalMilliseconds;
#if DEBUG
               Console.WriteLine("Sleep time is " + sleep.ToString("0"));
#endif
               if (sleep > 0)
               {
                  if (running)
                     Monitor.Wait(MoveEngineStepObj, (int)sleep);
               }
            }
         }

         workers.ForEach(w => w.Stop());
      }
   }

   public class MoveEngineObjGroup
   {
      public PathMap Map;
      public PointLocation MoveToLoc;
      public GenMoveableObj OneOfGroupsObj;
      public bool IsReady() { return Map.Prepared; }
      public readonly object MapCalcLock = new object();
      private bool inCalculation = false;

      public readonly int MoveGroupId;
      private static int MoveGroupIdGen;
      private static object MoveGroupIdGenLock = new object();
      public MoveEngineObjGroup()
      {
         lock (MoveGroupIdGenLock)
         {
            unchecked
            {
               MoveGroupId = ++MoveGroupIdGen;
            }
         }
      }

      public DateTime LastCalcTime = DateTime.MinValue;
      public void ForceRecalc()
      {
         lock (MapCalcLock)
         {
            Map.Prepared = false;
         }
      }
      public void DoCalc()
      {
         lock (MapCalcLock)
         {
            if (IsReady()) return;
            if (inCalculation) return;
            inCalculation = true;
            Map.Prepared = false;
         }
         LastCalcTime = DateTime.Now;
         Map.PreparePathMap(Map.MapGrid[MoveToLoc.X, MoveToLoc.Y], OneOfGroupsObj);
         inCalculation = false;
      }
   }

   class MoveEngineWorker
   {
      public Thread worker;
      public readonly List<GenMoveableObj> MoveObjs = new List<GenMoveableObj>();
      private bool running = true;
      private object finishLock = new object();
      private PathMap map;
      public MoveEngineWorker(PathMap m)
      {
         map = m;
      }
      public void Start()
      {
         running = true;
         worker = new Thread(new ThreadStart(DoWork));
         worker.Start();
      }
      public void Stop()
      {
         running = false;
         SetMoveObjs();
         worker.Join();
      }
      public void SetMoveObjs()
      {
         //if (MoveObjs.Count != 0) throw new Exception("wrong move1");
         lock (finishLock)
         {
            //MoveObjs = objs;
            Monitor.Pulse(finishLock);
         }
      }

      public void WaitForFinish()
      {
         lock (finishLock)
         {
            while (MoveObjs.Count != 0) Monitor.Wait(finishLock);
         }
      }
      private void DoWork()
      {
         while (running)
         {
            lock (finishLock)
            {
               if (!running) break;
               if (MoveObjs.Count == 0)
                  Monitor.Wait(finishLock);
               DateTime start = DateTime.Now;
               Dictionary<int, MoveEngineObjGroup> idgroups = new Dictionary<int, MoveEngineObjGroup>();
               foreach (var obj in MoveObjs)
               {
                  MoveEngineObjGroup grp = obj.MoveGroup;
                  if (grp != null)
                  {
                     if (!idgroups.ContainsKey(grp.MoveGroupId)) idgroups.Add(grp.MoveGroupId, grp);
                     MoveToDst(obj);
                  }
                  obj.UpdateMapVisual();
               }
               MoveObjs.Clear();

               double totalMills = DateTime.Now.Subtract(start).TotalMilliseconds;
               if (totalMills < MoveEngine.MoveEngineSleepTime)
               {
                  List<MoveEngineObjGroup> groups = new List<MoveEngineObjGroup>();
                  foreach (var grp in idgroups.Values)
                  {
                     if (DateTime.Now.Subtract(grp.LastCalcTime).TotalSeconds > MoveEngine.MoveEngineGroupRecalcTime)
                     {
                        groups.Add(grp);
                     }
                  }
                  groups.Sort((a, b) =>
                  {
                     return a.LastCalcTime.CompareTo(b.LastCalcTime);
                  });

                  foreach (var grp in groups)
                  {
                     double usedMis = DateTime.Now.Subtract(start).TotalMilliseconds;
                     if (usedMis >= MoveEngine.MoveEngineSleepTime)
                     {
                        break;
                     }
                     grp.ForceRecalc();
                     grp.DoCalc();

                  }
               }
               Monitor.Pulse(finishLock);
            }
         }
      }

      private void MoveToDst(GenMoveableObj obj)
      {
         MoveEngineObjGroup mgrp = obj.MoveGroup;
         if (mgrp == null) return;
         if (!mgrp.IsReady())
         {
            mgrp.OneOfGroupsObj = obj;
            mgrp.DoCalc();
         }

         obj.MoveObj();
         lock (mgrp.MapCalcLock)
         {
            if (mgrp.IsReady())
            {
               List<MapRoutCalculateUnit> cus = new List<MapRoutCalculateUnit>();
               PointLocation objLoc = obj.GetObjectLocation();
               MapRoutCalculateUnit firstcu = obj.MoveGroup.Map.RetrivePathAfterPrep(objLoc.X, objLoc.Y);
               if (firstcu != null && firstcu.calcFromLocation != null)
               {
                  PointLocation cuFromloc = firstcu.calcFromLocation.Location.MapLoc;
                  if (obj.CanMoveToSqure(cuFromloc))
                  {
                     if (map.PlaceObjectOnMap(obj, cuFromloc)) return;
                  }
               }
               foreach (PathMap.MapPosCheckWeight wets in PathMap.posToCheck)
               {
                  MapRoutCalculateUnit cu = mgrp.Map.RetrivePathAfterPrep(wets.X + objLoc.X, wets.Y + objLoc.Y);
                  if (cu != null)
                  {
                     if (cu.calcFromLocation != null)
                     {
                        if (obj.CanMoveToSqure(cu.calcFromLocation.Location.MapLoc))
                        {
                           cus.Add(cu);
                        }
                     }
                     else if (cu.Location.MapLoc.IsSameLoc(mgrp.MoveToLoc))
                     {
                        if (obj.CanMoveToSqure(cu.Location.MapLoc))
                        {
                           cus.Add(cu);
                        }
                     }
                  }
               }
               cus.Sort((a, b) =>
               {
                  if (a.CurrentCost < b.CurrentCost) return -1;
                  if (a.CurrentCost > b.CurrentCost) return 1;
                  return 0;
               });
               double curCost = mgrp.Map.RetrivePathAfterPrep(objLoc.X, objLoc.Y).CurrentCost;
               foreach (MapRoutCalculateUnit cu in cus)
               {
                  if (cu.CurrentCost >= curCost) break;
                  if (map.PlaceObjectOnMap(obj, cu.Location.MapLoc)) break;
               }
            }
         }
      }
   }
}
