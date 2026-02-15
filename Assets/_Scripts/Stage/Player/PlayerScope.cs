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
        
        [SF] private CharacterController characterController;
        [SF] private Animator animator;
        [SF] private Transform detectPoint;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<PlayerInput>(Lifetime.Scoped);
            
            builder.Register<MoveStatus>(Lifetime.Scoped);
            builder.Register<DetectStatus>(Lifetime.Scoped);
            builder.Register<CarryStatus>(Lifetime.Scoped);
            builder.Register<InteractStatus>(Lifetime.Scoped);

            builder.RegisterInstance(playerData);

            builder.RegisterComponent(characterController);
            builder.RegisterComponent(animator);
            builder.RegisterComponent(detectPoint);
            
            base.Configure(builder);
        }
    }
}