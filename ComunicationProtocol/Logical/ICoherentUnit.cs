using System.Net.Sockets;

namespace ComunicationProtocol.Logical
{
	/// <summary>
	/// ������ ������������/������. ������������� � ����������
	/// </summary>
	public interface ICoherentUnit: IDisposable
	{
		/// <summary>
		/// ������������ �����
		/// </summary>
		Socket Socket { get; }

		/// <summary>
		/// ������ ��� ��������� ��������� ������
		/// </summary>
		byte[] LenghtBuffer { get; }

		/// <summary>
		/// ������ ����� ������
		/// </summary>
		int Lenght { get; set; }

		/// <summary/>
		int CurrentPosition { get; set; }

		/// <summary>
		/// ������� ������
		/// </summary>
		byte[] ContentBuffer { get; set; }

		/// <summary>
		/// �������� ��������� ����������� �������������
		/// </summary>
		void ClearParameters();
	}

	/// <summary>
	/// ������ ������� ������������/������. ������������� � ����������
	/// </summary>
	public interface ICoherentReceiverUnit: ICoherentUnit
	{
		/// <summary>
		/// �������� �������
		/// </summary>
		ICoherentUnit Receiver { get; }
	}
}