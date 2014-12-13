using System;
using System.Collections.Generic;
using System.Text;

namespace PlayingCards
{
    public class Deck
    {
        private Random random = new Random();
        private List<Card> cards = new List<Card>();

        public List<Card> Cards
        {
            get { return cards; }
            set { cards = value; }
        }

        public int NumberOfCards
        {
            //Shortcut property to get the number of cards in the deck

            get { return cards.Count; }
        }

        public Card GetRandomCard()
        {
            // Returns a random card from the deck.  
            // Removes the card from the deck.  
            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            int cardId = random.Next(0, NumberOfCards);

            Card card = cards[cardId];
            this.Cards.Remove(card);

            return card;
        }

        public Card GetTopCard()
        {
            // Gets the first (top) card in the deck
            // Essentially pulling from the top of the deck.
            // Removes the card from the deck

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            Card card = cards[cards.Count - 1];
            this.Cards.Remove(card);

            return card;
        }

        public Card GetBottomCard()
        {
            // Gets the last (bottom) card in the deck
            // Essentially pulling from the bottom of the deck.
            // Removes the card from the deck

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            Card card = cards[0];
            this.Cards.Remove(card);

            return card;
        }

        public Card GetCard(int index)
        {
            // Gets a card in the deck
            // Removes the card from the deck

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            if (cards.Count <= index)
                throw new Exception("InvalidCardException");

            Card card = cards[index];
            this.Cards.Remove(card);

            return card;
        }

        public Card GetCard(CardValues cardValue, CardSuits suit)
        {
            foreach (Card c in this.Cards)
            {
                if (c.Value == cardValue && c.Suit == suit)
                {
                	this.Cards.Remove(c);
                    return c;
                }
            }

            return null;
        }
        
        public Card GetCard(string textValue)
        {
            string[] cardInfo = textValue.Split(',');

            if (cardInfo.GetUpperBound(0) != 1)
                throw new Exception("InvalidCardValue");

            CardValues cardValue = CardValues.Ace;
            CardSuits cardSuit = CardSuits.Spades;

            switch (cardInfo[0])
            {
                case "A": cardValue = CardValues.Ace; break;
                case "2": cardValue = CardValues.Deuce; break;
                case "3": cardValue = CardValues.Three; break;
                case "4": cardValue = CardValues.Four; break;
                case "5": cardValue = CardValues.Five; break;
                case "6": cardValue = CardValues.Six; break;
                case "7": cardValue = CardValues.Seven; break;
                case "8": cardValue = CardValues.Eight; break;
                case "9": cardValue = CardValues.Nine; break;
                case "10": cardValue = CardValues.Ten; break;
                case "J": cardValue = CardValues.Jack; break;
                case "Q": cardValue = CardValues.Queen; break;
                case "K": cardValue = CardValues.King; break;
                default:
                    throw new Exception("InvalidCardValue");
            }

            switch (cardInfo[1])
            {
                case "Spades": cardSuit = CardSuits.Spades; break;
                case "Hearts": cardSuit = CardSuits.Hearts; break;
                case "Diamonds": cardSuit = CardSuits.Diamonds; break;
                case "Clubs": cardSuit = CardSuits.Clubs; break;
                default:
                    throw new Exception("InvalidCardValue");
            }

            return this.GetCard(cardValue, cardSuit);

        }

        public Card PeekRandomCard()
        {
            // Returns a random card from the deck.  Does NOT remove the card from the
            // deck.  Consider a "pick a card, any card" scenario

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            int cardId = random.Next(0, NumberOfCards);

            return cards[cardId];
        }

        public Card PeekTopCard()
        {
            // Gets the first (top) card in the deck
            // Essentially pulling from the top of the deck.
            // Does NOT remove the card from the deck

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            return cards[cards.Count - 1];
        }

        public Card PeekBottomCard()
        {
            // Gets the last (bottom) card in the deck
            // Essentially pulling from the bottom of the deck.
            // Does NOT remove the card from the deck

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");

            return cards[0];
        }

