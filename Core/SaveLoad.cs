using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.Veda.PathFinder.PathUtils;
using com.Veda.PathFinder.Units;
using System.IO;

namespace com.Veda.PathFinder.Core
{
   public class SaveLoad
   {
      private const string FILETAG = "TOSSSAVEVOL";
      public static void Save(string fileName, PathMap map, List<BaseGameObj> units)
      {
         using (BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Create)))
         {
            bw.Write(FILETAG);
            bw.Write(map.MapWidth);
            bw.Write(map.MapHeight);
            for (int h = 0; h < map.MapHeight; h++)
            {
               for (int w = 0; w < map.MapWidth; w++)
               {
                  bw.Write((byte)map.MapGrid[w, h].MapType);
               }
            }
            foreach (BaseGameObj unit in units)
            {
               unit.Save(bw);
            }
         }
      }

      public static List<BaseGameObj> Load(string fileName, GameControler controler)
      {
         List<BaseGameObj> items = new List<BaseGameObj>();
         using (BinaryReader br = new BinaryReader(File.OpenRead(fileName)))
         {
            string tag = br.ReadString();
            if (tag != FILETAG) throw new Exception("bad tag");
            int mapWidth = br.ReadInt32();
            int mapHeight = br.ReadInt32();
            MapLocation[,] locs = new MapLocation[mapWidth, mapHeight];

            for (int j = 0; j < mapHeight; j++)
            {
               for (int i = 0; i < mapWidth; i++)
               {
                  locs[i, j] = new MapLocation(i, j)
                  {
                     MapType = (MapGeoTypes)br.ReadByte(),
                  };
               }
            }

            PathMap map = new PathMap(locs, null);
            controler.GetMoveEngine().SetMap(map);

            while (true)
            {
               try
               {
                  tag = br.ReadString();
               }
               catch
               {
                  break;
               }
               PointLocation pt = new PointLocation(br.ReadInt32(), br.ReadInt32());
               int faction = br.ReadInt32();
               float life = br.ReadSingle();
               BaseGameObj itm = null;
               switch (tag)
               {
                  case GameUnitTags.TAG_TANK:
                     itm = new Tank(controler, faction, pt);
                     itm.Life = life;                     
                     break;
                  case GameUnitTags.TAG_ZLOT:
                     itm = new zlot(controler, faction, pt);
                     itm.Life = life;
                     break;
               }
               if (itm != null && itm.PlaceObjOnMap())
               {
                  items.Add(itm);
               }
            }
         }
         return items;
      }
   }
}
