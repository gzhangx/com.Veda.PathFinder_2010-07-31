﻿#pragma checksum "..\..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "90B8C8870607ACB2FB8E97C34DEB00D7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace com.Veda.PathFinder {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\MainWindow.xaml"
        internal System.Windows.Controls.Canvas mapCanv;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\MainWindow.xaml"
        internal System.Windows.Shapes.Rectangle trackblk;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\MainWindow.xaml"
        internal System.Windows.Controls.Button btnDraw;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/com.Veda.PathFinder;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.mapCanv = ((System.Windows.Controls.Canvas)(target));
            
            #line 11 "..\..\..\MainWindow.xaml"
            this.mapCanv.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.mapCanv_MouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\MainWindow.xaml"
            this.mapCanv.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.mapCanv_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\MainWindow.xaml"
            this.mapCanv.MouseMove += new System.Windows.Input.MouseEventHandler(this.mapCanv_MouseMove);
            
            #line default
            #line hidden
            return;
            case 2:
            this.trackblk = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 3:
            
            #line 16 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ButtonDraw_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 17 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_Start);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 18 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_End);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 19 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_Clean);
            
            #line default
            #line hidden
            return;
            case 7:
            this.btnDraw = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\..\MainWindow.xaml"
            this.btnDraw.Click += new System.Windows.RoutedEventHandler(this.btnDraw_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 21 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_AddTank);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 22 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_Load);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 23 "..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_Save);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
