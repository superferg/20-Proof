using System;
using System.Collections.Generic;
using PlayingCards;

namespace CardGames
{
    partial class MainForm
    {
    	public enum States
    	{
    		Startup,
    		Teams,
    		Dealer,
    		Game,
    		Play,
    		GameOver
    	}
    	
    	public enum Games
    	{
    		TableTalk,
    		PassLeftAndLose,
    		SyncUp,
    		TradeIn,
    		Fusion,
    		PlusMinus,
    		CyclingTrump,
    		Loaner,
    		Poker,
    		War
    	}
    	
    	public States state = States.Startup;
    	public Games GameType = Games.TableTalk;
    	public CardSuits TrumpSuit;
    	public int WinnerIndex;
    	
    	private void EuchreValid()
    	{
    		if(WinnerIndex != clientIndex)
    		{
    			CardSuits LeadSuit = tableHands[players[WinnerIndex].TableIndex].PeekTopCard().Suit;
    			
    			if(IsLeft(tableHands[players[WinnerIndex].TableIndex].PeekTopCard()))
    				LeadSuit = TrumpSuit;
    			
    			List<Card> Cards = players[clientIndex].Hand.Cards.FindAll(item => item.Suit == LeadSuit);
    			
    			foreach(Card c in players[clientIndex].Hand.Cards)
    			{
    				if(IsLeft(c) && (LeadSuit == TrumpSuit))
    					Cards.Add(c);
    			}
    			
    			if(Cards.Count == 1 && IsLeft(Cards[0]) && LeadSuit != TrumpSuit)
    			{
    				Cards.Clear();
    			}
    			
    			if(Cards.Count > 0 )
    			{
	    			for(int i=0; i < players[clientIndex].Hand.NumberOfCards; i++)
	    			{
	    				if((players[clientIndex].Hand.Cards[i].Suit == LeadSuit && !IsLeft(players[clientIndex].Hand.Cards[i])) ||
	    				    (LeadSuit == TrumpSuit && IsLeft(players[clientIndex].Hand.Cards[i])))
	    				{
	    					HandBox[0,i].Enabled = true;
	    				}
	    				else
	    				{
	    					HandBox[0,i].Enabled = false;
	    					HandBox[0,i].Image = ImageHelper.ChangeImageOpacity(HandBox[0,i].Image, 0.39);
	    				}
	    			}
    			}
    			else
    			{
    				for(int i=0; i < players[clientIndex].Hand.NumberOfCards; i++)
    				{
    					HandBox[0,i].Enabled = true;
    				}
    			}
    		}
    		else
    		{
    			for(int i=0; i < players[clientIndex].Hand.NumberOfCards; i++)
	    		{
    				HandBox[0,i].Enabled = true;
    			}
    		}
    	}
    	
    	private void EuchreValidReset()
    	{
    		for(int i=0; i < players[clientIndex].Hand.NumberOfCards; i++)
    		{
    			if(players[clientIndex].Hand.Cards[i].Value >= CardValues.Nine)
    			{
    				HandBox[0,i].Image = ImageHelper.ChangeImageOpacity(HandBox[0,i].Image, 0.82);
    			}
    			else
    			{
    				HandBox[0,i].Image = ImageHelper.ChangeImageOpacity(HandBox[0,i].Image, 1.0);
    			}
    		}
    	}
    		
    	private bool IsLeft(Card c)
    	{
    		if(c.Value != CardValues.Jack)
    		{
    			return false;
    		}
    		else if(TrumpSuit == CardSuits.Clubs && c.Suit == CardSuits.Spades)
    		{
    			return true;
    		}
    		else if(TrumpSuit == CardSuits.Spades && c.Suit == CardSuits.Clubs)
    		{
    			return true;
    		}
    		else if(TrumpSuit == CardSuits.Hearts && c.Suit == CardSuits.Diamonds)
    		{
    			return true;
    		}
    		else if(TrumpSuit == CardSuits.Diamonds && c.Suit == CardSuits.Hearts)
    		{
    			return true;
    		}
    		
    		return false;
    	}
    } 
}