namespace Origam.Server.Configuration
{
    public class HtmlClientConfig
    {
        public bool ShowToolTipsForMemoFieldsOnly { get; set; }
        public int RowStatesDebouncingDelayMilliSeconds { get; set; } = 200;
    }
}