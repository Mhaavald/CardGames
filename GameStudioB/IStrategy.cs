namespace GameStudioB
{
    public interface IStrategy
    {
        string Name { get; }
        bool DecideToHit(Player player, Player dealer);
    }
}