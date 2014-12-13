using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PlayingCards;

namespace CardGamesServer
{
    public delegate void ClientEventHandler(Client client, ClientEventArgs args);

    public class Client: Player
    {
        public event ClientEventHandler OnClientConnected;
        public event ClientEventHandler OnClientDisconnected;
        public event ClientEventHandler OnClientReceiveData;

        private Socket socket = null;
        private bool isReady = false;
        private string incomingBuffer = "";

        public Socket Socket
        {
            get { return socket; }
        }

        public bool IsReady
        {
        	set { isReady = value; }
            get { return isReady; }
        }
        
        public Client()
        {
            this.Id = System.Guid.NewGuid().ToString();
            socket = new Socket(AddressFamily.Unspecified, SocketType.Stream, ProtocolType.Tcp);
        }

        public void SendMessage(string message)
        {
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);

            this.socket.Send(buffer);

        }

        public void AcceptSocket(Socket socket)
        {
            this.socket = socket;

            Thread monitorThread = new Thread(this.monitorSocketStatus);
            monitorThread.Start();

            if (OnClientConnected != null)
            {
                ClientEventArgs p = new ClientEventArgs();
                p.IsConnected = true;
                OnClientConnected(this, p);
            }
        }

        private void monitorSocketStatus()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    bool polled = socket.Poll(0, SelectMode.SelectRead);

                    if (polled == true && socket.Available == 0)
                    {
                        this.socket.Close();
                        this.socket = null;

                        ClientEventArgs p = new ClientEventArgs();
                        p.IsConnected = false;
                        OnClientDisconnected(this, p);

                        System.Threading.Thread.CurrentThread.Abort();
                    }
                    else
                    {
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
                            ClientEventArgs p = new ClientEventArgs();
                            p.LastMessage = incomingBuffer.Substring(0, crIndex);
                            incomingBuffer = incomingBuffer.Substring(crIndex + 2);
                            OnClientReceiveData(this, p);
                        }
                    }

                }
            }
            catch (ObjectDisposedException)
            {
            	if(this.socket != null)
            	{
	                this.socket.Close();
	                this.socket = null;
            	}

                System.Threading.Thread.CurrentThread.Abort();
            }
        }
    }
}
