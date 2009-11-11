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
            // 
            // search
            // 
            this.search.Location = new System.Drawing.Point(4, 22);
            this.search.Name = "search";
            this.search.Padding = new System.Windows.Forms.Padding(3);
            this.search.Size = new System.Drawing.Size(274, 410);
            this.search.TabIndex = 0;
            this.search.Text = "search";
            this.search.UseVisualStyleBackColor = true;
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
            this.twitter.Enter += new System.EventHandler(this.twitter_Enter);
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
        private global::Sonar.SocialPanel _social;
    }
}

