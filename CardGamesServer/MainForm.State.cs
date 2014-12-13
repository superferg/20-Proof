using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using PlayingCards;
using System.Threading;

namespace CardGamesServer
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
    	
    	public int TurnIndex = 0, GameSelected = 0;
    	public int DealerPosition;
    	private States state = States.Startup;
    	private Action[] StateFunc, Game;
    	public int[] DiceRollValue = new int[4];
    	private List<int> DealerIndex = new List<int>{ 0, 1, 2, 3};
    	private List<int> LastGames = new List<int>();
    	private int RoundIndex = 0;
    	
    	public States State
    	{
    		get { return state; }
    		set 
    		{ 
    			state = value;
    			BroadcastAll(msg_STATECHANGE, SERVER, state.ToString());
    		}
    	}
    	
    	public void InitializeStates()
    	{
    		//Game = new Action[10]{PlayTableTalk, PlayPoker, PlayPoker, PlayPoker, PlayPoker, PlayPoker, PlayPoker, PlayPoker, PlayPoker, PlayWar};
    		Game = new Action[10]{PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk, PlayTableTalk};
    		
    		StateFunc = new Action[] {null, TeamFunc, DealFunc, GameFunc, PlayFunc, GameOver};
    	}
    	
    	public void PlayFunc()
    	{
    		Game[GameSelected]();
    	}
    	
    	public void GameFunc()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				Thread.Sleep(1500);
    				if(TeamScore[0] == 9 && TeamScore[1] == 9)
    				{
    					GameSelected = (int)Games.War;
    					Games foo = (Games)Enum.ToObject(typeof(Games) , GameSelected);
    					BroadcastAll(msg_INFO, SERVER, "SUDDEN DEATH, game tied at 9.");
	    				BroadcastAll(msg_SETGAMETYPE, SERVER, foo.ToString());
	    				
	    				Thread.Sleep(1500);
	    				State = States.Play;
	    				StateFunc[(int)State]();
    				}
    				else
    				{
	    				BroadcastAll(msg_DEALERUPDATE, SERVER, DealerPosition.ToString());
	    				BroadcastAll(msg_INFO, SERVER, "Roll to pick the game type.");
	    				TurnIndex++;
    				}
    				break;
    			case 1:
    				if(!LastGames.Contains(GameSelected) || GameSelected == (int)Games.War)
    				{
	    				Games foo = (Games)Enum.ToObject(typeof(Games) , GameSelected);
	    				BroadcastAll(msg_SETGAMETYPE, SERVER, foo.ToString());
	    				Thread.Sleep(1500);
	    				
	    				LastGames.Add(GameSelected);
	    				if(LastGames.Count() > 4)
	    				{
	    					LastGames.RemoveAt(0);
	    				}
	    				
	    				TurnIndex = 0;
	    				State = States.Play;
	    				StateFunc[(int)State]();
    				}
    				else
    				{
    					TurnIndex++;
    					BroadcastAll(msg_INFO, SERVER, "This game has been played within the last 4 rounds.  Roll again.");
    					GameFunc();
    				}
    				break;
    			case 2:
    				Thread.Sleep(1500);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, DealerPosition.ToString());
    				TurnIndex = 1;
    				break;
    			default:
    				break;
    		}
    		
    	}
    	
    	public void TeamFunc()
    	{
    		switch(TurnIndex)
    		{
    			case 0:
    				Thread.Sleep(1000);
    				BroadcastAll(msg_INFO, SERVER, "Roll to determine teams.");
    				Thread.Sleep(500);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, TurnIndex.ToString());
    				break;
    				
    			case 1:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, TurnIndex.ToString());
    				break;
    				
    			case 2:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, TurnIndex.ToString());
    				break;
    				
    			case 3:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, TurnIndex.ToString());
    				break;
    				
    			case 4:
    				IEnumerable<int> distinct = DiceRollValue.Distinct();
    				if(distinct.Distinct().Count() == DiceRollValue.Length)
    				{
    					int largest = int.MinValue;
						int second = int.MinValue;
						int largeIndex = 0, secondIndex = 0;
						for(int i=0; i < DiceRollValue.Length; i++)
						{
							if (DiceRollValue[i] > largest)
							{
								second = largest;
								secondIndex = largeIndex;
								
								largest = DiceRollValue[i];
								largeIndex = i;
							}
							else if (DiceRollValue[i] > second)
							{
								second = DiceRollValue[i];
								secondIndex = i;
							}
						}
						
						clients[largeIndex].Team = Teams.TeamOne;
						clients[secondIndex].Team = Teams.TeamOne;
						
						for(int i=0; i < clients.Count; i++)
						{
							if(i != largeIndex && i != secondIndex)
							{
								clients[i].Team = Teams.TeamTwo;
							}
						}
						
						Thread.Sleep(2000);
						BroadcastAll(msg_SETTEAMS, SERVER, clients[0].Team + ":" + 
						             clients[1].Team + ":" + clients[2].Team + ":" + clients[3].Team);
    				}
    				else if((distinct.Distinct().Count() == DiceRollValue.Length-1) ||
    				        DiceRollValue.FindAllIndexof(DiceRollValue[0]).Length == 2)
    				{
    					foreach(int i in DiceRollValue)
    					{
    						int[] index = DiceRollValue.FindAllIndexof(i);
	    					if(index.Length == 2)
	    					{
	    						for(int x=0; x < DiceRollValue.Length; x++)
	    						{
									clients[x].Team = index.Contains(x) ? Teams.TeamOne : Teams.TeamTwo;
	    						}
	    						break;
	    					}
    					}
    					
    					Thread.Sleep(2000);
    					BroadcastAll(msg_SETTEAMS, SERVER, clients[0].Team + ":" + 
						             clients[1].Team + ":" + clients[2].Team + ":" + clients[3].Team);
    				}
    				else
    				{
    					Array.Clear(DiceRollValue, 0, 4);
    					TurnIndex = 0;
    					BroadcastAll(msg_INFO, SERVER, "3 or more players rolled the same value. Restart the roll.");
    					TeamFunc();
    					break;
    				}
    				
    				int Team1Index = 0;
    				int Team2Index = 1;
    				for(int i=0; i < clients.Count; i++)
					{
    					if(clients[i].Team == Teams.TeamOne)
    					{
    						clients[i].TableIndex = Team1Index;
    						Team1Index += 2;
    					}
    					else
    					{
    						clients[i].TableIndex = Team2Index;
    						Team2Index += 2;
    					}
    				}
    				BroadcastAll(msg_SETTBLPOS, SERVER, clients[0].TableIndex + ":" + clients[1].TableIndex + 
    				             ":" + clients[2].TableIndex + ":" + clients[3].TableIndex);
    				
    				State = States.Dealer;
    				TurnIndex = 0;
    				Array.Clear(DiceRollValue, 0, 4);
    				StateFunc[(int)State]();
    				break;
    			default:
    				break;
    		}
    	}
    	
    	public void DealFunc()
    	{
    		while((!DealerIndex.Contains(TurnIndex)) && (TurnIndex != 4))
    		{
    			TurnIndex++;
    		}
    		
    		int PlayerIndex = clients.FindIndex(item => item.TableIndex == TurnIndex);
    		
    		switch(TurnIndex)
    		{
    			case 0:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_INFO, SERVER, "Roll to determine who deals first.");
    				Thread.Sleep(500);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, PlayerIndex.ToString());
    				break;
    				
    			case 1:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, PlayerIndex.ToString());
    				break;
    				
    			case 2:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, PlayerIndex.ToString());
    				break;
    				
    			case 3:
    				Thread.Sleep(2000);
    				BroadcastAll(msg_DEALERUPDATE, SERVER, PlayerIndex.ToString());
    				break;
    				
    			case 4:
    				if(DiceRollValue.FindAllIndexof(DiceRollValue.Max()).Length == 1)
    				{
    					int indexAtMax = DiceRollValue.ToList().IndexOf(DiceRollValue.Max());
    					BroadcastAll(msg_SETDEALER, SERVER, clients.FindIndex(item => item.TableIndex == indexAtMax).ToString());
    					
    					DealerPosition = clients.FindIndex(item => item.TableIndex == indexAtMax);
    					TurnIndex = 0;
    					State = States.Game;
    					ResetDealerIndex();
    					StateFunc[(int)State]();
    				}
    				else
    				{
    					DealerIndex = DiceRollValue.FindAllIndexof(DiceRollValue.Max()).ToList();
    					TurnIndex = 0;
    					Array.Clear(DiceRollValue, 0, 4);
    					BroadcastAll(msg_INFO, SERVER, "2 or more players rolled the same high value. They will re-roll.");
    					DealFunc();
    				}
    				break;
    				
    			default:
    				break;
    		}
    	}
    	
    	private void GameOver()
    	{
    		if(TeamScore[(int)Teams.TeamOne] >= 10)
    		{
    			BroadcastAll(msg_INFO, SERVER, "Game Over, Team 1 has won.");
    		}
    		else
    		{
    			BroadcastAll(msg_INFO, SERVER, "Game Over, Team 2 has won.");
    		}
    	}
    	
    	public void ResetDealerIndex()
	    {
    		DealerIndex.Clear();
    		DealerIndex.Add(1);
    		DealerIndex.Add(2);
    		DealerIndex.Add(3);
    		DealerIndex.Add(4);
	    }
    }
    
    public static class EM
	{
	    public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
	    {
	        return values.Select((b,i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
	    }
	    
	    public static int[] FindAllIndexof<T>(this IEnumerable<T> values, Expression<Func<T,bool>> predicate)
	    {
	        return values.Select((b,i) => object.Equals(b, predicate) ? i : -1).Where(i => i != -1).ToArray();
	    }
	}
}