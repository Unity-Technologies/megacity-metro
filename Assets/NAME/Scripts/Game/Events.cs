using UnityEngine;

namespace Unity.NAME.Game
{
    // The Game Events used across the Game.
    // Anytime there is a need for a new event, it should be added here

    public static class Events
    {
        // Declare all events here to prevent runtime allocations
        public static ExampleEvent ExampleEvent = new ExampleEvent();

        public static SetTimeEvent SetTimeEvent = new SetTimeEvent();
        public static AdjustTimeEvent AdjustTimeEvent = new AdjustTimeEvent();
        public static GameStateChangeEvent GameStateChangeEvent = new GameStateChangeEvent();
        public static DisplayMessageEvent DisplayMessageEvent = new DisplayMessageEvent();
        public static UserMenuActionEvent UserMenuActionEvent = new UserMenuActionEvent();
    }

    public class ExampleEvent : GameEvent
    {
        public GameObject GameObject;
    }

    // Timer Events
    public class SetTimeEvent : GameEvent
    {
        public int Time;
    }

    public class AdjustTimeEvent : GameEvent
    {
        public int DeltaTime;
    }

    // Gameflow Events
    public class GameStateChangeEvent : GameEvent
    {
        public GameState CurrentGameState;
        public GameState NewGameState;
    }

    public class DisplayMessageEvent : GameEvent
    {
        public string MessageText;
        public float DelayBeforeDisplay;
    }

    // UI Events
    public class UserMenuActionEvent : GameEvent
    {
        public UserMenuAction UserMenuAction = UserMenuAction.Play;
    }
}
