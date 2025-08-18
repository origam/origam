using System.Collections.Generic;
using System.Linq;

namespace Origam.Windows.Editor.GIT;
public class DiffSectionViewModel
{
    public string DiffSectionHeader { get;  }
    public List<DiffLineViewModel> LeftDiff { get; }
    public List<DiffLineViewModel> RightDiff { get;  }
    public List<DiffLineViewModel> MixedDiff { get;  }
    public DiffSectionViewModel(string diffSectionHeader, List<DiffLineViewModel> leftDiff, List<DiffLineViewModel> rightDiff)
    {
        DiffSectionHeader = diffSectionHeader;
        LeftDiff = leftDiff;
        RightDiff = rightDiff;
        MixedDiff = MergeSideDiffs();
    }
    private List<DiffLineViewModel> MergeSideDiffs()
    {
        var allLines = LeftDiff.ToList();
        var adds = RightDiff
            .Where(x => x.Style == DiffContext.Added);
        allLines.AddRange(adds);
       return  allLines.OrderBy(x => x.PositionInSection).ToList();
    }
}
