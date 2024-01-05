namespace Unity.NAME.Game
{
    // A simple class to get a specific place where we put all sorts of Enums

    public enum GameState
    {
        Play,
        Success,
        Failure,
        Menu
    }

    public enum GameMode
    {
        TimeLimit,
        Adventure,
        Construction,
        Whatever,
    }

    public enum UserMenuAction
    {
        Play,
        ReturnToIntroMenu
    }
}
