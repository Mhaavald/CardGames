using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CardGames.Core;

namespace CardGames.Simulation
{
    /// <summary>
    /// Abstract base class for all card game simulations
    /// </summary>
    public abstract class BaseSimulation
    {
        protected readonly bool _verboseOutput;
        protected readonly string _gameName;

        protected BaseSimulation(string gameName, bool verboseOutput = false)
        {
            _gameName = gameName ?? throw new ArgumentNullException(nameof(gameName));
            _verboseOutput = verboseOutput;
        }

        /// <summary>
        /// Runs a fixed number of simulation rounds
        /// </summary>
        public SimulationResult RunSimulation(int numberOfRounds)
        {
            var simulationResult = new SimulationResult
            {
                TotalRounds = numberOfRounds,
                StartTime = DateTime.Now
            };

            // Initialize strategy stats
            InitializeStrategyStats(simulationResult);

            Console.WriteLine($"Starting {_gameName} simulation with {numberOfRounds:N0} rounds...");
            Console.WriteLine($"Strategies: {GetStrategyNames()}");
            Console.WriteLine();

            // Calculate progress reporting frequency
            int progressInterval = CalculateProgressInterval(numberOfRounds);

            // Run the specified number of rounds
            for (int round = 1; round <= numberOfRounds; round++)
            {
                var roundResult = ExecuteRound(round);
                simulationResult.RoundResults.Add(roundResult);

                // Update strategy statistics
                UpdateStrategyStats(simulationResult.StrategyStats, roundResult);

                // Show progress at appropriate intervals
                if (round % progressInterval == 0 || round == numberOfRounds)
                {
                    double progressPercent = (double)round / numberOfRounds * 100;
                    Console.WriteLine($"Progress: {round:N0}/{numberOfRounds:N0} rounds completed ({progressPercent:F1}%)");
                    
                    // For very long simulations, show interim results
                    if (numberOfRounds >= 1000 && round % (numberOfRounds / 4) == 0)
                    {
                        Console.WriteLine("\n--- Interim Results ---");
                        DisplayInterimResults(simulationResult.StrategyStats);
                        Console.WriteLine("----------------------\n");
                    }
                }

                // Display round details for verbose output
                if (_verboseOutput && numberOfRounds <= 10)
                {
                    DisplayRoundResult(roundResult);
                }
            }

            simulationResult.EndTime = DateTime.Now;
            return simulationResult;
        }

        /// <summary>
        /// Runs a simulation for a specified duration rather than fixed number of rounds
        /// </summary>
        public SimulationResult RunTimeBasedSimulation(int durationMinutes = 1)
        {
            var simulationResult = new SimulationResult
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(durationMinutes)
            };

            // Initialize strategy stats
            InitializeStrategyStats(simulationResult);

            Console.WriteLine($"Starting {_gameName} time-based simulation for {durationMinutes} minute{(durationMinutes != 1 ? "s" : "")}...");
            Console.WriteLine($"Strategies: {GetStrategyNames()}");
            Console.WriteLine($"Start time: {simulationResult.StartTime:HH:mm:ss}");
            Console.WriteLine($"End time: {simulationResult.EndTime:HH:mm:ss}");
            Console.WriteLine();

            // Set up timing
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            TimeSpan duration = TimeSpan.FromMinutes(durationMinutes);
            TimeSpan reportInterval = CalculateTimeReportInterval(durationMinutes);
            DateTime nextReportTime = DateTime.Now.Add(reportInterval);
            int roundNumber = 0;
            
            // Run until time expires
            while (stopwatch.Elapsed < duration)
            {
                roundNumber++;
                
                var roundResult = ExecuteRound(roundNumber);
                simulationResult.RoundResults.Add(roundResult);
                UpdateStrategyStats(simulationResult.StrategyStats, roundResult);
                
                // Show progress at appropriate intervals
                if (DateTime.Now >= nextReportTime || stopwatch.Elapsed >= duration)
                {
                    double elapsedPercent = Math.Min(100, (stopwatch.Elapsed.TotalSeconds / duration.TotalSeconds) * 100);
                    double remainingSeconds = Math.Max(0, duration.TotalSeconds - stopwatch.Elapsed.TotalSeconds);
                    
                    Console.WriteLine($"Progress: {elapsedPercent:F1}% complete, " +
                                     $"{remainingSeconds:F0} seconds remaining, " +
                                     $"{roundNumber:N0} rounds completed " +
                                     $"({roundNumber / stopwatch.Elapsed.TotalSeconds:F1} rounds/sec)");
                                     
                    // For longer simulations, show interim results
                    if (durationMinutes >= 5 && stopwatch.Elapsed.TotalMinutes >= durationMinutes / 4 && 
                        stopwatch.Elapsed.TotalMinutes % (durationMinutes / 4) < reportInterval.TotalMinutes)
                    {
                        Console.WriteLine("\n--- Interim Results ---");
                        DisplayInterimResults(simulationResult.StrategyStats);
                        Console.WriteLine("----------------------\n");
                    }
                    
                    nextReportTime = DateTime.Now.Add(reportInterval);
                }

                // Display round details for verbose output
                if (_verboseOutput && roundNumber <= 10)
                {
                    DisplayRoundResult(roundResult);
                }
            }
            
