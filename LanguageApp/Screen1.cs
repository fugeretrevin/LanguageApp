namespace WinFormsApp1
{
    public partial class Screen1 : Form
    {
        public Screen1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChatRoom secondForm = new ChatRoom();
            secondForm.Show();
            this.Hide(); // Or this.Close();
        }
    }
}
