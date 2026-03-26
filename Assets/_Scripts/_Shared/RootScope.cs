using _Scripts._Messages.Shared;
using _Scripts._Shared.Data;
using _Scripts.Messages;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage.Data;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts._Shared
{
    public class RootScope : LifetimeScope
    {
        [Header("[ Components ]")]
        [SF] private NetworkManager netManager;
        [SF] private UnityTransport utp;
        [Header("[ Data ]")]
        [SF] private StageListData stageList;
        [SF] private AvatarData avatarData;

        private DisposableBagBuilder _rootDisposableBagBuilder;

        protected override void OnDestroy()
        {
            _rootDisposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseEntryPoints(pointsBuilder =>
            {
                pointsBuilder.Add<SessionManager>().AsSelf();
                pointsBuilder.Add<RoomStatus>().AsSelf();
                pointsBuilder.Add<SceneChanger>().AsSelf();
                pointsBuilder.Add<PlayerStatus>().AsSelf();
            });
            
            builder.UseComponents(componentsBuilder =>
            {
                componentsBuilder.AddInstance(netManager);
                componentsBuilder.AddInstance(utp);
                componentsBuilder.AddInstance(avatarData);
            });
            
            builder
                .RegisterInstance(stageList)
                .AsImplementedInterfaces()
                .AsSelf();

            RegisterRootDisposableBag(builder);
            RegisterMessages(builder);
            
            base.Configure(builder);
        }

        private void RegisterRootDisposableBag(IContainerBuilder builder)
        {
            _rootDisposableBagBuilder = DisposableBag.CreateBuilder();
            builder.RegisterInstance(_rootDisposableBagBuilder);
        }

        private void RegisterMessages(IContainerBuilder builder)
        {
            var msgOpt = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<LoadSceneMessage>(msgOpt);
            builder.RegisterMessageBroker<AvatarMessage>(msgOpt);
            builder.RegisterMessageBroker<RenameMessage>(msgOpt);
        }
    }
}