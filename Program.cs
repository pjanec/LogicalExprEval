
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

			var equalsNull = new Condition()
			{
				Operator = Condition.EOperator.Equal,
				Value = null
			};

			var notEqualsNullNegated = new Condition()
			{
				Operator = Condition.EOperator.NotEqual,
				Value = null,
				Negate = true
			};

			var equalsOne = new Condition()
			{
				Operator = Condition.EOperator.Equal,
				Value = 1
			};

			var greaterThan3 = new Condition()
			{
				Operator = Condition.EOperator.GreaterThanOrEqual,
				Value = 3,
				Negate = true
			};

			var expr = new OrFilterContainer( new List<IFilter>()
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

		static void Test( IFilter expr, object arg )
		{
			Console.WriteLine( "----------------------");
			var result = expr.Passed( arg );
			Console.WriteLine( "Describe: " + expr.Describe( Condition.Describe(arg) ) );
			Console.WriteLine( "Result: " + result );
		}
	}
}