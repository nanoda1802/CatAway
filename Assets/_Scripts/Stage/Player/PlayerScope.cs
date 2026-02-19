using _Scripts.Stage.Player.Behaviour;
using _Scripts.Stage.Player.Status;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SF = UnityEngine.SerializeField;

namespace _Scripts.Stage.Player
{
    public class PlayerScope : LifetimeScope
    {
        [SF] private PlayerData playerData;
        
        [SF] private Rigidbody playerRb;
        [SF] private Animator animator;
        [SF] private Transform detectPoint;
         
        [SF] private MovementBehaviour movementBehaviour;
        [SF] private InteractionBehaviour interactionBehaviour;
        [SF] private CarrierBehaviour carrierBehaviour;
        [SF] private CollisionBehaviour collisionBehaviour;
        [SF] private EmotionBehaviour emotionBehaviour;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<PlayerInput>(Lifetime.Scoped);
            
            builder.Register<MoveStatus>(Lifetime.Scoped);
            builder.Register<DetectStatus>(Lifetime.Scoped);
            builder.Register<CarryStatus>(Lifetime.Scoped);
            builder.Register<InteractStatus>(Lifetime.Scoped);

            builder.RegisterInstance(playerData);

            builder.RegisterComponent(playerRb);
            builder.RegisterComponent(animator);
            builder.RegisterComponent(detectPoint);
            
            builder.RegisterComponent(movementBehaviour);
            builder.RegisterComponent(interactionBehaviour);
            builder.RegisterComponent(carrierBehaviour);
            builder.RegisterComponent(collisionBehaviour);
            // builder.RegisterComponent(emotionBehaviour);
            
            base.Configure(builder);
        }
    }
}