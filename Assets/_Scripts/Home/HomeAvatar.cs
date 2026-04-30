using _Scripts.Shared._Data;
using _Scripts.Shared._Messages;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace _Scripts.Home
{
    public class HomeAvatar : MonoBehaviour
    {
        private SkinnedMeshRenderer _renderer;
        
        private AvatarData _avatarData;
        private PlayerStatus _playerStatus;
        
        private MaterialPropertyBlock _matPropBlock;
        
        [Inject]
        private void Construct(
            AvatarData avatarData,
            PlayerStatus playerStatus,
            ISubscriber<AvatarMessage> avatarSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _matPropBlock = new MaterialPropertyBlock();
            
            _avatarData = avatarData;
            _playerStatus = playerStatus;
            
            avatarSub
                .Subscribe(SetAvatar)
                .AddTo(disposableBagBuilder);
            
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, _playerStatus.AvatarIndex);
        }

        private void SetAvatar(AvatarMessage msg)
        {
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, msg.AvatarIndex);
            _playerStatus.SetIndex(msg);
        }
    }
}