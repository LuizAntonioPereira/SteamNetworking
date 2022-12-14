using Godot;
using System;
using Steamworks;
using FlatBuffers;

public class packetReceiver : Node
{
	protected Callback<P2PSessionRequest_t> Callback_P2PSessionRequest;
	protected Callback<P2PSessionConnectFail_t> Callback_P2PSessionConnectFailed;

	Node2D player1 , player2;
	Global global;

	public override void _Ready()
	{
		Callback_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
		Callback_P2PSessionConnectFailed = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectionFailed);

		player1 = GetNode("/root/game/player1") as Node2D;
		player2 = GetNode("/root/game/player2") as Node2D;

		var global = (Global)GetNode("/root/Global");
	}

	// ----------- ACCEPT OR REJECT INCOMING CONNECTION -----------
	public void OnP2PSessionRequest(P2PSessionRequest_t request)
	{
		if (request.m_steamIDRemote == global.player1 || request.m_steamIDRemote == global.player2)
		{
			SteamNetworking.AcceptP2PSessionWithUser(request.m_steamIDRemote);
			GD.Print("You have accepted incoming connection from " + SteamFriends.GetFriendPersonaName(request.m_steamIDRemote));
		}
		else
			GD.Print("A connection was just rejected from " + request.m_steamIDRemote + ".");
	}

	public void OnP2PSessionConnectionFailed(P2PSessionConnectFail_t failure) => GD.Print("P2P session failed. Error code: " + failure.m_eP2PSessionError);

	// ----------- RECEIVE PACKETS -----------
	public override void _Process(float delta)
	{
		while (SteamNetworking.IsP2PPacketAvailable(out uint packetSize))
		{
			byte[] incomingPacket = new byte[packetSize];

			// Receive data
			if (SteamNetworking.ReadP2PPacket(incomingPacket, packetSize, out uint bytesRead, out CSteamID remoteID))
			{
				ByteBuffer buff = new ByteBuffer(incomingPacket);
				var otherPlayer = NetworkPacket.OtherPlayer.GetRootAsOtherPlayer(buff);

				switch (otherPlayer.Action)
				{
					case 1:
						moveOtherPlayer(otherPlayer);
						break;
					case 2:
						moveOtherPlayer(otherPlayer);
						break;
				}
			}
		}
	}

	private void moveOtherPlayer(NetworkPacket.OtherPlayer otherPlayer)
	{
		NetworkPacket.Vec2 pos = otherPlayer.Pos.Value;

		if (global.playingAsHost)
			player2.Transform = new Transform2D(0, new Vector2(pos.X, pos.Y));
		else
			player1.Transform = new Transform2D(0, new Vector2(pos.X, pos.Y));
	}
}

