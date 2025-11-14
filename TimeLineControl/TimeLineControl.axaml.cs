using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using TimeLineControl.Models;

namespace TimeLineControl;

public partial class TimeLineControl : TemplatedControl
{
    // Configuration Properties
    private int _numRows = 5;
    private double _daySize = 15;
    private double _margin = 30;
    private double _marginScale = 1.5;
    private DateTime _startDate = DateTime.Now;
    private bool _testRender = false;

    // Colors
    private readonly Color _lineColor = Color.Parse("#000000");
    private readonly Color _lineColorLight = Color.Parse("#7f7f7f");
    private readonly Color _backgroundColor = Color.Parse("#DEF3C9");

    // State
    private DateTime? _hoverDate = null;
    private int _hoverLine = 0;
    private DateTime _minDate = DateTime.Now;
    private DateTime _maxDate = DateTime.Now;
    private int _lineOver = -1;

    // Data
    private readonly List<TimeLineDataItem> _lineData = new();
    private readonly List<string> _lineLabels = new() { "LINE 1", "LINE 2", "LINE 3", "LINE 4", "LINE 5" };
    private readonly Dictionary<int, int?[]> _lineBitmaps = new();

    // Calculated values
    private double _totWidth = 0;
    private double _totHeight = 0;
    private int _daysAcross = 0;

    // Touch handling
    private Point? _touchStart = null;

