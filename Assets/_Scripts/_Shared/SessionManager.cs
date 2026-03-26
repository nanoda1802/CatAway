using System;
using System.Linq;
using _Scripts.Scene_Room.Data;
using Unity.Netcode;
using VContainer.Unity;

namespace _Scripts._Shared
{
    public class SessionManager : IInitializable, IDisposable
    {
        private readonly NetworkManager _netManager;
        private readonly RoomStatus _roomStatus;

        public SessionManager(
            NetworkManager netManager,
            RoomStatus roomStatus)
        {
            _netManager = netManager;
            _roomStatus = roomStatus;
        }

        public void Initialize()
        {
            _netManager.NetworkConfig.ConnectionApproval = true;
            _netManager.OnServerStarted += OnHostStarted;
            _netManager.OnPreShutdown += OnPreShutdown;
        }

        public void Dispose()
        {
            _netManager.OnServerStarted -= OnHostStarted;
            _netManager.OnPreShutdown -= OnPreShutdown;
        }
        
        private void OnHostStarted()
        {
            _netManager.ConnectionApprovalCallback = ApprovalCheck;
        }

        private void OnPreShutdown()
        {
            _netManager.ConnectionApprovalCallback = null;
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest req,
            NetworkManager.ConnectionApprovalResponse res)
        {
            res.Approved = false;
            res.Pending = false; // 추가 검증 없으니 대기 방지
            
            if (_netManager.ConnectedClients.Count >= 4) // [임시] 테스트용! 잊지말고 수정해주기
            {
                res.Reason = "Max Clients connected.";
                return;
            }
            
            if (_roomStatus.IsFull)
            {
                res.Reason = "Requested room is already full.";
                return;
            }

            if (_netManager.ConnectedClientsIds.Contains(req.ClientNetworkId))
            {
                res.Reason = "Duplicated client ID..?";
                return;
            }

            if (false) // [추가] 스테이지 시작된 룸인 경우
            {
                res.Reason = "Requested room is already started";
                return;
            }

            res.Approved = true; // 접속 승인
            res.CreatePlayerObject = false; // 자동 생성 방지
        }
    }
}