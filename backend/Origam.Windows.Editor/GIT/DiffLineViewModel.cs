using System;

namespace Origam.Windows.Editor.GIT;

public class DiffLineViewModel
{
    public string Text { get; private set; }
    public DiffContext Style { get; private set; }
    public string LineNumber { get; private set; }
    public string PrefixForStyle { get; private set; }
    public int PositionInSection { get; private set; }

    public static DiffLineViewModel Empty() =>
        new DiffLineViewModel
        {
            Style = DiffContext.Blank,
            Text = " ",
            PrefixForStyle = "",
            LineNumber="  "
        };

    public static DiffLineViewModel Create(int positionInSection, string lineNumber, string text)
    {
            var viewModel = new DiffLineViewModel();
            viewModel.LineNumber = lineNumber;
            viewModel.PositionInSection = positionInSection;
            
            if (text.StartsWith("+"))
            {
                viewModel.Style = DiffContext.Added;
                viewModel.PrefixForStyle = "+";
                viewModel.Text = text.Substring(1);
            }
            else if (text.StartsWith("-"))
            {
                viewModel.Style = DiffContext.Deleted;
                viewModel.PrefixForStyle = "-";
                viewModel.Text = text.Substring(1);
            }
            else
            {
                viewModel.Style = DiffContext.Context;
                viewModel.PrefixForStyle = "";
                viewModel.Text = text.Length > 1 ? text.Substring(1) : text; 
            }
            return viewModel;
        }

    public override string ToString()
    {
            return String.Format("{0}{1}", PrefixForStyle, Text);
        }
}