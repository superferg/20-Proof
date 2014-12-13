using System;
using System.Drawing;
using System.Windows.Forms;
using PlayingCards;

namespace CardGames
{
	/// <summary>
	/// Description of Trump2Form.
	/// </summary>
	public partial class Trump2Form : Form
	{
		public Trump2Form()
		{
			InitializeComponent();
		}
		
		public Trump2Form(CardSuits suit, bool b)
		{
			InitializeComponent();
			
			foreach (Control c in this.Controls)
			{
				var button = c as Button;
				if (button != null)
				{
					button.Enabled = suit.ToString() != button.Text;
				}
			}
			
			this.button1.Enabled = b;
		}
		
		public Trump2Form(string suit, bool b)
		{
			InitializeComponent();
			
			foreach (Control c in this.Controls)
			{
				var button = c as Button;
				if (button != null)
				{
					button.Enabled = suit != button.Text;
				}
			}
			
			button1.Enabled = b;
		}
		
		public bool PickedUp { get; set; }
		public string Suit { get; set; }
		
		void Button1Click(object sender, EventArgs e)
		{
			PickedUp = false;
			Suit = ((Button)sender).Text;
			this.Close();
		}
		
		void SuitButtonClick(object sender, EventArgs e)
		{
			PickedUp = true;
			Suit = ((Button)sender).Text;
			this.Close();
		}
	}
}
