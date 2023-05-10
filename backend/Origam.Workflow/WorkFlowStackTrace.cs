using System.Text;

namespace Origam.Workflow;

public class WorkFlowStackTrace
{
    private readonly StringBuilder messages = new("Workflow stack trace\n");
    
    public void RecordStepStart(string workflowName, string stepName)
    {
        messages.Append($"\tStep: '{workflowName}/{stepName}'\n");
    }

    public override string ToString()
    {
        return messages.ToString();
    }
}