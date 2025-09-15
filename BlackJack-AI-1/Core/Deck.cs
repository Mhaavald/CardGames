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

        public Deck(int numberOfDecks = 1)
        {
            InitializeDeck(numberOfDecks);
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