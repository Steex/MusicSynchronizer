namespace MusicSynchronizer
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
			this.textMusicSourceRoot = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textMusicTargetRoot = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textPlaylists = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.buttonCompare = new System.Windows.Forms.Button();
			this.textLog = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// textMusicSourceRoot
			// 
			this.textMusicSourceRoot.Location = new System.Drawing.Point(15, 25);
			this.textMusicSourceRoot.Name = "textMusicSourceRoot";
			this.textMusicSourceRoot.Size = new System.Drawing.Size(495, 20);
			this.textMusicSourceRoot.TabIndex = 0;
			this.textMusicSourceRoot.Text = "E:\\Media\\Music";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Music source root folder";
			// 
			// textMusicTargetRoot
			// 
			this.textMusicTargetRoot.Location = new System.Drawing.Point(15, 70);
			this.textMusicTargetRoot.Name = "textMusicTargetRoot";
			this.textMusicTargetRoot.Size = new System.Drawing.Size(495, 20);
			this.textMusicTargetRoot.TabIndex = 0;
			this.textMusicTargetRoot.Text = "E:\\_Music phone";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(115, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Music target root folder";
			// 
			// textPlaylists
			// 
			this.textPlaylists.AllowDrop = true;
			this.textPlaylists.Location = new System.Drawing.Point(15, 115);
			this.textPlaylists.Multiline = true;
			this.textPlaylists.Name = "textPlaylists";
			this.textPlaylists.Size = new System.Drawing.Size(495, 90);
			this.textPlaylists.TabIndex = 0;
			this.textPlaylists.Text = "E:\\Media\\Playlists\\_______test1.m3u8\r\nE:\\Media\\Playlists\\_______test2.m3u";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 99);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Playlists";
			// 
			// buttonCompare
			// 
			this.buttonCompare.Location = new System.Drawing.Point(435, 252);
			this.buttonCompare.Name = "buttonCompare";
			this.buttonCompare.Size = new System.Drawing.Size(75, 23);
			this.buttonCompare.TabIndex = 2;
			this.buttonCompare.Text = "Compare";
			this.buttonCompare.UseVisualStyleBackColor = true;
			this.buttonCompare.Click += new System.EventHandler(this.buttonCompare_Click);
			// 
			// textLog
			// 
			this.textLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.textLog.Location = new System.Drawing.Point(12, 474);
			this.textLog.Multiline = true;
			this.textLog.Name = "textLog";
			this.textLog.ReadOnly = true;
			this.textLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textLog.Size = new System.Drawing.Size(782, 132);
			this.textLog.TabIndex = 0;
			this.textLog.WordWrap = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(806, 623);
			this.Controls.Add(this.buttonCompare);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textLog);
			this.Controls.Add(this.textPlaylists);
			this.Controls.Add(this.textMusicTargetRoot);
			this.Controls.Add(this.textMusicSourceRoot);
			this.Name = "MainForm";
			this.Text = "Music Synchronizer";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textMusicSourceRoot;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textMusicTargetRoot;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textPlaylists;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonCompare;
		private System.Windows.Forms.TextBox textLog;


	}
}

