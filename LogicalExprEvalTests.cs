using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalExprEval
{
	[TestClass()]
	public class ExprTests
	{
		[TestMethod()]
		public void IntTest()
		{
			var equalsOne = new ValueComparer() { Operator = EOperator.Equal, Value = 1 };
			Assert.AreEqual( false, equalsOne.Evaluate(null) );
			Assert.AreEqual( true, equalsOne.Evaluate(1) );
			Assert.AreEqual( false, equalsOne.Evaluate(2) );
			Assert.AreEqual( false, equalsOne.Evaluate("1") );	// we do not support automatic string conversion, types have to match
			Assert.AreEqual( false, equalsOne.Evaluate("2") );

		}

		public void NullTest()
		{
			var equalsNull = new ValueComparer() { Operator = EOperator.Equal, Value = null };

			Assert.AreEqual( true, equalsNull.Evaluate(null) ); // only null is equal to null
			Assert.AreEqual( false, equalsNull.Evaluate(1) );

			var notEqualsNullNegated = new ValueComparer() { Operator = EOperator.NotEqual, Value = null, Negate = true	};
			Assert.AreEqual( true, notEqualsNullNegated.Evaluate(null) );
			Assert.AreEqual( false, notEqualsNullNegated.Evaluate(1) );
			Assert.AreEqual( false, notEqualsNullNegated.Evaluate("1") );	// we do not support automatic string conversion, types have to match

		}
	}
}