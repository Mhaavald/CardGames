using System;
using System.Collections.Generic;
using System.Linq;
using CardGames.Core;

namespace CardGames.Simulation
{
    /// <summary>
    /// Generic simulation for card games using the factory pattern
    /// </summary>
    public class CardGameSimulation : BaseSimulation
    {
        private readonly ICardGameFactory _gameFactory;
        private readonly IStrategy[] _strategies;

        public CardGameSimulation(ICardGameFactory gameFactory, IStrategy[] strategies, bool verboseOutput = false)
            : base(gameFactory?.GameName ?? "CardGame", verboseOutput)
        {
            _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        }

        /// <summary>
        /// Executes a single round of the simulation using the game factory
        /// </summary>
        protected override RoundResult ExecuteRound(int roundNumber)
        {
            // Create and setup the game
            var game = _gameFactory.CreateGame();
            game.SetupParticipants(_strategies.Length, _strategies);

            // Play the round
            game.InitializeGame();
            game.PlayRound();
            
            // Get the results using IGameResults if available
            if (game is IGameResults gameWithResults)
            {
                return gameWithResults.GetRoundResult(roundNumber);
            }
            
            // Default empty round result if the game doesn't implement IGameResults
            return new RoundResult 
            { 
                RoundNumber = roundNumber,
                Outcomes = new List<GameOutcome>()
            };
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