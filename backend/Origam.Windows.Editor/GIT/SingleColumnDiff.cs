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
            var margin = new DiffInfoMargin {Lines = diffInfo.Lines};
            var backgroundRenderer =
                new DiffLineBackgroundRenderer {Lines = diffInfo.Lines};

            Editor.TextArea.LeftMargins.Add(margin);
            Editor.TextArea.TextView.BackgroundRenderers.Add(
                backgroundRenderer);
            Editor.Text = String.Join("\r\n",
                diffInfo.Lines.Select(x => x.Text));
        }
}

public class DiffModelInfo
{
    public List<DiffLineViewModel> Lines { get;  }
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
            allLines = gitDiff.Split('\n')
                .Select(x => x.TrimEnd())
                .ToList();
        }

    public DiffModelInfo ParseToLines(int maxLinesToReturn){
            var lineNumbers = Enumerable.Range(0, allLines.Count);

            return allLines.Zip(lineNumbers,(x, index) => new {Line = x, Index = index})
                .Where(x => x.Line.StartsWith("diff --git a"))
                .Select(x=> ToLineViewList(x.Index, maxLinesToReturn))
                .First(); 
        }

    private DiffModelInfo ToLineViewList(int headerIndex, int maxLinesToReturn)
    {
            List<string> hunkElements = GetLinesFromHeaderIndex(headerIndex);
            List<DiffSectionViewModel> sections =
                ParseToSections(hunkElements);

            List<DiffSectionViewModel> filteredSections =
                ReduceByLineCount(sections, maxLinesToReturn);

            return new DiffModelInfo(
                lines:  ToSingleLineModelList(filteredSections), 
                linesReturned: CountMixedDiffLines(filteredSections),
                linesTotal: CountMixedDiffLines(sections));
        }

    private int CountMixedDiffLines(List<DiffSectionViewModel> sections)
    {
            return sections
                .Select(sec => sec.MixedDiff.Count)
                .Sum();
        }

    private List<DiffSectionViewModel> ReduceByLineCount(
        List<DiffSectionViewModel> sections, int maxLineCount)
    {
            List<DiffSectionViewModel> filtered = new List<DiffSectionViewModel>();
            int lineCount = 0;
            foreach (DiffSectionViewModel section in sections)
            {
                if (lineCount + section.MixedDiff.Count > maxLineCount)
                    return filtered;
                filtered.Add(section);
                lineCount += section.MixedDiff.Count;
            }
            return filtered;
        }

    private static List<DiffLineViewModel> ToSingleLineModelList(List<DiffSectionViewModel> sections)
    {
            return sections.Select(sec => sec.MixedDiff)
                .Aggregate(new List<DiffLineViewModel>(),
                    (allDiffLines, sectionLines) =>
                    {
                        allDiffLines.Add(DiffLineViewModel.Empty());
                        allDiffLines.Add(DiffLineViewModel.Empty());
                        allDiffLines.AddRange(sectionLines);
                        return allDiffLines;
                    });
        }

    private List<string> GetLinesFromHeaderIndex(int headerIndex)
    {
            var hunkElements = allLines
                .Skip(headerIndex + 1)
                .TakeWhile(x => !x.StartsWith("diff --git a"))
                .ToList();
            return hunkElements;
        }

    private List<DiffSectionViewModel> ParseToSections(IEnumerable<string> hunkElements)
    {
            List<string> diffContents = hunkElements.Skip(3).ToList();
            List<string> sectionHeaders = diffContents
                .Where(x => x.StartsWith("@@ "))
                .ToList();  

            var lineNumberRegEx = new Regex(@"\-(?<leftStart>\d{1,})(\,(?<leftCount>\d{1,}))*\s\+(?<rightStart>\d{1,})(\,(?<rightCount>\d{1,}))*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var sections = new List<DiffSectionViewModel>();

            foreach (var header in sectionHeaders)
            {
                Match lineNumberResult = lineNumberRegEx.Match(header);
                var startIndex = diffContents.IndexOf(header);
                IEnumerable<int> diffPositions = Enumerable.Range(0, diffContents.Count);
                var innerDiffContents = diffContents
                    .Zip( diffPositions, (line, pos ) => new{PositionInDiff = pos, Line = line})
                    .Skip(startIndex + 1)
                    .ToList();

                var leftStart = int.Parse(lineNumberResult.Groups["leftStart"].Value);
                var leftDiffSize = string.IsNullOrEmpty(lineNumberResult.Groups["leftCount"].Value)? leftStart :
                    int.Parse(lineNumberResult.Groups["leftCount"].Value);
                var rightStart = int.Parse(lineNumberResult.Groups["rightStart"].Value);
                var rightDiffSize = string.IsNullOrEmpty(lineNumberResult.Groups["rightCount"].Value)? rightStart:
                    int.Parse(lineNumberResult.Groups["rightCount"].Value);

                var leftLineNumbers = Enumerable.Range(leftStart, leftDiffSize)
                    .Select(x => x.ToString(CultureInfo.InvariantCulture));

                // left section - all context + deletes
                List<DiffLineViewModel> leftDiff = innerDiffContents
                    .Where(x => !x.Line.StartsWith("+"))
                    .Zip(leftLineNumbers, (x, line) => new { x.Line, x.PositionInDiff,  LineNumber = line })
                    .Select(x => DiffLineViewModel.Create(x.PositionInDiff, x.LineNumber, x.Line))
                    .ToList();

                // right section - all context + adds
                var rightLineNumbers = Enumerable.Range(rightStart, rightDiffSize)
                    .Select(x => x.ToString(CultureInfo.InvariantCulture));

                List<DiffLineViewModel> rightDiff = innerDiffContents
                    .Where(x => !x.Line.StartsWith("-"))
                    .Zip(rightLineNumbers, (x, line) => new { x.Line, x.PositionInDiff,  LineNumber = line })
                    .Select(x => DiffLineViewModel.Create(x.PositionInDiff, x.LineNumber, x.Line))
                    .ToList();

                var section = new DiffSectionViewModel(
                    diffSectionHeader: header, 
                    leftDiff: leftDiff, 
                    rightDiff: rightDiff);
                
                sections.Add(section);
            }
            return sections;
        }
}