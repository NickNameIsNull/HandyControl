using System;

namespace HandyControl.Tools.Helper
{
    /// <summary>
    /// 日期时间帮助类
    /// </summary>
    internal static class DateTimeHelper
    {
        #region 使用提示
        //DateTime dateTimeNow = DateTime.Now;
        ////本月第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfMonth(dateTimeNow).ToString("yyyy/MM/dd");

        ////本月最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfMonth(dateTimeNow).ToString("yyyy/MM/dd");

        ////上月第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfPrevMonth(dateTimeNow).ToString("yyyy/MM/dd");

        ////上月最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfPrevMonth(dateTimeNow).ToString("yyyy/MM/dd");

        ////下月第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfNextMonth(dateTimeNow).ToString("yyyy/MM/dd");

        ////下月最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfNextMonth(dateTimeNow).ToString("yyyy/MM/dd");

        ////前三个月第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfAssignMonth(dateTimeNow, -3).ToString("yyyy/MM/dd");

        ////前三个月最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfAssignMonth(dateTimeNow, -3).ToString("yyyy/MM/dd");

        ////后三个月第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfAssignMonth(dateTimeNow, 3).ToString("yyyy/MM/dd");

        ////后三个月的最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfAssignMonth(dateTimeNow, 3).ToString("yyyy/MM/dd");

        ////去年当月的第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfAssignMonthAndYear(dateTimeNow, 0, -1).ToString("yyyy/MM/dd");

        ////去年当月的最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfAssignMonthAndYear(dateTimeNow, 0, -1).ToString("yyyy/MM/dd");

        ////当年的第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfYear(dateTimeNow).ToString("yyyy/MM/dd");

        ////当年的最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfYear(dateTimeNow).ToString("yyyy/MM/dd");

        ////去年的第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfPrevYear(dateTimeNow).ToString("yyyy/MM/dd");

        ////去年的最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfPrevYear(dateTimeNow).ToString("yyyy/MM/dd");

        ////明年的第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfNextYear(dateTimeNow).ToString("yyyy/MM/dd");

        ////明年的最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfNextYear(dateTimeNow).ToString("yyyy/MM/dd");

        ////指定N年前的第一天
        //txtDateHelperTest.Text = DateTimeHelper.FirstDayOfAssignYear(dateTimeNow,-2).ToString("yyyy/MM/dd");

        ////指定N年前的最后一天
        //txtDateHelperTest.Text = DateTimeHelper.LastDayOfAssignYear(dateTimeNow, -2).ToString("yyyy/MM/dd");
        #endregion

        /// <summary>
        /// 获取指定时间所在月份的第一天
        /// </summary>
        /// <param name="dateTime">要取得月份第一天的时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return FirstDayOfAssignMonth(dateTime, 0);
        }

