using Godot;
using System;
using Steamworks;

public class joinHostGame : Node
{
	protected Callback<LobbyCreated_t> Callback_lobbyCreated;
	protected Callback<LobbyEnter_t> Callback_lobbyEntered;
	protected Callback<LobbyInvite_t> Callback_lobbyInvite;
	protected Callback<LobbyChatUpdate_t> Callback_lobbyChatUpdate;
	protected Callback<LobbyChatMsg_t> Callback_lobbyChatMessage;

	Button startHostingButton, continueGameButton, joinSelectedButton;
	Label hostSectionStatus , joinSectonStatus;
	Tree invitationTree;    
	RichTextLabel chatBox;
	LineEdit chatField;
	Global global;
	CSteamID lobbyID , joinID;

	public override void _Ready()
	{
		Callback_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		Callback_lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
		Callback_lobbyInvite = Callback<LobbyInvite_t>.Create(OnLobbyInvite);
		Callback_lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		Callback_lobbyChatMessage = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);

		startHostingButton = GetParent().GetNode("../GUI/HBoxContainer/HBoxContainer/hostSection/startHostingButton") as Button;
		hostSectionStatus = GetParent().GetNode("../GUI/HBoxContainer/HBoxContainer/hostSection/hostSectionStatus") as Label;
		continueGameButton = GetParent().GetNode("../GUI/HBoxContainer/HBoxContainer/hostSection/continueGameButton") as Button;
		invitationTree = GetParent().GetNode("../GUI/HBoxContainer/HBoxContainer/joinSection/VBoxContainer/Control/invitationTree") as Tree;
		joinSectonStatus = GetParent().GetNode("../GUI/HBoxContainer/HBoxContainer/joinSection/VBoxContainer/Label2") as Label;
		joinSelectedButton = GetParent().GetNode("../GUI/HBoxContainer/HBoxContainer/joinSection/VBoxContainer/joinSelectedButton") as Button;
		chatBox = GetParent().GetNode("../GUI/HBoxContainer/chatSection/chatBox") as RichTextLabel;
		chatField = GetParent().GetNode("../GUI/HBoxContainer/chatSection/chatField") as LineEdit;

		global = GetNode("/root/Global") as Global;

		TreeItem item = invitationTree.CreateItem();
		item.SetText(0, "FRIEND");
		item.SetText(1, "JOIN ID");
		
		continueGameButton.Connect("pressed",this,"Precionei");
	}

	// ----------------------- LOBBY CREATION -----------------------
	private void _on_startHostingButton_pressed()
	{
		if (startHostingButton.Text == "Start hosting new game")
		{
			continueGameButton.Disabled = !continueGameButton.Disabled;
			hostSectionStatus.Text = "Creating lobby...";

			// Attempt to create a new lobby
			SteamAPICall_t newLobby = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);
		}
		else if (startHostingButton.Text == "Cancel hosting") 
		{
			SteamMatchmaking.LeaveLobby(lobbyID);
			startHostingButton.Text = "Start hosting new game";
			hostSectionStatus.Text = "Status: IDLE";
			chatField.Editable = false;
		}
	}

	// When a lobby has been created (or failed to create)
	private void OnLobbyCreated(LobbyCreated_t lobby)
	{
		if (lobby.m_eResult == EResult.k_EResultOK)
		{
			global.playingAsHost = true;
			hostSectionStatus.Text = "Lobby ID: " + lobby.m_ulSteamIDLobby + "\nWaiting for player to join...";
			startHostingButton.Disabled = false;
			startHostingButton.Text = "Cancel hosting";
			lobbyID = (CSteamID)lobby.m_ulSteamIDLobby;

			// Set global value
			global.player1 = SteamUser.GetSteamID();
			
			GD.Print("até aqui tudo bem");
		}
		else
			hostSectionStatus.Text = "Failed to create lobby.\nReason: " + lobby.m_eResult;
	}

	// When a lobby has been entered
	private void OnLobbyEntered(LobbyEnter_t entrance)
	{
		global.global_lobbyID = (CSteamID)entrance.m_ulSteamIDLobby;
		chatField.Editable = true;

		if (global.playingAsHost)
		{
			lobbyID = (CSteamID)entrance.m_ulSteamIDLobby;
			GD.Print("You just entered lobby " + entrance.m_ulSteamIDLobby + " as host.");
		}
		else if (!global.playingAsHost)
		{
			GD.Print("You just entered lobby " + entrance.m_ulSteamIDLobby);

			// Set global values
			global.player1 = SteamMatchmaking.GetLobbyOwner((CSteamID)entrance.m_ulSteamIDLobby);
			global.player2 = SteamUser.GetSteamID();
		}

	}

	// ----------------------- JOINING A LOBBY -----------------------

