namespace GameStudioB
{
    public class Card
    {
        public enum SuitValue
        {
            Hearts,
            Diamonds,
            Clubs,
            Spades
        }

        public enum RankValue
        {
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13,
            Ace = 14
        }

        public SuitValue Suit { get; set; }
        public RankValue Rank { get; set; }

        public Card(SuitValue suit, RankValue rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public int GetBlackjackValue()
        {
            if (Rank == RankValue.Ace)
            {
                // Ace can be 1 or 11, but we'll return 11 here and handle the logic elsewhere
                return 11;
            }
            else if (Rank == RankValue.Jack || Rank == RankValue.Queen || Rank == RankValue.King)
            {
                return 10;
            }
            else
            {
                return (int)Rank;
            }
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}