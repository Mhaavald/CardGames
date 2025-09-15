using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public class Deck
    {
        private Stack<Card> cards;
        
        /// <summary>
        /// Number of cards remaining in the deck
        /// </summary>
        public int Count => cards.Count;

        /// <summary>
        /// Creates a new standard deck with the specified number of 52-card decks
        /// </summary>
        public Deck(int numberOfDecks = 1)
        {
            InitializeDeck(numberOfDecks);
        }
        
        /// <summary>
        /// Creates a deck from an existing list of cards
        /// </summary>
        public Deck(List<Card> existingCards)
        {
            cards = new Stack<Card>(existingCards);
        }

        private void InitializeDeck(int numberOfDecks)
        {
            cards = new Stack<Card>();
            // Initialize with standard 52 cards per deck
            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

            for (int d = 0; d < numberOfDecks; d++)
            {
                foreach (var suit in suits)
                {
                    foreach (var rank in ranks)
                    {
                        cards.Push(new Card { Suit = suit, Rank = rank });
                    }
                }
            }
            Shuffle();
        }

        public void Shuffle()
        {
            // Implement Fisher-Yates shuffle algorithm
            var rnd = new Random();
            var list = cards.ToList();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
            cards = new Stack<Card>(list);
        }

        public Card DealCard() => cards.Pop();
    }
}