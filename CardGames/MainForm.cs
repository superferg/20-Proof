using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;

using PlayingCards;

namespace CardGames
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
            HandBox = new PictureBox[,] {{myHandBox1, myHandBox2, myHandBox3, myHandBox4, myHandBox5},
								            {hand2Box1, hand2Box2, hand2Box3, hand2Box4, hand2Box5},
								            {hand3Box1, hand3Box2, hand3Box3, hand3Box4, hand3Box5},
								            {hand4Box1, hand4Box2, hand4Box3, hand4Box4, hand4Box5}};
            
            tableBox = new PictureBox[] {mytableHand1, tableHand2, tableHand3, tableHand4};
            dieBox = new PictureBox[] {dieBox1, dieBox2, dieBox3, dieBox4};
            dealBox = new PictureBox[] {dealBox1, dealBox2, dealBox3, dealBox4};
            deckCardBox = new PictureBox[] {deckCard1, deckCard2, deckCard3, deckCard4};
            nameLabel = new RotatingLabel[] {null, rotatingLabel1, rotatingLabel2, rotatingLabel3};
            
            for(int i=0; i < tableHands.Length; i++)
            {
            	tableHands[i] = new Hand();
            }
        }

        private delegate void AnonymousDelegate();
        private delegate void DrawCardDelegate(Card card);
        private delegate void CardToTableDelegate(int TableIndex);
        private delegate void RemoveCardFromTableDelegate(Card card);
        private delegate void UpdateNameLabel(Player p);

        System.Timers.Timer timer = new System.Timers.Timer();
        Queue<string> msgQueue = new Queue<string>();
        Socket socket = null;
        private string incomingBuffer;
        private bool diceFlag = false;
        private int DealerIndex = 0;

        private string clientId = "";
        private string clientName = "";
        private int clientIndex = -1;
        private bool EuchreEnable = false;
        
        private FacingSides myHandSide = FacingSides.FaceUp;
        private FacingSides otherHandSide = FacingSides.FaceDown;
        private FacingSides tableHandSide = FacingSides.FaceUp;

        PictureBox[,] HandBox;
        PictureBox[] tableBox;
        PictureBox[] dieBox;
        PictureBox[] dealBox, deckCardBox;
        RotatingLabel[] nameLabel;
        Hand[] tableHands = new Hand[4];
        List<Player> players = new List<Player>();
        Dealer dealer = new Dealer();

        private string selectedCardName = "";

        private void Form1_Load(object sender, EventArgs e)
        {
        	InitializeNames();
        	
        	//We need to turn this off as the OnPlayerConnect event will break if we dont.
            CheckForIllegalCrossThreadCalls = false;

            timer.Elapsed += new ElapsedEventHandler(ProcessMessageQueue);
            timer.Elapsed += new ElapsedEventHandler(monitorSocketStatus);

            dealer.InitializePokerDeck();
            dealer.Shuffle();
        }
        
        void Form1_Closed(object sender, FormClosedEventArgs e)
		{
			Disconnect("");
		}

        private void mouseDownHandler(object sender, MouseEventArgs e)
        {
        	Point p = Cursor.Position;
            string destination = determineClickSource(p);
            
            // If we are just now selecting a card
            if (selectedCardName == "")
            {
                //..and it is indeed a card
                if (sender.GetType() == typeof(System.Windows.Forms.PictureBox) && ((PictureBox)sender).Image != null)
                {
                    if(destination == "hand")
                	{
                    	UpdateCursor(((PictureBox)sender).Image);
                    	
	                    int index = Convert.ToInt32(((PictureBox)sender).Name.Substring(((PictureBox)sender).Name.Length-1));
	                    selectedCardName = players[clientIndex].Hand.Cards[index-1].TextValue;
	                    ((PictureBox)sender).Image = null;
                    }
                    else if(destination == "table")
                    {
                    	selectedCardName = tableHands[0].PeekTopCard().TextValue;
                    	
                    	UpdateCursor(((PictureBox)sender).Image);
                    	
                    	if(tableHands[0].Cards.Count > 1)
                    	{
                    		((PictureBox)sender).Image = tableHands[0].Cards[tableHands[0].Cards.Count - 2].FacingImage;
                    	}
                    	else
                    	{
                    		((PictureBox)sender).Image = null;
                    	}
                    }
                }
            }
            else
            {
                //Turn on regular cursor
                UpdateCursor(null);

                //Get the card face
                Card selectedCard = players[clientIndex].Hand.PeekCard(selectedCardName) ?? tableHands[0].PeekCard(selectedCardName);
                bool inHand = players[clientIndex].Hand.Exists(selectedCard);

                //If we pick up a card in hand and put it back
                if (inHand && destination == "hand")
                	DisplayHand(players[clientIndex]);

                if (!inHand && destination == "table")
                	DisplayTableHand(0);

                //If we are moving to the table...
                if (inHand && destination == "table")
                    MoveHandCardToTable(selectedCard);

                if (!inHand && destination == "hand")
                    MoveTableCardToHand(selectedCard);
                
                selectedCardName = "";
            }
        }

        private string determineClickSource(Point p)
        {
            int y = PointToClient(p).Y;

			return y > 504 ? "hand" : "table";
        }

        private void UpdateCursor(Image image)
        {
            if (image != null)
            {
                Bitmap b = new Bitmap(image);
                IntPtr ptr = b.GetHicon();
                Cursor c = new Cursor(ptr);
                this.Cursor = c;
                this.Refresh();
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }
        
        private void updateMsgLog(string msg)
        {
            txtMsgLog.SelectionStart = txtMsgLog.Text.Length;
            txtMsgLog.ScrollToCaret();
            txtMsgLog.SelectedText += Environment.NewLine + msg;
        }

        private void DisplayHand(Player p)
        {
            for(int i = 0; i < 5; i++)
            {
            	if(i < p.Hand.NumberOfCards)
            	{
	            	Image bitmap1 = p.Hand.Cards[i].FacingImage;
	            	
	            	if(p.TableIndex == 1 || p.TableIndex == 3)
	            	{
	            		HandBox[p.TableIndex,i].Image = p.Hand.Cards[i].RotatedFacingImage;
	            	}
	            	else
	            	{
	            		HandBox[p.TableIndex,i].Image = p.Hand.Cards[i].FacingImage;
	            	}
            	}
            	else
            	{
            		HandBox[p.TableIndex,i].Image = null;
            	}
            }
            
            if(EuchreEnable && p.Id == clientId)
            {
            	EuchreValid();
            }
            else if(p.Id == clientId)
            {
            	EuchreValidReset();
            }
        }
        
        private void DisplayTableHand(int TableIndex)
        {
        	if(tableHands[TableIndex].NumberOfCards > 0)
        	{
        		if(TableIndex == 1 || TableIndex == 3)
        		{
        			tableBox[TableIndex].Image = tableHands[TableIndex].PeekTopCard().RotatedFacingImage;
        		}
        		else
        		{
        			tableBox[TableIndex].Image = tableHands[TableIndex].PeekTopCard().FacingImage;
        		}
        	}
        	else
        	{
        		tableBox[TableIndex].Image = null;
        	}
        }

        private void MoveHandCardToTable(Card card)
        {
        	//Move actual card object from myhand to table
        	card.FacingSide = tableHandSide;
            tableHands[0] = players[clientIndex].Hand.Pass(tableHands[0], card);
            tableBox[0].Image = tableHands[0].PeekTopCard().FacingImage;
            SendMessage("HANDTOTABLE:" + card.TextValue);
            
            DisplayHand(players[clientIndex]);
            EnableMyHand(false);
        }

        private void MoveTableCardToHand(Card card)
        {
            //Move actual card object from table to hand
            card.FacingSide = myHandSide;
            players[clientIndex].Hand = tableHands[0].Pass(players[clientIndex].Hand, card);
            
            int index = players[clientIndex].Hand.Cards.IndexOf(card);
            HandBox[0, index].Image = players[clientIndex].Hand.Cards[index].FacingImage;
            
            if(tableHands[0].NumberOfCards > 0)
            {
	            tableBox[0].Image = tableHands[0].PeekTopCard().FacingImage;
            }
            
            EnableMyHand(false);
            SendMessage("TABLETOHAND:" + card.TextValue);
        }

        #region PictureBox Events

        private void OnButtonRollClick(object sender, EventArgs e)
        {
        	dieBox1.Enabled = false;
        	SendMessage("ROLLDICE");
        }

        #endregion

        #region Menu Events

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutWindow = new AboutBox1();
            aboutWindow.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectForm connectForm = new ConnectForm();
            connectForm.ShowDialog();

            if (!String.IsNullOrEmpty(connectForm.IPAddress))
            {
                clientName = connectForm.ClientName;
                
                // Truncate the name to a max of 8 characters
                if(clientName.Length > 8)
                {
                	clientName = clientName.Split(' ')[0];
                	
                	if(clientName.Length > 8)
                		clientName = clientName.Substring(0, 8);
                }
                
                Connect(connectForm.IPAddress);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect("Manually disconnected.");
        }

        #endregion

        #region Button Events

        private void btnSend_Click(object sender, EventArgs e)
        {
            KeyEventArgs args = new KeyEventArgs(Keys.Enter);
            txtMessage_KeyDown(null, args);
        }

        void BtnClearClick(object sender, EventArgs e)
		{
        	txtMsgLog.Clear();
		}
        
        void ReadyBtnClick(object sender, EventArgs e)
		{
        	if(ReadyBtn.Text == "Ready")
        	{
        		SendMessage("READY");
        		ReadyBtn.Text = "Unready";
        		
        		((PlayerUserControl)panel3.Controls[FindPUCIndex(players[clientIndex])]).ShowReady = true;
        		
        		if(state == States.Play)
        		{
        			tableBox[0].Enabled = false;
        			for(int i=0; i < 5; i++)
        			{
        				HandBox[0,i].Enabled = false;
        			}
        		}
        	}
        	else
        	{
        		SendMessage("NOTREADY");
        		ReadyBtn.Text = "Ready";
        		
        		if(state == States.Play)
        		{
        			tableBox[0].Enabled = true;
        			for(int i=0; i < 5; i++)
        			{
        				HandBox[0,i].Enabled = true;
        			}
        		}
        		
        		((PlayerUserControl)panel3.Controls[FindPUCIndex(players[clientIndex])]).ShowReady = false;
        	}
		}

        #endregion

        #region Keyboard Events

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (SendMessage("CHAT:" + txtMessage.Text))
                    updateMsgLog("<" + clientName + "> " + txtMessage.Text);

                txtMessage.Text = "";
                txtMessage.Focus();
                
                e.Handled = true;
        		e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region Communications

        private void Connect(string IPAddress)
        {
            updateMsgLog("Attempting connection to " + IPAddress + "...");

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(System.Net.IPAddress.Parse(IPAddress), 4994);
                timer.Interval = 100;
                timer.Start();

                connectToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = true;

                updateMsgLog("Connection established. Obtaining player list...");
                
            }
            catch (SocketException)
            {
                Disconnect("Connection attempt failed.");
            }
        }

        private void Disconnect(string message)
        {
            timer.Stop();
            if(this.socket != null) this.socket.Close();
            lblPlayers.Text = "Not Connected";
            players.Clear();
            
            ReadyBtn.Enabled = false;
            ReadyBtn.Visible = false;

            updateMsgLog(message);

            disconnectToolStripMenuItem.Enabled = false;
            connectToolStripMenuItem.Enabled = true;
            
            ClearPUC();
            InitializeNames();
        }

        private bool SendMessage(string message)
        {
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message + Environment.NewLine);

            try
            {
                this.socket.Send(buffer);
                return true;
            }
            catch (Exception)
            {
                this.Cursor = Cursors.Default;
                updateMsgLog("You are not connected to a server.");
                return false;
            }

        }
        
        private void monitorSocketStatus(object source, ElapsedEventArgs e)
        {
        	clientIndex = players.FindIndex(item => item.Id == clientId);
        	
        	try
            {
                bool polled = socket.Poll(0, SelectMode.SelectRead);

                if (polled == true && socket.Available == 0)
                {
                    this.socket.Close();
                    this.socket = null;
                }
                else
                {
                    //New code!!!
                    int crIndex = 0;
                    if (socket.Available > 0)
                    {
                        int bytes = socket.Available;
                        byte[] buffer = new byte[bytes];

                        socket.Receive(buffer);

                        incomingBuffer += System.Text.Encoding.ASCII.GetString(buffer);
                    }

                    if (incomingBuffer != null)
                        crIndex = incomingBuffer.IndexOf(Environment.NewLine);

                    if (crIndex > -1)
                    {
                        msgQueue.Enqueue(incomingBuffer.Substring(0, crIndex));
                        incomingBuffer = incomingBuffer.Substring(crIndex + 2);
                    }

                }
            }
            catch (ObjectDisposedException)
            {
               this.socket.Close();
               this.socket = null;
            }
            catch(NullReferenceException)
            {
            	timer.Stop();
            	updateMsgLog("Connection to the server was lost.");
            	
            	lblPlayers.Text = "Not Connected";
            	players.Clear();
            	
            	ReadyBtn.Enabled = false;
            	ReadyBtn.Visible = false;
            	
            	disconnectToolStripMenuItem.Enabled = false;
            	connectToolStripMenuItem.Enabled = true;
            	
		        ClearPUC();
		        InitializeNames();
            }
            catch (Exception ex)
            {
            	MessageBox.Show("Error occured in the monitorSocketStatus handler: " + ex.Message,
                        "Error", MessageBoxButtons.OK);
            }
        }

        private void ProcessMessageQueue(object source, ElapsedEventArgs e)
        {	                            	
        	if (msgQueue.Count > 0)
            {
                string msg = msgQueue.Dequeue();

                string[] parsedMsg = SplitQuoted(msg, ":");
                string srcClientId = "";
                string command = "";

                try
                {
                    srcClientId = parsedMsg[0];
                    command = parsedMsg[1];

                    switch (command.ToUpper())
                    {
                        case "YOURID":
                            clientId = parsedMsg[2];
                            SendMessage("NAME:" + clientName);
                            break;
                        case "NAMEREGISTERED":
                            SendMessage("WHO");
                            break;
                        case "CLIENTCONNECTED":
                            updateMsgLog("New player connected.");
                            break;
                        case "CLIENTDISCONNECTED":
                            foreach (Player player in players)
                            {
                                if (parsedMsg[2] == player.Id)
                                {
                                    updateMsgLog(player.Name + " has left the game.");
                                }    
                            }
                            
                            SendMessage("WHO");
                            break;
                        case "CHAT":
                            foreach (Player player in players)
                                if (srcClientId == player.Id)
                                    updateMsgLog("<" + player.Name + "> " + parsedMsg[2]);
                            		
                            break;
                        case "CLIENTID":
                            players.Clear();
                            lblPlayers.Text = "";
                            ClearPUC();
                            
                            for(int i=0; i < Convert.ToInt32(parsedMsg[2]); i++)
                            {
	                            Player p = new Player();
	                            p.Id = parsedMsg[i*3+3];
	                            p.Name = parsedMsg[i*3+4];
	                            players.Add(p);
	                            
	                            AnonymousDelegate d = delegate()
	                            {
	                            	PlayerUserControl uc = new PlayerUserControl();
	                            	uc.Name = p.Id;
	                            	uc.PlayerName = p.Name;
	                            	uc.Top = 53 * players.Count;
	                            	uc.Left = 4;
	                            	panel3.Controls.Add(uc);
	                            	uc.Show();
	                            	uc.BringToFront();
	                            	
	                            	if(state == States.Startup)
	                            	{
	                            		ReadyBtn.Enabled = true;
	           							ReadyBtn.Visible = true;
	                            	}
	                            	
	                            	if(parsedMsg[i*3+5] == "True")
	                            	{
	                            		((PlayerUserControl)panel3.Controls[FindPUCIndex(p)]).ShowReady = true;
	                            	}
	                            	else
	                            	{
	                            		((PlayerUserControl)panel3.Controls[FindPUCIndex(p)]).ShowReady = false;
	                            	}
	                            };
	                            this.Invoke(d, null);
                            }
                            break;
                        case "SERVERSHUTDOWN":
                            Disconnect("The server has terminated the connection.");
                            break;
                        case "INFO":
                            updateMsgLog(parsedMsg[2]);
                            break;
                        case "HANDTOTABLE":
                        {
                            foreach (Player player in players)
                            {
                            	if (srcClientId == player.Id)
                            	{
                            		Card card = player.Hand.GetCard(parsedMsg[2]);
                            		card.FacingSide = tableHandSide;
                            		tableHands[player.TableIndex].Cards.Add(card);
                            		
                            		this.Invoke(new CardToTableDelegate(DisplayTableHand), new Object[] { player.TableIndex });
                            	}
                            }
                            break;
                        }
                        case "TABLETOHAND":
                        {
                            foreach (Player player in players)
                            {
                            	if (srcClientId == player.Id)
                            	{
                            		Card card = tableHands[player.TableIndex].GetCard(parsedMsg[2]);
                            		card.FacingSide = otherHandSide;
                            		player.Hand.Cards.Add(card);
                            		
                            		this.Invoke(new CardToTableDelegate(DisplayTableHand), new Object[] { player.TableIndex });
                            	}
                            }
                            break;
                        }
                        case "DECKTOTABLE":
                        {
                            foreach (Player player in players)
                            {
                            	if (parsedMsg[2] == player.Id)
                            	{
                            		Card card = dealer.Deck.CopyCard(parsedMsg[3]);
                            		card.FacingSide = tableHandSide;
                            		tableHands[player.TableIndex].Cards.Add(card);
                            		
                            		this.Invoke(new CardToTableDelegate(DisplayTableHand), new Object[] { player.TableIndex });
                            	}
                            }
                            break;
                        }
                        case "DICEROLLED":
                        {
                        	if(state == States.Teams)
                            {
                            	int Index = DealerIndex - clientIndex;
                            	if(Index < 0)
                            	{
                            		Index += 4;
                            	}
                            	
                            	dieBox[Index].Image = Dice.GetDiceImage(Convert.ToInt32(parsedMsg[2]));
                            	updateMsgLog(players[DealerIndex].Name + " rolled a " + Convert.ToInt32(parsedMsg[2]) + ".");
                            }
                        	else if(GameType == Games.TradeIn)
                            {
                        		for(int i=0; i < dieBox.Length; i++)
                            	{
                        			if(dieBox[i].Image != null)
                            		{
                            			dieBox[i].Enabled = false;
	                            		dieBox[i].Image = Dice.GetDiceImage(Convert.ToInt32(parsedMsg[2]));
	                            		updateMsgLog(players[players.FindIndex(item => item.TableIndex == i)].Name + " rolled a " + Convert.ToInt32(parsedMsg[2]) + ".");
                            		}
                            	}
                            }
                            else
                            {
	                            dieBox[players[DealerIndex].TableIndex].Image = Dice.GetDiceImage(Convert.ToInt32(parsedMsg[2]));
	                            updateMsgLog(players[DealerIndex].Name + " rolled a " + Convert.ToInt32(parsedMsg[2]) + ".");
                            }
                            
                            diceFlag = false;
                            break;
                        }
                        case "DICECHANGED":
                        {
                            if(diceFlag == false)
                            {
	                            Dice.PlayDiceRollSound();
							    diceFlag = true;
                            }
                            
                            if(state == States.Teams)
                            {
                            	int Index = DealerIndex - clientIndex;
                            	if(Index < 0)
                            	{
                            		Index += 4;
                            	}
                            	
                            	dieBox[Index].Enabled = false;
	                            dieBox[Index].Image = Dice.GetDiceImage(Convert.ToInt32(parsedMsg[2]));
                            }
                            else if(GameType == Games.TradeIn)
                            {
                            	foreach(PictureBox p in dieBox)
                            	{
                            		if(p.Image != null)
                            		{
                            			p.Enabled = false;
	                            		p.Image = Dice.GetDiceImage(Convert.ToInt32(parsedMsg[2]));
                            		}
                            	}
                            }
                            else
                            {
	                            dieBox[players[DealerIndex].TableIndex].Enabled = false;
	                            dieBox[players[DealerIndex].TableIndex].Image = Dice.GetDiceImage(Convert.ToInt32(parsedMsg[2]));
                            }
                            break;
                        }
                        case "ROOMFULL":
                        {
                            Disconnect("Disconnected. Room is full. Please try again later.");
                            break;
                        }
                        case "READY":
                        {
                            if (srcClientId != "HOST")
                            {
                                foreach (Player player in players)
                            	{
                                    if (srcClientId == player.Id)
                            		{
                                        updateMsgLog(player.Name + " is ready.");
                                        
                                        AnonymousDelegate a = () => ((PlayerUserControl)panel3.Controls[FindPUCIndex(player)]).ShowReady = true;
                                        this.Invoke(a, null);
                            		}
                                }
                            }
                            break;
                        }
                        case "NOTREADY":
                        {
                            if (srcClientId != "HOST")
                            {
                                foreach (Player player in players)
                            	{
                                    if (srcClientId == player.Id)
                            		{
                                        updateMsgLog(player.Name + " is not ready.");
                                        
                                        AnonymousDelegate a = () => ((PlayerUserControl)panel3.Controls[FindPUCIndex(player)]).ShowReady = false;
                                        this.Invoke(a, null);
                            		}
                                }
                            }
                            break;
                        }
                        case "STATECHANGE":
                        {
                        	state = (States)Enum.Parse(typeof(States), parsedMsg[2]);
                        	/*if(state > States.Startup)
                        	{
                        		AnonymousDelegate a = delegate()
                                {
                                    ReadyBtn.Enabled = false;
               						ReadyBtn.Visible = false;
               						ReadyBtn.Text = "Ready";
               						
               						foreach(Player ply in players)
               						{
               							((PlayerUserControl)panel3.Controls[FindPUCIndex(ply)]).ShowReady = false;
               						}
                                };
                            	this.Invoke(a, null);
                        	}*/
                        	break;
                        }
                        case "DEALERUPDATE":
                        {
                        	DealerIndex = Convert.ToInt32(parsedMsg[2]);
                        	
                        	if(DealerIndex == clientIndex)
                            {
                            	updateMsgLog("It is your turn.");
                            	
                            	SendToFront();
                            }
                            else
                            {
                            	if(state == States.Game)
                            	{
                            		updateMsgLog(players[DealerIndex].Name + " is the dealer.");
                            	}
                            	else
                            	{
                            		updateMsgLog("It is " + players[DealerIndex].Name + "'s turn.");
                            	}
                            }
                        	
                        	if(state == States.Teams)
                        	{
                        		int Index = DealerIndex - clientIndex;
	                            if(Index < 0)
	                            {
	                            	Index += 4;
	                            }
	                            
	                            for(int i = 0; i < dieBox.Length; i++)
                        		{
									dieBox[i].Image = Index == i ? Dice.GetDiceImage(10) : null;
                        		}
                        			
								dieBox[0].Enabled = DealerIndex == clientIndex ? true : false;
                        	}
                        	else
                        	{
                        		for(int i = 0; i < dieBox.Length; i++)
                        		{
                        			if(state == States.Game)
                        			{
                        				if(players[DealerIndex].TableIndex == i)
                        				{
                        					AnonymousDelegate a = () => DisplayDeckStack(players[DealerIndex]);
                        					this.Invoke(a, null);
                        				}
                        				else
                        				{
                        					dealBox[i].Image = null;
                        				}
                        			}
                        			
									dieBox[i].Image = players[DealerIndex].TableIndex == i ? Dice.GetDiceImage(10) : null;
                        		}
                        			
								dieBox[0].Enabled = DealerIndex == clientIndex ? true : false;
                        	}
                        	
                        	break;
                        }
                        case "TURNUPDATE":
                        {
                            if(parsedMsg[2] == clientId)
                            {
                            	updateMsgLog("It is your turn.");
                            	
                            	SendToFront();
                            }
                            else
                            {
                            	foreach(Player p in players)
                            		if(p.Id == parsedMsg[2])
                            			updateMsgLog("It is " + p.Name + "'s turn.");
                            }
                        	break;
                        }
                        case "SETTEAMS":
                        {
                        	for(int i=0; i < players.Count; i++)
                        	{
	                        	players[i].Team = (Teams)Enum.Parse(typeof(Teams), parsedMsg[i+2]);
	                        	
	                        	if(players[i].Team == Teams.TeamOne)
	                        	{
	                        		AnonymousDelegate a = () => ((PlayerUserControl)panel3.Controls[FindPUCIndex(players[i])]).BackColor = Color.Red;
	                        		this.Invoke(a, null);
	                        	}
	                        	else
	                        	{
	                        		AnonymousDelegate a = () => ((PlayerUserControl)panel3.Controls[FindPUCIndex(players[i])]).BackColor = Color.Blue;
	                        		this.Invoke(a, null);
	                        	}
                        	}
                        	break;
                        }
                        case "SETTBLPOS":
                        {
                        	int clientPostion = Convert.ToInt32(parsedMsg[clientIndex+2]);
                        	for(int i=0; i < players.Count; i++)
                        	{
                        		players[i].TableIndex = Convert.ToInt32(parsedMsg[i+2]) - clientPostion;
                        		
                        		if(players[i].TableIndex < 0)
                        			players[i].TableIndex += 4;
                        		
                        		if(players[i].TableIndex != 0)
                        		{
	                        		this.Invoke(new UpdateNameLabel(UpdateName), new Object[] {players[i]});
                        		}
                        	}
                        	break;
                        }
                        case "SETDEALER":
                        {
                        	updateMsgLog(players[Convert.ToInt32(parsedMsg[2])].Name + " will deal first.");
                        	break;
                        }
                        case "SETGAMETYPE":
                        {
                        	GameType = (Games)Enum.Parse(typeof(Games), parsedMsg[2]);
                        	string newString = Regex.Replace(parsedMsg[2], "([a-z])([A-Z])", "$1 $2");
                        	updateMsgLog(newString + " has been selected.");
                        	break;
                        }
                        case "SETSCORE":
                        {
                        	team1Score.Text = parsedMsg[2];
                        	team2Score.Text = parsedMsg[3];
                        	break;
                        }
                        case "SETTRICKS":
                        {
                        	team1Tricks.Text = parsedMsg[2];
                        	team2Tricks.Text = parsedMsg[3];
                        	break;
                        }
                        case "SENDHAND":
                        {
                        	foreach (Player player in players)
                        	{
                        		if (parsedMsg[2] == player.Id)
                        		{
                        			player.Hand.Cards.Clear();
                        			
                        			for(int i=0; i < Convert.ToInt32(parsedMsg[3]); i++)
                        			{
                        				string cardTextValue = parsedMsg[i+4];
                        				player.Hand.Cards.Add(dealer.Deck.CopyCard(cardTextValue));
                        			}
                        			
                        			
                        			player.Hand.FacingSide = player.Id == clientId ? myHandSide : otherHandSide;
                        			AnonymousDelegate a = () => DisplayHand(player);
                        			this.Invoke(a, null);
                        		}
                        	}
                        	break;
                        }
                        case "SENDDECK":
                        {
                            foreach (Player player in players)
                        	{
                        		if (parsedMsg[2] == player.Id)
                        		{
		                            string cardTextValue = parsedMsg[3];
		                            Card c = dealer.Deck.CopyCard(cardTextValue);
		                            AnonymousDelegate a = () => DisplayDeckCard(player, c);
		                            this.Invoke(a, null);
                        		}
                            }
                            break;
                        }
                        case "SETMYHAND":
                        {
                        	myHandSide = (FacingSides)Enum.Parse(typeof(FacingSides), parsedMsg[2]);
                        	players[clientIndex].Hand.FacingSide = myHandSide;
                        	break;
                        }
                        case "SETOTHERHANDS":
                        {
                        	otherHandSide = (FacingSides)Enum.Parse(typeof(FacingSides), parsedMsg[2]);
                        	foreach(Player p in players)
                        	{
                        		if(p.Id != clientId)
                        		{
                        			players[clientIndex].Hand.FacingSide = otherHandSide;
                        		}
                        	}
                        	break;
                        }
                        case "SETTABLEHANDS":
                        {
                        	tableHandSide = (FacingSides)Enum.Parse(typeof(FacingSides), parsedMsg[2]);
                        	foreach(Hand h in tableHands)
                        	{
                        		h.FacingSide = tableHandSide;
                        	}
                        	break;
                        }
                        case "ENABLEREADY":
                        {
                        	bool b = Convert.ToBoolean(parsedMsg[2]);
                        	AnonymousDelegate d = delegate()
	                       	{
                        		ReadyBtn.Enabled = b;
                        		ReadyBtn.Visible = b;
	            				
                        		if(!b)
                        		{
                        			ReadyBtn.Text = "Ready";
                        			foreach(Player p in players)
                        				((PlayerUserControl)panel3.Controls[FindPUCIndex(p)]).ShowReady = false;
                        		}
                        	};
                        	this.Invoke(d, null);
                        	break;
                        }
                        case "ENABLEHAND":
                        {
                        	AnonymousDelegate a = () => EnableMyHand(Convert.ToBoolean(parsedMsg[2]));
                        	this.Invoke(a, null);
                        	break;
                        }
                        case "ENABLETABLEHAND":
                        {
                        	AnonymousDelegate a = () => tableBox[0].Enabled = Convert.ToBoolean(parsedMsg[2]);
                        	this.Invoke(a, null);
                        	break;
                        }
                        case "CLEARTABLEHAND":
                        {
                        	foreach(Player player in players)
                        	{
                        		if(player.Id == parsedMsg[2])
                        		{
                        			tableHands[player.TableIndex].Cards.Clear();
                        			this.Invoke(new CardToTableDelegate(DisplayTableHand), new Object[] { player.TableIndex });
                        		}
                        	}
                        	break;
                        }
                        case "RESETTABLE":
                        {
                            for(int i=0; i < 4; i++)
                            {
                            	players[i].Hand.Cards.Clear();
                            	tableHands[i].Cards.Clear();
                            		
                            	DisplayHand(players[i]);
                            	DisplayTableHand(i);
                            }
                            
                            trumpBox.Image = null;
                            
                            team1Tricks.Text = "0";
                            team2Tricks.Text = "0";
                            break;
                        }
                        case "CALLTRUMP":
                        {
                            if (parsedMsg[2] == clientId)
                        	{
                            	SendToFront();
                            	
                            	AnonymousDelegate a = () => DisplayTrumpForm(parsedMsg[3]);
                        		this.Invoke(a, null);
                        	}
                            else
                            {
                            	foreach(Player player in players)
                            	{
                            		if(player.Id == parsedMsg[2])
                            		{
                            			updateMsgLog(player.Name + "'s turn to pick trump or pass");
                            		}
                            	}
                            }
                            break;
                        }
                        case "CALLTRUMP2":
                        {
                            if (parsedMsg[2] == clientId)
                        	{
                            	SendToFront();
                            	
                            	AnonymousDelegate a = () => DisplayTrump2Form(parsedMsg[3], clientIndex != DealerIndex);
                        		this.Invoke(a, null);
                        	}
                            else
                            {
                            	foreach(Player player in players)
                            	{
                            		if(player.Id == parsedMsg[2])
                            		{
                            			updateMsgLog(player.Name + "'s turn to pick trump or pass");
                            		}
                            	}
                            }
                            break;
                        }
                        case "HIDEDECK":
                        {
                            foreach(Player player in players)
                            {
                            	if(player.Id == parsedMsg[2])
                            	{
		                            AnonymousDelegate a = () => deckCardBox[player.TableIndex].Visible = false;
		                        	this.Invoke(a, null);
                            	}
                            }
                        	break;
                        }
                        case "SHOWTABLEHAND":
                        {
                            foreach(Player player in players)
                            {
                            	if(player.Id == parsedMsg[2])
                            	{
		                            AnonymousDelegate a = () => tableBox[player.TableIndex].Visible = true;
		                        	this.Invoke(a, null);
                            	}
                            }
                        	break;
                        }
                        case "HIDETABLEHAND":
                        {
                            foreach(Player player in players)
                            {
                            	if(player.Id == parsedMsg[2])
                            	{
		                            AnonymousDelegate a = () => tableBox[player.TableIndex].Visible = false;
		                        	this.Invoke(a, null);
                            	}
                            }
                        	break;
                        }
                        case "EUCHREENABLE":
                        {
                            EuchreEnable = Convert.ToBoolean(parsedMsg[2]);
                            AnonymousDelegate a = () => DisplayHand(players[clientIndex]);
		                    this.Invoke(a, null);
                            break;
                        }
                        case "SETTRUMP":
                        {
                            TrumpSuit = (CardSuits)Enum.Parse(typeof(CardSuits), parsedMsg[2]);
                            AnonymousDelegate a = () => trumpBox.Image = ImageHelper.GetSuitImage(TrumpSuit);
		                    this.Invoke(a, null);
                            break;
                        }
                        case "SETWINNERINDEX":
                        {
                            WinnerIndex = Convert.ToInt32(parsedMsg[2]);
                            break;
                        }
                        case "ROLLTRADEIN":
                        {
                        	int Index = Convert.ToInt32(parsedMsg[2]);
                        	
                        	if(Index == clientIndex)
                            {
                            	updateMsgLog("It is your turn.");
                            	
                            	SendToFront();
                            }
                            else
                            {
                            	updateMsgLog("It is " + players[Index].Name + "'s turn.");
                            }
                        	
                            for(int i = 0; i < dieBox.Length; i++)
                            {
                            	dieBox[i].Image = players[Index].TableIndex == i ? Dice.GetDiceImage(10) : null;
                            }
                            
                            dieBox[0].Enabled = Index == clientIndex ? true : false;
                            break;
                        }
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured in the ProcessMessageQueue handler: " + ex.Message,
                        "Error", MessageBoxButtons.OK);
                }
            }
        }

        #endregion

        #region Helper Methods

        private string[] SplitQuoted(string text, string delimiters)
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
        
        public int FindPUCIndex(Player p)
        {
        	for (int x = panel3.Controls.Count-1; x >= 0 ; x--)
        	{
        		if (panel3.Controls[x].GetType() == typeof(PlayerUserControl))
        		{
        			if(panel3.Controls[x].Name == p.Id)
        			{
        				return x;
        			}
        		}
        	}
        	
        	return -1;
        }
        
        public void ClearPUC()
        {
        	for (int x = panel3.Controls.Count-1; x >= 0 ; x--)
            	if (panel3.Controls[x].GetType() == typeof(PlayerUserControl))
            		panel3.Controls[x].Dispose();
        }
        
        public void InitializeNames()
        {
        	rotatingLabel1.RotateAngle = -90;      // angle to rotate
			rotatingLabel2.RotateAngle = 0;        // angle to rotate
			rotatingLabel3.RotateAngle = 90;       // angle to rotate
        }
        
        public void UpdateName(Player p)
        {
        	nameLabel[p.TableIndex].Text = "";
			nameLabel[p.TableIndex].AutoSize = false;
			nameLabel[p.TableIndex].NewText = p.Name;
			nameLabel[p.TableIndex].ForeColor = p.Team == Teams.TeamOne ? Color.Red : Color.Blue;
			nameLabel[p.TableIndex].Size = new System.Drawing.Size(46, 126);
        }
        
        public void EnableMyHand(bool State)
        {
        	// Enable all boxes with a card
        	for(int i=0; i < 5; i++)
        	{
        		if(i < players[clientIndex].Hand.NumberOfCards)
        		{
        			HandBox[0,i].Enabled = State;
        		}
        	}
        }
        
        public void DisplayDeckStack(Player p)
        {
        	Bitmap b = ImageHelper.GetDeckStackImage();
        	
        	if(p.TableIndex == 1 || p.TableIndex == 3)
        	{
        		b.RotateFlip(RotateFlipType.Rotate90FlipNone);
        	}
        	
        	//b = (Bitmap)ImageHelper.ChangeImageOpacity(b, 0.82);
        	dealBox[p.TableIndex].Image = b;
        }
        
        public void DisplayDeckCard(Player p, Card c)
        {
        	Bitmap b = ImageHelper.GetFaceImageForCard(c);
        	
        	if(p.TableIndex == 1 || p.TableIndex == 3)
        	{
        		b.RotateFlip(RotateFlipType.Rotate90FlipNone);
        	}
        	
        	//b = (Bitmap)ImageHelper.ChangeImageOpacity(b, 0.82);
        	deckCardBox[p.TableIndex].Image = b;
        	deckCardBox[p.TableIndex].Visible = true;
        }
        
        public void DisplayTrumpForm(string card)
        {
        	TrumpForm frm = new TrumpForm(card);
        	frm.ShowDialog();
        	
        	SendMessage("CALLTRUMP:" + frm.PickedUp.ToString());
        }
        
        public void DisplayTrump2Form(string suit, bool b)
        {
        	Trump2Form frm = new Trump2Form(suit, b);
        	frm.ShowDialog();
        	
        	SendMessage("CALLTRUMP2:" + frm.PickedUp.ToString() + ":" + frm.Suit);
        }
        
        public void SendToFront()
        {
        	// Get the window to the front.
        	this.TopMost = true;
        	this.TopMost = false;
        	
        	// 'Steal' the focus.
        	this.Activate();
        }
        
        #endregion

    }
}
