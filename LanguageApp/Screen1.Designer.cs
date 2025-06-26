namespace WinFormsApp1
{
    partial class Screen1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StartChatButton = new Button();
            SuspendLayout();
            // 
            // StartChatButton
            // 
            StartChatButton.Location = new Point(670, 324);
            StartChatButton.Name = "StartChatButton";
            StartChatButton.Size = new Size(555, 309);
            StartChatButton.TabIndex = 0;
            StartChatButton.Text = "Start Chat";
            StartChatButton.UseVisualStyleBackColor = true;
            StartChatButton.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1871, 986);
            Controls.Add(StartChatButton);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button StartChatButton;
    }
}
