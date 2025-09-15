using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardGames.Core;
using CardGames.Simulation;

namespace CardGames.Blackjack
{
    public class Blackjack : CardGame, IGameResults
    {
        private const int BlackjackValue = 21;
        private const int DealerStandThreshold = 17;

        public override string Name => "Blackjack";

        public override string Rules => 
            "Blackjack Rules:\n" +
            "1. Each player is dealt 2 cards. Dealer gets 2 cards with one face-down.\n" +
            "2. Cards 2-10 are worth their face value. Face cards (J,Q,K) are worth 10. Aces are worth 1 or 11.\n" +
            "3. Players decide to hit (take another card) or stand (keep current hand).\n" +
            "4. Players aim to get as close to 21 without going over (busting).\n" +
            "5. After all players finish, dealer reveals their hidden card and hits until reaching 17 or higher.\n" +
            "6. Players win if they don't bust and have higher total than dealer, or dealer busts.\n" +
            "7. Blackjack (Ace + 10-value card) pays 3:2 if dealer doesn't have blackjack.";

        private GameContext _gameContext;

        public override void InitializeGame()
        {
            Deck = new Deck(1);
            Deck.Shuffle();
            
            // Reset hands
            foreach (var participant in Participants)
            {
                participant.ClearHand();
            }
            Dealer.ClearHand();
            
            // Set up game context
            _gameContext = new GameContext
            {
                Participants = Participants,
                Dealer = Dealer,
                Deck = Deck,
                RulesDescription = Rules,
                Game = this
            };
        }

        public override void PlayRound()
        {
            // Deal initial cards
            foreach (var participant in Participants)
            {
                participant.ReceiveCard(Deck.DealCard());
                participant.ReceiveCard(Deck.DealCard());
            }
            
            Dealer.ReceiveCard(Deck.DealCard());
            Dealer.ReceiveCard(Deck.DealCard());

            // Player turns
            foreach (var participant in Participants)
            {
                ProcessParticipantTurn(participant);
            }

            // Dealer's turn
            ProcessDealerTurn();
        }

        private void ProcessParticipantTurn(Participant participant)
        {
            bool continueTurn = true;

            while (continueTurn && !HasBusted(participant))
            {
                // Use participant's strategy to decide whether to hit
                bool wantsToHit = participant.Strategy.DecideToHit(participant, _gameContext);
                
                if (wantsToHit)
                {
                    participant.ReceiveCard(Deck.DealCard());
                }
                else
                {
                    continueTurn = false;
                }
                
                // Stop if blackjack or bust
                if (GetHandValue(participant) == BlackjackValue || HasBusted(participant))
                {
                    continueTurn = false;
                }
            }
        }

        private void ProcessDealerTurn()
        {
            // Dealer must hit until reaching at least 17
            while (GetHandValue(Dealer) < DealerStandThreshold)
            {
                Dealer.ReceiveCard(Deck.DealCard());
            }
        }

        public override void DetermineWinners()
        {
            var roundResult = GetRoundResult(0);
            
            foreach (var outcome in roundResult.Outcomes)
            {
                string resultText = outcome.Result switch
                {
                    GameResult.Win => outcome.HasBlackjack ? "wins with Blackjack!" : 
                                     outcome.DealerBusted ? $"wins with {outcome.HandValue}. Dealer busted with {outcome.DealerValue}." :
                                     $"wins with {outcome.HandValue} against dealer's {outcome.DealerValue}.",
                    GameResult.Loss => outcome.ParticipantBusted ? $"busted with {outcome.HandValue}." :
                                      outcome.DealerValue == BlackjackValue && roundResult.DealerHasBlackjack ? "loses to dealer's Blackjack." :
                                      $"loses with {outcome.HandValue} against dealer's {outcome.DealerValue}.",
                    GameResult.Push => $"pushes with {outcome.HandValue} (tie with dealer).",
                    GameResult.Bust => $"busted with {outcome.HandValue}.",
                    _ => "unknown result."
                };

                Console.WriteLine($"{outcome.ParticipantName} {resultText}");
            }
        }

        public RoundResult GetRoundResult(int roundNumber)
        {
            int dealerValue = GetHandValue(Dealer);
            bool dealerBusted = HasBusted(Dealer);
            bool dealerHasBlackjack = HasBlackjack(Dealer);

            var roundResult = new RoundResult
            {
                RoundNumber = roundNumber,
                DealerValue = dealerValue,
                DealerBusted = dealerBusted,
                DealerHasBlackjack = dealerHasBlackjack
            };

            foreach (var participant in Participants)
            {
                int participantValue = GetHandValue(participant);
                bool participantBusted = HasBusted(participant);
                bool participantHasBlackjack = HasBlackjack(participant);

                GameResult result;
                if (participantBusted)
                {
                    result = GameResult.Bust;
                }
                else if (dealerBusted)
                {
                    result = GameResult.Win;
                }
                else if (participantHasBlackjack && !dealerHasBlackjack)
                {
                    result = GameResult.Win;
                }
                else if (!participantHasBlackjack && dealerHasBlackjack)
                {
                    result = GameResult.Loss;
                }
                else if (participantValue > dealerValue)
                {
                    result = GameResult.Win;
                }
                else if (participantValue < dealerValue)
                {
                    result = GameResult.Loss;
                }
                else
                {
                    result = GameResult.Push;
                }

                var outcome = new GameOutcome
                {
                    ParticipantName = participant.Name,
                    StrategyName = participant.Strategy.Name,
                    HandValue = participantValue,
                    DealerValue = dealerValue,
                    Result = result,
                    HasBlackjack = participantHasBlackjack,
                    DealerBusted = dealerBusted,
                    ParticipantBusted = participantBusted
                };

                roundResult.Outcomes.Add(outcome);
            }

            return roundResult;
        }

        // Helper methods for game logic
        public int GetHandValue(Participant participant)
        {
            int value = 0;
            int aceCount = 0;

            foreach (var card in participant.Hand)
            {
                if (card.Rank == "Ace")
                {
                    aceCount++;
                    value += 11; // Initially count Ace as 11
                }
                else if (card.Rank == "King" || card.Rank == "Queen" || card.Rank == "Jack")
                {
                    value += 10;
                }
                else
                {
                    // For numbered cards, parse the rank
                    if (int.TryParse(card.Rank, out int rankValue))
                    {
                        value += rankValue;
                    }
                }
            }

            // Adjust for Aces if needed
            while (value > BlackjackValue && aceCount > 0)
            {
                value -= 10; // Convert Ace from 11 to 1
                aceCount--;
            }

            return value;
        }

        public bool HasBusted(Participant participant)
        {
            return GetHandValue(participant) > BlackjackValue;
        }

        public bool HasBlackjack(Participant participant)
        {
            return participant.Hand.Count == 2 && GetHandValue(participant) == BlackjackValue;
        }
    }
}