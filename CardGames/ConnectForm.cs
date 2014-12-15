using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CardGames
{
    public partial class ConnectForm : Form
    {
        public ConnectForm()
        {
            InitializeComponent();
            
            txtName.Text = "Scott";
            textBox1.Text = "192.168.1.70";
        }


        private string ipAddress;
        private string clientName;

        public string IPAddress
        {
            get { return ipAddress; }
        }

        public string ClientName
        {
            get { return clientName; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clientName = txtName.Text;
            ipAddress = textBox1.Text;

            this.Close();
        }
        
        private void KeyPressFunction(object sender, KeyEventArgs e)
        {
        	if (e.KeyCode == Keys.Return)
	        {
	            button1.PerformClick();
	            
	            e.Handled = true;
        		e.SuppressKeyPress = true;
	        }
        }
    }
}
