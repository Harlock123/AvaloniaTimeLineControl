using System;
using Avalonia.Interactivity;

namespace TimeLineControl.Models;

/// <summary>
/// Event arguments for when a date is hovered over on the timeline.
/// </summary>
public class DateHoveredEventArgs : RoutedEventArgs
{
    /// <summary>
    /// The date being hovered over.
    /// </summary>
    public DateTime DateHovered { get; set; }

    /// <summary>
    /// The line/row number (0-based) being hovered over.
    /// </summary>
    public int LineHovered { get; set; }

    /// <summary>
    /// The metadata associated with any timeline items at this date/line.
    /// </summary>
    public string MetaData { get; set; }

    public DateHoveredEventArgs(DateTime dateHovered, int lineHovered, string metaData)
    {
        RoutedEvent = TimeLineControl.DateHoveredEvent;
        DateHovered = dateHovered;
        LineHovered = lineHovered;
        MetaData = metaData ?? string.Empty;
    }
}
