using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;

namespace ComunicationProtocol
{
	/// <summary>
	/// ������� �� ���������� �������� ������������
	/// </summary>
	/// <param name="client">������ ������������</param>
	/// <param name="message">������������� ���������</param>
	public delegate void ListenEventHandler(ICoherentUnit client, IMessage message);

	/// <summary>
	/// ��������� ���������� ��������
	/// </summary>
	public enum ListenerResult
	{
		/// <summary>
		/// None
		/// </summary>
		None,
		
		/// <summary>
		/// ���������� ��������� (���������� ��� true)
		/// </summary>
		Continue,

		/// <summary>
		/// ������ ����� ��� ��� ��������� ���� �������, ������ ����������� ������ ������� �������� �� ����� �����������
		/// </summary>
		Override
	}

	/// <summary>
	/// �������-������� ���������� �������� ������������
	/// </summary>
	/// <param name="client">������ ������������</param>
	/// <param name="message">������������� ���������</param>
	/// <returns>���� ������������ ��������� ����� ����� = overide �� ������ ������� ������� ��������� ��������</returns>
	public delegate ListenerResult ListenForwardEventHandler(ICoherentUnit client, IMessage message);
}