using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlayingCards
{   
	public enum Teams
	{
		TeamOne,
		TeamTwo
	}
	
    public class Player
    {
        private Hand hand = new Hand();

		public string Id { get; set;}
		public string Name { get; set; }
        public int TableIndex { get; set; }
        public Teams Team { get; set; }

        public Player()
        {
        }

        public Hand Hand
        {
            get { return hand; }
            set { hand = value; }
        }    
    }
}
