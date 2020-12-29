namespace sr
{
  [System.Flags]
  public enum SearchStatus
  {
    None,
    InProgress,
    Complete,
    UserAborted,
    ExceptionThrown
  }
}