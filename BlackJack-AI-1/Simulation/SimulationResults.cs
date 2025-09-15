using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardGames.Core;

namespace CardGames.Simulation
{
    public enum GameResult
    {
        Win,
        Loss,
        Push,  // Tie
        Bust
    }

    public class GameOutcome
    {
        public string ParticipantName { get; set; } = string.Empty;
        public string StrategyName { get; set; } = string.Empty;
        public int HandValue { get; set; }
        public int DealerValue { get; set; }
        public GameResult Result { get; set; }
        public bool HasBlackjack { get; set; }
        public bool DealerBusted { get; set; }
        public bool ParticipantBusted { get; set; }
    }

    public class RoundResult
    {
        public int RoundNumber { get; set; }
        public List<GameOutcome> Outcomes { get; set; } = new List<GameOutcome>();
        public int DealerValue { get; set; }
        public bool DealerBusted { get; set; }
        public bool DealerHasBlackjack { get; set; }
    }
}