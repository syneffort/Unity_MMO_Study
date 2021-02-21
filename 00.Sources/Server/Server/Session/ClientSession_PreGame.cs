using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
			// TODO : 보안 체크
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			LobbyPlayers.Clear();

			// TODO : 문제점 개선
			// - 동시에 다른 사람이 같은 UniqueId를 보낸다면?
			// - 악의적으로 같은 패킷을 여러번 보낸다면?
			// - 뜬금없는 타이밍에 패킷을 보낸다면?
			using (AppDbContext db = new AppDbContext())
			{
				AccountDb findAccount = db.Accounts
					.Include(a => a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null)
				{
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach (PlayerDb playerDb in findAccount.Players)
                    {
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
						{
							PlayerDbId = playerDb.PlayerDbId,
							Name = playerDb.PlayerName,
							StatInfo = new StatInfo()
                            {
								Level = playerDb.Level,
								Hp = playerDb.Hp,
								MaxHp = playerDb.MaxHp,
								Attack = playerDb.Attack,
								Speed = playerDb.Speed,
								TotalExp = playerDb.TotalExp,
							}
						};

						// 메모리
						LobbyPlayers.Add(lobbyPlayer);
						// 패킷
						loginOk.Players.Add(lobbyPlayer);
                    }

					Send(loginOk);

					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
				else
				{
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					bool success = db.SaveChangesEx();
					if (success == false)
						return;

					AccountDbId = newAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);

					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if (playerInfo == null)
                return;

			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
				MyPlayer.Info.Name = playerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
				MyPlayer.Session = this;

				S_ItemList itemListPacket = new S_ItemList();
				// 아이템 목록
				using (AppDbContext db = new AppDbContext())
                {
					List<ItemDb> items = db.Items
						.Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
						.ToList();

					foreach (ItemDb itemDb in items)
                    {
						Item item = Item.MakeItem(itemDb);
						if (item != null)
                        {
							MyPlayer.Inven.Add(item);

							ItemInfo info = new ItemInfo();
							info.MergeFrom(item.Info);
							itemListPacket.Items.Add(info);
						}
					}
                }

				Send(itemListPacket);
			}

			ServerState = PlayerServerState.ServerStateGame;

			GameLogic.Instance.Push(() =>
			{
				GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.EnterGame, MyPlayer, true);
			});
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			using (AppDbContext db = new AppDbContext())
            {
				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

				if (findPlayer != null)
                {
					// 사용된 이름
					Send(new S_CreatePlayer());
                }
				else
                {
					// 초기 스텟
					StatInfo stat = null;
					DataManager.StatDict.TryGetValue(1, out stat);

					PlayerDb newPlayerDb = new PlayerDb()
					{
						PlayerName = createPacket.Name,
						Level = stat.Level,
						Hp = stat.Hp,
						MaxHp = stat.MaxHp,
						Attack = stat.Attack,
						Speed = stat.Speed,
						TotalExp = 0,
						AccountDbId = AccountDbId,
					};

					db.Players.Add(newPlayerDb);
					bool success = db.SaveChangesEx();
					if (success == false)
						return;

					// 메모리에 추가
					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
					{
						PlayerDbId = newPlayerDb.PlayerDbId,
						Name = createPacket.Name,
						StatInfo = new StatInfo()
						{
							Level = stat.Level,
							Hp = stat.Hp,
							MaxHp = stat.MaxHp,
							Attack = stat.Attack,
							Speed = stat.Speed,
							TotalExp = 0,
						}
					};

					// 메모리
					LobbyPlayers.Add(lobbyPlayer);

					S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
					newPlayer.Player.MergeFrom(lobbyPlayer);

					Send(newPlayer);
				}
            }
        }
    }
}
