using System;
using System.Collections.Generic;
using System.Threading;

namespace GameStudioB
{
    public class BlackJackGame
    {
        private Deck deck;
        private Player dealer;
        private Player player;
        private bool interactiveMode;

        public BlackJackGame(bool interactive = true)
        {
            deck = new Deck();
            dealer = new Player("Dealer", true);
            player = new Player("Player");
            interactiveMode = interactive;
        }

        public GameOutcome PlayGame(IStrategy? playerStrategy = null)
        {
            // Reset game state
            player.ClearHand();
            dealer.ClearHand();
            
            // Assign strategy if provided
            player.Strategy = playerStrategy;
            
            if (deck.RemainingCards() < 15)
            {
                deck.Initialize(); // Reshuffle if the deck is getting low
            }

            // Deal initial cards
            player.AddCard(deck.DrawCard());
            dealer.AddCard(deck.DrawCard());
            player.AddCard(deck.DrawCard());
            dealer.AddCard(deck.DrawCard());

            // Display initial hands
            if (interactiveMode)
            {
                Console.WriteLine("\nDealing cards...");
                Thread.Sleep(1000);
                
                dealer.DisplayHand(true); // Hide dealer's first card
                Console.WriteLine();
                player.DisplayHand();
            }

            // Check for immediate BlackJack
            if (player.HasBlackjack)
            {
                if (interactiveMode)
                {
                    Console.WriteLine("\nBlackjack! You win!");
                    dealer.DisplayHand(); // Show dealer's full hand
                }
                
                return CreateGameOutcome(GameResult.Win, true);
            }

            // Player's turn
            GameResult result = PlayerTurn();
            
            if (result == GameResult.Bust)
            {
                return CreateGameOutcome(result);
            }
            
            // If player hasn't busted, dealer plays
            if (!player.IsBusted)
            {
                DealerTurn();
                result = DetermineWinner();
            }
            
            return CreateGameOutcome(result);
        }

        private GameResult PlayerTurn()
        {
            bool playerTurnDone = false;

            while (!playerTurnDone)
            {
                bool wantsToHit;
                
                if (player.Strategy != null)
                {
                    // AI player
                    wantsToHit = player.Strategy.DecideToHit(player, dealer);
                    
                    if (interactiveMode)
                    {
                        Console.WriteLine($"\nPlayer ({player.Strategy.Name}) decides to {(wantsToHit ? "hit" : "stand")}.");
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    // Human player
                    Console.Write("\nDo you want to (H)it or (S)tand? ");
                    string choice = Console.ReadLine()?.Trim().ToUpper() ?? "";
                    
                    wantsToHit = (choice == "H" || choice == "HIT");
                    
                    if (wantsToHit)
                    {
                        Console.WriteLine("You chose to hit.");
                    }
                    else if (choice == "S" || choice == "STAND")
                    {
                        Console.WriteLine("You chose to stand.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please enter 'H' for Hit or 'S' for Stand.");
                        continue;
                    }
                    
                    Thread.Sleep(500);
                }
                
                if (wantsToHit)
                {
                    Card newCard = deck.DrawCard();
                    
                    if (interactiveMode)
                        Console.WriteLine($"Player drew: {newCard}");
                        
                    player.AddCard(newCard);
                    
                    if (interactiveMode)
                    {
                        Console.WriteLine();
                        player.DisplayHand();
                    }
                    
                    if (player.IsBusted)
                    {
                        if (interactiveMode)
                            Console.WriteLine("Bust! Player went over 21.");
                            
                        playerTurnDone = true;
                        return GameResult.Bust;
                    }
                    else if (player.CalculateHandValue() == 21)
                    {
                        if (interactiveMode)
                            Console.WriteLine("21! Perfect score.");
                            
                        playerTurnDone = true;
                    }
                }
                else
                {
                    player.HasStood = true;
                    playerTurnDone = true;
                }
            }
            
            return GameResult.Win; // Placeholder, actual result determined later
        }

        private void DealerTurn()
        {
            if (interactiveMode)
            {
                Console.WriteLine("\nDealer's turn:");
                Thread.Sleep(1000);
                
                // Reveal dealer's hidden card
                dealer.DisplayHand();
                Thread.Sleep(1000);
            }

            // Dealer must hit until they have 17 or higher
            while (dealer.CalculateHandValue() < 17)
            {
                if (interactiveMode)
                    Console.WriteLine("Dealer hits...");
                
                Card newCard = deck.DrawCard();
                dealer.AddCard(newCard);
                
                if (interactiveMode)
                {
                    Console.WriteLine($"Dealer drew: {newCard}");
                    Console.WriteLine();
                    dealer.DisplayHand();
                    Thread.Sleep(1000);
                }
            }

            if (interactiveMode)
            {
                if (dealer.IsBusted)
                {
                    Console.WriteLine("Dealer busts!");
                }
                else
                {
                    Console.WriteLine("Dealer stands.");
                }
            }
        }

        private GameResult DetermineWinner()
        {
            int playerValue = player.CalculateHandValue();
            int dealerValue = dealer.CalculateHandValue();
            
            if (interactiveMode)
            {
                Console.WriteLine("\nFinal Results:");
                Console.WriteLine($"Your hand value: {playerValue}");
                Console.WriteLine($"Dealer's hand value: {dealerValue}");
            }

            if (dealer.IsBusted)
            {
                if (interactiveMode)
                    Console.WriteLine("Dealer busted! You win!");
                    
                return GameResult.Win;
            }
            else if (playerValue > dealerValue)
            {
                if (interactiveMode)
                    Console.WriteLine("You win!");
                    
                return GameResult.Win;
            }
            else if (playerValue < dealerValue)
            {
                if (interactiveMode)
                    Console.WriteLine("Dealer wins!");
                    
                return GameResult.Loss;
            }
            else
            {
                if (interactiveMode)
                    Console.WriteLine("It's a tie!");
                    
                return GameResult.Push;
            }
        }
        
        private GameOutcome CreateGameOutcome(GameResult result, bool blackjack = false)
        {
            return new GameOutcome
            {
                PlayerName = player.Name,
                StrategyName = player.Strategy?.Name ?? "Human",
                Result = result,
                HandValue = player.CalculateHandValue(),
                DealerValue = dealer.CalculateHandValue(),
                PlayerBusted = player.IsBusted,
                DealerBusted = dealer.IsBusted,
                HasBlackjack = blackjack
            };
        }
    }
}