        /// <summary>
        /// 获取指定时间所在月份的最后一天
        /// </summary>
        /// <param name="dateTime">要取得月份最后一天的时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            return LastDayOfAssignMonth(dateTime, 0);
        }

        /// <summary>
        /// 取得上个月第一天
        /// </summary>
        /// <param name="dateTime">要取得上个月第一天的当前时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfPrevMonth(this DateTime dateTime)
        {
            return FirstDayOfAssignMonth(dateTime, -1);
        }

        /// <summary>
        /// 取得上个月的最后一天
        /// </summary>
        /// <param name="dateTime">要取得上个月最后一天的当前时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfPrevMonth(this DateTime dateTime)
        {
            return LastDayOfAssignMonth(dateTime, -1);
        }

        /// <summary>
        /// 取得下个月第一天
        /// </summary>
        /// <param name="dateTime">要取得上个月第一天的当前时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfNextMonth(this DateTime dateTime)
        {
            return FirstDayOfAssignMonth(dateTime, 1);
        }

        /// <summary>
        /// 取得下个月的最后一天
        /// </summary>
        /// <param name="dateTime">要取得上个月最后一天的当前时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfNextMonth(this DateTime dateTime)
        {
            return LastDayOfAssignMonth(dateTime, 1);
        }

        /// <summary>
        /// 获取指定时间指定月份前/后的第一天
        /// </summary>
        /// <param name="dateTime">指定的时间</param>
        /// <param name="iAssignMonthAmount">指定的月份 0为当前月份;负数为前N月;正数为后N月</param>
        /// <returns></returns>
        public static DateTime FirstDayOfAssignMonth(this DateTime dateTime, int iAssignMonthAmount)
        {
            return FirstDayOfAssignMonthAndYear(dateTime, iAssignMonthAmount, 0);
        }

        /// <summary>
        /// 获取指定时间指定月份前/后的最后一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="iAssignMonthAmount"></param>
        /// <returns></returns>
        public static DateTime LastDayOfAssignMonth(this DateTime dateTime, int iAssignMonthAmount)
        {
            return LastDayOfAssignMonthAndYear(dateTime, iAssignMonthAmount, 0);
        }

        /// <summary>
        /// 获取指定时间 所在季度或前/后N季度 的第一天
        /// </summary>
        /// <param name="dateTime">指定的时间</param>
        /// <param name="iAssignQuarter">指定季度 0为当前季度;负数为前N季度;正数为后N季度</param>
        /// <returns></returns>
        public static DateTime FirstDayOfAssignQuarter(this DateTime dateTime, int iAssignQuarter)
        {
            return dateTime.AddMonths(0 - (dateTime.Month - 1) % 3).AddDays(1 - dateTime.Day).AddMonths(iAssignQuarter * 3);
        }

        /// <summary>
        /// 获取指定时间 所季度或前/后N季度 的最后一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="iAssignQuarter">指定季度 0为当前季度;负数为前N季度;正数为后N季度</param>
        /// <returns></returns>
        public static DateTime LastDayOfAssignQuarter(this DateTime dateTime, int iAssignQuarter)
        {
            return dateTime.FirstDayOfAssignQuarter(iAssignQuarter).AddMonths(3).AddDays(-1);
        }

        /// <summary>
        /// 获取指定日期当年的第一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfYear(this DateTime dateTime)
        {
            return FirstDayOfAssignYear(dateTime, 0);
        }

        /// <summary>
        /// 获取指定日期当年的最后一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfYear(this DateTime dateTime)
        {
            return LastDayOfAssignYear(dateTime, 0);
        }

        /// <summary>
        /// 获取指定日期去年的第一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfPrevYear(this DateTime dateTime)
        {
            return FirstDayOfAssignYear(dateTime, -1);
        }

        /// <summary>
        /// 获取指定日期去年的最后一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfPrevYear(this DateTime dateTime)
        {
            return LastDayOfAssignYear(dateTime, -1);
        }

        /// <summary>
        /// 获取指定日期明年的第一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <returns></returns>
        public static DateTime FirstDayOfNextYear(this DateTime dateTime)
        {
            return FirstDayOfAssignYear(dateTime, 1);
        }

        /// <summary>
        /// 获取指定日期明年的最后一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <returns></returns>
        public static DateTime LastDayOfNextYear(this DateTime dateTime)
        {
            return LastDayOfAssignYear(dateTime, 1);
        }

        /// <summary>
        /// 获取指定日期 指定年份 前/后 的第一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <param name="iAssignYearAmount">指定的年份 0为当前年份;负数为前N年;正数为后N年</param>
        /// <returns></returns>
        public static DateTime FirstDayOfAssignYear(this DateTime dateTime, int iAssignYearAmount)
        {
            return FirstDayOfAssignMonthAndYear(dateTime, 1 - dateTime.Month, iAssignYearAmount);
        }

        /// <summary>
        /// 获取指定日期 指定年份 前/后 的最后一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <param name="iAssignYearAmount">指定的年份 0为当前年份;负数为前N年;正数为后N年</param>
        /// <returns></returns>
        public static DateTime LastDayOfAssignYear(this DateTime dateTime, int iAssignYearAmount)
        {
            return LastDayOfAssignMonthAndYear(dateTime, 12 - dateTime.Month, iAssignYearAmount);
        }

        /// <summary>
        /// 获取指定日期所在星期的第一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime FirstDayOfWeek(this DateTime dateTime)
        {
            int weekNow = ((int) dateTime.DayOfWeek);
            weekNow = (weekNow == 0 ? (7 - 1) : (weekNow - 1));
            int dayDiff = (-1) * weekNow;
            return dateTime.AddDays(dayDiff);
        }

        /// <summary>
        /// 获取指定日期所在周 前/后 N周 的第一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <param name="iAssignWeekAmount">0:当前周,正数：N周后的第一天，父数：N周前的第一天，</param>
        /// <returns></returns>
        public static DateTime FirstDayOfAssignWeek(this DateTime dateTime, int iAssignWeekAmount = 0)
        {
            return FirstDayOfWeek(dateTime).AddDays(7 * iAssignWeekAmount);
        }

        /// <summary>
        /// 获取指定日期所在周 前/后 N周 的最后一天
        /// </summary>
        /// <param name="dateTime">指定时间</param>
        /// <param name="iAssignWeekAmount">0:当前周,正数：N周后的第一天，父数：N周前的最后，</param>
        /// <returns></returns>
        public static DateTime LastDayOfAssignWeek(this DateTime dateTime, int iAssignWeekAmount = 0)
        {
            return LastDayOfWeek(dateTime).AddDays(7 * iAssignWeekAmount);
        }

        /// <summary>
        /// 获取指定日期所在星期的最后一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime LastDayOfWeek(this DateTime dateTime)
        {
            int weekNow = ((int) dateTime.DayOfWeek);
            weekNow = (weekNow == 0 ? 7 : weekNow);
            int dayDiff = (7 - weekNow);
            return dateTime.AddDays(dayDiff);
        }

        /// <summary>
        /// 获取参数sourceDateTime在dateTime中对应的周几的日期
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sourceDateTime"></param>
        /// <returns></returns>
        public static DateTime ThisWeek(this DateTime dateTime, DateTime sourceDateTime)
        {
            int sourceDateTimeWeek = ((int) sourceDateTime.DayOfWeek);
            sourceDateTimeWeek = (sourceDateTimeWeek == 0 ? (7 - 1) : (sourceDateTimeWeek - 1));
            //int dateTimeWeek = ((int)dateTime.DayOfWeek);
            DateTime dateTimeFirstDayOfWeek = FirstDayOfWeek(dateTime);
            return dateTimeFirstDayOfWeek.AddDays(sourceDateTimeWeek);
        }

        /// <summary>
        /// 获取参数sourceDateTime在dateTime中对应的月的所在日
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sourceDateTime"></param>
        /// <returns></returns>
        public static DateTime ThisMonth(this DateTime dateTime, DateTime sourceDateTime)
        {
            //闰年2月29日
            if (sourceDateTime.Month == 2
                && DateTime.IsLeapYear(sourceDateTime.Year)
                && dateTime.Month == 2
                && sourceDateTime.Day == 29)
            {
                sourceDateTime = sourceDateTime.AddDays(-1);
            }
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                sourceDateTime.Day,
                sourceDateTime.Hour,
                sourceDateTime.Minute,
                sourceDateTime.Second,
                sourceDateTime.Millisecond);
        }

        /// <summary>
        /// 获取参数sourceDateTime在dateTime中对应的所在月及所在日
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="sourceDateTime"></param>
        /// <returns></returns>
        public static DateTime ThisYear(this DateTime dateTime, DateTime sourceDateTime)
        {
            //闰年2月29日
            if (sourceDateTime.Month == 2
                && DateTime.IsLeapYear(sourceDateTime.Year)
                && sourceDateTime.Day == 29)
            {
                sourceDateTime = sourceDateTime.AddDays(-1);
            }
            return new DateTime(
                dateTime.Year,
                sourceDateTime.Month,
                sourceDateTime.Day,
                sourceDateTime.Hour,
                sourceDateTime.Minute,
                sourceDateTime.Second,
                sourceDateTime.Millisecond);
        }

        /// <summary>
        /// 获取指定时间指定 年份 前/后 和 月份前/后的第一天
        /// </summary>
        /// <param name="dateTime">指定的时间</param>
        /// <param name="iAssignMonthAmount">指定的月份 0为当前月份;负数为前N月;正数为后N月</param>
        /// <param name="iAssignYearAmount">指定的年份 0为当前年份;负数为前N年;正数为后N年</param>
        /// <returns></returns>
        public static DateTime FirstDayOfAssignMonthAndYear(this DateTime dateTime, int iAssignMonthAmount, int iAssignYearAmount)
        {
            return dateTime.AddDays(1 - dateTime.Day).AddMonths(iAssignMonthAmount).AddYears(iAssignYearAmount);
        }

        /// <summary>
        /// 获取指定时间指定 年份 前/后 和 月份前/后的最后一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="iAssignMonthAmount"></param>
        /// <param name="iAssignYearAmount"></param>
        /// <returns></returns>
        public static DateTime LastDayOfAssignMonthAndYear(this DateTime dateTime, int iAssignMonthAmount, int iAssignYearAmount)
        {
            return dateTime.AddDays(1 - dateTime.Day).AddMonths(1 + iAssignMonthAmount).AddDays(-1).AddYears(iAssignYearAmount);
        }

        /// <summary>
        /// 每日开始时间，即 yyyy-MM-dd 00:00:00.0000
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime DayStartTime(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }


        /// <summary>
        /// 每日结束时间，即 yyyy-MM-dd 23:59:59.9999
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime DayEndTime(this DateTime dateTime)
        {
            return DayStartTime(dateTime).AddDays(1).AddMilliseconds(-1);
        }

    }
}
