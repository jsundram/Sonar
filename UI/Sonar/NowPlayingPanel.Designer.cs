namespace Sonar
{
    partial class NowPlayingPanel
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
            this._AlbumArt = new System.Windows.Forms.PictureBox();
            this._Queue = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._TrackProgress = new System.Windows.Forms.ProgressBar();
            this._TrackTime = new System.Windows.Forms.Label();
            this._Volume = new System.Windows.Forms.TrackBar();
            this._Mute = new System.Windows.Forms.Button();
            this._Play = new System.Windows.Forms.Button();
            this._Back = new System.Windows.Forms.Button();
            this._Next = new System.Windows.Forms.Button();
            this._Artist = new System.Windows.Forms.LinkLabel();
            this._Album = new System.Windows.Forms.Label();
            this._Track = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._AlbumArt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._Volume)).BeginInit();
            this.SuspendLayout();
            // 
            // _AlbumArt
            // 
            this._AlbumArt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._AlbumArt.BackColor = System.Drawing.Color.Transparent;
            this._AlbumArt.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._AlbumArt.Location = new System.Drawing.Point(3, 3);
            this._AlbumArt.Name = "_AlbumArt";
            this._AlbumArt.Size = new System.Drawing.Size(403, 311);
            this._AlbumArt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._AlbumArt.TabIndex = 1;
            this._AlbumArt.TabStop = false;
            this._AlbumArt.DoubleClick += new System.EventHandler(this._AlbumArt_DoubleClick);
            // 
            // _Queue
            // 
            this._Queue.BackColor = System.Drawing.Color.Black;
            this._Queue.Dock = System.Windows.Forms.DockStyle.Right;
            this._Queue.ForeColor = System.Drawing.Color.White;
            this._Queue.FormattingEnabled = true;
            this._Queue.Location = new System.Drawing.Point(412, 0);
            this._Queue.Name = "_Queue";
            this._Queue.Size = new System.Drawing.Size(259, 524);
            this._Queue.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 428);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Artist:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 460);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Album:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 492);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Track:";
            // 
            // _TrackProgress
            // 
            this._TrackProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._TrackProgress.Location = new System.Drawing.Point(3, 320);
            this._TrackProgress.Name = "_TrackProgress";
            this._TrackProgress.Size = new System.Drawing.Size(403, 10);
            this._TrackProgress.TabIndex = 6;
            // 
            // _TrackTime
            // 
            this._TrackTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._TrackTime.AutoSize = true;
            this._TrackTime.Location = new System.Drawing.Point(334, 337);
            this._TrackTime.Name = "_TrackTime";
            this._TrackTime.Size = new System.Drawing.Size(72, 13);
            this._TrackTime.TabIndex = 7;
            this._TrackTime.Text = "00:00 / 00:00";
            // 
            // _Volume
            // 
            this._Volume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._Volume.Location = new System.Drawing.Point(361, 369);
            this._Volume.Maximum = 100;
            this._Volume.Name = "_Volume";
            this._Volume.Orientation = System.Windows.Forms.Orientation.Vertical;
            this._Volume.Size = new System.Drawing.Size(45, 121);
            this._Volume.TabIndex = 8;
            this._Volume.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // _Mute
            // 
            this._Mute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._Mute.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this._Mute.Location = new System.Drawing.Point(361, 492);
            this._Mute.Name = "_Mute";
            this._Mute.Size = new System.Drawing.Size(45, 23);
            this._Mute.TabIndex = 9;
            this._Mute.Text = "Mute";
            this._Mute.UseVisualStyleBackColor = false;
            // 
            // _Play
            // 
            this._Play.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Play.BackColor = System.Drawing.Color.DimGray;
            this._Play.Location = new System.Drawing.Point(95, 337);
            this._Play.Name = "_Play";
            this._Play.Size = new System.Drawing.Size(75, 22);
            this._Play.TabIndex = 10;
            this._Play.Text = "Play";
            this._Play.UseVisualStyleBackColor = false;
            // 
            // _Back
            // 
            this._Back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Back.BackColor = System.Drawing.Color.DimGray;
            this._Back.Location = new System.Drawing.Point(14, 337);
            this._Back.Name = "_Back";
            this._Back.Size = new System.Drawing.Size(75, 22);
            this._Back.TabIndex = 11;
            this._Back.Text = "Back";
            this._Back.UseVisualStyleBackColor = false;
            // 
            // _Next
            // 
            this._Next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Next.BackColor = System.Drawing.Color.DimGray;
            this._Next.Location = new System.Drawing.Point(176, 337);
            this._Next.Name = "_Next";
            this._Next.Size = new System.Drawing.Size(75, 22);
            this._Next.TabIndex = 12;
            this._Next.Text = "Next";
            this._Next.UseVisualStyleBackColor = false;
            // 
            // _Artist
            // 
            this._Artist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Artist.AutoSize = true;
            this._Artist.LinkColor = System.Drawing.Color.White;
            this._Artist.Location = new System.Drawing.Point(64, 428);
            this._Artist.Name = "_Artist";
            this._Artist.Size = new System.Drawing.Size(187, 13);
            this._Artist.TabIndex = 13;
            this._Artist.TabStop = true;
            this._Artist.Text = "                                                            ";
            this._Artist.VisitedLinkColor = System.Drawing.Color.White;
            this._Artist.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._Artist_LinkClicked);
            // 
            // _Album
            // 
            this._Album.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Album.AutoSize = true;
            this._Album.Location = new System.Drawing.Point(64, 460);
            this._Album.Name = "_Album";
            this._Album.Size = new System.Drawing.Size(0, 13);
            this._Album.TabIndex = 14;
            // 
            // _Track
            // 
            this._Track.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Track.AutoSize = true;
            this._Track.Location = new System.Drawing.Point(64, 492);
            this._Track.Name = "_Track";
            this._Track.Size = new System.Drawing.Size(0, 13);
            this._Track.TabIndex = 15;
            // 
            // NowPlayingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this._Track);
            this.Controls.Add(this._Album);
            this.Controls.Add(this._Artist);
            this.Controls.Add(this._Next);
            this.Controls.Add(this._Back);
            this.Controls.Add(this._Play);
            this.Controls.Add(this._Mute);
            this.Controls.Add(this._Volume);
            this.Controls.Add(this._TrackTime);
            this.Controls.Add(this._TrackProgress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._Queue);
            this.Controls.Add(this._AlbumArt);
            this.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.Name = "NowPlayingPanel";
            this.Size = new System.Drawing.Size(671, 525);
            ((System.ComponentModel.ISupportInitialize)(this._AlbumArt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._Volume)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox _AlbumArt;
        private System.Windows.Forms.ListBox _Queue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar _TrackProgress;
        private System.Windows.Forms.Label _TrackTime;
        private System.Windows.Forms.TrackBar _Volume;
        private System.Windows.Forms.Button _Mute;
        private System.Windows.Forms.Button _Play;
        private System.Windows.Forms.Button _Back;
        private System.Windows.Forms.Button _Next;
        private System.Windows.Forms.LinkLabel _Artist;
        private System.Windows.Forms.Label _Album;
        private System.Windows.Forms.Label _Track;
    }
}
