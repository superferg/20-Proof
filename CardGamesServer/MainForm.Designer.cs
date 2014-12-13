namespace CardGamesServer
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
        	this.label1 = new System.Windows.Forms.Label();
        	this.button1 = new System.Windows.Forms.Button();
        	this.textBox1 = new System.Windows.Forms.TextBox();
        	this.SuspendLayout();
        	// 
        	// label1
        	// 
        	this.label1.AutoSize = true;
        	this.label1.Location = new System.Drawing.Point(12, 18);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(133, 13);
        	this.label1.TabIndex = 0;
        	this.label1.Text = "Host listening on port 4994";
        	// 
        	// button1
        	// 
        	this.button1.Location = new System.Drawing.Point(305, 132);
        	this.button1.Name = "button1";
        	this.button1.Size = new System.Drawing.Size(75, 23);
        	this.button1.TabIndex = 1;
        	this.button1.Text = "Start";
        	this.button1.UseVisualStyleBackColor = true;
        	this.button1.Click += new System.EventHandler(this.button1_Click);
        	// 
        	// textBox1
        	// 
        	this.textBox1.Location = new System.Drawing.Point(12, 34);
        	this.textBox1.Multiline = true;
        	this.textBox1.Name = "textBox1";
        	this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        	this.textBox1.Size = new System.Drawing.Size(368, 92);
        	this.textBox1.TabIndex = 2;
        	// 
        	// MainForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(403, 167);
        	this.Controls.Add(this.textBox1);
        	this.Controls.Add(this.button1);
        	this.Controls.Add(this.label1);
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        	this.MaximizeBox = false;
        	this.Name = "MainForm";
        	this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        	this.Text = "CardGamesServer";
        	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_Closed);
        	this.Load += new System.EventHandler(this.MainForm_Load);
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
    }
}

