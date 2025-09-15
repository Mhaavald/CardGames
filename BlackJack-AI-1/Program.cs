using System;
using System.Collections.Generic;
using CardGames.Core;
using CardGames.Blackjack;
using CardGames.Simulation;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Blackjack AI Demo!");
        Console.WriteLine();

        // Set up strategies
        IStrategy conservativeStrategy = new ConservativeStrategy();
        IStrategy aggressiveStrategy = new AggressiveStrategy();
        IStrategy superAggressiveStrategy = new SuperAggressiveStrategy();
        IStrategy basicStrategy = new BasicStrategy();
        IStrategy[] strategies = { conservativeStrategy, aggressiveStrategy, superAggressiveStrategy, basicStrategy };

        // Demo 1: Traditional gameplay (few rounds with output)
        Console.WriteLine("=== DEMO 1: Traditional Gameplay ===");
        RunTraditionalDemo(strategies);

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // Demo 2: Small simulation (10 rounds with verbose output)
        Console.WriteLine("=== DEMO 2: Small Simulation (Verbose) ===");
        RunSmallSimulation(strategies);

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // Demo 3: Large simulation (1000 rounds for statistical analysis)
        Console.WriteLine("=== DEMO 3: Large Simulation (Statistical Analysis) ===");
        RunLargeSimulation(strategies);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static void RunTraditionalDemo(IStrategy[] strategies)
    {
        // Create and initialize the game
        Blackjack game = new Blackjack();
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

    private static void RunSmallSimulation(IStrategy[] strategies)
    {
        var simulation = new BlackjackSimulation(strategies, verboseOutput: true);
        var result = simulation.RunSimulation(10);
        simulation.DisplaySimulationSummary(result);
    }

    private static void RunLargeSimulation(IStrategy[] strategies)
    {
        var simulation = new BlackjackSimulation(strategies, verboseOutput: false);
        var result = simulation.RunSimulation(1000);
        simulation.DisplaySimulationSummary(result);
    }
}