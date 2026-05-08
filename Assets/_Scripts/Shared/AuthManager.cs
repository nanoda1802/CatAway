using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using VContainer.Unity;

namespace _Scripts.Shared
{
    public class AuthManager : IAsyncStartable, IDisposable
    {
        private const string CodePattern = "^[6789BCDFGHJKLMNPQRTWbcdfghjklmnpqrtw]{6,12}$";
        
        public string RoomCode { get; private set; }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            Debug.Log("AuthManager: StartAsync");
            try
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public void Dispose()
        {
            RoomCode = null;
            AuthenticationService.Instance.SignOut();
        }
        
        public async UniTask<RelayServerData> AllocateRelayServerAndGetJoinCode(int maxConnections, CancellationToken ct = default, string region = null)
        {
            RoomCode = string.Empty;
            
            ct.ThrowIfCancellationRequested();
            
            Allocation serverAlloc;
            
            try
            {
                serverAlloc = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay create allocation request failed : {e.Message}");
                throw;
            }

            // Debug.Log($"server: {serverAlloc.ConnectionData[0]} {serverAlloc.ConnectionData[1]}");
            // Debug.Log($"server: {serverAlloc.AllocationId}");

            ct.ThrowIfCancellationRequested();
            
            try
            {
                RoomCode = await RelayService.Instance.GetJoinCodeAsync(serverAlloc.AllocationId);
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay create join code request failed : {e.Message}");
                throw;
            }
            
            ct.ThrowIfCancellationRequested();

            var dtlsEndpoint = serverAlloc.ServerEndpoints.First(e => e.ConnectionType == "dtls");
            
            return new RelayServerData(
                dtlsEndpoint.Host,
                (ushort)dtlsEndpoint.Port,
                serverAlloc.AllocationIdBytes,
                serverAlloc.ConnectionData,
                serverAlloc.ConnectionData,
                serverAlloc.Key,
                true);
        }
        
        public async UniTask<RelayServerData> JoinRelayServerFromJoinCode(string joinCode, CancellationToken ct = default)
        {
            if (!IsValidateCode(joinCode))
            {
                throw new ArgumentException("Invalid code pattern.");
            }

            ct.ThrowIfCancellationRequested();

            JoinAllocation joinAlloc;
            
            try
            {
                joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay join request failed : {e.Message}");
                throw;
            }

            Debug.Log($"client: {joinAlloc.ConnectionData[0]} {joinAlloc.ConnectionData[1]}");
            Debug.Log($"host: {joinAlloc.HostConnectionData[0]} {joinAlloc.HostConnectionData[1]}");
            Debug.Log($"client: {joinAlloc.AllocationId}");
            
            ct.ThrowIfCancellationRequested();

            var dtlsEndpoint = joinAlloc.ServerEndpoints.First(e => e.ConnectionType == "dtls");
            
            return new RelayServerData(
                dtlsEndpoint.Host,
                (ushort)dtlsEndpoint.Port,
                joinAlloc.AllocationIdBytes,
                joinAlloc.ConnectionData,
                joinAlloc.HostConnectionData,
                joinAlloc.Key,
                true);
        }

        private bool IsValidateCode(string code)
        {
            return !string.IsNullOrEmpty(code) 
                   && Regex.IsMatch(code, CodePattern);
        }
    }
}