using System.Net.Sockets;

namespace ComunicationProtocol.Logical
{
	/// <summary>
	/// ������� ����������� ������.
	/// ������ ������ ��������� � ���� ����� � ���������, ����� ��� ���� ��������� ������� � ������ �����������, �������� � ����������� �������
	/// </summary>
	public class DefaultCoherentUnit: ICoherentUnit
	{
		/// <summary>
		/// ������������ �����
		/// </summary>
		public Socket Socket { get; internal set; }

		/// <summary>
		/// ������ ��� ��������� ��������� ������
		/// </summary>
		public byte[] LenghtBuffer { get; }

		/// <summary>
		/// ������ ����� ������
		/// </summary>
		public int Lenght { get; set; }

		/// <summary/>
		public int CurrentPosition { get; set; }

		/// <summary>
		/// ������� ������
		/// </summary>
		public byte[] ContentBuffer { get; set; }
		
		/// <summary>
		/// ���������������� ������
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// ������ ������ ��������� (True = ���������)
		/// </summary>
		public bool Disposed { get; private set; }

		/// <summary/>
		internal DefaultCoherentUnit(int lenghBytes)
		{
			LenghtBuffer = new byte[lenghBytes];
		}

		/// <summary/>
		public DefaultCoherentUnit(Socket socket, int lenghBytes): this(lenghBytes)
		{
			Socket = socket;
		}

		/// <summary>
		/// �������� ���������
		/// </summary>
		public void ClearParameters()
		{
			for (int i = 0; i < LenghtBuffer.Length; i++)
				LenghtBuffer[i] = 0;
			ContentBuffer = null;
			CurrentPosition = 0;
			Lenght = 0;
		}

		/// <summary>
		/// ��������� ������������ ����������� ������, ��������� � ���������, ��������������
		/// ��� ������� ������������� ��������.
		/// </summary>
		public void Dispose()
		{
			if (Disposed)
				return;
			
			Disposed = true;
			ClearParameters();

			if (Socket.Connected)
			{
				Socket.Disconnect(false);
				Socket.Shutdown(SocketShutdown.Both);
			}
			
			Socket.Dispose();
		}
	}
}