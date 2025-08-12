namespace ChatApp
{
    partial class Form1
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
            textMessages = new TextBox();
            textInput = new TextBox();
            btnSend = new Button();
            btnConnect = new Button();
            label1 = new Label();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // textMessages
            // 
            textMessages.Dock = DockStyle.Top;
            textMessages.Location = new Point(0, 0);
            textMessages.Multiline = true;
            textMessages.Name = "textMessages";
            textMessages.ReadOnly = true;
            textMessages.Size = new Size(800, 213);
            textMessages.TabIndex = 0;
            // 
            // textInput
            // 
            textInput.Location = new Point(108, 248);
            textInput.Name = "textInput";
            textInput.Size = new Size(514, 27);
            textInput.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(628, 246);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(94, 29);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(52, 335);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(165, 29);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(52, 255);
            label1.Name = "label1";
            label1.Size = new Size(43, 20);
            label1.TabIndex = 4;
            label1.Text = "Input";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(251, 339);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "label2";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblStatus);
            Controls.Add(label1);
            Controls.Add(btnConnect);
            Controls.Add(btnSend);
            Controls.Add(textInput);
            Controls.Add(textMessages);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textMessages;
        private TextBox textInput;
        private Button btnSend;
        private Button btnConnect;
        private Label label1;
        private Label lblStatus;
    }
}
