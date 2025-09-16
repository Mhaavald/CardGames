using System;
using System.Collections.Generic;
using CardGames.Core;
using CardGames.GamesStudio.Games.AceReset;
using CardGames.GamesStudio.Games.Blackjack;
using CardGames.GamesStudio.Games.HighLow;
using CardGames.GamesStudio.Games.Rummy;
using CardGames.Simulation;

namespace CardGames
{
    /// <summary>
    /// Main program for the Card Games application
    /// </summary>
    public class Program
    {
        // Registry of available card game factories
        private static readonly List<ICardGameFactory> GameFactories = new List<ICardGameFactory>
        {
            new BlackjackGameFactory(),
            new HighLowGameFactory(),
            new RummyGameFactory(),
            new AceResetGameFactory()
            // Add new game factories here when implemented
            // Example: new PokerGameFactory()
        };

        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Card Games AI Demo!");
            Console.WriteLine();

            // Select a game factory to use
            ICardGameFactory selectedGameFactory = SelectGameFactory();

            if (selectedGameFactory == null)
            {
                Console.WriteLine("No game selected. Exiting...");
                return;
            }

            // Create strategies for the selected game
            IStrategy[] strategies = selectedGameFactory.CreateDefaultStrategies();

            Console.WriteLine($"\nSelected Game: {selectedGameFactory.GameName}");
            Console.WriteLine($"Using strategies: {string.Join(", ", Array.ConvertAll(strategies, s => s.Name))}");
            Console.WriteLine();

            // Run the demos
            RunGameDemos(selectedGameFactory, strategies);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Presents a menu to select a game factory
        /// </summary>
        private static ICardGameFactory SelectGameFactory()
        {
            if (GameFactories.Count == 0)
            {
                Console.WriteLine("No card games are available.");
                return null;
            }

            // If there's only one game, select it automatically
            if (GameFactories.Count == 1)
            {
                return GameFactories[0];
            }

            // Display menu of available games
            Console.WriteLine("Available Card Games:");
            for (int i = 0; i < GameFactories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {GameFactories[i].GameName}");
            }

            Console.Write("\nSelect a game (enter number): ");
            if (int.TryParse(Console.ReadLine(), out int selection) && 
                selection > 0 && selection <= GameFactories.Count)
            {
                return GameFactories[selection - 1];
            }

            Console.WriteLine("Invalid selection. Using default game (Blackjack).");
            return GameFactories[0]; // Default to the first game (Blackjack)
        }

        /// <summary>
        /// Runs the demo modes for the selected game
        /// </summary>
        private static void RunGameDemos(ICardGameFactory gameFactory, IStrategy[] strategies)
        {
            bool exitDemos = false;
            
            while (!exitDemos)
            {
                Console.WriteLine("\nDemo Options:");
                Console.WriteLine("1. Traditional Gameplay (few rounds with output)");
                Console.WriteLine("2. Small Simulation (10 rounds with verbose output)");
                Console.WriteLine("3. Large Simulation (1000 rounds for statistical analysis)");
                Console.WriteLine("4. Time-based Simulation (1 minute)");
                Console.WriteLine("5. Exit Demo");
                
                Console.Write("\nSelect an option (enter number): ");
                string input = Console.ReadLine()?.Trim() ?? "";
                
                Console.WriteLine();
                
                switch (input)
                {
                    case "1":
                        Console.WriteLine($"=== DEMO 1: Traditional {gameFactory.GameName} Gameplay ===");
                        RunTraditionalDemo(gameFactory, strategies);
                        break;
                    case "2":
                        Console.WriteLine($"=== DEMO 2: Small {gameFactory.GameName} Simulation (Verbose) ===");
                        RunSmallSimulation(gameFactory, strategies);
                        break;
                    case "3":
                        Console.WriteLine($"=== DEMO 3: Large {gameFactory.GameName} Simulation (Statistical Analysis) ===");
                        RunLargeSimulation(gameFactory, strategies);
                        break;
                    case "4":
                        Console.WriteLine($"=== DEMO 4: Time-based {gameFactory.GameName} Simulation (1 minute) ===");
                        RunTimeBasedSimulation(gameFactory, strategies);
                        break;
                    case "5":
                        exitDemos = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                
                if (!exitDemos)
                {
                    Console.WriteLine("\n" + new string('=', 60));
                    Console.WriteLine("Press any key to continue to next demo...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.WriteLine($"Selected Game: {gameFactory.GameName}");
                }
            }
        }

        /// <summary>
        /// Runs a traditional gameplay demo with the selected game
        /// </summary>
        private static void RunTraditionalDemo(ICardGameFactory gameFactory, IStrategy[] strategies)
        {
            // Create and initialize the game
            CardGame game = gameFactory.CreateGame();
            game.SetupParticipants(strategies.Length, strategies);
            
            // Play a few rounds
            for (int round = 1; round <= 3; round++)
            {
                Console.WriteLine($"\n--- Round {round} ---");
                game.InitializeGame();
                game.PlayRound();
                game.DetermineWinners();
            }
        }

        /// <summary>
        /// Runs a small simulation with verbose output
        /// </summary>
        private static void RunSmallSimulation(ICardGameFactory gameFactory, IStrategy[] strategies)
        {
            var simulation = new CardGameSimulation(gameFactory, strategies, verboseOutput: true);
            var result = simulation.RunSimulation(10);
            simulation.DisplaySimulationSummary(result);
        }

        /// <summary>
        /// Runs a large simulation for statistical analysis
        /// </summary>
        private static void RunLargeSimulation(ICardGameFactory gameFactory, IStrategy[] strategies)
        {
            var simulation = new CardGameSimulation(gameFactory, strategies, verboseOutput: false);
            var result = simulation.RunSimulation(1000);
            simulation.DisplaySimulationSummary(result);
        }
        
        /// <summary>
        /// Runs a time-based simulation for 1 minute
        /// </summary>
        private static void RunTimeBasedSimulation(ICardGameFactory gameFactory, IStrategy[] strategies)
        {
            var simulation = new CardGameSimulation(gameFactory, strategies, verboseOutput: false);
            var result = simulation.RunTimeBasedSimulation(1); // 1 minute
            simulation.DisplaySimulationSummary(result);
        }
    }
}