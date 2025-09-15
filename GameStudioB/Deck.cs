using System;
using System.Collections.Generic;

namespace GameStudioB
{
    public class Deck
    {
        private List<Card> cards;
        private Random random;

        public Deck()
        {
            Initialize();
        }

        public void Initialize()
        {
            cards = new List<Card>();
            random = new Random();

            // Create a standard deck of 52 cards
            foreach (Card.SuitValue suit in Enum.GetValues(typeof(Card.SuitValue)))
            {
                foreach (Card.RankValue rank in Enum.GetValues(typeof(Card.RankValue)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }

            Shuffle();
        }

        public void Shuffle()
        {
            // Fisher-Yates shuffle algorithm
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Card temp = cards[k];
                cards[k] = cards[n];
                cards[n] = temp;
            }
        }

        public Card DrawCard()
        {
            if (cards.Count == 0)
            {
                // If the deck is empty, reinitialize it
                Initialize();
            }

            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        public int RemainingCards()
        {
            return cards.Count;
        }
    }
}