namespace LogicalExprEval
{
	public interface IVariable
	{
		string Id { get; }
		string DisplayName { get; }
		System.Type Type { get; }
		
		/// <summary>
		///   Reads the value. May use given source data object.
		/// </summary>
		object GetValue( object source );
	}


	/// <summary>
	///   Always returns the value which stored inside,
	///   ignoring data source passed to GetValue().
	/// </summary>
	public class VariableWithConstValue : IVariable
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public Type Type { get; set; }
		public object GetValue( object source ) => Value;
		public object Value;
		

		public VariableWithConstValue( string id, string displayName, System.Type type, object value )
		{
			Id = id;
			DisplayName = displayName;
			Type = type;
			Value = value;
		}
	}

	/// <summary>
	///   Reads the value using Fasterflect.MemberGetter from given source data object.
	/// </summary>
	public class VariableWithMemberGetter : IVariable
	{
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public Type Type { get; set; }
		public Fasterflect.MemberGetter ValueGetter { get; set; }
		public object GetValue( object source ) => ValueGetter( source );

		public VariableWithMemberGetter( string id, string displayName, System.Type type, Fasterflect.MemberGetter valueGetter )
		{
			Id = id;
			DisplayName = displayName;
			Type = type;
			ValueGetter = valueGetter;
		}
	}

	/// <summary>
	///   Reads the value using given getter from the given source data object.
	/// </summary>
	public class VariableWithValueGetter : IVariable
	{
		public delegate object ValueGetterDelegate( object source );
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public Type Type { get; set; }
		public ValueGetterDelegate ValueGetter { get; set; }
		public object GetValue( object source ) => ValueGetter( source );

		public VariableWithValueGetter( string id, string displayName, System.Type type, ValueGetterDelegate valueGetter )
		{
			Id = id;
			DisplayName = displayName;
			Type = type;
			ValueGetter = valueGetter;
		}
	}
}
