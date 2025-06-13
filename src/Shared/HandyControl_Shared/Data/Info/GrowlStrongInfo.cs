using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HandyControl.Data;

public class GrowlStrongInfo: GrowlInfo
{
    /// <summary>
    /// Growl的宽度，默认为320
    /// </summary>
    public double GrowlMaxWidth { get; set; } = 320d;

    /// <summary>
    /// 是否显示遮罩,仅对未指定GrowlPanel的情况下有效
    /// </summary>
    public bool ShowMask { get; set; } = false;

    /// <summary>
    /// GrowlPanel 中 Growl 的显示位置，默认值：<see cref="GrowlShowPosition.Default"/>,仅对未指定GrowlPanel的情况下有效
    /// </summary>
    public GrowlShowPosition ShowPosition { get; set; } = GrowlShowPosition.Default;
}
