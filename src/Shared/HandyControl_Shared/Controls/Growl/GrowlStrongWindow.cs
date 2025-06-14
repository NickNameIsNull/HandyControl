using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Tools;
using HandyControl.Tools.Interop;

namespace HandyControl.Controls;

/// <summary>
/// 消息通知窗口 强壮扩展
/// </summary>
public sealed class GrowlStrongWindow : Window
{
    internal List<Panel> GrowlPanel { get; set; } = new();

    private readonly Grid _windowContent = new Grid();
    internal GrowlStrongWindow(GrowlShowPosition showPosition = GrowlShowPosition.Default)
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;

        CreatePanel(showPosition);
        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            IsInertiaEnabled = true,
            Content = _windowContent
        };
        this.Topmost = true;
    }

    /// <summary>
    /// 创建Panel
    /// </summary>
    /// <param name="showPosition">显示位置</param>
    /// <returns></returns>
    internal Panel CreatePanel(GrowlShowPosition showPosition)
    {
        VerticalAlignment growlPanelVerticalAlignment = VerticalAlignment.Top;
        HorizontalAlignment growlPanelHorizontalAlignment = HorizontalAlignment.Right;
        switch (showPosition)
        {
            case GrowlShowPosition.LeftTop:
                growlPanelVerticalAlignment = VerticalAlignment.Top;
                growlPanelHorizontalAlignment = HorizontalAlignment.Left;
                break;
            case GrowlShowPosition.LeftCenter:
                growlPanelVerticalAlignment = VerticalAlignment.Center;
                growlPanelHorizontalAlignment = HorizontalAlignment.Left;
                break;
            case GrowlShowPosition.LeftBottom:
                growlPanelVerticalAlignment = VerticalAlignment.Bottom;
                growlPanelHorizontalAlignment = HorizontalAlignment.Left;
                break;
            case GrowlShowPosition.Default:
            case GrowlShowPosition.RightTop:
                growlPanelVerticalAlignment = VerticalAlignment.Top;
                growlPanelHorizontalAlignment = HorizontalAlignment.Right;
                break;
            case GrowlShowPosition.RightCenter:
                growlPanelVerticalAlignment = VerticalAlignment.Center;
                growlPanelHorizontalAlignment = HorizontalAlignment.Right;
                break;
            case GrowlShowPosition.RightBottom:
                growlPanelVerticalAlignment = VerticalAlignment.Bottom;
                growlPanelHorizontalAlignment = HorizontalAlignment.Right;
                break;
            case GrowlShowPosition.CenterTop:
                growlPanelVerticalAlignment = VerticalAlignment.Top;
                growlPanelHorizontalAlignment = HorizontalAlignment.Center;
                break;
            case GrowlShowPosition.Center:
                growlPanelVerticalAlignment = VerticalAlignment.Center;
                growlPanelHorizontalAlignment = HorizontalAlignment.Center;
                break;
            case GrowlShowPosition.CenterBottom:
                growlPanelVerticalAlignment = VerticalAlignment.Bottom;
                growlPanelHorizontalAlignment = HorizontalAlignment.Right;
                break;
        }

        string panelName =
            $"WGrowlPanel_{showPosition.ToString()}";
        var panel = new StackPanel
        {
            Name = panelName,
            VerticalAlignment = growlPanelVerticalAlignment,
            HorizontalAlignment = growlPanelHorizontalAlignment,
        };
        if (panel.VerticalAlignment == VerticalAlignment.Bottom && panel.Margin.Bottom is 0 or Double.NaN)
        {
            panel.Margin = new Thickness(panel.Margin.Left, panel.Margin.Top, panel.Margin.Right, 10);
        }

        GrowlPanel.Add(panel);
        _windowContent.Children.Add(panel);
        return panel;
    }

    internal void Init()
    {
        var desktopWorkingArea = SystemParameters.WorkArea;
        Height = desktopWorkingArea.Height;
        Left = desktopWorkingArea.Right - Width;
        Top = 0;

        //Todo 调整在多屏下显示的位置
        //if (Application.Current?.MainWindow != null)
        //{
        //    var ownerWindow = Application.Current?.MainWindow;
        //    //获取当前活动窗体
        //    var activeWindow = WindowHelper.GetActiveWindow();
        //    if (activeWindow != null && activeWindow.GetType() != typeof(GrowlStrongWindow))
        //    {
        //        ownerWindow = activeWindow;
        //    }
        //    this.Owner = ownerWindow;
        //    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        //    var window = this.Owner;
        //    if (window != null)
        //    {
        //        this.Width = window.ActualWidth;
        //        this.Height = window.ActualHeight;
        //        this.MaxWidth = window.ActualWidth;
        //        this.Left = window.Left;
        //        this.Top= window.Top;
        //    }

        //    this.SizeToContent = SizeToContent.Manual;
        //    this.Topmost = true;
        //}
    }

    protected override void OnSourceInitialized(EventArgs e)
        => InteropMethods.IntDestroyMenu(this.GetHwndSource().CreateHandleRef());
}
