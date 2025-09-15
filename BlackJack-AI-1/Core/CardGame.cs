using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames.Core
{
    public abstract class CardGame
    {
        public string Name { get; }
        public List<Participant> Participants { get; } = new List<Participant>();
        public Dealer Dealer { get; } = new Dealer();
        public Deck Deck { get; protected set; }
        public abstract string Rules { get; }
        public abstract void InitializeGame();
        public abstract void PlayRound();
        public abstract void DetermineWinners();

        public void SetupParticipants(int count, IStrategy[] strategies)
        {
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