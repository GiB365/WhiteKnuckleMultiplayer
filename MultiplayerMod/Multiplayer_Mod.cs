using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

public class Multiplayer_Mod : MonoBehaviour {
    public static Multiplayer_Mod instance; 

    EventBasedNetListener serverListener;
    EventBasedNetListener clientListener;
    public NetManager client;
    NetManager server;
    NetPeer serverPeer;

    int maxPlayerCount;
    Dictionary<int, GameObject> players;

    int nextPlayerId = 0;
    public int seed;

    enum PacketTypes {
        TransformUpdate = 0,
        ConnectedToServer = 1,
        SeedUpdate = 2,
        CreatePlayer = 3
    }

    private void Awake() {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        serverListener = new EventBasedNetListener();
        server = new NetManager(serverListener);
        clientListener = new EventBasedNetListener();
        client = new NetManager(clientListener);

        maxPlayerCount = 4;
        players = new Dictionary<int, GameObject>();
        seed = WorldLoader.instance.seed;

        CreateCommands();
    }

    public void CreateCommands() {
        CommandConsole.AddCommand("host", Host);
        CommandConsole.AddCommand("join", Join);
        CommandConsole.AddCommand("leave", Leave);
    }

    public void Leave(string[] args) {
        if (server.IsRunning) {
            server.DisconnectAll();
        }

        client.DisconnectAll();

        foreach (KeyValuePair<int, GameObject> player in players) {
            Destroy(player.Value);
        }

        players.Clear();
    }

    public void Host(string[] args) {
        if (args.Length < 1) {
            CommandConsole.LogError("Usage: host [port]\nEx: host 7777");
            return;
        }

        ushort port = ushort.Parse(args[0]);

        if (args.Length >= 2) {
            maxPlayerCount = int.Parse(args[1]);
        }

        server.Start(port);

        serverListener.ConnectionRequestEvent += request => {
            if (server.ConnectedPeersCount < maxPlayerCount) {
                request.Accept();
            }
            else {
                request.Reject();
            }
        };

        serverListener.PeerConnectedEvent += peer => {
            peer.Tag = nextPlayerId;
            nextPlayerId++;

            CommandConsole.Log("We got connection: " + peer.Tag.ToString());

            NetDataWriter writer = new NetDataWriter();

            writer.Put((int)PacketTypes.ConnectedToServer);
            writer.Put(server.ConnectedPeersCount-1);

            foreach (NetPeer connectedPeer in server.ConnectedPeerList) {
                if (connectedPeer.Tag == peer.Tag) {
                    continue;
                }

                writer.Put((int)connectedPeer.Tag);
            }

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            writer.Reset();

            writer.Put((int)PacketTypes.SeedUpdate);
            writer.Put(WorldLoader.instance.seed);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            writer.Reset();

            writer.Put((int)PacketTypes.CreatePlayer);
            writer.Put((int)peer.Tag);
            server.SendToAll(writer, DeliveryMethod.ReliableOrdered, peer);
        };

        serverListener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            int packetType = dataReader.GetInt();

            switch (packetType) {
                case (int)PacketTypes.TransformUpdate:
                    NetDataWriter writer = new NetDataWriter();

                    writer.Put((int)PacketTypes.TransformUpdate);
                    writer.Put((int)fromPeer.Tag);
                    writer.Put(dataReader.GetRemainingBytes());

                    server.SendToAll(writer, DeliveryMethod.ReliableOrdered, fromPeer);

                    break;
            }

            dataReader.Recycle();
        };

        Join(["127.0.0.1", port.ToString()]);

