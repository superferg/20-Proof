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
    	int[] TradeInCount = new int[2];
    	bool TrumpRound1 = false;
    	
    	public void PlayTradeIn()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				BroadcastAll(msg_SETMYHAND, SERVER, FacingSides.FaceUp.ToString());
		    		BroadcastAll(msg_SETOTHERHANDS, SERVER, FacingSides.FaceDown.ToString());
		    		BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceDown.ToString());
    		
    				Shuffle();
    				DealCards(true);
    				
    				Card card = dealer.Deck.PeekTopCard();
    				BroadcastAll(msg_SENDDECK, SERVER, clients[DealerPosition].Id + ":" + card.TextValue);
    				
    				foreach(Client cl in clients)
    				{
    					cl.IsReady = false;
    				}
    				
    				BroadcastAll(msg_ENABLEHAND, SERVER, "False");
    				BroadcastAll(msg_ENABLETABLEHAND, SERVER, "False");
    				
    				TableIndex = clients[DealerPosition].TableIndex + 1;
    				
    				if(TableIndex > 3)
    					TableIndex -= 4;
    				
    				Thread.Sleep(2000);
    				BroadcastAll(msg_ROLLTRADEIN, SERVER, clients.FindIndex(item => item.Team == Teams.TeamOne).ToString());
	    			BroadcastAll(msg_INFO, SERVER, "Roll to determine the number of cards your team must trade in.");
    				break;
    				
    			case 1:
    				Thread.Sleep(2000);
    				TradeInCount[(int)Teams.TeamOne] = DiceRollValue[TurnIndex-1];
    				BroadcastAll(msg_ROLLTRADEIN, SERVER, clients.FindIndex(item => item.Team == Teams.TeamTwo).ToString());
    				break;
    				
    			case 2:
    				Thread.Sleep(2000);
    				TradeInCount[(int)Teams.TeamTwo] = DiceRollValue[TurnIndex-1];
    				BroadcastAll(msg_CALLTRUMP, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id + ":" + dealer.Deck.PeekTopCard());
    				TurnIndex++;
    				break;
    				
    			case 3:
    				if(PickedUp == true)
		    		{
    					TrumpSuit = dealer.Deck.PeekTopCard().Suit;
    					
    					BroadcastAll(msg_INFO, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Name + " called " + TrumpSuit);
    					
    					trumpTeam = clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Team;
    					
    					TableIndex = clients[DealerPosition].TableIndex + 1;
    					
    					if(TableIndex > 3)
    						TableIndex -= 4;
    					
    					RoundIndex = 0;
    					WinnerIndex = clients.FindIndex(item => item.TableIndex == TableIndex);
		    			
		    			TurnIndex = 8;
		    			TrumpRound1 = true;
    					BroadcastAll(msg_INFO, SERVER, "Decide with your partner how many cards to trade in.");
    					BroadcastAll(msg_ENABLEHAND, SERVER, "True");
    					BroadcastAll(msg_ENABLETABLEHAND, SERVER, "True");
    					BroadcastAll(msg_ENABLEREADY, SERVER, "True");
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
    				break;
    				
    			case 4:
    				CallTrumpRound2();
    				if(PickedUp == true)
    				{
    					TurnIndex = 8;
    					TrumpRound1 = false;
    					BroadcastAll(msg_INFO, SERVER, "Decide with your partner how many cards to trade in.");
    					BroadcastAll(msg_ENABLEHAND, SERVER, "True");
    					BroadcastAll(msg_ENABLETABLEHAND, SERVER, "True");
    					BroadcastAll(msg_ENABLEREADY, SERVER, "True");
    				}
    				break;
    				
    			case 5:
    				PlayTrick(6);
    				break;
    				
    			case 6:
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
    					TurnIndex = 5;
    				}
    				break;
    				
    			case 7:
    				Thread.Sleep(1000);
    				BroadcastAll(msg_HIDEDECK, SERVER, clients[DealerPosition].Id);
    				
    				clients[DealerPosition].Hand.Cards.Add(dealer.Deck.GetTopCard());
    				clients[DealerPosition].Hand.Sort();
    				SendHandMessage(clients[DealerPosition], SERVER, false);
    				
    				BroadcastAll(msg_CLEARTABLEHAND, SERVER, clients[DealerPosition].Id);
    				BroadcastAll(msg_SHOWTABLEHAND, SERVER, clients[DealerPosition].Id);
    				
    				tableHand[DealerPosition].Cards.Clear();
    				
    				BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceUp.ToString());
    				BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
    				BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
    				TurnIndex = 5;
    				break;
    				
    			case 8:
    				if(clients[0].IsReady == true && clients[1].IsReady == true &&
    				   clients[2].IsReady == true && clients[3].IsReady == true)
    				{
    					int Team1_1 = clients.FindIndex(item => item.Team == Teams.TeamOne);
    					int Team1_2 = clients.FindLastIndex(item => item.Team == Teams.TeamOne);
    					int Team2_1 = clients.FindIndex(item => item.Team == Teams.TeamTwo);
    					int Team2_2 = clients.FindLastIndex(item => item.Team == Teams.TeamTwo);
    				
    					if(tableHand[Team1_1].NumberOfCards + tableHand[Team1_2].NumberOfCards == TradeInCount[0] &&
	    				   tableHand[Team2_1].NumberOfCards + tableHand[Team2_2].NumberOfCards == TradeInCount[1] )
	    				{
    						BroadcastAll(msg_ENABLEREADY, SERVER, "False");
    						
    						foreach(Client c in clients)
    						{
    							BroadcastAll(msg_CLEARTABLEHAND, SERVER, c.Id);
    							tableHand[clients.IndexOf(c)].Cards.Clear();
    						}
    						
    						foreach(Client c in clients)
    						{
    							Thread.Sleep(1000);
    							c.Hand = bottomDeck.Deal(c.Hand, 5-c.Hand.NumberOfCards);
    							c.Hand.Sort();
    							SendHandMessage(c, SERVER, false);
    						}
    						
    						if(TrumpRound1)
    						{
    							BroadcastAll(msg_HIDETABLEHAND, SERVER, clients[DealerPosition].Id);
    							BroadcastTo(msg_ENABLEHAND, SERVER, clients[DealerPosition].Id, "True");
    							
    							BroadcastTo(msg_INFO, SERVER, clients[DealerPosition].Id, "Discard a card, please.");
    							BroadcastAll(msg_INFO, clients[DealerPosition].Id, clients[DealerPosition].Name + " must discard a card.");
    							TurnIndex = 7;
    						}
    						else
    						{
	    						BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceUp.ToString());
	    						BroadcastAll(msg_TURNUPDATE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id);
	    						BroadcastTo(msg_EUCHREENABLE, SERVER, clients[clients.FindIndex(item => item.TableIndex == TableIndex)].Id, "True");
	    						TurnIndex = 5;
    						}
    					}
    				}
    				break;
    				
    			default:
    				break;
    		}
    	}
    }
}