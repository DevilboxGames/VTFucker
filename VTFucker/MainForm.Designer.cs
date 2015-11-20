namespace VTFucker
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TextureList = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.TilePreviewBox = new System.Windows.Forms.PictureBox();
            this.TileListBox = new System.Windows.Forms.ComboBox();
            this.NumTiles = new System.Windows.Forms.Label();
            this.NumTilesLabel = new System.Windows.Forms.Label();
            this.TextureDimensions = new System.Windows.Forms.Label();
            this.TextureDimensionsLabel = new System.Windows.Forms.Label();
            this.TextureName = new System.Windows.Forms.Label();
            this.TextureNameLabel = new System.Windows.Forms.Label();
            this.PageNumLabel = new System.Windows.Forms.Label();
            this.PageNumSelect = new System.Windows.Forms.ComboBox();
            this.PageType = new System.Windows.Forms.ComboBox();
            this.PageTypeLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TilePreviewBox)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 45);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.TextureList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(880, 484);
            this.splitContainer1.SplitterDistance = 265;
            this.splitContainer1.TabIndex = 0;
            // 
            // TextureList
            // 
            this.TextureList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextureList.FormattingEnabled = true;
            this.TextureList.Location = new System.Drawing.Point(0, 0);
            this.TextureList.Name = "TextureList";
            this.TextureList.Size = new System.Drawing.Size(265, 484);
            this.TextureList.TabIndex = 0;
            this.TextureList.SelectedIndexChanged += new System.EventHandler(this.TextureList_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.TilePreviewBox);
            this.splitContainer2.Panel2.Controls.Add(this.TileListBox);
            this.splitContainer2.Panel2.Controls.Add(this.NumTiles);
            this.splitContainer2.Panel2.Controls.Add(this.NumTilesLabel);
            this.splitContainer2.Panel2.Controls.Add(this.TextureDimensions);
            this.splitContainer2.Panel2.Controls.Add(this.TextureDimensionsLabel);
            this.splitContainer2.Panel2.Controls.Add(this.TextureName);
            this.splitContainer2.Panel2.Controls.Add(this.TextureNameLabel);
            this.splitContainer2.Size = new System.Drawing.Size(611, 484);
            this.splitContainer2.SplitterDistance = 432;
            this.splitContainer2.TabIndex = 0;
            // 
            // TilePreviewBox
            // 
            this.TilePreviewBox.Location = new System.Drawing.Point(21, 120);
            this.TilePreviewBox.Name = "TilePreviewBox";
            this.TilePreviewBox.Size = new System.Drawing.Size(128, 128);
            this.TilePreviewBox.TabIndex = 7;
            this.TilePreviewBox.TabStop = false;
            // 
            // TileListBox
            // 
            this.TileListBox.FormattingEnabled = true;
            this.TileListBox.Location = new System.Drawing.Point(6, 93);
            this.TileListBox.Name = "TileListBox";
            this.TileListBox.Size = new System.Drawing.Size(166, 21);
            this.TileListBox.TabIndex = 6;
            this.TileListBox.SelectedIndexChanged += new System.EventHandler(this.TileListBox_SelectedIndexChanged);
            // 
            // NumTiles
            // 
            this.NumTiles.AutoSize = true;
            this.NumTiles.Location = new System.Drawing.Point(92, 62);
            this.NumTiles.Name = "NumTiles";
            this.NumTiles.Size = new System.Drawing.Size(57, 13);
            this.NumTiles.TabIndex = 5;
            this.NumTiles.Text = "[NumTiles]";
            // 
            // NumTilesLabel
            // 
            this.NumTilesLabel.AutoSize = true;
            this.NumTilesLabel.Location = new System.Drawing.Point(3, 62);
            this.NumTilesLabel.Name = "NumTilesLabel";
            this.NumTilesLabel.Size = new System.Drawing.Size(86, 13);
            this.NumTilesLabel.TabIndex = 4;
            this.NumTilesLabel.Text = "Number Of Tiles:";
            // 
            // TextureDimensions
            // 
            this.TextureDimensions.AutoSize = true;
            this.TextureDimensions.Location = new System.Drawing.Point(70, 35);
            this.TextureDimensions.Name = "TextureDimensions";
            this.TextureDimensions.Size = new System.Drawing.Size(103, 13);
            this.TextureDimensions.TabIndex = 3;
            this.TextureDimensions.Text = "[TextureDimensions]";
            // 
            // TextureDimensionsLabel
            // 
            this.TextureDimensionsLabel.AutoSize = true;
            this.TextureDimensionsLabel.Location = new System.Drawing.Point(3, 35);
            this.TextureDimensionsLabel.Name = "TextureDimensionsLabel";
            this.TextureDimensionsLabel.Size = new System.Drawing.Size(64, 13);
            this.TextureDimensionsLabel.TabIndex = 2;
            this.TextureDimensionsLabel.Text = "Dimensions:";
            // 
            // TextureName
            // 
            this.TextureName.AutoSize = true;
            this.TextureName.Location = new System.Drawing.Point(55, 9);
            this.TextureName.Name = "TextureName";
            this.TextureName.Size = new System.Drawing.Size(77, 13);
            this.TextureName.TabIndex = 1;
            this.TextureName.Text = "[TextureName]";
            // 
            // TextureNameLabel
            // 
            this.TextureNameLabel.AutoSize = true;
            this.TextureNameLabel.Location = new System.Drawing.Point(3, 9);
            this.TextureNameLabel.Name = "TextureNameLabel";
            this.TextureNameLabel.Size = new System.Drawing.Size(46, 13);
            this.TextureNameLabel.TabIndex = 0;
            this.TextureNameLabel.Text = "Texture:";
            // 
            // PageNumLabel
            // 
            this.PageNumLabel.AutoSize = true;
            this.PageNumLabel.Location = new System.Drawing.Point(13, 13);
            this.PageNumLabel.Name = "PageNumLabel";
            this.PageNumLabel.Size = new System.Drawing.Size(60, 13);
            this.PageNumLabel.TabIndex = 1;
            this.PageNumLabel.Text = "Page Num:";
            // 
            // PageNumSelect
            // 
            this.PageNumSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PageNumSelect.FormattingEnabled = true;
            this.PageNumSelect.Location = new System.Drawing.Point(79, 10);
            this.PageNumSelect.Name = "PageNumSelect";
            this.PageNumSelect.Size = new System.Drawing.Size(121, 21);
            this.PageNumSelect.TabIndex = 2;
            this.PageNumSelect.SelectedIndexChanged += new System.EventHandler(this.PageNumSelect_SelectedIndexChanged);
            // 
            // PageType
            // 
            this.PageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PageType.FormattingEnabled = true;
            this.PageType.Items.AddRange(new object[] {
            "Diffuse",
            "Normal",
            "Specular"});
            this.PageType.Location = new System.Drawing.Point(303, 10);
            this.PageType.Name = "PageType";
            this.PageType.Size = new System.Drawing.Size(121, 21);
            this.PageType.TabIndex = 4;
            this.PageType.SelectedIndexChanged += new System.EventHandler(this.PageType_SelectedIndexChanged);
            // 
            // PageTypeLabel
            // 
            this.PageTypeLabel.AutoSize = true;
            this.PageTypeLabel.Location = new System.Drawing.Point(237, 13);
            this.PageTypeLabel.Name = "PageTypeLabel";
            this.PageTypeLabel.Size = new System.Drawing.Size(62, 13);
            this.PageTypeLabel.TabIndex = 3;
            this.PageTypeLabel.Text = "Page Type:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 529);
            this.Controls.Add(this.PageType);
            this.Controls.Add(this.PageTypeLabel);
            this.Controls.Add(this.PageNumSelect);
            this.Controls.Add(this.PageNumLabel);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Text = "VT Fucker";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TilePreviewBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox TextureList;
        public System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox TilePreviewBox;
        private System.Windows.Forms.ComboBox TileListBox;
        private System.Windows.Forms.Label NumTiles;
        private System.Windows.Forms.Label NumTilesLabel;
        private System.Windows.Forms.Label TextureDimensions;
        private System.Windows.Forms.Label TextureDimensionsLabel;
        private System.Windows.Forms.Label TextureName;
        private System.Windows.Forms.Label TextureNameLabel;
        public System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label PageNumLabel;
        private System.Windows.Forms.ComboBox PageNumSelect;
        private System.Windows.Forms.ComboBox PageType;
        private System.Windows.Forms.Label PageTypeLabel;
    }
}