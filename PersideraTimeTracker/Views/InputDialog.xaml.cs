using System.Windows;
using System.Windows.Input;

namespace PersideraTimeTracker.Views
{
    /// <summary>
    /// A small modal dialog that prompts the user for a single line of text.
    /// Used for creating a new project by name.
    /// </summary>
    public partial class InputDialog : Window
    {
        public string ResponseText { get; private set; } = "";

        public InputDialog(string title = "New Project", string prompt = "Project name", string initial = "")
        {
            InitializeComponent();
            Title = title;
            HeaderText.Text = title.ToUpperInvariant();
            PromptText.Text = prompt;
            Input.Text = initial;
            Loaded += (_, _) => { Input.Focus(); Input.SelectAll(); };
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = Input.Text?.Trim() ?? "";
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
