using System;

using ComunicationProtocol.Messages;

namespace TestClient
{
	internal class Program
	{
		static void Main(string[] args)
		{
			ClientProtocol protocol = ClientProtocol.Instance;
			if (!protocol.Connect())
			{
				Console.WriteLine("НЕ смог подключится к серверу");
				return;
			}
			Console.WriteLine("Успешно подключились !");

			while (true)
			{
				Console.Write("Введите сообщение для отправки: ");
				string message = Console.ReadLine();

				string s = message.ToLowerInvariant();
				if (s == "quit" || s == "exit" || protocol.Disposed)
					break;

				Message msg = new Message {
					Id = Guid.NewGuid(),
					ActionId = Guid.Empty,
					ActionName = "Chat",
					Args = new object[] { message }
				};


				object result = protocol.SendMessage(msg) ?? false;

				Console.WriteLine("Результат отправки: {0}", result);
			}

			protocol.Dispose();
		}
	}
}
