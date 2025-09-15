using System;
using System.Collections.Generic;

namespace CardGames.Core
{
    /// <summary>
    /// Interface for factories that create card games
    /// </summary>
    public interface ICardGameFactory
    {
        /// <summary>
        /// Creates a new instance of a card game
        /// </summary>
        CardGame CreateGame();

        /// <summary>
        /// Gets the name of the card game
        /// </summary>
        string GameName { get; }
        
        /// <summary>
        /// Creates strategies that are applicable for this card game
        /// </summary>
        IStrategy[] CreateDefaultStrategies();
    }
}