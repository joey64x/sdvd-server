namespace JunimoServer.Services.AlwaysOn
{
    public static class AlwaysOnOverrides
    {
        // When true, Game1.shouldTimePass returns false on the server so the
        // time-of-day clock freezes without setting netWorldState.IsPaused
        // (which would also freeze client controls and prevent players from moving).
        public static bool SoloTimeFrozen;

        public static bool ShouldTimePass_Prefix(ref bool __result)
        {
            if (SoloTimeFrozen)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
