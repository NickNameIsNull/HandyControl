#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HandyControl.Interactivity;
using HandyControl.Tools.Helper;

namespace HandyControl.Controls
{
    /// <summary>
    /// 日历元素附加属性
    /// Flag HandyControl 扩展
    /// </summary>
    public class CalendarElement
    {
        #region 允许使用的最小时间 - 附加属性
        /// <summary>
        /// 允许使用的最小时间 - 获取附加属性
        /// </summary>                
        public static DateTime? GetAllowMinDateTime(DependencyObject obj)
        {
            return (DateTime?) obj.GetValue(AllowMinDateTimeProperty);
        }

        /// <summary>
        /// 允许使用的最小时间 - 设置附加属性 设置该属性时一定要注意SelectedDate是否在范围外，若在范围外，将抛出异常
        /// </summary>  
        public static void SetAllowMinDateTime(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(AllowMinDateTimeProperty, value);
        }

        /// <summary>
        /// 允许使用的最小时间 - 注册附加属性 
        /// <para>使用DependencyProperty作为MyProperty的后台存储。这支持动画、样式、绑定等。</para>
        /// </summary>
        public static readonly DependencyProperty AllowMinDateTimeProperty =
            DependencyProperty.RegisterAttached(
                "AllowMinDateTime",
                typeof(DateTime?),
                typeof(CalendarElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender,
                    OnAllowMinDateTimeChanged));

        /// <summary>
        /// 当 允许使用的最小时间 - 附加属性发生变更时
        /// </summary>
        private static void OnAllowMinDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarBlackoutDatesCollection? blackoutDates = null;
            if (d is CalendarWithClock calendarWithClock)
            {
                blackoutDates = calendarWithClock.Calendar.BlackoutDates;
            }
            else if (d is DateTimePicker dateTimePicker)
            {
                blackoutDates = dateTimePicker.CalendarWithClock.Calendar.BlackoutDates;
            }
            else if (d is DatePicker dp)
            {
                blackoutDates = dp.BlackoutDates;
            }
            else if (d is Calendar ci)
            {
                blackoutDates = ci.BlackoutDates;
            }

            if (blackoutDates != null)
            {
                if (e.OldValue is not null && e.OldValue is DateTime oldDateTime)
                {
                    //移除旧值
                    var findResult = blackoutDates.Where(p => p.Start == DateTime.MinValue && p.End == oldDateTime.AddDays(-1)).ToList();
                    if (findResult.Any())
                    {
                        foreach (var item in findResult)
                        {
                            blackoutDates.Remove(item);
                        }
                    }
                }
                if (e.NewValue is not null && e.NewValue is DateTime newDateTime)
                {
                    //添加新值
                    blackoutDates.Add(new CalendarDateRange(DateTime.MinValue, newDateTime.AddDays(-1)));
                }
            }
        }
        #endregion 允许使用的最小时间 - 附加属性 

        #region 允许使用的最大时间 - 附加属性
        /// <summary>
        /// 允许使用的最大时间 - 获取附加属性
        /// </summary>                
        public static DateTime? GetAllowMaxDateTime(DependencyObject obj)
        {
            return (DateTime?) obj.GetValue(AllowMaxDateTimeProperty);
        }

        /// <summary>
        /// 允许使用的最大时间 - 设置附加属性，设置该属性时一定要注意SelectedDate是否在范围外，若在范围外，将抛出异常
        /// </summary>  
        public static void SetAllowMaxDateTime(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(AllowMaxDateTimeProperty, value);
        }

        /// <summary>
        /// 允许使用的最大时间 - 注册附加属性 
        /// <para>使用DependencyProperty作为MyProperty的后台存储。这支持动画、样式、绑定等。</para>
        /// </summary>
        public static readonly DependencyProperty AllowMaxDateTimeProperty =
            DependencyProperty.RegisterAttached(
                "AllowMaxDateTime",
                typeof(DateTime?),
                typeof(CalendarElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender,
                    OnAllowMaxDateTimeChanged));

