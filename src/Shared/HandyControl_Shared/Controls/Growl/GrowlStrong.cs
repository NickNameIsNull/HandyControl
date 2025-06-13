using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HandyControl.Data;
using HandyControl.Interactivity;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using HandyControl.Tools.Extension;

namespace HandyControl.Controls;

/// <summary>
///     消息提醒 强壮扩展
/// </summary>
[TemplatePart(Name = ElementPanelMore, Type = typeof(Panel))]
[TemplatePart(Name = ElementGridMain, Type = typeof(Grid))]
[TemplatePart(Name = ElementButtonClose, Type = typeof(Button))]
public class GrowlStrong : Growl
{
    private const string ElementPanelMore = "PART_PanelMore";
    private const string ElementGridMain = "PART_GridMain";
    private const string ElementButtonClose = "PART_ButtonClose";
    private const int MinWaitTime = 2;

    public new static readonly DependencyProperty GrowlParentProperty = DependencyProperty.RegisterAttached(
        "GrowlParent", typeof(bool), typeof(Growl), new PropertyMetadata(ValueBoxes.FalseBox, (o, args) =>
        {
            if ((bool) args.NewValue && o is Panel panel)
            {
                SetGrowlPanel(panel);
            }
        }));
    private static readonly DependencyProperty IsCreatedAutomaticallyProperty = DependencyProperty.RegisterAttached(
        "IsCreatedAutomatically", typeof(bool), typeof(Growl), new PropertyMetadata(ValueBoxes.FalseBox));
    private static GrowlStrongWindow GrowlStrongWindow;
    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly Dictionary<string, Panel> PanelDic = new();

    private Panel _panelMore;
    private Grid _gridMain;
    private Button _buttonClose;
    private bool _showCloseButton;
    private bool _staysOpen;
    private int _waitTime = 6;

    /// <summary>
    ///     计数
    /// </summary>
    private int _tickCount;

    /// <summary>
    ///     关闭计时器
    /// </summary>
    private DispatcherTimer _timerClose;

    /// <summary>
    ///     消息容器
    /// </summary>
    public new static List<Panel> GrowlPanel { get; set; } = new();

    private Func<bool, bool> ActionBeforeClose { get; init; }


    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        _buttonClose.Show(_showCloseButton);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        _buttonClose.Collapse();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _panelMore = GetTemplateChild(ElementPanelMore) as Panel;
        _gridMain = GetTemplateChild(ElementGridMain) as Grid;
        _buttonClose = GetTemplateChild(ElementButtonClose) as Button;

