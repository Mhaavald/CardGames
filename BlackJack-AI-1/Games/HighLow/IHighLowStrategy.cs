using System;
using CardGames.Core;

namespace CardGames.GamesStudio.Games.HighLow
{
    /// <summary>
    /// Interface for strategies specific to the High-Low game
    /// </summary>
    public interface IHighLowStrategy : IStrategy
    {
        /// <summary>
        /// Predicts whether the next card will be higher or lower than the current card
        /// </summary>
        /// <param name="participant">The participant making the prediction</param>
        /// <param name="context">The game context</param>
        /// <param name="currentCard">The current face-up card</param>
        /// <returns>True if predicting higher, False if predicting lower</returns>
        bool PredictHigher(Participant participant, GameContext context, Card currentCard);
    }
}