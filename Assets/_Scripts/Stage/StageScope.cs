using _Scripts.Shared._Data;
using _Scripts.Shared._Enums;
using _Scripts.Shared._Messages;
using _Scripts.Shared.UI;
using _Scripts.Shared.UI.QuickMenu.ButtonActions;
using _Scripts.Stage._Data;
using _Scripts.Stage._Data.Level;
using _Scripts.Stage._Messages;
using _Scripts.Stage.Player;
using _Scripts.Stage.Table;
using _Scripts.Stage.UI.Board.Order;
using _Scripts.Stage.UI.Board.Score;
using _Scripts.Stage.UI.Pop;
using MessagePipe;
using VContainer;
using VContainer.Unity;
using Application = UnityEngine.Device.Application;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage
{
    public class StageScope : LifetimeScope
    {
        [SF] private ProviderData providerData;
        [SF] private StageSfxListData sfxListData;
        [SF] private PlayerSyncer playerPrefab;

        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();
        
        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            
            this.autoRun = true;
            
            base.Awake();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseEntryPoints(RegisterEntryPoints);
            builder.UseComponents(RegisterComponents);
            
            RegisterQuickMenuActions(builder);
            RegisterMessages(builder);
            
            builder.Register<StageStatus>(Lifetime.Singleton);
            builder.Register<PlacementBroker>(Lifetime.Singleton);
            builder.Register<ContactBroker>(Lifetime.Singleton);

            builder.RegisterInstance(playerPrefab);
            
            builder.RegisterInstance(_disposableBagBuilder);
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }

        private void RegisterEntryPoints(EntryPointsBuilder builder)
        {
            builder.Add<StageInitiator>();
            builder.Add<StageHub>().AsSelf();
        }

        private void RegisterComponents(ComponentsBuilder builder)
        {
            var stageData = Parent.Container.Resolve<RoomStatus>().CurStageData;

            builder.AddInstance(stageData);
            builder.AddInstance(sfxListData)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.AddInstance(providerData)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        private void RegisterQuickMenuActions(IContainerBuilder builder)
        {
            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, LeaveAction>(Lifetime.Scoped);
        }

        private void RegisterMessages(IContainerBuilder builder)
        {
            var msgOptions = Parent.Container.Resolve<MessagePipeOptions>();
            
            builder.RegisterMessageBroker<IPlacable>(msgOptions);
            builder.RegisterMessageBroker<IProvider>(msgOptions);
            builder.RegisterMessageBroker<OrderPresenter>(msgOptions);
            builder.RegisterMessageBroker<ScorePresenter>(msgOptions);
            builder.RegisterMessageBroker<CuePresenter>(msgOptions);
            builder.RegisterMessageBroker<HubCallMessage>(msgOptions);
            
            builder.RegisterMessageBroker<PlayerDespawnMessage>(msgOptions);    
            
            builder.RegisterMessageBroker<ScoreMessage>(msgOptions);
            builder.RegisterMessageBroker<AddOrderMessage>(msgOptions);
            builder.RegisterMessageBroker<RemoveOrderMessage>(msgOptions);
            builder.RegisterMessageBroker<float>(msgOptions); // Timer
            
            builder.RegisterMessageBroker<PopUpMessage>(msgOptions);
            builder.RegisterMessageBroker<PopDownMessage>(msgOptions);
            
            builder.RegisterMessageBroker<CueMessage>(msgOptions);
            
            builder.RegisterMessageBroker<EndStageMessage>(msgOptions);
            builder.RegisterMessageBroker<StartStageMessage>(msgOptions);
        }
    }
}