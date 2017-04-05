using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using ComunicationProtocol;
using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;
using ComunicationProtocol.Protocols;

namespace TestServer
{
	public sealed class ServerProtocol: ServerProtocolBase<ServerProtocol>
	{
		protected override int Port => 21867;

		protected override string Host { get; }

		private ServerProtocol()
		{
			// find Host IP
			IPAddress hostIp = Dns.GetHostEntry(Dns.GetHostName())
								.AddressList.First(e => e.AddressFamily == AddressFamily.InterNetwork);

			Host = hostIp.ToString();

			MessageReceived += ListenerServer_MessageReceived;
			MessageSended += ListenerServer_MessageSended;

			ClientConnected += InformationEventHandler;
			ClientDisconnected += InformationEventHandler;
		}
		private void InformationEventHandler(ICoherentUnit client, IMessage message)
		{
			Console.WriteLine("{0} -> {1}", client.Socket.RemoteEndPoint,
				message != null ? message.ActionName : "Null Action Name");
		}
		protected override ICoherentReceiverUnit CreateReceiverUnit(Socket socket, Socket receiver, int lenghtBytes)
		{
			return new DefaultReceiverUnit(socket, receiver, lenghtBytes);
		}

		private void PrintMessage(IMessage message)
		{
			string[] s = { nameof(message.Id), nameof(message.ActionId), nameof(message.Args) };
			object[] p = { message.Id, message.ActionId, $"{{\n\t\t{string.Join(",\n\t\t", message.Args)}\n\t}}" };

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("{");
			for (int i = 0; i < s.Length; i++)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("\t{0}", s[i]);

				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.Write(" = ");

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("{0}", p[i]);
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("}");

			Console.ForegroundColor = ConsoleColor.Gray;
		}

		private ListenerResult ListenerServer_MessageSended(ICoherentUnit sender, IMessage message)
		{
			Console.WriteLine();

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("Message ");

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(message.ActionName);

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(" Sended ");

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("{0}  ", DateTime.Now.ToString("dd.MM.yyyy - hh:mm:ss"));

			PrintMessage(message);

			return ListenerResult.None;
		}

		private ListenerResult ListenerServer_MessageReceived(ICoherentUnit client, IMessage result)
		{
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("Message ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write(result.ActionName);
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(" Received From ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("{0}  ", client.Socket.RemoteEndPoint);
			PrintMessage(result);

			Message msg = new Message(result);
			object[] obj = new object[result.Args.Length + 2];

			obj[0] = client.Socket.RemoteEndPoint.ToString();
			obj[1] = msg.ActionName;

			for (int i = 2; i < obj.Length; i++)
			{
				obj[i] = result.Args[i - 2];
			}

			msg.Args = obj;
			msg.Id = Guid.NewGuid();

			SendMessageAll(msg, client);

			// ѕровер€ет зарезервированное ли данное сообщение как `ѕротокольное`
			// ≈сли да, то лучше не отправл€ть результат клиенту, потому что он уже получит результат от протокола.
			if (HasMessageRegistered(result))
			{
				return ListenerResult.Continue;// мы не обрабатываем такое
			}

			// возвращаем клиенту сообщение об результате обработки (или же любой другой результат, который нужен)
			ReturnMessage(new MessageResult(result) { Result = true }, client);

			return ListenerResult.Override;// собщение было уже обработано
		}


		public string GetHost()
		{
			return Host;
		}

		public override bool Disconect()
		{
			SendMessageAll(DisconectMesage());

			return base.Disconect();
		}
	}
}