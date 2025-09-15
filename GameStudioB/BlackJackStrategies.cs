using System;

namespace GameStudioB
{
    // Conservative strategy: Only hits on 16 or less
    public class ConservativeStrategy : IStrategy
    {
        public string Name => "Conservative";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            return player.CalculateHandValue() <= 16;
        }
    }

    // Aggressive strategy: Hits on 17 or less
    public class AggressiveStrategy : IStrategy
    {
        public string Name => "Aggressive";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            return player.CalculateHandValue() <= 17;
        }
    }
    
    // Very aggressive strategy: Hits on 18 or less
    public class VeryAggressiveStrategy : IStrategy
    {
        public string Name => "Very Aggressive";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            return player.CalculateHandValue() <= 18;
        }
    }
    
    // Basic strategy: Makes decisions based on player's hand and dealer's visible card
    public class BasicStrategy : IStrategy
    {
        public string Name => "Basic Strategy";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            int playerValue = player.CalculateHandValue();
            
            // Always hit on 11 or less
            if (playerValue <= 11)
                return true;
                
            // Never hit on 17 or more
            if (playerValue >= 17)
                return false;
                
            // If dealer has cards and is showing their hand
            if (dealer.Hand.Count > 0)
            {
                Card dealerVisibleCard = dealer.IsDealer && dealer.Hand.Count > 1 ? dealer.Hand[1] : dealer.Hand[0];
                int dealerVisibleValue = dealerVisibleCard.GetBlackjackValue();
                
                // If dealer shows 7 or higher
                if (dealerVisibleValue >= 7)
                {
                    // Hit on 16 or less
                    return playerValue <= 16;
                }
                else // dealer shows 2-6
                {
                    // Stand on 12 or more
                    return playerValue < 12;
                }
            }
            
            // Default to conservative if we don't have dealer info
            return playerValue <= 16;
        }
    }
    
    // Random strategy: Makes random decisions (for comparison)
    public class RandomStrategy : IStrategy
    {
        private Random random = new Random();
        
        public string Name => "Random";
        
        public bool DecideToHit(Player player, Player dealer)
        {
            // Completely random, but avoid hitting on very high values
            if (player.CalculateHandValue() >= 19)
                return false;
                
            return random.Next(2) == 0; // 50% chance to hit
        }
    }
}