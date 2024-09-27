using System.Collections.Generic;

namespace PlayVibe
{
    public class PopupGroupConstants
    {
        public static List<PopupGroup> IncludeAll = null;
        public static List<PopupGroup> DefaultExcludeGroups = new List<PopupGroup>() { PopupGroup.Hud, PopupGroup.Tutorial };
    }
}