namespace GameStart.Narrative
{
    [System.Serializable]
    public struct LoreEntry
    {
        public string Title;
        [UnityEngine.TextArea(3, 10)]
        public string Body;

        public LoreEntry(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }
}
