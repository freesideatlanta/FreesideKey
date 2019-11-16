namespace CertGen
{
    partial class MainForm
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
            this.SaveCertButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SHA256Radio = new System.Windows.Forms.RadioButton();
            this.SHA384Radio = new System.Windows.Forms.RadioButton();
            this.SHA512Radio = new System.Windows.Forms.RadioButton();
            this.MD5Radio = new System.Windows.Forms.RadioButton();
            this.SHA1Radio = new System.Windows.Forms.RadioButton();
            this.CNTextBox = new System.Windows.Forms.TextBox();
            this.StartDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.EndDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PassTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PassConfirmTextBox = new System.Windows.Forms.MaskedTextBox();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // SaveCertButton
            // 
            this.SaveCertButton.Location = new System.Drawing.Point(369, 149);
            this.SaveCertButton.Name = "SaveCertButton";
            this.SaveCertButton.Size = new System.Drawing.Size(75, 23);
            this.SaveCertButton.TabIndex = 0;
            this.SaveCertButton.Text = "Save Certificate";
            this.SaveCertButton.UseVisualStyleBackColor = true;
            this.SaveCertButton.Click += new System.EventHandler(this.SaveCertButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(450, 149);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Common Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Hash Algorithm:";
            // 
            // SHA256Radio
            // 
            this.SHA256Radio.AutoSize = true;
            this.SHA256Radio.Location = new System.Drawing.Point(108, 12);
            this.SHA256Radio.Name = "SHA256Radio";
            this.SHA256Radio.Size = new System.Drawing.Size(68, 17);
            this.SHA256Radio.TabIndex = 5;
            this.SHA256Radio.TabStop = true;
            this.SHA256Radio.Text = "SHA 256";
            this.SHA256Radio.UseVisualStyleBackColor = true;
            // 
            // SHA384Radio
            // 
            this.SHA384Radio.AutoSize = true;
            this.SHA384Radio.Location = new System.Drawing.Point(204, 12);
            this.SHA384Radio.Name = "SHA384Radio";
            this.SHA384Radio.Size = new System.Drawing.Size(68, 17);
            this.SHA384Radio.TabIndex = 6;
            this.SHA384Radio.TabStop = true;
            this.SHA384Radio.Text = "SHA 384";
            this.SHA384Radio.UseVisualStyleBackColor = true;
            // 
            // SHA512Radio
            // 
            this.SHA512Radio.AutoSize = true;
            this.SHA512Radio.Location = new System.Drawing.Point(300, 12);
            this.SHA512Radio.Name = "SHA512Radio";
            this.SHA512Radio.Size = new System.Drawing.Size(68, 17);
            this.SHA512Radio.TabIndex = 7;
            this.SHA512Radio.TabStop = true;
            this.SHA512Radio.Text = "SHA 512";
            this.SHA512Radio.UseVisualStyleBackColor = true;
            // 
            // MD5Radio
            // 
            this.MD5Radio.AutoSize = true;
            this.MD5Radio.Location = new System.Drawing.Point(396, 12);
            this.MD5Radio.Name = "MD5Radio";
            this.MD5Radio.Size = new System.Drawing.Size(48, 17);
            this.MD5Radio.TabIndex = 8;
            this.MD5Radio.TabStop = true;
            this.MD5Radio.Text = "MD5";
            this.MD5Radio.UseVisualStyleBackColor = true;
            // 
            // SHA1Radio
            // 
            this.SHA1Radio.AutoSize = true;
            this.SHA1Radio.Location = new System.Drawing.Point(472, 12);
            this.SHA1Radio.Name = "SHA1Radio";
            this.SHA1Radio.Size = new System.Drawing.Size(53, 17);
            this.SHA1Radio.TabIndex = 9;
            this.SHA1Radio.TabStop = true;
            this.SHA1Radio.Text = "SHA1";
            this.SHA1Radio.UseVisualStyleBackColor = true;
            // 
            // CNTextBox
            // 
            this.CNTextBox.Location = new System.Drawing.Point(108, 44);
            this.CNTextBox.Name = "CNTextBox";
            this.CNTextBox.Size = new System.Drawing.Size(417, 20);
            this.CNTextBox.TabIndex = 10;
            // 
            // StartDateTimePicker
            // 
            this.StartDateTimePicker.Location = new System.Drawing.Point(108, 70);
            this.StartDateTimePicker.Name = "StartDateTimePicker";
            this.StartDateTimePicker.Size = new System.Drawing.Size(200, 20);
            this.StartDateTimePicker.TabIndex = 11;
            // 
            // EndDateTimePicker
            // 
            this.EndDateTimePicker.Location = new System.Drawing.Point(108, 98);
            this.EndDateTimePicker.Name = "EndDateTimePicker";
            this.EndDateTimePicker.Size = new System.Drawing.Size(200, 20);
            this.EndDateTimePicker.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Start Date:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "End Date:";
            // 
            // PassTextBox
            // 
            this.PassTextBox.Location = new System.Drawing.Point(108, 124);
            this.PassTextBox.Name = "PassTextBox";
            this.PassTextBox.PasswordChar = '*';
            this.PassTextBox.Size = new System.Drawing.Size(200, 20);
            this.PassTextBox.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Password:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 153);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Confirm Password:";
            // 
            // PassConfirmTextBox
            // 
            this.PassConfirmTextBox.Location = new System.Drawing.Point(108, 150);
            this.PassConfirmTextBox.Name = "PassConfirmTextBox";
            this.PassConfirmTextBox.PasswordChar = '*';
            this.PassConfirmTextBox.Size = new System.Drawing.Size(200, 20);
            this.PassConfirmTextBox.TabIndex = 17;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 184);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.PassConfirmTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.PassTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EndDateTimePicker);
            this.Controls.Add(this.StartDateTimePicker);
            this.Controls.Add(this.CNTextBox);
            this.Controls.Add(this.SHA1Radio);
            this.Controls.Add(this.MD5Radio);
            this.Controls.Add(this.SHA512Radio);
            this.Controls.Add(this.SHA384Radio);
            this.Controls.Add(this.SHA256Radio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.SaveCertButton);
            this.Name = "MainForm";
            this.Text = "Self Signed Certificate Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SaveCertButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton SHA256Radio;
        private System.Windows.Forms.RadioButton SHA384Radio;
        private System.Windows.Forms.RadioButton SHA512Radio;
        private System.Windows.Forms.RadioButton MD5Radio;
        private System.Windows.Forms.RadioButton SHA1Radio;
        private System.Windows.Forms.TextBox CNTextBox;
        private System.Windows.Forms.DateTimePicker StartDateTimePicker;
        private System.Windows.Forms.DateTimePicker EndDateTimePicker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.MaskedTextBox PassTextBox;
        private System.Windows.Forms.MaskedTextBox PassConfirmTextBox;
    }
}

