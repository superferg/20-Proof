using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Timers;

using PlayingCards;

namespace CardGamesServer
{
    public partial class MainForm : Form
    {
        TcpListener listener;
        Socket socket;
        List<Client> clients = new List<Client>();
        Queue<string> msgQueue = new Queue<string>();

        Dealer dealer = new Dealer();
        Dealer bottomDeck = new Dealer();
        Hand[] tableHand = new Hand[4];
        Dice _Dice = new Dice();
        int[] TeamScore = new int[2]{0,0};
        int[] TrickCount = new int[2]{0,0};
        
        System.Timers.Timer timer = new System.Timers.Timer();

        #region Constant Strings

        private string CRLF = Environment.NewLine;

        private const string SERVER = "HOST";
        private const string msg_OK = "OK";
        private const string msg_ERR = "ERROR";
        private const string msg_CHAT = "CHAT";
        private const string msg_PRIVCHAT = "PRIVCHAT";
        private const string msg_GOODBYE = "GOODBYE";
        private const string msg_CLIENTID = "CLIENTID";
        private const string msg_SERVERSHUTDOWN = "SERVERSHUTDOWN";
        private const string msg_CLIENTDISCONNECTED = "CLIENTDISCONNECTED";
        private const string msg_CLIENTCONNECTED = "CLIENTCONNECTED";
        private const string msg_YOURID = "YOURID";
        private const string msg_NAMEREGISTERED = "NAMEREGISTERED";
        private const string msg_DECKCOUNT = "DECKCOUNT";
        private const string msg_HANDTOTABLE = "HANDTOTABLE";
        private const string msg_TABLETOHAND = "TABLETOHAND";
        private const string msg_DECKTOTABLE = "DECKTOTABLE";
        private const string msg_INFO = "INFO";
        private const string msg_DICEROLLED = "DICEROLLED";
        private const string msg_DICECHANGE = "DICECHANGED";
        private const string msg_ROOMFULL = "ROOMFULL";
        private const string msg_READY = "READY";
        private const string msg_NOTREADY = "NOTREADY";
        private const string msg_DEALERUPDATE = "DEALERUPDATE";
        private const string msg_TURNUPDATE = "TURNUPDATE";
        private const string msg_STATECHANGE = "STATECHANGE";
        private const string msg_SETTEAMS = "SETTEAMS";
        private const string msg_SETTBLPOS = "SETTBLPOS";
        private const string msg_SETDEALER = "SETDEALER";
        private const string msg_SETGAMETYPE = "SETGAMETYPE";
        private const string msg_SETSCORE = "SETSCORE";
        private const string msg_SETTRICKS = "SETTRICKS";
        private const string msg_SENDHAND = "SENDHAND";
        private const string msg_SENDDECK = "SENDDECK";
        private const string msg_SETMYHAND = "SETMYHAND";
        private const string msg_SETOTHERHANDS = "SETOTHERHANDS";
        private const string msg_SETTABLEHANDS = "SETTABLEHANDS";
        private const string msg_ENABLEREADY = "ENABLEREADY";
        private const string msg_ENABLEHAND = "ENABLEHAND";
        private const string msg_ENABLETABLEHAND = "ENABLETABLEHAND";
        private const string msg_CLEARTABLEHAND = "CLEARTABLEHAND";
        private const string msg_RESETTABLE = "RESETTABLE";
        private const string msg_CALLTRUMP = "CALLTRUMP";
        private const string msg_CALLTRUMP2 = "CALLTRUMP2";
        private const string msg_HIDEDECK = "HIDEDECK";
        private const string msg_HIDETABLEHAND = "HIDETABLEHAND";
        private const string msg_SHOWTABLEHAND = "SHOWTABLEHAND";
        private const string msg_EUCHREENABLE = "EUCHREENABLE";
        private const string msg_SETTRUMP = "SETTRUMP";
        private const string msg_SETWINNERINDEX = "SETWINNERINDEX";
        private const string msg_ROLLTRADEIN = "ROLLTRADEIN";
        #endregion

