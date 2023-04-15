
namespace LogicalExprEval
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			//// To customize application configuration such as set high DPI settings or default font,
			//// see https://aka.ms/applicationconfiguration.
			//ApplicationConfiguration.Initialize();
			//Application.Run( new Form1() );

			var equalsNull = new ValueComparer()
			{
				Operator = EOperator.Equal,
				Value = null
			};

			var notEqualsNullNegated = new ValueComparer()
			{
				Operator = EOperator.NotEqual,
				Value = null,
				Negate = true
			};

			var equalsOne = new ValueComparer()
			{
				Operator = EOperator.Equal,
				Value = 1
			};

			var greaterThan3 = new ValueComparer()
			{
				Operator = EOperator.GreaterThanOrEqual,
				Value = 3,
				Negate = true
			};

			var expr = new LogicalExpressionOr( new List<ILogicalExpression>()
			{
				equalsOne,
				greaterThan3
			});

			Test( equalsNull, null );
			Test( notEqualsNullNegated, null );

			Test( expr, 1 );
			Test( expr, 2 );
			Test( expr, 3 );
			Test( expr, "nazdar" );
			Test( expr, null );

			var app = new GuiApp();
			app.run();
			app.Dispose();

		}

		static void Test( ILogicalExpression expr, object arg )
		{
			Console.WriteLine( "----------------------");
			var result = expr.Evaluate( arg );
			Console.WriteLine( "Describe: " + expr.Describe( ValueComparer.Describe(arg) ) );
			Console.WriteLine( "Result: " + result );
		}
	}
}