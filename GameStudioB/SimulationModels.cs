using System;
using System.Collections.Generic;

namespace GameStudioB
{
    public enum GameResult
    {
        Win,
        Loss,
        Push, // Tie
        Bust
    }

    public class GameOutcome
    {
        public string PlayerName { get; set; } = string.Empty;
        public string StrategyName { get; set; } = string.Empty;
        public GameResult Result { get; set; }
        public int HandValue { get; set; }
        public int DealerValue { get; set; }
        public bool PlayerBusted { get; set; }
        public bool DealerBusted { get; set; }
        public bool HasBlackjack { get; set; }
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
        
        public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;
        public double BustRate => TotalGames > 0 ? (double)Busts / TotalGames * 100 : 0;
        public double BlackjackRate => TotalGames > 0 ? (double)Blackjacks / TotalGames * 100 : 0;
    }

    public class SimulationResult
    {
        public int TotalRounds { get; set; }
        public List<StrategyStats> StrategyStats { get; set; } = new List<StrategyStats>();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }
}