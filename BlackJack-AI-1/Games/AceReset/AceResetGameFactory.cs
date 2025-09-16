using CardGames.Core;

namespace CardGames.GamesStudio.Games.AceReset
{
    /// <summary>
    /// Factory for creating Ace Reset games
    /// </summary>
    public class AceResetGameFactory : ICardGameFactory
    {
        /// <summary>
        /// Gets the name of the card game
        /// </summary>
        public string GameName => "Ace Reset";

        /// <summary>
        /// Creates a new instance of an Ace Reset game
        /// </summary>
        public CardGame CreateGame()
        {
            return new AceReset();
        }

        /// <summary>
        /// Creates the default strategies for Ace Reset
        /// </summary>
        public IStrategy[] CreateDefaultStrategies()
        {
            return new IStrategy[]
            {
                new ConservativeStrategy(),
                new AggressiveStrategy(),
                new AlwaysDrawStrategy(),
                new RiskAwareStrategy(),
                new CatchUpStrategy()
            };
        }
    }
}