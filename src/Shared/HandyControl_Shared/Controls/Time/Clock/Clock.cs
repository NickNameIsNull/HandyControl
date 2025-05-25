using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Tools;

namespace HandyControl.Controls;

[TemplatePart(Name = ElementButtonAm, Type = typeof(RadioButton))]
[TemplatePart(Name = ElementButtonPm, Type = typeof(RadioButton))]
[TemplatePart(Name = ElementCanvas, Type = typeof(Canvas))]
[TemplatePart(Name = ElementBorderTitle, Type = typeof(Border))]
[TemplatePart(Name = ElementBorderClock, Type = typeof(Border))]
[TemplatePart(Name = ElementPanelNum, Type = typeof(CirclePanel))]
[TemplatePart(Name = ElementTimeStr, Type = typeof(TextBlock))]

#region Flag 时钟列表选择扩展
[TemplatePart(Name = ElementListClockPresenter, Type = typeof(ContentPresenter))]
[TemplatePart(Name = ElementListClockPopup, Type = typeof(Popup))]
[TemplatePart(Name = ElementListClockToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = ElementTimeStart, Type = typeof(Button))]
[TemplatePart(Name = ElementTimeNow, Type = typeof(Button))]
[TemplatePart(Name = ElementTimeEnd, Type = typeof(Button))]
[TemplatePart(Name = ElementClockTimeStart, Type = typeof(Button))]
[TemplatePart(Name = ElementClockTimeNow, Type = typeof(Button))]
[TemplatePart(Name = ElementClockTimeEnd, Type = typeof(Button))]
#endregion Flag 时钟列表选择扩展
public class Clock : ClockBase
{
    public Clock():base()
    {
        #region Flag 时钟列表选择扩展
        InitListClock();
        #endregion Flag 时钟列表选择扩展
    }

    #region Constants

    private const string ElementButtonAm = "PART_ButtonAm";
    private const string ElementButtonPm = "PART_ButtonPm";
    private const string ElementCanvas = "PART_Canvas";
    private const string ElementBorderTitle = "PART_BorderTitle";
    private const string ElementBorderClock = "PART_BorderClock";
    private const string ElementPanelNum = "PART_PanelNum";
    private const string ElementTimeStr = "PART_TimeStr";

    #region Flag 时钟列表选择扩展
    private const string ElementListClockPresenter = "PART_ListClockPresenter";
    private const string ElementListClockPopup = "PART_ListClockPopup";
    private const string ElementListClockToggleButton = "PART_ListClockToggleButton";
    private const string ElementTimeStart = "PART_TimeStart";
    private const string ElementTimeNow = "PART_TimeNow";
    private const string ElementTimeEnd = "PART_TimeEnd";
    private const string ElementClockTimeStart = "PART_ClockTimeStart";
    private const string ElementClockTimeNow = "PART_ClockTimeNow";
    private const string ElementClockTimeEnd = "PART_ClockTimeEnd";
    #endregion Flag 时钟列表选择扩展
    #endregion Constants

    #region Data

    private RadioButton _buttonAm;

    private RadioButton _buttonPm;

    private Canvas _canvas;

    private Border _borderTitle;

    private Border _borderClock;

    private ClockRadioButton _currentButton;

    private RotateTransform _rotateTransformClock;

    private CirclePanel _circlePanel;

    private List<ClockRadioButton> _radioButtonList;

    private TextBlock _blockTime;

    private int _secValue;

    #region Flag 时钟列表选择扩展
    private ListClock _listClock;
    private ContentPresenter _listClockPresenter;
    private Popup _listClockPopup;
    private ToggleButton _listClockToggleButton;
    private Button _timeStart;
    private Button _timeNow;
    private Button _timeEnd;
    private Button _clockTimeStart;
    private Button _clockTimeNow;
    private Button _clockTimeEnd;
    private DateTime? _showListClockDisplayDateTime;
    #endregion Flag 时钟列表选择扩展

    #endregion Data

    #region Public Properties

    public static readonly DependencyProperty ClockRadioButtonStyleProperty = DependencyProperty.Register(
        nameof(ClockRadioButtonStyle), typeof(Style), typeof(Clock), new PropertyMetadata(default(Style)));

    public Style ClockRadioButtonStyle
    {
        get => (Style) GetValue(ClockRadioButtonStyleProperty);
        set => SetValue(ClockRadioButtonStyleProperty, value);
    }

