namespace Origam.Server.Configuration
{
    public class HtmlClientConfig
    {
        public bool ShowToolTipsForMemoFieldsOnly { get; set; }
        public int RowStatesDebouncingDelayMilliseconds { get; set; }
        public int DropDownTypingDebouncingDelayMilliseconds { get; set; } = 300;
    }
}