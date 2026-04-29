using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;

namespace Origam.Windows.Editor.GIT;

public partial class SingleColumnDiff : UserControl
{
    private readonly SingleColumnDiffWPF singleColumnDiffWpf;

    public SingleColumnDiff()
    {
        InitializeComponent();
        singleColumnDiffWpf = new SingleColumnDiffWPF();
        elementHost1.Child = singleColumnDiffWpf;
    }

    private TextEditor Editor => singleColumnDiffWpf.TextEditor;

    public void Show(DiffModelInfo diffInfo)
    {
        var margin = new DiffInfoMargin { Lines = diffInfo.Lines };
        var backgroundRenderer = new DiffLineBackgroundRenderer { Lines = diffInfo.Lines };
        Editor.TextArea.LeftMargins.Add(item: margin);
        Editor.TextArea.TextView.BackgroundRenderers.Add(item: backgroundRenderer);
        Editor.Text = String.Join(
            separator: "\r\n",
            values: diffInfo.Lines.Select(selector: x => x.Text)
        );
    }
}

public class DiffModelInfo
{
    public List<DiffLineViewModel> Lines { get; }
    public int LinesReturned { get; }
    public int LinesTotal { get; }

    public DiffModelInfo(List<DiffLineViewModel> lines, int linesReturned, int linesTotal)
    {
        Lines = lines;
        LinesReturned = linesReturned;
        LinesTotal = linesTotal;
    }
}

public class DiffParser
{
    private readonly List<string> allLines;

    public DiffParser(string gitDiff)
    {
        allLines = gitDiff.Split(separator: '\n').Select(selector: x => x.TrimEnd()).ToList();
    }

    public DiffModelInfo ParseToLines(int maxLinesToReturn)
    {
        var lineNumbers = Enumerable.Range(start: 0, count: allLines.Count);
        return allLines
            .Zip(second: lineNumbers, resultSelector: (x, index) => new { Line = x, Index = index })
            .Where(predicate: x => x.Line.StartsWith(value: "diff --git a"))
            .Select(selector: x =>
                ToLineViewList(headerIndex: x.Index, maxLinesToReturn: maxLinesToReturn)
            )
            .First();
    }

    private DiffModelInfo ToLineViewList(int headerIndex, int maxLinesToReturn)
    {
        List<string> hunkElements = GetLinesFromHeaderIndex(headerIndex: headerIndex);
        List<DiffSectionViewModel> sections = ParseToSections(hunkElements: hunkElements);
        List<DiffSectionViewModel> filteredSections = ReduceByLineCount(
            sections: sections,
            maxLineCount: maxLinesToReturn
        );
        return new DiffModelInfo(
            lines: ToSingleLineModelList(sections: filteredSections),
            linesReturned: CountMixedDiffLines(sections: filteredSections),
            linesTotal: CountMixedDiffLines(sections: sections)
        );
    }

    private int CountMixedDiffLines(List<DiffSectionViewModel> sections)
    {
        return sections.Select(selector: sec => sec.MixedDiff.Count).Sum();
    }

    private List<DiffSectionViewModel> ReduceByLineCount(
        List<DiffSectionViewModel> sections,
        int maxLineCount
    )
    {
        List<DiffSectionViewModel> filtered = new List<DiffSectionViewModel>();
        int lineCount = 0;
        foreach (DiffSectionViewModel section in sections)
        {
            if (lineCount + section.MixedDiff.Count > maxLineCount)
            {
                return filtered;
            }

            filtered.Add(item: section);
            lineCount += section.MixedDiff.Count;
        }
        return filtered;
    }

    private static List<DiffLineViewModel> ToSingleLineModelList(
        List<DiffSectionViewModel> sections
    )
    {
        return sections
            .Select(selector: sec => sec.MixedDiff)
            .Aggregate(
                seed: new List<DiffLineViewModel>(),
                func: (allDiffLines, sectionLines) =>
                {
                    allDiffLines.Add(item: DiffLineViewModel.Empty());
                    allDiffLines.Add(item: DiffLineViewModel.Empty());
                    allDiffLines.AddRange(collection: sectionLines);
                    return allDiffLines;
                }
            );
    }