    // Routed Events
    public static readonly RoutedEvent<DateClickedEventArgs> DateClickedEvent =
        RoutedEvent.Register<TimeLineControl, DateClickedEventArgs>(nameof(DateClicked), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<DateHoveredEventArgs> DateHoveredEvent =
        RoutedEvent.Register<TimeLineControl, DateHoveredEventArgs>(nameof(DateHovered), RoutingStrategies.Bubble);

    public event EventHandler<DateClickedEventArgs>? DateClicked
    {
        add => AddHandler(DateClickedEvent, value);
        remove => RemoveHandler(DateClickedEvent, value);
    }

    public event EventHandler<DateHoveredEventArgs>? DateHovered
    {
        add => AddHandler(DateHoveredEvent, value);
        remove => RemoveHandler(DateHoveredEvent, value);
    }

    // Public Properties
    public int NumRows
    {
        get => _numRows;
        set
        {
            _numRows = Math.Max(1, Math.Min(10, value));
            InvalidateVisual();
        }
    }

    public double DaySize
    {
        get => _daySize;
        set
        {
            _daySize = Math.Max(5, value);
            InvalidateVisual();
        }
    }

    public double Margin
    {
        get => _margin;
        set
        {
            _margin = Math.Max(0, value);
            InvalidateVisual();
        }
    }

    public double MarginScale
    {
        get => _marginScale;
        set
        {
            _marginScale = value;
            InvalidateVisual();
        }
    }

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            _startDate = value;
            InvalidateVisual();
        }
    }

    public bool TestRender
    {
        get => _testRender;
        set
        {
            _testRender = value;
            InvalidateVisual();
        }
    }

    public IReadOnlyList<string> LineLabels => _lineLabels.AsReadOnly();

    public TimeLineControl()
    {
        Background = new SolidColorBrush(_backgroundColor);
        ClipToBounds = true;

        InitializeLineBitmaps();

        PointerWheelChanged += OnPointerWheelChanged;
        PointerMoved += OnPointerMoved;
        PointerPressed += OnPointerPressed;
        PointerExited += OnPointerExited;
    }

    private void InitializeLineBitmaps()
    {
        for (int i = 1; i <= 10; i++)
        {
            _lineBitmaps[i] = Array.Empty<int?>();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        _totWidth = Bounds.Width;
        _totHeight = Bounds.Height;

        // Draw background
        context.FillRectangle(new SolidColorBrush(_backgroundColor), new Rect(0, 0, _totWidth, _totHeight));

        RedrawTimeline(context);
    }

    private void RedrawTimeline(DrawingContext context)
    {
        _daysAcross = (int)Math.Floor((_totWidth - (_margin * 2)) / _daySize);
        double innerRegionHeight = Math.Floor(_totHeight - (_margin * 2));

        int currentMonth = _startDate.Month;

        // Draw timeline grid
        for (int dayIndex = 0; dayIndex < _daysAcross; dayIndex++)
        {
            double x = _margin + (dayIndex * _daySize);
            DateTime currentDate = _startDate.AddDays(dayIndex);
            int dayOfMonth = currentDate.Day - 1;

            for (int lineIndex = 0; lineIndex < _numRows; lineIndex++)
            {
                double y = (_margin * _marginScale) + ((innerRegionHeight / _numRows) * lineIndex);

                // Label the start date
                if (dayIndex == 0 && lineIndex == 0)
                {
                    DrawDateLabel(context, currentDate, x, y);
                }

                // Label month transitions
                if (currentDate.Month > currentMonth && dayIndex > 10 && currentDate.Day == 1 && lineIndex == 0)
                {
                    currentMonth = currentDate.Month;
                    DrawDateLabel(context, currentDate, x, y);
                }

                // Highlight weekends
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    var weekendBrush = new SolidColorBrush(Color.Parse("#BFBFBF"));
                    context.FillRectangle(weekendBrush, new Rect(x, y + _daySize, _daySize, _daySize / 3));
                }

                // Render chicklet
                if (_testRender)
                {
                    RenderChicklet(context, (ChickletStyles)dayOfMonth, x, y, false);
                }
                else
                {
                    int dayOffset = (int)(_startDate - _minDate).TotalDays + dayIndex;
                    if (_lineBitmaps.TryGetValue(lineIndex + 1, out var lineBitmap) &&
                        dayOffset >= 0 && dayOffset < lineBitmap.Length)
                    {
                        var renderData = lineBitmap[dayOffset];
                        if (renderData.HasValue)
                        {
                            bool dogEar = renderData.Value > 99;
                            ChickletStyles style = (ChickletStyles)(dogEar ? renderData.Value - 100 : renderData.Value);
                            RenderChicklet(context, style, x, y, dogEar);
                        }
                        else
                        {
                            DrawBlankChicklet(context, x, y);
                        }
                    }
                    else
                    {
                        DrawBlankChicklet(context, x, y);
                    }
                }
            }
        }

        // Draw line labels
        for (int lineIndex = 0; lineIndex < _numRows; lineIndex++)
        {
            double y = (_margin * _marginScale) + ((innerRegionHeight / _numRows) * lineIndex);
            if (lineIndex < _lineLabels.Count)
            {
                var text = new FormattedText(
                    _lineLabels[lineIndex],
                    new CultureInfo("en-US"),
                    FlowDirection.LeftToRight,
                    new Typeface("Courier New"),
                    11,
                    Brushes.Black);
                context.DrawText(text, new Point(_margin, y + _daySize + 10));
            }
        }

        // Draw hover indicator
        if (_hoverDate.HasValue)
        {
            DrawHoverIndicator(context);
        }
    }

    private void DrawDateLabel(DrawingContext context, DateTime date, double x, double y)
    {
        string dateStr = $"{date.Month}/{date.Day}/{date.Year}";
        var text = new FormattedText(
            dateStr,
            new CultureInfo("en-US"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            10,
            Brushes.Black);

        double offset = (text.Width - _daySize) / 2;
        context.DrawText(text, new Point(x - offset, y - 18));

        var pen = new Pen(new SolidColorBrush(_lineColor), 1);
        context.DrawLine(pen, new Point(x + (_daySize / 2), y), new Point(x + (_daySize / 2), y - 7));
    }

    private void DrawHoverIndicator(DrawingContext context)
    {
        if (!_hoverDate.HasValue) return;

        int dayOffset = (int)(_hoverDate.Value - _startDate).TotalDays;
        if (dayOffset < 0 || dayOffset >= _daysAcross) return;

        double x = _margin + (dayOffset * _daySize);

        // Draw crosshair
        var pen = new Pen(new SolidColorBrush(Colors.Red), 1);
        context.DrawLine(pen, new Point(x, 0), new Point(x, _totHeight));

        // Draw date text
        string dateStr = $"{_hoverDate.Value.Month}/{_hoverDate.Value.Day}/{_hoverDate.Value.Year}";
        var text = new FormattedText(
            dateStr,
            new CultureInfo("en-US"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            12,
            Brushes.Black);

        context.DrawText(text,
            new Point(_totWidth - text.Width, _totHeight - 14));
    }

    private void RenderChicklet(DrawingContext context, ChickletStyles style, double x, double y, bool dogEar)
    {
        switch (style)
        {
            case ChickletStyles.Chicklet_RedBox: DrawBox(context, x, y, "#FF0000"); break;
            case ChickletStyles.Chicklet_GreenBox: DrawBox(context, x, y, "#00FF00"); break;
            case ChickletStyles.Chicklet_BlueBox: DrawBox(context, x, y, "#0000FF"); break;
            case ChickletStyles.Chicklet_YellowBox: DrawBox(context, x, y, "#FFFF00"); break;
            case ChickletStyles.Chicklet_PurpleBox: DrawBox(context, x, y, "#B000B0"); break;
            case ChickletStyles.Chicklet_OrangeBox: DrawBox(context, x, y, "#FFA500"); break;
            case ChickletStyles.Chicklet_GoldBox: DrawBox(context, x, y, "#FFD700"); break;
            case ChickletStyles.Chicklet_BlackBox: DrawBox(context, x, y, "#101010", true); break;
            case ChickletStyles.Chicklet_WhiteBox: DrawBox(context, x, y, "#F0F0F0"); break;
            case ChickletStyles.Chicklet_GreyBox: DrawBox(context, x, y, "#808080"); break;
            case ChickletStyles.Chicklet_RedCircle: DrawCircle(context, x, y, "#FF0000"); break;
            case ChickletStyles.Chicklet_GreenCircle: DrawCircle(context, x, y, "#00FF00"); break;
            case ChickletStyles.Chicklet_BlueCircle: DrawCircle(context, x, y, "#0000FF"); break;
            case ChickletStyles.Chicklet_YellowCircle: DrawCircle(context, x, y, "#FFFF00"); break;
            case ChickletStyles.Chicklet_PurpleCircle: DrawCircle(context, x, y, "#B000B0"); break;
            case ChickletStyles.Chicklet_OrangeCircle: DrawCircle(context, x, y, "#FFA500"); break;
            case ChickletStyles.Chicklet_GoldCircle: DrawCircle(context, x, y, "#FFD700"); break;
            case ChickletStyles.Chicklet_BlackCircle: DrawCircle(context, x, y, "#101010"); break;
            case ChickletStyles.Chicklet_WhiteCircle: DrawCircle(context, x, y, "#F0F0F0"); break;
            case ChickletStyles.Chicklet_GreyCircle: DrawCircle(context, x, y, "#808080"); break;
            case ChickletStyles.Chicklet_RedTriangle: DrawTriangle(context, x, y, "#FF0000"); break;
            case ChickletStyles.Chicklet_GreenTriangle: DrawTriangle(context, x, y, "#00FF00"); break;
            case ChickletStyles.Chicklet_BlueTriangle: DrawTriangle(context, x, y, "#0000FF"); break;
            case ChickletStyles.Chicklet_YellowTriangle: DrawTriangle(context, x, y, "#FFFF00"); break;
            case ChickletStyles.Chicklet_PurpleTriangle: DrawTriangle(context, x, y, "#B000B0"); break;
            case ChickletStyles.Chicklet_OrangeTriangle: DrawTriangle(context, x, y, "#FFA500"); break;
            case ChickletStyles.Chicklet_GoldTriangle: DrawTriangle(context, x, y, "#FFD700"); break;
            case ChickletStyles.Chicklet_BlackTriangle: DrawTriangle(context, x, y, "#101010"); break;
            case ChickletStyles.Chicklet_WhiteTriangle: DrawTriangle(context, x, y, "#F0F0F0"); break;
            case ChickletStyles.Chicklet_GreyTriangle: DrawTriangle(context, x, y, "#808080"); break;
            default: DrawBlankChicklet(context, x, y); break;
        }

        if (dogEar)
        {
            DrawDogEar(context, x, y);
        }
    }

    private void DrawBlankChicklet(DrawingContext context, double x, double y)
    {
        var pen = new Pen(new SolidColorBrush(_lineColor), 1);
        context.DrawRectangle(null, pen, new Rect(x, y, _daySize, _daySize));
    }

    private void DrawBox(DrawingContext context, double x, double y, string colorHex, bool useLight = false)
    {
        var brush = new SolidColorBrush(Color.Parse(colorHex));
        var pen = new Pen(new SolidColorBrush(useLight ? _lineColorLight : _lineColor), 1);
        context.DrawRectangle(brush, pen, new Rect(x, y, _daySize, _daySize));
    }

    private void DrawCircle(DrawingContext context, double x, double y, string colorHex)
    {
        var brush = new SolidColorBrush(Color.Parse(colorHex));
        var pen = new Pen(new SolidColorBrush(_lineColor), 1);
        var center = new Point(x + (_daySize / 2), y + (_daySize / 2));
        context.DrawEllipse(brush, pen, center, _daySize / 2, _daySize / 2);
        context.DrawRectangle(null, pen, new Rect(x, y, _daySize, _daySize));
    }

    private void DrawTriangle(DrawingContext context, double x, double y, string colorHex)
    {
        var brush = new SolidColorBrush(Color.Parse(colorHex));
        var pen = new Pen(new SolidColorBrush(_lineColor), 1);

        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(x, y + _daySize), IsClosed = true };
        figure.Segments.Add(new LineSegment { Point = new Point(x + (_daySize / 2), y) });
        figure.Segments.Add(new LineSegment { Point = new Point(x + _daySize, y + _daySize) });
        geometry.Figures.Add(figure);

        context.DrawGeometry(brush, null, geometry);
        context.DrawRectangle(null, pen, new Rect(x, y, _daySize, _daySize));
    }

    private void DrawDogEar(DrawingContext context, double x, double y)
    {
        var brush = new SolidColorBrush(Color.Parse("#ff99ff"));
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(x + (_daySize / 2), y), IsClosed = true };
        figure.Segments.Add(new LineSegment { Point = new Point(x + _daySize, y) });
        figure.Segments.Add(new LineSegment { Point = new Point(x + _daySize, y + (_daySize / 2)) });
        geometry.Figures.Add(figure);
        context.DrawGeometry(brush, null, geometry);
    }

    // Event Handlers
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        int delta = e.Delta.Y > 0 ? 7 : -7;
        _startDate = _startDate.AddDays(delta);
        InvalidateVisual();
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);

        if (position.X >= _margin && (position.X - _margin) / _daySize <= _daysAcross)
        {
            int dayOffset = (int)Math.Floor((position.X - _margin) / _daySize);
            _hoverDate = _startDate.AddDays(dayOffset);

            double innerRegionHeight = Math.Floor(_totHeight - (_margin * 2));
            _lineOver = -1;

            for (int lineIndex = 0; lineIndex < _numRows; lineIndex++)
            {
                double y = (_margin * _marginScale) + ((innerRegionHeight / _numRows) * lineIndex);

                if (position.Y >= y && position.Y <= y + _daySize)
                {
                    _lineOver = lineIndex;
                    _hoverLine = lineIndex;

                    string metadata = GetMetaDataAt(lineIndex + 1, _hoverDate.Value);
                    ToolTip.SetTip(this, metadata);

                    var args = new DateHoveredEventArgs(_hoverDate.Value, lineIndex, metadata);
                    RaiseEvent(args);
                    break;
                }
            }

            if (_lineOver == -1)
            {
                ToolTip.SetTip(this, null);
            }
        }
        else
        {
            _hoverDate = null;
            ToolTip.SetTip(this, null);
        }

        InvalidateVisual();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _hoverDate = null;
        ToolTip.SetTip(this, null);
        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var position = e.GetPosition(this);

        if (position.X >= _margin && (position.X - _margin) / _daySize <= _daysAcross)
        {
            int dayOffset = (int)Math.Floor((position.X - _margin) / _daySize);
            var clickedDate = _startDate.AddDays(dayOffset);

            double innerRegionHeight = Math.Floor(_totHeight - (_margin * 2));

            for (int lineIndex = 0; lineIndex < _numRows; lineIndex++)
            {
                double y = (_margin * _marginScale) + ((innerRegionHeight / _numRows) * lineIndex);

                if (position.Y >= y && position.Y <= y + _daySize)
                {
                    string metadata = GetMetaDataAt(lineIndex + 1, clickedDate);
                    var args = new DateClickedEventArgs(clickedDate, lineIndex, metadata);
                    RaiseEvent(args);
                    break;
                }
            }
        }
    }

    // Data Management
    public void AddDataItem(TimeLineDataItem item)
    {
        // Normalize times
        item.BeginDate = new DateTime(item.BeginDate.Year, item.BeginDate.Month, item.BeginDate.Day, 0, 0, 1);
        item.EndDate = new DateTime(item.EndDate.Year, item.EndDate.Month, item.EndDate.Day, 23, 59, 59);

        _lineData.Add(item);
        UpdateDateRange();
        RecreateLineBitmaps();
        InvalidateVisual();
    }

    public void ClearAllDataItems()
    {
        _lineData.Clear();
        UpdateDateRange();
        RecreateLineBitmaps();
        InvalidateVisual();
    }

    public void ClearSpecificLine(int lineId)
    {
        _lineData.RemoveAll(item => item.LineID == lineId);
        UpdateDateRange();
        RecreateLineBitmaps();
        InvalidateVisual();
    }

    public void SetLineLabel(int lineId, string label)
    {
        if (lineId >= 0 && lineId < _lineLabels.Count)
        {
            _lineLabels[lineId] = label;
            InvalidateVisual();
        }
    }

    public string GetLineLabel(int lineId)
    {
        return lineId >= 0 && lineId < _lineLabels.Count ? _lineLabels[lineId] : string.Empty;
    }

    private void UpdateDateRange()
    {
        if (_lineData.Count == 0)
        {
            _minDate = _startDate;
            _maxDate = _startDate.AddDays(_daysAcross);
            return;
        }

        _minDate = _lineData.Min(item => item.BeginDate);
        _maxDate = _lineData.Max(item => item.EndDate);
    }

    private void RecreateLineBitmaps()
    {
        // Clear existing bitmaps
        foreach (var key in _lineBitmaps.Keys.ToList())
        {
            _lineBitmaps[key] = Array.Empty<int?>();
        }

        if (_lineData.Count == 0) return;

        int span = (int)(_maxDate - _minDate).TotalDays + 1;

        // Initialize arrays
        for (int i = 1; i <= 10; i++)
        {
            _lineBitmaps[i] = new int?[span];
        }

        // Fill bitmaps
        foreach (var item in _lineData)
        {
            int offset1 = (int)(item.BeginDate - _minDate).TotalDays + 1;
            int offset2 = (int)(item.EndDate - _minDate).TotalDays;

            for (int i = offset1; i <= offset2 && i < span; i++)
            {
                if (i < 0) continue;

                if (_lineBitmaps.TryGetValue(item.LineID, out var bitmap) && bitmap != null)
                {
                    // If already has a value, mark as multiple (add 100)
                    if (bitmap[i].HasValue)
                    {
                        bitmap[i] = (int)item.RenderStyle + 100;
                    }
                    else
                    {
                        bitmap[i] = (int)item.RenderStyle;
                    }
                }
            }
        }
    }

    private string GetMetaDataAt(int lineId, DateTime date)
    {
        var items = _lineData.Where(item =>
            item.LineID == lineId &&
            item.BeginDate <= date &&
            item.EndDate >= date).ToList();

        return items.Count > 0 ? string.Join("\n", items.Select(i => i.MetaData)) : string.Empty;
    }
}
