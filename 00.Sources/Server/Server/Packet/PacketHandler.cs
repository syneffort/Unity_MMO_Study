using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"C_MOVE ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY})");

		if (clientSession.MyPlayer == null)
			return;

		if (clientSession.MyPlayer.Room == null)
			return;

		// TODO : 정보 검증

		// 서버에서 좌표 이동
		PlayerInfo info = clientSession.MyPlayer.Info;
		info.PosInfo = movePacket.PosInfo;

		// 다른 플레이어에게 브로드캐스팅
		S_Move resMovePacket = new S_Move();
		resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
		resMovePacket.PosInfo = movePacket.PosInfo;

		clientSession.MyPlayer.Room.Broadcast(resMovePacket);

	}
}
