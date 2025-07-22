using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
        
        

        public MainWindow()
        {
            InitializeComponent();
            SwitchPage(sender: HomeButton, e: new RoutedEventArgs());

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

                    _chatWindow = new ChatWindow(scenario);
                _chatWindow.Closed += (s, args) => _chatWindow = null;
            }
            _chatWindow.Activate();
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
                selectedPack = flashcardPacks[0];
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
                        Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x55, 0x55, 0x55))
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
    
        
    }



