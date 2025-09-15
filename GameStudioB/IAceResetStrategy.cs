namespace GameStudioB
{
    public interface IAceResetStrategy : IStrategy
    {
        /// <summary>
        /// Decides whether to draw a card or skip in the AceReset game.
        /// </summary>
        /// <param name="currentScore">Current player's score</param>
        /// <param name="dealerScore">Current dealer's score</param>
        /// <param name="remainingCards">Number of cards remaining in the deck</param>
        /// <returns>True if the player should draw a card, false to skip</returns>
        bool DecideToDrawCard(int currentScore, int dealerScore, int remainingCards);
        
        // The IStrategy interface's method will be implemented but not used in this game
    }
}