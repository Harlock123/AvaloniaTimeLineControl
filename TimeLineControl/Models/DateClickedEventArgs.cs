using System;
using Avalonia.Interactivity;

namespace TimeLineControl.Models;

/// <summary>
/// Event arguments for when a date is clicked on the timeline.
/// </summary>
public class DateClickedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The date that was clicked.
    /// </summary>
    public DateTime DateClicked { get; set; }

    /// <summary>
    /// The line/row number (0-based) that was clicked.
    /// </summary>
    public int LineClicked { get; set; }

    /// <summary>
    /// The metadata associated with any timeline items at this date/line.
    /// </summary>
    public string MetaData { get; set; }

    public DateClickedEventArgs(DateTime dateClicked, int lineClicked, string metaData)
    {
        RoutedEvent = TimeLineControl.DateClickedEvent;
        DateClicked = dateClicked;
        LineClicked = lineClicked;
        MetaData = metaData ?? string.Empty;
    }
}
