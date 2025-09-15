using System;
using System.Collections.Generic;
using System.Threading;

namespace GameStudioB
{
    public class AceResetGame
    {
        private Deck deck;
        private Player dealer;
        private Player player;
        private bool interactiveMode;

        public AceResetGame(bool interactive = true)
        {
            deck = new Deck();
            dealer = new Player("Dealer", true);
            player = new Player("Player");
            interactiveMode = interactive;
        }

        public GameOutcome PlayGame(IAceResetStrategy? playerStrategy = null)
        {
            // Reset game state
            player.ClearHand();
            dealer.ClearHand();
            
            // Assign strategy if provided
            player.Strategy = playerStrategy;
            
            if (deck.RemainingCards() < 15)
            {
                deck.Initialize(); // Ensure we have enough cards
            }

            // Initialize player scores
            int playerScore = 0;
            int dealerScore = 0;
            bool gameOver = false;
            
            if (interactiveMode)
            {
                Console.WriteLine("\nStarting a new game of AceReset...");
                Console.WriteLine("Try to accumulate the highest score before the deck runs out.");
                Console.WriteLine("If you draw an Ace, your score resets to 0!");
                Console.WriteLine("The dealer's score doesn't reset on an Ace, but gets 0 points for Ace, Jack, Queen, or King.");
                Console.WriteLine("You may skip your turn at any time.");
                Console.WriteLine($"Current deck size: {deck.RemainingCards()} cards\n");
                Thread.Sleep(1000);
            }

            // Main game loop
            while (!gameOver)
            {
                // Display current scores
                if (interactiveMode)
                {
                    Console.WriteLine($"\nCurrent scores: Player: {playerScore} | Dealer: {dealerScore}");
                    Console.WriteLine($"Cards remaining in deck: {deck.RemainingCards()}");
                }

                // Player's turn
                if (!ProcessPlayerTurn(ref playerScore, playerStrategy))
                {
                    // Player skipped
                    if (interactiveMode)
                    {
                        Console.WriteLine("You chose to skip your turn.");
                    }
                }

                // Dealer's turn (always draws)
                ProcessDealerTurn(ref dealerScore);
                
                // Check if the game is over (deck is empty)
                if (deck.RemainingCards() == 0)
                {
                    gameOver = true;
                    if (interactiveMode)
                    {
                        Console.WriteLine("\nDeck is empty! Game over.");
                    }
                }
                
                if (interactiveMode && !gameOver)
                {
                    Console.WriteLine($"\nScores after this round: Player: {playerScore} | Dealer: {dealerScore}");
                    Console.WriteLine("-------------------------------------");
                    Thread.Sleep(1000);
                }
            }
            
            // Determine winner
            GameResult result = DetermineWinner(playerScore, dealerScore);
            
            if (interactiveMode)
            {
                Console.WriteLine($"\nFinal Scores:");
                Console.WriteLine($"Player: {playerScore}");
                Console.WriteLine($"Dealer: {dealerScore}");
                
                switch (result)
                {
                    case GameResult.Win:
                        Console.WriteLine("You win!");
                        break;
                    case GameResult.Loss:
                        Console.WriteLine("Dealer wins!");
                        break;
                    case GameResult.Push:
                        Console.WriteLine("It's a tie! Dealer wins on ties.");
                        break;
                }
            }
            
            return new GameOutcome
            {
                PlayerName = player.Name,
                StrategyName = playerStrategy?.Name ?? "Human",
                Result = result,
                HandValue = playerScore,
                DealerValue = dealerScore
            };
        }

        private bool ProcessPlayerTurn(ref int playerScore, IAceResetStrategy? playerStrategy)
        {
            bool shouldDraw;
            
            if (playerStrategy != null)
            {
                // AI player
                shouldDraw = playerStrategy.DecideToDrawCard(playerScore, dealer.CalculateHandValue(), deck.RemainingCards());
                
                if (interactiveMode)
                {
                    Console.WriteLine($"\nPlayer ({playerStrategy.Name}) decides to {(shouldDraw ? "draw a card" : "skip")}.");
                    Thread.Sleep(500);
                }
            }
            else
            {
                // Human player
                Console.Write("\nDo you want to (D)raw a card or (S)kip? ");
                string choice = Console.ReadLine()?.Trim().ToUpper() ?? "";
                shouldDraw = (choice == "D" || choice == "DRAW");
                
                if (!shouldDraw && choice != "S" && choice != "SKIP")
                {
                    Console.WriteLine("Invalid choice. Skipping turn by default.");
                }
            }
            
            if (shouldDraw && deck.RemainingCards() > 0)
            {
                Card drawnCard = deck.DrawCard();
                player.AddCard(drawnCard);
                
                if (interactiveMode)
                {
                    Console.WriteLine($"You drew: {drawnCard}");
                }
                
                // Check if the card is an Ace
                if (drawnCard.Rank == Card.RankValue.Ace)
                {
                    if (interactiveMode)
                    {
                        Console.WriteLine("Oh no! You drew an Ace - your score resets to 0!");
                    }
                    playerScore = 0;
                }
                else
                {
                    // Add card value to score
                    int cardValue = drawnCard.GetBlackjackValue();
                    playerScore += cardValue;
                    
                    if (interactiveMode)
                    {
                        Console.WriteLine($"Added {cardValue} to your score. New score: {playerScore}");
                    }
                }
                
                return true;
            }
            
            return false; // Player skipped or deck was empty
        }

        private void ProcessDealerTurn(ref int dealerScore)
        {
            // Dealer always draws a card if possible
            if (deck.RemainingCards() > 0)
            {
                Card drawnCard = deck.DrawCard();
                dealer.AddCard(drawnCard);
                
                if (interactiveMode)
                {
                    Console.WriteLine($"\nDealer drew: {drawnCard}");
                }
                
                // Check if the card is an Ace, Jack, Queen, or King
                if (drawnCard.Rank == Card.RankValue.Ace || 
                    drawnCard.Rank == Card.RankValue.Jack || 
                    drawnCard.Rank == Card.RankValue.Queen || 
                    drawnCard.Rank == Card.RankValue.King)
                {
                    if (interactiveMode)
                    {
                        string cardName = drawnCard.Rank.ToString();
                        Console.WriteLine($"Dealer drew a {cardName} - they get 0 points for face cards and Aces.");
                    }
                    // Dealer gets 0 points for these cards but score doesn't reset
                }
                else
                {
                    // Add card value to dealer's score for non-face cards
                    int cardValue = drawnCard.GetBlackjackValue();
                    dealerScore += cardValue;
                    
                    if (interactiveMode)
                    {
                        Console.WriteLine($"Added {cardValue} to dealer's score. New dealer score: {dealerScore}");
                    }
                }
            }
            else if (interactiveMode)
            {
                Console.WriteLine("Deck is empty - dealer cannot draw a card.");
            }
        }

        private GameResult DetermineWinner(int playerScore, int dealerScore)
        {
            if (playerScore > dealerScore)
            {
                return GameResult.Win;
            }
            else if (playerScore < dealerScore)
            {
                return GameResult.Loss;
            }
            else
            {
                // On equal points, dealer wins
                return GameResult.Loss;
            }
        }
    }
}