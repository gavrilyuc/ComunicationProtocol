using System;

namespace TestServer
{
	internal class Program
	{
		public static void Main()
		{
			Console.BackgroundColor = ConsoleColor.Black;


			ServerProtocol protocol = ServerProtocol.Instance;
			
			Console.WriteLine("[Server] Crerating server listener...");

			if (protocol.Connect())
			{
				Console.Write("[Server] Server started on IP: ");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(protocol.GetHost());
				Console.ForegroundColor = ConsoleColor.Gray;
			}

			while (true)
			{
				string msg = Console.ReadLine().ToLower();

				if (msg == "quit" || msg == "exit")
					break;
			}

			protocol.Dispose();
		}
	}
}
