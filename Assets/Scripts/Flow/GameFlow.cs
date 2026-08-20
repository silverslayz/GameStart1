namespace GameStart.Flow
{
    public static class GameFlow
    {
        public const string TitleSceneName = "TitleScreen";
        public const string GameplaySceneName = "DemoRoom";

        /// <summary>Set by the title screen's "New Game" button; consumed once by PlayerSaveController.Start() in the gameplay scene.</summary>
        public static bool PendingNewGame;
    }
}
