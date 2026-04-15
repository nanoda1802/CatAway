using System;
using _Scripts.Shared._Messages;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace _Scripts.Shared._Data
{
    public class PlayerStatus : IInitializable, IDisposable
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

        public void Initialize()
        {
            Nickname = PlayerPrefs.GetString("nickname", string.Empty);
            AvatarIndex = PlayerPrefs.GetInt("avatar", 0);
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
            PlayerPrefs.SetInt("avatar", AvatarIndex);
            PlayerPrefs.SetString("nickname", Nickname);
        }
    }
}