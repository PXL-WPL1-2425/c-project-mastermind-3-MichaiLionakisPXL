using Microsoft.VisualBasic;
using System.ComponentModel.Design.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        struct player
        {
            public string name;
            public int score;
            public int highscore;
        }
        List<player> playerList = new List<player>();
        string combinedString;
        Random rnd = new Random();
        string[] colors = { "rood", "geel", "oranje", "blauw", "wit", "groen" };
        string[] tittleCheck = new string[4];
        private DispatcherTimer timer = new DispatcherTimer();
        bool[] isCorrect = new bool[4];
        int timerTick = 0;
        int attempt = 0;
        int attemptAmount = 10;
        int penaltyPoints = 0;
        bool allCorrect = true;
        bool nameInput = false;
        

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            GameMesage();
            GenerateCode();
            StartGame();
            this.KeyDown += toggleDebug;
            UpdateCurrentPlayerDisplay();
        }
        private void codeCheck_Click(object sender, RoutedEventArgs e)
        {
            if (attempt >= attemptAmount) { GameMesage(); return; }
            addAttempt();
            StopCountDown();
            StartCountDown();
            ComboBox[] comboBoxes = { comboBox1, comboBox2, comboBox3, comboBox4 };
            Label[] colorLabels = { colorLabel1, colorLabel2, colorLabel3, colorLabel4 };
            StackPanel newStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            bool[] isCorrect = new bool[4];
            for (int i = 0; i < comboBoxes.Length; i++)
            {
                ComboBox comboBox = comboBoxes[i];
                Label colorLabel = colorLabels[i];
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    penaltyLabel.Content = $"Strafpunten: {penaltyPoints}";
                    string selectedColor = selectedItem.Content.ToString().Trim();
                    if (selectedColor == tittleCheck[i])
                    {
                        colorLabel.BorderBrush = new SolidColorBrush(Colors.Wheat);
                        colorLabel.BorderThickness = new Thickness(3);
                        isCorrect[i] = true;
                    }
                    else if (tittleCheck.Contains(selectedColor))
                    {
                        penaltyPoints += 1; colorLabel.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                        colorLabel.BorderThickness = new Thickness(3);
                    }
                    else
                    {
                        penaltyPoints += 2;
                        colorLabel.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                        colorLabel.BorderThickness = new Thickness(3);
                        isCorrect[i] = false;
                    }

                    Label newLabel = new Label
                    {
                        Background = colorLabel.Background,
                        BorderBrush = colorLabel.BorderBrush,
                        BorderThickness = colorLabel.BorderThickness,
                        Width = 94,
                        Height = 10,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,

                    };
                    newStackPanel.Children.Add(newLabel);
                }
                gameWon(isCorrect);
            }
            historyList.Items.Add(newStackPanel);
        }
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox[] comboBoxes = { comboBox1, comboBox2, comboBox3, comboBox4 };
            Label[] colorLabels = { colorLabel1, colorLabel2, colorLabel3, colorLabel4 };
            for (int i = 0; i < comboBoxes.Length; i++)
            {
                ComboBox comboBox = comboBoxes[i];
                Label colorLabel = colorLabels[i];
                ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
                if (item != null)
                {
                    string selectedColor = item.Content.ToString().Trim();
                    switch (selectedColor)
                    {
                        case "wit":
                            colorLabel.Background = new SolidColorBrush(Colors.White);
                            break;
                        case "groen":
                            colorLabel.Background = new SolidColorBrush(Colors.Green);
                            break;
                        case "blauw":
                            colorLabel.Background = new SolidColorBrush(Colors.Blue);
                            break;
                        case "rood":
                            colorLabel.Background = new SolidColorBrush(Colors.Red);
                            break;
                        case "geel":
                            colorLabel.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "oranje":
                            colorLabel.Background = new SolidColorBrush(Colors.Orange);
                            break;
                        default:
                            colorLabel.Background = null;
                            break;
                    }
                }
            }
        }
        private void addAttempt()
        {
            if (attempt < attemptAmount)
            {
                attempt++;
                guessLabel.Content = $"Poging: " + $"{attempt.ToString()}/10";
            }
            else if (attempt > (attemptAmount - 1))
            {
                GameMesage();
            }
        }
        private void toggleDebug(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.F12)
            {
                codeBlock.Visibility = Visibility.Visible;
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            timerTick++;
            timerBlock.Text = timerTick.ToString();
            if (timerTick == 10)
            {
                attempt++;
                StopCountDown();
                StartCountDown();
            }
        }
        private void StartCountDown()
        {
            timerTick = 0;
            timer.Start();
        }
        private void StopCountDown()
        {
            timer.Stop();
            timerTick = 0;
            guessLabel.Content = $"Poging: " + $"{attempt.ToString()}/10";
        }
        private void ResetGame()
        {
            attempt = 0;
            timerTick = 0;
            penaltyPoints = 0;
            if (attempt == 0) { historyList.Items.Clear(); }
            GenerateCode();
            guessLabel.Content = $"Poging: " + $"{attempt.ToString()}/10";

        }
        private void GameMesage()
        {
            if (attempt > 9)
            {
                string currentPlayer = playerList[attempt % playerList.Count].name;
                string nextPlayer = playerList[(attempt + 1) % playerList.Count].name;
                MessageBoxResult result = MessageBox.Show($"{currentPlayer}, je hebt gefaald. De juiste code was {combinedString}. Volgende speler is {nextPlayer}.", "FAILED");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Weet je zeker dat je de applicatie wilt afsluiten?je zit op poging {attempt}/10",
                                                      "Bevestigen", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
        private void GenerateCode()
        {
            Array.Clear(tittleCheck, 0, tittleCheck.Length);
            for (int i = 0; i < 4; i++) { tittleCheck[i] += colors[rnd.Next(0, 6)]; }
            combinedString = string.Join(",", tittleCheck);
            codeBlock.Text = combinedString;
        }
        private void StartGame()
        {
            string name = Interaction.InputBox("Geef hier jouw naam in:", "Naam");

            while (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Geef een naam in!!", "Foutieve invoer");
                name = Interaction.InputBox("Geef hier jouw naam in:", "Naam");
            }

            player newPlayer = new player
            {
                name = name
            };

            playerList.Add(newPlayer);

            bool addMorePlayers = true;
            while (addMorePlayers)
            {
                MessageBoxResult result = MessageBox.Show("Wil je nog een speler toevoegen?", "Nieuwe speler?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    name = Interaction.InputBox("Geef hier jouw naam in:", "Naam");
                    while (string.IsNullOrEmpty(name))
                    {
                        MessageBox.Show("Geef een naam in!!", "Foutieve invoer");
                        name = Interaction.InputBox("Geef hier jouw naam in:", "Naam");
                    }
                    newPlayer = new player { name = name };
                    playerList.Add(newPlayer);
                }
                else
                {
                    addMorePlayers = false;
                }
            }
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
            StartGame();
        }

        private void AttemptAmount_Click(object sender, RoutedEventArgs e)
        {
            string attemptAmountStr = Interaction.InputBox("geef hier de attempts in: ", "Attempt amount change");
            

            if (int.TryParse(attemptAmountStr, out attemptAmount))
            {
                if (attemptAmount >= 3 && attemptAmount <= 20)
                {
                    guessLabel.Content = $"Poging: {attempt.ToString()}/10";
                }
                else
                {
                    MessageBox.Show("Aantal pogingen moet tussen 3 en 20 zijn.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Ongeldige invoer. Vul een getal in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePlayerScore(string playerName, int penaltyPoints)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].name == playerName)
                {
                    player updatedPlayer = playerList[i];
                    updatedPlayer.score += (100 - penaltyPoints);

                    if (updatedPlayer.score > updatedPlayer.highscore)
                    {
                        updatedPlayer.highscore = updatedPlayer.score;
                    }

                    playerList[i] = updatedPlayer;
                    return;
                }
            }

            MessageBox.Show($"Player '{playerName}' not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void gameWon(bool[] isCorrect)
        {
            allCorrect = isCorrect.All(value => value);

            if (allCorrect)
            {
                UpdateHighScoreList();
                string playerName = Interaction.InputBox("Geef jouw naam in voor de score:", "Speler Naam");

                if (!string.IsNullOrEmpty(playerName))
                {
                    UpdatePlayerScore(playerName, penaltyPoints);
                }

                MessageBoxResult result = MessageBox.Show($"Code is gekraakt in {attempt} pogingen.");
            }
        }
        private void UpdateHighScoreList()
        {
            HighScoreList.Items.Clear();

            foreach (var player in playerList)
            {
                string highScoreEntry = $"{player.name} - {attempt} pogingen - {player.highscore}";
                HighScoreList.Items.Add(highScoreEntry);
            }
        }
        private void UpdateCurrentPlayerDisplay()
        {
            string currentPlayerName = playerList[attempt % playerList.Count].name;
            currentPlayerLabel.Content = $"Huidige speler: {currentPlayerName}";
        }

        private void OfferHint()
        {
            MessageBoxResult result = MessageBox.Show("Wil je een hint kopen? Een juiste kleur kost 15 strafpunten, een juiste kleur op de juiste plaats kost 25 strafpunten.", "Hint kopen", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                string hintChoice = Interaction.InputBox("Kies je hint:\n1 - Juiste kleur\n2 - Juiste kleur op juiste plaats", "Kies Hint");

                if (hintChoice == "1")
                {
                    penaltyPoints += 15;
                    MessageBox.Show("Je hebt een hint gekocht: Juiste kleur.");
                }
                else if (hintChoice == "2")
                {
                    penaltyPoints += 25;
                    MessageBox.Show("Je hebt een hint gekocht: Juiste kleur op juiste plaats.");
                }
            }
        }


    }
}
