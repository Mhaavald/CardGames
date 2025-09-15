using System;

namespace GameStudioB
{
    /// <summary>
    /// Conservative strategy that focuses on minimizing risk.
    /// Takes into account that dealer doesn't score on face cards or Aces.
    /// </summary>
    public class ConservativeAceResetStrategy : IAceResetStrategy
    {
        public string Name => "Conservative AceReset";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            // Not used in AceReset game
            return false;
        }
        
        public bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // If we're losing by a lot, take a risk
            if (currentScore < dealerScore - 10)
                return true;
                
            // If we're already winning, be cautious
            if (currentScore > dealerScore)
            {
                // Only draw if the lead is small and there are few cards left
                if (currentScore - dealerScore < 5 && remainingCards <= 5)
                    return true;
                    
                return false; // Keep our lead, don't risk it
            }
            
            // If we're behind but not by much, be cautious
            if (dealerScore - currentScore < 5)
            {
                // If the deck is almost empty, need to take a chance
                if (remainingCards <= 5)
                    return true;
                    
                return false;
            }
            
            // Factor in that dealer gets fewer points on average (no face cards)
            // This makes us slightly less likely to draw when behind
            if (remainingCards < 26 && currentScore > dealerScore * 0.7)
                return false;
            
            // In other cases, draw a card
            return true;
        }
    }
    
    /// <summary>
    /// Aggressive strategy that focuses on building a high score quickly.
    /// Takes into account that dealer doesn't score on face cards or Aces.
    /// </summary>
    public class AggressiveAceResetStrategy : IAceResetStrategy
    {
        public string Name => "Aggressive AceReset";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            // Not used in AceReset game
            return false;
        }
        
        public bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // If we're significantly ahead, we can be cautious
            if (currentScore > dealerScore + 12) // Reduced threshold due to dealer's disadvantage
                return false;
                
            // Always draw if behind
            if (currentScore <= dealerScore)
                return true;
                
            // If we're ahead but deck is getting small, play safe
            if (remainingCards <= 3 && currentScore > dealerScore)
                return false;
                
            // Generally be aggressive and draw cards
            // Dealer disadvantage means we can be more aggressive
            return true;
        }
    }
    
    /// <summary>
    /// Balanced strategy that adjusts based on game state.
    /// Takes into account that dealer doesn't score on face cards or Aces.
    /// </summary>
    public class BalancedAceResetStrategy : IAceResetStrategy
    {
        public string Name => "Balanced AceReset";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            // Not used in AceReset game
            return false;
        }
        
        public bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // Early game - build score
            if (remainingCards > 26) // More than half the deck
            {
                // Draw until we have a decent score
                // Can be slightly higher threshold since dealer has disadvantage
                return currentScore < 35;
            }
            
            // Mid game - strategic choices
            if (remainingCards > 10)
            {
                // Factor in dealer's disadvantage (approximately 40% of cards give 0 points)
                int effectiveDealerScore = (int)(dealerScore * 1.3); // Project dealer's potential
                
                // If we're winning, be a bit more cautious
                if (currentScore > effectiveDealerScore + 8)
                    return false;
                    
                // If we're losing, take risks
                if (currentScore < dealerScore)
                    return true;
                    
                // If close, draw cautiously
                return currentScore - dealerScore < 7; // Increased threshold
            }
            
            // End game - careful plays
            if (currentScore > dealerScore)
            {
                // Protect our lead, but consider dealer disadvantage
                // If very close, might still need to draw
                return currentScore - dealerScore < 3;
            }
            else
            {
                // Must draw to catch up
                return true;
            }
        }
    }
    
    /// <summary>
    /// Uses probability to make decisions based on potential to get an Ace
    /// and accounts for the dealer not scoring on face cards.
    /// </summary>
    public class ProbabilisticAceResetStrategy : IAceResetStrategy
    {
        private readonly Random random = new Random();
        
        public string Name => "Probabilistic AceReset";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            // Not used in AceReset game
            return false;
        }
        
        public bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            if (remainingCards == 0)
                return false; // Can't draw from empty deck
                
            // Calculate approximate probability of drawing an Ace
            double aceProbability = CalculateAceProbability(remainingCards);
            
            // Calculate probability of drawing a face card
            double faceCardProbability = CalculateFaceCardProbability(remainingCards);
            
            // Calculate dealer disadvantage factor
            // Dealer gets 0 points on approximately 16/52 cards (4 each of A,J,Q,K)
            double dealerDisadvantage = 16.0 / 52.0;
            
            // If we're ahead, be more cautious but factor in dealer disadvantage
            if (currentScore > dealerScore)
            {
                // Determine how much ahead we are relative to dealer's disadvantage
                double relativeLead = currentScore - dealerScore * (1.0 / (1.0 - dealerDisadvantage));
                
                if (relativeLead > 10)
                    return false; // Safe lead considering dealer disadvantage
                    
                // Only draw if chance of Ace is low
                return aceProbability < 0.12; // Slightly higher threshold
            }
            
            // If we're behind, calculate expected value
            int scoreDifference = dealerScore - currentScore;
            
            // If behind by a large amount, might need to risk it regardless of Ace probability
            if (scoreDifference > 12) // Reduced threshold due to dealer disadvantage
                return true;
                
            // Calculate expected value of drawing
            // Average non-face, non-ace card value is about 5.5
            // Dealer's average points per card is lower since they get 0 on face cards and aces
            double playerExpectedGain = 5.5 * (1 - aceProbability - faceCardProbability) + 10 * faceCardProbability - currentScore * aceProbability;
            
            // Draw if expected gain is positive
            if (playerExpectedGain > 0)
                return true;
                
            // Draw if near the end and we're losing
            if (remainingCards < 5 && currentScore < dealerScore)
                return true;
                
            return false;
        }
        
        private double CalculateAceProbability(int remainingCards)
        {
            // Estimate Aces remaining based on deck size
            double totalAces = 4.0; // Standard deck has 4 Aces
            double totalDeckSize = 52.0;
            
            // Estimate remaining Aces proportional to remaining deck
            double estimatedAcesRemaining = totalAces * (remainingCards / totalDeckSize);
            
            // Probability of drawing an Ace
            return estimatedAcesRemaining / remainingCards;
        }
        
        private double CalculateFaceCardProbability(int remainingCards)
        {
            // Estimate face cards (J,Q,K) remaining based on deck size
            double totalFaceCards = 12.0; // Standard deck has 12 face cards (3 types × 4 suits)
            double totalDeckSize = 52.0;
            
            // Estimate remaining face cards proportional to remaining deck
            double estimatedFaceCardsRemaining = totalFaceCards * (remainingCards / totalDeckSize);
            
            // Probability of drawing a face card
            return estimatedFaceCardsRemaining / remainingCards;
        }
    }
    
    /// <summary>
    /// Makes decisions completely randomly for comparison purposes.
    /// </summary>
    public class RandomAceResetStrategy : IAceResetStrategy
    {
        private readonly Random random = new Random();
        
        public string Name => "Random AceReset";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            // Not used in AceReset game
            return false;
        }
        
        public bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // 60% chance to draw, 40% chance to skip
            return random.NextDouble() < 0.6;
        }
    }
}