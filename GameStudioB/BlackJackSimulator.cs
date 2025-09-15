using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace GameStudioB
{
    public class BlackJackSimulator
    {
        private readonly IStrategy[] _strategies;
        private readonly bool _verboseOutput;
        private readonly int _numberOfDecks;

        public BlackJackSimulator(IStrategy[] strategies, bool verboseOutput = false, int numberOfDecks = 1)
        {
            _strategies = strategies;
            _verboseOutput = verboseOutput;
            _numberOfDecks = numberOfDecks;
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

            Console.WriteLine($"Starting BlackJack simulation with {numberOfRounds:N0} rounds...");
            Console.WriteLine($"Strategies: {string.Join(", ", _strategies.Select(s => s.Name))}");
            Console.WriteLine();

            // Calculate progress reporting frequency
            int progressInterval = CalculateProgressInterval(numberOfRounds);

            // Create a single deck for the entire simulation
            Deck deck = new Deck();

            // Run the specified number of rounds
            for (int round = 1; round <= numberOfRounds; round++)
            {
                // Reshuffle the deck if it's getting low
                if (deck.RemainingCards() < 15)
                {
                    deck = new Deck();
                }

                // Run a round for each strategy
                for (int i = 0; i < _strategies.Length; i++)
                {
                    var outcome = PlaySingleGame(deck, _strategies[i], $"Player {i + 1}");
                    UpdateStats(simulationResult.StrategyStats, outcome);
                }

                // Show progress at appropriate intervals
                if (round % progressInterval == 0 || round == numberOfRounds)
                {
                    double progressPercent = (double)round / numberOfRounds * 100;
                    Console.WriteLine($"Progress: {round:N0}/{numberOfRounds:N0} rounds completed ({progressPercent:F1}%)");
                    
                    // Show current strategy rankings at each progress update
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

            Console.WriteLine($"Starting BlackJack time-based simulation for {durationMinutes} minute{(durationMinutes != 1 ? "s" : "")}...");
            Console.WriteLine($"Strategies: {string.Join(", ", _strategies.Select(s => s.Name))}");
            Console.WriteLine($"Start time: {simulationResult.StartTime:HH:mm:ss}");
            Console.WriteLine($"End time: {simulationResult.EndTime:HH:mm:ss}");
            Console.WriteLine();

            // Create a single deck for the entire simulation
            Deck deck = new Deck();

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
                
                // Reshuffle the deck if it's getting low
                if (deck.RemainingCards() < 15)
                {
                    deck = new Deck();
                }

                // Run a round for each strategy
                for (int i = 0; i < _strategies.Length; i++)
                {
                    var outcome = PlaySingleGame(deck, _strategies[i], $"Player {i + 1}");
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
                    
                    // Show current strategy rankings at each progress update
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
            if (numberOfRounds <= 100) return 10;          // Show every 10th round for medium simulations
            if (numberOfRounds <= 1000) return 100;        // Show every 100th round for large simulations
            if (numberOfRounds <= 10000) return 500;       // Show every 500th round for very large simulations
            return 1000;                                  // Show every 1000th round for massive simulations
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

        private GameOutcome PlaySingleGame(Deck deck, IStrategy strategy, string playerName)
        {
            // Create dealer and player
            Player dealer = new Player("Dealer", true);
            Player player = new Player(playerName);

            // Deal initial cards
            player.AddCard(deck.DrawCard());
            dealer.AddCard(deck.DrawCard());
            player.AddCard(deck.DrawCard());
            dealer.AddCard(deck.DrawCard());

            // Check for immediate blackjack
            if (player.HasBlackjack)
            {
                if (_verboseOutput)
                {
                    Console.WriteLine($"{playerName} ({strategy.Name}) has Blackjack!");
                    DisplayHands(player, dealer, false);
                }

                return new GameOutcome
                {
                    PlayerName = playerName,
                    StrategyName = strategy.Name,
                    Result = dealer.HasBlackjack ? GameResult.Push : GameResult.Win,
                    HandValue = player.CalculateHandValue(),
                    DealerValue = dealer.CalculateHandValue(),
                    HasBlackjack = true
                };
            }

            // Player's turn - use strategy to decide whether to hit or stand
            while (!player.IsBusted && !player.HasStood)
            {
                bool shouldHit = strategy.DecideToHit(player, dealer);

                if (shouldHit)
                {
                    player.AddCard(deck.DrawCard());

                    if (_verboseOutput)
                    {
                        Console.WriteLine($"{playerName} ({strategy.Name}) hits and gets {player.Hand[player.Hand.Count - 1]}");
                        Console.WriteLine($"Hand value: {player.CalculateHandValue()}");
                    }
                }
                else
                {
                    player.HasStood = true;

                    if (_verboseOutput)
                    {
                        Console.WriteLine($"{playerName} ({strategy.Name}) stands with {player.CalculateHandValue()}");
                    }
                }
            }

            // If player busted, game is over
            if (player.IsBusted)
            {
                if (_verboseOutput)
                {
                    Console.WriteLine($"{playerName} ({strategy.Name}) busted with {player.CalculateHandValue()}");
                    DisplayHands(player, dealer, false);
                }

                return new GameOutcome
                {
                    PlayerName = playerName,
                    StrategyName = strategy.Name,
                    Result = GameResult.Bust,
                    HandValue = player.CalculateHandValue(),
                    DealerValue = dealer.CalculateHandValue(),
                    PlayerBusted = true
                };
            }

            // Dealer's turn
            while (dealer.CalculateHandValue() < 17)
            {
                dealer.AddCard(deck.DrawCard());

                if (_verboseOutput)
                {
                    Console.WriteLine($"Dealer hits and gets {dealer.Hand[dealer.Hand.Count - 1]}");
                    Console.WriteLine($"Dealer's hand value: {dealer.CalculateHandValue()}");
                }
            }

            // Determine winner
            int playerValue = player.CalculateHandValue();
            int dealerValue = dealer.CalculateHandValue();
            bool dealerBusted = dealer.IsBusted;
            GameResult result;

            if (dealerBusted)
            {
                result = GameResult.Win;
            }
            else if (playerValue > dealerValue)
            {
                result = GameResult.Win;
            }
            else if (playerValue < dealerValue)
            {
                result = GameResult.Loss;
            }
            else
            {
                result = GameResult.Push;
            }

            if (_verboseOutput)
            {
                string resultText = result switch
                {
                    GameResult.Win => "wins",
                    GameResult.Loss => "loses",
                    GameResult.Push => "pushes (tie)",
                    _ => "unknown result"
                };

                Console.WriteLine($"{playerName} ({strategy.Name}) {resultText} with {playerValue} vs dealer's {dealerValue}");
                DisplayHands(player, dealer, false);
                Console.WriteLine();
            }

            return new GameOutcome
            {
                PlayerName = playerName,
                StrategyName = strategy.Name,
                Result = result,
                HandValue = playerValue,
                DealerValue = dealerValue,
                DealerBusted = dealerBusted
            };
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

            if (outcome.HasBlackjack)
            {
                stats.Blackjacks++;
            }
        }

        private void DisplayHands(Player player, Player dealer, bool hideDealerCard)
        {
            if (_verboseOutput)
            {
                Console.WriteLine();
                player.DisplayHand();
                dealer.DisplayHand(hideDealerCard);
                Console.WriteLine();
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
            
            Console.WriteLine("Strategy".PadRight(20) + "Games".PadRight(10) + "Win%".PadRight(8) + "Bust%".PadRight(8) + "BJ%");
            Console.WriteLine(new string('-', 54));
            
            foreach (var strategy in rankedStrategies)
            {
                Console.WriteLine($"{strategy.StrategyName.PadRight(20)}" +
                                $"{strategy.TotalGames.ToString("N0").PadRight(10)}" +
                                $"{strategy.WinRate:F2}%".PadRight(8) +
                                $"{strategy.BustRate:F2}%".PadRight(8) +
                                $"{strategy.BlackjackRate:F2}%");
            }
        }

        public void DisplaySimulationSummary(SimulationResult result)
        {
            Console.WriteLine("=== BLACKJACK SIMULATION SUMMARY ===");
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