using CommunityToolkit.Mvvm.ComponentModel;

namespace AskBofah.Models
{
    public partial class ChatMessage : ObservableObject
    {
        [ObservableProperty]
        private string text = string.Empty;

        [ObservableProperty]
        private bool isUser;

        [ObservableProperty]
        private bool isStreaming;

        [ObservableProperty]
        private DateTime timestamp = DateTime.Now;

        public string TimestampDisplay =>
            Timestamp.ToString("HH:mm");
    }
}