namespace GlobalSpace
{
    public class PlayerState
    {
    }
    
    public class GameProgress
    {
        public bool skipIntro = false;
        public PlayerState PlayerState { get; private set; }

        public GameProgress()
        {
            PlayerState = new PlayerState();
        }
    }
}