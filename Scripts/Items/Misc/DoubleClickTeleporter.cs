using System;
using Server;
using Server.Network;

namespace Server.Items
{
	public class DoubleClickTeleporter : Item
	{
		private bool m_Active, m_Creatures;
		private Point3D m_PointDest;
		private Map m_MapDest;
		private bool m_SourceEffect;
		private bool m_DestEffect;
		private int m_SoundID;
		private TimeSpan m_Delay;

		[CommandProperty( AccessLevel.GameMaster )]
		public bool SourceEffect
		{
			get { return m_SourceEffect; }
			set { m_SourceEffect = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool DestEffect
		{
			get { return m_DestEffect; }
			set { m_DestEffect = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int SoundID
		{
			get { return m_SoundID; }
			set { m_SoundID = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public TimeSpan Delay
		{
			get { return m_Delay; }
			set { m_Delay = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Active
		{
			get { return m_Active; }
			set { m_Active = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D PointDest
		{
			get { return m_PointDest; }
			set { m_PointDest = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Map MapDest
		{
			get { return m_MapDest; }
			set { m_MapDest = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Creatures
		{
			get { return m_Creatures; }
			set { m_Creatures = value; }
		}

		public override int LabelNumber { get { return 1026095; } } // teleporter

		[Constructable]
		public DoubleClickTeleporter()
			: this( new Point3D( 0, 0, 0 ), null, false )
		{
		}

		[Constructable]
		public DoubleClickTeleporter( Point3D pointDest, Map mapDest )
			: this( pointDest, mapDest, false )
		{
		}

		[Constructable]
		public DoubleClickTeleporter( Point3D pointDest, Map mapDest, bool creatures )
			: base( 0x1BC3 )
		{
			Movable = false;
			Visible = false;

			m_Active = true;
			m_PointDest = pointDest;
			m_MapDest = mapDest;
			m_Creatures = creatures;
		}

		public virtual void StartTeleport( Mobile m )
		{
			if( m_Delay == TimeSpan.Zero )
				DoTeleport( m );
			else
				Timer.DelayCall( m_Delay, new TimerStateCallback( DoTeleport_Callback ), m );
		}

		private void DoTeleport_Callback( object state )
		{
			DoTeleport( (Mobile)state );
		}

		public virtual void DoTeleport( Mobile m )
		{
			Map map = m_MapDest;

			if( map == null || map == Map.Internal )
				map = m.Map;

			Point3D p = m_PointDest;

			if( p == Point3D.Zero )
				p = m.Location;

			Server.Mobiles.BaseCreature.TeleportPets( m, p, map );

			bool sendEffect = (!m.Hidden || m.AccessLevel == AccessLevel.Player);

			if( m_SourceEffect && sendEffect )
				Effects.SendLocationEffect( m.Location, m.Map, 0x3728, 10, 10 );

			m.MoveToWorld( p, map );

			if( m_DestEffect && sendEffect )
				Effects.SendLocationEffect( m.Location, m.Map, 0x3728, 10, 10 );

			if( m_SoundID > 0 && sendEffect )
				Effects.PlaySound( m.Location, m.Map, m_SoundID );
		}

		public override void OnDoubleClick( Mobile m )
		{
			if( m_Active && (m.InRange( this, 3 ) || (this.RootParent != null && this.RootParent == m)) )
			{
				if( !m_Creatures && !m.Player )
					return;

				StartTeleport( m );
			}
			return;
		}

		public DoubleClickTeleporter( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)2 ); // version

			writer.Write( (bool)m_SourceEffect );
			writer.Write( (bool)m_DestEffect );
			writer.Write( (TimeSpan)m_Delay );
			writer.WriteEncodedInt( (int)m_SoundID );

			writer.Write( m_Creatures );

			writer.Write( m_Active );
			writer.Write( m_PointDest );
			writer.Write( m_MapDest );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch( version )
			{
				case 2:
					{
						m_SourceEffect = reader.ReadBool();
						m_DestEffect = reader.ReadBool();
						m_Delay = reader.ReadTimeSpan();
						m_SoundID = reader.ReadEncodedInt();

						goto case 1;
					}
				case 1:
					{
						m_Creatures = reader.ReadBool();

						goto case 0;
					}
				case 0:
					{
						m_Active = reader.ReadBool();
						m_PointDest = reader.ReadPoint3D();
						m_MapDest = reader.ReadMap();

						break;
					}
			}
		}
	}
}