        CommandConsole.Log("Hosting lobby...");
        CommandConsole.LogError("You are a hosting a peer-to-peer lobby\nBy sharing your IP you are also sharing your address\nBe careful... :)");
    }

    public void Join(string[] args) {
        if (args.Length < 2) {
            CommandConsole.LogError("Usage: join [ip] [port]\nEx: join 127.0.0.1 7777");
            return;
        }

        string ip = args[0];
        int port = int.Parse(args[1]);

        client.Start();
        serverPeer = client.Connect(ip, port, "");
        
        clientListener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            int packetType = dataReader.GetInt();

            switch (packetType) {
                case (int)PacketTypes.TransformUpdate:
                    int playerId = dataReader.GetInt();

                    Vector3 new_position = new Vector3(dataReader.GetFloat(), dataReader.GetFloat(), dataReader.GetFloat());
                    Vector3 new_rotation = new Vector3(dataReader.GetFloat(), dataReader.GetFloat(), dataReader.GetFloat());

                    if (!players.ContainsKey(playerId)) {
                        break;
                    }

                    Multiplayer_Player player = players[playerId].GetComponent<Multiplayer_Player>();

                    player.UpdatePosition(new_position);
                    player.UpdateRotation(new_rotation);

                    break;
                case (int)PacketTypes.ConnectedToServer:
                    int peerCount = dataReader.GetInt();

                    CommandConsole.Log("Connected!\nCreating " + peerCount.ToString() + " player instance(s).");

                    for (int i = 0; i < peerCount; i++) {
                        CreatePlayer(dataReader.GetInt());
                    }

                    break;
                case (int)PacketTypes.SeedUpdate:
                    seed = dataReader.GetInt();

                    WorldLoader.ReloadWithSeed([seed.ToString()]);

                    break;
                case (int)PacketTypes.CreatePlayer:
                    int Tag = dataReader.GetInt();

                    CreatePlayer(Tag);

                    break;
            }

            dataReader.Recycle();
        };
        
        CommandConsole.Log("Trying to join ip: " + ip + "...");
    }

    private void CreatePlayer(int Tag) {
        CommandConsole.Log("Creating Player with Tag: " + Tag);

        GameObject player = new GameObject();
        player.AddComponent<Multiplayer_Player>();

        Material eyeMaterial = new Material(Shader.Find("Unlit/Color"));
        eyeMaterial.color = Color.black;
        Material bodyMaterial = new Material(Shader.Find("Unlit/Color"));
        bodyMaterial.color = Color.grey;

        GameObject graphics = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        graphics.transform.parent = player.transform;
        Destroy(graphics.GetComponent<CapsuleCollider>());
        graphics.GetComponent<Renderer>().material = bodyMaterial;

        GameObject eyeLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeLeft.transform.parent = graphics.transform;
        Destroy(eyeLeft.GetComponent<SphereCollider>());
        eyeLeft.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        eyeLeft.transform.localPosition = new Vector3(0.25f, 0.7f, 0.25f);
        eyeLeft.GetComponent<Renderer>().material = eyeMaterial;

        GameObject eyeRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeRight.transform.parent = graphics.transform;
        Destroy(eyeRight.GetComponent<SphereCollider>());
        eyeRight.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        eyeRight.transform.localPosition = new Vector3(-0.25f, 0.7f, 0.25f);
        eyeRight.GetComponent<Renderer>().material = eyeMaterial;

        players.Add((int)Tag, player);

        DontDestroyOnLoad(player);
    }

    private void UpdateTransform() {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((int)PacketTypes.TransformUpdate);

        Vector3 playerPosition = CL_Player.GetPlayer().transform.position;
        Vector3 playerRotation = CL_Player.GetPlayer().transform.eulerAngles;

        writer.Put(playerPosition.x);
        writer.Put(playerPosition.y);
        writer.Put(playerPosition.z);
        writer.Put(playerRotation.x);
        writer.Put(playerRotation.y);
        writer.Put(playerRotation.z);

        serverPeer.Send(writer, DeliveryMethod.ReliableOrdered); 
    }

    private void Update() {
        client.PollEvents();

        if (server.IsRunning) {
            server.PollEvents();
        }

        if (serverPeer != null) {
            UpdateTransform();
        }

    }
}
