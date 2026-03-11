using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Scripts.Lobby.UI
{
    public interface IView
    {
        public QuickMenuType RequiredQuickMenu { get; }
        public UniTask Activate(CancellationToken ct = default);
        public UniTask Deactivate(CancellationToken ct = default);
    }
}