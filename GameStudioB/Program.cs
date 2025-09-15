using System;
using System.Collections.Generic;
using System.Linq;

namespace GameStudioB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to BlackJack Game!");
            Console.WriteLine("-------------------------");
            DisplayMainMenu();
        }

        private static void DisplayMainMenu()
        {
            bool exitProgram = false;

            while (!exitProgram)
            {
                Console.WriteLine("\nMain Menu:");
                Console.WriteLine("1. Play BlackJack");
                Console.WriteLine("2. Run Strategy Simulation");
                Console.WriteLine("3. Exit");
                Console.Write("\nSelect an option: ");

                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
                {
                    case "1":
                        PlayGame();
                        break;
                    case "2":
                        SimulationMenu();
                        break;
                    case "3":
                        exitProgram = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }

            Console.WriteLine("\nThank you for playing BlackJack!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void PlayGame()
        {
            BlackJackGame game = new BlackJackGame(true);
            bool continuePlaying = true;

            while (continuePlaying)
            {
                Console.WriteLine("\nStarting a new game of BlackJack...");
                game.PlayGame();

                Console.Write("\nWould you like to play again? (Y/N): ");
                string answer = Console.ReadLine()?.Trim().ToUpper() ?? "N";
                continuePlaying = answer == "Y" || answer == "YES";
            }
        }

        private static void SimulationMenu()
        {
            Console.WriteLine("\nBlackJack Strategy Simulation");
            Console.WriteLine("----------------------------");

            // Create the strategies
            IStrategy[] strategies = new IStrategy[]
            {
                new ConservativeStrategy(),
                new AggressiveStrategy(),
                new VeryAggressiveStrategy(),
                new BasicStrategy(),
                new RandomStrategy()
            };

            Console.WriteLine("\nAvailable Strategies:");
            for (int i = 0; i < strategies.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {strategies[i].Name}");
            }

            bool runSimulation = true;
            while (runSimulation)
            {
                Console.WriteLine("\nSimulation Options:");
                Console.WriteLine("1. Quick Simulation (10 rounds)");
                Console.WriteLine("2. Standard Simulation (100 rounds)");
                Console.WriteLine("3. Long Simulation (1000 rounds)");
                Console.WriteLine("4. Very Long Simulation (10000 rounds)");
                Console.WriteLine("5. Time-based Simulation (1 minute)");
                Console.WriteLine("6. Custom Time-based Simulation");
                Console.WriteLine("7. Return to Main Menu");
                Console.Write("\nSelect an option: ");

                string choice = Console.ReadLine()?.Trim() ?? "";
                int rounds = 0;
                int minutes = 0;
                bool verbose = false;
                bool isTimeBased = false;

                switch (choice)
                {
                    case "1":
                        rounds = 10;
                        verbose = true;
                        break;
                    case "2":
                        rounds = 100;
                        verbose = false;
                        break;
                    case "3":
                        rounds = 1000;
                        verbose = false;
                        break;
                    case "4":
                        rounds = 10000;
                        verbose = false;
                        Console.WriteLine("\nStarting very long simulation. This may take a while...");
                        break;
                    case "5":
                        isTimeBased = true;
                        minutes = 1;
                        verbose = false;
                        break;
                    case "6":
                        isTimeBased = true;
                        minutes = GetCustomDurationMinutes();
                        verbose = false;
                        break;
                    case "7":
                        runSimulation = false;
                        continue;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        continue;
                }

                if (rounds > 0 || (isTimeBased && minutes > 0))
                {
                    // Clear the console before starting a new simulation for better readability
                    Console.Clear();
                    Console.WriteLine("== BlackJack Strategy Simulation ==");
                    
                    if (isTimeBased)
                    {
                        RunTimeBasedSimulation(strategies, minutes, verbose);
                    }
                    else
                    {
                        RunSimulation(strategies, rounds, verbose);
                    }
                }
            }
        }
        
        private static int GetCustomDurationMinutes()
        {
            int minutes = 1;
            bool validInput = false;
            
            while (!validInput)
            {
                Console.Write("\nEnter simulation duration in minutes (1-60): ");
                string input = Console.ReadLine()?.Trim() ?? "1";
                
                if (int.TryParse(input, out minutes) && minutes > 0 && minutes <= 60)
                {
                    validInput = true;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number between 1 and 60.");
                    minutes = 1; // Default
                }
            }
            
            return minutes;
        }

        private static void RunSimulation(IStrategy[] strategies, int rounds, bool verbose)
        {
            Console.WriteLine($"\nRunning BlackJack simulation with {rounds:N0} rounds per strategy...");
            
            var startTime = DateTime.Now;
            Console.WriteLine($"Simulation started at: {startTime:HH:mm:ss}");
            Console.WriteLine("=======================================");
            
            var simulator = new BlackJackSimulator(strategies, verbose);
            var result = simulator.RunSimulation(rounds);
            
            Console.WriteLine("=======================================");
            var endTime = DateTime.Now;
            Console.WriteLine($"Simulation completed at: {endTime:HH:mm:ss}");
            Console.WriteLine($"Total time: {(endTime - startTime).TotalMinutes:F2} minutes");
            
            // Add a divider before the final results
            Console.WriteLine("\n================================================");
            Console.WriteLine("               FINAL RESULTS                   ");
            Console.WriteLine("================================================");
            
            simulator.DisplaySimulationSummary(result);
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        
        private static void RunTimeBasedSimulation(IStrategy[] strategies, int minutes, bool verbose)
        {
            Console.WriteLine($"\nRunning BlackJack time-based simulation for {minutes} minute{(minutes != 1 ? "s" : "")}...");
            
            var startTime = DateTime.Now;
            Console.WriteLine($"Simulation started at: {startTime:HH:mm:ss}");
            Console.WriteLine("=======================================");
            
            var simulator = new BlackJackSimulator(strategies, verbose);
            var result = simulator.RunTimeBasedSimulation(minutes);
            
            Console.WriteLine("=======================================");
            Console.WriteLine($"Simulation completed at: {DateTime.Now:HH:mm:ss}");
            
            // Add a divider before the final results
            Console.WriteLine("\n================================================");
            Console.WriteLine("               FINAL RESULTS                   ");
            Console.WriteLine("================================================");
            
            simulator.DisplaySimulationSummary(result);
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}