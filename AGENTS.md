# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers the features, usage examples and the
public API; this file covers how the code is put together and the rules that keep it working.

## What this is

A timeline control for Avalonia 11 (11.2.2), in C# on .NET 8, ported from a TypeScript `Timeline.ts` component. It
draws up to ten rows of dated "chicklets" (10 colours by 3 shapes) with weekend shading, month labels, overlap
markers, and click and hover events. The library is packaged for NuGet as `AvaloniaTimeLineControl`, and a small demo
app shows it off.

## Commands

```bash
dotnet build TimeLineControl.sln                       # needs the .NET 8 SDK or later
dotnet run --project TimeLineControl.Demo              # the demo window
dotnet pack TimeLineControl/TimeLineControl.csproj -c Release   # the NuGet package (GeneratePackageOnBuild is off)
```

There are no tests yet.

## Layout

- `TimeLineControl/`: the library (namespace `TimeLineControl`).
  - `TimeLineControl.axaml.cs`: the whole control, a `TemplatedControl` that draws itself in `Render`.
    - `RedrawTimeline` draws the grid, date header, line labels and chicklets. `RenderChicklet` and the
      `DrawBox`/`DrawCircle`/`DrawTriangle`/`DrawDogEar` helpers draw each item.
    - Pointer handlers: the wheel scrolls a week at a time (Ctrl+wheel changes `DaySize`); moving raises
      `DateHovered`; pressing raises `DateClicked`.
    - `AddDataItem`, `ClearAllDataItems`, `ClearSpecificLine`, `SetLineLabel` and `GetLineLabel` are the data API.
      `UpdateDateRange` and `RecreateLineBitmaps` rebuild the per-line day maps used for overlap detection.
  - `TimeLineControl.axaml`: a `ControlTheme` with the default background.
  - `Models/`: `TimeLineDataItem` (line, style, begin and end dates, metadata), the `ChickletStyles` enum, and the
    two event-argument classes.
- `TimeLineControl.Demo/`: an Avalonia desktop app whose `MainWindow` fills the control with sample data and logs
  its events.
- `ScreenShots/`: the pictures the README links to.

## Rules that keep it working

- **Plain properties, not styled properties.** The settings (`NumRows`, `DaySize`, `StartDate`, the colours and so
  on) are CLR properties that clamp the value and call `InvalidateVisual()`. They cannot be bound or styled. A new
  setting should follow the same pattern, or the whole set should move to `StyledProperty` together.
- **`Margin` is the control's own `double`.** It hides Avalonia's `Thickness Margin` on this type, so XAML sets the
  timeline's inner margin, not the layout margin.
- **Line ids are 1 to 10.** `NumRows` is clamped to 1..10 and the day maps are keyed 1..10.
- **Whole days.** `AddDataItem` moves `BeginDate` to 00:00:01 and `EndDate` to 23:59:59. Date maths elsewhere assumes
  that.
- **Every data change rebuilds the maps.** A new data method must call `UpdateDateRange()`, `RecreateLineBitmaps()`
  and `InvalidateVisual()`, as `AddDataItem` does, or overlap markers and metadata lookups go stale.
- **Styles live in two places.** `ChickletStyles` values 0-29 are colour plus 10 times shape, and `RenderChicklet`
  has one `case` per value. A new style needs both; add it at the end so existing numbers keep their meaning.

## Making a change

- Check changes by running the demo; there are no automated tests.
- Keep `README.md` (features and the API reference) in step with public changes.
- For a NuGet release, bump `<Version>` in `TimeLineControl/TimeLineControl.csproj`, then pack.

## CI and releases

There is no CI workflow. Packages are built and published by hand.
