namespace PlayVibe
{
    public sealed class PopupOptions
    {
        public object Data { get; }
        public string PopupKey { get; }
        public PopupGroup PopupGroup { get; }
        public int? SortingOrder { get; }

        public PopupOptions(
            string popupKey,
            object data = null,
            PopupGroup popupGroup = PopupGroup.Gameplay,
            int? sortingOrder = null)
        {
            PopupKey = popupKey;
            Data = data;
            PopupGroup = popupGroup;
            SortingOrder = sortingOrder;
        }
    }
}
