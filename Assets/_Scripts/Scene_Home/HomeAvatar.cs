using System;
using _Scripts._Shared.Data;
using _Scripts.Messages;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace _Scripts.Scene_Home
{
    public class HomeAvatar : MonoBehaviour // [임시]
    {
        private SkinnedMeshRenderer _renderer;
        
        private AvatarData _avatarData;
        private PlayerStatus _playerStatus;
        
        private MaterialPropertyBlock _matPropBlock;
        
        private IDisposable _subs;
        
        [Inject]
        private void Construct(
            AvatarData avatarData,
            PlayerStatus playerStatus,
            ISubscriber<AvatarMessage> avatarSub)
        {
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _matPropBlock = new MaterialPropertyBlock();
            
            _avatarData = avatarData;
            _playerStatus = playerStatus;
            
            _subs = avatarSub.Subscribe(SetAvatar);
            
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, _playerStatus.AvatarIndex);
        }

        private void OnDestroy()
        {
            _subs?.Dispose();
        }

        private void SetAvatar(AvatarMessage msg)
        {
            _avatarData.ChangeAvatar(_renderer, _matPropBlock, msg.AvatarIndex);
            _playerStatus.SetIndex(msg);
        }
    }
}