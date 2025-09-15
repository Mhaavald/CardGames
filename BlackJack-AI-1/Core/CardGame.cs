using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public abstract class CardGame
    {
        /// <summary>
        /// Gets the name of the card game
        /// </summary>
        public virtual string Name { get; }
        
        /// <summary>
        /// List of participants (players) in the game
        /// </summary>
        public List<Participant> Participants { get; } = new List<Participant>();
        
        /// <summary>
        /// The dealer for the game
        /// </summary>
        public Dealer Dealer { get; } = new Dealer();
        
        /// <summary>
        /// The deck of cards for the game
        /// </summary>
        public Deck Deck { get; protected set; }
        
        /// <summary>
        /// Gets the rules of the game as a string
        /// </summary>
        public abstract string Rules { get; }
        
        /// <summary>
        /// Initializes the game state, shuffles the deck, etc.
        /// </summary>
        public abstract void InitializeGame();
        
        /// <summary>
        /// Plays one round of the game
        /// </summary>
        public abstract void PlayRound();
        
        /// <summary>
        /// Determines and displays the winners of the current round
        /// </summary>
        public abstract void DetermineWinners();

        /// <summary>
        /// Sets up the participants (players) for the game
        /// </summary>
        /// <param name="count">Number of participants</param>
        /// <param name="strategies">Strategies for each participant</param>
        public void SetupParticipants(int count, IStrategy[] strategies)
        {
            Participants.Clear();
            for (int i = 0; i < count; i++)
            {
                var participant = new Participant
                {
                    Name = $"Player {i + 1}",
                    Strategy = strategies[i]
                };
                Participants.Add(participant);
            }
        }
    }
}