using _Scripts.Shared._Data;
using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Player.Status;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Player
{
    public class PlayerScope : LifetimeScope
    {
        [SF] private PlayerData playerData;
        [SF] private AvatarData avatarData;
        
        [SF] private Rigidbody playerRb;
        [SF] private SkinnedMeshRenderer playerMeshRenderer;
        [SF] private Animator animator;
        [SF] private Transform detectPoint;
         
        [SF] private PlayerSyncer playerSyncer;
        [SF] private MovementBehaviour movementBehaviour;
        [SF] private InteractionBehaviour interactionBehaviour;
        [SF] private CarrierBehaviour carrierBehaviour;
        [SF] private CollisionBehaviour collisionBehaviour;
        [SF] private EmotionBehaviour emotionBehaviour;

        private readonly DisposableBagBuilder _disposableBagBuilder = DisposableBag.CreateBuilder();

        protected override void OnDestroy()
        {
            _disposableBagBuilder?.Build().Dispose();
            base.OnDestroy();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.UseComponents(RegisterComponents);
            
            builder.Register<MoveStatus>(Lifetime.Singleton);
            builder.Register<DetectStatus>(Lifetime.Singleton);
            builder.Register<CarryStatus>(Lifetime.Singleton);
            builder.Register<InteractStatus>(Lifetime.Singleton);

            builder.RegisterInstance(playerData);
            builder.RegisterInstance(avatarData);
            
            builder.RegisterInstance(_disposableBagBuilder);
            
            base.Configure(builder);
        }

        private void RegisterComponents(ComponentsBuilder builder)
        {
            builder.AddInstance(playerRb);
            builder.AddInstance(playerMeshRenderer);
            builder.AddInstance(animator);
            builder.AddInstance(detectPoint);
                
            builder.AddInstance(playerSyncer);
            builder.AddInstance(collisionBehaviour);
                
            builder
                .AddInstance(movementBehaviour)
                .As<IBehaviourWithInput>()
                .AsSelf();
            builder
                .AddInstance(interactionBehaviour)
                .As<IBehaviourWithInput>()
                .AsSelf();
            builder
                .AddInstance(carrierBehaviour)
                .As<IBehaviourWithInput>()
                .AsSelf();
        }
    }
}