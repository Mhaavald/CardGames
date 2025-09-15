using System;
using System.Collections.Generic;
using System.Linq;
using CardGames.Core;
using CardGames.Simulation;

namespace CardGames.Rummy
{
    /// <summary>
    /// Implementation of a Simple Rummy card game
    /// </summary>
    public class Rummy : CardGame, IGameResults
    {
        // The discard pile
        private List<Card> _discardPile = new List<Card>();
        
        // Game context
        private GameContext _gameContext;
        
        // Tracks participant hands and scores
        private Dictionary<Participant, RummyPlayerState> _playerStates = new Dictionary<Participant, RummyPlayerState>();
        
        // Track if someone has gone out (declared)
        private Participant _declaredParticipant = null;
        
        // Track the winner of the round (may be different from declared participant if no one goes out)
        private Participant _winner = null;
        
        // Track the results of the round
        private Dictionary<Participant, RummyRoundResult> _roundResults = new Dictionary<Participant, RummyRoundResult>();
        
        /// <summary>
        /// Number of cards to deal to each player
        /// </summary>
        private const int InitialHandSize = 7;
        
        /// <summary>
        /// Maximum number of turns to play before ending the round if no one goes out
        /// </summary>
        private const int MaxTurns = 10; // Reduced from 30 to 10 for faster simulations
        
        /// <summary>
        /// Current turn counter
        /// </summary>
        private int _currentTurn = 0;

        /// <summary>
        /// The name of the game
        /// </summary>
        public override string Name => "Simple Rummy";

        /// <summary>
        /// Rules of the Rummy game
        /// </summary>
        public override string Rules => 
            "Simple Rummy Rules:\n" +
            "1. Each player is dealt 7 cards.\n" +
            "2. On each turn, players draw a card (from deck or discard pile) and then discard one card.\n" +
            "3. The goal is to form sets (3-4 cards of the same rank) or runs (3+ consecutive cards of the same suit).\n" +
            "4. Players aim to arrange all their cards in valid combinations.\n" +
            "5. A player goes out by forming all cards into valid combinations.\n" +
            "6. When a player goes out, other players lose points for their unmatched cards.\n" +
            "7. If no player goes out after 10 turns, the player with lowest unmatched points wins.\n" +
            "8. In case of a tie in points, the player with more sets/runs wins.\n" + 
            "9. Face cards (J,Q,K) are worth 10 points, Ace is 1, and number cards are worth their face value.";

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
            
            _discardPile.Clear();
            _playerStates.Clear();
            _roundResults.Clear();
            _declaredParticipant = null;
            _winner = null;
            _currentTurn = 0;
            
            // Deal initial cards
            foreach (var participant in Participants)
            {
                for (int i = 0; i < InitialHandSize; i++)
                {
                    participant.ReceiveCard(Deck.DealCard());
                }
                
                // Initialize player state
                _playerStates[participant] = new RummyPlayerState
                {
                    Participant = participant,
                    Combinations = AnalyzeHand(participant.Hand)
                };
            }
            
            // Turn up first card for discard pile
            Card initialDiscard = Deck.DealCard();
            _discardPile.Add(initialDiscard);
            
            // Set up game context
            _gameContext = new GameContext
            {
                Participants = Participants,
                Dealer = Dealer,
                Deck = Deck,
                RulesDescription = Rules,
                Game = this
            };
            
            Console.WriteLine($"Game initialized. Top card of discard pile: {initialDiscard}");
            
            // Show initial hands
            foreach (var participant in Participants)
            {
                Console.WriteLine($"{participant.Name}'s hand: {string.Join(", ", participant.Hand)}");
            }
        }

