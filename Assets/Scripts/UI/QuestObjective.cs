namespace GameStart.UI
{
    [System.Serializable]
    public class QuestObjective
    {
        public string Description;
        public int TargetCount;
        public int CurrentCount;

        public bool IsComplete => CurrentCount >= TargetCount;
    }
}
