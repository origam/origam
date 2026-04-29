using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;
using Origam.UI;

namespace Origam.Windows.Editor.GIT;

public class DiffLineBackgroundRenderer : IBackgroundRenderer
{
    static readonly Brush AddedBackground;
    static readonly Brush DeletedBackground;
    static readonly Brush BlankBackground;
    static readonly Pen BorderlessPen;

    static DiffLineBackgroundRenderer()
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
    }

    public void Draw(TextView textView, System.Windows.Media.DrawingContext drawingContext)
    {
        if (Lines == null)
        {
            return;
        }

        foreach (var v in textView.VisualLines)
        {
            var linenum = v.FirstDocumentLine.LineNumber - 1;
            if (linenum >= Lines.Count)
            {
                continue;
            }

            var diffLine = Lines[index: linenum];
            if (diffLine.Style == DiffContext.Context)
            {
                continue;
            }

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
            foreach (
                var rc in BackgroundGeometryBuilder.GetRectsFromVisualSegment(
                    textView: textView,
                    line: v,
                    startVC: 0,
                    endVC: 1000
                )
            )
            {
                drawingContext.DrawRectangle(
                    brush: brush,
                    pen: BorderlessPen,
                    rectangle: new Rect(
                        x: 0,
                        y: rc.Top,
                        width: textView.ActualWidth,
                        height: rc.Height
                    )
                );
            }
        }
    }

    public KnownLayer Layer
    {
        get { return KnownLayer.Background; }
    }
    public List<DiffLineViewModel> Lines { get; set; }
}
