namespace DeBox.ObjectPool
{

	/// <summary>
	/// Describes a generic object pool interface
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IPool<T> {

		/// <summary>
		/// Borrow an instance of T
		/// </summary>
		/// <returns>instance of T</returns>
		T Borrow();

		/// <summary>
		/// Return a borrowed instance of T
		/// </summary>
		/// <param name="obj">Returned instance of type T</param>
		void Revert(T obj);
	}

}
