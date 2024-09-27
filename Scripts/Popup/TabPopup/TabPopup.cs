using Cysharp.Threading.Tasks;

namespace PlayVibe.TabPopup
{
    public class TabPopup : AbstractBasePopup
    {
        protected override UniTask OnShow(object data = null)
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnHide()
        {
            return UniTask.CompletedTask;
        }

        protected override void OnShowen()
        {
            
        }

        protected override void OnHiden()
        {
            
        }
    }
}