        public MainForm()
        {
            InitializeComponent();
            
            InitializeDice(); 
            InitializeStates();
            
            for(int i=0; i < tableHand.Length; i++)
            {
            	tableHand[i] = new Hand();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            timer.Elapsed += new ElapsedEventHandler(PollForConnections);
            timer.Elapsed += new ElapsedEventHandler(ProcessMessageQueue);
            
            Shuffle();
        }
        
        private void MainForm_Closed(object sender, EventArgs e)
        {
			ShutdownClientSockets();
		}

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Interval = 100;       
     
            if (button1.Text == "Start")
            {
                dealer.InitializePokerDeck(CardValues.Nine, CardValues.Ace);
                dealer.Shuffle();

                textBox1.AppendText(CRLF + "Waiting for connections..");

                listener = new TcpListener(System.Net.IPAddress.Any, 4994);
                listener.Start();
                timer.Start();
                button1.Text = "Stop";
                
            }
            else
            {
                timer.Stop();
                listener.Stop();
                BroadcastAll(msg_SERVERSHUTDOWN, SERVER, "");
                ShutdownClientSockets();

                textBox1.AppendText(clients.Count.ToString() + " clients." + CRLF);
                textBox1.AppendText("Server stopped." + CRLF);
                button1.Text = "Start";
            }
        }

