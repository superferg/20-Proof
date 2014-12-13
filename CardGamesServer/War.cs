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
    	private List<int> MaxIndex = new List<int>();
    	
    	public void PlayWar()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				BroadcastAll(msg_SETMYHAND, SERVER, FacingSides.FaceDown.ToString());
		    		BroadcastAll(msg_SETOTHERHANDS, SERVER, FacingSides.FaceDown.ToString());
		    		BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceUp.ToString());
    		
    				Shuffle();
    				DealCards(false);
    				
    				Thread.Sleep(500);
    				BroadcastAll(msg_ENABLEHAND, SERVER, "True");
    				BroadcastAll(msg_ENABLETABLEHAND, SERVER, "False");
    				
    				TurnIndex++;
    				break;
    				
    			case 1:
    				if(tableHand[0].NumberOfCards == 1 && tableHand[1].NumberOfCards == 1 && tableHand[2].NumberOfCards == 1 && tableHand[3].NumberOfCards == 1)
    				{
    					List<Card> TableCards = new List<Card>();
    					foreach(Hand h in tableHand)
    					{
    						TableCards.Add(h.Cards[0]);
    					}
    					
    					// Get max card value from table, and index of all instances of the max value
    					CardValues max = TableCards.Max(item => item.Value);
    					MaxIndex = TableCards.Select((b,i) => object.Equals(b.Value, max) ? i : -1).Where(i => i != -1).ToList();
    					
    					bool flag = true;
    					for(int i = 0; i < MaxIndex.Count; i++)
    					{
    						if(clients[MaxIndex[0]].Team != clients[MaxIndex[i]].Team)
    						{
    							flag = false;
    							break;
    						}
    					}
    						
    					if(MaxIndex.Count > 1)
    					{
    						string names = String.Empty;
	    					foreach(int i in MaxIndex)
	    					{
	    						names += clients[i].Name + " and ";                 
	    					}
	    					names = names.Remove(names.Length-5, 5);
	    					
	    					if(flag == false)	// Different teams tied
    						{
	    						BroadcastAll(msg_INFO, SERVER, "There is a tie between " + names + ".  Cards will be drawn from the deck.");
	    						War();
    						}
    						else // Same team tied
    						{
    							TrickCount[(int)clients[MaxIndex[0]].Team]++;
    							BroadcastAll(msg_SETTRICKS, SERVER, TrickCount[0] + ":" + TrickCount[1]);
	    						BroadcastAll(msg_INFO, SERVER, clients[MaxIndex[0]].Name + " and " + clients[MaxIndex[1]].Name+ " win.");
	    						
	    						TurnIndex = 3;
	    						PlayWar();
    						}
    					}
    					else
    					{
    						TrickCount[(int)clients[MaxIndex[0]].Team]++;
    						BroadcastAll(msg_SETTRICKS, SERVER, TrickCount[0] + ":" + TrickCount[1]);
    						BroadcastAll(msg_INFO, SERVER, clients[MaxIndex[0]].Name + " wins.");
    						
    						TurnIndex = 3;
    						PlayWar();
    					}
    				}
    				break;
    				
    			case 2:
    				List<Card> TableCardsWar = new List<Card>();
    				foreach(int i in MaxIndex)
    				{
    					TableCardsWar.Add(tableHand[i].Cards[0]);
    				}
    				
    				// Get max card value from table, and index of all instances of the max value
    				CardValues maxCard = TableCardsWar.Max(item => item.Value);
    				List<int> WarMaxIndex = TableCardsWar.Select((b,i) => object.Equals(b.Value, maxCard) ? i : -1).Where(i => i != -1).ToList();
    				
    				for(int i=0; i < WarMaxIndex.Count; i++)
    				{
    					WarMaxIndex[i] = MaxIndex[WarMaxIndex[i]];
    				}
    				
    				if(WarMaxIndex.Count > 1)
    				{    
						int MaxCount = MaxIndex.Count;
    					for(int i=0, x=0; i < MaxCount; i++, x++)
    					{
    						if(!WarMaxIndex.Contains(MaxIndex[x]))
    						{
    							MaxIndex.RemoveAt(x);
    							x--;
    						}
    					}
    					
    					string names = String.Empty;
    					foreach(int i in MaxIndex)
    					{
    						names += clients[i].Name + " and ";
    					}
    					
    					names = names.Remove(names.Length-5, 5);
    					BroadcastAll(msg_INFO, SERVER, "There is a tie between " + names + ".  Cards will be drawn from the deck.");
    					
    					War();
    				}
    				else
    				{
    					TrickCount[(int)clients[WarMaxIndex[0]].Team]++;
    					BroadcastAll(msg_SETTRICKS, SERVER, TrickCount[0] + ":" + TrickCount[1]);
    					BroadcastAll(msg_INFO, SERVER, clients[WarMaxIndex[0]].Name + " wins.");
    					TurnIndex = 3;
    					PlayWar();
    				}
    				break;
    				
    			case 3:
    				Thread.Sleep(3000);
    				foreach(Client c in clients)
    				{
    					BroadcastAll(msg_CLEARTABLEHAND, SERVER, c.Id);
    					tableHand[clients.IndexOf(c)].Cards.Clear();
    				}
    				
    				RoundIndex++;
    				if(RoundIndex >= 5)
    				{
    					if(TrickCount[(int)Teams.TeamOne] > TrickCount[(int)Teams.TeamTwo])
    					{
    						TeamScore[(int)Teams.TeamOne]++;
    						
    						if(TrickCount[(int)Teams.TeamOne] == 5)
    						{
    							TeamScore[(int)Teams.TeamOne]++;
    						}
    						
    						string newString = Teams.TeamOne + " wins, " + TrickCount[0] + " to " + TrickCount[1] + ".";
    						newString = Regex.Replace(newString, @"\B([A-Z])", " $1");
    						BroadcastAll(msg_INFO, SERVER, newString);
    					}
    					else
    					{
    						TeamScore[(int)Teams.TeamTwo]++;
    						
    						if(TrickCount[(int)Teams.TeamTwo] == 5)
    						{
    							TeamScore[(int)Teams.TeamTwo]++;
    						}
    						
    						string newString = Teams.TeamTwo + " wins, " + TrickCount[1] + " to " + TrickCount[0] + ".";
    						newString = Regex.Replace(newString, @"\B([A-Z])", " $1");
    						BroadcastAll(msg_INFO, SERVER, newString);
    					}
    					
    					BroadcastAll(msg_SETSCORE, SERVER, TeamScore[0] + ":" + TeamScore[1]);
    					
    					Thread.Sleep(2000);
    					BroadcastAll(msg_RESETTABLE, SERVER, "");
    					ClearTable();
    					Array.Clear(TrickCount, 0, TrickCount.Length);
    					TurnIndex = 0;
    					RoundIndex = 0;
    					
    					IncrementDealer();
    					
    					if(TeamScore[0] >= 10 || TeamScore[1] >= 10)
    					{
    						State = States.GameOver;
    					}
    					else
    					{
    						State = States.Game;
    					}
    					
    					StateFunc[(int)State]();
    				}
    				else
    				{
    					TurnIndex = 1;
    					BroadcastAll(msg_ENABLEHAND, SERVER, "True");
    				}
    				break;
    				
    			default:
    				break;
    		}
    	}
    	
    	public void War()
    	{
    		Thread.Sleep(3000);
    		foreach(Client c in clients)
    		{
    			BroadcastAll(msg_CLEARTABLEHAND, SERVER, c.Id);
    			tableHand[clients.IndexOf(c)].Cards.Clear();
    		}
    				
    		foreach(int i in MaxIndex)
    		{
	    		Card card = bottomDeck.Deck.GetRandomCard();
	    		tableHand[i].Cards.Add(card);
	    		BroadcastAll(msg_DECKTOTABLE, SERVER, clients[i].Id + ":" + card.TextValue);
	    		Thread.Sleep(2000);
	    	}
    		
    		TurnIndex = 2;
    		PlayWar();
    	}
    }
}