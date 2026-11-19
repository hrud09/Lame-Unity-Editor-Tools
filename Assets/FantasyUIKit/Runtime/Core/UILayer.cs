namespace FantasyUI.Core
{
    /// <summary>
    /// Deterministic sorting-order layers matching GameplayScene & standard game UI stacks.
    /// </summary>
    public enum UILayer
    {
        Minimap = 0,
        WorldHealthbars = 1,
        GameplayHUD = 2,
        EdgeMarkers = 3,
        PowerupOverlay = 5,
        Popups = 8,
        FeedbackBanners = 10,
        SystemDebug = 12
    }
}
