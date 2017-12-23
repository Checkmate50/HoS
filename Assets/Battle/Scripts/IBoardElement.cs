namespace battle
{
  public interface IBoardElement
  {
    int Row { get; }
    int Column { get; }
    int Cost { get; }
    int Damage { get; }
  }
}