    private List<string> GetLinesFromHeaderIndex(int headerIndex)
    {
        var hunkElements = allLines
            .Skip(count: headerIndex + 1)
            .TakeWhile(predicate: x => !x.StartsWith(value: "diff --git a"))
            .ToList();
        return hunkElements;
    }

    private List<DiffSectionViewModel> ParseToSections(IEnumerable<string> hunkElements)
    {
        List<string> diffContents = hunkElements.Skip(count: 3).ToList();
        List<string> sectionHeaders = diffContents
            .Where(predicate: x => x.StartsWith(value: "@@ "))
            .ToList();
        var lineNumberRegEx = new Regex(
            pattern: @"\-(?<leftStart>\d{1,})(\,(?<leftCount>\d{1,}))*\s\+(?<rightStart>\d{1,})(\,(?<rightCount>\d{1,}))*",
            options: RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        var sections = new List<DiffSectionViewModel>();
        foreach (var header in sectionHeaders)
        {
            Match lineNumberResult = lineNumberRegEx.Match(input: header);
            var startIndex = diffContents.IndexOf(item: header);
            IEnumerable<int> diffPositions = Enumerable.Range(start: 0, count: diffContents.Count);
            var innerDiffContents = diffContents
                .Zip(
                    second: diffPositions,
                    resultSelector: (line, pos) => new { PositionInDiff = pos, Line = line }
                )
                .Skip(count: startIndex + 1)
                .ToList();
            var leftStart = int.Parse(s: lineNumberResult.Groups[groupname: "leftStart"].Value);
            var leftDiffSize = string.IsNullOrEmpty(
                value: lineNumberResult.Groups[groupname: "leftCount"].Value
            )
                ? leftStart
                : int.Parse(s: lineNumberResult.Groups[groupname: "leftCount"].Value);
            var rightStart = int.Parse(s: lineNumberResult.Groups[groupname: "rightStart"].Value);
            var rightDiffSize = string.IsNullOrEmpty(
                value: lineNumberResult.Groups[groupname: "rightCount"].Value
            )
                ? rightStart
                : int.Parse(s: lineNumberResult.Groups[groupname: "rightCount"].Value);
            var leftLineNumbers = Enumerable
                .Range(start: leftStart, count: leftDiffSize)
                .Select(selector: x => x.ToString(provider: CultureInfo.InvariantCulture));
            // left section - all context + deletes
            List<DiffLineViewModel> leftDiff = innerDiffContents
                .Where(predicate: x => !x.Line.StartsWith(value: "+"))
                .Zip(
                    second: leftLineNumbers,
                    resultSelector: (x, line) =>
                        new
                        {
                            x.Line,
                            x.PositionInDiff,
                            LineNumber = line,
                        }
                )
                .Select(selector: x =>
                    DiffLineViewModel.Create(
                        positionInSection: x.PositionInDiff,
                        lineNumber: x.LineNumber,
                        text: x.Line
                    )
                )
                .ToList();
            // right section - all context + adds
            var rightLineNumbers = Enumerable
                .Range(start: rightStart, count: rightDiffSize)
                .Select(selector: x => x.ToString(provider: CultureInfo.InvariantCulture));
            List<DiffLineViewModel> rightDiff = innerDiffContents
                .Where(predicate: x => !x.Line.StartsWith(value: "-"))
                .Zip(
                    second: rightLineNumbers,
                    resultSelector: (x, line) =>
                        new
                        {
                            x.Line,
                            x.PositionInDiff,
                            LineNumber = line,
                        }
                )
                .Select(selector: x =>
                    DiffLineViewModel.Create(
                        positionInSection: x.PositionInDiff,
                        lineNumber: x.LineNumber,
                        text: x.Line
                    )
                )
                .ToList();
            var section = new DiffSectionViewModel(
                diffSectionHeader: header,
                leftDiff: leftDiff,
                rightDiff: rightDiff
            );

            sections.Add(item: section);
        }
        return sections;
    }
}
