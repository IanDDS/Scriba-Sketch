﻿#pragma checksum "C:\Users\Ian C\Desktop\Heart-rate-monitor-UWP--ScribaDraw\Heart-rate-monitor-UWP--master\HeartBeat\HeartBeat\Controls\ChartWin2DControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EC0BFE9FD36EDC6EBC4432A18AFD9599"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HeartBeat.Controls
{
    partial class ChartWin2DControl : 
        global::Windows.UI.Xaml.Controls.UserControl, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                {
                    this.uc = (global::Windows.UI.Xaml.Controls.UserControl)(target);
                }
                break;
            case 2:
                {
                    this.chartGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                    #line 13 "..\..\..\Controls\ChartWin2DControl.xaml"
                    ((global::Windows.UI.Xaml.Controls.Grid)this.chartGrid).SizeChanged += this.chartGrid_SizeChanged;
                    #line default
                }
                break;
            case 3:
                {
                    this.ChartWin2DCanvas = (global::Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl)(target);
                    #line 14 "..\..\..\Controls\ChartWin2DControl.xaml"
                    ((global::Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl)this.ChartWin2DCanvas).Draw += this.ChartWin2DCanvas_Draw;
                    #line default
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 14.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

