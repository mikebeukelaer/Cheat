using System.Drawing;

namespace Cheat
{
    partial class CustomListBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // CustomListBox
            // 
            DoubleBuffered = true;
            Name = "CustomListBox";
            Size = new Size(148, 148);
            Load += CustomListBox_Load;
            Scroll += CustomListBox_Scroll;
            SizeChanged += CustomListBox_SizeChanged;
            VisibleChanged += CustomListBox_VisibleChanged;
            Paint += CustomListBox_Paint;
            KeyDown += CustomListBox_KeyDown;
            KeyPress += CustomListBox_KeyPress;
            MouseDown += CustomListBox_MouseDown;
            PreviewKeyDown += CustomListBox_PreviewKeyDown;
            ResumeLayout(false);
        }

        #endregion
    }
}
