using System;
using System.Collections.Generic;
using CardGames.Core;

namespace CardGames.HighLow
{
    /// <summary>
    /// Factory for creating High-Low games
    /// </summary>
    public class HighLowGameFactory : ICardGameFactory
    {
        /// <summary>
        /// Gets the name of the card game
        /// </summary>
        public string GameName => "High-Low";

        /// <summary>
        /// Creates a new instance of a High-Low game
        /// </summary>
        public CardGame CreateGame()
        {
            return new HighLow();
        }

        /// <summary>
        /// Creates the default strategies for High-Low
        /// </summary>
        public IStrategy[] CreateDefaultStrategies()
        {
            return new IStrategy[]
            {
                new BasicHighLowStrategy(),
                new ThresholdHighLowStrategy(),
                new ProbabilisticHighLowStrategy(),
                new RiskyHighLowStrategy()
            };
        }
    }
}