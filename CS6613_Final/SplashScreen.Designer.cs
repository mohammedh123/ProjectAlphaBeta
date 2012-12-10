namespace CS6613_Final
{
    partial class SplashScreen
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
            this.pvpRadio = new System.Windows.Forms.RadioButton();
            this.pvcRadio = new System.Windows.Forms.RadioButton();
            this.cvcRadio = new System.Windows.Forms.RadioButton();
            this.cOneComboBox = new System.Windows.Forms.ComboBox();
            this.cTwoComboBox = new System.Windows.Forms.ComboBox();
            this.cOneLabel = new System.Windows.Forms.Label();
            this.cTwoLabel = new System.Windows.Forms.Label();
            this.playButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pvpRadio
            // 
            this.pvpRadio.AutoSize = true;
            this.pvpRadio.Checked = true;
            this.pvpRadio.Location = new System.Drawing.Point(20, 20);
            this.pvpRadio.Name = "pvpRadio";
            this.pvpRadio.Size = new System.Drawing.Size(103, 17);
            this.pvpRadio.TabIndex = 0;
            this.pvpRadio.TabStop = true;
            this.pvpRadio.Text = "Player vs. Player";
            this.pvpRadio.UseVisualStyleBackColor = true;
            this.pvpRadio.CheckedChanged += new System.EventHandler(this.pvpButton_CheckedChanged);
            // 
            // pvcRadio
            // 
            this.pvcRadio.AutoSize = true;
            this.pvcRadio.Location = new System.Drawing.Point(20, 45);
            this.pvcRadio.Name = "pvcRadio";
            this.pvcRadio.Size = new System.Drawing.Size(119, 17);
            this.pvcRadio.TabIndex = 1;
            this.pvcRadio.Text = "Player vs. Computer";
            this.pvcRadio.UseVisualStyleBackColor = true;
            this.pvcRadio.CheckedChanged += new System.EventHandler(this.pvcButton_CheckedChanged);
            // 
            // cvcRadio
            // 
            this.cvcRadio.AutoSize = true;
            this.cvcRadio.Location = new System.Drawing.Point(20, 70);
            this.cvcRadio.Name = "cvcRadio";
            this.cvcRadio.Size = new System.Drawing.Size(132, 17);
            this.cvcRadio.TabIndex = 2;
            this.cvcRadio.TabStop = true;
            this.cvcRadio.Text = "Computer vs Computer";
            this.cvcRadio.UseVisualStyleBackColor = true;
            this.cvcRadio.CheckedChanged += new System.EventHandler(this.cvcRadio_CheckedChanged);
            // 
            // cOneComboBox
            // 
            this.cOneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cOneComboBox.Enabled = false;
            this.cOneComboBox.FormattingEnabled = true;
            this.cOneComboBox.Location = new System.Drawing.Point(20, 125);
            this.cOneComboBox.Name = "cOneComboBox";
            this.cOneComboBox.Size = new System.Drawing.Size(120, 21);
            this.cOneComboBox.TabIndex = 3;
            // 
            // cTwoComboBox
            // 
            this.cTwoComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cTwoComboBox.Enabled = false;
            this.cTwoComboBox.FormattingEnabled = true;
            this.cTwoComboBox.Location = new System.Drawing.Point(160, 125);
            this.cTwoComboBox.Name = "cTwoComboBox";
            this.cTwoComboBox.Size = new System.Drawing.Size(120, 21);
            this.cTwoComboBox.TabIndex = 4;
            // 
            // cOneLabel
            // 
            this.cOneLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cOneLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cOneLabel.Location = new System.Drawing.Point(20, 105);
            this.cOneLabel.Name = "cOneLabel";
            this.cOneLabel.Size = new System.Drawing.Size(120, 13);
            this.cOneLabel.TabIndex = 5;
            this.cOneLabel.Text = "Computer One";
            this.cOneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cTwoLabel
            // 
            this.cTwoLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cTwoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cTwoLabel.Location = new System.Drawing.Point(160, 105);
            this.cTwoLabel.Name = "cTwoLabel";
            this.cTwoLabel.Size = new System.Drawing.Size(120, 13);
            this.cTwoLabel.TabIndex = 6;
            this.cTwoLabel.Text = "Computer Two";
            this.cTwoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // playButton
            // 
            this.playButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.playButton.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playButton.Location = new System.Drawing.Point(160, 20);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(120, 67);
            this.playButton.TabIndex = 7;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // SplashScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Snow;
            this.ClientSize = new System.Drawing.Size(300, 209);
            this.ControlBox = false;
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.cTwoLabel);
            this.Controls.Add(this.cOneLabel);
            this.Controls.Add(this.cTwoComboBox);
            this.Controls.Add(this.cOneComboBox);
            this.Controls.Add(this.cvcRadio);
            this.Controls.Add(this.pvcRadio);
            this.Controls.Add(this.pvpRadio);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashScreen";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SplashScreen_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton pvpRadio;
        private System.Windows.Forms.RadioButton pvcRadio;
        private System.Windows.Forms.RadioButton cvcRadio;
        private System.Windows.Forms.ComboBox cOneComboBox;
        private System.Windows.Forms.ComboBox cTwoComboBox;
        private System.Windows.Forms.Label cOneLabel;
        private System.Windows.Forms.Label cTwoLabel;
        private System.Windows.Forms.Button playButton;
    }
}