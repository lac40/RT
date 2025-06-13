namespace RepTrackWeb.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; }
        public bool ShowDetails { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool HasMessage => !string.IsNullOrEmpty(Message);
    }
}