    private int SecValue
    {
        get => _secValue;
        set
        {
            if (value < 0)
            {
                _secValue = 59;
            }
            else if (value > 59)
            {
                _secValue = 0;
            }
            else
            {
                _secValue = value;
            }
        }
    }

    #endregion Public Properties

    #region Public Methods

    public override void OnApplyTemplate()
    {
        AppliedTemplate = false;
        if (_buttonAm != null)
        {
            _buttonAm.Click -= ButtonAm_OnClick;
        }

        if (_buttonPm != null)
        {
            _buttonPm.Click -= ButtonPm_OnClick;
        }

        if (ButtonConfirm != null)
        {
            ButtonConfirm.Click -= ButtonConfirm_OnClick;
        }

        if (_borderTitle != null)
        {
            _borderTitle.MouseWheel -= BorderTitle_OnMouseWheel;
        }

        if (_canvas != null)
        {
            _canvas.MouseWheel -= Canvas_OnMouseWheel;
            _canvas.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Canvas_OnClick));
            _canvas.MouseMove -= Canvas_OnMouseMove;
        }

        #region Flag 时钟列表选择扩展
        if (_timeStart != null)
        {
            _timeStart.Click -= TimeButton_Click;
        }
        if (_timeNow != null)
        {
            _timeNow.Click -= TimeButton_Click;
        }
        if (_timeEnd != null)
        {
            _timeEnd.Click -= TimeButton_Click;
        }
        if (_clockTimeStart != null)
        {
            _clockTimeStart.Click -= TimeButton_Click;
        }
        if (_clockTimeNow != null)
        {
            _clockTimeNow.Click -= TimeButton_Click;
        }
        if (_clockTimeEnd != null)
        {
            _clockTimeEnd.Click -= TimeButton_Click;
        }

        if (_listClockToggleButton != null)
        {
            _listClockToggleButton.Checked -= ListClockToggleButton_Checked;
            _listClockToggleButton.Unchecked -= ListClockToggleButton_Unchecked;
            //_blockTime.Click -= TimeStrButton_Click;
        }
        #endregion Flag 时钟列表选择扩展

        base.OnApplyTemplate();

        _buttonAm = GetTemplateChild(ElementButtonAm) as RadioButton;
        _buttonPm = GetTemplateChild(ElementButtonPm) as RadioButton;
        ButtonConfirm = GetTemplateChild(ElementButtonConfirm) as Button;
        _borderTitle = GetTemplateChild(ElementBorderTitle) as Border;
        _canvas = GetTemplateChild(ElementCanvas) as Canvas;
        _borderClock = GetTemplateChild(ElementBorderClock) as Border;
        _circlePanel = GetTemplateChild(ElementPanelNum) as CirclePanel;
        _blockTime = GetTemplateChild(ElementTimeStr) as TextBlock;


        #region Flag 时钟列表选择扩展
        _listClockPresenter = GetTemplateChild(ElementListClockPresenter) as ContentPresenter;
        _listClockPopup = GetTemplateChild(ElementListClockPopup) as Popup;
        _listClockToggleButton = GetTemplateChild(ElementListClockToggleButton) as ToggleButton;
        _timeStart = GetTemplateChild(ElementTimeStart) as Button;
        _timeNow = GetTemplateChild(ElementTimeNow) as Button;
        _timeEnd = GetTemplateChild(ElementTimeEnd) as Button;
        _clockTimeStart = GetTemplateChild(ElementClockTimeStart) as Button;
        _clockTimeNow = GetTemplateChild(ElementClockTimeNow) as Button;
        _clockTimeEnd = GetTemplateChild(ElementClockTimeEnd) as Button;
        #endregion Flag 时钟列表选择扩展

        if (!CheckNull()) return;

        #region Flag 时钟列表选择扩展
        if (_timeStart != null)
        {
            _timeStart.Click += TimeButton_Click;
        }
        if (_timeNow != null)
        {
            _timeNow.Click += TimeButton_Click;
        }
        if (_timeEnd != null)
        {
            _timeEnd.Click += TimeButton_Click;
        }
        if (_clockTimeStart != null)
        {
            _clockTimeStart.Click += TimeButton_Click;
        }
        if (_clockTimeNow != null)
        {
            _clockTimeNow.Click += TimeButton_Click;
        }
        if (_clockTimeEnd != null)
        {
            _clockTimeEnd.Click -= TimeButton_Click;
            _clockTimeEnd.Click += TimeButton_Click;
        }

        if (_listClockToggleButton != null)
        {
            _listClockToggleButton.Checked += ListClockToggleButton_Checked;
            _listClockToggleButton.Unchecked += ListClockToggleButton_Unchecked;
            //_blockTime.Click -= TimeStrButton_Click;
        }
        if (_listClockPresenter != null)
        {
            _listClockPresenter.Content = _listClock;
        }
        #endregion Flag 时钟列表选择扩展

        _buttonAm.Click += ButtonAm_OnClick;
        _buttonPm.Click += ButtonPm_OnClick;
        ButtonConfirm.Click += ButtonConfirm_OnClick;
        _borderTitle.MouseWheel += BorderTitle_OnMouseWheel;

        _canvas.MouseWheel += Canvas_OnMouseWheel;
        _canvas.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Canvas_OnClick));
        _canvas.MouseMove += Canvas_OnMouseMove;

        _rotateTransformClock = new RotateTransform();
        _borderClock.RenderTransform = _rotateTransformClock;

        _radioButtonList = new List<ClockRadioButton>();
        for (var i = 0; i < 12; i++)
        {
            var num = i + 1;
            var button = new ClockRadioButton
            {
                Num = num,
                Content = num
            };
            button.SetBinding(StyleProperty, new Binding(ClockRadioButtonStyleProperty.Name) { Source = this });
            _radioButtonList.Add(button);
            _circlePanel.Children.Add(button);
        }

        AppliedTemplate = true;
        if (SelectedTime.HasValue)
        {
            Update(SelectedTime.Value);
        }
        else
        {
            DisplayTime = DateTime.Now;
            Update(DisplayTime);
        }
    }

    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// 初始化列表时钟的时间
    /// </summary>
    private void InitListClock()
    {
        _listClock = new ListClock()
        {
            Width = 200,
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            ShowConfirmButton = true,
        };
        Binding displayTimeBinding = new Binding(nameof(DisplayTime))
        {
            Source = this
        };

        Binding selectedTimeBinding = new Binding(nameof(SelectedTimeProperty))
        {
            Source = this
        };

        _listClock.SetBinding(ListClock.DisplayTimeProperty, displayTimeBinding);
        _listClock.SetBinding(ListClock.SelectedTimeProperty, displayTimeBinding);
        _listClock.SetValue(BorderElement.CornerRadiusProperty, new CornerRadius(0, 0, 4, 4));
        _listClock.Confirmed += ListClock_Confirmed;
        //_listClock.SelectedTimeChanged -= ListClock_SelectedTimeChanged;
        //_listClock.DisplayTimeChanged -= ListClock_DisplayTimeChanged;
    }

    /// <summary>
    /// 时钟列表确认
    /// </summary>
    private void ListClock_Confirmed()
    {
        if (_listClockPopup != null)
        {
            _listClockPopup.IsOpen = false;
        }
        Update(_listClock.SelectedTime.GetValueOrDefault());
    }

    /// <summary>
    /// 列表时钟选中
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListClockToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        if (_listClock != null)
        {
            if (e.Source is ToggleButton { IsChecked: true })
            {
                _showListClockDisplayDateTime = DisplayTime;
                _listClock.SelectedTime = DisplayTime;
                _listClock.DisplayTime = DisplayTime;
            }
        }
        e.Handled = true;
    }
    /// <summary>
    /// 列表时钟取消选中
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListClockToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        //if(_showListClockDisplayDateTime != null && _showListClockDisplayDateTime!= DateTime.MinValue)
        //{
        //    //关闭时恢复打开时的时间
        //    Update(_showListClockDisplayDateTime.GetValueOrDefault());
        //}
        e.Handled = true;
    }

    /// <summary>
    /// 时间按钮点击事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.Name.EndsWith("TimeStart"))
            {
                if (_listClock != null)
                {
                    _listClock.DisplayTime = _listClock.DisplayTime.Date;
                    _listClock.SelectedTime = _listClock.SelectedTime.GetValueOrDefault().Date;
                }
                Update(DisplayTime.Date);
                //DisplayTime = DisplayTime.Date;
                //SelectedTime = SelectedTime.GetValueOrDefault().Date;

            }
            else if (btn.Name.EndsWith("TimeNow"))
            {
                if (_listClock != null)
                {
                    _listClock.DisplayTime = DateTime.Now;
                    _listClock.SelectedTime = DateTime.Now;
                }
                Update(DateTime.Now);
            }
            else if (btn.Name.EndsWith("TimeEnd"))
            {
                if (_listClock != null)
                {
                    _listClock.DisplayTime = _listClock.DisplayTime.Date.AddDays(1).AddSeconds(-1);
                    _listClock.SelectedTime = _listClock.SelectedTime.GetValueOrDefault().Date.AddDays(1).AddSeconds(-1);
                }
                Update(DisplayTime.Date.AddDays(1).AddSeconds(-1));
                //DisplayTime = DisplayTime.Date.AddDays(1).AddSeconds(-1);
                //SelectedTime = SelectedTime.GetValueOrDefault().Date.AddDays(1).AddSeconds(-1);
            }
        }
    }

    /// <summary>
    /// 时间显示字符串按钮点击事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TimeStrButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private bool CheckNull()
    {
        if (_buttonPm == null || _buttonAm == null || ButtonConfirm == null || _canvas == null ||
            _borderTitle == null || _borderClock == null || _circlePanel == null ||
            _blockTime == null) return false;
        return true;
    }

    private void BorderTitle_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta < 0)
        {
            SecValue--;
            Update();
        }
        else
        {
            SecValue++;
            Update();
        }
        e.Handled = true;
    }

    private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var value = (int) _rotateTransformClock.Angle;
        if (e.Delta < 0)
        {
            value += 6;
        }
        else
        {
            value -= 6;
        }
        if (value < 0)
        {
            value = value + 360;
        }
        _rotateTransformClock.Angle = value;

        Update();
        e.Handled = true;
    }

    private void Canvas_OnClick(object sender, RoutedEventArgs e)
    {
        _currentButton = e.OriginalSource as ClockRadioButton;
        if (_currentButton != null)
        {
            Update();
        }
    }

    private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var value = ArithmeticHelper.CalAngle(new Point(85, 85), e.GetPosition(_canvas)) + 90;
            if (value < 0)
            {
                value = value + 360;
            }
            value = value - value % 6;
            _rotateTransformClock.Angle = value;
            Update();
        }
    }

    private void Update()
    {
        if (!AppliedTemplate) return;
        var hValue = _currentButton.Num;
        if (_buttonPm.IsChecked == true)
        {
            hValue += 12;
            if (hValue == 24) hValue = 12;
        }
        else if (hValue == 12)
        {
            hValue = 0;
        }
        if (hValue == 12 && _buttonAm.IsChecked == true)
        {
            _buttonPm.IsChecked = true;
            _buttonAm.IsChecked = false;
        }

        if (_blockTime != null)
        {
            DisplayTime = GetDisplayTime();
            _blockTime.Text = DisplayTime.ToString(TimeFormat);
        }
    }

    /// <summary>
    ///     更新
    /// </summary>
    /// <param name="time"></param>
    internal override void Update(DateTime time)
    {
        if (!AppliedTemplate) return;
        var h = time.Hour;
        var m = time.Minute;

        if (h >= 12)
        {
            _buttonPm.IsChecked = true;
            _buttonAm.IsChecked = false;
        }
        else
        {
            _buttonPm.IsChecked = false;
            _buttonAm.IsChecked = true;
        }

        _rotateTransformClock.Angle = m * 6;

        var hRest = h % 12;
        if (hRest == 0) hRest = 12;
        var ctl = _radioButtonList[hRest - 1];
        ctl.IsChecked = true;
        ctl.RaiseEvent(new RoutedEventArgs { RoutedEvent = ButtonBase.ClickEvent });

        _secValue = time.Second;
        Update();
    }

    /// <summary>
    ///     获取显示时间
    /// </summary>
    /// <returns></returns>
    private DateTime GetDisplayTime()
    {
        var hValue = _currentButton.Num;
        if (_buttonPm.IsChecked == true)
        {
            hValue += 12;
            if (hValue == 24) hValue = 12;
        }
        else if (hValue == 12)
        {
            hValue = 0;
        }
        var now = DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, hValue, (int) Math.Abs(_rotateTransformClock.Angle) % 360 / 6, _secValue);
    }

    private void ButtonAm_OnClick(object sender, RoutedEventArgs e) => Update();

    private void ButtonPm_OnClick(object sender, RoutedEventArgs e) => Update();

    #endregion Private Methods       
}
