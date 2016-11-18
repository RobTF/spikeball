namespace Platformer.DirectX
{
    partial class GameForm
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
            this.loadMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scale1xToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scale2xToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scale3xToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTracesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAABBsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEntityOriginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCollisionMapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTileFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.displayToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(368, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadMapToolStripMenuItem,
            this.stopGameToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadMapToolStripMenuItem
            // 
            this.loadMapToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.loadMapToolStripMenuItem.Name = "loadMapToolStripMenuItem";
            this.loadMapToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.loadMapToolStripMenuItem.Text = "Load Map...";
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.testToolStripMenuItem.Text = "test";
            // 
            // stopGameToolStripMenuItem
            // 
            this.stopGameToolStripMenuItem.Name = "stopGameToolStripMenuItem";
            this.stopGameToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.stopGameToolStripMenuItem.Text = "Stop Game";
            this.stopGameToolStripMenuItem.Click += new System.EventHandler(this.OnStopGameClicked);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.OnExitClicked);
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scale1xToolStripMenuItem,
            this.scale2xToolStripMenuItem,
            this.scale3xToolStripMenuItem});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.displayToolStripMenuItem.Text = "Display";
            // 
            // scale1xToolStripMenuItem
            // 
            this.scale1xToolStripMenuItem.Name = "scale1xToolStripMenuItem";
            this.scale1xToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.scale1xToolStripMenuItem.Text = "Scale 1x";
            this.scale1xToolStripMenuItem.Click += new System.EventHandler(this.OnScaleItemClicked);
            // 
            // scale2xToolStripMenuItem
            // 
            this.scale2xToolStripMenuItem.Name = "scale2xToolStripMenuItem";
            this.scale2xToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.scale2xToolStripMenuItem.Text = "Scale 2x";
            this.scale2xToolStripMenuItem.Click += new System.EventHandler(this.OnScaleItemClicked);
            // 
            // scale3xToolStripMenuItem
            // 
            this.scale3xToolStripMenuItem.Name = "scale3xToolStripMenuItem";
            this.scale3xToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.scale3xToolStripMenuItem.Text = "Scale 3x";
            this.scale3xToolStripMenuItem.Click += new System.EventHandler(this.OnScaleItemClicked);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showTracesToolStripMenuItem,
            this.showAABBsToolStripMenuItem,
            this.showEntityOriginsToolStripMenuItem,
            this.showCollisionMapsToolStripMenuItem,
            this.showTileFramesToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // showTracesToolStripMenuItem
            // 
            this.showTracesToolStripMenuItem.CheckOnClick = true;
            this.showTracesToolStripMenuItem.Name = "showTracesToolStripMenuItem";
            this.showTracesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.showTracesToolStripMenuItem.Tag = "r_showtracelines";
            this.showTracesToolStripMenuItem.Text = "Show Traces";
            this.showTracesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.OnMenuItem_CheckedChanged);
            // 
            // showAABBsToolStripMenuItem
            // 
            this.showAABBsToolStripMenuItem.CheckOnClick = true;
            this.showAABBsToolStripMenuItem.Name = "showAABBsToolStripMenuItem";
            this.showAABBsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.showAABBsToolStripMenuItem.Tag = "r_showcollisionboxes";
            this.showAABBsToolStripMenuItem.Text = "Show AABBs";
            this.showAABBsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.OnMenuItem_CheckedChanged);
            // 
            // showEntityOriginsToolStripMenuItem
            // 
            this.showEntityOriginsToolStripMenuItem.CheckOnClick = true;
            this.showEntityOriginsToolStripMenuItem.Name = "showEntityOriginsToolStripMenuItem";
            this.showEntityOriginsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.showEntityOriginsToolStripMenuItem.Tag = "r_showentityorigins";
            this.showEntityOriginsToolStripMenuItem.Text = "Show Entity Origins";
            this.showEntityOriginsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.OnMenuItem_CheckedChanged);
            // 
            // showCollisionMapsToolStripMenuItem
            // 
            this.showCollisionMapsToolStripMenuItem.CheckOnClick = true;
            this.showCollisionMapsToolStripMenuItem.Name = "showCollisionMapsToolStripMenuItem";
            this.showCollisionMapsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.showCollisionMapsToolStripMenuItem.Tag = "r_showcollisionmaps";
            this.showCollisionMapsToolStripMenuItem.Text = "Show Collision Maps";
            this.showCollisionMapsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.OnMenuItem_CheckedChanged);
            // 
            // showTileFramesToolStripMenuItem
            // 
            this.showTileFramesToolStripMenuItem.CheckOnClick = true;
            this.showTileFramesToolStripMenuItem.Name = "showTileFramesToolStripMenuItem";
            this.showTileFramesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.showTileFramesToolStripMenuItem.Tag = "r_showtileframes";
            this.showTileFramesToolStripMenuItem.Text = "Show Tile Frames";
            this.showTileFramesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.OnMenuItem_CheckedChanged);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 201);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GameForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Spikeball Engine";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTracesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAABBsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEntityOriginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCollisionMapsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTileFramesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scale1xToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scale2xToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scale3xToolStripMenuItem;
    }
}

