namespace ComunicationProtocol
{
	/// <summary>
	/// Определяет методы высвобождения распределенных ресурсов.
	/// </summary>
	/// <filterpriority>2</filterpriority>
	public interface IDisposable : System.IDisposable
	{
		/// <summary>
		/// Данный Объект Освобождён (True = освобождён)
		/// </summary>
		bool Disposed { get; }
	}
}