using ImGuiNET;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace LogicalExprEval
{
	public static class ConditionImGuiRendeder
	{
		public static void Draw( Condition vc, float width )
		{
			DrawNeg( vc );
			ImGui.SameLine();
			DrawOper( vc, width*0.33f );
			ImGui.SameLine();
			DrawValue( vc, width*0.75f );
		}

		static void DrawNeg( Condition vc )
		{
			ImGui.Checkbox("##negate", ref vc.Negate );
		}

		static string[] _operDispNames = new string[]
		{
			"==",
			"!=",
			">",
			">=",
			"<",
			"<=",
			"Starts with",
			"Ends with",
			"Contains",
		};
		static void DrawOper( Condition vc, float width )
		{
			int operIndex = (int) vc.Operator;
			ImGui.SetNextItemWidth ( width );
			if( ImGui.Combo( "##oper", ref operIndex, _operDispNames, _operDispNames.Length ) )
			{
				vc.Operator = (Condition.EOperator) operIndex;
			}
		}

		static void DrawValue( Condition vc, float width )
		{
			var text = (string) Convert.ChangeType( vc.Value, TypeCode.String) ?? string.Empty;

			bool convFailed = false;

			ImGui.SetNextItemWidth ( width );
			if( ImGui.InputText( "##value", ref text, 1000 ) )
			{
				try
				{
					vc.Value = Convert.ChangeType( text, vc.ValueType );
				}
				catch( Exception )
				{
					convFailed = true;
				}
			}

			if( convFailed )
			{
				ImGui.SameLine();
				ImGui.TextColored( new System.Numerics.Vector4( 1, 0, 0, 1 ), "!" );
			}
		}
	}
}
