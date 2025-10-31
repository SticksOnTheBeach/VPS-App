using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VPSControl.Services;

namespace VPSControl.Pages
{
    public partial class LoginPage : Page
    {
        private readonly System.Action<string> _navigate;

        public LoginPage(System.Action<string> navigate)
        {
            InitializeComponent();
            _navigate = navigate;
        }

        // --- Placeholder simulation (grisé) ---
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

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var pass = PasswordBox.Password;

            if (email == "Email" || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                ErrorText.Text = "Please enter your email and password.";
                return;
            }

            bool ok = MainWindow.Database.CheckUser(email, pass);
            if (ok)
            {
                ErrorText.Text = "";
                _navigate("Dashboard");
            }
            else
            {
                ErrorText.Text = "Invalid email or password.";
            }
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            _navigate("Signup");
        }
    }
}