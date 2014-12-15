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
    	public void PlaySyncUp()
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
    				BroadcastAll(msg_ENABLEHAND, SERVER, "False");
    				BroadcastAll(msg_ENABLETABLEHAND, SERVER, "False");
    				
    				TableIndex = clients[DealerPosition].TableIndex + 1;
    				
    				if(TableIndex > 3)
    					TableIndex -= 4;
    				
    				BroadcastAll(msg_CALLTRUMP, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id + ":" + dealer.Deck.PeekTopCard());
    				TurnIndex++;
    				break;
    				
    			case 1:
    				CallTrumpRound1();
    				if(PickedUp == true)
    				{
    					TurnIndex = 5;
    				}
    				break;
    				
    			case 2:
    				CallTrumpRound2();
    				if(PickedUp == true)
    				{
    					TurnIndex = 3;
    					BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    					BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    				}
    				break;
    				
    			case 3:
    				PlayTrick(4);
    				break;
    				
    			case 4:
    				double[] Value = EuchreEvaluate();
    				int Team1_1 = clients.FindIndex(item => item.Team == Teams.TeamOne);
    				int Team1_2 = clients.FindLastIndex(item => item.Team == Teams.TeamOne);
    				int Team2_1 = clients.FindIndex(item => item.Team == Teams.TeamTwo);
    				int Team2_2 = clients.FindLastIndex(item => item.Team == Teams.TeamTwo);
    				
    				if(tableHand[Team1_1].PeekTopCard().Value == tableHand[Team1_2].PeekTopCard().Value)
    				{
    					if(tableHand[Team2_1].PeekTopCard().Value == tableHand[Team2_2].PeekTopCard().Value)
    					{
    						if(tableHand[Team1_1].PeekTopCard().Value > tableHand[Team2_1].PeekTopCard().Value)
    						{
    							WinnerIndex = Value[Team1_1] > Value[Team1_2] ? Team1_1 : Team1_2;
    						}
    						else if(tableHand[Team2_1].PeekTopCard().Value > tableHand[Team1_1].PeekTopCard().Value)
    						{
    							WinnerIndex = Value[Team2_1] > Value[Team2_2] ? Team2_1 : Team2_2;
    						}
    						else
    						{
    							WinnerIndex = Value.ToList().IndexOf(Value.Max());
    						}
    					}
    					else
    					{
    						WinnerIndex = Value[Team1_1] > Value[Team1_2] ? Team1_1 : Team1_2;
    					}
    				}
    				else if(tableHand[Team2_1].PeekTopCard().Value == tableHand[Team2_2].PeekTopCard().Value)
    				{
    					WinnerIndex = Value[Team2_1] > Value[Team2_2] ? Team2_1 : Team2_2;
    				}
    				else
    				{
    					WinnerIndex = Value.ToList().IndexOf(Value.Max());
    				}
    				
    				TrickCount[(int)clients[WinnerIndex].Team]++;
    				string s1 = clients[WinnerIndex].Team + " wins.";
    				s1 = Regex.Replace(s1, @"\B([A-Z])", " $1");
    				BroadcastAll(msg_INFO, SERVER, s1);
    				
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
    					if(TrickCount[(int)Teams.TeamOne] > TrickCount[(int)Teams.TeamTwo])
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
    					TurnIndex = 3;
    				}
    				break;
    				
    			case 5:
    				PickUpCard(3);
    				break;
    				
    			default:
    				break;
    		}
    	}
    }
}