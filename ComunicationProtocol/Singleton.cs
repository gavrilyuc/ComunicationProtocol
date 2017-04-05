using System;
using System.Reflection;

namespace ComunicationProtocol
{
	/// <summary>
	/// Объект реализующий паттерн одиночка (Singleton)
	/// </summary>
	/// <typeparam name="TCurrentObject">Конечный объект, который и будет представлять себя в виде одиночки</typeparam>
	public abstract class Singleton<TCurrentObject> where TCurrentObject : class
	{
		private static volatile TCurrentObject _instance;

		private static readonly object LockOnInstance = new object();

		static Singleton()
		{
			lock (LockOnInstance)
			{
				if (_instance != null)
				{
					return;
				}

				ConstructorInfo constructor =
					typeof(TCurrentObject).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
						new Type[0], null);

				if (constructor == null || constructor.IsAssembly)
				{
					throw new SingletonException(
						$"A private or protected constructor is missing for '{typeof(TCurrentObject).Name}'.");
				}

				if (_instance == null)
				{
					_instance = (TCurrentObject)constructor.Invoke(null);
				}
			}
		}

		/// <summary>
		/// Объект <see cref="Singleton{T}" />
		/// </summary>
		public static TCurrentObject Instance => _instance;

		private class SingletonException : Exception
		{
			public SingletonException(string s) : base(s)
			{

			}
		}
	}
}