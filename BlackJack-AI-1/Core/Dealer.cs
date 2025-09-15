using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public class Dealer : Participant
    {
        public Dealer()
        {
            Name = "Dealer";
            Strategy = new DealerStrategy();
        }

        // Returns the dealer's up card (first card that is visible to players)
        public Card GetUpCard()
        {
            return Hand.Count > 0 ? Hand[0] : null;
        }
    }

    // Specific strategy for the dealer that follows standard blackjack rules
    public class DealerStrategy : IStrategy
    {
        public string Name => "Dealer";

        // Dealers in blackjack typically hit until they reach 17 or higher
        public bool DecideToHit(Participant participant, GameContext context)
        {
            // This logic is handled in the Blackjack.ProcessDealerTurn method
            // This method won't be directly called for the dealer in the current implementation
            // but is included for completeness

            if (context.Game is CardGames.GamesStudio.Games.Blackjack.Blackjack blackjackGame)
            {
                int handValue = blackjackGame.GetHandValue(participant);
                
                // Standard dealer rule: hit on 16 or less, stand on 17 or more
                return handValue < 17;
            }
            
            // Default behavior if not in a blackjack game
            return false;
        }
    }
}