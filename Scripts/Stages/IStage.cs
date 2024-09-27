using Cysharp.Threading.Tasks;

namespace PlayVibe
{
    public interface IStage
    { 
        UniTask Initialize(object data);
        UniTask DeInitialize();
    }
}