        private void PollForConnections(object source, ElapsedEventArgs e)
        {
            try
            {
                if (listener.Pending())
                {
                    socket = listener.AcceptSocket();

                    if (socket.Connected && clients.Count < 4)
                    {
                        Client client = new Client();
                        client.OnClientReceiveData += new ClientEventHandler(client_OnClientReceiveData);
                        client.OnClientDisconnected += new ClientEventHandler(client_OnClientDisconnected);
                        client.OnClientConnected += new ClientEventHandler(client_OnClientConnected);
                        client.AcceptSocket(socket);

                        clients.Add(client);
                        listener.Stop();

                        BroadcastTo(msg_YOURID, SERVER, client.Id, client.Id);
                        textBox1.AppendText(clients.Count.ToString() + " clients." + CRLF);

                        listener.Start();
                    }
                    else
                    {
                    	byte[] buffer = System.Text.Encoding.ASCII.GetBytes(SERVER + ":" + msg_ROOMFULL + ":" + CRLF);
                    	this.socket.Send(buffer);
                    	
                    	this.socket.Close();
                    	listener.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ProcessMessageQueue(object source, ElapsedEventArgs e)
        {
            if (msgQueue.Count > 0)
            {
                string msg = msgQueue.Dequeue();

                string[] parsedMsg = SplitQuoted(msg, ":");
                string clientId = "";
                string command = "";

                try
                {
                    clientId = parsedMsg[0];
                    command = parsedMsg[1];

                    switch (command.ToUpper())
                    {
                        case "NAME":
                            {
                                Client c = GetClient(clientId);
                                c.Name = parsedMsg[2];
                                BroadcastAll(msg_NAMEREGISTERED, SERVER, clientId + ":" + c.Name);
                                break;
                            }
                        case "CHAT":
                            BroadcastAll(msg_CHAT, clientId, parsedMsg[2]);
                            break;
                        case "BYE":
                            ShutdownClientSocket(clientId);
                            break;
                        case "WHO":
                            string message = clients.Count.ToString();
                            foreach (Client client in clients)
                            	message += ":" + client.Id + ":" + client.Name + ":" + client.IsReady;
                            BroadcastTo(msg_CLIENTID, SERVER, clientId, message);
                            break;
                        case "HANDTOTABLE":
                            {
                                Client c = GetClient(clientId);
                                int index = clients.IndexOf(c);
                                Card card = clients[index].Hand.GetCard(parsedMsg[2]);
                                tableHand[index].Cards.Add(card);
                                BroadcastAll(msg_HANDTOTABLE, clientId, parsedMsg[2]);
                                BroadcastTo(msg_EUCHREENABLE, SERVER, clientId, "False");
                                
                                if((State == States.Play && GameSelected == (int)Games.Poker) ||
                                  (State == States.Play && GameSelected == (int)Games.TradeIn && TurnIndex == 8))
                                {
                                	SendHandMessage(clients[index], clientId, true);
                                }
                                else
                                {
                                	SendHandMessage(clients[index], clientId, false);
                                }
                                
                                StateFunc[(int)State]();
                                break;
                            }
                        case "TABLETOHAND":
                            {
                                Client c = GetClient(clientId);
                                int index = clients.IndexOf(c);
                                Card card = tableHand[index].GetCard(parsedMsg[2]);
                                clients[index].Hand.Cards.Add(card);
                                BroadcastAll(msg_TABLETOHAND, clientId, parsedMsg[2]);
                                SendHandMessage(clients[index], clientId, true);
                                break;
                            }
                        case "ROLLDICE":
                            {
                                _Dice.Roll();
                                break;
                            }
                        case "READY":
                            {
                            	Client c = GetClient(clientId);
                            	clients[clients.IndexOf(c)].IsReady = true;
                            	BroadcastAll(msg_READY, clientId, "");
                            	
                            	if(clients.Count == 4 && clients[0].IsReady == true && clients[1].IsReady == true && 
                            	   clients[2].IsReady == true && clients[3].IsReady == true)
                            	{
                            		if(State == States.Startup)
                            		{
                            			State = States.Teams;
                            		}
                            		
                            		StateFunc[(int)State]();
                            	}
                            	break;
                            }
                        case "NOTREADY":
                            {
                            	Client c = GetClient(clientId);
                            	clients[clients.IndexOf(c)].IsReady = false;
                            	BroadcastAll(msg_NOTREADY, clientId, "");
                            	break;
                            }    
                        case "CALLTRUMP":
                            {
                            	PickedUp = Convert.ToBoolean(parsedMsg[2]);
                            	StateFunc[(int)State]();
                            	break;
                            }  
                        case "CALLTRUMP2":
                            {
                            	PickedUp = Convert.ToBoolean(parsedMsg[2]);
                            	
                            	if(parsedMsg[3] != "Pass")
                            		TrumpSuit = (CardSuits)Enum.Parse(typeof(CardSuits), parsedMsg[3]);
                            	
                            	StateFunc[(int)State]();
                            	break;
                            } 
                        default:
                            BroadcastTo(msg_ERR, SERVER, clientId, msg);
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                	BroadcastTo(msg_ERR, SERVER, clientId, msg);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "EmptyDeckException")
                        BroadcastTo(msg_INFO, SERVER, clientId, "The deck is empty.");
                }

            }
        }

        private Client GetClient(string clientId)
        {
            foreach (Client c in clients)
                if (c.Id == clientId)
                    return c;

            return null;
        }
        
        private Client GetClient(int TableIndex)
        {
            foreach (Client c in clients)
                if (c.TableIndex == TableIndex)
                    return c;

            return null;
        }

        public string[] SplitQuoted(string text, string delimiters)
        {
            // Default delimiters are a space and tab (e.g. " \t").
            // All delimiters not inside quote pair are ignored. 
            // Default quotes pair is two double quotes ( e.g. '""' ).
            if (text == null)
                throw new ArgumentNullException("text", "text is null.");
            if (delimiters == null || delimiters.Length < 1)
                delimiters = " \t"; // Default is a space and tab.

            ArrayList res = new ArrayList();

            // Build the pattern that searches for both quoted and unquoted elements
            // notice that the quoted element is defined by group #2 (g1)
            // and the unquoted element is defined by group #3 (g2).

            string pattern =
             @"""([^""\\]*[\\.[^""\\]*]*)""" +
             "|" +
             @"([^" + delimiters + @"]+)";

            // Search the string.
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, pattern))
            {
                //string g0 = m.Groups[0].Value;
                string g1 = m.Groups[1].Value;
                string g2 = m.Groups[2].Value;
                if (!String.IsNullOrEmpty(g2))
                {
                    res.Add(g2);
                }
                else
                {
                    // get the quoted string, but without the quotes in g1;
                    res.Add(g1);
                }
            }
            return (string[])res.ToArray(typeof(string));
        } 

        private void BroadcastAll(string msgType, string fromClientId, string msg)
        {
            foreach (Client c in clients)
                if (c.Id != fromClientId)
                    c.SendMessage(fromClientId + ":" + msgType + ":" + msg + CRLF);
        }

        private void BroadcastTo(string msgType, string fromClientId, string toClientId, string msg)
        {
            Client c = GetClient(toClientId);
            if (c != null)
                c.SendMessage(fromClientId + ":" + msgType + ":" + msg + CRLF);
        }

        private void ShutdownClientSockets()
        {
            for (int x = clients.Count - 1; x >= 0; x--)
            {
                clients[x].Socket.Close();
                clients.Remove(clients[x]);
            }
        }

        private void ShutdownClientSocket(string clientId)
        {
            BroadcastTo(msg_GOODBYE, SERVER, clientId, "");

            Client c = GetClient(clientId);
            c.Socket.Close();
            clients.Remove(c);

            BroadcastAll(msg_CLIENTDISCONNECTED, SERVER, clientId);

            textBox1.AppendText(clients.Count.ToString() + " clients." + CRLF);
        }

        private void client_OnClientReceiveData(Client client, ClientEventArgs args)
        {
            string msg = client.Id + ":" + args.LastMessage;

            msgQueue.Enqueue(msg);
        }

        private void client_OnClientDisconnected(Client client, ClientEventArgs args)
        {
            Client c = GetClient(client.Id);
            clients.Remove(c);
            BroadcastAll(msg_CLIENTDISCONNECTED, SERVER, client.Id);

            textBox1.AppendText(clients.Count.ToString() + " clients." + CRLF);

        }

        private void client_OnClientConnected(Client client, ClientEventArgs args)
        {
            Client c = GetClient(client.Id);
            BroadcastAll(msg_CLIENTCONNECTED, SERVER, client.Id );
        }
        
        private void InitializeDice()
        {
        	_Dice = new Dice();
        	_Dice.Maximum = 10;
        	_Dice.RollingChanged += OnDiceRollingChanged;
        	_Dice.Rolled += OnDiceRolled;
        }

        void OnDiceRolled(object sender, EventArgs e)
        {
        	BroadcastAll(msg_DICEROLLED, SERVER, _Dice.Result.ToString());
        	
        	if(State == States.Game)
        	{
        		GameSelected = _Dice.Result - 1;
        	}
        	else
        	{
        		DiceRollValue[TurnIndex++] = _Dice.Result;
        	}
        	
        	StateFunc[(int)State]();
        }

        void OnDiceRollingChanged(object sender, EventArgs e)
        {
        	BroadcastAll(msg_DICECHANGE, SERVER, _Dice.Result.ToString());
        }
        
        private void Shuffle()
        {
        	dealer.InitializePokerDeck(CardValues.Nine, CardValues.Ace);
            dealer.Shuffle();
            
            bottomDeck.InitializePokerDeck(CardValues.Deuce, CardValues.Eight);
            bottomDeck.Shuffle();
        }
        
        private void DealCards(bool Sorting)
        {
        	for(int i=0; i < clients.Count; i++)
        	{
        		int Index = clients[DealerPosition].TableIndex + i + 1;
        		if(Index > 3)
        			Index -= 4;
        		
        		int PlayerIndex = clients.FindIndex(item => item.TableIndex == Index);
        		
        		if(i % 2 == 0)
        		{
        			clients[PlayerIndex].Hand = dealer.Deal(clients[PlayerIndex].Hand, 3);
        			SendHandMessage(clients[PlayerIndex], SERVER, false);
        			Thread.Sleep(1000);
        		}
        		else
        		{
        			clients[PlayerIndex].Hand = dealer.Deal(clients[PlayerIndex].Hand, 2);
        			SendHandMessage(clients[PlayerIndex], SERVER, false);
        			Thread.Sleep(1000);
        		}
        	}
        	
        	for(int i=0; i < clients.Count; i++)
        	{
        		int Index = clients[DealerPosition].TableIndex + i + 1;
        		if(Index > 3)
        			Index -= 4;
        		
        		int PlayerIndex = clients.FindIndex(item => item.TableIndex == Index);
        		
        		if(i % 2 == 0)
        		{
        			clients[PlayerIndex].Hand = dealer.Deal(clients[PlayerIndex].Hand, 2);
        			SendHandMessage(clients[PlayerIndex], SERVER, false);
        			Thread.Sleep(1000);
        		}
        		else
        		{
        			clients[PlayerIndex].Hand = dealer.Deal(clients[PlayerIndex].Hand, 3);
        			SendHandMessage(clients[PlayerIndex], SERVER, false);
        			Thread.Sleep(1000);
        		}
        	}
        	
        	for(int i=0; i < clients.Count; i++)
        	{
        		if(Sorting) clients[i].Hand.Sort();
        		SendHandMessage(clients[i], SERVER, false);
        	}
        }
        
        private void SendHandMessage(Client c, string fromClientId, bool Enable)
        {
        	string msg = c.Id + ":" + c.Hand.NumberOfCards;
        	foreach (Card card in c.Hand.Cards)
        		msg += ":" + card.TextValue;
        	
        	BroadcastAll(msg_SENDHAND, fromClientId, msg);
        	BroadcastTo(msg_ENABLEHAND, SERVER, c.Id, Enable.ToString());
        }
        
        private void IncrementDealer()
        {
        	int tableIndex = clients[DealerPosition].TableIndex + 1;
        	
        	if(tableIndex > 3)
        		tableIndex -= 4;
        	
        	DealerPosition = clients.FindIndex(item => item.TableIndex == tableIndex);
        }
        
        private void ClearTable()
        {
        	for(int i=0; i < 4; i++)
        	{
        		clients[i].Hand.Cards.Clear();
        		tableHand[i].Cards.Clear();
        	}
        }
    }
}
