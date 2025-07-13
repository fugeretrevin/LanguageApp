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
                string userInput = textBox1.Text;
                richTextBox1.AppendText("You: " + userInput + "\n" + "\n");
                textBox1.Clear();

                string aiOutput = await SendMessageAsync(userInput);
                string aiResponse = aiOutput.Substring(0, aiOutput.IndexOf("Feedback:"));
                string aiFeedback = aiOutput.Substring(aiOutput.IndexOf("Feedback:"));
                List<string> spellingMistakes = new();
                List<string> grammarMistakes = new();
                if (!aiFeedback.Contains("None"))
                {
                    int spellingMistakeAmt = int.Parse(aiFeedback.Substring(aiFeedback.IndexOf("Spelling:") - 3, 1));
                    int currInd = aiFeedback.IndexOf("Spelling:") + 10;
                    for (int i = 0; i < spellingMistakeAmt; i++)
                    {
                        int nextCommaIndex = aiFeedback.IndexOf(',', currInd);
                        if (nextCommaIndex != -1)
                        {
                            int length = nextCommaIndex - currInd;
                            spellingMistakes.Add(aiFeedback.Substring(currInd, length));
                            currInd = nextCommaIndex + 1;


                        }
                        else
                        {
                            nextCommaIndex = aiFeedback.IndexOf('(', currInd);
                            int length = nextCommaIndex - currInd;
                            spellingMistakes.Add(aiFeedback.Substring(currInd, length));
                        }

                    }


                }
                richTextBox1.AppendText(aiResponse + "\n");
                richTextBox2.AppendText(aiFeedback + "\n");
                foreach (string x in spellingMistakes)
                {
                    richTextBox2.AppendText(x + "\n");

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
    }
}
