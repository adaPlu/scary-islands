using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ScaryIslands.Combat;
using ScaryIslands.Pets;
using ScaryIslands.VR;

namespace ScaryIslands.Multiplayer
{
    /// <summary>
    /// Direct-IP/LAN multiplayer session for 2-4 players.
    /// Uses NGO custom messages so the existing XR rig does not need to become a network prefab.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class MultiplayerSession : MonoBehaviour
    {
        private const string PlayerSnapshotMessage = "si-player-snapshot";
        private const string PlayerLeftMessage = "si-player-left";
        private const string MonsterDamageMessage = "si-monster-damage";
        private const string MonsterStateMessage = "si-monster-state";
        private const string MonsterSnapshotMessage = "si-monster-snapshot";

        public static MultiplayerSession Instance { get; private set; }

        [Header("Connection")]
        [SerializeField] private string joinAddress = "127.0.0.1";
        [SerializeField] private ushort port = 7777;
        [SerializeField, Range(2, 8)] private int maximumPlayers = 4;

        [Header("Synchronization")]
        [SerializeField, Range(5f, 30f)] private float playerSnapshotsPerSecond = 20f;
        [SerializeField, Range(2f, 20f)] private float monsterSnapshotsPerSecond = 10f;

        private readonly Dictionary<ulong, RemotePlayerAvatar> remotePlayers = new Dictionary<ulong, RemotePlayerAvatar>();

        private NetworkManager manager;
        private UnityTransport transport;
        private ArmSwingLocomotion localRig;
        private float nextPlayerSnapshotTime;
        private float nextMonsterSnapshotTime;
        private bool handlersRegistered;
        private string portText = "7777";

        public bool IsActive => manager != null && manager.IsListening;
        public bool IsServer => IsActive && manager.IsServer;
        public bool IsClient => IsActive && manager.IsClient;
        public string JoinAddress => joinAddress;
        public ushort Port => port;
        public int ConnectedPlayerCount => manager == null || !manager.IsListening
            ? 1
            : manager.IsServer ? manager.ConnectedClientsIds.Count : 2;
        public string Status
        {
            get
            {
                if (manager == null || !manager.IsListening) return "OFFLINE";
                if (manager.IsHost) return "HOST — " + ConnectedPlayerCount + "/" + maximumPlayers;
                if (manager.IsServer) return "SERVER — " + ConnectedPlayerCount + "/" + maximumPlayers;
                return manager.IsConnectedClient ? "CLIENT — CONNECTED" : "CLIENT — CONNECTING";
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            new GameObject("Scary Islands Multiplayer").AddComponent<MultiplayerSession>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            portText = port.ToString();
            SetupNetworkManager();
        }

        private void Update()
        {
            if (!IsActive) return;

            FindLocalRig();

            if (localRig != null && Time.unscaledTime >= nextPlayerSnapshotTime)
            {
                nextPlayerSnapshotTime = Time.unscaledTime + 1f / playerSnapshotsPerSecond;
                SendLocalPlayerSnapshot();
            }

            if (manager.IsServer && Time.unscaledTime >= nextMonsterSnapshotTime)
            {
                nextMonsterSnapshotTime = Time.unscaledTime + 1f / monsterSnapshotsPerSecond;
                BroadcastMonsterSnapshot();
            }
        }

        public void SetJoinAddress(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
                joinAddress = address.Trim();
        }

        public bool Host()
        {
            if (IsActive) return false;
            SetupNetworkManager();
            manager.NetworkConfig.ConnectionApproval = true;
            manager.ConnectionApprovalCallback = ApproveConnection;
            transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");

            bool started = manager.StartHost();
            if (started)
                RegisterHandlers();
            return started;
        }

        public bool Join()
        {
            if (IsActive) return false;
            SetupNetworkManager();
            transport.SetConnectionData(joinAddress, port);

            bool started = manager.StartClient();
            if (started)
                RegisterHandlers();
            return started;
        }

        public bool JoinLocalhost()
        {
            SetJoinAddress("127.0.0.1");
            return Join();
        }

        public void Disconnect()
        {
            UnregisterHandlers();

            if (manager != null && manager.IsListening)
                manager.Shutdown();

            ClearRemotePlayers();
        }

        public bool RequestMonsterDamage(MonsterHealth monster, float damage)
        {
            if (monster == null || damage <= 0f) return false;
            if (!IsActive) return false;

            if (manager.IsServer)
            {
                bool hit = monster.ApplyAuthoritativeDamage(damage);
                if (hit) BroadcastMonsterState(monster);
                return hit;
            }

            using (FastBufferWriter writer = new FastBufferWriter(32, Allocator.Temp))
            {
                writer.WriteValueSafe(monster.NetworkId);
                writer.WriteValueSafe(Mathf.Clamp(damage, 0f, 25f));
                manager.CustomMessagingManager.SendNamedMessage(
                    MonsterDamageMessage,
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableSequenced);
            }

            return monster.IsAlive;
        }

        private void SetupNetworkManager()
        {
            if (manager != null) return;

            NetworkManager existing = NetworkManager.Singleton;
            if (existing != null)
            {
                manager = existing;
                transport = manager.GetComponent<UnityTransport>();
                if (transport == null)
                    transport = manager.gameObject.AddComponent<UnityTransport>();
            }
            else
            {
                GameObject networkObject = new GameObject("Scary Islands Network Manager");
                DontDestroyOnLoad(networkObject);
                transport = networkObject.AddComponent<UnityTransport>();
                manager = networkObject.AddComponent<NetworkManager>();
            }

            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.PlayerPrefab = null;
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.ConnectionApproval = true;
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            int connected = manager.ConnectedClientsIds.Count;
            response.Approved = connected < maximumPlayers;
            response.CreatePlayerObject = false;
            response.PlayerPrefabHash = null;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
            response.Reason = response.Approved ? string.Empty : "Room is full.";
            response.Pending = false;
        }

        private void RegisterHandlers()
        {
            if (handlersRegistered || manager == null || !manager.IsListening) return;

            manager.CustomMessagingManager.RegisterNamedMessageHandler(PlayerSnapshotMessage, ReceivePlayerSnapshot);
            manager.CustomMessagingManager.RegisterNamedMessageHandler(PlayerLeftMessage, ReceivePlayerLeft);
            manager.CustomMessagingManager.RegisterNamedMessageHandler(MonsterDamageMessage, ReceiveMonsterDamage);
            manager.CustomMessagingManager.RegisterNamedMessageHandler(MonsterStateMessage, ReceiveMonsterState);
            manager.CustomMessagingManager.RegisterNamedMessageHandler(MonsterSnapshotMessage, ReceiveMonsterSnapshot);
            handlersRegistered = true;
        }

        private void UnregisterHandlers()
        {
            if (!handlersRegistered || manager == null) return;

            manager.CustomMessagingManager.UnregisterNamedMessageHandler(PlayerSnapshotMessage);
            manager.CustomMessagingManager.UnregisterNamedMessageHandler(PlayerLeftMessage);
            manager.CustomMessagingManager.UnregisterNamedMessageHandler(MonsterDamageMessage);
            manager.CustomMessagingManager.UnregisterNamedMessageHandler(MonsterStateMessage);
            manager.CustomMessagingManager.UnregisterNamedMessageHandler(MonsterSnapshotMessage);
            handlersRegistered = false;
        }

        private void OnClientConnected(ulong clientId)
        {
            FindLocalRig();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            RemoveRemotePlayer(clientId);

            if (manager != null && manager.IsServer)
            {
                using (FastBufferWriter writer = new FastBufferWriter(16, Allocator.Temp))
                {
                    writer.WriteValueSafe(clientId);
                    SendToRemoteClients(PlayerLeftMessage, writer, NetworkDelivery.ReliableSequenced);
                }
            }

            if (manager != null && !manager.IsServer)
                ClearRemotePlayers();
        }

        private void FindLocalRig()
        {
            if (localRig == null)
                localRig = FindFirstObjectByType<ArmSwingLocomotion>();
        }

        private void SendLocalPlayerSnapshot()
        {
            if (localRig == null || localRig.Head == null || localRig.LeftHand == null || localRig.RightHand == null)
                return;

            ulong playerId = manager.LocalClientId;
            Vector3 rootPosition = localRig.transform.position;
            Quaternion rootRotation = localRig.transform.rotation;
            byte petIndex = GetEquippedPetIndex();

            if (manager.IsServer)
            {
                using (FastBufferWriter writer = BuildPlayerSnapshotWriter(
                    playerId,
                    rootPosition,
                    rootRotation,
                    localRig.Head,
                    localRig.LeftHand,
                    localRig.RightHand,
                    petIndex))
                {
                    SendToRemoteClients(PlayerSnapshotMessage, writer, NetworkDelivery.UnreliableSequenced);
                }
            }
            else
            {
                using (FastBufferWriter writer = BuildPlayerSnapshotWriter(
                    playerId,
                    rootPosition,
                    rootRotation,
                    localRig.Head,
                    localRig.LeftHand,
                    localRig.RightHand,
                    petIndex))
                {
                    manager.CustomMessagingManager.SendNamedMessage(
                        PlayerSnapshotMessage,
                        NetworkManager.ServerClientId,
                        writer,
                        NetworkDelivery.UnreliableSequenced);
                }
            }
        }

        private static FastBufferWriter BuildPlayerSnapshotWriter(
            ulong playerId,
            Vector3 rootPosition,
            Quaternion rootRotation,
            Transform head,
            Transform leftHand,
            Transform rightHand,
            byte petIndex)
        {
            FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteValueSafe(playerId);
            writer.WriteValueSafe(rootPosition);
            writer.WriteValueSafe(rootRotation);
            writer.WriteValueSafe(head.position);
            writer.WriteValueSafe(head.rotation);
            writer.WriteValueSafe(leftHand.position);
            writer.WriteValueSafe(leftHand.rotation);
            writer.WriteValueSafe(rightHand.position);
            writer.WriteValueSafe(rightHand.rotation);
            writer.WriteValueSafe(petIndex);
            return writer;
        }

        private void ReceivePlayerSnapshot(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong playerId);
            reader.ReadValueSafe(out Vector3 rootPosition);
            reader.ReadValueSafe(out Quaternion rootRotation);
            reader.ReadValueSafe(out Vector3 headPosition);
            reader.ReadValueSafe(out Quaternion headRotation);
            reader.ReadValueSafe(out Vector3 leftPosition);
            reader.ReadValueSafe(out Quaternion leftRotation);
            reader.ReadValueSafe(out Vector3 rightPosition);
            reader.ReadValueSafe(out Quaternion rightRotation);
            reader.ReadValueSafe(out byte petIndex);

            if (manager.IsServer && senderId != NetworkManager.ServerClientId)
                playerId = senderId;

            if (playerId != manager.LocalClientId)
            {
                GetOrCreateRemotePlayer(playerId).ApplySnapshot(
                    rootPosition,
                    rootRotation,
                    headPosition,
                    headRotation,
                    leftPosition,
                    leftRotation,
                    rightPosition,
                    rightRotation,
                    petIndex);
            }

            if (manager.IsServer)
            {
                using (FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp))
                {
                    writer.WriteValueSafe(playerId);
                    writer.WriteValueSafe(rootPosition);
                    writer.WriteValueSafe(rootRotation);
                    writer.WriteValueSafe(headPosition);
                    writer.WriteValueSafe(headRotation);
                    writer.WriteValueSafe(leftPosition);
                    writer.WriteValueSafe(leftRotation);
                    writer.WriteValueSafe(rightPosition);
                    writer.WriteValueSafe(rightRotation);
                    writer.WriteValueSafe(petIndex);
                    SendToRemoteClients(PlayerSnapshotMessage, writer, NetworkDelivery.UnreliableSequenced);
                }
            }
        }

