using System;
using _Scripts._Messages.Shared;
using _Scripts.Messages;
using MessagePipe;
using UnityEngine;

namespace _Scripts._Shared.Data
{
    public class PlayerStatus : IDisposable
    {
        public string Nickname { get; private set; }
        public int AvatarIndex { get; private set; }
        
        public PlayerStatus(
            ISubscriber<RenameMessage> renameSub,
            ISubscriber<AvatarMessage> avatarSub,
            DisposableBagBuilder disposableBagBuilder)
        {
            renameSub
                .Subscribe(SetNickname)
                .AddTo(disposableBagBuilder);
            
            avatarSub
                .Subscribe(SetIndex)
                .AddTo(disposableBagBuilder);
        }

        public void SetNickname(RenameMessage msg)
        {
            Nickname = msg.Nickname;
        }

        public void SetIndex(AvatarMessage msg)
        {
            AvatarIndex = msg.AvatarIndex;
        }

        public void Dispose()
        {
        }
    }
}