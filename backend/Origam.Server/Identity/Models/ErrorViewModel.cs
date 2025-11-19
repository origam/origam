namespace Origam.Server.Identity.Models;

public class ErrorViewModel
{
    public ErrorViewModel() { }

    public ErrorViewModel(string error)
    {
        Error = error;
    }

    public string Error { get; set; }
    public string ErrorDescription { get; set; }
}
