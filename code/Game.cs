using Sandbox;
using System.Linq;
using System.Threading.Tasks;

partial class FpsBase : GameManager
{
	public FpsBase()
	{
		if ( Game.IsServer )
		{
			_ = new Hud();
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );
		var player = new FpsPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
