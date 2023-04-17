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

	/// <summary>
	/// Draws FilterNode using ImGui.
	/// </summary>
	public static class FilterNodeRenderer
	{
		public static void Draw( FilterNode node )
		{
			if ( node == null ) return;


			node.ExecuteDeferredChanges();

			var ww = ImGui.GetWindowWidth();
			
			ImGui.PushID( node.Uuid );

			if( node.NodeType == FilterNode.EType.Leaf )
			{
				// variable name combo
				DrawVar( node );

				// condition operator & reference value
				ImGui.SameLine();
				ConditionRendeder.Draw( node.Condition, ww/3 );	

				// allow adding just inside AND/OR branch; there always is at least one node
				if( node.NodeType==FilterNode.EType.Leaf && node.Parent != null )
				{
					ImGui.SameLine();
					if( ImGui.Button( "+" ) )
					{
						node.DuplicateAsSibling();
					}
				}

				// add a new node using the AND condition
				// this converts the node into a new AND branch with the original node as the first child
				ImGui.SameLine();
				if( ImGui.Button( "&&" ) )
				{
					var newLeaf = node.InsertNewParent( FilterNode.EType.And );
					newLeaf.DuplicateAsSibling();
				}

				// add a new node using the OR condition
				// convert the node into a new OR branch with the node as the first child
				ImGui.SameLine();
				if( ImGui.Button( "||" ) )
				{
					var newLeaf = node.InsertNewParent( FilterNode.EType.Or );
					newLeaf.DuplicateAsSibling();
				}

				// remove the node
				// if there is just one left in the branch, remove the branch and replace it with the remaining node
				if( !node.IsOnlyChild() )
				{
					ImGui.SameLine();
					if( ImGui.Button( "X" ) )
					{
						node.RemoveAndTurnIntoLeafIfOrphaned();
					}
				}

			}
			else // draw tree
			{
				bool opened = ImGui.TreeNodeEx( node.NodeType.ToString(), ImGuiTreeNodeFlags.DefaultOpen );

				if( opened )
				{
					//if( node.Children != null )
					{
						foreach( var child in node.Children )
						{
							Draw( child ); // warnign this can change node.Children as well as node.Parent
							// Such changes need to be deferred after Draw()
						}
					}

					ImGui.TreePop();
				}
			}

			//ImGui.Text("   parent: " + (node.Parent?.ToString() ?? "null") );

			ImGui.PopID();
		}
		

		static void DrawVar( FilterNode node )
		{
			IVariable selectedVar = node.VarIndex < 0 ? null : node.Variables[node.VarIndex];
			var preview = selectedVar == null ? "<none>" : selectedVar.DisplayName;
			var ww = ImGui.GetWindowWidth();
			ImGui.SetNextItemWidth(ww/3);
			if( ImGui.BeginCombo( "##var", preview ) )
			{
				for( int i=0; i < node.Variables.Count; ++i )
				{
					var v = node.Variables[i];
					if( ImGui.Selectable( v.DisplayName, i == node.VarIndex ) )
					{
						node.SelectVariable( i );
					}
				}
				ImGui.EndCombo();
			}
		}

	}
}
