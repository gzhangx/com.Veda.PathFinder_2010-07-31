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
using com.Veda.PathFinder.Units;
using com.Veda.PathFinder.Core;
using com.Veda.PathFinder.PathUtils;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace com.Veda.PathFinder.UnitPresenters
{
   /// <summary>
   /// Interaction logic for tkpsentr.xaml
   /// </summary>
   public partial class TankPresenter : UserControl
   {
      public Tank gobj;
      //public Storyboard story;
      //public PointAnimation anim;
      //private Point loc = new Point();


      //float speed;

      
      public TankPresenter(GameControler ctrl, int faction, PointLocation pt)
      {
         InitializeComponent();
         gobj = new Tank(ctrl, faction, pt);
         //story = this.Resources["myStoryboard"] as Storyboard;
         //Storyboard.SetTarget(story, this);
         //anim = story.Children[0] as PointAnimation;
         //loc.X = pt.X * MoveSettings.DisplaySqureSize;
         //loc.Y = pt.Y * MoveSettings.DisplaySqureSize;
         //speed = gobj.MapObj.GetSpeed()*MoveSettings.DisplaySqureSize;
         UpdatePos();
         gobj.MapObj.MovingToEvent += new Action<PointLocation>(MapObj_MovingToEvent);

         initPositionUpdate();
      }

      private void initPositionUpdate()
      {
         DispatcherTimer dp = new DispatcherTimer();
         dp.Interval = new TimeSpan(0, 0, 0, 0, 50);
         dp.Tick += new EventHandler(dp_Tick);
         dp.Start();
      }
      public TankPresenter(Tank obj)
      {
         InitializeComponent();
         gobj = obj;
         UpdatePos();
         initPositionUpdate();
      }

      void dp_Tick(object sender, EventArgs e)
      {
         UpdatePos();
      }

      void MapObj_MovingToEvent(PointLocation obj)
      {
         lastUpdateTime = DateTime.Now;
         //Dispatcher.BeginInvoke(new Action(() =>
         //{
         //   story.Pause();
         //   anim.From = new Point
         //   {
         //      X = Canvas.GetLeft(this),
         //      Y = Canvas.GetTop(this),
         //   };
         //   anim.To = new Point
         //   {
         //      X = obj.X * MoveSettings.DisplaySqureSize,
         //      Y = obj.Y * MoveSettings.DisplaySqureSize,
         //   };
         //   anim.Duration = new TimeSpan(0, 0, 0, 0, 500);
         //   story.Begin();
         //}), null);
      }
      DateTime lastUpdateTime = DateTime.Now;
      public void UpdatePos()
      {
         //float tox = gobj.MapObj.GetUILocX() * MoveSettings.DisplaySqureSize;
         //float toy = gobj.MapObj.GetUILocY() * MoveSettings.DisplaySqureSize;
         //float diffx = tox - (float)loc.X;
         //float diffy = toy - (float)loc.Y;

         //float updateMs = (float)DateTime.Now.Subtract(lastUpdateTime).TotalMilliseconds;
         //lastUpdateTime = DateTime.Now;
         //Console.WriteLine("Update " + updateMs+" " + (updateMs / 1000));
         //float speed50ms = speed * updateMs/1000;
         //if (Math.Abs(diffx) > (speed50ms * 2 / 3))
         //{
         //   loc.X += Math.Sign(diffx) * speed50ms;
         //   if (Math.Abs(diffx) > speed50ms * 1.5)
         //   {
         //      loc.X += Math.Sign(diffx) * speed50ms / 3;
         //   }
         //}
         //if (Math.Abs(diffy) > speed50ms * 2 / 3)
         //{
         //   loc.Y += Math.Sign(diffy) * speed50ms;
         //   if (Math.Abs(diffy) > speed50ms * 1.5)
         //   {
         //      loc.Y += Math.Sign(diffy) * speed50ms / 3;
         //   }
         //}
         //Console.WriteLine("diff x = " + diffx + "/" + speed50ms + " diffy = " + diffy);
         //Canvas.SetLeft(this,loc.X);
         //Canvas.SetTop(this, loc.Y);
         Canvas.SetLeft(this, gobj.MapObj.GetUILocX() * MoveSettings.DisplaySqureSize);
         Canvas.SetTop(this, gobj.MapObj.GetUILocY() * MoveSettings.DisplaySqureSize);
      }
   }
}
