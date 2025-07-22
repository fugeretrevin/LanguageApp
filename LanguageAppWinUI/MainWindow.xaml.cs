using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageAppWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        private Window _chatWindow;
        List<FlashcardPack> flashcardPacks;
        FlashcardPack selectedPack;
        List<Flashcard> cards;
        Flashcard currentCard;
        int currentCardIndex = 0;
        int flashcardSelection;

        UserStats stats;
        private DispatcherTimer _timeLoggerTimer;



        public MainWindow()
        {
            InitializeComponent();
            SwitchPage(sender: HomeButton, e: new RoutedEventArgs());
            stats = StatsManager.LoadStats();
            StartTimeLogger();


        }

        private void StartTimeLogger()
        {
            _timeLoggerTimer = new DispatcherTimer();
            _timeLoggerTimer.Interval = TimeSpan.FromSeconds(60);
            _timeLoggerTimer.Tick += TimeLogger_Tick;
            _timeLoggerTimer.Start();



        }
        private void TimeLogger_Tick(object sender, object e)
        {
            StatsManager.LogTimeSpent(60);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*var dialog = new ContentDialog
            {
                Title = "Hello!",
                Content = "You clicked the button.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot // Required for ContentDialog

            };
            
            _ = dialog.ShowAsync();
            */
            if (_chatWindow == null)
            {
                if (sender is Button btn && btn.Tag is string scenario)
                {
                    StatsManager.ChangeMostRecentScenario(scenario);
                    UpdateMostRecentTile(scenario);


                    _chatWindow = new ChatWindow(scenario);
                }

                   

                _chatWindow.Closed += (s, args) => _chatWindow = null;
            }



            _chatWindow.Activate();
        }

        private void UpdateMostRecentTile(string scenario)
        {
            int start = scenario.IndexOf('(') + 1;
            int end = scenario.IndexOf(')');
            string imageName = scenario.Substring(start, end - start);
            string imageSource = $"ms-appx:///images/{imageName}.png";
            MostRecentScenarioImage.Source = new BitmapImage(new Uri(imageSource));
            foreach (var child in ScenariosPage.Children)
            {
                if (child is FrameworkElement fe && fe.Tag is string tag && tag == scenario)
                {
                    if (fe is Button btn)
                    {
                        // The Button contains a StackPanel -> Image + TextBlock
                        if (btn.Content is StackPanel stack)
                        {
                            foreach (var stackChild in stack.Children)
                            {
                                if (stackChild is TextBlock textBlock)
                                {
                                    MostRecentScenarioTitle.Text = textBlock.Text;
                                    MostRecentScenarioBlock.Tag = tag;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void SwitchPage(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            ChangeSelected(button);
            foreach (var child in BaseLayout.Children)
            {
                if (child is FrameworkElement fe && fe.Tag is string tag && tag == "PageContent")
                {
                    fe.Visibility = Visibility.Collapsed;
                }
            }
            var btnTag = button.Tag as string;

            var contentToShow = BaseLayout.Children
                        .OfType<FrameworkElement>()
                        .FirstOrDefault(fe => fe.Name == btnTag || (fe.Tag is string t && t == btnTag));

            if (contentToShow != null)
            {
                contentToShow.Visibility = Visibility.Visible;
            }

            if (btnTag == "FlashcardsPage")
            {
                flashcardPacks = await LoadFlashcardPacksAsync();
                selectedPack = flashcardPacks[flashcardSelection];
                cards = selectedPack.Cards;
                currentCardIndex = 0;
                currentCard = cards[currentCardIndex];
                FlashcardText.Text = currentCard.Phrase;
                string phrase = currentCard.Phrase;
                string translation = currentCard.Translation;

                FlashcardPacksMenuFlyout.Items.Clear();

                foreach (var pack in flashcardPacks)
                {
                    var item = new MenuFlyoutItem { Text = pack.Title };
                    item.Click += (s, e) => {
                        // When user clicks this pack:
                        StartPack(pack);
                    };
                    FlashcardPacksMenuFlyout.Items.Add(item);
                }




                FlashcardText.Text = phrase;
                FlashcardPackTitleText.Text = selectedPack.Title;
                FlashcardPackTitleText.Text += $" ({currentCardIndex + 1}/{cards.Count})";


            }
            if (btnTag == "MistakesPage")
            {
                UpdateMistakes();


            }
            if (btnTag == "HomePage")
            {
                stats = StatsManager.LoadStats();

                UpdateHomePage();
            }
        }

        private async void UpdateHomePage()
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            int todaysMessages = stats.DailyStats.ContainsKey(today) ? stats.DailyStats[today].MessagesSent : 0;
            int todaysTimeSec = stats.DailyStats.ContainsKey(today) ? stats.DailyStats[today].TimeSpentSeconds : 0;
            string todaysTimeFormatted = TimeSpan.FromSeconds(todaysTimeSec).ToString(@"hh\:mm");


            string totalTimeFormatted = TimeSpan.FromSeconds(stats.TotalTimeSpentSeconds).ToString(@"hh\:mm");
            TotalTimeSpentText.Text = totalTimeFormatted;
            TodaysTimeSpentText.Text = $"{todaysTimeFormatted} Today";
            TotalMessagesSentText.Text = stats.TotalMessagesSent.ToString();
            MessagesSentTodayText.Text = $"{todaysMessages} Today";

            if (stats.TotalTimeSpentSeconds < 600)
            {
                WelcomeText.Text = "Welcome!";
            }
            else
            {
                WelcomeText.Text = "Welcome back!";

            }
            if (todaysTimeSec > 600) {
                YouStudiedForText.Text = $"You have studied for {todaysTimeSec / 60} minutes today. Great work!";

            }
            else if (todaysTimeSec < 600 && todaysTimeSec > 120)
            {
                YouStudiedForText.Text = $"You have studied for {todaysTimeSec / 60} minutes today. Keep it up!";

            }
            else
            {
                YouStudiedForText.Text = $"Explore your stats and learning options below";

            }

            flashcardPacks = await LoadFlashcardPacksAsync();

            Random rand = new Random();
            flashcardSelection = rand.Next(0, flashcardPacks.Count);
            RecommendedFlashcardsText.Text = flashcardPacks[flashcardSelection].Title;
            if (stats.MostRecentScenario == null)
            {
                MostRecentScenarioBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                MostRecentScenarioBlock.Visibility = Visibility.Visible;

                UpdateMostRecentTile(stats.MostRecentScenario);

            }


        }

        private void GoToRecommendedFlashcards(object sender, RoutedEventArgs e)
        {
        
            SwitchPage(FlashcardsButton, new RoutedEventArgs());

           


        }

        private void ChangeSelected(Button selectedButton)
        {
            foreach (var child in Sidebar.Children)
            {
                if (child is Button x)
                {
                    x.Background = new SolidColorBrush(default); // Or default
                }
            }
            selectedButton.Background = new SolidColorBrush(Color.FromArgb(255, 0x20, 0x20, 0x20)); // Example selected color

        }
        private void FlashcardRight(object sender, RoutedEventArgs e)
        {
            currentCardIndex++;
            if (currentCardIndex >= cards.Count)
            {
                currentCardIndex = 0;
                currentCard = cards[currentCardIndex];
            }
            else
            {
                currentCard = selectedPack.Cards[currentCardIndex];

            }
            FlashcardText.Text = currentCard.Phrase;
            FlashcardPackTitleText.Text = selectedPack.Title;
            FlashcardPackTitleText.Text += $" ({currentCardIndex + 1}/{cards.Count})";
        }
        private void FlashcardLeft(object sender, RoutedEventArgs e)
        {
            currentCardIndex--;
            if (currentCardIndex < 0)
            {
                currentCardIndex = cards.Count - 1;
                currentCard = cards[currentCardIndex];
            }
            else
            {
                currentCard = selectedPack.Cards[currentCardIndex];

            }
            FlashcardText.Text = currentCard.Phrase;
            FlashcardPackTitleText.Text = selectedPack.Title;
            FlashcardPackTitleText.Text += $" ({currentCardIndex + 1}/{cards.Count})";

        }
        private void FlashcardRotate(object sender, RoutedEventArgs e)
        {
            if (FlashcardText.Text == currentCard.Phrase)
            {
                FlashcardText.Text = currentCard.Translation;
            }
            else
            {
                FlashcardText.Text = currentCard.Phrase;

            }
        }
        private void StartPack(FlashcardPack pack)
        {
            selectedPack = pack;
            cards = pack.Cards;
            currentCardIndex = 0;

            currentCard = cards[currentCardIndex];
            FlashcardText.Text = currentCard.Phrase;
            FlashcardPackTitleText.Text = selectedPack.Title;
            FlashcardPackTitleText.Text += $" ({currentCardIndex + 1}/{cards.Count})";


        }
        private async void UpdateMistakes()
        {
            MistakesText.Blocks.Clear();

            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("mistakes.json");

            string json = await FileIO.ReadTextAsync(file);
            List<MistakeSentence> mistakeSentences = JsonSerializer.Deserialize<List<MistakeSentence>>(json);
            
            foreach (var mistakeSentence in mistakeSentences)
            {
                var paragraph = new Paragraph();

                Run sentence = new Run
                {
                    Text = $"Sentence: {mistakeSentence.Sentence}\n",
                };
                paragraph.Inlines.Add(sentence);
                
                foreach (var mis in mistakeSentence.Mistakes)
                {
                    Run mistake = new Run
                    {
                        Text = $"Mistake: {mis.Incorrect}\n"
                    };
                    Run corrected = new Run
                    {
                        Text = $"Correction: {mis.Corrected}\n",
                        Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 34, 139, 34))
                    };
                    paragraph.Inlines.Add(mistake);

                    paragraph.Inlines.Add(corrected);

                }
                MistakesText.Blocks.Add(paragraph);

            }
        }
        public async Task<List<FlashcardPack>> LoadFlashcardPacksAsync()
        {

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/flashcards.json")); 
            string json = await FileIO.ReadTextAsync(file);
            return JsonSerializer.Deserialize<List<FlashcardPack>>(json);
        }
    }
    public class Flashcard
    {
        public Flashcard() { }
        public string Phrase { get; set; }
        public string Translation { get; set; }
    }

    public class FlashcardPack
    {
        public FlashcardPack() { }
        public string Title { get; set; }
        public List<Flashcard> Cards { get; set; }
    }
    public class Mistake
    {
        public Mistake() { }
        public string Incorrect { get; set; }
        public string Corrected { get; set; }



    }
    public class MistakeSentence
    {
        public MistakeSentence() { }
        public string Sentence { get; set; }
        public List<Mistake> Mistakes { get; set; }

    }
    public class DailyStats
    {
        public DailyStats() { }
        public int MessagesSent { get; set; }
        public int TimeSpentSeconds { get; set; }
    }
    public class UserStats
    {
        public UserStats() { }
        public int TotalMessagesSent { get; set; }
        public int TotalTimeSpentSeconds { get; set; }
        public string MostRecentScenario { get; set; }
        public Dictionary<string, DailyStats> DailyStats { get; set; } = new();

    }
    public static class StatsManager
    {
        private static readonly string StatsFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "stats.json");
        private static UserStats _stats;

        public static UserStats LoadStats()
        {
            if (!File.Exists(StatsFilePath))
            {
                _stats = new UserStats();
                SaveStats();
            }
            else
            {
                string json = File.ReadAllText(StatsFilePath);
                _stats = JsonSerializer.Deserialize<UserStats>(json) ?? new UserStats();

            }
            return _stats;
        }
        public static void SaveStats()
        {
            string json = JsonSerializer.Serialize(_stats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StatsFilePath, json);
        }
        public static void LogMessageSent()
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");

            _stats.TotalMessagesSent++;
            if (!_stats.DailyStats.ContainsKey(today))
            {
                _stats.DailyStats[today] = new DailyStats();
            }
            _stats.DailyStats[today].MessagesSent++;
            SaveStats();
        }
        public static void ChangeMostRecentScenario(string scenario)
        {
            _stats.MostRecentScenario = scenario;
            SaveStats();
            
        }
        public static void LogTimeSpent(int seconds)
        {
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            _stats.TotalTimeSpentSeconds += seconds;
            if (!_stats.DailyStats.ContainsKey(today))
            {
                _stats.DailyStats[today] = new DailyStats();
            }
            _stats.DailyStats[today].TimeSpentSeconds += seconds;
            SaveStats();
        }
    }
    }



