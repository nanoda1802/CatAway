using _Scripts._Helper;
using _Scripts.Shared._Data;
using _Scripts.Shared._Messages;
using _Scripts.Shared.Sound;
using _Scripts.Stage._Data;
using MessagePipe;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Shared
{
    public class RootScope : LifetimeScope
    {
        [Header("[ Components ]")]
        [SF] private NetworkManager netManager;
        [SF] private UnityTransport utp;
        [SF] private SoundManager soundManager;
        [Header("[ Data ]")]
        [SF] private StageListData stageList;
        [SF] private AvatarData avatarData;
        [SF] private SoundSettingsData soundSettingsData;

        private DisposableBagBuilder _rootDisposableBagBuilder;

        protected override void OnDestroy()
        {
            _rootDisposableBagBuilder?.Build().Dispose();
            
            base.OnDestroy();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseEntryPoints(RegisterEntryPoints);
            builder.UseComponents(RegisterComponents);
            
            builder.Register<PlayerInput>(Lifetime.Singleton);
            builder.Register<VfxHandler>(Lifetime.Singleton);
            builder.Register<TweenHandler>(Lifetime.Singleton);
            
            RegisterRootDisposableBag(builder);
            
            RegisterMessages(builder);
            
            base.Configure(builder);
        }

        private void RegisterEntryPoints(EntryPointsBuilder builder)
        {
            builder.Add<ApprovalManager>().AsSelf();
            builder.Add<AuthManager>().AsSelf();
            builder.Add<RoomStatus>().AsSelf();
            builder.Add<SceneChanger>().AsSelf();
            builder.Add<PlayerStatus>().AsSelf();
            builder.Add<SfxProvider>().AsSelf();
        }

        private void RegisterComponents(ComponentsBuilder builder)
        {
            builder.AddInstance(netManager);
            builder.AddInstance(utp);
            builder.AddInstance(soundManager);
            builder.AddInstance(avatarData);
            builder.AddInstance(stageList).AsImplementedInterfaces().AsSelf();
            builder.AddInstance(soundSettingsData).AsImplementedInterfaces().AsSelf();
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