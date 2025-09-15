using System.Collections.Generic;

namespace GameStudioB
{
    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public bool IsDealer { get; set; }
        public bool HasStood { get; set; } = false;
        public bool IsBusted => CalculateHandValue() > 21;
        public bool HasBlackjack => Hand.Count == 2 && CalculateHandValue() == 21;

        public Player(string name, bool isDealer = false)
        {
            Name = name;
            IsDealer = isDealer;
        }

        public void ClearHand()
        {
            Hand.Clear();
            HasStood = false;
        }

        public void AddCard(Card card)
        {
            Hand.Add(card);
        }

        public int CalculateHandValue()
        {
            int value = 0;
            int aceCount = 0;

            foreach (Card card in Hand)
            {
                int cardValue = card.GetBlackjackValue();
                
                if (cardValue == 11) // If it's an Ace
                {
                    aceCount++;
                }
                
                value += cardValue;
            }

            // Adjust for Aces if needed
            while (value > 21 && aceCount > 0)
            {
                value -= 10; // Convert an Ace from 11 to 1
                aceCount--;
            }

            return value;
        }

        public void DisplayHand(bool hideFirstCard = false)
        {
            if (Hand.Count == 0)
            {
                System.Console.WriteLine($"{Name}'s hand is empty");
                return;
            }

            System.Console.WriteLine($"{Name}'s hand:");
            
            for (int i = 0; i < Hand.Count; i++)
            {
                if (i == 0 && hideFirstCard && IsDealer)
                {
                    System.Console.WriteLine(" - [Hidden Card]");
                }
                else
                {
                    System.Console.WriteLine($" - {Hand[i]}");
                }
            }

            if (!hideFirstCard)
            {
                System.Console.WriteLine($"Total value: {CalculateHandValue()}");
            }
            else if (IsDealer)
            {
                // Only show the value of the visible card for the dealer
                System.Console.WriteLine($"Visible card value: {(Hand.Count > 1 ? Hand[1].GetBlackjackValue() : 0)}");
            }
        }
    }
}