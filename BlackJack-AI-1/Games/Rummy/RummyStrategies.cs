using System;
using System.Collections.Generic;
using System.Linq;
using CardGames.Core;

namespace CardGames.GamesStudio.Games.Rummy
{
    /// <summary>
    /// Base class for Rummy strategies that provides common functionality
    /// </summary>
    public abstract class BaseRummyStrategy : IRummyStrategy
    {
        public abstract string Name { get; }
        
        /// <summary>
        /// This method is not used in Rummy but is required by the IStrategy interface
        /// </summary>
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // This is not used in Rummy but required by IStrategy
            return false;
        }
        
        public abstract bool DrawFromDeck(Participant participant, GameContext context, Card topDiscard);
        
        public abstract int SelectCardToDiscard(Participant participant, GameContext context);
        
        public abstract bool ShouldDeclare(Participant participant, GameContext context, RummyCombinations combinations);
        
        /// <summary>
        /// Analyzes a hand to find potential sets and runs
        /// </summary>
        protected RummyCombinations AnalyzeHand(List<Card> hand)
        {
            var result = new RummyCombinations();
            var workingHand = new List<Card>(hand);
            
            // First look for sets (same rank)
            var rankGroups = workingHand
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 3)
                .OrderByDescending(g => g.Count());
                
            foreach (var group in rankGroups)
            {
                var set = new RummySet { Cards = group.ToList() };
                result.Sets.Add(set);
                
                // Remove cards from working hand
                foreach (var card in set.Cards)
                {
                    workingHand.RemoveAll(c => c.Suit == card.Suit && c.Rank == card.Rank);
                }
            }
            
            // Then look for runs (consecutive ranks in same suit)
            var suitGroups = workingHand.GroupBy(c => c.Suit);
            foreach (var suitGroup in suitGroups)
            {
                var suitCards = suitGroup.OrderBy(c => GetCardValue(c)).ToList();
                
                // Find runs in this suit
                for (int i = 0; i < suitCards.Count; i++)
                {
                    var potentialRun = new List<Card> { suitCards[i] };
                    int currentValue = GetCardValue(suitCards[i]);
                    
                    // Look for consecutive cards
                    for (int j = i + 1; j < suitCards.Count; j++)
                    {
                        int nextValue = GetCardValue(suitCards[j]);
                        if (nextValue == currentValue + 1)
                        {
                            potentialRun.Add(suitCards[j]);
                            currentValue = nextValue;
                        }
                        else if (nextValue > currentValue + 1)
                        {
                            // Gap in sequence, stop looking
                            break;
                        }
                    }
                    
                    // If we found a valid run
                    if (potentialRun.Count >= 3)
                    {
                        var run = new RummyRun { Cards = potentialRun };
                        result.Runs.Add(run);
                        
                        // Remove cards from working hand
                        foreach (var card in potentialRun)
                        {
                            workingHand.RemoveAll(c => c.Suit == card.Suit && c.Rank == card.Rank);
                        }
                        
                        // Adjust index to skip past this run
                        i += potentialRun.Count - 1;
                    }
                }
            }
            
            // Remaining cards are unmatched
            result.UnmatchedCards = workingHand;
            
