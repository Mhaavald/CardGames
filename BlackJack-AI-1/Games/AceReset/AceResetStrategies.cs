using CardGames.Core;

namespace CardGames.GamesStudio.Games.AceReset
{
    /// <summary>
    /// Base class for Ace Reset strategies implementing common functionality
    /// </summary>
    public abstract class BaseAceResetStrategy : IAceResetStrategy
    {
        public abstract string Name { get; }
        
        public abstract bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards);
        
        // Implementation of the base IStrategy interface
        // This is needed for compatibility with the framework but not used in AceReset
        public virtual bool DecideToHit(Participant participant, GameContext context)
        {
            // This method isn't used in AceReset but needs to be implemented
            // Default implementation always returns false
            return false;
        }
    }

    /// <summary>
    /// Conservative strategy that avoids drawing when ahead
    /// </summary>
    public class ConservativeStrategy : BaseAceResetStrategy
    {
        public override string Name => "Conservative";
        
        public override bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // If we're ahead by 5 or more points, skip to preserve the lead
            if (currentScore > dealerScore + 5)
                return false;
                
            // If our score is 15 or higher, be cautious
            if (currentScore >= 15)
                return false;
                
            // Otherwise, draw a card
            return true;
        }
    }
    
    /// <summary>
    /// Aggressive strategy that usually draws unless far ahead
    /// </summary>
    public class AggressiveStrategy : BaseAceResetStrategy
    {
        public override string Name => "Aggressive";
        
        public override bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // If we're ahead by 10 or more points and near end game, be conservative
            if (currentScore > dealerScore + 10 && remainingCards < 10)
                return false;
                
            // If our score is very high, be a bit careful
            if (currentScore >= 25)
                return false;
                
            // Otherwise, draw a card
            return true;
        }
    }
    
    /// <summary>
    /// Always draw strategy - never skips
    /// </summary>
    public class AlwaysDrawStrategy : BaseAceResetStrategy
    {
        public override string Name => "Always Draw";
        
        public override bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // Always draw, regardless of situation
            return true;
        }
    }
    
    /// <summary>
    /// Risk-aware strategy that adapts based on game state
    /// </summary>
    public class RiskAwareStrategy : BaseAceResetStrategy
    {
        public override string Name => "Risk Aware";
        
        public override bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // Calculate the remaining cards in the deck that would reset our score (Aces)
            int numberOfAces = 4;
            double aceProbability = (double)numberOfAces / 52; // Approximate probability
            double riskOfResetting = aceProbability * remainingCards;
            
            // If we're significantly ahead, don't risk it
            if (currentScore > dealerScore + 15)
                return false;
                
            // If our score is low, we should draw
            if (currentScore < 10)
                return true;
                
            // If we're ahead and there are few cards remaining, be conservative
            if (currentScore > dealerScore && remainingCards < 10)
                return false;
                
            // If dealer is ahead, take more risks
            if (dealerScore > currentScore)
                return true;
                
            // Otherwise, make a balanced decision
            if (currentScore >= 15)
                return false;
                
            return true;
        }
    }
    
    /// <summary>
    /// Catch-up strategy that is aggressive when behind but conservative when ahead
    /// </summary>
    public class CatchUpStrategy : BaseAceResetStrategy
    {
        public override string Name => "Catch Up";
        
        public override bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards)
        {
            // If we're behind, always draw
            if (currentScore < dealerScore)
                return true;
                
            // If we're ahead, be more conservative
            if (currentScore > dealerScore)
            {
                // If there are few cards remaining and we're ahead, skip
                if (remainingCards < 15)
                    return false;
                    
                // If our score is high enough, be cautious
                if (currentScore >= 20)
                    return false;
            }
            
            // Otherwise draw
            return true;
        }
    }
}