using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;

/* to do: */

namespace TicTacToeApp
{
    public partial class MainPage : ContentPage
    {
        private Button[,] buttons;
        private bool playerX = true;
        private int moves = 0;
        private System.Timers.Timer aiTimer;
        private bool isAgainstAI = false;
        private ObservableCollection<Player> players = new ObservableCollection<Player>();
        private Stopwatch turnStopwatch;
        private const string dataFileName = "playerdata.txt";
        private static string dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataFileName);

        public MainPage()
        {
            InitializeComponent();

            // Luodaan playerdata.txt solutionin kansioon, jos sellaista ei ole

            if (!File.Exists(dataFilePath))
            {
                // Tekee tyhjän tiedoston
                File.WriteAllText(dataFilePath, string.Empty);

                // Tekee tyhjään tiedostoon AI pelaajan
                if (new FileInfo(dataFilePath).Length == 0)
                {
                    CreateAiPlayer();
                }
            }
            else
            {
                // Ladataan pelaajien data tiedostosta
                LoadPlayerData();

                // varmistetaan vielä, että tiedostosta löytyy computer ai niminen pelaaja
                if (!PlayerExists("Computer AI"))
                {
                    CreateAiPlayer();
                }
            }

            // aloitetaan ajastin botin toimintaa varten
            InitializeAiTimer();

            // asetetaan pelimuodon valinnat näkyviin, myöhemmin näitä piilotellaan, niin pitää erikseen laittaa arvo: näkyy ensin.
            GameMode.IsVisible = true;

            PlayerPicker.ItemsSource = players;
            PlayerPickerO.ItemsSource = players;
        }

        private void OnAddPlayerClicked(object sender, EventArgs e)
        {
            string playerFirstName = FirstNameEntry.Text;
            string playerLastName = LastNameEntry.Text;
            DateTime dateOfBirth = MyDatePicker.Date;

            // Tarkistaa onko kentät täytetty
            if (!string.IsNullOrWhiteSpace(playerFirstName) && !string.IsNullOrWhiteSpace(playerLastName))
            {
                // Tarkistaa, ettei pelaaja ole asettanut nimeksi AI tai Computer
                if (!IsInvalidPlayerName(playerFirstName) && !IsInvalidPlayerName(playerLastName))
                {
                    Player newPlayer = new Player
                    {
                        FirstName = playerFirstName,
                        LastName = playerLastName,
                        DateOfBirth = dateOfBirth,
                        Wins = 0,
                        Losses = 0,
                        Draws = 0,
                        TimeElapsed = TimeSpan.Zero
                    };

                    players.Add(newPlayer);

                    ClearEntryFields();
                }
                else
                {
                    DisplayAlert("Error", "Syötä kelvollinen nimi, 'AI' sekä 'Computer' ovat botille varattuna.", "OK");
                }
            }
            else
            {
                DisplayAlert("Error", "Syötä etu-, sekä sukunimi", "OK");
            }
        }
        private bool IsInvalidPlayerName(string name) // tässä vielä yllä oleva nimen tarkistus
        {
            return name.Equals("AI", StringComparison.OrdinalIgnoreCase) || name.Equals("Computer", StringComparison.OrdinalIgnoreCase);
        }
        private void ClearEntryFields() // tyhjentää syötekentät pelaajan lisäyksen jälkeen
        {
            FirstNameEntry.Text = string.Empty;
            LastNameEntry.Text = string.Empty;
            MyDatePicker.Date = DateTime.Now;
        }

        private void OnPlayerPickerSelectedIndexChanged(object sender, EventArgs e) // kertoo DisplayPlayerInformationille, mikä pelaaja on valittu pickeristä.
        {
            if (PlayerPicker.SelectedIndex != -1)
            {
                Player selectedPlayer = players[PlayerPicker.SelectedIndex];
                DisplayPlayerInformation(selectedPlayer);
            }
        }

