using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using Origam.UI;

namespace Origam.Windows.Editor.GIT;

public class DiffInfoMargin : AbstractMargin
{
    static readonly Brush AddedBackground;
    static readonly Brush DeletedBackground;
    static readonly Brush BlankBackground;
    static readonly SolidColorBrush BackBrush;
    static readonly SolidColorBrush ForegroundBrush;
    static readonly Pen BorderlessPen;
    const double TextHorizontalMargin = 4.0;
    FormattedText _lineFt,
        _plusMinusFt;

    static DiffInfoMargin()
    {
        AddedBackground = new SolidColorBrush(color: Color.FromRgb(r: 0x6e, g: 0xff, b: 0x6e));
        AddedBackground.Opacity = 0.5;
        AddedBackground.Freeze();
        DeletedBackground = new SolidColorBrush(
            color: Color.FromRgb(
                r: OrigamColorScheme.DirtyColor.R,
                g: OrigamColorScheme.DirtyColor.G,
                b: OrigamColorScheme.DirtyColor.B
            )
        );
        DeletedBackground.Opacity = 0.3;
        DeletedBackground.Freeze();
        BlankBackground = new SolidColorBrush(color: Color.FromRgb(r: 0xfa, g: 0xfa, b: 0xfa));
        BlankBackground.Freeze();
        var transparentBrush = new SolidColorBrush(color: Colors.Transparent);
        transparentBrush.Freeze();
        BorderlessPen = new Pen(brush: transparentBrush, thickness: 0.0);
        BorderlessPen.Freeze();
        BackBrush = new SolidColorBrush(color: Color.FromRgb(r: 255, g: 0, b: 255));
        BackBrush.Freeze();
        ForegroundBrush = new SolidColorBrush(color: Colors.DarkGray);
        ForegroundBrush.Freeze();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Lines == null || Lines.Count == 0)
        {
            return new Size(width: 0.0, height: 0.0);
        }

        var textToUse = Lines.Last().LineNumber.ToString();
        var tf = CreateTypeface();
        _lineFt = new FormattedText(
            textToFormat: textToUse,
            culture: CultureInfo.CurrentCulture,
            flowDirection: FlowDirection.LeftToRight,
            typeface: tf,
            emSize: (double)GetValue(dp: TextBlock.FontSizeProperty),
            foreground: BackBrush,
            pixelsPerDip: VisualTreeHelper.GetDpi(visual: this).PixelsPerDip
        );
        _plusMinusFt = new FormattedText(
            textToFormat: "+ ",
            culture: CultureInfo.CurrentCulture,
            flowDirection: FlowDirection.LeftToRight,
            typeface: tf,
            emSize: (double)GetValue(dp: TextBlock.FontSizeProperty),
            foreground: BackBrush,
            pixelsPerDip: VisualTreeHelper.GetDpi(visual: this).PixelsPerDip
        );
        // NB: This is a bit tricky. We use the margin control to actually
        // draw the diff "+/-" prefix, so that it's not selectable. So, looking
        // at this from the perspective of a single line, the arrangement is:
        //
        // margin-lineFt-margin-lineFt-margin-margin-plusMinusFt
        return new Size(
            width: _lineFt.Width
                + _plusMinusFt.WidthIncludingTrailingWhitespace
                + (TextHorizontalMargin * 2.0),
            height: 0.0
        );
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext: drawingContext);
        if (Lines == null || Lines.Count == 0)
        {
            return;
        }

        var lineNumberWidth = Math.Round(a: _lineFt.Width + (TextHorizontalMargin * 2.0));
        var tf = CreateTypeface();
        var fontSize = (double)GetValue(dp: TextBlock.FontSizeProperty);
        var visualLines = TextView.VisualLinesValid
            ? TextView.VisualLines
            : Enumerable.Empty<VisualLine>();
        foreach (var v in visualLines)
        {
            var rcs = BackgroundGeometryBuilder
                .GetRectsFromVisualSegment(textView: TextView, line: v, startVC: 0, endVC: 1000)
                .ToArray();
            var linenum = v.FirstDocumentLine.LineNumber - 1;
            if (linenum >= Lines.Count)
            {
                continue;
            }

            var diffLine = Lines[index: linenum];
            FormattedText ft;
            if (diffLine.Style != DiffContext.Context)
            {
                var brush = default(Brush);
                switch (diffLine.Style)
                {
                    case DiffContext.Added:
                    {
                        brush = AddedBackground;
                        break;
                    }

                    case DiffContext.Deleted:
                    {
                        brush = DeletedBackground;
                        break;
                    }

                    case DiffContext.Blank:
                    {
                        brush = BlankBackground;
                        break;
                    }
                }
                foreach (var rc in rcs)
                {
                    drawingContext.DrawRectangle(
                        brush: brush,
                        pen: BorderlessPen,
                        rectangle: new Rect(x: 0, y: rc.Top, width: ActualWidth, height: rc.Height)
                    );
                }
            }
            if (diffLine.Text != "")
            {
                ft = new FormattedText(
                    textToFormat: diffLine.LineNumber,
                    culture: CultureInfo.CurrentCulture,
                    flowDirection: FlowDirection.LeftToRight,
                    typeface: tf,
                    emSize: fontSize,
                    foreground: ForegroundBrush,
                    pixelsPerDip: VisualTreeHelper.GetDpi(visual: this).PixelsPerDip
                );
                var left = TextHorizontalMargin;
                drawingContext.DrawText(
                    formattedText: ft,
                    origin: new Point(x: left, y: rcs[0].Top)
                );
            }
            if (diffLine.PrefixForStyle != "")
            {
                var prefix = diffLine.PrefixForStyle;
                ft = new FormattedText(
                    textToFormat: prefix,
                    culture: CultureInfo.CurrentCulture,
                    flowDirection: FlowDirection.LeftToRight,
                    typeface: tf,
                    emSize: fontSize,
                    foreground: (Brush)TextView.GetValue(dp: Control.ForegroundProperty),
                    pixelsPerDip: VisualTreeHelper.GetDpi(visual: this).PixelsPerDip
                );
                drawingContext.DrawText(
                    formattedText: ft,
                    origin: new Point(x: lineNumberWidth + TextHorizontalMargin, y: rcs[0].Top)
                );
            }
        }
    }

    public List<DiffLineViewModel> Lines { get; set; }

    Typeface CreateTypeface()
    {
        var fe = TextView;
        return new Typeface(
            fontFamily: (FontFamily)fe.GetValue(dp: TextBlock.FontFamilyProperty),
            style: (FontStyle)fe.GetValue(dp: TextBlock.FontStyleProperty),
            weight: (FontWeight)fe.GetValue(dp: TextBlock.FontWeightProperty),
            stretch: (FontStretch)fe.GetValue(dp: TextBlock.FontStretchProperty)
        );
    }
}
