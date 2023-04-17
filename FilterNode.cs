using Newtonsoft.Json.Linq;
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

	///   Some changes to the tree structure can't be executed immediately, because they would
	///   break the children iteration if done from within the iteration. Instead, they are deferred
	///   to later. You need to call ExecuteDeferredChanges() to execute them before any other changes.
	///   
	///   Up to one tree change operation shall be made during a single loop over Children
	///   (like for example if drawing the UI using ImGui). Call ExecuteDeferredChanges
	///   before repeating the loop to make sure the change is properly applied.
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

		protected FilterNode( EType nodeType, List<IVariable> varList, int varIndex, Condition condition, FilterNode parent, List<FilterNode> children, List<Action> deferredChanges )
		{
			NodeType = nodeType;
			VarIndex = varIndex;
			Variables = varList;
			Condition = condition;
			Parent = parent;
			Children = children;
			DeferredChanges = deferredChanges ?? new List<Action>();
		}

		/// <summary>
		///   Creates And/Or node
		/// </summary>
		public FilterNode( EType nodeType, List<IVariable> varList )
			: this( nodeType, varList, -1, null, null, null, null )
		{
			if( nodeType == EType.Leaf )
				throw new ArgumentException( "This ctor is just for And/Or nodes, not the Leaf" );
		}

		/// <summary>
		///   Creates leaf node for given variable
		/// </summary>
		public FilterNode( List<IVariable> varList, int varIndex, Condition.EOperator oper=Condition.EOperator.Equal, object value=null, bool negate=false )
			: this( EType.Leaf, varList, varIndex, new Condition( varList[varIndex].Type, oper, value ), null, null, null )
		{
		}

		/// <summary>
		///   Shallow clone
		/// </summary>
		protected FilterNode( FilterNode other )
			: this( other.NodeType, other.Variables, other.VarIndex, other.Condition, other.Parent, other.Children, other.DeferredChanges )
		{
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

		/// <summary>
		///   Performs changes collected during recent tree change operation.
		///   Call this after any changes to the tree structure to apply them.
		///   Chnages are deferred because they can happen from within iteration of children - which would throw 'collection modified'.
		/// </summary>
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

		public void SelectVariable( int varIndex )
		{
			VarIndex = varIndex;

			// retype the value stored in the condition to match the new variable type
			if( VarIndex >= 0 )
			{
				var v = Variables[VarIndex];
				Condition.ValueType = v.Type;
			}
		}

		protected void ShallowCopyFrom( FilterNode other )
		{
			NodeType = other.NodeType;
			VarIndex = other.VarIndex;
			Variables = other.Variables;
			Condition = other.Condition;
			Parent = other.Parent;
			Children = other.Children;
			DeferredChanges = other.DeferredChanges;
		}

		/// <summary>
		///  Turns this node into a copy of the other node; The other node is expected to be abandoned.
		/// </summary>
		protected void ReincarnateFrom( FilterNode other )
		{
			ShallowCopyFrom( other );

			// reparent the newly acquired children to us
			if( Children != null )
			{
				foreach( var child in Children )
				{
					child.Parent = this;
				}
			}

		}

		/// <summary>
		///		Converts the node into a new branch with the node as the first child
		///		(basically inserts a branch node above this one)
		/// </summary>
		/// <param name="newBranchType">AND/OR node</param>
		/// <returns>newly created node made from the original one but now under the newly insterted parent </returns>
		public FilterNode InsertNewParent( EType newBranchType )
		{
			var newChild = new FilterNode( this );

			ConvertToBranch( newBranchType );
			AddChild( newChild );

			return newChild;
		}

		/// <summary>
		///   make and AND/OR brnach node from this one (whatever it was before is forgotten)
		/// </summary>
		/// <param name="newBranchType"></param>
		protected void ConvertToBranch( EType newBranchType )
		{
			NodeType = newBranchType;
			Condition = null;
			VarIndex = -1;
			Children = null;
		}


		public FilterNode AddNewSibling( EType nodeType, int varIndex, List<IVariable> varList, Condition condition )
		{
			var parent = Parent;
			if( parent == null ) return null;

			// creates a new leaf and adds it to the node's children
			var newChild = new FilterNode( nodeType, varList, varIndex, condition, parent, null, null);

			var index = parent.Children.IndexOf( this );
			
			parent.DeferredChanges.Add( () =>
			{
				parent.Children.Insert( index+1, newChild );
			});

			return newChild;
		}

		public FilterNode DuplicateAsSibling()
		{
			return AddNewSibling( EType.Leaf, VarIndex, Variables, new Condition() );
		}

		/// <summary>
		///   Remove the leaf node.
		///   If there is just one node left in the branch, remove the branch and replace it with the only remaining node.
		/// </summary>
		public void RemoveAndTurnIntoLeafIfOrphaned()
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

						parent.ReincarnateFrom( onlyChild );
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

		public FilterNode AddChild( FilterNode node )
		{
			if( Children == null )
			{
				Children = new List<FilterNode>();
			}
			Children.Add( node );
			
			node.Parent = this;
			
			return node;
		}
	}
}
