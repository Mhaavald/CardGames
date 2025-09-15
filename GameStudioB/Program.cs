using System;
using System.Collections.Generic;

namespace GameStudioB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to BlackJack Game!");
            Console.WriteLine("-------------------------");

            BlackJackGame game = new BlackJackGame();
            bool continuePlaying = true;

            while (continuePlaying)
            {
                Console.WriteLine("\nStarting a new game of BlackJack...");
                game.PlayGame();

                Console.Write("\nWould you like to play again? (Y/N): ");
                string answer = Console.ReadLine()?.Trim().ToUpper() ?? "N";
                continuePlaying = answer == "Y" || answer == "YES";
            }

            Console.WriteLine("\nThank you for playing BlackJack!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}