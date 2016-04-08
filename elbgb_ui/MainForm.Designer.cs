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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.regenerateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.displayPanel = new System.Windows.Forms.Panel();
			this.imageTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bppIndexedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bppARGBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.imageTypeToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(172, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
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
			// 
			// displayPanel
			// 
			this.displayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.displayPanel.Location = new System.Drawing.Point(0, 24);
			this.displayPanel.Name = "displayPanel";
			this.displayPanel.Size = new System.Drawing.Size(172, 168);
			this.displayPanel.TabIndex = 2;
			this.displayPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.displayPanel_Paint);
			// 
			// imageTypeToolStripMenuItem
			// 
			this.imageTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bppIndexedToolStripMenuItem,
            this.bppARGBToolStripMenuItem});
			this.imageTypeToolStripMenuItem.Name = "imageTypeToolStripMenuItem";
			this.imageTypeToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
			this.imageTypeToolStripMenuItem.Text = "ImageType";
			// 
			// bppIndexedToolStripMenuItem
			// 
			this.bppIndexedToolStripMenuItem.Name = "bppIndexedToolStripMenuItem";
			this.bppIndexedToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.bppIndexedToolStripMenuItem.Text = "4bpp Indexed";
			this.bppIndexedToolStripMenuItem.Click += new System.EventHandler(this.bppIndexedToolStripMenuItem_Click);
			// 
			// bppARGBToolStripMenuItem
			// 
			this.bppARGBToolStripMenuItem.Name = "bppARGBToolStripMenuItem";
			this.bppARGBToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.bppARGBToolStripMenuItem.Text = "32bpp ARGB";
			this.bppARGBToolStripMenuItem.Click += new System.EventHandler(this.bppARGBToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(172, 192);
			this.Controls.Add(this.displayPanel);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.Panel displayPanel;
		private System.Windows.Forms.ToolStripMenuItem regenerateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem imageTypeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bppIndexedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bppARGBToolStripMenuItem;
	}
}

