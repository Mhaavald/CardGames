using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardGames.Core;

namespace CardGames.GamesStudio.Games.Blackjack
{
    // Blackjack-specific strategies
    public class ConservativeStrategy : IStrategy
    {
        public string Name => "Conservative";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // Conservative strategy: only hit when total is 16 or below
            var blackjack = (Blackjack)context.Game;
            int handValue = blackjack.GetHandValue(participant);
            return handValue <= 16;
        }
    }

    public class AggressiveStrategy : IStrategy
    {
        public string Name => "Aggressive";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // Aggressive strategy: hit when total is 17 or below
            var blackjack = (Blackjack)context.Game;
            int handValue = blackjack.GetHandValue(participant);
            return handValue <= 17;
        }
    }

    public class SuperAggressiveStrategy : IStrategy
    {
        public string Name => "Super Aggressive";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // Super aggressive strategy: hit when total is 18 or below
            var blackjack = (Blackjack)context.Game;
            int handValue = blackjack.GetHandValue(participant);
            return handValue <= 18;
        }
    }

    public class BasicStrategy : IStrategy
    {
        public string Name => "Basic Strategy";
        
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // Simplified basic strategy based on player hand and dealer's up card
            var blackjack = (Blackjack)context.Game;
            int handValue = blackjack.GetHandValue(participant);
            int dealerUpCard = GetDealerUpCardValue(context.Dealer);

            // If we have 11 or less, always hit
            if (handValue <= 11) return true;

            // If we have 17 or more, never hit
            if (handValue >= 17) return false;

            // Basic strategy for 12-16 based on dealer's up card
            if (handValue >= 12 && handValue <= 16)
            {
                // Hit if dealer shows 7, 8, 9, 10, or Ace
                return dealerUpCard >= 7 || dealerUpCard == 1;
            }

            // For other cases, be conservative
            return handValue <= 16;
        }

        private int GetDealerUpCardValue(Dealer dealer)
        {
            if (dealer.Hand.Count == 0) return 10; // Default assumption

            var upCard = dealer.Hand[0]; // First card is face up
            if (upCard.Rank == "Ace") return 1;
            if (upCard.Rank == "King" || upCard.Rank == "Queen" || upCard.Rank == "Jack") return 10;
            if (int.TryParse(upCard.Rank, out int value)) return value;
            return 10; // Default
        }
    }
}