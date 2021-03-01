using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;

namespace Server
{
	// 스레드 구조
	// 1. Recv (N개)
	// 2. GameRoomManager (1개) -> GameLogic
	// 3. Send (1개)
	// 4. DB (1개)

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

		static void NetworkTask()
        {
			while (true)
            {
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
				foreach (ClientSession session in sessions)
                {
					session.FlushSend();
                }

				Thread.Sleep(0);
            }
        }

		static void StartServerInfoTask()
        {
            System.Timers.Timer t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new ElapsedEventHandler((s, e) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
                {
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if (serverDb != null)
                    {
						serverDb.IpAddress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
					}
					else
                    {
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							IpAddress = Program.IpAddress,
							Port = Program.Port,
							BusyScore = SessionManager.Instance.GetBusyScore()
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
                    }
                }
			});
			t.Interval = 10 * 1000;
			t.Start();
        }

		public static string Name { get; } = "테스트 서버 1";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		static void Main(string[] args)
		{
			using (SharedDbContext shared = new SharedDbContext())
            {

            }

			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameLogic.Instance.Push(() => { GameLogic.Instance.Add(1); });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			StartServerInfoTask();

            // DB Task
            {
				Thread t = new Thread(DbTask);
				t.Name = "DB";
				t.Start();
            }

			// Network Task
			{
				Thread t = new Thread(NetworkTask);
				t.Name = "Network send";
				t.Start();
			}

			// GameLogic Task
			Thread.CurrentThread.Name = "GameLogic";
			GameLogicTask();
		}
	}
}
