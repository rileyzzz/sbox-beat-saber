using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace LightsaberGame
{
	public partial class LightsaberGame : Game
	{
		public List<DuelPlayer> Players = new();

		public LightsaberGame()
		{
			if (IsServer)
			{
				//new MinimalHudEntity();
			}

			Global.PhysicsSubSteps = 8;
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

		}

		public override void ClientJoined(Client client)
		{
			base.ClientJoined(client);

			var player = new DuelPlayer();
			client.Pawn = player;

			player.Respawn();

			Players.Add(player);
		}

		public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
		{
			base.ClientDisconnect(cl, reason);

		}

		public override void Simulate(Client cl)
		{
			base.Simulate(cl);
		}

		[ServerCmd]
		public static void extend_both()
		{
			if ( Game.Current is not LightsaberGame game )
				return;

			if ( !game.IsServer )
				return;

			Log.Info( "extending both" );

			foreach ( var player in game.Players)
			{

				player.LeftHand.Saber.Extend = true;
				player.RightHand.Saber.Extend = true;
			}

		}
	}
}
