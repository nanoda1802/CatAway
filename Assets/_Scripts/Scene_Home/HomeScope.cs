using _Scripts._Shared.Enums;
using _Scripts._Shared.UI;
using _Scripts._Shared.UI.QuickMenu.ButtonActions;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using _Scripts.Scene_Home.Data;
using MessagePipe;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Home
{
    public class HomeScope : LifetimeScope
    {
        [SF] private HomeViewData  homeViewData;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(homeViewData);
            
            builder.Register<IButtonAction<QuickMenuButtonType>, RenameAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, CustomizeAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, TutorialAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, ExitAction>(Lifetime.Scoped);

            var msgOptions = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<CreateRoomRequest>(msgOptions);
            builder.RegisterMessageBroker<JoinRoomRequest>(msgOptions);
            builder.RegisterMessageBroker<PopUpMessage>(msgOptions);
            builder.RegisterMessageBroker<PopDownMessage>(msgOptions);
            builder.RegisterMessageBroker<DialogMessage>(msgOptions);
            builder.RegisterMessageBroker<AvatarMessage>(msgOptions);
            
            base.Configure(builder);
        }
    }
}