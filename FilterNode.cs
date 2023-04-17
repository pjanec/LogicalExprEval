using System.Xml.Linq;
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

		public void ExecuteDeferredChanges()
		{
			foreach (var action in DeferredChanges)
			{
				action();
			}
			DeferredChanges.Clear();

			// recurse to children to be sure all changes are applied
			if( Children != null )
			{
				foreach( var child in Children )
				{
					child.ExecuteDeferredChanges();
				}
			}
		}

		public void SelectVar( int varIndex )
		{
			VarIndex = varIndex;

			// retype the value stored in the condition to match the new variable type
			if( VarIndex >= 0 )
			{
				var v = Variables[VarIndex];
				Condition.ValueType = v.Type;
			}
		}

		public FilterNode ConvertLeafIntoBranch( FilterNode.EType newBranchType )
		{
			var newChild = new FilterNode( NodeType, VarIndex, Variables, Condition, this );

			// convert the node into a new branch with the node as the first child
			NodeType = newBranchType;
			Children = new List<FilterNode>() { newChild };
			Condition = null;
			VarIndex = -1;

			return newChild;

		}

		public FilterNode AddNewSibling( EType nodeType, int varIndex, List<IVariable> varList, Condition condition )
		{
			var parent = Parent;
			if( parent == null ) return null;

			// creates a new leaf and adds it to the node's children
			var newChild = new FilterNode( nodeType, varIndex, varList, condition, parent);

			var index = parent.Children.IndexOf( this );
			
			parent.DeferredChanges.Add( () =>
			{
				parent.Children.Insert( index+1, newChild );
			});

			return newChild;
		}

		public FilterNode AddNewSibling()
		{
			return AddNewSibling( EType.Leaf, VarIndex, Variables, new Condition() );
		}

		public void RemoveLeaf()
		{
			// remove from parent's list
			var parent = Parent;
			if( parent != null )
			{

				// determine what child will remain once the deferred removal executes
				int onlyChildLeftIndex = -1;
				if( parent.Children.Count == 2 ) // after the removal action executes, there will be just 1...
				{
					var childToRemoveIndex = parent.Children.IndexOf( this );
					if( childToRemoveIndex == 0 ) // removing first one
					{
						onlyChildLeftIndex = 1; // second one will remain
					}
					else // removing second one
					{
						onlyChildLeftIndex = 0; // first one will remain
					}
				}

				parent.DeferredChanges.Add( () =>
				{
					parent.Children.Remove( this );
				});

				// if just one child left, use it instead of the parent
				if( onlyChildLeftIndex >=0 )
				{
					var onlyChild = parent.Children[onlyChildLeftIndex];

					// the onlychild's parent turns into the onlychild
					var newParentForOnlyChild = parent.Parent; 
					
					parent.DeferredChanges.Add( () =>
					{
						onlyChild.Parent = newParentForOnlyChild;

						parent.InitFrom( onlyChild );
					});
				}
			}
		}

		public bool IsOnlyChild()
		{
			var parent = this.Parent;
			if( parent != null )
			{
				return parent.Children.Count == 1;
			}
			return true; // no parent = act as if only child
		}
	}
}
