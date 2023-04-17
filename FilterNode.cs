using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace LogicalExprEval
{
	/// <summary>
	///   A node of a logical expression tree combining multiple conditions using AND/OR operators.
	///   FilterNode gets the source value (to evaluate the conditions with) from an IVariable
	///   object pointed by VarIndex in an array of variables.
	/// </summary>
	/// <remarks>
	///   Note: Each node can be individually negated => we do not need the negation operator.
	/// </remarks>
	public class FilterNode : IFilter
	{
		public enum EType
		{
			Leaf, // no children, just [variable - operator - refval]
			And, // AND branch (all subconditions must pass); at least two children always
			Or	 // OR branch (at least one subcondition must pass); at least two children always
		}

		public EType NodeType;
		public FilterNode Parent;
		public List<FilterNode> Children; // subnodes, used just for AND/OR nodes (null for leaf nodes)
		
		public int VarIndex; // what variable to read the current value from; index to Variables
		public Condition Condition; // how to compare the variable value

		
		/// <summary>
		///   All variables available for selection in the filter condition.
		/// </summary>
		public List<IVariable> Variables;

		// ImGui needs a unique ID for each node
		public string Uuid = Guid.NewGuid().ToString();

		// Changes made during prev Draw() call that can't be executed immediately 
		// because they would throw 'collection modified' when iterating the child node collection.
		public List<Action> DeferredChanges = new List<Action>();

		public override string ToString()
		{
			return $"[{NodeType}] {Describe(null)}";
		}

		public FilterNode( EType nodeType, int varIndex, List<IVariable> varList, Condition condition, FilterNode parent=null )
		{
			NodeType = nodeType;
			VarIndex = varIndex;
			Variables = varList;
			Condition = condition;
			Parent = parent;
			Children = null;
		}

		/// <summary>
		///  Turns this node into a copy of the other node; The other node is expected to be abandoned.
		/// </summary>
		public void InitFrom( FilterNode other )
		{
			NodeType = other.NodeType;
			VarIndex = other.VarIndex;
			Variables = other.Variables;
			Condition = other.Condition;
			Parent = other.Parent;
			Children = other.Children;
			DeferredChanges = other.DeferredChanges;

			// reparent the newly acquired children to us
			if( Children != null )
			{
				foreach( var child in Children )
				{
					child.Parent = this;
				}
			}

		}

		public bool Passed( object dataSample )
		{
			switch( NodeType )
			{
				case EType.Leaf:
					return EvalLeaf( dataSample );
				case EType.And:
					return EvalAnd( dataSample );
				case EType.Or:
					return EvalOr( dataSample );
			}
			throw new Exception("Unknown node type");
		}

		bool EvalLeaf( object dataSample )
		{
			object val = VarIndex < 0 ? null : Variables[VarIndex].GetValue( dataSample );
			return Condition.Passed( val );
		}


		bool EvalAnd( object dataSample )
		{
			bool result = true;
			if( Children != null)
			{
				foreach( var item in Children)
				{
					result = result && item.Passed( dataSample );
				}
			}
			return result;
		}

		bool EvalOr( object dataSample )
		{
			bool result = false;
			if( Children != null)
			{
				foreach( var item in Children)
				{
					result = result || item.Passed( dataSample);
				}
			}
			return result;
		}


		// implementation of the IFilter interface
		public string Describe( string varPrefix="" )
		{
			switch( NodeType )
			{
				case EType.Leaf:
				{
					string varName = VarIndex < 0 ? "<none>" : Variables[VarIndex].DisplayName;
					return Condition.Describe( varPrefix+varName );
				}
				case EType.And:
				{
					return Describe( varPrefix, "AND" );
				}
				case EType.Or:
				{
					return Describe( varPrefix, "OR" );
				}
			}
			throw new Exception("Unknown node type");
		}

		string Describe( string varPrefix, string operName )
		{
			string result = "";
			if( Children != null)
			{
				for( int i= 0; i < Children.Count; i++)
				{
					if( i > 0)
					{
						result += " "+operName+" ";
					}
					result += "("+Children[i].Describe(varPrefix)+")";
				}
			}
			return result;
		}

	}
}
