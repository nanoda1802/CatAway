using _Scripts.Lobby.Room;
using _Scripts.Lobby.UI.Messages;
using _Scripts.Lobby.UI.Messages.Member;
using _Scripts.Lobby.UI.Messages.Room;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Lobby
{
    public class LobbyScope : LifetimeScope
    {
        [SF] private NetworkManager networkManager;
        [SF] private UnityTransport transport;
        [SF] private RoomConnector roomConnector;
        [SF] private RoomSyncer roomSyncer;
        [SF] private MemberSyncer memberSyncer;
         
        [SF] private RoomMember roomMemberPrefab;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(networkManager);
            builder.RegisterComponent(transport);
            builder.RegisterComponent(roomConnector);
            builder.RegisterComponent(roomSyncer);
            builder.RegisterComponent(memberSyncer);
            
            builder.RegisterInstance(roomMemberPrefab);
            
            var msgOptions = builder.RegisterMessagePipe();

            builder.RegisterMessageBroker<ChangeViewRequest>(msgOptions);
            
            builder.RegisterMessageBroker<PopUpMessage>(msgOptions);
            builder.RegisterMessageBroker<PopDownMessage>(msgOptions);
            
            builder.RegisterMessageBroker<DialogMessage>(msgOptions);
            builder.RegisterMessageBroker<NoticeMessage>(msgOptions);
            
            builder.RegisterMessageBroker<CreateRoomRequest>(msgOptions);
            builder.RegisterMessageBroker<JoinRoomRequest>(msgOptions);
            builder.RegisterMessageBroker<LeaveRoomRequest>(msgOptions);
            
            builder.RegisterMessageBroker<InitRoomMessage>(msgOptions);
            builder.RegisterMessageBroker<SwitchModeRequest>(msgOptions);
            builder.RegisterMessageBroker<SwitchModeRespond>(msgOptions);
            builder.RegisterMessageBroker<SwitchStartMessage>(msgOptions);
            
            builder.RegisterMessageBroker<ShowMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<HideMemberCardMessage>(msgOptions);
            builder.RegisterMessageBroker<SwitchReadyRequest>(msgOptions);
            builder.RegisterMessageBroker<SwitchReadyRespond>(msgOptions);
        }
    }
}