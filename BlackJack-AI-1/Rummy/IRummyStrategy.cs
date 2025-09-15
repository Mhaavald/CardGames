using System;
using CardGames.Core;

namespace CardGames.Rummy
{
    /// <summary>
    /// Interface for strategies specific to the Rummy game
    /// </summary>
    public interface IRummyStrategy : IStrategy
    {
        /// <summary>
        /// Decides whether to draw from the deck or from the discard pile
        /// </summary>
        /// <param name="participant">The participant making the decision</param>
        /// <param name="context">The game context</param>
        /// <param name="topDiscard">The top card of the discard pile</param>
        /// <returns>True to draw from deck, false to take from discard pile</returns>
        bool DrawFromDeck(Participant participant, GameContext context, Card topDiscard);
        
        /// <summary>
        /// Decides which card to discard from the participant's hand
        /// </summary>
        /// <param name="participant">The participant making the decision</param>
        /// <param name="context">The game context</param>
        /// <returns>The index of the card to discard from the hand</returns>
        int SelectCardToDiscard(Participant participant, GameContext context);
        
        /// <summary>
        /// Decides whether the participant should declare and go out
        /// </summary>
        /// <param name="participant">The participant making the decision</param>
        /// <param name="context">The game context</param>
        /// <param name="combinations">The combinations detected in the participant's hand</param>
        /// <returns>True to declare, false to continue playing</returns>
        bool ShouldDeclare(Participant participant, GameContext context, RummyCombinations combinations);
    }
}