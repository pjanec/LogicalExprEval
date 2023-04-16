namespace LogicalExprEval
{
	public interface IVariable
	{
		string Id { get; }
		string DisplayName { get; }
		System.Type Type { get; }
		object Value { get; }
	}


	public class Variable : IVariable
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public Type Type { get; set; }
		public object Value { get; set; }

		public Variable( string id, string displayName, System.Type type, object value )
		{
			Id = id;
			DisplayName = displayName;
			Type = type;
			Value = value;
		}
	}
}