        /// <summary>
        /// 当 允许使用的最大时间 - 附加属性发生变更时
        /// </summary>
        private static void OnAllowMaxDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            CalendarBlackoutDatesCollection? blackoutDates = null;
            if (d is CalendarWithClock calendarWithClock)
            {
                blackoutDates = calendarWithClock.Calendar.BlackoutDates;
            }
            else if (d is DateTimePicker dateTimePicker)
            {
                blackoutDates = dateTimePicker.CalendarWithClock.Calendar.BlackoutDates;
            }
            else if (d is DatePicker dp)
            {
                blackoutDates = dp.BlackoutDates;
            }
            else if (d is Calendar ci)
            {
                blackoutDates = ci.BlackoutDates;
            }

            if (blackoutDates != null)
            {
                if (e.OldValue is not null && e.OldValue is DateTime oldDateTime)
                {
                    //移除旧值
                    var findResult = blackoutDates.Where(p => p.Start == oldDateTime.AddDays(1) && p.End == DateTime.MaxValue).ToList();
                    if (findResult.Any())
                    {
                        foreach (var item in findResult)
                        {
                            blackoutDates.Remove(item);
                        }
                    }
                }
                if (e.NewValue is not null && e.NewValue is DateTime newDateTime)
                {
                    //添加新值
                    blackoutDates.Add(new CalendarDateRange(newDateTime.AddDays(1), DateTime.MaxValue));
                }
            }
        }
        #endregion 允许使用的最大时间 - 附加属性 

        #region 显示快捷按钮 - 附加属性
        /// <summary>
        /// 显示快捷按钮 - 获取附加属性
        /// </summary>                
        public static bool? GetShowShortcutButton(DependencyObject obj)
        {
            return (bool?) obj.GetValue(ShowShortcutButtonProperty);
        }

        /// <summary>
        /// 显示快捷按钮 - 设置附加属性
        /// </summary>  
        public static void SetShowShortcutButton(DependencyObject obj, bool? value)
        {
            obj.SetValue(ShowShortcutButtonProperty, value);
        }

        /// <summary>
        /// 显示快捷按钮 - 注册附加属性 
        /// <para>使用DependencyProperty作为MyProperty的后台存储。这支持动画、样式、绑定等。</para>
        /// </summary>
        public static readonly DependencyProperty ShowShortcutButtonProperty =
            DependencyProperty.RegisterAttached(
                "ShowShortcutButton",
                typeof(bool?),
                typeof(CalendarElement),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnShowShortcutButtonChanged));

        /// <summary>
        /// 显示快捷按键命令列表
        /// </summary>
        private static readonly List<ICommand> ShowShortcutButtonCommandList =
        [
            ControlCommands.Today,
            ControlCommands.Yesterday,
            ControlCommands.Tomorrow,
            ControlCommands.FirstDayOfWeek,
            ControlCommands.ThisWeek,
            ControlCommands.PrevWeek,
            ControlCommands.NextWeek,
            ControlCommands.LastDayOfWeek,
            ControlCommands.FirstDayOfMonth,
            ControlCommands.ThisMonth,
            ControlCommands.PrevMonth,
            ControlCommands.NextMonth,
            ControlCommands.LastDayOfMonth,
            ControlCommands.FirstDayOfYear,
            ControlCommands.ThisYear,
            ControlCommands.PrevYear,
            ControlCommands.NextYear,
            ControlCommands.LastDayOfYear
        ];

        /// <summary>
        /// 当 显示快捷按钮 - 附加属性发生变更时
        /// </summary>
        private static void OnShowShortcutButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool newValue)
            {
                CommandBindingCollection? commandBindingCollection = null;
                if (d is Calendar calendar)
                {
                    commandBindingCollection = calendar.CommandBindings;
                }
                else if (d is System.Windows.Controls.DatePicker datePicker)
                {
                    commandBindingCollection = datePicker.CommandBindings;
                }
                else if (d is DateTimePicker dateTimePicker)
                {
                    commandBindingCollection = dateTimePicker.CommandBindings;
                    if (!newValue)
                    {
                        dateTimePicker.CalendarWithClock.SetValue(CalendarElement.ShowShortcutButtonProperty, newValue);
                    }
                }
                else if (d is CalendarWithClock calendarWithClock)
                {
                    commandBindingCollection = calendarWithClock.CommandBindings;
                    //commandBindingCollection = calendarWithClock.Calendar.CommandBindings;
                }

                if (newValue && commandBindingCollection != null)
                {
                    foreach (var item in ShowShortcutButtonCommandList)
                    {
                        commandBindingCollection.Add(new CommandBinding(item, ExecuteShortcutButton, CanExecuteShortcutButton));
                    }
                }
                else
                {
                    //移除已绑定的命令
                    if (commandBindingCollection != null)
                    {
                        for (int i = commandBindingCollection.Count - 1; i >= 0; i--)
                        {
                            var commandBinding = commandBindingCollection[i];
                            if (ShowShortcutButtonCommandList.Contains(commandBinding.Command))
                            {
                                commandBindingCollection.RemoveAt(i);
                            }
                        }
                    }
                }
            }

        }
        #endregion 显示快捷按钮 - 附加属性 

        #region 快捷键相关方法

        /// <summary>
        /// 执行快捷按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ExecuteShortcutButton(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Source is Calendar calendar)
            {
                var setDateTime = CalendarElement.ExecuteShortcutButton(sender, e, calendar.SelectedDate, calendar.BlackoutDates);
                calendar.SelectedDate = setDateTime;
            }
            else if (e.Source is System.Windows.Controls.DatePicker datePicker)
            {
                var setDateTime = CalendarElement.ExecuteShortcutButton(sender, e, datePicker.SelectedDate, datePicker.BlackoutDates);
                datePicker.SelectedDate = setDateTime;
            }
            else if (e.Source is DateTimePicker dateTimePicker)
            {
                var setDateTime = CalendarElement.ExecuteShortcutButton(sender, e, dateTimePicker.SelectedDateTime, dateTimePicker.CalendarWithClock.Calendar.BlackoutDates);
                dateTimePicker.SelectedDateTime = setDateTime;
            }
            else if (e.Source is CalendarWithClock calendarWithClock)
            {
                var setDateTime = CalendarElement.ExecuteShortcutButton(sender, e, calendarWithClock.Calendar.SelectedDate, calendarWithClock.Calendar.BlackoutDates);
                //Todo 需后期观察实际情况，决定是否需要溴冷 SelectedDate
                //calendarWithClock.Calendar.SelectedDate = setDateTime;
                //calendarWithClock.SelectedDateTime = setDateTime;
                if (setDateTime != null)
                {
                    calendarWithClock.Calendar.DisplayDate = setDateTime.GetValueOrDefault();
                    calendarWithClock.DisplayDateTime = setDateTime.GetValueOrDefault();
                }
            }
        }


        /// <summary>
        /// 判定能否执行快捷按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CanExecuteShortcutButton(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Source is Calendar calendar)
            {
                CalendarElement.CanExecuteShortcutButton(sender, e, calendar.SelectedDate, calendar.BlackoutDates);
            }
            else if (e.Source is System.Windows.Controls.DatePicker datePicker)
            {
                CalendarElement.CanExecuteShortcutButton(sender, e, datePicker.SelectedDate, datePicker.BlackoutDates);
            }
            else if (e.Source is CalendarWithClock calendarWithClock)
            {
                CalendarElement.CanExecuteShortcutButton(sender, e, calendarWithClock.Calendar.SelectedDate, calendarWithClock.Calendar.BlackoutDates);
            }
            else if (e.Source is DateTimePicker dateTimePicker)
            {
                CalendarElement.CanExecuteShortcutButton(sender, e, dateTimePicker.CalendarWithClock.Calendar.SelectedDate, dateTimePicker.CalendarWithClock.Calendar.BlackoutDates);
            }
        }

        /// <summary>
        /// 获取快捷按钮设置的时间
        /// </summary>
        /// <param name="routedCommandName"></param>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        public static DateTime? GetShortcutButtonSetDateTime(string routedCommandName, DateTime? selectedDate)
        {
            //当前时间， Flag，后续可以通过获取服务器时间来减少误差
            DateTime nowTime = DateTime.Now;
            selectedDate ??= DateTime.Now;
            DateTime? setDateTime = selectedDate;
            switch (routedCommandName)
            {
                case nameof(ControlCommands.Today):
                    setDateTime = nowTime;
                    break;
                case nameof(ControlCommands.Yesterday):
                    setDateTime = nowTime.AddDays(-1);
                    break;
                case nameof(ControlCommands.Tomorrow):
                    setDateTime = nowTime.AddDays(1);
                    break;
                case nameof(ControlCommands.FirstDayOfWeek):
                    setDateTime = setDateTime.GetValueOrDefault().FirstDayOfWeek();
                    break;
                case nameof(ControlCommands.ThisWeek):
                    setDateTime = nowTime.ThisWeek(setDateTime.GetValueOrDefault());
                    break;
                case nameof(ControlCommands.PrevWeek):
                    setDateTime = setDateTime.GetValueOrDefault().AddDays(-1 * 7);
                    break;
                case nameof(ControlCommands.NextWeek):
                    setDateTime = setDateTime.GetValueOrDefault().AddDays(1 * 7);
                    break;
                case nameof(ControlCommands.LastDayOfWeek):
                    setDateTime = setDateTime.GetValueOrDefault().LastDayOfWeek();
                    break;
                case nameof(ControlCommands.FirstDayOfMonth):
                    setDateTime = setDateTime.GetValueOrDefault().FirstDayOfMonth();
                    break;
                case nameof(ControlCommands.ThisMonth):
                    setDateTime = nowTime.ThisMonth(setDateTime.GetValueOrDefault());
                    break;
                case nameof(ControlCommands.PrevMonth):
                    setDateTime = setDateTime.GetValueOrDefault().AddMonths(-1);
                    break;
                case nameof(ControlCommands.NextMonth):
                    setDateTime = setDateTime.GetValueOrDefault().AddMonths(1);
                    break;
                case nameof(ControlCommands.LastDayOfMonth):
                    setDateTime = setDateTime.GetValueOrDefault().LastDayOfMonth();
                    break;
                case nameof(ControlCommands.FirstDayOfYear):
                    setDateTime = setDateTime.GetValueOrDefault().FirstDayOfYear();
                    break;
                case nameof(ControlCommands.ThisYear):
                    setDateTime = nowTime.ThisYear(setDateTime.GetValueOrDefault());
                    break;
                case nameof(ControlCommands.PrevYear):
                    setDateTime = setDateTime.GetValueOrDefault().AddYears(-1);
                    break;
                case nameof(ControlCommands.NextYear):
                    setDateTime = setDateTime.GetValueOrDefault().AddYears(1);
                    break;
                case nameof(ControlCommands.LastDayOfYear):
                    setDateTime = setDateTime.GetValueOrDefault().LastDayOfYear();
                    break;
            }
            return setDateTime;
        }



        /// <summary>
        /// 判定能否执行快捷按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="selectedDate">选中的日期</param>
        /// <param name="blackoutDates">限制日期</param>
        public static void CanExecuteShortcutButton(object sender, CanExecuteRoutedEventArgs e, DateTime? selectedDate, CalendarBlackoutDatesCollection blackoutDates)
        {
            bool canExecute = false;
            try
            {
                //Flag 后续DateTime.Now 可考虑替换为获取服务器时间，增强经度
                selectedDate ??= DateTime.Now;
                DateTime? setDateTime = selectedDate;
                if (e.Command is RoutedCommand rc)
                {
                    setDateTime = GetShortcutButtonSetDateTime(rc.Name, selectedDate);
                }
                if (setDateTime != null)
                {
                    canExecute = !blackoutDates.Contains(setDateTime.GetValueOrDefault());
                }
                //BlackoutDates.Contains
            }
            catch (Exception)
            {
                canExecute = false;
            }
            finally
            {
                e.CanExecute = canExecute;
            }

        }


        /// <summary>
        /// 执行快捷按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="selectedDate">选中的日期</param>
        /// <param name="blackoutDates">限制日期</param>
        public static DateTime? ExecuteShortcutButton(object sender, ExecutedRoutedEventArgs e, DateTime? selectedDate, CalendarBlackoutDatesCollection blackoutDates)
        {
            //Flag 后续DateTime.Now 可考虑替换为获取服务器时间，增强经度
            selectedDate ??= DateTime.Now;
            DateTime? setDateTime = selectedDate;
            if (e.Command is RoutedCommand rc)
            {
                setDateTime = GetShortcutButtonSetDateTime(rc.Name, selectedDate);
            }
            if (setDateTime != null)
            {
                if (blackoutDates.Contains(setDateTime.GetValueOrDefault()))
                {
                    setDateTime = selectedDate;
                }
            }
            return setDateTime;
        }
        #endregion 快捷键相关方法
    }
}
