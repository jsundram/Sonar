namespace Sonar
{
    partial class ArtistInspector
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
            this._Art = new System.Windows.Forms.PictureBox();
            this._Artist = new System.Windows.Forms.Label();
            this._LastFmInfo = new System.Windows.Forms.TextBox();
            this._ArtistInfo = new System.Windows.Forms.TextBox();
            this._Familiarity = new Sonar.VerticalProgressBar();
            this._Hotness = new Sonar.VerticalProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this._Art)).BeginInit();
            this.SuspendLayout();
            // 
            // _Art
            // 
            this._Art.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._Art.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this._Art.Location = new System.Drawing.Point(0, 0);
            this._Art.Name = "_Art";
            this._Art.Size = new System.Drawing.Size(294, 250);
            this._Art.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._Art.TabIndex = 0;
            this._Art.TabStop = false;
            this._Art.DoubleClick += new System.EventHandler(this._Art_DoubleClick);
            // 
            // _Artist
            // 
            this._Artist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._Artist.AutoSize = true;
            this._Artist.Location = new System.Drawing.Point(89, 263);
            this._Artist.Name = "_Artist";
            this._Artist.Size = new System.Drawing.Size(61, 13);
            this._Artist.TabIndex = 1;
            this._Artist.Text = "Artist Name";
            // 
            // _LastFmInfo
            // 
            this._LastFmInfo.BackColor = System.Drawing.Color.Black;
            this._LastFmInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._LastFmInfo.Dock = System.Windows.Forms.DockStyle.Right;
            this._LastFmInfo.ForeColor = System.Drawing.Color.White;
            this._LastFmInfo.Location = new System.Drawing.Point(379, 0);
            this._LastFmInfo.Multiline = true;
            this._LastFmInfo.Name = "_LastFmInfo";
            this._LastFmInfo.ReadOnly = true;
            this._LastFmInfo.Size = new System.Drawing.Size(243, 380);
            this._LastFmInfo.TabIndex = 5;
            // 
            // _ArtistInfo
            // 
            this._ArtistInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._ArtistInfo.BackColor = System.Drawing.Color.Black;
            this._ArtistInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._ArtistInfo.ForeColor = System.Drawing.Color.White;
            this._ArtistInfo.Location = new System.Drawing.Point(0, 295);
            this._ArtistInfo.Multiline = true;
            this._ArtistInfo.Name = "_ArtistInfo";
            this._ArtistInfo.ReadOnly = true;
            this._ArtistInfo.Size = new System.Drawing.Size(363, 85);
            this._ArtistInfo.TabIndex = 6;
            // 
            // _Familiarity
            // 
            this._Familiarity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._Familiarity.ForeColor = System.Drawing.SystemColors.HotTrack;
            this._Familiarity.Location = new System.Drawing.Point(314, 0);
            this._Familiarity.Name = "_Familiarity";
            this._Familiarity.Size = new System.Drawing.Size(17, 250);
            this._Familiarity.TabIndex = 3;
            // 
            // _Hotness
            // 
            this._Hotness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._Hotness.ForeColor = System.Drawing.Color.Orange;
            this._Hotness.Location = new System.Drawing.Point(346, 0);
            this._Hotness.Name = "_Hotness";
            this._Hotness.Size = new System.Drawing.Size(17, 250);
            this._Hotness.TabIndex = 2;
            // 
            // ArtistInspector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(622, 380);
            this.Controls.Add(this._ArtistInfo);
            this.Controls.Add(this._LastFmInfo);
            this.Controls.Add(this._Familiarity);
            this.Controls.Add(this._Hotness);
            this.Controls.Add(this._Artist);
            this.Controls.Add(this._Art);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ArtistInspector";
            this.Text = "Artist Inspector";
            this.Load += new System.EventHandler(this.ArtistInspector_Load);
            ((System.ComponentModel.ISupportInitialize)(this._Art)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox _Art;
        private System.Windows.Forms.Label _Artist;
        private VerticalProgressBar _Hotness;
        private VerticalProgressBar _Familiarity;
        private System.Windows.Forms.TextBox _LastFmInfo;
        private System.Windows.Forms.TextBox _ArtistInfo;
    }
}