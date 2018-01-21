namespace Board
{
  public interface IBoardElement
  {
    // The cost of moving into/through this board element
    int Cost { get; }
  }
}
