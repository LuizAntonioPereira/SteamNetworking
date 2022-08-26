/*
 * Criado por SharpDevelop.
 * Usuário: Cliente
 * Data: 27/10/2020
 * Hora: 17:46
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */

namespace NetworkPacket
{
	using global::System;
	using global::FlatBuffers;
	public struct Vec2 : IFlatbufferObject
	{
		private Struct __p;
		public ByteBuffer ByteBuffer { get { return __p.bb; } }
		public void __init(int _i, ByteBuffer _bb){ __p.bb_pos = _i; __p.bb = _bb; }
		public Vec2 __assign(int _i, ByteBuffer _bb){ __init(_i, _bb); return this; }

		public float X { get { return __p.bb.GetFloat(__p.bb_pos + 0); } }
		public float Y { get { return __p.bb.GetFloat(__p.bb_pos + 4); } }

		public static Offset<Vec2> CreateVec2(FlatBufferBuilder builder, float X, float Y) {
			builder.Prep(4, 8);
			builder.PutFloat(Y);
			builder.PutFloat(X);
			return new Offset<Vec2>(builder.Offset);
		}
	};

	public struct OtherPlayer : IFlatbufferObject
	{
		private Table __p;
		public ByteBuffer ByteBuffer { get { return __p.bb; } }
		public static OtherPlayer GetRootAsOtherPlayer(ByteBuffer _bb) { return GetRootAsOtherPlayer(_bb, new OtherPlayer());}
		public static OtherPlayer GetRootAsOtherPlayer(ByteBuffer _bb, OtherPlayer obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
		public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
		public OtherPlayer __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

		public int Action { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(0 + __p.bb_pos) : (int)0; } }
		public Vec2? Pos { get { int o = __p.__offset(6); return o != 0 ? (Vec2?)(new Vec2()).__assign(0 + __p.bb_pos, __p.bb) : null; } }

		public static void StartOtherPlayer(FlatBufferBuilder builder) { builder.StartObject(2); }
		public static void AddAction(FlatBufferBuilder builder, int action) { builder.AddInt(0, action, 0); }
		public static void AddPos(FlatBufferBuilder builder, Offset<Vec2> posOffset) { builder.AddStruct(1, posOffset.Value, 0); }
		public static Offset<OtherPlayer> EndOtherPlayer(FlatBufferBuilder builder) {
			int o = builder.EndObject();
			return new Offset<OtherPlayer>(o);
		}
	};

}
