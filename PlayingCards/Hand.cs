using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PlayingCards
{
    public class Hand : Deck
    {
    	public FacingSides FacingSide
    	{
    		set
    		{
    			foreach(Card c in this.Cards)
    			{
    				c.FacingSide = value;
    			}
    		}
    		
    		get { return this.Cards[0].FacingSide; }
    	}
    	
    	public Hand Pass(Hand hand, Card c)
        {
            // Method to pass a card to another hand

            if (Exists(c))
            {
                this.Cards.Remove(c);
                hand.Cards.Add(c);
            }

            return hand;
        }
    	
    	public Hand Pass(Hand hand)
        {
            // Method to pass all cards to another hand

            int NumCards = this.NumberOfCards;
            for(int i=0; i < NumCards; i++)
            {
            	hand = this.Pass(hand, Cards[0]);
            }

            return hand;
        }
        
        public Hand Exchange(Hand hand, Card c1, Card c2)
        {
            // Method to exchange two cards between hands

            if (Exists(c1) && hand.Exists(c2))
            {
                this.Cards.Remove(c1);
                hand.Cards.Remove(c2);
                hand.Cards.Add(c1);
                this.Cards.Add(c2);
            }

            return hand;
        }
        
        public Hand Exchange(Hand hand)
        {
            // Method to exchange all cards between hands
            for(int i=0; i < this.NumberOfCards; i++)
            {
            	hand = this.Exchange(hand, Cards[0], hand.Cards[0]);
            }

            return hand;
        }
        
        public void Sort()
        {
        	this.Cards = this.Cards.OrderBy(x => x.Suit).ThenBy(x => x.Value).ToList();
        }
    }
}
