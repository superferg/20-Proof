using System;
using System.Drawing;
using System.Windows.Forms;
using PlayingCards;

namespace CardGames
{
	/// <summary>
	/// Description of TrumpForm.
	/// </summary>
	public partial class TrumpForm : Form
	{
		public TrumpForm()
		{
			InitializeComponent();
			
		}
		
		public TrumpForm(Card c)
		{
			InitializeComponent();
			
			label1.Text = c.TextValue;
		}
		
		public TrumpForm(string card)
		{
			InitializeComponent();
			
			label1.Text = card;
		}
		
		public bool PickedUp { get; set; }
		
		
		void Button1Click(object sender, EventArgs e)
		{
			PickedUp = true;
			this.Close();
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			PickedUp = false;
			this.Close();
		}
	}
}
