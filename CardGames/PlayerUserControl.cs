using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace CardGames
{
    public partial class PlayerUserControl : UserControl
    {
        private string name;

        public string PlayerName
        {
            get { return name; }
            set { 
                
                name = value;
                lblName.Text = name;
            }
        }
        
        public bool ShowReady
        {
        	get { return lblRdy.Visible; }
        	set { lblRdy.Visible = value; }
        }

        public PlayerUserControl()
        {
            InitializeComponent();
        }
    }
}
