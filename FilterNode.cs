using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace LogicalExprEval
{
	/// <summary>
	///   A node of a logical expression tree combining multiple filter conditions using AND/OR operators.
	/// </summary>
	/// <remarks>
	///   Note: each node can be individually negated => we do not need the negation operator.
	/// </remarks>
	public class FilterNode	: IFilter
	{
		public enum EType
		{
			Leaf, // no subitems, just Field - operator - value
			And,
			Or
		}

		public EType NodeType;
		public string VariableId; // where to get the value from
		public Condition Condition; // how to compare the variable value

		public FilterNode Parent;
		public List<FilterNode> Children;
		public List<IVariable> VariableList;

		public string Uuid = Guid.NewGuid().ToString();

		// Changes made during prev Draw() call that can't be executed immediately 
		// because they would throw 'collection modified' when iterating the child node collection.
		public List<Action> DeferredChanges = new List<Action>();

		public override string ToString()
		{
			return $"[{NodeType}] {Describe(null)}";
		}

		public FilterNode( EType nodeType, string varId, List<IVariable> varList, Condition condition, FilterNode parent=null )
		{
			NodeType = nodeType;
			VariableId = varId;
			VariableList = varList;
			Condition = condition;
			Parent = parent;
			Children = null;
		}

		public void InitFrom( FilterNode other )
		{
			NodeType = other.NodeType;
			VariableId = other.VariableId;
			VariableList = other.VariableList;
			Condition = other.Condition;
			Parent = other.Parent;
			Children = other.Children;
			DeferredChanges = other.DeferredChanges;

			// reparent the bewly acquired children to us
			if( Children != null )
			{
				foreach( var child in Children )
				{
					child.Parent = this;
				}
			}

		}

		public bool Passed( object arg )
		{
			switch( NodeType )
			{
				case EType.Leaf:
					return EvalLeaf( arg );
				case EType.And:
					return EvalAnd( arg );
				case EType.Or:
					return EvalOr( arg );
			}
			throw new Exception("Unknown node type");
		}

		bool EvalLeaf( object arg )
		{
			var var1 = VariableList.FirstOrDefault( x => x.Id == VariableId );
			object val = var1?.Value;
			return Condition.Passed( val );
		}


		bool EvalAnd( object arg )
		{
			bool result = true;
			if( Children != null)
			{
				foreach( var item in Children)
				{
					result = result && item.Passed( arg );
				}
			}
			return result;
		}

		bool EvalOr( object arg )
		{
			bool result = false;
			if( Children != null)
			{
				foreach( var item in Children)
				{
					result = result || item.Passed( arg );
				}
			}
			return result;
		}


		public string Describe( string argDescr )
		{
			switch( NodeType )
			{
				case EType.Leaf:
					return Condition.Describe( VarName );
				case EType.And:
					return Describe( argDescr, "AND" );
				case EType.Or:
					return Describe( argDescr, "OR" );
			}
			throw new Exception("Unknown node type");
		}

		public string Describe( string argDescr, string operName )
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
					result += "("+Children[i].Describe( argDescr )+")";
				}
			}
			return result;
		}

		string VarName { get
		{
			var var1 = VariableList.FirstOrDefault( x => x.Id == VariableId );
			return var1?.DisplayName ?? "<none>";
		}}

	}
}
