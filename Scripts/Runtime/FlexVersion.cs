namespace FlexAnimation
{
    public static class FlexVersion
    {
        public const string VERSION = "2.1.0";
        public const string LAST_UPDATE = "2026-03-19";
        
        public static void LogVersion()
        {
            UnityEngine.Debug.Log($"<b>Flex Animation Pro</b> Version: {VERSION} (Updated: {LAST_UPDATE})");
        }
    }
}
