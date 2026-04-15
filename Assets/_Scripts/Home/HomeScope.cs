using _Scripts.Home._Data;
using _Scripts.Room._Messages;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI;
using _Scripts.Shared.UI.QuickMenu.ButtonActions;
using MessagePipe;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Home
{
    public class HomeScope : LifetimeScope
    {
        [SF] private HomeViewData  homeViewData;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(homeViewData);
            
            builder.RegisterInstance(_disposableBagBuilder);

            RegisterQuickMenuActions(builder);
            RegisterMessages(builder);   
            
            base.Configure(builder);
        }

        private void RegisterQuickMenuActions(IContainerBuilder builder)
        {
            builder.Register<IButtonAction<QuickMenuButtonType>, RenameAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, CustomizeAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, TutorialAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, ExitAction>(Lifetime.Scoped);
        }

        private void RegisterMessages(IContainerBuilder builder)
        {
            var msgOptions = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<CreateRoomRequest>(msgOptions);
            builder.RegisterMessageBroker<JoinRoomRequest>(msgOptions);
            builder.RegisterMessageBroker<PopUpMessage>(msgOptions);
            builder.RegisterMessageBroker<PopDownMessage>(msgOptions);
            builder.RegisterMessageBroker<DialogMessage>(msgOptions);
            builder.RegisterMessageBroker<AvatarMessage>(msgOptions);
        }
    }
}