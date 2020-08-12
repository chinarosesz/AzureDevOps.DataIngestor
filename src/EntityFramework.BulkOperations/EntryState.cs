using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFramework.BulkOperations
{
	[Flags]
	public enum EntryState
	{
		Added = 0x1,
		Modified = 0x2,
		Deleted = 0x4,
		Unchanged = 0x8
	}
}
