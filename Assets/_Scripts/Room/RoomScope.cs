using _Scripts.Room._Data;
using _Scripts.Room._Messages;
using _Scripts.Room.UI;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI;
using _Scripts.Shared.UI.QuickMenu.ButtonActions;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Room
{
    public class RoomScope : LifetimeScope
    {
        [Header("[ Components ]")]
        [SF] private RectTransform viewRectTr;
        [Header("[ Data ]")]   
        [SF] private RoomMember memberPrefab;
        [SF] private RoomViewData viewData;
        [SF] private RoomMemberCard roomMemberCardPrefab;
        
        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<RoomMemberCardProvider>(Lifetime.Singleton);
            builder.Register<PointSwapper>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterInstance(memberPrefab);
            builder.RegisterInstance(roomMemberCardPrefab);
            
            builder.UseComponents(RegisterComponents);

            RegisterQuickMenuActions(builder);
            RegisterMessages(builder);
            
            builder.RegisterInstance(_disposableBagBuilder);
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }

        private void RegisterQuickMenuActions(IContainerBuilder builder)
        {
            builder.Register<IButtonAction<QuickMenuButtonType>, RenameAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, CustomizeAction>(Lifetime.Scoped);
            // builder.Register<IButtonAction<QuickMenuButtonType>, TutorialAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, LeaveAction>(Lifetime.Scoped);
        }

        private void RegisterComponents(ComponentsBuilder builder)
        {
            builder.AddInstance(viewRectTr);
            // builder.AddInstance(memberPrefab);
            builder.AddInstance(viewData);
            // builder.AddInstance(roomMemberCardPrefab);
        }

        private void RegisterMessages(IContainerBuilder builder)
        {
            var msgOptions = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<InitRoomMessage>(msgOptions);
            builder.RegisterMessageBroker<LeaveRoomMessage>(msgOptions);
            builder.RegisterMessageBroker<RoomToastMessage>(msgOptions);
            
            builder.RegisterMessageBroker<SwitchStartMessage>(msgOptions);
            
            builder.RegisterMessageBroker<SwitchModeRequest>(msgOptions);
            builder.RegisterMessageBroker<SwitchModeRespond>(msgOptions);
            
            builder.RegisterMessageBroker<SwitchReadyRequest>(msgOptions);
            builder.RegisterMessageBroker<SwitchReadyRespond>(msgOptions);
            
            builder.RegisterMessageBroker<SelectStageRequest>(msgOptions);
            builder.RegisterMessageBroker<SelectStageRespond>(msgOptions);
            
            builder.RegisterMessageBroker<ShowRoomMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<HideMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<MoveMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<UpdateMemberNameMessage>(msgOptions);
        }
    }
}