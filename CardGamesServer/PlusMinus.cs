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
    	CardColor positiveColor = CardColor.Black;
    	
    	public void PlayPlusMinus()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, DealerPosition.ToString());
	    			BroadcastAll(msg_INFO, SERVER, "Roll to determine which color is positive.  Even for red, odd for black");
    				break;
    				
    			case 1:
    				if(DiceRollValue[TurnIndex-1] % 2 == 0)
    				{
    					positiveColor = CardColor.Red;
    					BroadcastAll(msg_INFO, SERVER, "Red is positive, Black is negative.");
    				}
    				else
    				{
    					positiveColor = CardColor.Black;
    					BroadcastAll(msg_INFO, SERVER, "Black is positive, Red is negative.");
    				}
    				Thread.Sleep(1500);
    				
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
    					TurnIndex = 4;
    					BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    					BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    				}
    				break;
    				
    			case 4:
    				PlayTrick(5);
    				break;
    				
    			case 5:
    				double[] Value = EuchreEvaluate();
    				WinnerIndex = Value.ToList().IndexOf(Value.Max());
    				
    				if(tableHand[WinnerIndex].PeekTopCard().Color == positiveColor)
    					TrickCount[(int)clients[WinnerIndex].Team]++;
    				else
    					TrickCount[(int)clients[WinnerIndex].Team]--;
    				
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
    						
    						if(TrickCount[(int)Teams.TeamOne] - TrickCount[(int)Teams.TeamTwo] == 5 || trumpTeam == Teams.TeamTwo)
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
    						
    						if(TrickCount[(int)Teams.TeamTwo] - TrickCount[(int)Teams.TeamOne] == 5 || trumpTeam == Teams.TeamOne)
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
    				PickUpCard(4);
    				break;
    				
    			default:
    				break;
    		}
    	}
    }
}