        private void ReceivePlayerLeft(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ulong clientId);
            RemoveRemotePlayer(clientId);
        }

        private RemotePlayerAvatar GetOrCreateRemotePlayer(ulong clientId)
        {
            if (remotePlayers.TryGetValue(clientId, out RemotePlayerAvatar avatar) && avatar != null)
                return avatar;

            GameObject remote = new GameObject("Remote Player " + clientId);
            avatar = remote.AddComponent<RemotePlayerAvatar>();
            avatar.Initialize(clientId);
            remotePlayers[clientId] = avatar;
            return avatar;
        }

        private void RemoveRemotePlayer(ulong clientId)
        {
            if (!remotePlayers.TryGetValue(clientId, out RemotePlayerAvatar avatar))
                return;

            if (avatar != null) Destroy(avatar.gameObject);
            remotePlayers.Remove(clientId);
        }

        private void ClearRemotePlayers()
        {
            foreach (RemotePlayerAvatar avatar in remotePlayers.Values)
                if (avatar != null) Destroy(avatar.gameObject);
            remotePlayers.Clear();
        }

        private byte GetEquippedPetIndex()
        {
            if (PetShop.Instance == null) return 0;
            switch (PetShop.Instance.EquippedPetId)
            {
                case "fog-moth": return 1;
                case "lantern-crab": return 2;
                case "grave-crow": return 3;
                case "mire-slime": return 4;
                case "storm-bat": return 5;
                case "little-leviathan": return 6;
                default: return 0;
            }
        }

        private void ReceiveMonsterDamage(ulong senderId, FastBufferReader reader)
        {
            if (!manager.IsServer) return;

            reader.ReadValueSafe(out int monsterId);
            reader.ReadValueSafe(out float requestedDamage);
            MonsterHealth monster = FindMonster(monsterId);
            if (monster == null || !monster.IsAlive) return;

            float damage = Mathf.Clamp(requestedDamage, 0f, 25f);
            if (monster.ApplyAuthoritativeDamage(damage))
                BroadcastMonsterState(monster);
        }

        private void BroadcastMonsterState(MonsterHealth monster)
        {
            using (FastBufferWriter writer = new FastBufferWriter(32, Allocator.Temp))
            {
                writer.WriteValueSafe(monster.NetworkId);
                writer.WriteValueSafe(monster.CurrentHealth);
                SendToRemoteClients(MonsterStateMessage, writer, NetworkDelivery.ReliableSequenced);
            }
        }

        private void ReceiveMonsterState(ulong senderId, FastBufferReader reader)
        {
            if (manager.IsServer) return;

            reader.ReadValueSafe(out int monsterId);
            reader.ReadValueSafe(out float health);
            MonsterHealth monster = FindMonster(monsterId);
            if (monster != null)
                monster.ApplyNetworkState(health);
        }

        private void BroadcastMonsterSnapshot()
        {
            MonsterHealth[] monsters = FindObjectsByType<MonsterHealth>(FindObjectsSortMode.None);
            using (FastBufferWriter writer = new FastBufferWriter(Mathf.Max(64, 8 + monsters.Length * 40), Allocator.Temp))
            {
                writer.WriteValueSafe(monsters.Length);
                foreach (MonsterHealth monster in monsters)
                {
                    writer.WriteValueSafe(monster.NetworkId);
                    writer.WriteValueSafe(monster.transform.position);
                    writer.WriteValueSafe(monster.transform.rotation);
                    writer.WriteValueSafe(monster.CurrentHealth);
                }

                SendToRemoteClients(MonsterSnapshotMessage, writer, NetworkDelivery.UnreliableSequenced);
            }
        }

        private void ReceiveMonsterSnapshot(ulong senderId, FastBufferReader reader)
        {
            if (manager.IsServer) return;

            reader.ReadValueSafe(out int count);
            count = Mathf.Clamp(count, 0, 256);
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out int monsterId);
                reader.ReadValueSafe(out Vector3 position);
                reader.ReadValueSafe(out Quaternion rotation);
                reader.ReadValueSafe(out float health);

                MonsterHealth monster = FindMonster(monsterId);
                if (monster == null) continue;

                monster.transform.SetPositionAndRotation(position, rotation);
                monster.ApplyNetworkState(health);
            }
        }

        private static MonsterHealth FindMonster(int networkId)
        {
            MonsterHealth[] monsters = FindObjectsByType<MonsterHealth>(FindObjectsSortMode.None);
            foreach (MonsterHealth monster in monsters)
                if (monster.NetworkId == networkId) return monster;
            return null;
        }

        private void SendToRemoteClients(string messageName, FastBufferWriter writer, NetworkDelivery delivery)
        {
            if (manager == null || !manager.IsServer) return;

            foreach (ulong clientId in manager.ConnectedClientsIds)
            {
                if (clientId == NetworkManager.ServerClientId) continue;
                manager.CustomMessagingManager.SendNamedMessage(messageName, clientId, writer, delivery);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(18, 18, 330, IsActive ? 155 : 205), GUI.skin.box);
            GUILayout.Label("SCARY ISLANDS MULTIPLAYER");
            GUILayout.Label(Status);

            if (!IsActive)
            {
                GUILayout.Label("Host IP / LAN address");
                joinAddress = GUILayout.TextField(joinAddress, 64);

                GUILayout.Label("UDP port");
                portText = GUILayout.TextField(portText, 5);
                if (ushort.TryParse(portText, out ushort parsedPort))
                    port = parsedPort;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("HOST")) Host();
                if (GUILayout.Button("JOIN")) Join();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Port: " + port);
                if (GUILayout.Button("DISCONNECT")) Disconnect();
            }

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnClientConnectedCallback -= OnClientConnected;
                manager.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            UnregisterHandlers();
            ClearRemotePlayers();
            if (Instance == this) Instance = null;
        }
    }
}
