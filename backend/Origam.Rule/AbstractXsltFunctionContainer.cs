namespace Origam.Rule;

public abstract class AbstractXsltFunctionContainer: IXsltFunctionContainer
{
    public string XslNameSpacePrefix { get; set; }
    public string XslNameSpaceUri { get; set; }
}