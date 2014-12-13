using System;

namespace PlayingCards
{
	public enum POKERSCORE 
	{ 
		HighCard, 
		Pair,
		TwoPair, 
		ThreeOfAKind,
		Straight, 
		Flush, 
		FullHouse, 
		FourOfAKind, 
		StraightFlush,
		RoyalFlush 
	}
	
	public static class PokerLogic
	{
		private static CardValues HighCard;
		
		// flush is when all of the suits are the same
		private static bool isFlush(Hand h)
		{
			if (h.Cards[0].Suit == h.Cards[1].Suit &&
			    h.Cards[1].Suit == h.Cards[2].Suit &&
			    h.Cards[2].Suit == h.Cards[3].Suit &&
			    h.Cards[3].Suit == h.Cards[4].Suit)
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			
			return false;
		}
	
		// make sure the rank differs by one
		// we can do this since the Hand is 
		// sorted by this point
		private static bool isStraight(Hand h)
		{
			if (h.Cards[0].Value == h.Cards[1].Value-1 &&
			    h.Cards[1].Value == h.Cards[2].Value-1 &&
			    h.Cards[2].Value == h.Cards[3].Value-1 &&
			    h.Cards[3].Value == h.Cards[4].Value-1)
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			
			return false;
		}
		
		// must be flush and straight and
		// be certain cards. No wonder I have
		private static bool isRoyalFlush(Hand h)
		{
			if (isStraight(h) && isFlush(h) &&
			      h.Cards[0].Value == CardValues.Ten &&
			      h.Cards[1].Value == CardValues.Jack &&
			      h.Cards[2].Value == CardValues.Queen &&
			      h.Cards[3].Value == CardValues.King &&
			      h.Cards[4].Value == CardValues.Ace)
			{
				HighCard = CardValues.Ace;
				return true;
			}
			
			return false;
		}
	
		private static bool isStraightFlush(Hand h)
		{
			if (isStraight(h) && isFlush(h))
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			return false;
		}
		
		/*
		 * Two choices here, the first four cards
		 * must match in rank, or the second four
		 * must match in rank. Only because the hand
		 * is sorted
		 */
		private static bool isFourOfAKind(Hand h)
		{
			if (h.Cards[0].Value == h.Cards[1].Value &&
				h.Cards[1].Value == h.Cards[2].Value &&
				h.Cards[2].Value == h.Cards[3].Value)
			{
				HighCard = h.Cards[3].Value;
				return true;
			}
			
			if (h.Cards[1].Value == h.Cards[2].Value &&
				h.Cards[2].Value == h.Cards[3].Value &&
				h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			return false;
		}
		
		/*
		 * two choices here, the pair is in the
		 * front of the hand or in the back of the
		 * hand, because it is sorted
		 */
		private static bool isFullHouse(Hand h)
		{
			if (h.Cards[0].Value == h.Cards[1].Value &&
				h.Cards[2].Value == h.Cards[3].Value &&
				h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[2].Value;
				return true;
			}
				
			if (h.Cards[0].Value == h.Cards[1].Value &&
				h.Cards[1].Value == h.Cards[2].Value &&
				h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[0].Value;
				return true;
			}
			
			return false;
		}
	
		/*
		 * three choices here, first three cards match
		 * middle three cards match or last three cards
		 * match
		 */
		private static bool isThreeOfAKind(Hand h)
		{
			if (h.Cards[0].Value == h.Cards[1].Value &&
				h.Cards[1].Value == h.Cards[2].Value)
			{
				HighCard = h.Cards[1].Value;
				return true;
			}
			
			if (h.Cards[1].Value == h.Cards[2].Value &&
				h.Cards[2].Value == h.Cards[3].Value)
			{
				HighCard = h.Cards[2].Value;
				return true;
			}
			
			if (h.Cards[2].Value == h.Cards[3].Value &&
				h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[3].Value;
				return true;
			}
			
			return false;
		}
		  
		/*
		 * three choices, two pair in the front,
		 * separated by a single card or
		 * two pair in the back
		 */
		private static bool isTwoPair(Hand h)
		{
			if (h.Cards[0].Value == h.Cards[1].Value &&
				h.Cards[2].Value == h.Cards[3].Value)
			{
				HighCard = h.Cards[3].Value;
				return true;
			}
			
			if (h.Cards[0].Value == h.Cards[1].Value &&
				h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			
			if (h.Cards[1].Value == h.Cards[2].Value &&
				h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			
			return false;
		}
		
		private static bool isPair(Hand h)
		{
			if (h.Cards[0].Value == h.Cards[1].Value)
			{
				HighCard = h.Cards[0].Value;
				return true;
			}
			if (h.Cards[1].Value == h.Cards[2].Value)
			{
				HighCard = h.Cards[1].Value;
				return true;
			}
			if (h.Cards[2].Value == h.Cards[3].Value)
			{
				HighCard = h.Cards[2].Value;
				return true;
			}
			if (h.Cards[3].Value == h.Cards[4].Value)
			{
				HighCard = h.Cards[4].Value;
				return true;
			}
			
			return false;
		}
	
		// must be in order of hands and must be
		// mutually exclusive choices
		public static POKERSCORE score(Hand h)
		{
			h.Cards.Sort();
			
			if (isRoyalFlush(h))
				return POKERSCORE.RoyalFlush;
			else if (isStraightFlush(h))
				return POKERSCORE.StraightFlush;
			else if (isFourOfAKind(h))
				return POKERSCORE.FourOfAKind;
			else if (isFullHouse(h))
				return POKERSCORE.FullHouse;
			else if (isFlush(h))
				return POKERSCORE.Flush;
			else if (isStraight(h))
				return POKERSCORE.Straight;
			else if (isThreeOfAKind(h))
				return POKERSCORE.ThreeOfAKind;
			else if (isTwoPair(h))
				return POKERSCORE.TwoPair;
			else if (isPair(h))
				return POKERSCORE.Pair;
			else
				return POKERSCORE.HighCard;
		}
		
		public static int CompareHands(Hand h1, Hand h2)
		{
			if(score(h1) > score(h2)) return 1;
			if(score(h1) < score(h2)) return 2;
			
			score(h1);
			CardValues tempCard = HighCard;
			score(h2);
			
			if(tempCard > HighCard) return 1;
			if(tempCard < HighCard) return 2;
				
			return 0;
		}
	}
}