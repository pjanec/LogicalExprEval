using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Drawing;
using ImGuiNET;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Fasterflect;
using System.Diagnostics;

namespace LogicalExprEval
{
	public class GuiWindow : Disposable
	{
		public bool HasMenu => true;

		private ImGuiWindow _wnd;

		FilterNode _rootFN;
		List<IVariable> _vars;



		// sample data structure we are using Fastreflect for retrieving the members
		class MyData
		{
			public string Field1;
			public int Field2;
		}


		public GuiWindow( ImGuiWindow wnd )
		{
			_wnd = wnd;
			//_menuRenderer = new GuiWinMenuRenderer( _wnd, _reflStates );

			//_vars = new List<IVariable>()
			//{
			//	new VariableWithConstValue( "var1", "Variable1_str", typeof(String), "hello" ),
			//	new VariableWithConstValue( "var2", "Variable2_int", typeof(Int64), 42 ),
			//};

			//// statically compiled members
			//_vars = new List<IVariable>()
			//{
			//	new VariableWithValueGetter( "var1", "Field1", typeof(String), (object source ) => ((MyData)source).Field1 ),
			//	new VariableWithMemberGetter( "var2", "Field2", typeof(Int64), (object source ) => ((MyData)source).Field2 ),
			//};

			// fastreflect generated members
			_vars = new List<IVariable>()
			{
				new VariableWithMemberGetter( "var1", "Field1", typeof(String), Reflect.FieldGetter(typeof(MyData), "Field1") ),
				new VariableWithMemberGetter( "var2", "Field2", typeof(Int64), Reflect.FieldGetter(typeof(MyData), "Field2") ),
			};


			//_rootFN = new FilterNode( FilterNode.EType.Leaf, _vars );
			_rootFN = CreateTree();



			Benchmark();

		}

		FilterNode CreateTree()
		{
			var root = new FilterNode( FilterNode.EType.Or, _vars );
			{
				var n1 = root.AddChild( new FilterNode( _vars, 0, Condition.EOperator.StartsWith, "he" ) );

				var n2 = root.AddChild( new FilterNode( FilterNode.EType.And, _vars ) );
				{
					n2.AddChild( new FilterNode( _vars, 0, Condition.EOperator.Equal, "hello" ) );
					n2.AddChild( new FilterNode( _vars, 1, Condition.EOperator.Equal, 42 ) );
				}
			}
			return root;
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


			FilterNodeRenderer.Draw( _rootFN );

			var descr = _rootFN.Describe("");
			ImGui.Text($"Descr: {descr}" );
			
			
			var dataSample = new MyData()
			{
				Field1 = "hello",
				Field2 = 42,
			};

			var result = _rootFN.Passed( dataSample );

			ImGui.Text($"Result: {result}" );
		}

		void Benchmark()
		{
			var root = CreateTree();

			Console.WriteLine($"{root.Describe()}");

			var dataSample = new MyData()
			{
				Field1 = "hello",
				Field2 = 42,
			};

			var sw = new Stopwatch();
			sw.Start();

			for(int i=0; i < 1000000; i++)
			{
				var result = root.Passed( dataSample );
			}
			Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms");

		}

		



	}
}
