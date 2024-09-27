using Zenject;

namespace PlayVibe
{
    public static class StageServiceExtensions
    {
        public static StageService BindStageService(this DiContainer container)
        {
            container.Bind<StageService>().AsSingle();
            
            return container.Resolve<StageService>();
        }

        public static StageService BindStage<TStage>(this StageService service) where TStage : class
        {
            service.Container.Bind<TStage>().AsSingle();
            
            return service;
        }
    }
}