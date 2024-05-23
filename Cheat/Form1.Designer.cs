namespace Cheat
{
    partial class Form1
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
            textBox1 = new System.Windows.Forms.TextBox();
            textBox2 = new System.Windows.Forms.TextBox();
            lblCopyIndicator = new System.Windows.Forms.Label();
            picCopy = new System.Windows.Forms.PictureBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            listBox1 = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)picCopy).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox1.ForeColor = System.Drawing.Color.White;
            textBox1.Location = new System.Drawing.Point(49, 11);
            textBox1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(642, 22);
            textBox1.TabIndex = 0;
            textBox1.Text = "Start Typing...";
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += textBox1_KeyDown;
            textBox1.MouseDown += textBox1_MouseDown;
            textBox1.MouseMove += textBox1_MouseMove;
            textBox1.MouseUp += textBox1_MouseUp;
            // 
            // textBox2
            // 
            textBox2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            textBox2.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox2.Font = new System.Drawing.Font("Cascadia Mono", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox2.ForeColor = System.Drawing.Color.White;
            textBox2.Location = new System.Drawing.Point(14, 51);
            textBox2.Margin = new System.Windows.Forms.Padding(4);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new System.Drawing.Size(706, 307);
            textBox2.TabIndex = 1;
            textBox2.TabStop = false;
            textBox2.KeyDown += textBox2_KeyDown;
            // 
            // lblCopyIndicator
            // 
            lblCopyIndicator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblCopyIndicator.AutoSize = true;
            lblCopyIndicator.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lblCopyIndicator.ForeColor = System.Drawing.Color.WhiteSmoke;
            lblCopyIndicator.Location = new System.Drawing.Point(705, 11);
            lblCopyIndicator.Name = "lblCopyIndicator";
            lblCopyIndicator.Size = new System.Drawing.Size(0, 21);
            lblCopyIndicator.TabIndex = 3;
            // 
            // picCopy
            // 
            picCopy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            picCopy.Image = Properties.Resources.copyto_greyscale;
            picCopy.Location = new System.Drawing.Point(698, 8);
            picCopy.Name = "picCopy";
            picCopy.Size = new System.Drawing.Size(24, 24);
            picCopy.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            picCopy.TabIndex = 4;
            picCopy.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.mag;
            pictureBox1.Location = new System.Drawing.Point(14, 11);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(26, 29);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // listBox1
            // 
            listBox1.BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            listBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            listBox1.ForeColor = System.Drawing.Color.White;
            listBox1.FormattingEnabled = true;
            listBox1.IntegralHeight = false;
            listBox1.ItemHeight = 55;
            listBox1.Location = new System.Drawing.Point(5, 43);
            listBox1.Name = "listBox1";
            listBox1.Size = new System.Drawing.Size(599, 84);
            listBox1.TabIndex = 5;
            listBox1.DrawItem += listBox1_DrawItem;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            listBox1.KeyDown += listBox1_KeyDown;
            listBox1.Leave += listBox1_Leave;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            ClientSize = new System.Drawing.Size(730, 371);
            Controls.Add(listBox1);
            Controls.Add(picCopy);
            Controls.Add(lblCopyIndicator);
            Controls.Add(pictureBox1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            Name = "Form1";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Cheat";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            SizeChanged += Form1_SizeChanged;
            Paint += Form1_Paint;
            ((System.ComponentModel.ISupportInitialize)picCopy).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblCopyIndicator;
        private System.Windows.Forms.PictureBox picCopy;
        private System.Windows.Forms.ListBox listBox1;
        
    }
}

