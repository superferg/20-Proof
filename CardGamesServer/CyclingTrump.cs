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
    	public void PlayCyclingTrump()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				BroadcastAll(msg_SETMYHAND, SERVER, FacingSides.FaceUp.ToString());
    				BroadcastAll(msg_SETOTHERHANDS, SERVER, FacingSides.FaceDown.ToString());
    				BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceUp.ToString());
    				
    				Shuffle();
    				DealCards(true);
    				
    				Card card = dealer.Deck.GetTopCard();
    				BroadcastAll(msg_SENDDECK, SERVER, clients[DealerPosition].Id + ":" + card.TextValue);
    				dealer.Deck.Cards.Insert(0, card);
    				TrumpSuit = card.Suit;
    				
    				Thread.Sleep(500);
    				BroadcastAll(msg_ENABLEHAND, SERVER, "False");
    				BroadcastAll(msg_ENABLETABLEHAND, SERVER, "False");
    				
    				TableIndex = clients[DealerPosition].TableIndex + 1;
    				
    				if(TableIndex > 3)
    					TableIndex -= 4;
    				
    				WinnerIndex = clients.FindIndex(item => item.TableIndex == TableIndex);
    				BroadcastAll(msg_INFO, SERVER, TrumpSuit + " is trump.");
    				BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    				BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    				TurnIndex++;
    				break;
    				
    			case 1:
    				PlayTrick(2);
    				break;
    				
    			case 2:
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
    					BroadcastAll(msg_HIDEDECK, SERVER, clients[DealerPosition].Id);
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
    					Card crd = dealer.Deck.GetTopCard();
    					BroadcastAll(msg_SENDDECK, SERVER, clients[DealerPosition].Id + ":" + crd.TextValue);
    					TrumpSuit = crd.Suit;
    					BroadcastAll(msg_INFO, SERVER, TrumpSuit + " is trump.");
    					
    					TableIndex = clients[WinnerIndex].TableIndex;
    					BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    					BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    					TurnIndex = 1;
    				}
    				break;
    				
    			default:
    				break;
    		}
    	}
    }
}