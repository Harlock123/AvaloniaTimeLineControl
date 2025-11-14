using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TimeLineControl.Models;

namespace TimeLineControl.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Timeline.MarginScale = 1;
        Timeline.DateClicked += OnDateClicked;
        Timeline.DateHovered += OnDateHovered;
    }

    private void OnSet3Rows(object? sender, RoutedEventArgs e)
    {
        Timeline.NumRows = 3;
    }

    private void OnSet4Rows(object? sender, RoutedEventArgs e)
    {
        Timeline.NumRows = 4;
    }

    private void OnSet5Rows(object? sender, RoutedEventArgs e)
    {
        Timeline.NumRows = 5;
    }

    private void OnDaySizeUp(object? sender, RoutedEventArgs e)
    {
        Timeline.DaySize++;
    }

    private void OnDaySizeDown(object? sender, RoutedEventArgs e)
    {
        Timeline.DaySize--;
    }

    private void OnForwardDay(object? sender, RoutedEventArgs e)
    {
        Timeline.StartDate = Timeline.StartDate.AddDays(1);
    }

    private void OnBackDay(object? sender, RoutedEventArgs e)
    {
        Timeline.StartDate = Timeline.StartDate.AddDays(-1);
    }

    private void OnToggleTestRender(object? sender, RoutedEventArgs e)
    {
        Timeline.TestRender = !Timeline.TestRender;
    }

    private void OnLoadSampleData(object? sender, RoutedEventArgs e)
    {
        Timeline.ClearAllDataItems();

        var baseDate = new DateTime(2021, 4, 1);

        Timeline.AddDataItem(new TimeLineDataItem(1, ChickletStyles.Chicklet_BlueTriangle,
            new DateTime(2021, 4, 1), new DateTime(2021, 4, 30), "Blue Triangle - April"));
        Timeline.AddDataItem(new TimeLineDataItem(1, ChickletStyles.Chicklet_BlueTriangle,
            new DateTime(2021, 4, 7), new DateTime(2021, 4, 21), "Blue Triangle - Week 2-3 (Overlap)"));

        Timeline.AddDataItem(new TimeLineDataItem(2, ChickletStyles.Chicklet_GoldTriangle,
            new DateTime(2021, 4, 1), new DateTime(2021, 4, 30), "Gold Triangle - Line 2"));

        Timeline.AddDataItem(new TimeLineDataItem(3, ChickletStyles.Chicklet_OrangeCircle,
            new DateTime(2021, 4, 1), new DateTime(2021, 4, 30), "Orange Circle - Line 3"));

        Timeline.AddDataItem(new TimeLineDataItem(4, ChickletStyles.Chicklet_PurpleTriangle,
            new DateTime(2021, 4, 1), new DateTime(2021, 4, 30), "Purple Triangle - Line 4"));

        Timeline.AddDataItem(new TimeLineDataItem(5, ChickletStyles.Chicklet_GreenCircle,
            new DateTime(2021, 4, 1), new DateTime(2021, 4, 30), "Green Circle - Line 5"));

        Timeline.AddDataItem(new TimeLineDataItem(3, ChickletStyles.Chicklet_WhiteBox,
            new DateTime(2015, 12, 1), new DateTime(2015, 12, 31), "Old Data - December 2015"));

        Timeline.SetLineLabel(0, "Custom Line 1");
        Timeline.SetLineLabel(1, "Custom Line 2");
        Timeline.SetLineLabel(2, "Custom Line 3");
        Timeline.SetLineLabel(3, "Custom Line 4");
        Timeline.SetLineLabel(4, "Custom Line 5");

        Timeline.StartDate = new DateTime(2021, 4, 1);
    }

    private void OnDateClicked(object? sender, DateClickedEventArgs e)
    {
        ClickedDateText.Text = $"Clicked Date: {e.DateClicked:yyyy-MM-dd} on Line {e.LineClicked} - {e.MetaData}";
    }

    private void OnDateHovered(object? sender, DateHoveredEventArgs e)
    {
        HoverDateText.Text = $"Hover Date: {e.DateHovered:yyyy-MM-dd} on Line {e.LineHovered}";
    }
}
