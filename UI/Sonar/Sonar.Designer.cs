namespace Sonar
{
    partial class Sonar
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.discover_tabs = new System.Windows.Forms.TabControl();
            this.search = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._Track = new System.Windows.Forms.TextBox();
            this._Artist = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this._SearchResults = new System.Windows.Forms.ListBox();
            this.twitter = new System.Windows.Forms.TabPage();
            this._social = new SocialPanel();
            this.now_playing_tabs = new System.Windows.Forms.TabControl();
            this.now_playing = new System.Windows.Forms.TabPage();
            this.now_playing_panel = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.discover_tabs.SuspendLayout();
            this.search.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.twitter.SuspendLayout();
            this.now_playing_tabs.SuspendLayout();
            this.now_playing.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.discover_tabs);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.now_playing_tabs);
            this.splitContainer1.Size = new System.Drawing.Size(848, 436);
            this.splitContainer1.SplitterDistance = 282;
            this.splitContainer1.TabIndex = 0;
            // 
            // discover_tabs
            // 
            this.discover_tabs.Controls.Add(this.search);
            this.discover_tabs.Controls.Add(this.twitter);
            this.discover_tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.discover_tabs.Location = new System.Drawing.Point(0, 0);
            this.discover_tabs.Multiline = true;
            this.discover_tabs.Name = "discover_tabs";
            this.discover_tabs.SelectedIndex = 0;
            this.discover_tabs.Size = new System.Drawing.Size(282, 436);
            this.discover_tabs.TabIndex = 0;
            this.discover_tabs.Selected += new System.Windows.Forms.TabControlEventHandler(this.discover_tabs_Selected);
            // 
            // search
            // 
            this.search.Controls.Add(this.groupBox1);
            this.search.Controls.Add(this._SearchResults);
            this.search.Location = new System.Drawing.Point(4, 22);
            this.search.Name = "search";
            this.search.Padding = new System.Windows.Forms.Padding(3);
            this.search.Size = new System.Drawing.Size(274, 410);
            this.search.TabIndex = 0;
            this.search.Text = "search";
            this.search.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this._Track);
            this.groupBox1.Controls.Add(this._Artist);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(268, 79);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search Terms";
            // 
            // _Track
            // 
            this._Track.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._Track.Location = new System.Drawing.Point(57, 44);
            this._Track.Name = "_Track";
            this._Track.Size = new System.Drawing.Size(203, 20);
            this._Track.TabIndex = 3;
            this._Track.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._Track_KeyPress);
            // 
            // _Artist
            // 
            this._Artist.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._Artist.Location = new System.Drawing.Point(57, 18);
            this._Artist.Name = "_Artist";
            this._Artist.Size = new System.Drawing.Size(203, 20);
            this._Artist.TabIndex = 0;
            this._Artist.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._Artist_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Title:";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Artist:";
            // 
            // _SearchResults
            // 
            this._SearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._SearchResults.FormattingEnabled = true;
            this._SearchResults.Location = new System.Drawing.Point(3, 91);
            this._SearchResults.Name = "_SearchResults";
            this._SearchResults.Size = new System.Drawing.Size(268, 316);
            this._SearchResults.TabIndex = 0;
            // 
            // twitter
            // 
            this.twitter.Controls.Add(this._social);
            this.twitter.Location = new System.Drawing.Point(4, 22);
            this.twitter.Name = "twitter";
            this.twitter.Padding = new System.Windows.Forms.Padding(3);
            this.twitter.Size = new System.Drawing.Size(274, 410);
            this.twitter.TabIndex = 1;
            this.twitter.Text = "twitter";
            this.twitter.UseVisualStyleBackColor = true;
            // 
            // _social
            // 
            this._social.Dock = System.Windows.Forms.DockStyle.Fill;
            this._social.Location = new System.Drawing.Point(3, 3);
            this._social.Name = "_social";
            this._social.Size = new System.Drawing.Size(268, 404);
            this._social.TabIndex = 0;
            // 
            // now_playing_tabs
            // 
            this.now_playing_tabs.Controls.Add(this.now_playing);
            this.now_playing_tabs.Controls.Add(this.tabPage3);
            this.now_playing_tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.now_playing_tabs.Location = new System.Drawing.Point(0, 0);
            this.now_playing_tabs.Name = "now_playing_tabs";
            this.now_playing_tabs.SelectedIndex = 0;
            this.now_playing_tabs.Size = new System.Drawing.Size(562, 436);
            this.now_playing_tabs.TabIndex = 0;
            // 
            // now_playing
            // 
            this.now_playing.Controls.Add(this.now_playing_panel);
            this.now_playing.Location = new System.Drawing.Point(4, 22);
            this.now_playing.Name = "now_playing";
            this.now_playing.Padding = new System.Windows.Forms.Padding(3);
            this.now_playing.Size = new System.Drawing.Size(554, 410);
            this.now_playing.TabIndex = 0;
            this.now_playing.Text = "tabPage1";
            this.now_playing.UseVisualStyleBackColor = true;
            // 
            // now_playing_panel
            // 
            this.now_playing_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.now_playing_panel.Location = new System.Drawing.Point(3, 3);
            this.now_playing_panel.Name = "now_playing_panel";
            this.now_playing_panel.Size = new System.Drawing.Size(548, 404);
            this.now_playing_panel.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(554, 410);
            this.tabPage3.TabIndex = 1;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // Sonar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 436);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Sonar";
            this.Text = "Sonar";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.discover_tabs.ResumeLayout(false);
            this.search.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.twitter.ResumeLayout(false);
            this.now_playing_tabs.ResumeLayout(false);
            this.now_playing.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl discover_tabs;
        private System.Windows.Forms.TabPage search;
        private System.Windows.Forms.TabPage twitter;
        private System.Windows.Forms.TabControl now_playing_tabs;
        private System.Windows.Forms.TabPage now_playing;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel now_playing_panel;
        private SocialPanel _social;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox _Track;
        private System.Windows.Forms.TextBox _Artist;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox _SearchResults;
    }
}

