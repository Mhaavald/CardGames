using System;
using System.Collections.Generic;
using CardGames.Simulation;

namespace CardGames.Core
{
    /// <summary>
    /// Interface for card games that can provide round results for simulation
    /// </summary>
    public interface IGameResults
    {
        /// <summary>
        /// Gets the results of a round
        /// </summary>
        /// <param name="roundNumber">The round number</param>
        /// <returns>A RoundResult object containing the outcomes</returns>
        RoundResult GetRoundResult(int roundNumber);
    }
}