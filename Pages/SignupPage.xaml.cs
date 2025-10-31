using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VPSControl.Services;

namespace VPSControl.Pages
{
    public partial class SignupPage : Page
    {
        private readonly System.Action<string> _navigate;

        public SignupPage(System.Action<string> navigate)
        {
            InitializeComponent();
            _navigate = navigate;
        }

        // --- Simule PlaceholderText (grisé) ---
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Foreground == Brushes.Gray)
            {
                tb.Text = "";
                tb.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "Email";
                tb.Foreground = Brushes.Gray;
            }
        }

        // --- Action lors du clic sur "Sign Up" ---
        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;
            var confirm = ConfirmBox.Password;

            if (email == "Email" || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                SetMessage("All fields are required.", Brushes.Red);
                return;
            }

            if (password != confirm)
            {
                SetMessage("Passwords do not match.", Brushes.Red);
                return;
            }

            bool added = MainWindow.Database.AddUser(email, password);
            if (added)
            {
                SetMessage("Account created! Please log in.", Brushes.Green);
            }
            else
            {
                SetMessage("This email is already registered.", Brushes.Red);
            }
        }

        private void SetMessage(string text, Brush color)
        {
            MessageText.Text = text;
            MessageText.Foreground = color;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            _navigate("Login");
        }
    }
}
