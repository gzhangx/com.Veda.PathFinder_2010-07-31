using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.Veda.PathFinder.PathUtils;
using com.Veda.PathFinder.Units;
using com.Veda.PathFinder.Core;
using System.Threading;
using System.Windows.Threading;
using com.Veda.PathFinder.UnitPresenters;

namespace com.Veda.PathFinder
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    PathMap map;
    MapLocation from;
    MapLocation to;
    private int mapWidth = 250;
    private int mapHeight = 250;

    const int BlockWidth = MoveSettings.DisplaySqureSize;
    GameControler controler;

    

    List<TankPresenter> prsenters = new List<TankPresenter>();
    //MoveEngine engine;
    //TextBlock[,] mapCounts;
    MoveEngineObjGroup curgrptoDebug = null;
    private int ClearnFrom = 0;

    private int debugtrackx = 10;
    public MainWindow()
    {
      InitializeComponent();
      
      initMap();

      controler = new GameControler(map);
      controler.SetFactionGroup(0, 0);
      
      this.Closing += new System.ComponentModel.CancelEventHandler((ce, cg) =>
      {
         controler.StopEngine();
      });
      mapCanv.Width = mapWidth * BlockWidth;
      mapCanv.Height = mapWidth * BlockWidth;
#if DOSHOWBLKTXT
      mapCounts = new TextBlock[mapWidth, mapHeight];
      for (int i = 0; i < mapWidth; i++)
      {
         for (int j = 0; j < mapHeight; j++)
         {
            TextBlock txt = new TextBlock();
            txt.FontSize = 6;
            txt.Text = "0";
            mapCounts[i, j] = txt;
            mapCanv.Children.Add(txt);
            Canvas.SetLeft(txt, i * BlockWidth);
            Canvas.SetTop(txt, j * BlockWidth);
         }
      }
#endif
      ClearnFrom = mapCanv.Children.Count;
      DispatcherTimer tmr = new DispatcherTimer();
      tmr.Interval = new TimeSpan(0, 0, 0, 0, 50);
      tmr.Tick += new EventHandler((ooo, eee) =>
      {
         DoRedraw();
         //Canvas.SetLeft(trackblk, debugtrackx);
      });
      tmr.Start();

      //new Thread(new ThreadStart(() =>
      //{
      //   for (int i = 0; i < 100; i++)
      //   {
      //      Thread.Sleep(10);
      //      debugtrackx += 2;
      //   }
      //})).Start();
    }

    private void initMap()
    {
      MapLocation[,] locs = new MapLocation[mapWidth, mapHeight];
      for (int i = 0; i < mapWidth; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          locs[i, j] = new MapLocation(i, j)
          {
            MapType = MapGeoTypes.MapGeoTypeLand,
          };
        }
      }
      //for (int i = 1; i <= 3; i++)
      //{
      //  locs[3, i].MapType = MapGeoTypes.MapGeoTypeClif;
      //}
      //locs[2, 1].MapType = MapGeoTypes.MapGeoTypeWater;
      //locs[2, 3].MapType = MapGeoTypes.MapGeoTypeWater;
      map = new PathMap(locs, null);
    }
    private void btnDraw_Click(object sender, RoutedEventArgs e)
    {
       map.PreparePathMap(to, from.CurObject);
       MapRoutCalculateUnit ut = map.RetrivePathAfterPrep(from.CurObject.GetObjectLocation().X,
          from.CurObject.GetObjectLocation().Y);
      Console.WriteLine(ut.Location.MapLoc);
      List<MapRoutCalculateUnit> lst = new List<MapRoutCalculateUnit>();
      lst.Add(ut);
      while (ut != null)
      {
        if (ut.calcFromLocation != null)
          lst.Add(ut.calcFromLocation);
        ut = ut.calcFromLocation;
      }
      lst.Reverse();
      foreach (MapRoutCalculateUnit uu in lst)
      {
        Console.WriteLine(uu);
        DrawRct(uu.Location.MapLoc.X, uu.Location.MapLoc.Y, Brushes.Red);
      }
    }

    TestState state = TestState.StateDraw;
    bool isMouseDown = false;
    PointLocation TranslateMap(MouseEventArgs e)
    {
      Point pt = e.GetPosition(mapCanv);
      int x = (int)(pt.X / BlockWidth);
      int y = (int)(pt.Y / BlockWidth);
      if (x < 0 || x >= mapWidth) return null;
      if (y < 0 || y >= mapHeight) return null;
      return new PointLocation(x,y);      
    }

    private int debugTankNumber = 0;
    private void mapCanv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      isMouseDown = false;
      PointLocation pt = TranslateMap(e);
      if (pt == null) return;
      GenMoveableObj mvobj;
      switch (state)
      {
        case TestState.StateEndPos:
          to = map.MapGrid[pt.X, pt.Y];
          MoveEngineObjGroup grp = new MoveEngineObjGroup();
          curgrptoDebug = grp;
          grp.Map = map.MakeCloneForThread();
          grp.MoveToLoc = pt;
          //DrawBlock(pt.X, pt.Y, Brushes.Red);
          Canvas.SetLeft(trackblk, pt.X * MoveSettings.DisplaySqureSize);
          Canvas.SetTop(trackblk, pt.Y * MoveSettings.DisplaySqureSize);
          prsenters.ForEach(mv => mv.gobj.MapObj.MoveGroup = grp);
          //allMovables.ForEach(mv => mv.MapObj.MoveGroup = grp);
          break;
        case TestState.StateStartPos:
          from = map.MapGrid[pt.X, pt.Y];
          DrawBlock(pt.X, pt.Y, Brushes.Blue);
          {
            mvobj = new Tank(controler, 0, pt).MapObj;
            mvobj.SetObjectLocation(pt);
            map.PlaceObjectOnMap(mvobj, null);
          }
          
          break;
        case TestState.StateDrawTank:
          TankPresenter psnt = new TankPresenter(controler, 0, pt);
          mvobj = psnt.gobj.MapObj;
          //Tank tank = new Tank(controler, 0, pt);
          //tank.DebugName = (debugTankNumber++).ToString();
          //mvobj = tank.MapObj;
            
            if (psnt.gobj.PlaceObjOnMap())
            {
              //allMovables.Add(tank);
              //DrawBlock(pt.X, pt.Y, Brushes.Green);
               prsenters.Add(psnt);
               mapCanv.Children.Add(psnt);
            }
            //engine.AddMoveableObj(tank.MapObj);
          break;
      }
    }

    private void DrawBlock(int x, int y, Brush clr)
    {
      for (int i = 0; i < 2; i++)
      {
        for (int j = 0; j < 2; j++)
        {
          //from = map.MapGrid[x + i, y + j];
          DrawRct(x + i, y + j, clr);
        }
      }
    }
    private void mapCanv_MouseMove(object sender, MouseEventArgs e)
    {
      if (!isMouseDown) return;
      PointLocation pt = TranslateMap(e);
      if (pt != null)
      {
        if (state == TestState.StateDraw)
        {
          MapLocation loc =  map.MapGrid[pt.X, pt.Y];
          if (loc.MapType != MapGeoTypes.MapGeoTypeClif)
          {
            loc.MapType = MapGeoTypes.MapGeoTypeClif;
            DrawRct(pt.X, pt.Y, Brushes.Black);
          }
        }
      }
    }

    private void DrawRct(int x, int y, Brush clr)
    {
      Rectangle rct = new Rectangle
      {
         Width = BlockWidth,
         Height = BlockWidth,
      };
      rct.Fill = clr;
      rct.Opacity = 0.5;
      Canvas.SetLeft(rct, x * BlockWidth);
      Canvas.SetTop(rct, y * BlockWidth);
      mapCanv.Children.Add(rct);
    }

    private void mapCanv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      isMouseDown = true;
    }



    private void Button_Click_Start(object sender, RoutedEventArgs e)
    {
      state = TestState.StateStartPos;
    }

    private void Button_Click_End(object sender, RoutedEventArgs e)
    {
      state = TestState.StateEndPos;
    }

    private void ButtonDraw_Click(object sender, RoutedEventArgs e)
    {
      state = TestState.StateDraw;
    }


    private void CleanMapConv()
    {
       while (mapCanv.Children.Count > ClearnFrom)
       {
          mapCanv.Children.RemoveAt(mapCanv.Children.Count - 1);
       }
    }
    private void Button_Click_Clean(object sender, RoutedEventArgs e)
    {
      //allMovables.Clear();
      state = TestState.StateDraw;
      CleanMapConv();
      for (int i = 0; i < mapWidth; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          map.MapGrid[i, j].
            MapType = MapGeoTypes.MapGeoTypeLand;
          map.MapGrid[i, j].CurObject = null;
        }
      }
    }


    private void DoRedraw()
    {
      
      // CleanMapConv();
      //for (int i = 0; i < mapWidth; i++)
      //{
      //  for (int j = 0; j < mapHeight; j++)
      //  {
      //    MapLocation mloc = map.MapGrid[i, j];
      //    if (mloc.MapType == MapGeoTypes.MapGeoTypeClif)
      //    {
      //      DrawRct(i, j, Brushes.Black);
      //    }
      //    if (mloc.CurObject != null)
      //    {
      //      DrawRct(i, j, Brushes.Green);
      //    }
      //  }
      //}

      // prsenters.ForEach(p => {  p.UpdatePos(); });
      //if (to != null)
      //{
      //  DrawBlock(to.MapLoc.X, to.MapLoc.Y, Brushes.Red);
      //}
    }


    private void Button_Click_AddTank(object sender, RoutedEventArgs e)
    {
      state = TestState.StateDrawTank;
      
    }

    private void Button_Click_Move(object sender, RoutedEventArgs e)
    {
       //MoveEngine.MoveEngineSleepTime = 1000 * 100;
       //engine.StepMoveEngine();
#if DOSHOWBLKTXT
       if (curgrptoDebug != null && curgrptoDebug.IsReady())
       {
          for (int i = 0; i < mapWidth; i++)
          {
             for (int j = 0; j < mapHeight; j++)
             {
                MapRoutCalculateUnit cu = curgrptoDebug.Map.RetrivePathAfterPrep(i, j);
                mapCounts[i, j].Text = cu.CurrentCost.ToString("0");

             }
          }
       }
#endif
    }

    private void Button_Click_Load(object sender, RoutedEventArgs e)
    {
       List<BaseGameObj> allMovables = SaveLoad.Load(@"c:\temp\test.sav", controler);
       allMovables.ForEach(mv =>
       {
          prsenters.Add(new TankPresenter(mv as Tank));
       });
       prsenters.ForEach(p => mapCanv.Children.Add(p));
       map = controler.GetMoveEngine().GetMap();
       for (int i = 0; i < mapWidth; i++)
       {
          for (int j = 0; j < mapHeight; j++)
          {
             MapLocation loc = map.MapGrid[i, j];
             if (loc.MapType == MapGeoTypes.MapGeoTypeClif)
             {
                DrawRct(i, j, Brushes.Black);
             }
          }
       }
    }

    private void Button_Click_Save(object sender, RoutedEventArgs e)
    {
       List<BaseGameObj> allMovables = new List<BaseGameObj>();
       prsenters.ForEach(p => allMovables.Add(p.gobj));
       SaveLoad.Save(@"c:\temp\test.sav", map, allMovables);
    }

  }


}