private void Precionei(){

GetTree().ChangeScene("res://Scenes/game.tscn");

}
	private void OnLobbyInvite(LobbyInvite_t invitation)
	{
		GD.Print("youve just been invited");
		// Display invitation in tree
		TreeItem item = invitationTree.CreateItem();
		item.SetText(0, SteamFriends.GetFriendPersonaName((CSteamID)invitation.m_ulSteamIDUser));
		item.SetText(1, invitation.m_ulSteamIDLobby.ToString());
	}

	private void _on_joinSelectedButton_pressed()
	{
		if (joinSelectedButton.Text == "Join selected...")
		{
			global.playingAsHost = false;

			// Parse string from invitation tree into a CSteamID and join the lobby
			joinID = (CSteamID)ulong.Parse(invitationTree.GetSelected().GetText(1), System.Globalization.NumberStyles.None);
			SteamMatchmaking.JoinLobby(joinID);

			// Visual stuff
			joinSelectedButton.Text = "Leave lobby";
		}
		else if (joinSelectedButton.Text == "Leave lobby")
		{
			SteamMatchmaking.LeaveLobby(joinID);
			joinSelectedButton.Text = "Join selected...";
			chatField.Editable = false;
		}
	}

	private void _on_invitationTree_item_selected()
	{
		if (invitationTree.GetSelected().GetText(0) != "FRIEND")
			joinSelectedButton.Disabled = false;
		else
			joinSelectedButton.Disabled = true;
	}

	private void _on_invitationTree_nothing_selected() => joinSelectedButton.Disabled = true;

	// NOTIFY PLAYERS OF LOBBY ACTIVITY
	private void OnLobbyChatUpdate(LobbyChatUpdate_t update)
	{
		chatBox.AddText("\n" + SteamFriends.GetFriendPersonaName((CSteamID)update.m_ulSteamIDUserChanged) + " made a change in the lobby. Change: " + update.m_rgfChatMemberStateChange);

		// Set global value
		global.player2 = (CSteamID)update.m_ulSteamIDMakingChange;
	}

	// ----------------------- CHAT SECTION -----------------------

	private void _on_chatField_text_entered(String new_text)
	{
		chatField.Clear();
		byte[] message = System.Text.Encoding.UTF8.GetBytes(new_text);

		if (!SteamMatchmaking.SendLobbyChatMsg(global.global_lobbyID, message, message.Length))
			chatBox.AddText("\nMessage failed to send.");
	}

	private void OnLobbyChatMessage(LobbyChatMsg_t message)
	{
		byte[] messageData = new byte[32];
		SteamMatchmaking.GetLobbyChatEntry(global.global_lobbyID, (int)message.m_iChatID, out CSteamID user, messageData, messageData.Length, out EChatEntryType type);
		string messageString = System.Text.Encoding.UTF8.GetString(messageData);
		chatBox.AddText("\n" + SteamFriends.GetFriendPersonaName((CSteamID)message.m_ulSteamIDUser) + ": " + messageString);

		if (messageString.Contains("CONTINUE_SESSION") && (CSteamID)message.m_ulSteamIDUser == global.player1) {
			GetTree().ChangeScene("Scenes/game.tscn");
		}
	}
}
