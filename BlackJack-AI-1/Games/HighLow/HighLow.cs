using System;
using System.Collections.Generic;
using CardGames.Core;
using CardGames.Simulation;

namespace CardGames.GamesStudio.Games.HighLow
{
    /// <summary>
    /// Implementation of High-Low card game
    /// Players guess whether the next card will be higher or lower than the current one
    /// </summary>
    public class HighLow : CardGame, IGameResults
    {
        // Used to store the current face-up card
        private Card _currentCard;
        
        // Store the game context
        private GameContext _gameContext;
        
        // Track player predictions and results
        private Dictionary<Participant, GuessResult> _participantResults = new Dictionary<Participant, GuessResult>();

        /// <summary>
        /// The name of the game
        /// </summary>
        public override string Name => "High-Low";

        /// <summary>
        /// Rules of the High-Low game
        /// </summary>
        public override string Rules => 
            "High-Low Rules:\n" +
            "1. The dealer draws a card and places it face up.\n" +
            "2. Each player predicts whether the next card will be higher or lower.\n" +
            "3. The dealer then draws the next card.\n" +
            "4. Players who predicted correctly win the round.\n" +
            "5. Aces are considered lowest (value 1).\n" +
            "6. If the cards have the same value, it's considered a push (tie).";

        /// <summary>
        /// Initializes the game for a new round
        /// </summary>
        public override void InitializeGame()
        {
            // Create and shuffle a new deck
            Deck = new Deck(1);
            Deck.Shuffle();
            
            // Reset participants' hands
            foreach (var participant in Participants)
            {
                participant.ClearHand();
            }
            Dealer.ClearHand();
            
            // Clear previous results
            _participantResults.Clear();
            
            // Set up game context
            _gameContext = new GameContext
            {
                Participants = Participants,
                Dealer = Dealer,
                Deck = Deck,
                RulesDescription = Rules,
                Game = this
            };
            
            // Deal the first card face up
            _currentCard = Deck.DealCard();
            Dealer.ReceiveCard(_currentCard); // The dealer holds the current face-up card
            
            Console.WriteLine($"Initial card: {_currentCard}");
        }

        /// <summary>
        /// Plays a round of High-Low
        /// </summary>
        public override void PlayRound()
        {
            // Each participant makes their prediction (high or low)
            foreach (var participant in Participants)
            {
                // Use the participant's strategy to decide
                bool predictHigher = ((IHighLowStrategy)participant.Strategy).PredictHigher(participant, _gameContext, _currentCard);
                
                // Store the prediction
                var result = new GuessResult
                {
                    Participant = participant,
                    InitialCard = _currentCard,
                    PredictedHigher = predictHigher,
                };
                
                _participantResults[participant] = result;
                
                Console.WriteLine($"{participant.Name} ({participant.Strategy.Name}) predicts the next card will be {(predictHigher ? "higher" : "lower")}");
            }
            
            // Dealer reveals the next card
            Card nextCard = Deck.DealCard();
            Console.WriteLine($"Next card: {nextCard}");
            
            // Determine the result
            foreach (var participant in Participants)
            {
                var result = _participantResults[participant];
                result.NextCard = nextCard;
                
                // Compare card values
                int currentValue = GetCardValue(_currentCard);
                int nextValue = GetCardValue(nextCard);
                
                if (currentValue == nextValue)
                {
                    result.Outcome = GuessOutcome.Push;
                }
                else
                {
                    bool isHigher = nextValue > currentValue;
                    result.Outcome = isHigher == result.PredictedHigher ? GuessOutcome.Correct : GuessOutcome.Incorrect;
                }
                
                // For simulation purposes, add the next card to the participant's hand
                participant.ReceiveCard(nextCard);
            }
            
            // Update current card for next round
            _currentCard = nextCard;
        }

        /// <summary>
        /// Determines and displays winners
        /// </summary>
        public override void DetermineWinners()
        {
            foreach (var participant in Participants)
            {
                var result = _participantResults[participant];
                
                string outcomeText = result.Outcome switch
                {
                    GuessOutcome.Correct => "wins! Correct prediction.",
                    GuessOutcome.Incorrect => "loses. Incorrect prediction.",
                    GuessOutcome.Push => "pushes (tie). Cards have the same value.",
                    _ => "unknown result."
                };
                
                Console.WriteLine($"{participant.Name} {outcomeText}");
            }
        }

        /// <summary>
        /// Gets the results of a round for simulation
        /// </summary>
        public RoundResult GetRoundResult(int roundNumber)
        {
            var roundResult = new RoundResult
            {
                RoundNumber = roundNumber,
                Outcomes = new List<GameOutcome>()
            };

            foreach (var participant in Participants)
            {
                var result = _participantResults[participant];
                
                GameResult gameResult;
                switch (result.Outcome)
                {
                    case GuessOutcome.Correct:
                        gameResult = GameResult.Win;
                        break;
                    case GuessOutcome.Incorrect:
                        gameResult = GameResult.Loss;
                        break;
                    case GuessOutcome.Push:
                        gameResult = GameResult.Push;
                        break;
                    default:
                        gameResult = GameResult.Loss;
                        break;
                }
                
                var outcome = new GameOutcome
                {
                    ParticipantName = participant.Name,
                    StrategyName = participant.Strategy.Name,
                    Result = gameResult,
                    // For High-Low, we can use HandValue to store our card values
                    HandValue = GetCardValue(result.InitialCard),
                    DealerValue = GetCardValue(result.NextCard),
                };
                
                roundResult.Outcomes.Add(outcome);
            }
            
            return roundResult;
        }

        /// <summary>
        /// Converts a card to its numerical value (Ace=1, Jack=11, Queen=12, King=13)
        /// </summary>
        public int GetCardValue(Card card)
        {
            if (int.TryParse(card.Rank, out int value))
                return value;
                
            return card.Rank switch
            {
                "Ace" => 1,
                "Jack" => 11,
                "Queen" => 12,
                "King" => 13,
                _ => 0
            };
        }
    }

    /// <summary>
    /// Represents the outcome of a guess
    /// </summary>
    public enum GuessOutcome
    {
        Correct,
        Incorrect,
        Push
    }

    /// <summary>
    /// Tracks a participant's guess and result
    /// </summary>
    public class GuessResult
    {
        public Participant Participant { get; set; }
        public Card InitialCard { get; set; }
        public Card NextCard { get; set; }
        public bool PredictedHigher { get; set; }
        public GuessOutcome Outcome { get; set; }
    }
}