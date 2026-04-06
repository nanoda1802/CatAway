using _Scripts._Messages.Stage;
using _Scripts._Shared.Enums;
using _Scripts._Shared.Sound;
using _Scripts._Shared.UI;
using _Scripts._Shared.UI.QuickMenu.ButtonActions;
using _Scripts.Messages;
using _Scripts.Messages.Stage;
using _Scripts.Scene_Room.Data;
using _Scripts.Scene_Stage.Data;
using _Scripts.Scene_Stage.Data.Level;
using _Scripts.Scene_Stage.Player;
using _Scripts.Scene_Stage.Table;
using _Scripts.Scene_Stage.UI.Board.Order;
using _Scripts.Scene_Stage.UI.Board.Score;
using _Scripts.Scene_Stage.UI.Pop;
using _Scripts.Stage;
using MessagePipe;
using VContainer;
using VContainer.Unity;
using Application = UnityEngine.Device.Application;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage
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
            builder.RegisterEntryPoint<StageInitiator>();
            builder.RegisterEntryPoint<StageHub>().AsSelf();

            builder.Register<IButtonAction<QuickMenuButtonType>, SettingsAction>(Lifetime.Scoped);
            builder.Register<IButtonAction<QuickMenuButtonType>, LeaveAction>(Lifetime.Scoped);
            
            builder.Register<StageStatus>(Lifetime.Singleton);
            builder.Register<PlacementBroker>(Lifetime.Singleton);
            builder.Register<ContactBroker>(Lifetime.Singleton);

            var soundManager = Parent.Container.Resolve<SoundManager>();
            sfxListData.Inject(soundManager);
            
            var stageData = Parent.Container.Resolve<RoomStatus>().CurStageData;
            builder.RegisterInstance(stageData);
            builder.RegisterInstance(sfxListData)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterInstance(providerData)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterInstance(playerPrefab);
            
            builder.RegisterInstance(_disposableBagBuilder);
            
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
            
            base.Configure(builder);
        }

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }
    }
}