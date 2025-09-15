using System;
using System.Collections.Generic;
using System.Linq;
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

    public class StrategyStats
    {
        public string StrategyName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Pushes { get; set; }
        public int Busts { get; set; }
        public int Blackjacks { get; set; }
        
        /// <summary>
        /// Gets the win rate as a percentage
        /// </summary>
        public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;
        
        /// <summary>
        /// Gets the bust rate as a percentage
        /// </summary>
        public double BustRate => TotalGames > 0 ? (double)Busts / TotalGames * 100 : 0;
        
        /// <summary>
        /// Gets the blackjack rate as a percentage
        /// </summary>
        public double BlackjackRate => TotalGames > 0 ? (double)Blackjacks / TotalGames * 100 : 0;
    }

    public class SimulationResult
    {
        /// <summary>
        /// Gets or sets the total number of rounds in the simulation
        /// </summary>
        public int TotalRounds { get; set; }
        
        /// <summary>
        /// Gets or sets the list of strategy statistics
        /// </summary>
        public List<StrategyStats> StrategyStats { get; set; } = new List<StrategyStats>();
        
        /// <summary>
        /// Gets or sets the list of round results
        /// </summary>
        public List<RoundResult> RoundResults { get; set; } = new List<RoundResult>();
        
        /// <summary>
        /// Gets or sets the simulation start time
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the simulation end time
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// Gets the total duration of the simulation
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;
    }
}