using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RussianLanguageTextbook
{
    public partial class LoginWindow : Window
    {
        private const string USERS_FILE = "users.xml";
        private List<User> users;

        public User CurrentUser { get; private set; }
        public bool IsAdmin { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (File.Exists(USERS_FILE))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                using (FileStream fs = new FileStream(USERS_FILE, FileMode.Open))
                {
                    users = (List<User>)serializer.Deserialize(fs);
                }
            }
            else
            {
                users = new List<User>();
                users.Add(new User { Username = "admin", Password = "admin123", IsAdmin = true });
                SaveUsers();
            }
        }

        private void SaveUsers()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
            using (FileStream fs = new FileStream(USERS_FILE, FileMode.Create))
            {
                serializer.Serialize(fs, users);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = loginUsernameTextBox.Text.Trim();
            string password = loginPasswordBox.Password;

            var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                CurrentUser = user;
                IsAdmin = user.IsAdmin;
                DialogResult = true;
                Close();
            }
            else
            {
                loginMessageText.Text = "Неверный логин или пароль";
                loginMessageText.Visibility = Visibility.Visible;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = regUsernameTextBox.Text.Trim();
            string password = regPasswordBox.Password;
            string confirmPassword = regConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                registerMessageText.Text = "Заполните все поля";
                registerMessageText.Visibility = Visibility.Visible;
                return;
            }

            if (password != confirmPassword)
            {
                registerMessageText.Text = "Пароли не совпадают";
                registerMessageText.Visibility = Visibility.Visible;
                return;
            }

            if (users.Any(u => u.Username == username))
            {
                registerMessageText.Text = "Пользователь уже существует";
                registerMessageText.Visibility = Visibility.Visible;
                return;
            }

            var newUser = new User
            {
                Username = username,
                Password = password,
                IsAdmin = false,
                Score = 0
            };

            users.Add(newUser);
            SaveUsers();

            MessageBox.Show("Регистрация успешна!");
            mainTabControl.SelectedIndex = 0;
        }
    }
}
