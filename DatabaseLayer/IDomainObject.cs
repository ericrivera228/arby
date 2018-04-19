using System;

namespace DatabaseLayer
{
	public interface IDomainObject
	{
		int? Id { get; set; }
		DateTime? CreateDateTime { get; set; }
		DateTime? LastModifiedDateTime { get; set; }

		int? PersistToDb();
	}
}
