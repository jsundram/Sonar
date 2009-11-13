namespace Sonar
{
    partial class SocialPanel
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
            this._Feed = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // _Feed
            // 
            this._Feed.Alignment = System.Windows.Forms.ListViewAlignment.Default;
            this._Feed.Dock = System.Windows.Forms.DockStyle.Fill;
            this._Feed.LabelWrap = false;
            this._Feed.Location = new System.Drawing.Point(0, 0);
            this._Feed.Name = "_Feed";
            this._Feed.ShowItemToolTips = true;
            this._Feed.Size = new System.Drawing.Size(287, 445);
            this._Feed.TabIndex = 0;
            this._Feed.UseCompatibleStateImageBehavior = false;
            this._Feed.View = System.Windows.Forms.View.List;
            // 
            // SocialPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._Feed);
            this.Name = "SocialPanel";
            this.Size = new System.Drawing.Size(287, 445);
            this.Leave += new System.EventHandler(this.SocialPanel_Leave);
            this.Enter += new System.EventHandler(this.SocialPanel_Enter);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView _Feed;

    }
}
