using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalExprEval
{
	public interface ILogicalExpression
	{
		bool Evaluate( object arg );
		string Describe( string argDescr );
	}

	public class LogicalExpressionAnd : ILogicalExpression
	{
		List<ILogicalExpression> _list;

		public LogicalExpressionAnd(List<ILogicalExpression> list)
		{
			_list = list;
		}

		public override string ToString() => Describe("x");

		public bool Evaluate( object arg )
		{
			bool result = true;
			if( _list != null)
			{
				foreach( var item in _list)
				{
					result = result && item.Evaluate( arg );
				}
			}
			return result;
		}

		public string Describe( string argDescr )
		{
			string result = "";
			if( _list != null)
			{
				for( int i= 0; i < _list.Count; i++)
				{
					if( i > 0)
					{
						result += " AND ";
					}
					result += "("+_list[i].Describe( argDescr )+")";
				}
			}
			return result;
		}
	}

	public class LogicalExpressionOr : ILogicalExpression
	{
		List<ILogicalExpression> _list;

		public LogicalExpressionOr(List<ILogicalExpression> list)
		{
			_list = list;
		}

		public override string ToString() => Describe("x");

		public bool Evaluate( object arg )
		{
			bool result = false;
			if( _list != null)
			{
				foreach( var item in _list)
				{
					result = result || item.Evaluate( arg );
				}
			}
			return result;
		}

		public string Describe( string argDescr )
		{
			string result = "";
			if( _list != null)
			{
				for( int i= 0; i < _list.Count; i++)
				{
					if( i > 0)
					{
						result += " OR ";
					}
					result += "("+_list[i].Describe( argDescr )+")";
				}
			}
			return result;
		}
	}
}
