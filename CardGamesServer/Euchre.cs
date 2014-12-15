using System;
using System.Collections.Generic;
using System.Linq;
using PlayingCards;
using System.Threading;
using System.Text.RegularExpressions;

namespace CardGamesServer
{
    partial class MainForm
    {
    	private bool PickedUp = false;
    	private Teams trumpTeam = Teams.TeamOne;
    	private CardSuits trumpSuit = CardSuits.Clubs;
    	private int winnerIndex = 0;
    	private int TableIndex = -1;
    	
    	public int WinnerIndex
    	{
    		get { return winnerIndex; }
    		set 
    		{ 
    			winnerIndex = value;
    			BroadcastAll(msg_SETWINNERINDEX, SERVER, winnerIndex.ToString());
    		}
    	}
    	
    	public CardSuits TrumpSuit
    	{
    		get { return trumpSuit; }
    		set 
    		{ 
    			trumpSuit = value;
    			BroadcastAll(msg_SETTRUMP, SERVER, trumpSuit.ToString());
    		}
    	}
    	
    	public void CallTrumpRound1()
    	{
    		if(PickedUp == true)
    		{
    			AssignTrump(true);
    		}
    		else
    		{
    			BroadcastAll(msg_INFO, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Name + " passed.");
    			Thread.Sleep(1000);
    			TableIndex++;
    			
    			if(TableIndex > 3)
    				TableIndex -= 4;
    			
    			RoundIndex++;
    			if(RoundIndex >= 4)
    			{
    				RoundIndex = 0;
    				TurnIndex++;
    				
    				BroadcastAll(msg_HIDEDECK, SERVER, clients[DealerPosition].Id);
    				BroadcastAll(msg_CALLTRUMP2, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id + ":" + dealer.Deck.PeekTopCard().Suit);
    			}
    			else
    			{
    				BroadcastAll(msg_CALLTRUMP, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id + ":" + dealer.Deck.PeekTopCard());
    			}
    		}
    	}
    	
    	public void CallTrumpRound2()
    	{
    		if(PickedUp == true)
    		{
    			AssignTrump(false);
    		}
    		else
    		{
    			BroadcastAll(msg_INFO, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Name + " passed.");
    			Thread.Sleep(1000);
    			TableIndex++;
    			
    			if(TableIndex > 3)
    				TableIndex -= 4;
    			
    			BroadcastAll(msg_CALLTRUMP2, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id + ":" + dealer.Deck.PeekTopCard().Suit);
    		}
    	}
    	
    	public void PlayTrick(int ScoreIndex)
    	{
    		TableIndex++;
    		if(TableIndex > 3)
    			TableIndex -= 4;
    		
    		RoundIndex++;
    		if(RoundIndex >= 4)
    		{
    			RoundIndex = 0;
    			TurnIndex = ScoreIndex;
    			StateFunc[(int)State]();
    		}
    		else
    		{
    			BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    			BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    		}
    	}
    	
    	public void PickUpCard(int PlayIndex)
    	{
    		Thread.Sleep(1000);
    		BroadcastAll(msg_HIDEDECK, SERVER, clients[DealerPosition].Id);
    		
    		clients[DealerPosition].Hand.Cards.Add(dealer.Deck.GetTopCard());
    		clients[DealerPosition].Hand.Sort();
    		SendHandMessage(clients[DealerPosition], SERVER, false);
    		
    		BroadcastAll(msg_CLEARTABLEHAND, SERVER, clients[DealerPosition].Id);
    		BroadcastAll(msg_SHOWTABLEHAND, SERVER, clients[DealerPosition].Id);
    		
    		tableHand[DealerPosition].Cards.Clear();
    		
    		TurnIndex = PlayIndex;
    		BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    		BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    	}
    	
    	public void AssignTrump(bool suit)
    	{
    		if(suit) TrumpSuit = dealer.Deck.PeekTopCard().Suit;
    		
    		BroadcastAll(msg_INFO, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Name + " called " + TrumpSuit);
    		
    		trumpTeam = clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Team;
    		
    		TableIndex = clients[DealerPosition].TableIndex + 1;
    		
    		if(TableIndex > 3)
    			TableIndex -= 4;
    		
    		RoundIndex = 0;
    		WinnerIndex = clients.FindIndex(item => item.TableIndex == TableIndex);
    		
    		if(suit)
    		{
    			BroadcastAll(msg_HIDETABLEHAND, SERVER, clients[DealerPosition].Id);
    			BroadcastTo(msg_ENABLEHAND, SERVER, clients[DealerPosition].Id, "True");
    			
    			BroadcastTo(msg_INFO, SERVER, clients[DealerPosition].Id, "Discard a card, please.");
    			BroadcastAll(msg_INFO, clients[DealerPosition].Id, clients[DealerPosition].Name + " must discard a card.");
    		}
    		else
    		{
    			BroadcastAll(msg_HIDEDECK, SERVER, clients[DealerPosition].Id);
    		}
    	}
    	
    	public double[] EuchreEvaluate()
    	{
    		double[] Value = new double[4];
    		
    		for(int i=0; i < 4; i++)
    		{
    			if(tableHand[i].Cards[0].Suit == TrumpSuit || IsLeft(tableHand[i].Cards[0]))
    			{
    				if(IsLeft(tableHand[i].Cards[0])) 
    					Value[i] = 14;
    				else if(tableHand[i].Cards[0].Value == CardValues.Jack)
    					Value[i] = 15;
    				else if(tableHand[i].Cards[0].Value >= CardValues.Queen)
    					Value[i] = (int)tableHand[i].Cards[0].Value + 1;
    				else if(tableHand[i].Cards[0].Value >= CardValues.Nine)
    					Value[i] = (int)tableHand[i].Cards[0].Value + 2;
    				else
    				{
    					Value[i] = 14.5 + ((int)tableHand[i].Cards[0].Value) * 0.5;
    					continue;
    				}
    				
    				Value[i] *= 2;
    			}
    			else if(tableHand[i].Cards[0].Suit == tableHand[WinnerIndex].Cards[0].Suit)
    			{
    				Value[i] = (int)tableHand[i].Cards[0].Value + 2;
    			}
    			else
    			{
    				Value[i] = 0;
    			}
    		}
    		
    		return Value;
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