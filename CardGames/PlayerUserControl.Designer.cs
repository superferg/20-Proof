namespace CardGames
{
    partial class PlayerUserControl
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayerUserControl));
        	this.pictureBox1 = new System.Windows.Forms.PictureBox();
        	this.lblName = new System.Windows.Forms.Label();
        	this.lblRdy = new System.Windows.Forms.Label();
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// pictureBox1
        	// 
        	this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
        	this.pictureBox1.Location = new System.Drawing.Point(3, 3);
        	this.pictureBox1.Name = "pictureBox1";
        	this.pictureBox1.Size = new System.Drawing.Size(46, 50);
        	this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        	this.pictureBox1.TabIndex = 0;
        	this.pictureBox1.TabStop = false;
        	// 
        	// lblName
        	// 
        	this.lblName.AutoSize = true;
        	this.lblName.Font = new System.Drawing.Font("Arial Black", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblName.Location = new System.Drawing.Point(58, 9);
        	this.lblName.Name = "lblName";
        	this.lblName.Size = new System.Drawing.Size(56, 15);
        	this.lblName.TabIndex = 1;
        	this.lblName.Text = "lblName";
        	// 
        	// lblRdy
        	// 
        	this.lblRdy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.lblRdy.Location = new System.Drawing.Point(58, 33);
        	this.lblRdy.Name = "lblRdy";
        	this.lblRdy.Size = new System.Drawing.Size(56, 23);
        	this.lblRdy.TabIndex = 2;
        	this.lblRdy.Text = "Ready";
        	this.lblRdy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        	this.lblRdy.Visible = false;
        	// 
        	// PlayerUserControl
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.BackColor = System.Drawing.Color.White;
        	this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.Controls.Add(this.lblRdy);
        	this.Controls.Add(this.lblName);
        	this.Controls.Add(this.pictureBox1);
        	this.Name = "PlayerUserControl";
        	this.Size = new System.Drawing.Size(149, 56);
        	((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblRdy;
    }
}
