using System;
using System.Collections.Generic;
using CardGames.Core;

namespace CardGames.Rummy
{
    /// <summary>
    /// Factory for creating Rummy games
    /// </summary>
    public class RummyGameFactory : ICardGameFactory
    {
        /// <summary>
        /// Gets the name of the card game
        /// </summary>
        public string GameName => "Simple Rummy";

        /// <summary>
        /// Creates a new instance of a Rummy game
        /// </summary>
        public CardGame CreateGame()
        {
            return new Rummy();
        }

        /// <summary>
        /// Creates the default strategies for Rummy
        /// </summary>
        public IStrategy[] CreateDefaultStrategies()
        {
            return new IStrategy[]
            {
                new SetFocusStrategy(),
                new RunFocusStrategy(),
                new BalancedRummyStrategy(),
                new LowPointStrategy()
            };
        }
    }
}