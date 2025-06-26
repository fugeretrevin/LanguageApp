using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
namespace WinFormsApp1
{
    partial class ChatRoom
    {


    

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBox1 = new TextBox();
            richTextBox1 = new RichTextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.FromArgb(64, 64, 64);
            textBox1.ForeColor = SystemColors.Window;
            textBox1.Location = new Point(613, 797);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(688, 39);
            textBox1.TabIndex = 0;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += CheckEnterKeyPress;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = Color.FromArgb(32, 32, 32);
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Font = new Font("Segoe UI", 10.875F, FontStyle.Regular, GraphicsUnit.Point, 0);
            richTextBox1.ForeColor = SystemColors.Window;
            richTextBox1.Location = new Point(613, 216);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(688, 456);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 22.125F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.Window;
            label1.Location = new Point(664, 64);
            label1.Name = "label1";
            label1.Size = new Size(587, 78);
            label1.TabIndex = 3;
            label1.Text = "At the French Bakery";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(32, 32, 32);
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1915, 977);
            Controls.Add(label1);
            Controls.Add(richTextBox1);
            Controls.Add(textBox1);
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private RichTextBox richTextBox1;
        private Label label1;
    }
}