            return result;
        }
        
        /// <summary>
        /// Gets the numeric value of a card (Ace=1, Jack=11, Queen=12, King=13)
        /// </summary>
        protected int GetCardValue(Card card)
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
    }
    
    /// <summary>
    /// A basic strategy that focuses on completing sets
    /// </summary>
    public class SetFocusStrategy : BaseRummyStrategy
    {
        public override string Name => "Set Focus";
        
        public override bool DrawFromDeck(Participant participant, GameContext context, Card topDiscard)
        {
            // Take from discard if it would complete a pair or triple
            var rankCounts = participant.Hand.GroupBy(c => c.Rank).ToDictionary(g => g.Key, g => g.Count());
            
            // If we already have 1 or 2 cards of this rank, take it
            if (rankCounts.TryGetValue(topDiscard.Rank, out int count) && count >= 1 && count < 3)
                return false; // Don't draw from deck, take the discard
                
            // Otherwise draw from deck
            return true;
        }
        
        public override int SelectCardToDiscard(Participant participant, GameContext context)
        {
            var hand = participant.Hand;
            var combinations = AnalyzeHand(hand);
            
            // If no unmatched cards, discard any card (this should not happen if we can declare)
            if (combinations.UnmatchedCards.Count == 0)
                return 0;
                
            // Try to discard a card that doesn't help form sets
            var rankCounts = hand.GroupBy(c => c.Rank).ToDictionary(g => g.Key, g => g.Count());
            
            // Find single cards (not part of potential sets)
            var singleCards = hand.Where(c => rankCounts[c.Rank] == 1).ToList();
            if (singleCards.Any())
            {
                // Discard the highest value single card
                var cardToDiscard = singleCards.OrderByDescending(c => GetCardValue(c)).First();
                return hand.IndexOf(cardToDiscard);
            }
            
            // If all cards are part of potential sets, discard the highest card
            return hand.IndexOf(hand.OrderByDescending(c => GetCardValue(c)).First());
        }
        
        public override bool ShouldDeclare(Participant participant, GameContext context, RummyCombinations combinations)
        {
            // Declare when all cards form valid combinations
            return combinations.CanGoOut;
        }
    }
    
    /// <summary>
    /// A strategy that prioritizes creating runs
    /// </summary>
    public class RunFocusStrategy : BaseRummyStrategy
    {
        public override string Name => "Run Focus";
        
        public override bool DrawFromDeck(Participant participant, GameContext context, Card topDiscard)
        {
            var hand = participant.Hand;
            
            // Take from discard if it would extend a potential run
            foreach (var card in hand)
            {
                if (card.Suit == topDiscard.Suit)
                {
                    int cardValue = GetCardValue(card);
                    int discardValue = GetCardValue(topDiscard);
                    
                    // If the discard is adjacent to a card in our hand
                    if (Math.Abs(cardValue - discardValue) == 1)
                        return false; // Take from discard pile
                }
            }
            
            // Otherwise draw from deck
            return true;
        }
        
        public override int SelectCardToDiscard(Participant participant, GameContext context)
        {
            var hand = participant.Hand;
            var combinations = AnalyzeHand(hand);
            
            // If no unmatched cards, discard any card (this should not happen if we can declare)
            if (combinations.UnmatchedCards.Count == 0)
                return 0;
            
            // Group cards by suit
            var suitGroups = hand.GroupBy(c => c.Suit).ToDictionary(g => g.Key, g => g.ToList());
            
            // Find isolated cards (with no adjacent cards of the same suit)
            foreach (var card in hand)
            {
                bool hasAdjacentCard = false;
                int cardValue = GetCardValue(card);
                
                // Check if there are adjacent cards of the same suit
                if (suitGroups.TryGetValue(card.Suit, out var suitCards))
                {
                    foreach (var otherCard in suitCards)
                    {
                        if (card == otherCard) continue;
                        
                        int otherValue = GetCardValue(otherCard);
                        if (Math.Abs(cardValue - otherValue) == 1)
                        {
                            hasAdjacentCard = true;
                            break;
                        }
                    }
                }
                
                if (!hasAdjacentCard)
                {
                    return hand.IndexOf(card);
                }
            }
            
            // If all cards are potentially useful, discard the one with highest point value
            return hand.IndexOf(combinations.UnmatchedCards.OrderByDescending(GetCardPointValue).First());
        }
        
        public override bool ShouldDeclare(Participant participant, GameContext context, RummyCombinations combinations)
        {
            // Only declare if we have at least one run
            return combinations.CanGoOut && combinations.Runs.Count > 0;
        }
        
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
    
    /// <summary>
    /// A balanced strategy that looks for both sets and runs
    /// </summary>
    public class BalancedRummyStrategy : BaseRummyStrategy
    {
        public override string Name => "Balanced";
        
        public override bool DrawFromDeck(Participant participant, GameContext context, Card topDiscard)
        {
            var hand = participant.Hand;
            
            // Take from discard if it helps form a set
            var rankCounts = hand.GroupBy(c => c.Rank).ToDictionary(g => g.Key, g => g.Count());
            if (rankCounts.TryGetValue(topDiscard.Rank, out int count) && count >= 1 && count < 3)
                return false; // Take from discard pile
                
            // Or if it helps form a run
            foreach (var card in hand)
            {
                if (card.Suit == topDiscard.Suit)
                {
                    int cardValue = GetCardValue(card);
                    int discardValue = GetCardValue(topDiscard);
                    
                    // If the discard is adjacent to a card in our hand
                    if (Math.Abs(cardValue - discardValue) == 1)
                        return false; // Take from discard pile
                }
            }
            
            // Otherwise draw from deck
            return true;
        }
        
        public override int SelectCardToDiscard(Participant participant, GameContext context)
        {
            var hand = participant.Hand;
            var combinations = AnalyzeHand(hand);
            
            // If no unmatched cards, discard any card (this should not happen if we can declare)
            if (combinations.UnmatchedCards.Count == 0)
                return 0;
                
            // Discard a card that's not helping form combinations
            return hand.IndexOf(combinations.UnmatchedCards.OrderByDescending(GetCardPointValue).First());
        }
        
        public override bool ShouldDeclare(Participant participant, GameContext context, RummyCombinations combinations)
        {
            // Declare when all cards form valid combinations
            return combinations.CanGoOut;
        }
        
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
    
    /// <summary>
    /// A strategy that aims to minimize hand point value
    /// </summary>
    public class LowPointStrategy : BaseRummyStrategy
    {
        public override string Name => "Low Point";
        
        public override bool DrawFromDeck(Participant participant, GameContext context, Card topDiscard)
        {
            // Don't take high point cards from discard
            int discardValue = GetCardPointValue(topDiscard);
            if (discardValue > 5)
                return true; // Draw from deck instead
                
            // Otherwise, take low cards or cards that help sets/runs
            var hand = participant.Hand;
            
            // Check for potential sets
            var rankCounts = hand.GroupBy(c => c.Rank).ToDictionary(g => g.Key, g => g.Count());
            if (rankCounts.TryGetValue(topDiscard.Rank, out int count) && count >= 1)
                return false; // Take from discard pile
                
            // Check for potential runs
            foreach (var card in hand)
            {
                if (card.Suit == topDiscard.Suit)
                {
                    int cardValue = GetCardValue(card);
                    int discardNumValue = GetCardValue(topDiscard);
                    
                    // If the discard is adjacent to a card in our hand
                    if (Math.Abs(cardValue - discardNumValue) == 1)
                        return false; // Take from discard pile
                }
            }
            
            // If low value and doesn't help, still take it
            if (discardValue <= 3)
                return false; // Take from discard pile
                
            // Otherwise draw from deck
            return true;
        }
        
        public override int SelectCardToDiscard(Participant participant, GameContext context)
        {
            var hand = participant.Hand;
            
            // Discard the highest point value card that doesn't help form sets/runs
            var combinations = AnalyzeHand(hand);
            
            // If no unmatched cards, discard any card (this should not happen if we can declare)
            if (combinations.UnmatchedCards.Count == 0)
                return 0;
                
            // Discard highest value unmatched card
            return hand.IndexOf(combinations.UnmatchedCards.OrderByDescending(GetCardPointValue).First());
        }
        
        public override bool ShouldDeclare(Participant participant, GameContext context, RummyCombinations combinations)
        {
            // Declare when we can go out or when our unmatched cards total less than 10 points
            return combinations.CanGoOut || combinations.UnmatchedPoints <= 10;
        }
        
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