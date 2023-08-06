namespace Client
{
    partial class LoginForm
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
            label1 = new Label();
            label2 = new Label();
            tbx_id = new TextBox();
            tbx_nickname = new TextBox();
            btn_login = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(36, 33);
            label1.Name = "label1";
            label1.Size = new Size(24, 20);
            label1.TabIndex = 0;
            label1.Text = "ID";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(12, 83);
            label2.Name = "label2";
            label2.Size = new Size(54, 20);
            label2.TabIndex = 1;
            label2.Text = "닉네임";
            // 
            // tbx_id
            // 
            tbx_id.Location = new Point(82, 34);
            tbx_id.Name = "tbx_id";
            tbx_id.Size = new Size(109, 23);
            tbx_id.TabIndex = 2;
            // 
            // tbx_nickname
            // 
            tbx_nickname.Location = new Point(82, 84);
            tbx_nickname.Name = "tbx_nickname";
            tbx_nickname.Size = new Size(109, 23);
            tbx_nickname.TabIndex = 3;
            // 
            // btn_login
            // 
            btn_login.Location = new Point(36, 138);
            btn_login.Name = "btn_login";
            btn_login.Size = new Size(155, 23);
            btn_login.TabIndex = 4;
            btn_login.Text = "로그인";
            btn_login.UseVisualStyleBackColor = true;
            btn_login.Click += btn_login_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(225, 184);
            Controls.Add(btn_login);
            Controls.Add(tbx_nickname);
            Controls.Add(tbx_id);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "아이디생성";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox tbx_id;
        private TextBox tbx_nickname;
        private Button btn_login;
    }
}