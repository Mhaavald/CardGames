using System;
using System.Collections.Generic;
using System.Linq;
using CardGames.Core;

namespace CardGames.Rummy
{
    /// <summary>
    /// Represents a set (3 or 4 cards of the same rank)
    /// </summary>
    public class RummySet
    {
        public List<Card> Cards { get; set; } = new List<Card>();
        public string Rank => Cards.Count > 0 ? Cards[0].Rank : string.Empty;
        public bool IsValid => Cards.Count >= 3 && Cards.Count <= 4 && Cards.All(c => c.Rank == Rank);
        
        public override string ToString()
        {
            return $"Set of {Rank}s: {string.Join(", ", Cards.Select(c => c.ToString()))}";
        }
    }
    
    /// <summary>
    /// Represents a run (3+ consecutive cards of the same suit)
    /// </summary>
    public class RummyRun
    {
        public List<Card> Cards { get; set; } = new List<Card>();
        public string Suit => Cards.Count > 0 ? Cards[0].Suit : string.Empty;
        
        /// <summary>
        /// Checks if the run is valid (3+ consecutive cards of the same suit)
        /// </summary>
        public bool IsValid 
        { 
            get 
            {
                if (Cards.Count < 3 || !Cards.All(c => c.Suit == Suit))
                    return false;
                
                // Sort cards by rank
                var sortedCards = Cards.OrderBy(c => GetCardValue(c)).ToList();
                
                // Check if they are consecutive
                for (int i = 1; i < sortedCards.Count; i++)
                {
                    if (GetCardValue(sortedCards[i]) != GetCardValue(sortedCards[i-1]) + 1)
                        return false;
                }
                
                return true;
            } 
        }
        
        private int GetCardValue(Card card)
        {
            if (int.TryParse(card.Rank, out int value))
                return value;
                
            return card.Rank switch
            {
                "Ace" => 1,
                "Jack" => 11,
                "Queen" => 12,
                "King" => 13,
                _ => 0
            };
        }
        
        public override string ToString()
        {
            var sortedCards = Cards.OrderBy(c => GetCardValue(c)).ToList();
            return $"Run of {Suit}: {string.Join(", ", sortedCards.Select(c => c.ToString()))}";
        }
    }
    
    /// <summary>
    /// Holds all the possible combinations (sets and runs) detected in a player's hand
    /// </summary>
    public class RummyCombinations
    {
        public List<RummySet> Sets { get; set; } = new List<RummySet>();
        public List<RummyRun> Runs { get; set; } = new List<RummyRun>();
        
        /// <summary>
        /// The remaining cards that aren't part of any valid combination
        /// </summary>
        public List<Card> UnmatchedCards { get; set; } = new List<Card>();
        
        /// <summary>
        /// Gets the point value of unmatched cards
        /// </summary>
        public int UnmatchedPoints
        {
            get
            {
                int total = 0;
                foreach (var card in UnmatchedCards)
                {
                    total += GetCardPointValue(card);
                }
                return total;
            }
        }
        
        /// <summary>
        /// Calculates if all cards can be arranged in valid combinations (for going out)
        /// </summary>
        public bool CanGoOut => UnmatchedCards.Count == 0 && (Sets.Count > 0 || Runs.Count > 0);
        
        /// <summary>
        /// Gets the point value of a card
        /// </summary>
        private int GetCardPointValue(Card card)
        {
            if (int.TryParse(card.Rank, out int value))
                return value;
                
            return card.Rank switch
            {
                "Ace" => 1,
                "Jack" => 10,
                "Queen" => 10,
                "King" => 10,
                _ => 0
            };
        }
    }
}