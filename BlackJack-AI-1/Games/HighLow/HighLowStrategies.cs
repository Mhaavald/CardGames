using System;
using System.Collections.Generic;
using CardGames.Core;

namespace CardGames.GamesStudio.Games.HighLow
{
    /// <summary>
    /// Strategy that predicts higher if the current card value is low (1-6), lower if high (8-13)
    /// </summary>
    public class BasicHighLowStrategy : IHighLowStrategy
    {
        public string Name => "Basic";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // This is not used in High-Low but required by IStrategy
            return false;
        }
        
        public bool PredictHigher(Participant participant, GameContext context, Card currentCard)
        {
            var highLow = (HighLow)context.Game;
            int cardValue = highLow.GetCardValue(currentCard);
            
            // Middle value is 7 (in a range of 1-13)
            // Basic strategy: predict higher if current value is below 7, lower if above 7
            // For 7 itself, slightly favor predicting higher
            return cardValue < 7;
        }
    }
    
    /// <summary>
    /// Strategy that always predicts higher for cards <= 7, lower for cards > 7
    /// </summary>
    public class ThresholdHighLowStrategy : IHighLowStrategy
    {
        public string Name => "Threshold";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // This is not used in High-Low but required by IStrategy
            return false;
        }
        
        public bool PredictHigher(Participant participant, GameContext context, Card currentCard)
        {
            var highLow = (HighLow)context.Game;
            int cardValue = highLow.GetCardValue(currentCard);
            
            // Threshold at 7 (out of 13 possible values)
            return cardValue <= 7;
        }
    }
    
    /// <summary>
    /// Strategy that uses probability - calculates odds based on remaining cards
    /// </summary>
    public class ProbabilisticHighLowStrategy : IHighLowStrategy
    {
        public string Name => "Probabilistic";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // This is not used in High-Low but required by IStrategy
            return false;
        }
        
        public bool PredictHigher(Participant participant, GameContext context, Card currentCard)
        {
            var highLow = (HighLow)context.Game;
            int cardValue = highLow.GetCardValue(currentCard);
            
            // Calculate middle value of the deck (7 in a standard deck)
            int middleValue = 7;
            
            if (cardValue == middleValue)
            {
                // For the middle value, it's 50/50 - use a random choice
                return new Random().Next(2) == 0;
            }
            
            // For other values, predict based on position relative to middle
            return cardValue < middleValue;
        }
    }
    
    /// <summary>
    /// Risk-taking strategy that makes less obvious predictions
    /// </summary>
    public class RiskyHighLowStrategy : IHighLowStrategy
    {
        public string Name => "Risky";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // This is not used in High-Low but required by IStrategy
            return false;
        }
        
        public bool PredictHigher(Participant participant, GameContext context, Card currentCard)
        {
            var highLow = (HighLow)context.Game;
            int cardValue = highLow.GetCardValue(currentCard);
            
            // Risky strategy - predict higher for high cards (10-13) and lower for low cards (1-4)
            // This is counter to the obvious strategy
            if (cardValue >= 10)
                return true;  // Predict higher for already high cards
            else if (cardValue <= 4)
                return false; // Predict lower for already low cards
            else
                return cardValue < 7; // Use basic strategy for middle cards
        }
    }
}