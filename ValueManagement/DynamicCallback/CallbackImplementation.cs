using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// the type of dynamic execution to perform
	/// </summary>
	public enum CallbackImplementation
	{
		/// <summary>
		/// no type defined
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// call Python code
		/// </summary>
		Python = 1,

		/// <summary>
		/// call Ruby code
		/// </summary>
		Ruby = 2,

		/// <summary>
		/// call F# code - interactive mode (non assembly)
		/// </summary>
		FSharpInteractive = 3,

		/// <summary>
		/// call C# code - interactive mode (non assembly)
		/// </summary>
		CSharpInteractive = 4,

		/// <summary>
		/// call a compiled method (type) in an assembly
		/// </summary>
		DotNetMethod = 5,

		/// <summary>
		/// call a stored procedure in SQL Server
		/// </summary>
		SqlStoredProcedure = 6,

		/// <summary>
		/// interactive SQL code which is directly executed
		/// </summary>
		SqlInteractive = 7,

        /// <summary>
        /// A Windows Workflow module
        /// </summary>
        SoftwareAgent = 8
	}
}