        /// <summary>
        /// Plays a round of Rummy
        /// </summary>
        public override void PlayRound()
        {
            // Play until someone declares or max turns are reached
            while (_declaredParticipant == null && _currentTurn < MaxTurns)
            {
                _currentTurn++;
                Console.WriteLine($"\n=== Turn {_currentTurn} of {MaxTurns} ===");
                
                // Each participant takes a turn
                foreach (var participant in Participants)
                {
                    if (_declaredParticipant != null)
                    {
                        // Skip turns if someone has already gone out
                        continue;
                    }
                    
                    Console.WriteLine($"\n{participant.Name}'s turn:");
                    Console.WriteLine($"Current hand: {string.Join(", ", participant.Hand)}");
                    Console.WriteLine($"Top card of discard pile: {_discardPile.Last()}");
                    
                    // Draw phase - decide to draw from deck or discard pile
                    Card drawnCard;
                    bool drawFromDeck = ((IRummyStrategy)participant.Strategy).DrawFromDeck(participant, _gameContext, _discardPile.Last());
                    
                    if (drawFromDeck)
                    {
                        if (Deck.Count == 0)
                        {
                            // Reshuffle discard pile if the deck is empty
                            if (_discardPile.Count > 1)
                            {
                                // Keep the top card aside
                                Card topCard = _discardPile.Last();
                                _discardPile.RemoveAt(_discardPile.Count - 1);
                                
                                // Shuffle the rest into a new deck
                                List<Card> newDeckCards = new List<Card>(_discardPile);
                                _discardPile.Clear();
                                _discardPile.Add(topCard);
                                
                                // Create new deck from the discarded cards
                                Deck = new Deck(newDeckCards);
                                Deck.Shuffle();
                                Console.WriteLine("Deck was empty. Reshuffled discard pile into a new deck.");
                            }
                            else
                            {
                                // If there's not enough cards, end the game
                                Console.WriteLine("Not enough cards to continue. Round ends.");
                                return;
                            }
                        }
                        
                        drawnCard = Deck.DealCard();
                        Console.WriteLine($"{participant.Name} draws from deck: {drawnCard}");
                    }
                    else
                    {
                        if (_discardPile.Count == 0)
                        {
                            drawnCard = Deck.DealCard();
                            Console.WriteLine($"No cards in discard pile. {participant.Name} draws from deck: {drawnCard}");
                        }
                        else
                        {
                            drawnCard = _discardPile.Last();
                            _discardPile.RemoveAt(_discardPile.Count - 1);
                            Console.WriteLine($"{participant.Name} takes {drawnCard} from discard pile");
                        }
                    }
                    
                    participant.ReceiveCard(drawnCard);
                    
                    // Analyze hand after drawing
                    RummyCombinations combinations = AnalyzeHand(participant.Hand);
                    _playerStates[participant].Combinations = combinations;
                    
                    // Display detected combinations
                    if (combinations.Sets.Count > 0)
                    {
                        Console.WriteLine("Sets:");
                        foreach (var set in combinations.Sets)
                        {
                            Console.WriteLine("  " + set);
                        }
                    }
                    
                    if (combinations.Runs.Count > 0)
                    {
                        Console.WriteLine("Runs:");
                        foreach (var run in combinations.Runs)
                        {
                            Console.WriteLine("  " + run);
                        }
                    }
                    
                    // Check if player wants to declare and go out
                    bool shouldDeclare = ((IRummyStrategy)participant.Strategy).ShouldDeclare(participant, _gameContext, combinations);
                    
                    if (shouldDeclare && combinations.CanGoOut)
                    {
                        Console.WriteLine($"{participant.Name} declares and goes out!");
                        _declaredParticipant = participant;
                        _winner = participant;
                        
                        // No need to discard if going out
                        continue;
                    }
                    
                    // Discard phase - select a card to discard
                    int discardIndex = ((IRummyStrategy)participant.Strategy).SelectCardToDiscard(participant, _gameContext);
                    if (discardIndex < 0 || discardIndex >= participant.Hand.Count)
                    {
                        discardIndex = 0; // Default to first card if invalid index
                    }
                    
                    Card discardedCard = participant.Hand[discardIndex];
                    participant.Hand.RemoveAt(discardIndex);
                    _discardPile.Add(discardedCard);
                    
                    Console.WriteLine($"{participant.Name} discards: {discardedCard}");
                    Console.WriteLine($"Hand after discard: {string.Join(", ", participant.Hand)}");
                    
                    // Re-analyze hand after discarding
                    combinations = AnalyzeHand(participant.Hand);
                    _playerStates[participant].Combinations = combinations;
                }
            }
            
            // If max turns reached without anyone going out
            if (_declaredParticipant == null && _currentTurn >= MaxTurns)
            {
                Console.WriteLine($"\nReached maximum turns ({MaxTurns}). Round ends without anyone going out.");
            }
        }

