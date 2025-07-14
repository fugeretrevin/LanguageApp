using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;

namespace WinFormsApp1
{

    public partial class ChatRoom : Form
    {
        private static readonly HttpClient client = new HttpClient();

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
        public ChatRoom()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private async void CheckEnterKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)

            {
                richTextBox3.Clear();
                string userInput = textBox1.Text;
                richTextBox1.AppendText("You: " + userInput + "\n" + "\n");
                textBox1.Clear();

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
                richTextBox1.AppendText(aiResponse + "\n");
                richTextBox2.AppendText(aiFeedback + "\n");
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
                int startInd = 0;
                richTextBox3.AppendText(correctedUserInput + "\n");

                foreach (string x in spellingMistakes)
                {
                    string correct = x.Substring(x.IndexOf(" - ") + 3).Trim();
                    int indexInBox = richTextBox3.Text.IndexOf(correct, startInd);
                    if (indexInBox != -1)
                    {
                        richTextBox3.Select(indexInBox, correct.Length);
                        richTextBox3.SelectionColor = Color.Green;
                    }

                }
                foreach (string x in grammarMistakes)
                {
                    string correct = x.Substring(x.IndexOf(" - ") + 3).Trim();
                    int indexInBox = richTextBox3.Text.IndexOf(correct, startInd);
                    if (indexInBox != -1)
                    {
                        richTextBox3.Select(indexInBox, correct.Length);
                        richTextBox3.SelectionColor = Color.Orange;
                    }

                }
                richTextBox3.DeselectAll();
                richTextBox3.SelectionStart = richTextBox3.TextLength;
                richTextBox3.SelectionColor = richTextBox3.ForeColor;

                
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
