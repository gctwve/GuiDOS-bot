namespace YourCheese
{
    static class TaskScreenLayout
    {
        public static readonly Vector2 Center = Percent(0.5f, 0.5f);
        public static readonly Vector2 CloseButton = Percent(0.885f, 0.278f);
        public static readonly Vector2 MouseTestCard = Percent(0.356f, 0.756f);
        public static readonly Vector2 MouseTestSwipeEnd = Percent(0.765f, 0.379f);

        public static Vector2 Percent(float x, float y)
        {
            return new Vector2(GameWindow.DesignWidth * x, GameWindow.DesignHeight * y);
        }
    }
}
