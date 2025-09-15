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

        public BlackJackGame()
        {
            deck = new Deck();
            dealer = new Player("Dealer", true);
            player = new Player("Player");
        }

        public void PlayGame()
        {
            // Reset game state
            player.ClearHand();
            dealer.ClearHand();
            
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
            Console.WriteLine("\nDealing cards...");
            Thread.Sleep(1000);
            
            dealer.DisplayHand(true); // Hide dealer's first card
            Console.WriteLine();
            player.DisplayHand();

            // Check for immediate BlackJack
            if (player.HasBlackjack)
            {
                Console.WriteLine("\nBlackjack! You win!");
                dealer.DisplayHand(); // Show dealer's full hand
                return;
            }

            // Player's turn
            PlayerTurn();
            
            // If player hasn't busted, dealer plays
            if (!player.IsBusted)
            {
                DealerTurn();
                DetermineWinner();
            }
            else
            {
                Console.WriteLine("\nYou busted! Dealer wins.");
            }
        }

        private void PlayerTurn()
        {
            bool playerTurnDone = false;

            while (!playerTurnDone)
            {
                Console.Write("\nDo you want to (H)it or (S)tand? ");
                string choice = Console.ReadLine()?.Trim().ToUpper() ?? "";
                
                if (choice == "H" || choice == "HIT")
                {
                    Console.WriteLine("You chose to hit.");
                    Thread.Sleep(500);
                    
                    Card newCard = deck.DrawCard();
                    Console.WriteLine($"You drew: {newCard}");
                    player.AddCard(newCard);
                    
                    Console.WriteLine();
                    player.DisplayHand();
                    
                    if (player.IsBusted)
                    {
                        Console.WriteLine("Bust! You went over 21.");
                        playerTurnDone = true;
                    }
                    else if (player.CalculateHandValue() == 21)
                    {
                        Console.WriteLine("21! Perfect score.");
                        playerTurnDone = true;
                    }
                }
                else if (choice == "S" || choice == "STAND")
                {
                    Console.WriteLine("You chose to stand.");
                    player.HasStood = true;
                    playerTurnDone = true;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please enter 'H' for Hit or 'S' for Stand.");
                }
            }
        }

        private void DealerTurn()
        {
            Console.WriteLine("\nDealer's turn:");
            Thread.Sleep(1000);
            
            // Reveal dealer's hidden card
            dealer.DisplayHand();
            Thread.Sleep(1000);

            // Dealer must hit until they have 17 or higher
            while (dealer.CalculateHandValue() < 17)
            {
                Console.WriteLine("Dealer hits...");
                Thread.Sleep(1000);
                
                Card newCard = deck.DrawCard();
                dealer.AddCard(newCard);
                Console.WriteLine($"Dealer drew: {newCard}");
                
                Console.WriteLine();
                dealer.DisplayHand();
                Thread.Sleep(1000);
            }

            if (dealer.IsBusted)
            {
                Console.WriteLine("Dealer busts!");
            }
            else
            {
                Console.WriteLine("Dealer stands.");
            }
        }

        private void DetermineWinner()
        {
            int playerValue = player.CalculateHandValue();
            int dealerValue = dealer.CalculateHandValue();
            
            Console.WriteLine("\nFinal Results:");
            Console.WriteLine($"Your hand value: {playerValue}");
            Console.WriteLine($"Dealer's hand value: {dealerValue}");

            if (dealer.IsBusted)
            {
                Console.WriteLine("Dealer busted! You win!");
            }
            else if (playerValue > dealerValue)
            {
                Console.WriteLine("You win!");
            }
            else if (playerValue < dealerValue)
            {
                Console.WriteLine("Dealer wins!");
            }
            else
            {
                Console.WriteLine("It's a tie!");
            }
        }
    }
}