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
public class GrowlStrong : Control
{
    private const string ElementPanelMore = "PART_PanelMore";
    private const string ElementGridMain = "PART_GridMain";
    private const string ElementButtonClose = "PART_ButtonClose";
    private const int MinWaitTime = 2;

    public static readonly DependencyProperty GrowlParentProperty = DependencyProperty.RegisterAttached(
        "GrowlParent", typeof(bool), typeof(GrowlStrong), new PropertyMetadata(ValueBoxes.FalseBox, (o, args) =>
        {
            if ((bool) args.NewValue && o is Panel panel)
            {
                SetGrowlPanel(panel);
            }
        }));
    public static readonly DependencyProperty ShowModeProperty = DependencyProperty.RegisterAttached(
        "ShowMode", typeof(GrowlShowMode), typeof(GrowlStrong),
        new FrameworkPropertyMetadata(default(GrowlShowMode), FrameworkPropertyMetadataOptions.Inherits));
    public static readonly DependencyProperty ShowDateTimeProperty = DependencyProperty.Register(
        nameof(ShowDateTime), typeof(bool), typeof(GrowlStrong), new PropertyMetadata(ValueBoxes.TrueBox));
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(GrowlStrong), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
        nameof(Time), typeof(DateTime), typeof(GrowlStrong), new PropertyMetadata(default(DateTime)));
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Geometry), typeof(GrowlStrong), new PropertyMetadata(default(Geometry)));
    public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(
        nameof(IconBrush), typeof(Brush), typeof(GrowlStrong), new PropertyMetadata(default(Brush)));
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
        nameof(Type), typeof(InfoType), typeof(GrowlStrong), new PropertyMetadata(default(InfoType)));
    public static readonly DependencyProperty TokenProperty = DependencyProperty.RegisterAttached(
        "Token", typeof(string), typeof(GrowlStrong), new PropertyMetadata(default(string), OnTokenChanged));
    internal static readonly DependencyProperty CancelStrProperty = DependencyProperty.Register(
        nameof(CancelStr), typeof(string), typeof(GrowlStrong), new PropertyMetadata(default(string)));
    internal static readonly DependencyProperty ConfirmStrProperty = DependencyProperty.Register(
        nameof(ConfirmStr), typeof(string), typeof(GrowlStrong), new PropertyMetadata(default(string)));
    private static readonly DependencyProperty IsCreatedAutomaticallyProperty = DependencyProperty.RegisterAttached(
        "IsCreatedAutomatically", typeof(bool), typeof(GrowlStrong), new PropertyMetadata(ValueBoxes.FalseBox));
    private static GrowlStrongWindow GrowlStrongWindow;
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
    public static List<Panel> GrowlPanel { get; set; } = new();

    public InfoType Type
    {
        get => (InfoType) GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public bool ShowDateTime
    {
        get => (bool) GetValue(ShowDateTimeProperty);
        set => SetValue(ShowDateTimeProperty, ValueBoxes.BooleanBox(value));
    }

    public string Message
    {
        get => (string) GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public DateTime Time
    {
        get => (DateTime) GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public Geometry Icon
    {
        get => (Geometry) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Brush IconBrush
    {
        get => (Brush) GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    internal string CancelStr
    {
        get => (string) GetValue(CancelStrProperty);
        set => SetValue(CancelStrProperty, value);
    }

    internal string ConfirmStr
    {
        get => (string) GetValue(ConfirmStrProperty);
        set => SetValue(ConfirmStrProperty, value);
    }

    private Func<bool, bool> ActionBeforeClose { get; set; }


    public GrowlStrong()
    {
        CommandBindings.Add(new CommandBinding(ControlCommands.Close, ButtonClose_OnClick));
        CommandBindings.Add(new CommandBinding(ControlCommands.Cancel, ButtonCancel_OnClick));
        CommandBindings.Add(new CommandBinding(ControlCommands.Confirm, ButtonOk_OnClick));
    }
    public static void Register(string token, Panel panel)
    {
        if (string.IsNullOrEmpty(token) || panel == null) return;
        PanelDic[token] = panel;
        InitGrowlPanel(panel);
    }

    public static void Unregister(string token, Panel panel)
    {
        if (string.IsNullOrEmpty(token) || panel == null) return;

        if (PanelDic.ContainsKey(token))
        {
            if (ReferenceEquals(PanelDic[token], panel))
            {
                PanelDic.Remove(token);
                panel.ContextMenu = null;
                panel.SetCurrentValue(PanelElement.FluidMoveBehaviorProperty, DependencyProperty.UnsetValue);
            }
        }
    }

    public static void Unregister(Panel panel)
    {
        if (panel == null) return;
        var first = PanelDic.FirstOrDefault(item => ReferenceEquals(panel, item.Value));
        if (!string.IsNullOrEmpty(first.Key))
        {
            PanelDic.Remove(first.Key);
            panel.ContextMenu = null;
            panel.SetCurrentValue(PanelElement.FluidMoveBehaviorProperty, DependencyProperty.UnsetValue);
        }
    }

    public static void Unregister(string token)
    {
        if (string.IsNullOrEmpty(token)) return;

        if (PanelDic.ContainsKey(token))
        {
            var panel = PanelDic[token];
            PanelDic.Remove(token);
            panel.ContextMenu = null;
            panel.SetCurrentValue(PanelElement.FluidMoveBehaviorProperty, DependencyProperty.UnsetValue);
        }
    }


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
        DependencyProperty.Register(nameof(ShowMask), typeof(bool), typeof(GrowlStrong), new PropertyMetadata(false, OnShowMaskPropertyChanged));

    /// <summary>
    /// 当属性 显示遮罩 出现变更时
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void OnShowMaskPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

    }
    #endregion 显示遮罩 - 依赖属性 

    public static void SetToken(DependencyObject element, string value) => element.SetValue(TokenProperty, value);

    public static string GetToken(DependencyObject element) => (string) element.GetValue(TokenProperty);

    public static void SetShowMode(DependencyObject element, GrowlShowMode value) => element.SetValue(ShowModeProperty, value);

    public static GrowlShowMode GetShowMode(DependencyObject element) => (GrowlShowMode) element.GetValue(ShowModeProperty);

    public static void SetGrowlParent(DependencyObject element, bool value) => element.SetValue(GrowlParentProperty, ValueBoxes.BooleanBox(value));

    public static bool GetGrowlParent(DependencyObject element) => (bool) element.GetValue(GrowlParentProperty);

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
                        GrowlStrongWindow = new GrowlStrongWindow(growlInfo.ShowPosition);
                        GrowlStrongWindow.Show();
                        InitGrowlPanel(GrowlStrongWindow.GrowlPanel.FirstOrDefault());
                        GrowlStrongWindow.Init();
                    }

                    GrowlStrongWindow.Show(true);

                    //growlInfo.GrowlMaxWidth = GrowlStrongWindow.ActualWidth;
                    //if (growlInfo.GrowlMaxWidth > GrowlStrongWindow.ActualWidth - 100)
                    //{
                    //    if (GrowlStrongWindow.ActualWidth > 100)
                    //    {
                    //        growlInfo.GrowlMaxWidth = GrowlStrongWindow.ActualWidth - 100;
                    //    }
                    //    else
                    //    {
                    //        growlInfo.GrowlMaxWidth = GrowlStrongWindow.ActualWidth;
                    //    }
                    //}

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
    public static void Success(string message, string token = "") => Success(new GrowlStrongInfo
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
    public static void SuccessGlobal(string message) => SuccessGlobal(new GrowlStrongInfo
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
    public static void Info(string message, string token = "") => Info(new GrowlStrongInfo
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
    public static void InfoGlobal(string message) => InfoGlobal(new GrowlStrongInfo
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
    public static void Warning(string message, string token = "") => Warning(new GrowlStrongInfo
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
    private static void WarningGlobal(string message) => WarningGlobal(new GrowlStrongInfo
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
    public static void Error(string message, string token = "") => Error(new GrowlStrongInfo
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
    public static void ErrorGlobal(string message) => ErrorGlobal(new GrowlStrongInfo
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
    public static void Fatal(string message, string token = "") => Fatal(new GrowlStrongInfo
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
    public static void FatalGlobal(string message) => FatalGlobal(new GrowlStrongInfo
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
    public static void Ask(string message, Func<bool, bool> actionBeforeClose, string token = "") => Ask(new GrowlStrongInfo
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
    public static void AskGlobal(string message, Func<bool, bool> actionBeforeClose) => AskGlobal(new GrowlStrongInfo
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


    #region 成功
    /// <summary>
    ///     成功 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void SuccessToLeftTop(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     成功 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void SuccessToLeftCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     成功 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void SuccessToLeftBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     成功 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void SuccessToCenterTop(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     成功 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void SuccessToCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     成功 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void SuccessToCenterBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     成功 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void SuccessToRightTop(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     成功 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void SuccessToRightCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     成功 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void SuccessToRightBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Success(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom
    });

    /// <summary>
    ///     成功 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToLeftTop(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     成功 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToLeftCenter(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     成功 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToLeftBottom(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     成功 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToCenterTop(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     成功 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToCenter(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     成功 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToCenterBottom(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     成功 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToRightTop(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     成功 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToRightCenter(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     成功 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void SuccessGlobalToRightBottom(string message, string topic = null) => SuccessGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightBottom
    });

    #endregion 成功

    #region 消息
    /// <summary>
    ///     消息 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void InfoToLeftTop(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     消息 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void InfoToLeftCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     消息 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void InfoToLeftBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     消息 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void InfoToCenterTop(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     消息 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void InfoToCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     消息 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void InfoToCenterBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     消息 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void InfoToRightTop(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     消息 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void InfoToRightCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     消息 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void InfoToRightBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Info(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom
    });


    /// <summary>
    ///     消息 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToLeftTop(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     消息 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToLeftCenter(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     消息 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToLeftBottom(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     消息 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToCenterTop(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     消息 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToCenter(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     消息 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToCenterBottom(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     消息 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToRightTop(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     消息 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToRightCenter(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     消息 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void InfoGlobalToRightBottom(string message, string topic = null) => InfoGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightBottom
    });
    #endregion 消息

    #region 警告
    /// <summary>
    ///     警告 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void WarningToLeftTop(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     警告 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void WarningToLeftCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     警告 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void WarningToLeftBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     警告 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void WarningToCenterTop(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     警告 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void WarningToCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     警告 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void WarningToCenterBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     警告 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void WarningToRightTop(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     警告 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void WarningToRightCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     警告 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void WarningToRightBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Warning(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom
    });


    /// <summary>
    ///     警告 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToLeftTop(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     警告 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToLeftCenter(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     警告 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToLeftBottom(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     警告 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToCenterTop(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     警告 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToCenter(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     警告 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToCenterBottom(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     警告 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToRightTop(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     警告 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToRightCenter(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     警告 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void WarningGlobalToRightBottom(string message, string topic = null) => WarningGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightBottom
    });
    #endregion 警告

    #region 错误
    /// <summary>
    ///     错误 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorToLeftTop(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     错误 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorToLeftCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     错误 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorToLeftBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     错误 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorToCenterTop(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     错误 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorToCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     错误 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorToCenterBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     错误 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorToRightTop(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     错误 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorToRightCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     错误 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorToRightBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom
    });


    /// <summary>
    ///     错误 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToLeftTop(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     错误 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToLeftCenter(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     错误 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToLeftBottom(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     错误 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToCenterTop(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     错误 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToCenter(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     错误 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToCenterBottom(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     错误 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToRightTop(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     错误 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToRightCenter(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     错误 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalToRightBottom(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightBottom
    });
    #endregion 错误

    #region 错误自动关闭

    /// <summary>
    ///     错误(自动关闭)
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="token"></param>
    public static void ErrorAutoClose(string message, string topic = null, string token = "") => Error(new GrowlStrongInfo()
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误(自动关闭)
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorGlobalAutoClose(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo()
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToLeftTop(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToLeftCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToLeftBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToCenterTop(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToCenterBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToRightTop(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToRightCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void ErrorAutoCloseToRightBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Error(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom,
        IsCustom = true,
        StaysOpen = false,
    });


    /// <summary>
    ///     错误（自动关闭） 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToLeftTop(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftTop,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToLeftCenter(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftCenter,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToLeftBottom(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftBottom,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToCenterTop(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterTop,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToCenter(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.Center,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToCenterBottom(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterBottom,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToRightTop(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightTop,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToRightCenter(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightCenter,
        IsCustom = true,
        StaysOpen = false,
    });

    /// <summary>
    ///     错误（自动关闭） 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void ErrorAutoCloseGlobalToRightBottom(string message, string topic = null) => ErrorGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightBottom,
        IsCustom = true,
        StaysOpen = false,
    });
    #endregion 错误自动关闭

    #region 严重
    /// <summary>
    ///     严重 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void FatalToLeftTop(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     严重 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void FatalToLeftCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     严重 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void FatalToLeftBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     严重 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void FatalToCenterTop(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     严重 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void FatalToCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     严重 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void FatalToCenterBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     严重 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void FatalToRightTop(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     严重 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void FatalToRightCenter(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     严重 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void FatalToRightBottom(string message, string topic = null, bool isShowMask = false, string token = "") => Fatal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom
    });


    /// <summary>
    ///     严重 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToLeftTop(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     严重 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToLeftCenter(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     严重 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToLeftBottom(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     严重 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToCenterTop(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     严重 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToCenter(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     严重 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToCenterBottom(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     严重 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToRightTop(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     严重 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToRightCenter(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     严重 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="topic">信息主题</param>
    public static void FatalGlobalToRightBottom(string message, string topic = null) => FatalGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ShowPosition = GrowlShowPosition.RightBottom
    });
    #endregion 严重

    #region 询问

    /// <summary>
    ///     询问 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void AskToLeftTop(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     询问 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void AskToLeftCenter(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     询问 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void AskToLeftBottom(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     询问 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask">是否显示遮罩</param>
    /// <param name="token"></param>
    public static void AskToCenterTop(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     询问 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void AskToCenter(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     询问 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void AskToCenterBottom(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     询问 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void AskToRightTop(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     询问 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void AskToRightCenter(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     询问 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    /// <param name="isShowMask"></param>
    /// <param name="token"></param>
    public static void AskToRightBottom(string message, Func<bool, bool> actionBeforeClose, string topic = null, bool isShowMask = false, string token = "") => Ask(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        Token = token,
        ShowMask = isShowMask,
        ShowPosition = GrowlShowPosition.RightBottom
    });


    /// <summary>
    ///     询问 左上
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToLeftTop(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.LeftTop
    });

    /// <summary>
    ///     询问 左中
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToLeftCenter(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.LeftCenter
    });

    /// <summary>
    ///     询问 左下
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToLeftBottom(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.LeftBottom
    });

    /// <summary>
    ///     询问 居中上方
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToCenterTop(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.CenterTop
    });

    /// <summary>
    ///     询问 居中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToCenter(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.Center
    });

    /// <summary>
    ///     询问 居中下方
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToCenterBottom(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.CenterBottom
    });

    /// <summary>
    ///     询问 右上
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToRightTop(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.RightTop
    });

    /// <summary>
    ///     询问 右中
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToRightCenter(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.RightCenter
    });

    /// <summary>
    ///     询问 右下
    /// </summary>
    /// <param name="message">信息内容</param>
    /// <param name="actionBeforeClose"></param>
    /// <param name="topic">信息主题</param>
    public static void AskGlobalToRightBottom(string message, Func<bool, bool> actionBeforeClose, string topic = null) => AskGlobal(new GrowlStrongInfo
    {
        Message = (string.IsNullOrEmpty(topic) ? "" : $"[{topic}] ") + message,
        ActionBeforeClose = actionBeforeClose,
        ShowPosition = GrowlShowPosition.RightBottom
    });
    #endregion 询问

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
    public static void Clear(string token = "")
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
    public static void ClearGlobal()
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
