using StardewModdingAPI;

namespace JunimoServer.Services.AlwaysOn
{
    public class AlwaysOnConfig
    {
        public SButton HotKeyToggleAutoMode { get; set; } = SButton.F9;
        public SButton HotKeyToggleVisibility { get; set; } = SButton.F10;

        public string PetName { get; set; } = "Apples";
        public bool FarmCaveChoiceIsMushrooms { get; set; } = true;
        public bool IsCommunityCenterRun { get; set; } = true;

        public bool LockPlayerChests { get; set; } = true;

        public int EggHuntCountDownConfig { get; set; } = 300;
        public int FlowerDanceCountDownConfig { get; set; } = 300;
        public int LuauSoupCountDownConfig { get; set; } = 300;
        public int JellyDanceCountDownConfig { get; set; } = 300;
        public int GrangeDisplayCountDownConfig { get; set; } = 300;
        public int IceFishingCountDownConfig { get; set; } = 300;

        public int EndOfDayTimeOut { get; set; } = 120000;
        public int FairTimeOut { get; set; } = 120000;
        public int SpiritsEveTimeOut { get; set; } = 120000;
        public int WinterStarTimeOut { get; set; } = 120000;

        public int EggFestivalTimeOut { get; set; } = 120000;
        public int FlowerDanceTimeOut { get; set; } = 120000;
        public int LuauTimeOut { get; set; } = 120000;
        public int DanceOfJelliesTimeOut { get; set; } = 120000;
        public int FestivalOfIceTimeOut { get; set; } = 120000;

        // Pause time when the sole connected player is inactive. Menus prevent
        // movement, so idle detection approximates single-player menu-pause
        // without needing a client-side mod. Overridden by SOLO_AUTO_PAUSE_ENABLED.
        public bool SoloAutoPauseEnabled { get; set; } = true;

        // Ticks of inactivity before solo auto-pause engages. 30 ≈ 0.5s at 60 TPS.
        // Overridden by SOLO_AUTO_PAUSE_IDLE_TICKS.
        public int SoloAutoPauseIdleTicks { get; set; } = 30;
    }
}
