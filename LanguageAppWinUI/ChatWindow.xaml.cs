using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;

using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageAppWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class ChatWindow : Page
    {
            private static readonly HttpClient client = new HttpClient();
        private readonly Action _returnCallback;
        private readonly string _scenario;
        public ChatWindow(string scenario, Action returnCallback)
            {
            this.InitializeComponent();
            _returnCallback = returnCallback;
            _scenario = scenario;
            SetScenario(scenario);

        }

       
        private async void SetScenario(string scenario)
        {


            // Set the source and play


            // Set to MediaPlayerElement


            // Start playback
            AnimatedContainer.Visibility = Visibility.Collapsed;
            int end = scenario.IndexOf(".");
            string scenarioTitle = scenario.Substring(0, end);
            ScenarioTitleText.Text = scenarioTitle;

            string aiResponse = await SendMessageAsync(scenario);
            aiResponse = aiResponse.Substring(0, aiResponse.IndexOf("Feedback:"));
            aiResponse = aiResponse.Trim();
            AddChatBubble(aiResponse, false);

            int start = scenario.IndexOf('(') + 1;
            end = scenario.IndexOf(')');
            string imageName = scenario.Substring(start, end - start);
            string imageSource = $"ms-appx:///images/{imageName}.png";
            scenarioImage.Source = new BitmapImage(new Uri(imageSource));

        }

        private async Task<string> SendMessageAsync(string userMessage)
            {
                

            var payload = new { message = userMessage, session_id = "mySession123" };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("https://web-production-a5f66.up.railway.app/chat", content);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse>(responseString);
                return result.response;
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }
           

            private void textBox1_TextChanged(object sender, EventArgs e)
            {

            }
            private async void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
            {
            if (e.Key == Windows.System.VirtualKey.Enter)

            {
                string isNullString;
                InputBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out isNullString);

                if (string.IsNullOrWhiteSpace(isNullString))
                    return;

                InputBox.IsEnabled = false;
                AnimateOutAndHide();
                InputBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string userInput);
                InputBox.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");
                try
                {
                    userInput = userInput.Trim();
                    AddChatBubble(userInput, true);
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await Task.Delay(50); // Let layout update
                        ChatScrollViewer.ChangeView(null, ChatScrollViewer.ScrollableHeight, null, false);
                    });


                    string aiOutput = await SendMessageAsync(userInput);
                    string aiResponse = aiOutput.Substring(0, aiOutput.IndexOf("Feedback:"));
                    string aiFeedback = aiOutput.Substring(aiOutput.IndexOf("Feedback:"));
                    string spellingFeedback = aiFeedback.Substring(aiFeedback.IndexOf(") Spelling:") - 2, aiFeedback.IndexOf(") Grammar") - 2);
                    string grammarFeedback = aiFeedback.Substring(aiFeedback.IndexOf(") Grammar") - 2);
                    List<string> spellingMistakes = new();
                    List<string> grammarMistakes = new();
                    if (!spellingFeedback.Contains("None"))
                    {
                        int spellingMistakeAmt = int.Parse(spellingFeedback.Substring(1, 1));
                        int currInd = spellingFeedback.IndexOf("Spelling:") + 10;
                        for (int i = 0; i < spellingMistakeAmt; i++)
                        {
                            int nextCommaIndex = spellingFeedback.IndexOf(',', currInd);
                            if (nextCommaIndex != -1)
                            {
                                int length = nextCommaIndex - currInd;
                                spellingMistakes.Add(spellingFeedback.Substring(currInd, length).Trim());
                                currInd = nextCommaIndex + 1;


                            }
                            else
                            {
                                nextCommaIndex = spellingFeedback.IndexOf('(', currInd);
                                int length = nextCommaIndex - currInd;
                                spellingMistakes.Add(spellingFeedback.Substring(currInd, length).Trim());
                            }

                        }


                    }
                    if (!grammarFeedback.Contains("None"))
                    {
                        int grammarMistakeAmt = int.Parse(grammarFeedback.Substring(1, 1));
                        int currInd = grammarFeedback.IndexOf("Grammar:") + 8;
                        for (int i = 0; i < grammarMistakeAmt; i++)
                        {
                            int nextCommaIndex = grammarFeedback.IndexOf(',', currInd);
                            if (nextCommaIndex != -1)
                            {
                                int length = nextCommaIndex - currInd;
                                grammarMistakes.Add(grammarFeedback.Substring(currInd, length).Trim());
                                currInd = nextCommaIndex + 1;


                            }
                            else
                            {
                                nextCommaIndex = grammarFeedback.IndexOf('\n', currInd);
                                int length = nextCommaIndex - currInd;
                                grammarMistakes.Add(grammarFeedback.Substring(currInd, length).Trim());
                            }

                        }


                    }
                    var aiFeedbackParagraph = new Paragraph();

                    aiFeedbackParagraph.Inlines.Add(new Run { Text = $"{aiFeedback}\n" });
                    aiResponse = aiResponse.Trim();
                    AddChatBubble(aiResponse, false);
                    string correctedUserInput = userInput;
                    foreach (string x in spellingMistakes)
                    {
                        string incorrect = x.Substring(0, x.IndexOf(" - ")).Trim();
                        string correct = x.Substring(x.IndexOf(" - ") + 3).Trim();

                        correctedUserInput = correctedUserInput.Replace(incorrect, correct);


                    }
                    foreach (string x in grammarMistakes)
                    {
                        string incorrect = x.Substring(0, x.IndexOf(" - ")).Trim();
                        string correct = x.Substring(x.IndexOf(" - ") + 3).Trim();

                        correctedUserInput = correctedUserInput.Replace(incorrect, correct);


                    }
                    RichEditTextRange range = null;

                    int startInd = 0;
                    var correctedUserInputParagraph = new Paragraph();
                    correctedUserInputParagraph.Inlines.Add(new Run { Text = $"{correctedUserInput}\n" });
                    CorrectedUserDisplay.IsReadOnly = false;

                    CorrectedUserDisplay.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, correctedUserInput);
                    var fullRange = CorrectedUserDisplay.Document.GetRange(0, int.MaxValue);
                    fullRange.CharacterFormat.ForegroundColor = Microsoft.UI.Colors.White;
                    foreach (string x in spellingMistakes)
                    {
                        string correct = x.Substring(x.IndexOf(" - ") + 3).Trim();

                        CorrectedUserDisplay.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string fullText);
                        int index = fullText.IndexOf(correct, startInd, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                        {
                            range = (RichEditTextRange)CorrectedUserDisplay.Document.GetRange(index, index + correct.Length);
                            range.CharacterFormat.ForegroundColor = Microsoft.UI.Colors.Green;
                        }


                    }
                    foreach (string x in grammarMistakes)
                    {
                        string correct = x.Substring(x.IndexOf(" - ") + 3).Trim();

                        CorrectedUserDisplay.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string fullText);
                        int index = fullText.IndexOf(correct, startInd, StringComparison.OrdinalIgnoreCase);
                        if (index >= 0)
                        {
                            range = (RichEditTextRange)CorrectedUserDisplay.Document.GetRange(index, index + correct.Length);
                            range.CharacterFormat.ForegroundColor = Microsoft.UI.Colors.Orange;
                        }

                    }
                    if (grammarMistakes.Count > 0 || spellingMistakes.Count > 0)
                    {
                        MistakeSentence sen = new MistakeSentence
                        {
                            Sentence = userInput.Trim(),
                            Mistakes = new List<Mistake>()
                        };
                        foreach (string x in grammarMistakes)
                        {
                            Mistake m = new Mistake();
                            m.Incorrect = x.Substring(0, x.IndexOf(" - ")).Trim();
                            m.Corrected = x.Substring(x.IndexOf(" - ") + 3).Trim();
                            if((m.Incorrect.Trim() != m.Corrected.Trim()) && !m.Incorrect.Contains("User Grammar Mistake") && !m.Incorrect.Contains("User Misspelling"))
                            {
                                sen.Mistakes.Add(m);

                            }
                        }
                        foreach (string x in spellingMistakes)
                        {
                            Mistake m = new Mistake();
                            m.Incorrect = x.Substring(0, x.IndexOf(" - ")).Trim();
                            m.Corrected = x.Substring(x.IndexOf(" - ") + 3).Trim();
                            if ((m.Incorrect.Trim() != m.Corrected.Trim()) && !m.Incorrect.Contains("User Grammar Mistake") && !m.Incorrect.Contains("User Misspelling") && m.Corrected.Trim() != "?")
                            {
                                sen.Mistakes.Add(m);

                            }
                        }
                        CorrectedUserDisplay.Document.GetText(TextGetOptions.None, out string correctedText);

                        if (sen.Mistakes.Count != 0 && correctedText.Trim() != "?")
                        {
                            AnimateIn();
                        }
                        if (sen.Mistakes.Count != 0)
                        {
                            await AddMistake(sen);

                        }
                    }
                    StatsManager.LogMessageSent();
                }
                finally
                {
                    
                    InputBox.IsEnabled = true;
                    InputBox.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");
                    InputBox.Document.Selection.StartPosition = 0;
                    InputBox.Document.Selection.EndPosition = 0;
                    CorrectedUserDisplay.IsReadOnly = true;
                    InputBox.Focus(FocusState.Programmatic);


                }
                CorrectedUserDisplay.IsReadOnly = true;

            }

        }

        private async Task AddMistake(MistakeSentence sentence)
            {
            string fileName = "mistakes.json";
            StorageFile file;


            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            }
            catch
            {

                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
                await FileIO.WriteTextAsync(file, "[]");
            }
            string existingFile = await FileIO.ReadTextAsync(file);
            var list = System.Text.Json.JsonSerializer.Deserialize<List<MistakeSentence>>(existingFile) ?? new List<MistakeSentence>();
            list.Add(sentence);
            string updatedJson = System.Text.Json.JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await FileIO.WriteTextAsync(file, updatedJson);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
            {

            }

            private void Form2_Load(object sender, EventArgs e)
            {

            }

            private void richTextBox2_TextChanged(object sender, EventArgs e)
            {

            }

            private void richTextBox3_TextChanged(object sender, EventArgs e)
            {

            }


        private void GoHome(object sender, RoutedEventArgs e)
        {
            _returnCallback?.Invoke();
          

        }
        private void AddChatBubble(string message, bool isUser)
        {
            Border bubble = new Border
            {
                Background = isUser ? new SolidColorBrush(Colors.SteelBlue) : new SolidColorBrush(ColorHelper.FromArgb(255, 26, 26, 26)),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(5,5,5,10),
                Padding = new Thickness(10),
                MaxWidth = 400,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Opacity = 0, // Start invisible
                RenderTransform = new TranslateTransform { X = isUser ? 50 : -50 } // Slide in

            };

            TextBlock text = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI Variable Text"),
                FontSize = 18,
                TextWrapping = TextWrapping.WrapWholeWords
            };

            bubble.Child = text;
            ChatStackPanel.Children.Add(bubble);

            // Scroll to bottom
            DispatcherQueue.TryEnqueue(() =>
            {
                ChatScrollViewer.ChangeView(null, ChatScrollViewer.ScrollableHeight, null, false);
            });
            AnimateChatBubble(bubble);


        }

        private void AnimateIn()
        {
            var slideIn = new DoubleAnimation
            {
                From = 300, // start offset (px)
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(slideIn, SlideTransform);
            Storyboard.SetTargetProperty(slideIn, "X");

            var storyboard = new Storyboard();
            storyboard.Children.Add(slideIn);
            
            AnimatedContainer.Visibility = Visibility.Visible;
            
            storyboard.Begin();
        }

        private void AnimateOutAndHide()
        {
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = 300, // slide right
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            Storyboard.SetTarget(slideOut, SlideTransform);
            Storyboard.SetTargetProperty(slideOut, "X");

            var storyboard = new Storyboard();
            storyboard.Children.Add(slideOut);
            storyboard.Completed += (s, e) =>
            {
                AnimatedContainer.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin();
        }



        private void AnimateChatBubble(UIElement bubble)
        {
            var storyboard = new Storyboard();

            // Fade-in animation
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(600))
            };
            Storyboard.SetTarget(fade, bubble);
            Storyboard.SetTargetProperty(fade, "Opacity");

            // Slide animation
            var slide = new DoubleAnimation
            {
                From = ((TranslateTransform)bubble.RenderTransform).X,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slide, bubble);
            Storyboard.SetTargetProperty(slide, "(UIElement.RenderTransform).(TranslateTransform.X)");

            storyboard.Children.Add(fade);
            storyboard.Children.Add(slide);
            storyboard.Begin();
        }



    }
    public class ApiResponse
    {
        public string response { get; set; }
    }
}
    





