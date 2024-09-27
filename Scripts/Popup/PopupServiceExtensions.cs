namespace PlayVibe
{
    public static class PopupServiceExtensions
    {
        public static bool HasAnyOpenedPopup(this PopupService service)
        {
            return service.HasAnyOpenedPopup(PopupGroupConstants.DefaultExcludeGroups);
        }
    }
}