namespace LogicalExprEval
{
	public interface IVariable
	{
		string Id { get; }
		string DisplayName { get; }
		TypeCode TypeCode { get; }
		object Value { get; }
	}


	public class Variable : IVariable
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public TypeCode TypeCode { get; set; }
		public object Value { get; set; }

		public Variable( string id, string displayName, TypeCode typeCode, object value )
		{
			Id = id;
			DisplayName = displayName;
			TypeCode = typeCode;
			Value = value;
		}
	}
}
