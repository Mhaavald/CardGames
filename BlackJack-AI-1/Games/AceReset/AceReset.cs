using System;
using System.Collections.Generic;
using CardGames.Core;
using CardGames.Simulation;

namespace CardGames.GamesStudio.Games.AceReset
{
    /// <summary>
    /// Implementation of the Ace Reset card game
    /// </summary>
    public class AceReset : CardGame, IGameResults
    {
        // Track participant scores
        private Dictionary<Participant, int> _scores = new Dictionary<Participant, int>();
        
        // Store the game context
        private GameContext? _gameContext;
        
        // Track participant actions and results
        private Dictionary<Participant, AceResetAction> _participantActions = new Dictionary<Participant, AceResetAction>();
        
        /// <summary>
        /// The name of the game
        /// </summary>
        public override string Name => "Ace Reset";

        /// <summary>
        /// Rules of the Ace Reset game
        /// </summary>
        public override string Rules => 
            "Ace Reset Rules:\n" +
            "1. Each player, including the dealer, takes turns drawing cards.\n" +
            "2. Players accumulate points equal to the sum of their card values.\n" +
            "3. If a player draws an Ace, their score resets to 0.\n" +
            "4. If the dealer draws an Ace, their score does not reset, but they get 0 points.\n" +
            "5. The dealer also gets 0 points for face cards (Jack, Queen, King).\n" +
            "6. Players may choose to skip their turn.\n" +
            "7. When the deck is empty, the game ends.\n" +
            "8. The player with the highest score wins.\n" +
            "9. If scores are tied, the dealer wins.";

        /// <summary>
        /// Initializes the game for a new round
        /// </summary>
        public override void InitializeGame()
        {
            // Create and shuffle a new deck
            Deck = new Deck(1);
            Deck.Shuffle();
            
            // Reset scores and hands
            _scores.Clear();
            foreach (var participant in Participants)
            {
                participant.ClearHand();
                _scores[participant] = 0;
            }
            
            Dealer.ClearHand();
            _scores[Dealer] = 0;
            
            // Reset participant actions
            _participantActions.Clear();
            
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

        /// <summary>
        /// Plays a round of Ace Reset
        /// </summary>
        public override void PlayRound()
        {
            // Each participant takes their turn
            foreach (var participant in Participants)
            {
                ProcessParticipantTurn(participant);
            }
            
            // Dealer's turn (always draws)
            ProcessDealerTurn();
        }

        /// <summary>
        /// Processes a participant's turn
        /// </summary>
        private void ProcessParticipantTurn(Participant participant)
        {
            // Use the participant's strategy to decide whether to draw
            bool wantsToDraw = ((IAceResetStrategy)participant.Strategy).DecideToDrawCard(
                _scores[participant], 
                _scores[Dealer], 
                Deck.Count
            );
            
            // Store the participant's action
            var action = new AceResetAction
            {
                Participant = participant,
                ChoseToSkip = !wantsToDraw,
                DrawnCard = null
            };
            
            _participantActions[participant] = action;
            
            // If the participant wants to draw and there are cards left, process the draw
            if (wantsToDraw && Deck.Count > 0)
            {
                Card drawnCard = Deck.DealCard();
                participant.ReceiveCard(drawnCard);
                action.DrawnCard = drawnCard;
                
                // Check if the card is an Ace
                if (drawnCard.Rank == "Ace")
                {
                    // Score resets to 0 for players who draw Aces
                    _scores[participant] = 0;
                    action.ScoreReset = true;
                }
                else
                {
                    // Add card value to score
                    int cardValue = GetCardValue(drawnCard);
                    _scores[participant] += cardValue;
                    action.PointsGained = cardValue;
                }
            }
        }

        /// <summary>
        /// Processes the dealer's turn
        /// </summary>
        private void ProcessDealerTurn()
        {
            // Dealer always draws if possible
            if (Deck.Count > 0)
            {
                Card drawnCard = Deck.DealCard();
                Dealer.ReceiveCard(drawnCard);
                
                // Check if the card is a face card or Ace
                if (drawnCard.Rank == "Ace" || 
                    drawnCard.Rank == "Jack" || 
                    drawnCard.Rank == "Queen" || 
                    drawnCard.Rank == "King")
                {
                    // Dealer gets 0 points for face cards and aces but score doesn't reset
                }
                else
                {
                    // Add card value to dealer's score for non-face cards
                    int cardValue = GetCardValue(drawnCard);
                    _scores[Dealer] += cardValue;
                }
            }
        }

        /// <summary>
        /// Determines and displays the winners
        /// </summary>
        public override void DetermineWinners()
        {
            int highestScore = _scores[Dealer];
            bool dealerWins = true;
            
            // Check if any participant has a higher score than the dealer
            foreach (var participant in Participants)
            {
                int participantScore = _scores[participant];
                
                if (participantScore > highestScore)
                {
                    highestScore = participantScore;
                    dealerWins = false;
                }
            }
            
            // Determine winners (on equal high scores, dealer wins)
            foreach (var participant in Participants)
            {
                int participantScore = _scores[participant];
                bool isWinner = participantScore == highestScore && !dealerWins;
                
                Console.WriteLine($"{participant.Name} ({participant.Strategy.Name}): {participantScore} points - {(isWinner ? "Winner" : "Not Winner")}");
            }
            
            Console.WriteLine($"Dealer: {_scores[Dealer]} points - {(dealerWins ? "Winner" : "Not Winner")}");
        }

        /// <summary>
        /// Gets the results of a round for simulation
        /// </summary>
        public RoundResult GetRoundResult(int roundNumber)
        {
            var roundResult = new RoundResult
            {
                RoundNumber = roundNumber,
                Outcomes = new List<GameOutcome>(),
                DealerValue = _scores[Dealer]
            };

            int highestScore = _scores[Dealer];
            bool dealerWins = true;
            
            // Find highest score
            foreach (var participant in Participants)
            {
                if (_scores[participant] > highestScore)
                {
                    highestScore = _scores[participant];
                    dealerWins = false;
                }
            }
            
            // Add outcomes for each participant
            foreach (var participant in Participants)
            {
                int participantScore = _scores[participant];
                var action = _participantActions.ContainsKey(participant) ? _participantActions[participant] : null;
                
                GameResult result;
                if (participantScore > _scores[Dealer])
                {
                    result = GameResult.Win;
                }
                else if (participantScore < _scores[Dealer])
                {
                    result = GameResult.Loss;
                }
                else
                {
                    // On equal scores, dealer wins
                    result = GameResult.Loss;
                }
                
                var outcome = new GameOutcome
                {
                    ParticipantName = participant.Name,
                    StrategyName = participant.Strategy.Name,
                    Result = result,
                    HandValue = participantScore,
                    DealerValue = _scores[Dealer],
                    ParticipantBusted = action?.ScoreReset ?? false,
                };
                
                roundResult.Outcomes.Add(outcome);
            }
            
            return roundResult;
        }

        /// <summary>
        /// Converts a card to its numerical value
        /// </summary>
        private int GetCardValue(Card card)
        {
            if (int.TryParse(card.Rank, out int value))
                return value;
                
            return card.Rank switch
            {
                "Jack" => 10,
                "Queen" => 10,
                "King" => 10,
                "Ace" => 1,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Tracks a participant's action and result
    /// </summary>
    public class AceResetAction
    {
        public Participant Participant { get; set; } = null!;
        public bool ChoseToSkip { get; set; }
        public Card? DrawnCard { get; set; }
        public bool ScoreReset { get; set; }
        public int PointsGained { get; set; }
    }
}