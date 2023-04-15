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

		private string _uniqueUiId = Guid.NewGuid().ToString();
		private ImGuiWindow _wnd;
		private ImageInfo _txKillAll;
		string _rootForRelativePaths;

		public GuiWindow( ImGuiWindow wnd )
		{
			_wnd = wnd;
			//_menuRenderer = new GuiWinMenuRenderer( _wnd, _reflStates );
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
		}
		



	}
}
