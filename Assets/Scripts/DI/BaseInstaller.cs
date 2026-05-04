using Events;
using Zenject;
namespace DI
{
    public class BaseInstaller : MonoInstaller
    {
        public EventBus EventBus;

        public override void InstallBindings()
        {
            Container.Bind<EventBus>().FromScriptableObject(EventBus).AsSingle().NonLazy();
        }
    }
}