        CheckNull();
        Update();
    }

    private void CheckNull()
    {
        if (_panelMore == null || _gridMain == null || _buttonClose == null) throw new Exception();
    }

    private static void OnTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel)
        {
            if (e.NewValue == null)
            {
                Unregister(panel);
            }
            else
            {
                Register(e.NewValue.ToString(), panel);
            }
        }
    }


    #region 显示遮罩 - 依赖属性
    /// <summary>
    /// 显示遮罩 - 定义依赖属性 - 属性
    /// </summary>          
    public bool ShowMask
    {
        get { return (bool) GetValue(ShowMaskProperty); }
        set { SetValue(ShowMaskProperty, value); }
    }

    /// <summary>
    /// 显示遮罩 - 注册依赖属性 
    /// <para>使用DependencyProperty作为MyProperty的后台存储。这支持动画、样式、绑定等。</para>
    /// </summary>
    public static readonly DependencyProperty ShowMaskProperty =
        DependencyProperty.Register(nameof(ShowMask), typeof(bool), typeof(Growl), new PropertyMetadata(false, OnShowMaskPropertyChanged));

    /// <summary>
    /// 当属性 显示遮罩 出现变更时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void OnShowMaskPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

    }
    #endregion 显示遮罩 - 依赖属性 

    private static void SetIsCreatedAutomatically(DependencyObject element, bool value) => element.SetValue(IsCreatedAutomaticallyProperty, ValueBoxes.BooleanBox(value));

    private static bool GetIsCreatedAutomatically(DependencyObject element) => (bool) element.GetValue(IsCreatedAutomaticallyProperty);

    /// <summary>
    ///     开始计时器
    /// </summary>
    private void StartTimer()
    {
        _timerClose = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timerClose.Tick += delegate
        {
            if (IsMouseOver)
            {
                _tickCount = 0;
                return;
            }

            _tickCount++;
            if (_tickCount >= _waitTime)
            {
                Close(true);
            }
        };
        _timerClose.Start();
    }

    /// <summary>
    ///     消息容器
    /// </summary>
    /// <param name="panel"></param>
    private static void SetGrowlPanel(Panel panel)
    {
        if (GrowlPanel.All(p => p?.Name != panel.Name))
        {
            //列表中不存在，创建
            GrowlPanel.Add(panel);
        }
        else
        {
            for (int i = 0; i < GrowlPanel.Count; i++)
            {
                if (GrowlPanel[i]?.Name == panel.Name)
                {
                    GrowlPanel[i] = panel;
                }
            }
        }
        InitGrowlPanel(panel);
    }

    private static void InitGrowlPanel(Panel panel)
    {
        if (panel == null) return;

        var menuItem = new MenuItem();
        LangProvider.SetLang(menuItem, HeaderedItemsControl.HeaderProperty, LangKeys.Clear);

        menuItem.Click += (s, e) =>
        {
            foreach (var item in panel.Children.OfType<GrowlStrong>())
            {
                item.Close(false);
            }
        };
        panel.ContextMenu = new ContextMenu
        {
            Items =
            {
                menuItem
            }
        };

        PanelElement.SetFluidMoveBehavior(panel, ResourceHelper.GetResourceInternal<FluidMoveBehavior>(ResourceToken.BehaviorXY400));
    }

    private void Update()
    {
        if (DesignerHelper.IsInDesignMode) return;

        if (Type == InfoType.Ask)
        {
            _panelMore.IsEnabled = true;
            _panelMore.Show();
        }

        var transform = new TranslateTransform
        {
            X = FlowDirection == FlowDirection.LeftToRight ? MaxWidth : -MaxWidth
        };
        _gridMain.RenderTransform = transform;
        transform.BeginAnimation(TranslateTransform.XProperty, AnimationHelper.CreateAnimation(0));
        if (!_staysOpen) StartTimer();
    }

    private static void ShowInternal(Panel panel, UIElement growl)
    {
        if (panel is null)
        {
            return;
        }

        if (GetShowMode(panel) == GrowlShowMode.Prepend)
        {
            panel.Children.Insert(0, growl);
        }
        else
        {
            panel.Children.Add(growl);
        }
    }

    private static void ShowGlobal(GrowlStrongInfo growlInfo)
    {
        Application.Current.Dispatcher?.Invoke(
#if NET40
            new Action(
#endif
                () =>
                {
                    if (GrowlStrongWindow == null)
                    {
                        GrowlStrongWindow = new GrowlStrongWindow();
                        GrowlStrongWindow.Show();
                        InitGrowlPanel(GrowlStrongWindow.GrowlPanel.FirstOrDefault());
                        GrowlStrongWindow.Init();
                    }

                    GrowlStrongWindow.Show(true);

                    var ctl = new GrowlStrong
                    {
                        Message = growlInfo.Message,
                        Time = DateTime.Now,
                        Icon = ResourceHelper.GetResource<Geometry>(growlInfo.IconKey) ?? growlInfo.Icon,
                        IconBrush = ResourceHelper.GetResource<Brush>(growlInfo.IconBrushKey) ?? growlInfo.IconBrush,
                        _showCloseButton = growlInfo.ShowCloseButton,
                        ActionBeforeClose = growlInfo.ActionBeforeClose,
                        _staysOpen = growlInfo.StaysOpen,
                        ShowDateTime = growlInfo.ShowDateTime,
                        ConfirmStr = growlInfo.ConfirmStr,
                        CancelStr = growlInfo.CancelStr,
                        Type = growlInfo.Type,
                        _waitTime = Math.Max(growlInfo.WaitTime, MinWaitTime),
                        FlowDirection = growlInfo.FlowDirection,
                        ShowMask = growlInfo.ShowMask,
                    };
                    string panelName = $"WGrowlPanel_{growlInfo.ShowPosition.ToString()}";
                    var growlPanel = GrowlStrongWindow.GrowlPanel.FirstOrDefault(p => p.Name == panelName);
                    if (growlPanel == null)
                    {
                        growlPanel = GrowlStrongWindow.CreatePanel(growlInfo.ShowPosition);
                        InitGrowlPanel(growlPanel);
                    }
                    ShowInternal(growlPanel, ctl);
                }
#if NET40
            )
#endif
        );
    }

    /// <summary>
    ///     显示信息
    /// </summary>
    /// <param name="growlInfo"></param>
    private static void Show(GrowlStrongInfo growlInfo)
    {
        (Application.Current.Dispatcher ?? growlInfo.Dispatcher)?.Invoke(
#if NET40
            new Action(
#endif
                () =>
                {
                    var ctl = new GrowlStrong()
                    {
                        Message = growlInfo.Message,
                        Time = DateTime.Now,
                        Icon = ResourceHelper.GetResource<Geometry>(growlInfo.IconKey) ?? growlInfo.Icon,
                        IconBrush = ResourceHelper.GetResource<Brush>(growlInfo.IconBrushKey) ?? growlInfo.IconBrush,
                        _showCloseButton = growlInfo.ShowCloseButton,
                        ActionBeforeClose = growlInfo.ActionBeforeClose,
                        _staysOpen = growlInfo.StaysOpen,
                        ShowDateTime = growlInfo.ShowDateTime,
                        ConfirmStr = growlInfo.ConfirmStr,
                        CancelStr = growlInfo.CancelStr,
                        Type = growlInfo.Type,
                        _waitTime = Math.Max(growlInfo.WaitTime, MinWaitTime),
                        FlowDirection = growlInfo.FlowDirection,
                        ShowMask = growlInfo.ShowMask,
                    };

                    if (!string.IsNullOrEmpty(growlInfo.Token))
                    {
                        if (PanelDic.TryGetValue(growlInfo.Token, out var panel))
                        {
                            ShowInternal(panel, ctl);
                        }
                    }
                    else
                    {
                        // GrowlPanel is null, we create it automatically
                        string panelName = $"GrowlPanel_{growlInfo.ShowPosition.ToString()}";
                        Panel growlPanel = GrowlPanel.FirstOrDefault(p => p?.Name == panelName);
                        if (growlPanel == null)
                        {
                            growlPanel = CreateDefaultPanel(growlInfo.ShowPosition);
                            GrowlPanel.Add(growlPanel);
                        }
                        ShowInternal(growlPanel, ctl);
                    }
                }
#if NET40
            )
#endif
        );
    }

    private static Panel CreateDefaultPanel(GrowlShowPosition showPosition = GrowlShowPosition.Default)
    {
        FrameworkElement element = WindowHelper.GetActiveWindow();
        var decorator = VisualHelper.GetChild<AdornerDecorator>(element);

        if (decorator != null)
        {
            var layer = decorator.AdornerLayer;
            if (layer != null)
            {
                var panel = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Top
                };

                InitGrowlPanel(panel);
                SetIsCreatedAutomatically(panel, true);

                var scrollViewer = new ScrollViewer
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    IsInertiaEnabled = true,
                    IsPenetrating = true,
                    Content = panel
                };

                var container = new AdornerContainer(layer)
                {
                    Child = scrollViewer
                };

                layer.Add(container);

                return panel;
            }
        }

        return null;
    }

    private static void RemoveDefaultPanel(Panel panel)
    {
        FrameworkElement element = WindowHelper.GetActiveWindow();
        var decorator = VisualHelper.GetChild<AdornerDecorator>(element);

        if (decorator != null)
        {
            var layer = decorator.AdornerLayer;
            var adorner = VisualHelper.GetParent<Adorner>(panel);

            if (adorner != null)
            {
                layer?.Remove(adorner);
            }
        }
    }

    private static void InitGrowlStrongInfo(ref GrowlStrongInfo growlInfo, InfoType infoType)
    {
        if (growlInfo == null) throw new ArgumentNullException(nameof(growlInfo));
        growlInfo.Type = infoType;

        switch (infoType)
        {
            case InfoType.Success:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.SuccessGeometry;
                    growlInfo.IconBrushKey = ResourceToken.SuccessBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.SuccessGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.SuccessBrush;
                }
                break;
            case InfoType.Info:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.InfoGeometry;
                    growlInfo.IconBrushKey = ResourceToken.InfoBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.InfoGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.InfoBrush;
                }
                break;
            case InfoType.Warning:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.WarningGeometry;
                    growlInfo.IconBrushKey = ResourceToken.WarningBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.WarningGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.WarningBrush;
                }
                break;
            case InfoType.Error:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.ErrorGeometry;
                    growlInfo.IconBrushKey = ResourceToken.DangerBrush;
                    growlInfo.StaysOpen = true;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.ErrorGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.DangerBrush;
                }
                break;
            case InfoType.Fatal:
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.FatalGeometry;
                    growlInfo.IconBrushKey = ResourceToken.PrimaryTextBrush;
                    growlInfo.StaysOpen = true;
                    growlInfo.ShowCloseButton = false;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.FatalGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.PrimaryTextBrush;
                }
                break;
            case InfoType.Ask:
                growlInfo.StaysOpen = true;
                growlInfo.ShowCloseButton = false;
                if (!growlInfo.IsCustom)
                {
                    growlInfo.IconKey = ResourceToken.AskGeometry;
                    growlInfo.IconBrushKey = ResourceToken.AccentBrush;
                }
                else
                {
                    growlInfo.IconKey ??= ResourceToken.AskGeometry;
                    growlInfo.IconBrushKey ??= ResourceToken.AccentBrush;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(infoType), infoType, null);
        }
    }

    #region 健壮的弹出方法
    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    public new static void Success(string message, string token = "") => Success(new GrowlStrongInfo
    {
        Message = message,
        Token = token
    });

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void Success(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Success);
        Show(growlStrongInfo);
    }

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="message"></param>
    public new static void SuccessGlobal(string message) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = message
    });

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void SuccessGlobal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Success);
        ShowGlobal(growlStrongInfo);
    }

    /// <summary>
    ///     消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    public new static void Info(string message, string token = "") => Info(new GrowlStrongInfo
    {
        Message = message,
        Token = token
    });

    /// <summary>
    ///     消息
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void Info(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Info);
        Show(growlStrongInfo);
    }

    /// <summary>
    ///     消息
    /// </summary>
    /// <param name="message"></param>
    public new static void InfoGlobal(string message) => InfoGlobal(new GrowlStrongInfo
    {
        Message = message
    });

    /// <summary>
    ///     消息
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void InfoGlobal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Info);
        ShowGlobal(growlStrongInfo);
    }

    /// <summary>
    ///     警告
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    public new static void Warning(string message, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = message,
        Token = token
    });

    /// <summary>
    ///     警告
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void Warning(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Warning);
        Show(growlStrongInfo);
    }

    /// <summary>
    ///     警告
    /// </summary>
    /// <param name="message"></param>
    public new static void WarningGlobal(string message) => WarningGlobal(new GrowlStrongInfo
    {
        Message = message
    });

    /// <summary>
    ///     警告
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void WarningGlobal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Warning);
        ShowGlobal(growlStrongInfo);
    }

    /// <summary>
    ///     错误
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    public new static void Error(string message, string token = "") => Error(new GrowlStrongInfo
    {
        Message = message,
        Token = token
    });

    /// <summary>
    ///     错误
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void Error(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Error);
        Show(growlStrongInfo);
    }

    /// <summary>
    ///     错误
    /// </summary>
    /// <param name="message"></param>
    public new static void ErrorGlobal(string message) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = message
    });

    /// <summary>
    ///     错误
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void ErrorGlobal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Error);
        ShowGlobal(growlStrongInfo);
    }

    /// <summary>
    ///     严重
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    public new static void Fatal(string message, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = message,
        Token = token
    });

    /// <summary>
    ///     严重
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void Fatal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Fatal);
        Show(growlStrongInfo);
    }

    /// <summary>
    ///     严重
    /// </summary>
    /// <param name="message"></param>
    public new static void FatalGlobal(string message) => FatalGlobal(new GrowlStrongInfo
    {
        Message = message
    });

    /// <summary>
    ///     严重
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void FatalGlobal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Fatal);
        ShowGlobal(growlStrongInfo);
    }

    /// <summary>
    ///     询问
    /// </summary>
    /// <param name="message"></param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="token"></param>
    public new static void Ask(string message, Func<bool, bool> actionBeforeClose, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = message,
        ActionBeforeClose = actionBeforeClose,
        Token = token
    });

    /// <summary>
    ///     询问
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void Ask(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Ask);
        Show(growlStrongInfo);
    }

    /// <summary>
    ///     询问
    /// </summary>
    /// <param name="message"></param>
    /// <param name="actionBeforeClose"></param>
    public new static void AskGlobal(string message, Func<bool, bool> actionBeforeClose) => AskGlobal(new GrowlStrongInfo
    {
        Message = message,
        ActionBeforeClose = actionBeforeClose
    });

    /// <summary>
    ///     询问
    /// </summary>
    /// <param name="growlStrongInfo"></param>
    public static void AskGlobal(GrowlStrongInfo growlStrongInfo)
    {
        InitGrowlStrongInfo(ref growlStrongInfo, InfoType.Ask);
        ShowGlobal(growlStrongInfo);
    }

    #endregion 健壮的弹出方法

    #region 弹出位置的扩展方法

    /// <summary>
    ///     成功
    /// </summary>
    /// <param name="message"></param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void SuccessToLeftTop(string message,bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    // TODO 其他方向需要进行扩展
    #endregion 弹出位置的扩展方法

    private void ButtonClose_OnClick(object sender, RoutedEventArgs e) => Close(false);

    /// <summary>
    ///     关闭
    /// </summary>
    private void Close(bool invokeParam)
    {
        if (ActionBeforeClose?.Invoke(invokeParam) == false)
        {
            return;
        }

        _timerClose?.Stop();
        var transform = new TranslateTransform();
        _gridMain.RenderTransform = transform;
        var animation = AnimationHelper.CreateAnimation(FlowDirection == FlowDirection.LeftToRight ? ActualWidth : -ActualWidth);
        animation.Completed += (s, e) =>
        {
            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);

                if (GrowlStrongWindow != null)
                {
                    if (panel.Children.Count == 0)
                    {
                        GrowlStrongWindow.GrowlPanel.Remove(panel);
                    }
                    if (GrowlStrongWindow.GrowlPanel is { Count: 0 })
                    {
                        GrowlStrongWindow.Close();
                        GrowlStrongWindow = null;
                    }
                }
                else
                {
                    //获取当前容器的Panel
                    if (GrowlPanel is { Count: > 0 } && panel.Children.Count == 0 && GetIsCreatedAutomatically(panel))
                    {
                        // If the count of children is zero, we need to remove the panel, provided that the panel was created automatically  
                        RemoveDefaultPanel(panel);
                        GrowlPanel.Remove(panel);
                    }
                }
            }
        };
        transform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    /// <summary>
    ///     清除
    /// </summary>
    /// <param name="token"></param>
    public new static void Clear(string token = "")
    {
        if (!string.IsNullOrEmpty(token))
        {
            if (PanelDic.TryGetValue(token, out var panel))
            {
                Clear(panel);
            }
        }
        else
        {
            foreach (var item in GrowlPanel)
            {
                Clear(item);
            }
        }
    }

    /// <summary>
    ///     清除
    /// </summary>
    /// <param name="panel"></param>
    private static void Clear(Panel panel) => panel?.Children.Clear();

    /// <summary>
    ///     清除
    /// </summary>
    public new static void ClearGlobal()
    {
        if (GrowlStrongWindow == null) return;
        foreach (var item in GrowlStrongWindow.GrowlPanel)
        {
            Clear(item);
        }
        GrowlStrongWindow.Close();
        GrowlStrongWindow = null;
    }

    private void ButtonCancel_OnClick(object sender, RoutedEventArgs e) => Close(false);

    private void ButtonOk_OnClick(object sender, RoutedEventArgs e) => Close(true);
}
