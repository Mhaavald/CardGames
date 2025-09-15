using CardGames.Core;

namespace CardGames.GamesStudio.Games.Blackjack
{
    /// <summary>
    /// Factory for creating Blackjack games
    /// </summary>
    public class BlackjackGameFactory : ICardGameFactory
    {
        /// <summary>
        /// Gets the name of the card game
        /// </summary>
        public string GameName => "Blackjack";

        /// <summary>
        /// Creates a new instance of a Blackjack game
        /// </summary>
        public CardGame CreateGame()
        {
            return new Blackjack();
        }

        /// <summary>
        /// Creates the default strategies for Blackjack
        /// </summary>
        public IStrategy[] CreateDefaultStrategies()
        {
            return new IStrategy[]
            {
                new ConservativeStrategy(),
                new AggressiveStrategy(),
                new SuperAggressiveStrategy(),
                new BasicStrategy()
            };
        }
    }
}