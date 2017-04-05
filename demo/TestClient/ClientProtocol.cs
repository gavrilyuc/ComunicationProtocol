using System;

using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;
using ComunicationProtocol.Protocols;

namespace TestClient
{
	public sealed class ClientProtocol: ClientProtocolBase<ClientProtocol>
	{
		protected override string Host { get; }

		protected override int Port => 21867;

		private ClientProtocol(): base()
		{
			#region Custom Comand Ip Parametr
			const string IpCommand = "-ip";
			
			var comands = Environment.GetCommandLineArgs();
			for (int i = 0; i < comands.Length; i++)
			{
				if (comands[i] == IpCommand && i + 1 < comands.Length)
				{
					Host = comands[i + 1];
					break;
				}
			}

			if (string.IsNullOrWhiteSpace(Host))
			{
				throw new NotFoundComandLine(IpCommand, "<Your Server IP example: 127.0.0.1>");
			}
			#endregion

			MessageServerReceived += ClientProtocol_MessageReceived;
		}

		private void ClientProtocol_MessageReceived(ICoherentUnit client, IMessage message)
		{
			if (message.ActionName == nameof(Disconect))
			{
				Dispose();
				return;
			}

			Console.WriteLine($"Не ожидано сервер прислал нам [{message.ActionName}]: {string.Join(", ", message.Args)}");
		}

		public object SendMessage(Message msg)
		{
			return base.SendMessage(msg)?.Result;
		}


		private class NotFoundComandLine: Exception
		{
			public NotFoundComandLine(string parametr, string defaultValue): base($"Not Found Comand Parameter, please add parameter: {parametr} {defaultValue}")
			{

			}
		}
	}
}