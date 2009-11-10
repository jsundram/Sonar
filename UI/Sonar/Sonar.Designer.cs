namespace SonarGUI
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
            this.discovery = new System.Windows.Forms.TabPage();
            this.twitter = new System.Windows.Forms.TabPage();
            this.twitter_view = new System.Windows.Forms.ListView();
            this.now_playing_tabs = new System.Windows.Forms.TabControl();
            this.now_playing = new System.Windows.Forms.TabPage();
            this.now_playing_panel = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.sonos = new System.Windows.Forms.TabPage();
            this.last_fm = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.discover_tabs.SuspendLayout();
            this.twitter.SuspendLayout();
            this.now_playing_tabs.SuspendLayout();
            this.now_playing.SuspendLayout();
            this.sonos.SuspendLayout();
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
            this.discover_tabs.Controls.Add(this.discovery);
            this.discover_tabs.Controls.Add(this.twitter);
            this.discover_tabs.Controls.Add(this.sonos);
            this.discover_tabs.Controls.Add(this.last_fm);
            this.discover_tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.discover_tabs.Location = new System.Drawing.Point(0, 0);
            this.discover_tabs.Multiline = true;
            this.discover_tabs.Name = "discover_tabs";
            this.discover_tabs.SelectedIndex = 0;
            this.discover_tabs.Size = new System.Drawing.Size(282, 436);
            this.discover_tabs.TabIndex = 0;
            // 
            // discovery
            // 
            this.discovery.Location = new System.Drawing.Point(4, 22);
            this.discovery.Name = "discovery";
            this.discovery.Padding = new System.Windows.Forms.Padding(3);
            this.discovery.Size = new System.Drawing.Size(274, 410);
            this.discovery.TabIndex = 0;
            this.discovery.Text = "search";
            this.discovery.UseVisualStyleBackColor = true;
            // 
            // twitter
            // 
            this.twitter.Controls.Add(this.twitter_view);
            this.twitter.Location = new System.Drawing.Point(4, 22);
            this.twitter.Name = "twitter";
            this.twitter.Padding = new System.Windows.Forms.Padding(3);
            this.twitter.Size = new System.Drawing.Size(274, 410);
            this.twitter.TabIndex = 1;
            this.twitter.Text = "twitter";
            this.twitter.UseVisualStyleBackColor = true;
            this.twitter.Enter += new System.EventHandler(this.twitter_Enter);
            // 
            // twitter_view
            // 
            this.twitter_view.Dock = System.Windows.Forms.DockStyle.Fill;
            this.twitter_view.Location = new System.Drawing.Point(3, 3);
            this.twitter_view.Name = "twitter_view";
            this.twitter_view.ShowItemToolTips = true;
            this.twitter_view.Size = new System.Drawing.Size(268, 404);
            this.twitter_view.TabIndex = 0;
            this.twitter_view.UseCompatibleStateImageBehavior = false;
            this.twitter_view.View = System.Windows.Forms.View.List;
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
            // sonos
            // 
            this.sonos.Controls.Add(this.listView1);
            this.sonos.Location = new System.Drawing.Point(4, 22);
            this.sonos.Name = "sonos";
            this.sonos.Padding = new System.Windows.Forms.Padding(3);
            this.sonos.Size = new System.Drawing.Size(274, 410);
            this.sonos.TabIndex = 2;
            this.sonos.Text = "sonos";
            this.sonos.UseVisualStyleBackColor = true;
            this.sonos.Enter += new System.EventHandler(this.sonos_Enter);
            // 
            // last_fm
            // 
            this.last_fm.Location = new System.Drawing.Point(4, 22);
            this.last_fm.Name = "last_fm";
            this.last_fm.Padding = new System.Windows.Forms.Padding(3);
            this.last_fm.Size = new System.Drawing.Size(274, 410);
            this.last_fm.TabIndex = 3;
            this.last_fm.Text = "last.fm";
            this.last_fm.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(268, 404);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            // 
            // Sonar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 436);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Sonar";
            this.Text = "Sonar";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.discover_tabs.ResumeLayout(false);
            this.twitter.ResumeLayout(false);
            this.now_playing_tabs.ResumeLayout(false);
            this.now_playing.ResumeLayout(false);
            this.sonos.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl discover_tabs;
        private System.Windows.Forms.TabPage discovery;
        private System.Windows.Forms.TabPage twitter;
        private System.Windows.Forms.TabControl now_playing_tabs;
        private System.Windows.Forms.TabPage now_playing;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel now_playing_panel;
        private System.Windows.Forms.ListView twitter_view;
        private System.Windows.Forms.TabPage sonos;
        private System.Windows.Forms.TabPage last_fm;
        private System.Windows.Forms.ListView listView1;
    }
}

