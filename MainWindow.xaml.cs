using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RussianLanguageTextbook
{
    public partial class MainWindow : Window
    {
        private User currentUser;
        private bool isAdmin;
        private List<Section> sections;
        private List<User> allUsers;
        private const string SECTIONS_FILE = "sections.xml";
        private const string USERS_FILE = "users.xml";

        // Для теста
        private Section currentTestSection;
        private int currentQuestionIndex = 0;
        private int score = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            UpdateStatus("Загрузка...");
        }

        private void UpdateStatus(string status)
        {
            statusText.Text = status;
        }

        private void LoadData()
        {
            // Загрузка разделов
            if (File.Exists(SECTIONS_FILE))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Section>));
                using (FileStream fs = new FileStream(SECTIONS_FILE, FileMode.Open))
                {
                    sections = (List<Section>)serializer.Deserialize(fs);
                }
            }
            else
            {
                sections = new List<Section>();
                // Добавляем начальные разделы с вопросами
                var grammar = new Section
                {
                    Title = "Грамматика",
                    Content = "Грамматика - это раздел лингвистики, изучающий строй языка."
                };
                grammar.Questions.Add(new SimpleQuestion
                {
                    Text = "Что изучает морфология?",
                    Options = new List<string> { "Звуки речи", "Части речи", "Знаки препинания", "Правила написания" },
                    CorrectOption = 1
                });
                sections.Add(grammar);

                var orthography = new Section
                {
                    Title = "Орфография",
                    Content = "Орфография - правила написания слов."
                };
                orthography.Questions.Add(new SimpleQuestion
                {
                    Text = "Что изучает орфография?",
                    Options = new List<string> { "Правила написания", "Знаки препинания", "Звуки речи", "Части речи" },
                    CorrectOption = 0
                });
                sections.Add(orthography);

                var punctuation = new Section
                {
                    Title = "Пунктуация",
                    Content = "Пунктуация - правила расстановки знаков препинания."
                };
                punctuation.Questions.Add(new SimpleQuestion
                {
                    Text = "Какой знак ставится в конце предложения?",
                    Options = new List<string> { "Запятая", "Точка", "Тире", "Двоеточие" },
                    CorrectOption = 1
                });
                sections.Add(punctuation);

                SaveSections();
            }

            // Загрузка пользователей
            if (File.Exists(USERS_FILE))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                using (FileStream fs = new FileStream(USERS_FILE, FileMode.Open))
                {
                    allUsers = (List<User>)serializer.Deserialize(fs);
                }
            }
            else
            {
                allUsers = new List<User>();
            }

            sectionsListBox.ItemsSource = sections;
        }

        private void SaveSections()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Section>));
            using (FileStream fs = new FileStream(SECTIONS_FILE, FileMode.Create))
            {
                serializer.Serialize(fs, sections);
            }
        }

        private void SaveUsers()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
            using (FileStream fs = new FileStream(USERS_FILE, FileMode.Create))
            {
                serializer.Serialize(fs, allUsers);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLoginDialog();
        }

        private void ShowLoginDialog()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            loginWindow.ShowDialog();

            if (loginWindow.DialogResult == true)
            {
                currentUser = loginWindow.CurrentUser;
                isAdmin = loginWindow.IsAdmin;

                // Обновляем очки из общего списка
                var savedUser = allUsers.FirstOrDefault(u => u.Username == currentUser.Username);
                if (savedUser != null)
                {
                    currentUser.Score = savedUser.Score;
                }

                userInfoText.Text = $"Пользователь: {currentUser.Username}";
                userScoreText.Text = $"Очки: {currentUser.Score}";

                if (isAdmin)
                {
                    Title = $"Учебник - Админ: {currentUser.Username}";
                    adminPanel.Visibility = Visibility.Visible;
                    UpdateStatus("Режим администратора");
                }
                else
                {
                    Title = $"Учебник - {currentUser.Username}";
                    adminPanel.Visibility = Visibility.Collapsed;
                    UpdateStatus("Выберите раздел");
                }

                ShowWelcomeScreen();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void ShowWelcomeScreen()
        {
            contentTitle.Text = "Добро пожаловать!";
            contentText.Text = $"Здравствуйте, {currentUser.Username}!\nВыберите раздел для изучения.";
            HideAllPanels();
        }

        private void HideAllPanels()
        {
            testPanel.Visibility = Visibility.Collapsed;
            addSectionPanel.Visibility = Visibility.Collapsed;
            addQuestionPanel.Visibility = Visibility.Collapsed;
            leaderboardPanel.Visibility = Visibility.Collapsed;
        }

        private void SectionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sectionsListBox.SelectedItem is Section selected)
            {
                contentTitle.Text = selected.Title;
                contentText.Text = selected.Content;
                HideAllPanels();
                UpdateStatus($"Раздел: {selected.Title}");
            }
        }

        // Админ методы для разделов
        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            if (!isAdmin) return;
            HideAllPanels();
            addSectionPanel.Visibility = Visibility.Visible;
            newSectionTitle.Text = "";
            newSectionContent.Text = "";
            contentTitle.Text = "Добавление раздела";
            contentText.Text = "";
            UpdateStatus("Добавление нового раздела");
        }

        private void SaveNewSection_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newSectionTitle.Text))
            {
                MessageBox.Show("Введите название");
                return;
            }

            sections.Add(new Section
            {
                Title = newSectionTitle.Text,
                Content = newSectionContent.Text,
                Questions = new List<SimpleQuestion>()
            });

            SaveSections();
            sectionsListBox.ItemsSource = null;
            sectionsListBox.ItemsSource = sections;
            addSectionPanel.Visibility = Visibility.Collapsed;
            MessageBox.Show("Раздел добавлен");
            UpdateStatus("Раздел добавлен");
        }

        private void CancelAddSection_Click(object sender, RoutedEventArgs e)
        {
            addSectionPanel.Visibility = Visibility.Collapsed;
            UpdateStatus("Добавление отменено");
        }

        private void DeleteSection_Click(object sender, RoutedEventArgs e)
        {
            if (!isAdmin) return;

            if (sectionsListBox.SelectedItem is Section selected)
            {
                sections.Remove(selected);
                SaveSections();
                sectionsListBox.ItemsSource = null;
                sectionsListBox.ItemsSource = sections;
                MessageBox.Show("Раздел удален");
                UpdateStatus("Раздел удален");
            }
            else
            {
                MessageBox.Show("Выберите раздел");
            }
        }

        // Админ методы для вопросов
        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!isAdmin) return;

            if (sectionsListBox.SelectedItem is Section selected)
            {
                HideAllPanels();
                addQuestionPanel.Visibility = Visibility.Visible;
                currentSectionForQuestion.Text = selected.Title;

                // Очищаем поля
                newQuestionText.Text = "";
                newOption1.Text = "";
                newOption2.Text = "";
                newOption3.Text = "";
                newOption4.Text = "";

                contentTitle.Text = "Добавление вопроса";
                contentText.Text = $"Раздел: {selected.Title}";
                UpdateStatus($"Добавление вопроса в раздел {selected.Title}");
            }
            else
            {
                MessageBox.Show("Сначала выберите раздел");
            }
        }

        private void SaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (sectionsListBox.SelectedItem is Section selected)
            {
                // Проверка полей
                if (string.IsNullOrWhiteSpace(newQuestionText.Text) ||
                    string.IsNullOrWhiteSpace(newOption1.Text))
                {
                    MessageBox.Show("Заполните вопрос и первый вариант ответа");
                    return;
                }

                var question = new SimpleQuestion
                {
                    Text = newQuestionText.Text,
                    Options = new List<string>(),
                    CorrectOption = 0 // Первый вариант - правильный
                };

                // Добавляем варианты (только непустые)
                if (!string.IsNullOrWhiteSpace(newOption1.Text))
                    question.Options.Add(newOption1.Text);
                if (!string.IsNullOrWhiteSpace(newOption2.Text))
                    question.Options.Add(newOption2.Text);
                if (!string.IsNullOrWhiteSpace(newOption3.Text))
                    question.Options.Add(newOption3.Text);
                if (!string.IsNullOrWhiteSpace(newOption4.Text))
                    question.Options.Add(newOption4.Text);

                selected.Questions.Add(question);
                SaveSections();

                addQuestionPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show("Вопрос добавлен");
                UpdateStatus("Вопрос добавлен");
            }
        }

        private void CancelAddQuestion_Click(object sender, RoutedEventArgs e)
        {
            addQuestionPanel.Visibility = Visibility.Collapsed;
            UpdateStatus("Добавление вопроса отменено");
        }

        // Тест
        private void StartTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (sectionsListBox.SelectedItem is Section selected)
            {
                if (selected.Questions.Count == 0)
                {
                    MessageBox.Show("В этом разделе пока нет вопросов");
                    return;
                }

                currentTestSection = selected;
                currentQuestionIndex = 0;
                score = 0;
                ShowQuestion();
                startTestButton.Visibility = Visibility.Collapsed;
                testPanel.Visibility = Visibility.Visible;
                leaderboardPanel.Visibility = Visibility.Collapsed;
                addSectionPanel.Visibility = Visibility.Collapsed;
                addQuestionPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Выберите раздел");
            }
        }

        private void ShowQuestion()
        {
            if (currentQuestionIndex < currentTestSection.Questions.Count)
            {
                var q = currentTestSection.Questions[currentQuestionIndex];
                contentTitle.Text = $"Вопрос {currentQuestionIndex + 1} из {currentTestSection.Questions.Count}";
                contentText.Text = q.Text;

                optionsStackPanel.Children.Clear();

                for (int i = 0; i < q.Options.Count; i++)
                {
                    var rb = new RadioButton
                    {
                        Content = q.Options[i],
                        Tag = i,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    optionsStackPanel.Children.Add(rb);
                }

                UpdateStatus($"Вопрос {currentQuestionIndex + 1} из {currentTestSection.Questions.Count}");
            }
        }

        private void CheckAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            int selected = -1;
            foreach (RadioButton rb in optionsStackPanel.Children)
            {
                if (rb.IsChecked == true)
                {
                    selected = (int)rb.Tag;
                    break;
                }
            }

            if (selected == -1)
            {
                MessageBox.Show("Выберите ответ!");
                return;
            }

            var q = currentTestSection.Questions[currentQuestionIndex];
            if (selected == q.CorrectOption)
            {
                score++;
                MessageBox.Show("Правильно! +1 очко");
            }
            else
            {
                MessageBox.Show($"Неправильно. Правильный ответ: {q.Options[q.CorrectOption]}");
            }

            currentQuestionIndex++;

            if (currentQuestionIndex < currentTestSection.Questions.Count)
            {
                ShowQuestion();
            }
            else
            {
                // Тест окончен
                contentTitle.Text = "Результат";
                contentText.Text = $"Правильных ответов: {score} из {currentTestSection.Questions.Count}";
                testPanel.Visibility = Visibility.Collapsed;
                startTestButton.Visibility = Visibility.Visible;

                // Добавляем очки
                currentUser.Score += score;
                userScoreText.Text = $"Очки: {currentUser.Score}";

                // Сохраняем
                var userInList = allUsers.FirstOrDefault(u => u.Username == currentUser.Username);
                if (userInList != null)
                {
                    userInList.Score = currentUser.Score;
                }
                else
                {
                    allUsers.Add(currentUser);
                }
                SaveUsers();

                UpdateStatus($"Тест завершен. Результат: {score}/{currentTestSection.Questions.Count}");
            }
        }

        // Лидерборд
        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            leaderboardPanel.Visibility = Visibility.Visible;
            contentTitle.Text = "Таблица лидеров";
            contentText.Text = "";

            var leaders = allUsers
                .Where(u => !u.IsAdmin)
                .OrderByDescending(u => u.Score)
                .ToList();

            var items = new List<string>();
            for (int i = 0; i < leaders.Count; i++)
            {
                string medal = i == 0 ? "🥇 " : i == 1 ? "🥈 " : i == 2 ? "🥉 " : $"{i + 1}. ";
                items.Add($"{medal}{leaders[i].Username} - {leaders[i].Score} очков");
            }

            if (items.Count == 0)
                items.Add("Пока нет участников");

            leaderboardList.ItemsSource = items;
            UpdateStatus("Просмотр таблицы лидеров");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                currentUser = null;
                ShowLoginDialog();
            }
        }
    }
}
