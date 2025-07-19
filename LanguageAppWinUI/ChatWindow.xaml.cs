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
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LanguageAppWinUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class ChatWindow : Window
    {
            private static readonly HttpClient client = new HttpClient();
            public ChatWindow(string scenario)
            {
            SetScenario(scenario);

            this.InitializeComponent();
            }

        private async void SetScenario(string scenario)
        {
           string aiResponse = await SendMessageAsync(scenario);
            aiResponse = aiResponse.Substring(0, aiResponse.IndexOf("Feedback:"));
            var aiResponseParagraph = new Paragraph();
            aiResponseParagraph.Inlines.Add(new Run { Text = $"{aiResponse}" });
            ChatDisplay.Blocks.Add(aiResponseParagraph);



        }

        private async Task<string> SendMessageAsync(string userMessage)
            {
                var payload = new { message = userMessage, session_id = "mySession123" };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("http://localhost:5000/chat", content);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseString);
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
                    InputBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string userInput);
                    InputBox.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");

                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run { Text = $"You: {userInput}" });
                ChatDisplay.Blocks.Add(paragraph);
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
                    var aiResponseParagraph = new Paragraph();
                    var aiFeedbackParagraph = new Paragraph();

                    aiResponseParagraph.Inlines.Add(new Run { Text = $"{aiResponse}\n"});
                    aiFeedbackParagraph.Inlines.Add(new Run { Text = $"{aiFeedback}\n" });

                    ChatDisplay.Blocks.Add(aiResponseParagraph);
                    AiFeedbackDisplay.Blocks.Add(aiFeedbackParagraph);
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


                }

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
            
    }
    }



    
    