        public Card PeekCard(int index)
        {
            // Gets a card in the deck
            // Does NOT remove the card from the deck

            if (cards.Count == 0)
                throw new Exception("EmptyDeckException");
            
            if (cards.Count <= index)
                throw new Exception("InvalidCardException");

            return cards[index];
        }
                
        public Card PeekCard(CardValues cardValue, CardSuits suit)
        {
            foreach (Card c in this.Cards)
            {
                if (c.Value == cardValue && c.Suit == suit)
                    return c;
            }

            return null;
        }

        public Card PeekCard(string textValue)
        {
        	string[] cardInfo = textValue.Split(',');

            if (cardInfo.GetUpperBound(0) != 1)
                throw new Exception("InvalidCardValue");

            CardValues cardValue = CardValues.Ace;
            CardSuits cardSuit = CardSuits.Spades;

            switch (cardInfo[0])
            {
                case "A": cardValue = CardValues.Ace; break;
                case "2": cardValue = CardValues.Deuce; break;
                case "3": cardValue = CardValues.Three; break;
                case "4": cardValue = CardValues.Four; break;
                case "5": cardValue = CardValues.Five; break;
                case "6": cardValue = CardValues.Six; break;
                case "7": cardValue = CardValues.Seven; break;
                case "8": cardValue = CardValues.Eight; break;
                case "9": cardValue = CardValues.Nine; break;
                case "10": cardValue = CardValues.Ten; break;
                case "J": cardValue = CardValues.Jack; break;
                case "Q": cardValue = CardValues.Queen; break;
                case "K": cardValue = CardValues.King; break;
                default:
                    throw new Exception("InvalidCardValue");
            }

            switch (cardInfo[1])
            {
                case "Spades": cardSuit = CardSuits.Spades; break;
                case "Hearts": cardSuit = CardSuits.Hearts; break;
                case "Diamonds": cardSuit = CardSuits.Diamonds; break;
                case "Clubs": cardSuit = CardSuits.Clubs; break;
                default:
                    throw new Exception("InvalidCardValue");
            }

            return this.PeekCard(cardValue, cardSuit);

        }

        public bool Exists(Card aCard)
        {
            foreach (Card c in cards)
            {
                if (aCard.TextValue == c.TextValue)
                {
                    return true;
                }
            }

            return false;
        }

		public Card CopyCard(CardValues cardValue, CardSuits suit)
        {
            foreach (Card c in this.Cards)
            {
                if (c.Value == cardValue && c.Suit == suit)
                {
                    return c;
                }
            }

            return null;
        }
        
        public Card CopyCard(string textValue)
        {
            string[] cardInfo = textValue.Split(',');

            if (cardInfo.GetUpperBound(0) != 1)
                throw new Exception("InvalidCardValue");

            CardValues cardValue = CardValues.Ace;
            CardSuits cardSuit = CardSuits.Spades;

            switch (cardInfo[0])
            {
                case "A": cardValue = CardValues.Ace; break;
                case "2": cardValue = CardValues.Deuce; break;
                case "3": cardValue = CardValues.Three; break;
                case "4": cardValue = CardValues.Four; break;
                case "5": cardValue = CardValues.Five; break;
                case "6": cardValue = CardValues.Six; break;
                case "7": cardValue = CardValues.Seven; break;
                case "8": cardValue = CardValues.Eight; break;
                case "9": cardValue = CardValues.Nine; break;
                case "10": cardValue = CardValues.Ten; break;
                case "J": cardValue = CardValues.Jack; break;
                case "Q": cardValue = CardValues.Queen; break;
                case "K": cardValue = CardValues.King; break;
                default:
                    throw new Exception("InvalidCardValue");
            }

            switch (cardInfo[1])
            {
                case "Spades": cardSuit = CardSuits.Spades; break;
                case "Hearts": cardSuit = CardSuits.Hearts; break;
                case "Diamonds": cardSuit = CardSuits.Diamonds; break;
                case "Clubs": cardSuit = CardSuits.Clubs; break;
                default:
                    throw new Exception("InvalidCardValue");
            }

            return this.CopyCard(cardValue, cardSuit);

        }

        

    }
}
