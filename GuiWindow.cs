using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Drawing;
using ImGuiNET;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace LogicalExprEval
{
	public class GuiWindow : Disposable
	{
		public bool HasMenu => true;

		private ImGuiWindow _wnd;

		FilterNode _rootFN;
		List<IVariable> _vars;

		public GuiWindow( ImGuiWindow wnd )
		{
			_wnd = wnd;
			//_menuRenderer = new GuiWinMenuRenderer( _wnd, _reflStates );
					
			_vars = new List<IVariable>()
			{
				new Variable( "var1", "Variable1_str", TypeCode.String, "hello" ),
				new Variable( "var2", "Variable2_int", TypeCode.Int64, 42 ),
			};
			_rootFN = new FilterNode( FilterNode.EType.Leaf, _vars[0].Id, _vars, new Condition( TypeCode.String ) );
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		public void Tick()
		{
		}

		
		public void DrawUI()
		{

			float ww = ImGui.GetWindowWidth();

			//_menuRenderer.DrawUI();

			if( ImGui.BeginTabBar("MainTabBar") )
			{
				if( ImGui.BeginTabItem("Tab1") )
				{
					//DrawApps();
					ImGui.EndTabItem();
				}
				ImGui.EndTabBar();
			}


			ExprTreeImGuiRenderer.Draw( _rootFN );

			var descr = _rootFN.Describe(null);
			ImGui.Text($"Descr: {descr}" );
			
			
			var result = _rootFN.Evaluate(null);
			ImGui.Text($"Result: {result}" );
		}
		



	}
}
