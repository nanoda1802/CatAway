using _Scripts._Messages.Room;
using _Scripts._Messages.Shared;
using _Scripts._Shared.Enums;
using _Scripts._Shared.UI;
using _Scripts._Shared.UI.QuickMenu.ButtonActions;
using _Scripts.Messages;
using _Scripts.Messages.Room;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Room.UI;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Room
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
            builder.Register<IButtonAction<QuickMenuButtonType>, RenameAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, CustomizeAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, TutorialAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, LeaveAction>(Lifetime.Scoped);
            
            builder.Register<RoomMemberCardProvider>(Lifetime.Scoped);

            builder.RegisterInstance(_disposableBagBuilder);
            
            builder.UseComponents(componentsBuilder =>
            {
                componentsBuilder.AddInstance(viewRectTr);
                componentsBuilder.AddInstance(memberPrefab);
                componentsBuilder.AddInstance(viewData);
                componentsBuilder.AddInstance(roomMemberCardPrefab);
            });

            var msgOptions = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<InitRoomMessage>(msgOptions);
            builder.RegisterMessageBroker<LeaveRoomMessage>(msgOptions);
            builder.RegisterMessageBroker<RoomNoticeMessage>(msgOptions);
            
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
            
            
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }
    }
}