        private void OnPlayerPickerOSelectedIndexChanged(object sender, EventArgs e) // sama, mutta pelaajalle 2 (O) 
        {
            if (PlayerPickerO.SelectedIndex != -1)
            {
                Player selectedPlayer = players[PlayerPickerO.SelectedIndex];
                DisplayPlayer2Information(selectedPlayer);
            }
        }
        // laitetaan valittujen pelaajien infot näkyviin
        private void DisplayPlayerInformation(Player player)
        {
            PlayerInfoLabel.Text = GetPlayerInfoText(player);
        }
        private void DisplayPlayer2Information(Player player)
        {
            Player2InfoLabel.Text = GetPlayerInfoText(player);
        }

        // Tilastot sivun pohjalla
        private string GetPlayerInfoText(Player player)
        {
            return $"Etunimi: {player.FirstName}\n" +
                   $"Sukunimi: {player.LastName}\n" +
                   $"Syntymäpäivä: {player.DateOfBirth.ToShortDateString()}\n" +
                   $"Voitot: {player.Wins}\n" +
                   $"Tappiot: {player.Losses}\n" +
                   $"Tasapelit: {player.Draws}\n" +
                   $"Kulunut aika yhteensä: {player.TimeElapsed.TotalSeconds:F2}";
        }

        private void CreateAiPlayer() // AI:n luonti
        {
            try
            {
                // AI pelaajaa ei ole, tehdään uusi
                Player aiPlayer = new Player
                {
                    FirstName = "Computer",
                    LastName = "AI",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Wins = 0,
                    Losses = 0,
                    Draws = 0,
                    TimeElapsed = TimeSpan.Zero,
                };
                players.Add(aiPlayer);
                SavePlayerData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating AI player: {ex.Message}");
            }
        }
        private bool PlayerExists(string playerName) // käytetään vain AI:n olemassa olon tarkistukseen, jottei luoda toista. Voisi käyttää myös muihin pelaajiin, jos duplikaatteja ei sallita
        {
            return players.Any(player => player.FullName == playerName);
        }
        private void LoadPlayerData() // hakee pelaajien tilastot ja tiedot playerdata.txt tiedostosta
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    using (StreamReader reader = new StreamReader(dataFilePath))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Player>));
                        players = (ObservableCollection<Player>)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading player data: {ex.Message}");
            }
        }
        public void SavePlayerData() // tallentaa pelaajien tilastot ja tiedot playerdata.txt tiedostoon
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(dataFilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Player>));
                    serializer.Serialize(writer, players);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving player data: {ex.Message}");
            }
        }
        // ohjelmasta löytyy nappi, mitä painamalla käynnistetään sovellus uudestaan, jolloin voi esim vaihtaa pelaajia, tai vaihtaa vaikka bottipeliin.
        private void RestartAppClicked(object sender, EventArgs e)
        {
            SavePlayerData();
            App.Current.MainPage = new MainPage();
        }
        // Tuodaan esille sopiva määrä kenttiä, mihin informaatiota pyydetään, ja piilotetaan tässä vaiheessa turhat napit.
        private void AddPlayersButtonClicked(object sender, EventArgs e)
        {
            PlayerEntry.IsVisible = true;
            AddPlayers.IsVisible = false;
        }
        // sama tässä, mutta eri nappi :)
        private void PvP_Clicked(object sender, EventArgs e)
        {
            AddPlayers.IsVisible = true;
            PlayerInfo.IsVisible = true;
            StartButton.IsVisible = true;
            PlayerSelect.IsVisible = true;
            ResetButton.IsVisible = true;
            GameMode.IsVisible = false;
        }
        // sama tässä, mutta eri nappi :)
        private void PvAI_Clicked(object sender, EventArgs e)
        {
            AddPlayers.IsVisible = true;
            StartButton.IsVisible = true;
            PlayerInfo.IsVisible = true;
            PlayerSelect.IsVisible = true;
            ResetButton.IsVisible = true;
            GameMode.IsVisible = false;

            // Piilotetaan toisen pelaajan valinta, ja asetetaan pelaaja O:ksi AI
            PlayerPickerO.IsVisible = false;
            Player aiPlayer = players.FirstOrDefault(player => player.FullName == "Computer AI");
            PlayerPickerO.SelectedItem = aiPlayer;

            // laitetaan myös ai toimintaan tällä ehdolla. Ai ei tee mitään, jos tämä ehto ei täyty.
            isAgainstAI = true;
        }
        // pelin aloitusnappi
        private void StartButtonClick(object sender, EventArgs e)
        {
            Player selectedPlayerX = GetSelectedPlayer(PlayerPicker);
            Player selectedPlayerO = GetSelectedPlayer(PlayerPickerO);

            // pelaajien valintojen tarkistuksia, error viestit kertovat aika hyvin mitä nämä ovat ;)
            if (PlayerPicker.SelectedItem == PlayerPickerO.SelectedItem)
            {
                DisplayAlert("Error", "Pelaajat X ja O eivät voi olla sama pelaaja.", "OK");
                return;
            }
            if (selectedPlayerX?.FullName == "Computer AI")
            {
                DisplayAlert("Error", "Valitse jokin muu pelaaja, kuin Computer AI.", "OK");
                return;
            }
            if (selectedPlayerO?.FullName == "Computer AI" && isAgainstAI == false)
            {
                DisplayAlert("Error", "Valitse jokin muu pelaaja, kuin Computer AI.", "OK");
                return;
            }
            if (PlayerPicker.SelectedItem == null && isAgainstAI == true)
            {
                DisplayAlert("Error", "Valitse pelaaja", "OK");
                return;
            }
            if (PlayerPicker.SelectedItem == null || PlayerPickerO.SelectedItem == null)
            {
                DisplayAlert("Error", "Valitse molemmat pelaajat", "OK");
                return;
            }
            

            // Vaihdetaan vähän elementtien näkyvyyttä napin painalluksesta.
            GameGrid.IsVisible = true;
            PohjaNapit.IsVisible = true;
            StartButton.IsVisible = false;
            AddPlayers.IsVisible = false;
            PlayerEntry.IsVisible = false;
            PlayerSelect.IsVisible = false;

            SavePlayerData();
            InitializeGameBoard();
            UpdateCurrentTurnLabel();
        }
        private Player GetSelectedPlayer(Picker picker) // kertoo muille funktioille, mikä pelaaja on valittu.
        {
            if (picker.SelectedIndex != -1)
            {
                return players[picker.SelectedIndex];
            }
            return null;
        }
        private void UpdateCurrentTurnLabel() // näyttää sivun alhaalla olevassa elementissä kuluvan vuoron
        {
            CurrentTurnLabel.Text = $"Pelaajan {(playerX ? "X" : "O")} vuoro.";
        }

        private void InitializeAiTimer() // aloitetaan ajastin botin toimintaa varten
        {
            // botti pelaa 0.5s - 2s viiveellä
            Random random = new Random();
            double randomInterval = random.NextDouble() * (2.0 - 0.5) + 0.5;

            aiTimer = new System.Timers.Timer(randomInterval * 1000); // käännetään millisekunneiksi
            aiTimer.Elapsed += OnAiTimerElapsed;
            aiTimer.AutoReset = true;
            aiTimer.Start();
        }
        private void UpdatePlayerStatistics(Player player, GameResult result) // statistiikkojen päivitys tiedostoon
        {
            try
            {
                switch (result)
                {
                    case GameResult.Win:
                        player.Wins++;
                        break;
                    case GameResult.Loss:
                        player.Losses++;
                        break;
                    case GameResult.Draw:
                        player.Draws++;
                        break;
                }
                SavePlayerData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating player statistics: {ex.Message}");
            }
        }
        private enum GameResult // mahdolliset tulokset pelissä
        {
            Win,
            Loss,
            Draw
        }

        // Luo pelikentän
        private void InitializeGameBoard()
        {
            buttons = new Button[3, 3];
            turnStopwatch = new Stopwatch();

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var button = new Button // pelikentän ruutujen muotoilua
                    {
                        Text = "",
                        FontSize = 24,
                        WidthRequest = 80,
                        HeightRequest = 80,
                        CommandParameter = (row, col)
                    };
                    button.Clicked += OnButtonClicked;

                    buttons[row, col] = button;

                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);

                    GameGrid.Children.Add(button);
                }
            }
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            if (isAgainstAI && !playerX) // tarkistaa että peli on ai:ta vastaan, jos on niin ai tekee liikkeen. Tämä alempana tarkemmin kommentoituna :)
            {
                MakeAIMove();
            }
            else if (sender is Button button) // jos peli ei ole AI:ta vastaan, niin tehdään tämä.
            {
                int row = Grid.GetRow(button);
                int col = Grid.GetColumn(button);

                if (button.Text == "" && moves < 9)
                {
                    button.Text = playerX ? "X" : "O";
                    playerX = !playerX; // vaihdetaan vuoroa
                    moves++;

                    Player playerXInstance = GetSelectedPlayer(PlayerPicker);
                    Player playerOInstance = GetSelectedPlayer(PlayerPickerO);

                    UpdateCurrentTurnLabel(); // näytetään vielä, että vuoroa on vaihdettu

                    if (CheckForWinner(row, col)) // Tarkistetaan voittaja, sekä päivitetään tilastoja. Tulokset kertoo aika hyvin missä on mitäkin ;)
                    {
                        if (playerX)
                        {
                            UpdatePlayerStatistics(playerOInstance, GameResult.Win);
                            UpdatePlayerStatistics(playerXInstance, GameResult.Loss);
                            ResultLabel.Text = $"Tulos: Pelaaja O voitti!";
                        }
                        else
                        {
                            UpdatePlayerStatistics(playerXInstance, GameResult.Win);
                            UpdatePlayerStatistics(playerOInstance, GameResult.Loss);
                            ResultLabel.Text = $"Tulos: Pelaaja X voitti!";
                        }
                        DisableButtons();
                    }
                    else if (moves == 9) 
                    {
                        UpdatePlayerStatistics(playerXInstance, GameResult.Draw);
                        UpdatePlayerStatistics(playerOInstance, GameResult.Draw);
                        ResultLabel.Text = "Tulos: Tasapeli!";
                    }
                    // Tallennetaan tiedot varmuudeksi
                    SavePlayerData();

                    // Lisätään aikaa pelaajien tiedostoihin, päivitetään vain pelaaja X jos on bottipeli.
                    if (isAgainstAI)
                    {
                        UpdatePlayerTurnTime(playerXInstance);
                    }
                    else
                    {
                        UpdatePlayerTurnTime(playerX ? playerXInstance : playerOInstance);
                    }
                }
            }
        }

        private void MakeAIMove()
        {
            bool draw = true;
            Player playerXInstance = GetSelectedPlayer(PlayerPicker);
            Player playerOInstance = GetSelectedPlayer(PlayerPickerO);

            // tosi tyhmä ai, etsii ensimmäisen vapaan ruudun ja asettaa siihen O:n
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (buttons[row, col].Text == "")
                    {
                        buttons[row, col].Text = "O"; // AI on aina "O"
                        playerX = true;
                        moves++;

                        UpdateCurrentTurnLabel();

                        if (CheckForWinner(row, col)) // AI:n voitontarkistus
                        {
                            Player selectedPlayer = GetSelectedPlayer(PlayerPickerO);
                            if (selectedPlayer != null)
                            {
                                ResultLabel.Text = "Result: AI wins!";
                                UpdatePlayerStatistics(playerXInstance, GameResult.Loss);
                                UpdatePlayerStatistics(playerOInstance, GameResult.Win);
                                DisableButtons();
                            }
                            draw = false;
                        }
                        else if (moves == 9) // Tasapelissä tämä
                        {
                            Player selectedPlayer = GetSelectedPlayer(PlayerPickerO);
                            if (selectedPlayer != null)
                            {
                                ResultLabel.Text = "Result: Draw!";
                                UpdatePlayerStatistics(playerXInstance, GameResult.Draw);
                                UpdatePlayerStatistics(playerOInstance, GameResult.Draw);
                            }
                            draw = false;
                        }

                        // Only make one move and then exit the loop
                        return;
                    }
                }
            }
            // Jos looppi menee jostain syystä kokonaan läpi, mutta voittavaa liikettä ole tehty, päivitetään silti tilastoja. Tämä on vain varmistus
            if (draw && moves == 9)
            {
                ResultLabel.Text = "Result: Draw!";
                UpdatePlayerStatistics(playerXInstance, GameResult.Draw);
                UpdatePlayerStatistics(playerOInstance, GameResult.Draw);
            }

            // Tallennetaan varmuudeksi joka vuoron jälkeen statistiikka, vaikkei olisi välttämätöntä
            SavePlayerData();

            // Päivitetään AI:n käyttämä aika tilastoihin
            UpdatePlayerTurnTime(playerOInstance);
        }
        private void UpdatePlayerTurnTime(Player player) // funktio pelaajien kokonaisajan lisäämiseen
        {
            if (turnStopwatch != null && player != null)
            {
                turnStopwatch.Stop();
                player.TimeElapsed = player.TimeElapsed.Add(turnStopwatch.Elapsed);
                turnStopwatch.Reset();
                turnStopwatch.Start();
                SavePlayerData();
            }
            else if (turnStopwatch == null)
            {
                // tehdään ajastin, jos se on null
                turnStopwatch = new Stopwatch();
                turnStopwatch.Start();
            }

            // AI:n ajan seuranta
            if (!playerX && isAgainstAI)
            {
                Player aiPlayer = GetSelectedPlayer(PlayerPickerO);
                aiPlayer.TimeElapsed = aiPlayer.TimeElapsed.Add(TimeSpan.FromSeconds(aiTimer.Interval / 1000));
                SavePlayerData();
            }
        }
        private async void OnAiTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // ai tekee liikkeen vain jos on ai:n vuoro, pelimuoto on Ai:ta vastaan, eikä peli ole ohi.
            if (!playerX && isAgainstAI && moves < 9 && !IsGameOver())
            {
                // ai tekee liikkeen, kun ylläoleva ehto täyttyy
                await Device.InvokeOnMainThreadAsync(() => MakeAIMove());

                Random random = new Random();
                double randomInterval = random.NextDouble() * (2.0 - 0.5) + 0.5;
                aiTimer.Interval = randomInterval * 1000; // Convert seconds to milliseconds
            }
        }

        // Tuloksen tarkistus
        private bool CheckForWinner(int row, int col)
        {
            // Tarkistetaan rivi
            if (buttons[row, 0].Text == buttons[row, 1].Text && buttons[row, 1].Text == buttons[row, 2].Text && buttons[row, 0].Text != "")
            {
                return true;
            }

            // Tarkistetaan kolumni
            if (buttons[0, col].Text == buttons[1, col].Text && buttons[1, col].Text == buttons[2, col].Text && buttons[0, col].Text != "")
            {
                return true;
            }

            // Ja vielä viistoon tarkistus
            if ((buttons[0, 0].Text == buttons[1, 1].Text && buttons[1, 1].Text == buttons[2, 2].Text && buttons[0, 0].Text != "") ||
                (buttons[0, 2].Text == buttons[1, 1].Text && buttons[1, 1].Text == buttons[2, 0].Text && buttons[0, 2].Text != ""))
            {
                return true;
            }
            return false;
        }
        // Tarkistaa vielä varmuudeksi, onko peli ohi ja estää AI:ta tekemästä liikkeitä, jos on
        private bool IsGameOver()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (CheckForWinner(row, col) || moves == 9)
                    {
                        SavePlayerData();
                        return true;
                    }
                }
            }
            return false;
        }

        // Estää ihmisiä tekemästä lisää liikkeitä kun peli on ohi.
        private void DisableButtons()
        {
            foreach (var button in buttons)
            {
                button.IsEnabled = false;
            }
        }

        // Nollaa laudan tilanteen, eli ns. uusi peli
        private void OnResetClicked(object sender, EventArgs e)
        {
            foreach (var button in buttons)
            {
                button.Text = "";
                button.IsEnabled = true;
            }

            ResultLabel.Text = "Result: ";
            playerX = true;
            moves = 0;

            UpdateCurrentTurnLabel();
            SavePlayerData();

            Player playerXInstance = GetSelectedPlayer(PlayerPicker);
            Player playerOInstance = GetSelectedPlayer(PlayerPickerO);
            DisplayPlayerInformation(playerXInstance);
            DisplayPlayer2Information(playerOInstance);
        }
    }
    // Pelaaja-luokka, sisältää vaadittavat tilastot mitä tallennetaan, sekä ladataan.
    public class Player
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public TimeSpan TimeElapsed { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public override string ToString()
        {
            return FullName; // Näytetään pickerissä pelaajan nimi
        }
    }
}