using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace PlayingCards
{
   
    public class Dealer : Player
    {
        private Deck deck = new Deck();

        public Deck Deck
        {
            get { return deck; }
            set { deck = value; }
        }

        public Dealer() : base()
        {
        }

        public void InitializePokerDeck()
        {
            // Generates a standard 52 card poker deck
            // This must be called or else the deck will be empty
            deck.Cards.Clear();

            AddSuit(CardSuits.Clubs, CardValues.Deuce, CardValues.Ace);
            AddSuit(CardSuits.Diamonds, CardValues.Deuce, CardValues.Ace);
            AddSuit(CardSuits.Hearts, CardValues.Deuce, CardValues.Ace);
            AddSuit(CardSuits.Spades, CardValues.Deuce, CardValues.Ace);
        }
        
        public void InitializePokerDeck(CardValues min, CardValues max)
        {
            // Generates a poker deck
            // This must be called or else the deck will be empty
            deck.Cards.Clear();

            AddSuit(CardSuits.Clubs, min, max);
            AddSuit(CardSuits.Diamonds, min, max);
            AddSuit(CardSuits.Hearts, min, max);
            AddSuit(CardSuits.Spades, min, max);
        }

        private void AddSuit(CardSuits suit, CardValues min, CardValues max)
        {
        	for(CardValues card = min; card <= max; card++)
        	{
        		deck.Cards.Add(new Card(suit, card));
        	}
        }

        public void Shuffle()
        {
            // Randomizes the deck by creating a new deck and randomly
            // adding cards from this one.  This should be called after
            // InitalizePokerDeck() because that method generates a deck
            // with the cards all in order.

            if (deck.NumberOfCards == 0)
                throw new Exception("EmptyDeckException");

            Deck newDeck = new Deck();

            int totalCards = deck.NumberOfCards;

            while (newDeck.NumberOfCards < totalCards)
            {
                Card c = deck.GetRandomCard();
                deck.Cards.Remove(c);
                newDeck.Cards.Add(c);
            }

            deck.Cards.Clear();
            deck.Cards = newDeck.Cards;

        }

        public Hand Deal(int numberOfCards)
        {
            //Returns a new hand of x cards, removing them from the deck

            return Deal(null, numberOfCards);
        }

        public Hand Deal(Hand hand, int numberOfCards)
        {
            // Overloaded deal method allowing you to specify an existing hand to deal to

            if (hand == null) hand = new Hand();

            for (int x = 0; x < numberOfCards; x++)
            {
                Card card = deck.GetRandomCard();
                deck.Cards.Remove(card);

                hand.Cards.Add(card);
            }

            return hand;
        }
    }
}
