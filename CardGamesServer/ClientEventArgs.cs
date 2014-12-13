using System;
using System.Collections.Generic;
using System.Text;

namespace CardGamesServer
{
    public class ClientEventArgs : EventArgs 
    {
        private bool isConnected;
        private string lastMessage;

        public bool IsConnected
        {
            get { return true; }
            set { isConnected = value; }
        }

        public string LastMessage
        {
            get { return lastMessage; }
            set { lastMessage = value; }
        }
    }
}
