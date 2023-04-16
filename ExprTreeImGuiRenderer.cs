using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using ImGuiNET;

namespace LogicalExprEval
{

	public static class ExprTreeImGuiRenderer
	{
		public static void Draw( FilterNode node )
		{
			if ( node == null ) return;


			ExecuteDeferredChanges( node );

			var ww = ImGui.GetWindowWidth();
			
			ImGui.PushID( node.Uuid );

			if( node.NodeType == FilterNode.EType.Leaf )
			{
				DrawVar( node );

				ImGui.SameLine();

				ConditionImGuiRendeder.Draw( node.Condition, ww/3 );	
				ImGui.SameLine();

				// remove button
				if( !IsOnlyChild( node ) )
				{
					if( ImGui.Button( "X" ) )
					{
						RemoveLeaf( node );
					}

					ImGui.SameLine();
				}

				// allow adding just inside AND/OR branch; there always is at least one node
				if( node.NodeType==FilterNode.EType.Leaf && node.Parent != null )
				{
					if( ImGui.Button( "+" ) )
					{
						AddNewSiblingLeaf( node );
					}
					ImGui.SameLine();
				}

				// convert the node into a new AND branch with the node as the first child
				if( ImGui.Button( "&&" ) )
				{
					var newLeaf = ConvertLeafIntoBranch( node, FilterNode.EType.And );
					AddNewSiblingLeaf( newLeaf );
				}

				ImGui.SameLine();

				// convert the node into a new OR branch with the node as the first child
				if( ImGui.Button( "||" ) )
				{
					var newLeaf = ConvertLeafIntoBranch( node, FilterNode.EType.Or );
					AddNewSiblingLeaf( newLeaf );
				}

			}
			else // draw tree
			{
				bool opened = ImGui.TreeNodeEx( node.NodeType.ToString(), ImGuiTreeNodeFlags.DefaultOpen );

				if( opened )
				{
					//var childrenCopy = new List<FilterNode>(node.Children);
					foreach( var child in node.Children )
					{
						Draw( child ); // warnign this can change node.Children as well as node.Parent
						// Such changes need to be deferred after Draw()
					}

					ImGui.TreePop();
				}
			}

			//ImGui.Text("   parent: " + (node.Parent?.ToString() ?? "null") );

			ImGui.PopID();
		}
		

		static bool IsOnlyChild( FilterNode node )
		{
			var parent = node.Parent;
			if( parent != null )
			{
				return parent.Children.Count == 1;
			}
			return true; // no parent = act as if only child
		}

		static void AddNewSiblingLeaf( FilterNode node )
		{
			var parent = node.Parent;
			if( parent == null ) return;

			// creates a new leaf and adds it to the node's children
			var newChild = new FilterNode( FilterNode.EType.Leaf, node.VariableId, node.VariableList, new Condition(), parent);

			var index = parent.Children.IndexOf( node );
			
			parent.DeferredChanges.Add( () =>
			{
				parent.Children.Insert( index+1, newChild );
			});
		}

		static void RemoveLeaf( FilterNode node )
		{
			// remove from parent's list
			var parent = node.Parent;
			if( parent != null )
			{

				// determine what child will remain once the deferred removal executes
				int onlyChildLeftIndex = -1;
				if( parent.Children.Count == 2 ) // after the removal action executes, there will be just 1...
				{
					var childToRemoveIndex = parent.Children.IndexOf( node );
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
					parent.Children.Remove( node );
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

		static FilterNode ConvertLeafIntoBranch( FilterNode node, FilterNode.EType newBranchType )
		{
			var newChild = new FilterNode( node.NodeType, node.VariableId, node.VariableList, node.Condition, node );

			// convert the node into a new branch with the node as the first child
			node.NodeType = newBranchType;
			node.Children = new List<FilterNode>() { newChild };
			node.Condition = null;
			node.VariableId = null;

			return newChild;

		}

		static void DrawVar( FilterNode node )
		{
			var selectedVar = node.VariableList.FirstOrDefault( x => x.Id == node.VariableId );
			var preview = selectedVar == null ? "<none>" : selectedVar.DisplayName;
			var ww = ImGui.GetWindowWidth();
			ImGui.SetNextItemWidth(ww/3);
			if( ImGui.BeginCombo( "##var", preview ) )
			{
				foreach( var v in node.VariableList )
				{
					if( ImGui.Selectable( v.DisplayName, selectedVar==null ? false : selectedVar.Id == v.Id ) )
					{
						node.VariableId = v.Id;
						// retype the value stored in the condition to match the new variable type
						node.Condition.ValueType = v.Type;
					}
				}
				ImGui.EndCombo();
			}
		}

		static void ExecuteDeferredChanges( FilterNode node )
		{
			foreach (var action in node.DeferredChanges)
			{
				action();
			}
			node.DeferredChanges.Clear();

			// recurse to children to be sure all changes are applied
			if( node.Children != null )
			{
				foreach( var child in node.Children )
				{
					ExecuteDeferredChanges( child );
				}
			}
		}

	}
}
