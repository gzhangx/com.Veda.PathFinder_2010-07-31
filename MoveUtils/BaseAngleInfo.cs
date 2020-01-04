using System;
using System.Windows;

namespace com.Veda.PathFinder.PathUtils
{
   public interface BaseAngleInfo
   {
      double ActualHeight { get; }      
      double ActualWidth { get; }
      double BaseAngle
      {
         get;
      }
      //UIElement BaseUIElement
      //{
      //   get;
      //}
      double PosX { get; }
      double PosY { get; }
   }
}
