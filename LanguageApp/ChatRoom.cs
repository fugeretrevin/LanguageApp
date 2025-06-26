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

                string aiResponse = await SendMessageAsync(userInput);
                richTextBox1.AppendText(aiResponse + "\n");
            }
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
