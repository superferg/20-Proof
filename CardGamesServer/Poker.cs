using System;
using System.Linq;
using PlayingCards;
using System.Threading;
using System.Text.RegularExpressions;

namespace CardGamesServer
{
    partial class MainForm
    {
    	public void PlayPoker()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				BroadcastAll(msg_SETMYHAND, SERVER, FacingSides.FaceUp.ToString());
		    		BroadcastAll(msg_SETOTHERHANDS, SERVER, FacingSides.FaceDown.ToString());
		    		BroadcastAll(msg_SETTABLEHANDS, SERVER, FacingSides.FaceDown.ToString());
    		
    				Shuffle();
    				DealCards(true);
    				
    				Thread.Sleep(500);
    				foreach(Client cl in clients)
    				{
    					cl.IsReady = false;
    				}
    				
    				BroadcastAll(msg_INFO, SERVER, "Decide with your partner how many cards to trade.");
    				BroadcastAll(msg_ENABLEHAND, SERVER, "True");
    				BroadcastAll(msg_ENABLETABLEHAND, SERVER, "True");
    				BroadcastAll(msg_ENABLEREADY, SERVER, "True");
    				TurnIndex++;
    				break;
    				
    			case 1:
    				if(clients[0].IsReady == true && clients[1].IsReady == true &&
    				   clients[2].IsReady == true && clients[3].IsReady == true)
    				{
    					int Team1_1 = clients.FindIndex(item => item.Team == Teams.TeamOne);
    					int Team1_2 = clients.FindLastIndex(item => item.Team == Teams.TeamOne);
    					int Team2_1 = clients.FindIndex(item => item.Team == Teams.TeamTwo);
    					int Team2_2 = clients.FindLastIndex(item => item.Team == Teams.TeamTwo);
    				
	    				if(tableHand[Team1_1].NumberOfCards == tableHand[Team1_2].NumberOfCards &&
	    				   tableHand[Team2_1].NumberOfCards == tableHand[Team2_2].NumberOfCards)
	    				{
	    					clients[Team1_2].Hand = tableHand[Team1_1].Pass(clients[Team1_2].Hand);
	    					clients[Team1_1].Hand = tableHand[Team1_2].Pass(clients[Team1_1].Hand);
	    					
	    					clients[Team2_1].Hand = tableHand[Team2_2].Pass(clients[Team2_1].Hand);
	    					clients[Team2_2].Hand = tableHand[Team2_1].Pass(clients[Team2_2].Hand);
	    					
	    					BroadcastAll(msg_ENABLEREADY, SERVER, "False");
	    					
	    					foreach(Client c in clients)
	    					{
	    						BroadcastAll(msg_CLEARTABLEHAND, SERVER, c.Id);
	    					}
	    					
	    					foreach(Client c in clients)
	    					{
	    						SendHandMessage(c, SERVER, false);
	    					}
	    					
	    					Thread.Sleep(2000);
	    					BroadcastAll(msg_SETOTHERHANDS, SERVER, FacingSides.FaceUp.ToString());
	    					foreach(Client c in clients)
	    					{
	    						SendHandMessage(c, SERVER, false);
	    					}
	    					
	    					int Compare1 = PokerLogic.CompareHands(clients[Team1_1].Hand, clients[Team2_1].Hand);
	    					int Compare2 = PokerLogic.CompareHands(clients[Team1_1].Hand, clients[Team2_2].Hand);
	    					int Compare3 = PokerLogic.CompareHands(clients[Team1_2].Hand, clients[Team2_1].Hand);
	    					int Compare4 = PokerLogic.CompareHands(clients[Team1_2].Hand, clients[Team2_2].Hand);
	    					string newString = String.Empty;
	    					
	    					if(Compare1 == 1 && Compare2 == 1)
	    					{
	    						TeamScore[(int)Teams.TeamOne]++;
	    						
	    						if(Compare3 == 1 && Compare4 == 1)
	    						{
	    							TeamScore[(int)Teams.TeamOne]++;
	    							newString = Teams.TeamOne + " won 2 points. ";
	    							newString += clients[Team1_1].Name + " with a " + PokerLogic.score(clients[Team1_1].Hand).ToString() + ". ";
	    							newString += clients[Team1_2].Name + " with a " + PokerLogic.score(clients[Team1_2].Hand).ToString() + ".";
	    						}
	    						else
	    						{
	    							newString = Teams.TeamOne + " won 1 point. ";
	    							newString += clients[Team1_1].Name + " with a " + PokerLogic.score(clients[Team1_1].Hand).ToString() + ".";
	    						}
	    					}
	    					else if(Compare3 == 1 && Compare4 == 1)
	    					{
	    						TeamScore[(int)Teams.TeamOne]++;
	    						newString = Teams.TeamOne + " won 1 point. ";
	    						newString += clients[Team1_2].Name + " with a " + PokerLogic.score(clients[Team1_2].Hand).ToString() + ".";
	    					}
	    					else if(Compare1 == 2 && Compare2 == 2)
	    					{
	    						TeamScore[(int)Teams.TeamTwo]++;
	    						
	    						if(Compare3 == 2 && Compare4 == 2)
	    						{
	    							TeamScore[(int)Teams.TeamTwo]++;
	    							newString = Teams.TeamTwo + " won 2 points. ";
	    							newString += clients[Team2_1].Name + " with a " + PokerLogic.score(clients[Team2_1].Hand).ToString() + ". ";
	    							newString += clients[Team2_2].Name + " with a " + PokerLogic.score(clients[Team2_2].Hand).ToString() + ".";
	    						}
	    						else
	    						{
	    							newString = Teams.TeamTwo + " won 1 point. ";
	    							newString += clients[Team2_1].Name + " with a " + PokerLogic.score(clients[Team2_1].Hand).ToString() + ".";
	    						}
	    					}
	    					else if(Compare3 == 2 && Compare4 == 2)
	    					{
	    						TeamScore[(int)Teams.TeamTwo]++;
	    						newString = Teams.TeamTwo + " won 1 point. ";
	    						newString += clients[Team2_1].Name + " with a " + PokerLogic.score(clients[Team2_1].Hand).ToString() + ".";
	    					}
	    					else
	    					{
	    						POKERSCORE Max = POKERSCORE.HighCard;
	    						foreach(Client c in clients)
	    						{
	    							if(PokerLogic.score(c.Hand) > Max)
	    							{
	    								Max = PokerLogic.score(c.Hand);
	    							}
	    						}
	    						
	    						newString = "There is a tie with a " + Max + ", no points have been awarded.";
	    					}
	    					
	    					BroadcastAll(msg_SETSCORE, SERVER, TeamScore[0] + ":" + TeamScore[1]);
	    					
	    					newString = Regex.Replace(newString, @"\B([A-Z])", " $1");
	    					BroadcastAll(msg_INFO, SERVER, newString);
	    					Thread.Sleep(8000);
	    					
	    					BroadcastAll(msg_RESETTABLE, SERVER, "");
	    					ClearTable();
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
    				}
		    		break;
		    		
		    	default:
		    		break;
    		}
    	}
    }
}