using System;
using System.Collections.Generic;
using System.Linq;
using CardGames.Core;

namespace CardGames.Simulation
{
    /// <summary>
    /// Generic simulation for card games
    /// </summary>
    public class CardGameSimulation
    {
        private readonly ICardGameFactory _gameFactory;
        private readonly IStrategy[] _strategies;
        private readonly bool _verboseOutput;

        public CardGameSimulation(ICardGameFactory gameFactory, IStrategy[] strategies, bool verboseOutput = false)
        {
            _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _verboseOutput = verboseOutput;
        }

        public SimulationResult RunSimulation(int numberOfRounds)
        {
            var simulationResult = new SimulationResult
            {
                TotalRounds = numberOfRounds,
                StartTime = DateTime.Now
            };

            // Initialize strategy stats
            foreach (var strategy in _strategies)
            {
                simulationResult.StrategyStats.Add(new StrategyStats
                {
                    StrategyName = strategy.Name
                });
            }

            Console.WriteLine($"Starting {_gameFactory.GameName} simulation with {numberOfRounds} rounds...");
            Console.WriteLine($"Strategies: {string.Join(", ", _strategies.Select(s => s.Name))}");
            Console.WriteLine();

            // Run the specified number of rounds
            for (int round = 1; round <= numberOfRounds; round++)
            {
                var roundResult = RunSingleRound(round);
                simulationResult.RoundResults.Add(roundResult);

                // Update strategy statistics
                UpdateStrategyStats(simulationResult.StrategyStats, roundResult);

                // Show progress for long simulations
                if (numberOfRounds >= 100 && round % (numberOfRounds / 10) == 0)
                {
                    Console.WriteLine($"Progress: {round}/{numberOfRounds} rounds completed ({(double)round / numberOfRounds * 100:F1}%)");
                }

                if (_verboseOutput && numberOfRounds <= 10)
                {
                    DisplayRoundResult(roundResult);
                }
            }

            simulationResult.EndTime = DateTime.Now;
            return simulationResult;
        }

        private RoundResult RunSingleRound(int roundNumber)
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

        private void UpdateStrategyStats(List<StrategyStats> strategyStats, RoundResult roundResult)
        {
            foreach (var outcome in roundResult.Outcomes)
            {
                var stats = strategyStats.First(s => s.StrategyName == outcome.StrategyName);
                stats.TotalGames++;

                switch (outcome.Result)
                {
                    case GameResult.Win:
                        stats.Wins++;
                        break;
                    case GameResult.Loss:
                        stats.Losses++;
                        break;
                    case GameResult.Push:
                        stats.Pushes++;
                        break;
                    case GameResult.Bust:
                        stats.Busts++;
                        stats.Losses++; // Bust counts as a loss
                        break;
                }

                if (outcome.HasBlackjack)
                {
                    stats.Blackjacks++;
                }
            }
        }

        private void DisplayRoundResult(RoundResult roundResult)
        {
            Console.WriteLine($"--- Round {roundResult.RoundNumber} ---");
            foreach (var outcome in roundResult.Outcomes)
            {
                string resultText = outcome.Result switch
                {
                    GameResult.Win => $"wins with {outcome.HandValue}",
                    GameResult.Loss => outcome.ParticipantBusted ? $"busted with {outcome.HandValue}" : 
                                      $"loses with {outcome.HandValue} against dealer's {outcome.DealerValue}",
                    GameResult.Push => $"pushes with {outcome.HandValue} (tie with dealer)",
                    GameResult.Bust => $"busted with {outcome.HandValue}",
                    _ => "unknown result"
                };

                if (outcome.HasBlackjack)
                {
                    resultText = "wins with Blackjack!";
                }

                Console.WriteLine($"{outcome.ParticipantName} ({outcome.StrategyName}) {resultText}");
            }
            Console.WriteLine();
        }

        public void DisplaySimulationSummary(SimulationResult result)
        {
            Console.WriteLine($"=== {_gameFactory.GameName.ToUpper()} SIMULATION SUMMARY ===");
            Console.WriteLine($"Total Rounds: {result.TotalRounds}");
            Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
            Console.WriteLine();

            Console.WriteLine("Strategy Performance:");
            Console.WriteLine("Strategy".PadRight(20) + "Games".PadRight(8) + "Wins".PadRight(8) + 
                            "Losses".PadRight(8) + "Pushes".PadRight(8) + "Busts".PadRight(8) + 
                            "BJ".PadRight(6) + "Win%".PadRight(8) + "Bust%".PadRight(8) + "BJ%");
            Console.WriteLine(new string('-', 90));

            foreach (var stats in result.StrategyStats)
            {
                Console.WriteLine($"{stats.StrategyName.PadRight(20)}" +
                                $"{stats.TotalGames.ToString().PadRight(8)}" +
                                $"{stats.Wins.ToString().PadRight(8)}" +
                                $"{stats.Losses.ToString().PadRight(8)}" +
                                $"{stats.Pushes.ToString().PadRight(8)}" +
                                $"{stats.Busts.ToString().PadRight(8)}" +
                                $"{stats.Blackjacks.ToString().PadRight(6)}" +
                                $"{stats.WinRate:F1}%".PadRight(8) +
                                $"{stats.BustRate:F1}%".PadRight(8) +
                                $"{stats.BlackjackRate:F1}%");
            }

            Console.WriteLine();
            
            // Rank strategies by win rate
            var rankedStrategies = result.StrategyStats.OrderByDescending(s => s.WinRate).ToList();
            Console.WriteLine("Strategy Rankings (by Win Rate):");
            for (int i = 0; i < rankedStrategies.Count; i++)
            {
                var strategy = rankedStrategies[i];
                Console.WriteLine($"{i + 1}. {strategy.StrategyName}: {strategy.WinRate:F1}% win rate");
            }
        }
    }
}