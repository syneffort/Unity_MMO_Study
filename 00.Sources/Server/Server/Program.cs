using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;

namespace Server
{
	// 스레드 구조
	// 1. Recv (N개)
	// 2. GameRoomManager (1개) -> GameLogic
	// 3. DB (1개)

	class Program
	{
		static Listener _listener = new Listener();

		static void GameLogicTask()
        {
			while (true)
            {
				GameLogic.Instance.Update();
				Thread.Sleep(0);
            }
        }

		static void DbTask()
        {
			while (true)
			{
				DbTransaction.Instance.Flush();
				Thread.Sleep(0);
			}
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameLogic.Instance.Push(() => { GameLogic.Instance.Add(1); });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

            // GameLogicTask
            {
				Task gameLogicTask = new Task(GameLogicTask, TaskCreationOptions.LongRunning);
				gameLogicTask.Start();
            }

			DbTask();
		}
	}
}
