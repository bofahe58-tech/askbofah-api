namespace AskBofah.Views
{
    public partial class WorkspaceView : ContentView
    {
        public WorkspaceView(
            string title,
            string description)
        {
            InitializeComponent();

            TitleLabel.Text = title;
            DescriptionLabel.Text = description;
        }
    }
}