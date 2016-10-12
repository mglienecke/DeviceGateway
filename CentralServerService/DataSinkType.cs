using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CentralServerService
{
	/// <summary>
	/// defines the types of sink available
    /// NOTE: the actual enumeration element values are used as indices in an array. Thus - TAKE HEED WHEN CHANGING THEM
	/// </summary>
	public enum DataSinkType:int
	{
		/// <summary>
		/// after a new value has arrived 
		/// </summary>
		AfterValueArrived = 0,

		/// <summary>
		/// Before the value has been persisted
		/// </summary>
		BeforeValuePersisted = 1,

		/// <summary>
		/// after the value has been persisted in the database
		/// </summary>
		AfterValuePersisted = 2 ,

		/// <summary>
		/// Before a virtual value has computed
		/// </summary>
		BeforeVirtualValueComputed = 3,

		/// <summary>
		/// After a virtual value has computed
		/// </summary>
		AfterVirtualValueComputed = 4
	}
}
