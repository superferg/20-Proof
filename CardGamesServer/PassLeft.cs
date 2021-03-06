﻿using System;
using System.Collections.Generic;
using System.Linq;
using PlayingCards;
using System.Threading;
using System.Text.RegularExpressions;

namespace CardGamesServer
{
    partial class MainForm
    {
    	public void PlayPassLeft()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				BroadcastAll(msg_SETMYHAND, SERVER, FacingSides.FaceUp.ToString());
		    		BroadcastAll(msg_SETOTHERHANDS, SERVER, FacingSides.FaceDown.ToString());
		    		BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceUp.ToString());
    		
    				Shuffle();
    				DealCards(true);
    				
    				Card card = dealer.Deck.PeekTopCard();
    				BroadcastAll(msg_SENDDECK, SERVER, clients[DealerPosition].Id + ":" + card.TextValue);
    				
    				Thread.Sleep(500);
    				foreach(Client cl in clients)
    				{
    					cl.IsReady = false;
    				}
    				
    				BroadcastAll(msg_ENABLEHAND, SERVER, "False");
    				BroadcastAll(msg_ENABLETABLEHAND, SERVER, "False");
    				
    				TableIndex = clients[DealerPosition].TableIndex + 1;
    				
    				if(TableIndex > 3)
    					TableIndex -= 4;
    				
    				BroadcastAll(msg_CALLTRUMP, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id + ":" + dealer.Deck.PeekTopCard());
    				TurnIndex = 2;
    				break;
    				
    			case 1:
    				if(clients[0].IsReady == true && clients[1].IsReady == true &&
    				   clients[2].IsReady == true && clients[3].IsReady == true)
    				{
    					BroadcastAll(msg_ENABLEREADY, SERVER, "False");
    					
    					int Team1_1 = clients.FindIndex(item => item.TableIndex == 0);
    					int Team1_2 = clients.FindIndex(item => item.TableIndex == 2);
    					int Team2_1 = clients.FindIndex(item => item.TableIndex == 1);
    					int Team2_2 = clients.FindIndex(item => item.TableIndex == 3);
    					
    					clients[Team2_1].Hand = clients[Team1_1].Hand.Exchange(clients[Team2_1].Hand);
    					clients[Team1_2].Hand = clients[Team1_1].Hand.Exchange(clients[Team1_2].Hand);
    					clients[Team2_2].Hand = clients[Team1_1].Hand.Exchange(clients[Team2_2].Hand);
    					
    					foreach(Client c in clients)
    					{
    						SendHandMessage(c, SERVER, false);
    					}
    					
    					Thread.Sleep(2000);
			    		TableIndex = clients[DealerPosition].TableIndex + 1;
			        	
			        	if(TableIndex > 3)
			        		TableIndex -= 4;
			        	
			        	BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    					BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
			        	TurnIndex = 4;
    				}
    				break;
    				
    			case 2:
    				CallTrumpRound1();
    				if(PickedUp == true)
    				{
    					TurnIndex = 6;
    				}
    				break;
    				
    			case 3:
    				CallTrumpRound2();
    				if(PickedUp == true)
    				{
    					TurnIndex = 1;
    					BroadcastAll(msg_INFO, SERVER, "Select 'Ready' to pass your hand left.");
    					BroadcastAll(msg_ENABLEREADY, SERVER, "True");
    				}
    				break;
    				
    			case 4:
    				PlayTrick(5);
    				break;
    				
    			case 5:
    				double[] Value = EuchreEvaluate();
    				WinnerIndex = Value.ToList().IndexOf(Value.Max());
    				
    				TrickCount[(int)clients[WinnerIndex].Team]++;
    				BroadcastAll(msg_INFO, SERVER, clients[WinnerIndex].Name + " wins.");
    				
    				Thread.Sleep(2000);
    				BroadcastAll(msg_SETTRICKS, SERVER, TrickCount[0] + ":" + TrickCount[1]);
    				foreach(Client c in clients)
    				{
    					BroadcastAll(msg_CLEARTABLEHAND, SERVER, c.Id);
    					tableHand[clients.IndexOf(c)].Cards.Clear();
    				}
    				
    				RoundIndex = 0;
    				
    				if(clients[0].Hand.NumberOfCards == 0)
    				{
    					if(TrickCount[(int)Teams.TeamTwo] > TrickCount[(int)Teams.TeamOne])
    					{
    						TeamScore[(int)Teams.TeamOne]++;
    						
    						if(TrickCount[(int)Teams.TeamOne] == 5 || trumpTeam == Teams.TeamTwo)
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
    						
    						if(TrickCount[(int)Teams.TeamTwo] == 5 || trumpTeam == Teams.TeamOne)
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
    					TableIndex = clients[WinnerIndex].TableIndex;
    					BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    					BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    					TurnIndex = 4;
    				}
    				break;
    				
    			case 6:
    				Thread.Sleep(1000);
    				BroadcastAll(msg_HIDEDECK, SERVER, clients[DealerPosition].Id);
    				
    				clients[DealerPosition].Hand.Cards.Add(dealer.Deck.GetTopCard());
    				clients[DealerPosition].Hand.Sort();
    				SendHandMessage(clients[DealerPosition], SERVER, false);
    				
    				BroadcastAll(msg_CLEARTABLEHAND, SERVER, clients[DealerPosition].Id);
    				BroadcastAll(msg_SHOWTABLEHAND, SERVER, clients[DealerPosition].Id);
    				
    				tableHand[DealerPosition].Cards.Clear();
    				
    				TurnIndex = 1;
    				BroadcastAll(msg_INFO, SERVER, "Select 'Ready' to pass your hand left.");
    				BroadcastAll(msg_ENABLEREADY, SERVER, "True");
    				break;
    				
    			default:
    				break;
    		}
    	}
    }
}