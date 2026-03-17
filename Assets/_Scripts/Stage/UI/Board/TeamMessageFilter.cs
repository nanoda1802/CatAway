using System;
using _Scripts.Stage.Data;
using MessagePipe;
using UnityEngine;

namespace _Scripts.Stage.UI.Board
{
    public class TeamMessageFilter<T> : MessageHandlerFilter<T> where T : ITeamMessage
    {
        private readonly Team _team;

        public TeamMessageFilter(Team team)
        {
            _team = team;
        }

        public override void Handle(T message, Action<T> next)
        {
            if (message.Team != _team) return;
            next(message);
        }
    }
}