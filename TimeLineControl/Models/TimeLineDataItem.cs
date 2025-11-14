using System;

namespace TimeLineControl.Models;

/// <summary>
/// Represents a data item to be displayed on the timeline.
/// </summary>
public class TimeLineDataItem
{
    /// <summary>
    /// The line/row number (1-based) where this item should be displayed.
    /// </summary>
    public int LineID { get; set; }

    /// <summary>
    /// The visual style (shape and color) for rendering this item.
    /// </summary>
    public ChickletStyles RenderStyle { get; set; }

    /// <summary>
    /// The start date for this timeline item.
    /// </summary>
    public DateTime BeginDate { get; set; }

    /// <summary>
    /// The end date for this timeline item.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Additional metadata/information associated with this item.
    /// Displayed in tooltips and available in click/hover events.
    /// </summary>
    public string MetaData { get; set; }

    public TimeLineDataItem()
    {
        MetaData = string.Empty;
    }

    public TimeLineDataItem(int lineId, ChickletStyles renderStyle, DateTime beginDate, DateTime endDate, string metaData)
    {
        LineID = lineId;
        RenderStyle = renderStyle;
        BeginDate = beginDate;
        EndDate = endDate;
        MetaData = metaData ?? string.Empty;
    }
}
