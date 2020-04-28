namespace DeBox.ObjectPool
{
	/// <summary>
	/// Describes an optional interface that a pooled MonoBehaviour can implement in order to receive callbacks on borrow
	/// and revert operations performed on its instances
	///
	/// An instance of a GameObject can contain multiple components implementing this interface. In this case
	/// the callbacks will be called on ALL the components of the object (But not their children!)
	/// </summary>
	public interface IPooledComponent
	{
		/// <summary>
		/// Called just after the pooled object instance is borrowed, but not yet returned to the borrower
		/// </summary>
		void OnBorrow();
		
		/// <summary>
		/// Called just before the pooled object instance is returned
		/// </summary>
		void OnRevert();
	}
}