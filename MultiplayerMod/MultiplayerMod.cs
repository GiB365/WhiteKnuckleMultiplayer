using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

public class Multiplayer_Mod : MonoBehaviour {

    EventBasedNetListener listener;
    NetManager manager;

    int maxPlayerCount;
    Dictionary<int, Multiplayer_Player> players;

    enum PacketTypes {
        PositionUpdate,
        ConnectedToServer
    }

    public void Start() {
        listener = new EventBasedNetListener();
        manager = new NetManager(listener);
        maxPlayerCount = 4;
        players = new Dictionary<int, Multiplayer_Player>();

        CreateCommands();
    }

    private void CreateCommands() {
        CommandConsole.AddCommand("host", Host);
        CommandConsole.AddCommand("join", Join);
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

        manager.Start(port);

        listener.ConnectionRequestEvent += request => {
            if (manager.ConnectedPeersCount < maxPlayerCount) {
                NetPeer peer = request.Accept();
                CreatePlayer(peer.RemoteId);
            }
            else {
                request.Reject();
            }

        };

        listener.PeerConnectedEvent += peer => {
            CommandConsole.Log("We got connection: " + peer.RemoteId.ToString());

            NetDataWriter writer = new NetDataWriter();
            writer.Put(PacketTypes.ConnectedToServer);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

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

        manager.Start();
        manager.Connect(ip, port, "");
        
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            int packetType = dataReader.GetInt();

            switch (dataType) {
                case PacketTypes.PositionUpdate:
                    int newX = dataReader.GetInt();
                    int newY = dataReader.GetInt();
                    int newZ = dataReader.GetInt();

                    players[fromPeer.RemoteId].UpdatePosition(newX, newY, newZ);
                    break;
                case PacketTypes.ConnectedToServer:
                    CommandConsole.Log("Connected!\nCreating player instance(s).");
                    CreatePlayerInstances();
                    break;
            }

            CommandConsole.Log("Recieved data: " + data);
            dataReader.Recycle();
        };
        
        CommandConsole.Log("Trying to join ip: " + ip + "...");
    }

    public void CreatePlayerInstances() {
        foreach (NetPeer peer in manager.ConnectedPeerList) {
            CreatePlayer(peer.RemoteId);
        }
    }

    public void CreatePlayer(int id) {
        CommandConsole.Log("Creating Player with id: " + id.ToString());

        GameObject player = new GameObject();
        player.AddComponent<Multiplayer_Player>();

        GameObject graphics = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        graphics.transform.parent = player.transform;

        players[id] = player;
    }

    public void UpdateTransform() {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(PacketTypes.PositionUpdate);

        CL_Player playerPosition = CL_Player.GetPlayer().transform.position;

        writer.Put(playerPosition.x);
        writer.Put(playerPosition.y);
        writer.Put(playerPosition.z);

        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void FixedUpdate() {
        manager.PollEvents();

        UpdateTransform();
    }
}
