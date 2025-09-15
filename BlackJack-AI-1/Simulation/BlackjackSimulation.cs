using System;
using System.Collections.Generic;
using System.Linq;
using CardGames.Core;
using CardGames.GamesStudio.Games.Blackjack;

namespace CardGames.Simulation
{
    /// <summary>
    /// Specialized simulation for Blackjack games
    /// </summary>
    public class BlackjackSimulation : BaseSimulation
    {
        private readonly IStrategy[] _strategies;

        public BlackjackSimulation(IStrategy[] strategies, bool verboseOutput = false)
            : base("Blackjack", verboseOutput)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        }

        /// <summary>
        /// Executes a single round of Blackjack simulation
        /// </summary>
        protected override RoundResult ExecuteRound(int roundNumber)
        {
            // Create and setup the Blackjack game
            var game = new Blackjack();
            game.SetupParticipants(_strategies.Length, _strategies);

            // Play the round
            game.InitializeGame();
            game.PlayRound();
            
            // Get the round result
            return game.GetRoundResult(roundNumber);
        }

        /// <summary>
        /// Returns a string with all strategy names
        /// </summary>
        protected override string GetStrategyNames()
        {
            return string.Join(", ", _strategies.Select(s => s.Name));
        }

        /// <summary>
        /// Initializes the strategy statistics for this simulation
        /// </summary>
        protected override void InitializeStrategyStats(SimulationResult simulationResult)
        {
            foreach (var strategy in _strategies)
            {
                simulationResult.StrategyStats.Add(new StrategyStats
                {
                    StrategyName = strategy.Name
                });
            }
        }
    }
}