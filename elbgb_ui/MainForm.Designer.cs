namespace elbgb_ui
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
			this.mainFormMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.regenerateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.displayPanel = new Components.DoubleBufferedPanel();
			this.mainFormMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainFormMenuStrip
			// 
			this.mainFormMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.mainFormMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainFormMenuStrip.Name = "mainFormMenuStrip";
			this.mainFormMenuStrip.Size = new System.Drawing.Size(172, 24);
			this.mainFormMenuStrip.TabIndex = 0;
			this.mainFormMenuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.regenerateToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// regenerateToolStripMenuItem
			// 
			this.regenerateToolStripMenuItem.Name = "regenerateToolStripMenuItem";
			this.regenerateToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.regenerateToolStripMenuItem.Text = "Regenerate";
			this.regenerateToolStripMenuItem.Click += new System.EventHandler(this.regenerateToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// displayPanel
			// 
			this.displayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.displayPanel.Location = new System.Drawing.Point(0, 24);
			this.displayPanel.Name = "displayPanel";
			this.displayPanel.Size = new System.Drawing.Size(172, 168);
			this.displayPanel.TabIndex = 2;
			this.displayPanel.Click += new System.EventHandler(this.displayPanel_Click);
			this.displayPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.displayPanel_Paint);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(172, 192);
			this.Controls.Add(this.displayPanel);
			this.Controls.Add(this.mainFormMenuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.mainFormMenuStrip;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Text = "elbgb";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.mainFormMenuStrip.ResumeLayout(false);
			this.mainFormMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip mainFormMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private Components.DoubleBufferedPanel displayPanel;
		private System.Windows.Forms.ToolStripMenuItem regenerateToolStripMenuItem;
	}
}