        /// <summary>
        /// Determines and displays winners
        /// </summary>
        public override void DetermineWinners()
        {
            Console.WriteLine("\n--- Round Summary ---");
            
            // Ensure we have player states for all participants
            foreach (var participant in Participants)
            {
                if (!_playerStates.ContainsKey(participant))
                {
                    _playerStates[participant] = new RummyPlayerState
                    {
                        Participant = participant,
                        Combinations = AnalyzeHand(participant.Hand)
                    };
                }
            }
            
            if (_declaredParticipant != null)
            {
                Console.WriteLine($"{_declaredParticipant.Name} has gone out!");
                _winner = _declaredParticipant; // Player who goes out is the winner
            }
            else
            {
                Console.WriteLine("No one has gone out. Determining winner based on points.");
                
                // Find player with lowest unmatched points
                List<Participant> lowestPointsPlayers = new List<Participant>();
                int lowestPoints = int.MaxValue;
                
                foreach (var participant in Participants)
                {
                    int points = _playerStates[participant].Combinations.UnmatchedPoints;
                    
                    if (points < lowestPoints)
                    {
                        lowestPoints = points;
                        lowestPointsPlayers.Clear();
                        lowestPointsPlayers.Add(participant);
                    }
                    else if (points == lowestPoints)
                    {
                        lowestPointsPlayers.Add(participant);
                    }
                }
                
                // If there's a tie, use a tiebreaker: most sets/runs
                if (lowestPointsPlayers.Count > 1)
                {
                    Console.WriteLine($"Tie between {lowestPointsPlayers.Count} players with {lowestPoints} points. Using tiebreaker.");
                    
                    Participant tiebreakWinner = null;
                    int highestCombinations = -1;
                    
                    foreach (var participant in lowestPointsPlayers)
                    {
                        int combinationsCount = _playerStates[participant].Combinations.Sets.Count + 
                                              _playerStates[participant].Combinations.Runs.Count;
                        
                        if (combinationsCount > highestCombinations)
                        {
                            highestCombinations = combinationsCount;
                            tiebreakWinner = participant;
                        }
                    }
                    
                    // If there's still a tie, select the first player (arbitrary but consistent)
                    _winner = tiebreakWinner ?? lowestPointsPlayers[0];
                    
                    Console.WriteLine($"Tiebreaker: {_winner.Name} wins with {highestCombinations} combinations.");
                }
                else if (lowestPointsPlayers.Count == 1)
                {
                    _winner = lowestPointsPlayers[0];
                    Console.WriteLine($"{_winner.Name} wins with the lowest unmatched points: {lowestPoints}");
                }
                else
                {
                    // This should never happen if there are participants, but just in case
                    _winner = Participants.FirstOrDefault();
                    Console.WriteLine("No clear winner determined. Selecting first player as winner.");
                }
            }
            
            // Calculate scores and prepare results
            foreach (var participant in Participants)
            {
                var state = _playerStates[participant];
                int unmatchedPoints = state.Combinations.UnmatchedPoints;
                
                var result = new RummyRoundResult
                {
                    Participant = participant,
                    WentOut = participant == _declaredParticipant,
                    IsWinner = participant == _winner,
                    UnmatchedPoints = unmatchedPoints,
                    Hand = new List<Card>(participant.Hand),
                    CombinationsCount = state.Combinations.Sets.Count + state.Combinations.Runs.Count
                };
                
                _roundResults[participant] = result;
                
                // Display results
                if (participant == _declaredParticipant)
                {
                    Console.WriteLine($"{participant.Name} went out! Score: 0 points");
                }
                else
                {
                    string resultText = participant == _winner && _declaredParticipant == null
                        ? $"wins with {unmatchedPoints} points in unmatched cards ({result.CombinationsCount} combinations)"
                        : $"has {unmatchedPoints} points in unmatched cards ({result.CombinationsCount} combinations)";
                        
                    Console.WriteLine($"{participant.Name} {resultText}");
                    
                    if (state.Combinations.UnmatchedCards.Count > 0)
                    {
                        Console.WriteLine($"  Unmatched cards: {string.Join(", ", state.Combinations.UnmatchedCards)}");
                    }
                    
                    if (state.Combinations.Sets.Count > 0 || state.Combinations.Runs.Count > 0)
                    {
                        Console.WriteLine($"  Formed {state.Combinations.Sets.Count} sets and {state.Combinations.Runs.Count} runs");
                    }
                }
            }
            
            // Ensure a winner is always set
            if (_winner == null && Participants.Count > 0)
            {
                _winner = Participants[0];
                Console.WriteLine($"Fallback winner selection: {_winner.Name}");
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

            // Make sure a winner exists if we have participants
            if (_winner == null && Participants.Count > 0)
            {
                // This is a fallback case that should rarely happen
                _winner = Participants[0];
            }

            foreach (var participant in Participants)
            {
                // Check if the participant has round results
                if (!_roundResults.TryGetValue(participant, out var result))
                {
                    // If not, create default results based on the player's hand
                    var combinations = AnalyzeHand(participant.Hand);
                    
                    result = new RummyRoundResult
                    {
                        Participant = participant,
                        WentOut = false,
                        IsWinner = participant == _winner,
                        UnmatchedPoints = combinations.UnmatchedPoints,
                        Hand = new List<Card>(participant.Hand),
                        CombinationsCount = combinations.Sets.Count + combinations.Runs.Count
                    };
                    
                    _roundResults[participant] = result;
                }
                
                GameResult gameResult;
                if (result.IsWinner)
                {
                    gameResult = GameResult.Win;
                }
                else if (_winner != null) // Ensure there's a winner to compare against
                {
                    gameResult = GameResult.Loss;
                }
                else
                {
                    gameResult = GameResult.Push; // This case should now be very rare
                }
                
                var outcome = new GameOutcome
                {
                    ParticipantName = participant.Name,
                    StrategyName = participant.Strategy.Name,
                    Result = gameResult,
                    // Use HandValue to store the unmatched points (for Rummy, lower is better)
                    HandValue = result.UnmatchedPoints,
                    // Mark if this participant went out
                    HasBlackjack = result.WentOut,  // Using HasBlackjack field to indicate going out
                    // Store the number of combinations in DealerValue field for reporting
                    DealerValue = result.CombinationsCount
                };
                
                roundResult.Outcomes.Add(outcome);
            }
            
            return roundResult;
        }

        /// <summary>
        /// Analyzes a hand to find sets and runs
        /// </summary>
        public RummyCombinations AnalyzeHand(List<Card> hand)
        {
            var result = new RummyCombinations();
            var workingHand = new List<Card>(hand);
            
            // First look for sets (same rank)
            var rankGroups = workingHand
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 3)
                .OrderByDescending(g => g.Count());
                
            foreach (var group in rankGroups)
            {
                var set = new RummySet { Cards = group.ToList() };
                result.Sets.Add(set);
                
                // Remove cards from working hand
                foreach (var card in set.Cards)
                {
                    workingHand.RemoveAll(c => c.Suit == card.Suit && c.Rank == card.Rank);
                }
            }
            
            // Then look for runs (consecutive ranks in same suit)
            var suitGroups = workingHand.GroupBy(c => c.Suit);
            foreach (var suitGroup in suitGroups)
            {
                var suitCards = suitGroup.OrderBy(c => GetCardValue(c)).ToList();
                
                // Find runs in this suit
                for (int i = 0; i < suitCards.Count; i++)
                {
                    var potentialRun = new List<Card> { suitCards[i] };
                    int currentValue = GetCardValue(suitCards[i]);
                    
                    // Look for consecutive cards
                    for (int j = i + 1; j < suitCards.Count; j++)
                    {
                        int nextValue = GetCardValue(suitCards[j]);
                        if (nextValue == currentValue + 1)
                        {
                            potentialRun.Add(suitCards[j]);
                            currentValue = nextValue;
                        }
                        else if (nextValue > currentValue + 1)
                        {
                            // Gap in sequence, stop looking
                            break;
                        }
                    }
                    
                    // If we found a valid run
                    if (potentialRun.Count >= 3)
                    {
                        var run = new RummyRun { Cards = potentialRun };
                        result.Runs.Add(run);
                        
                        // Remove cards from working hand
                        foreach (var card in potentialRun)
                        {
                            workingHand.RemoveAll(c => c.Suit == card.Suit && c.Rank == card.Rank);
                        }
                        
                        // Adjust index to skip past this run
                        i += potentialRun.Count - 1;
                    }
                }
            }
            
            // Remaining cards are unmatched
            result.UnmatchedCards = workingHand;
            
            return result;
        }
        
        /// <summary>
        /// Gets the numeric value of a card (Ace=1, Jack=11, Queen=12, King=13)
        /// </summary>
        private int GetCardValue(Card card)
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
        
        /// <summary>
        /// Gets a card from the top of the discard pile without removing it
        /// </summary>
        public Card GetTopDiscard()
        {
            if (_discardPile.Count == 0)
                return null;
                
            return _discardPile[_discardPile.Count - 1];
        }
    }
    
    /// <summary>
    /// Tracks a player's state during the game
    /// </summary>
    public class RummyPlayerState
    {
        public Participant Participant { get; set; }
        public RummyCombinations Combinations { get; set; }
    }
    
    /// <summary>
    /// Tracks the result of a round for a specific player
    /// </summary>
    public class RummyRoundResult
    {
        public Participant Participant { get; set; }
        public bool WentOut { get; set; }
        public bool IsWinner { get; set; }
        public int UnmatchedPoints { get; set; }
        public List<Card> Hand { get; set; }
        public int CombinationsCount { get; set; } // Number of sets + runs
    }
}