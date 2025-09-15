using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GameStudioB
{
    public class AceResetSimulator
    {
        private readonly IAceResetStrategy[] _strategies;
        private readonly bool _verboseOutput;

        public AceResetSimulator(IAceResetStrategy[] strategies, bool verboseOutput = false)
        {
            _strategies = strategies;
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

            Console.WriteLine($"Starting AceReset simulation with {numberOfRounds:N0} rounds...");
            Console.WriteLine($"Strategies: {string.Join(", ", _strategies.Select(s => s.Name))}");
            Console.WriteLine();

            // Calculate progress reporting frequency
            int progressInterval = CalculateProgressInterval(numberOfRounds);

            // Run the specified number of rounds
            for (int round = 1; round <= numberOfRounds; round++)
            {
                // Run a round for each strategy
                for (int i = 0; i < _strategies.Length; i++)
                {
                    var outcome = PlaySingleGame(_strategies[i], $"Player {i + 1}");
                    UpdateStats(simulationResult.StrategyStats, outcome);
                }

                // Show progress at appropriate intervals
                if (round % progressInterval == 0 || round == numberOfRounds)
                {
                    double progressPercent = (double)round / numberOfRounds * 100;
                    Console.WriteLine($"Progress: {round:N0}/{numberOfRounds:N0} rounds completed ({progressPercent:F1}%)");
                    
                    // Show current strategy rankings
                    DisplayCurrentRankings(simulationResult.StrategyStats);
                    
                    // For very long simulations, show more detailed interim results
                    if (numberOfRounds >= 10000 && round % (numberOfRounds / 4) == 0)
                    {
                        Console.WriteLine("\n--- Detailed Interim Results ---");
                        DisplayInterimResults(simulationResult.StrategyStats);
                        Console.WriteLine("-----------------------------\n");
                    }
                }
            }

            simulationResult.EndTime = DateTime.Now;
            return simulationResult;
        }
        
        public SimulationResult RunTimeBasedSimulation(int durationMinutes = 1)
        {
            var simulationResult = new SimulationResult
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(durationMinutes)
            };

            // Initialize strategy stats
            foreach (var strategy in _strategies)
            {
                simulationResult.StrategyStats.Add(new StrategyStats
                {
                    StrategyName = strategy.Name
                });
            }

            Console.WriteLine($"Starting AceReset time-based simulation for {durationMinutes} minute{(durationMinutes != 1 ? "s" : "")}...");
            Console.WriteLine($"Strategies: {string.Join(", ", _strategies.Select(s => s.Name))}");
            Console.WriteLine($"Start time: {simulationResult.StartTime:HH:mm:ss}");
            Console.WriteLine($"End time: {simulationResult.EndTime:HH:mm:ss}");
            Console.WriteLine();

            // Set up timing
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            TimeSpan duration = TimeSpan.FromMinutes(durationMinutes);
            TimeSpan reportInterval = CalculateTimeReportInterval(durationMinutes);
            DateTime nextReportTime = DateTime.Now.Add(reportInterval);
            int totalRounds = 0;
            
            // Run until time expires
            while (stopwatch.Elapsed < duration)
            {
                totalRounds++;
                
                // Run a round for each strategy
                for (int i = 0; i < _strategies.Length; i++)
                {
                    var outcome = PlaySingleGame(_strategies[i], $"Player {i + 1}");
                    UpdateStats(simulationResult.StrategyStats, outcome);
                }
                
                // Show progress at appropriate intervals
                if (DateTime.Now >= nextReportTime || stopwatch.Elapsed >= duration)
                {
                    double elapsedPercent = Math.Min(100, (stopwatch.Elapsed.TotalSeconds / duration.TotalSeconds) * 100);
                    double remainingSeconds = Math.Max(0, duration.TotalSeconds - stopwatch.Elapsed.TotalSeconds);
                    
                    Console.WriteLine($"Progress: {elapsedPercent:F1}% complete, " +
                                     $"{remainingSeconds:F0} seconds remaining, " +
                                     $"{totalRounds:N0} rounds completed " +
                                     $"({totalRounds / stopwatch.Elapsed.TotalSeconds:F1} rounds/sec)");
                    
                    // Show current strategy rankings
                    DisplayCurrentRankings(simulationResult.StrategyStats);
                                     
                    // For longer simulations, show more detailed interim results
                    if (durationMinutes >= 5 && stopwatch.Elapsed.TotalMinutes >= durationMinutes / 4 && 
                        stopwatch.Elapsed.TotalMinutes % (durationMinutes / 4) < reportInterval.TotalMinutes)
                    {
                        Console.WriteLine("\n--- Detailed Interim Results ---");
                        DisplayInterimResults(simulationResult.StrategyStats);
                        Console.WriteLine("-----------------------------\n");
                    }
                    
                    nextReportTime = DateTime.Now.Add(reportInterval);
                }
            }
            
            stopwatch.Stop();
            simulationResult.TotalRounds = totalRounds;
            simulationResult.EndTime = DateTime.Now;
            
            Console.WriteLine($"\nSimulation complete: {totalRounds:N0} rounds in {stopwatch.Elapsed.TotalMinutes:F2} minutes");
            Console.WriteLine($"Average speed: {totalRounds / stopwatch.Elapsed.TotalSeconds:F1} rounds/sec");
            
            return simulationResult;
        }

        private int CalculateProgressInterval(int numberOfRounds)
        {
            // Adjust progress reporting frequency based on simulation size
            if (numberOfRounds <= 10) return 1;            // Show every round for small simulations
            if (numberOfRounds <= 100) return 10;          // Every 10th round for medium simulations
            if (numberOfRounds <= 1000) return 100;        // Every 100th round for large simulations
            if (numberOfRounds <= 10000) return 500;       // Every 500th round for very large simulations
            return 1000;                                  // Every 1000th round for massive simulations
        }
        
        private TimeSpan CalculateTimeReportInterval(int durationMinutes)
        {
            // Adjust reporting frequency based on simulation duration
            if (durationMinutes <= 1) return TimeSpan.FromSeconds(10);     // Every 10 seconds for ?1 min
            if (durationMinutes <= 5) return TimeSpan.FromSeconds(30);     // Every 30 seconds for ?5 min
            if (durationMinutes <= 15) return TimeSpan.FromMinutes(1);     // Every minute for ?15 min
            if (durationMinutes <= 60) return TimeSpan.FromMinutes(5);     // Every 5 minutes for ?1 hour
            return TimeSpan.FromMinutes(15);                              // Every 15 minutes for >1 hour
        }

        private GameOutcome PlaySingleGame(IAceResetStrategy strategy, string playerName)
        {
            var game = new AceResetGame(false); // Non-interactive mode
            return game.PlayGame(strategy);
        }

        private void UpdateStats(List<StrategyStats> strategyStats, GameOutcome outcome)
        {
            var stats = strategyStats.FirstOrDefault(s => s.StrategyName == outcome.StrategyName);
            if (stats == null)
                return;

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
        }
        
        private void DisplayCurrentRankings(List<StrategyStats> stats)
        {
            var rankedStrategies = stats.OrderByDescending(s => s.WinRate).ToList();
            
            Console.WriteLine("\nCurrent Strategy Rankings:");
            for (int i = 0; i < rankedStrategies.Count; i++)
            {
                var strategy = rankedStrategies[i];
                // Only show win rate if enough games have been played
                string winRateStr = strategy.TotalGames >= 10 ? $"{strategy.WinRate:F2}%" : "N/A";
                Console.WriteLine($"{i + 1}. {strategy.StrategyName}: {winRateStr} win rate ({strategy.TotalGames:N0} games)");
            }
            Console.WriteLine();
        }
        
        private void DisplayInterimResults(List<StrategyStats> stats)
        {
            var rankedStrategies = stats.OrderByDescending(s => s.WinRate).ToList();
            
            Console.WriteLine("Strategy".PadRight(30) + "Games".PadRight(10) + "Win%".PadRight(8));
            Console.WriteLine(new string('-', 50));
            
            foreach (var strategy in rankedStrategies)
            {
                Console.WriteLine($"{strategy.StrategyName.PadRight(30)}" +
                                $"{strategy.TotalGames.ToString("N0").PadRight(10)}" +
                                $"{strategy.WinRate:F2}%".PadRight(8));
            }
        }

        public void DisplaySimulationSummary(SimulationResult result)
        {
            Console.WriteLine("=== ACERESET SIMULATION SUMMARY ===");
            Console.WriteLine($"Total Rounds: {result.TotalRounds:N0}");
            Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
            Console.WriteLine();

            Console.WriteLine("Strategy Performance:");
            Console.WriteLine("Strategy".PadRight(30) + "Games".PadRight(10) + "Wins".PadRight(10) + 
                            "Losses".PadRight(10) + "Win%".PadRight(8));
            Console.WriteLine(new string('-', 68));

            foreach (var stats in result.StrategyStats.OrderByDescending(s => s.WinRate))
            {
                Console.WriteLine($"{stats.StrategyName.PadRight(30)}" +
                                $"{stats.TotalGames.ToString("N0").PadRight(10)}" +
                                $"{stats.Wins.ToString("N0").PadRight(10)}" +
                                $"{stats.Losses.ToString("N0").PadRight(10)}" +
                                $"{stats.WinRate:F2}%".PadRight(8));
            }

            Console.WriteLine();
            
            // Rank strategies by win rate
            var rankedStrategies = result.StrategyStats.OrderByDescending(s => s.WinRate).ToList();
            Console.WriteLine("Final Strategy Rankings (by Win Rate):");
            for (int i = 0; i < rankedStrategies.Count; i++)
            {
                var strategy = rankedStrategies[i];
                Console.WriteLine($"{i + 1}. {strategy.StrategyName}: {strategy.WinRate:F2}% win rate");
            }
            
            // Calculate statistical confidence
            if (result.TotalRounds >= 1000)
            {
                Console.WriteLine("\nStatistical Confidence:");
                Console.WriteLine($"With {result.TotalRounds:N0} rounds per strategy, results have a higher statistical significance.");
                Console.WriteLine("Differences of more than 1% in win rates are likely to be statistically meaningful.");
            }
        }
    }
}