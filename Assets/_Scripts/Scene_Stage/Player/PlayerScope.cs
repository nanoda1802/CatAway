using _Scripts._Shared.Data;
using _Scripts.Scene_Stage.Player.Behaviour;
using _Scripts.Scene_Stage.Player.Status;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Scene_Stage.Player
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
            builder.Register<PlayerInput>(Lifetime.Scoped);
            
            builder.Register<MoveStatus>(Lifetime.Scoped);
            builder.Register<DetectStatus>(Lifetime.Scoped);
            builder.Register<CarryStatus>(Lifetime.Scoped);
            builder.Register<InteractStatus>(Lifetime.Scoped);

            builder.RegisterInstance(playerData);
            builder.RegisterInstance(avatarData);
            
            builder.RegisterInstance(_disposableBagBuilder);

            builder.UseComponents(componentsBuilder =>
            {
                componentsBuilder.AddInstance(playerRb);
                componentsBuilder.AddInstance(playerMeshRenderer);
                componentsBuilder.AddInstance(animator);
                componentsBuilder.AddInstance(detectPoint);
                
                componentsBuilder.AddInstance(playerSyncer);
                componentsBuilder.AddInstance(collisionBehaviour);
                
                componentsBuilder
                    .AddInstance(movementBehaviour)
                    .As<IBehaviourWithInput>()
                    .AsSelf();
                componentsBuilder
                    .AddInstance(interactionBehaviour)
                    .As<IBehaviourWithInput>()
                    .AsSelf();
                componentsBuilder
                    .AddInstance(carrierBehaviour)
                    .As<IBehaviourWithInput>()
                    .AsSelf();
                // componentsBuilder.AddInstance(emotionBehaviour);
            });
            
            base.Configure(builder);
        }
    }
}