            stopwatch.Stop();
            simulationResult.TotalRounds = roundNumber;
            simulationResult.EndTime = DateTime.Now;
            
            Console.WriteLine($"\nSimulation complete: {roundNumber:N0} rounds in {stopwatch.Elapsed.TotalMinutes:F2} minutes");
            Console.WriteLine($"Average speed: {roundNumber / stopwatch.Elapsed.TotalSeconds:F1} rounds/sec");
            
            return simulationResult;
        }

        /// <summary>
        /// Executes a single round of the simulation
        /// </summary>
        protected abstract RoundResult ExecuteRound(int roundNumber);

        /// <summary>
        /// Returns a string containing the names of all strategies used in the simulation
        /// </summary>
        protected abstract string GetStrategyNames();

        /// <summary>
        /// Initializes strategy statistics for this simulation
        /// </summary>
        protected abstract void InitializeStrategyStats(SimulationResult simulationResult);

        protected virtual void UpdateStrategyStats(List<StrategyStats> strategyStats, RoundResult roundResult)
        {
            foreach (var outcome in roundResult.Outcomes)
            {
                var stats = strategyStats.FirstOrDefault(s => s.StrategyName == outcome.StrategyName);
                if (stats == null)
                    continue;

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

        protected virtual void DisplayRoundResult(RoundResult roundResult)
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
        
        protected virtual void DisplayInterimResults(List<StrategyStats> stats)
        {
            var rankedStrategies = stats.OrderByDescending(s => s.WinRate).ToList();
            
            Console.WriteLine("Strategy".PadRight(20) + "Win%".PadRight(8) + "Bust%".PadRight(8) + "BJ%");
            Console.WriteLine(new string('-', 44));
            
            foreach (var strategy in rankedStrategies)
            {
                Console.WriteLine($"{strategy.StrategyName.PadRight(20)}" +
                                $"{strategy.WinRate:F2}%".PadRight(8) +
                                $"{strategy.BustRate:F2}%".PadRight(8) +
                                $"{strategy.BlackjackRate:F2}%");
            }
        }

        public virtual void DisplaySimulationSummary(SimulationResult result)
        {
            Console.WriteLine($"=== {_gameName.ToUpper()} SIMULATION SUMMARY ===");
            Console.WriteLine($"Total Rounds: {result.TotalRounds:N0}");
            Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F2} seconds");
            Console.WriteLine();

            Console.WriteLine("Strategy Performance:");
            Console.WriteLine("Strategy".PadRight(20) + "Games".PadRight(10) + "Wins".PadRight(10) + 
                            "Losses".PadRight(10) + "Pushes".PadRight(10) + "Busts".PadRight(10) + 
                            "BJ".PadRight(6) + "Win%".PadRight(8) + "Bust%".PadRight(8) + "BJ%");
            Console.WriteLine(new string('-', 100));

            foreach (var stats in result.StrategyStats.OrderByDescending(s => s.WinRate))
            {
                Console.WriteLine($"{stats.StrategyName.PadRight(20)}" +
                                $"{stats.TotalGames.ToString("N0").PadRight(10)}" +
                                $"{stats.Wins.ToString("N0").PadRight(10)}" +
                                $"{stats.Losses.ToString("N0").PadRight(10)}" +
                                $"{stats.Pushes.ToString("N0").PadRight(10)}" +
                                $"{stats.Busts.ToString("N0").PadRight(10)}" +
                                $"{stats.Blackjacks.ToString("N0").PadRight(6)}" +
                                $"{stats.WinRate:F2}%".PadRight(8) +
                                $"{stats.BustRate:F2}%".PadRight(8) +
                                $"{stats.BlackjackRate:F2}%");
            }

            Console.WriteLine();
            
            // Rank strategies by win rate
            var rankedStrategies = result.StrategyStats.OrderByDescending(s => s.WinRate).ToList();
            Console.WriteLine("Strategy Rankings (by Win Rate):");
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
    }
}