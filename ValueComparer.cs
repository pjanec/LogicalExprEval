using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalExprEval
{
	public enum EOperator
	{
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		StartsWith,
		EndsWith,
		Contains,
		Between // inclusive; Value <= value <= Value2
	}

	public class ValueComparer : ILogicalExpression
	{
		public object Value;
		public object Value2;
		public EOperator Operator;
		public bool Negate;

		public override string ToString()
		{
			return Describe("x");
		}

		public static string Describe( object value )
		{
			if( value == null ) return "null";
			if( value is string ) return $"\"{value}\"";
			if( value is bool ) return value.ToString().ToLower();
			if( value is DateTime ) return $"\"{value}\"";
			if( value is IEnumerable )
			{
				var sb = new StringBuilder();
				sb.Append("[");
				var list = value as IEnumerable;
				bool first = true;
				foreach( var item in list)
				{
					if( first ) first = false;
					else sb.Append(", ");
					sb.Append( Describe( item ) );
				}
				sb.Append("]");
				return sb.ToString();
			}
			return value.ToString();
		}

		public string Describe( string argDesc )
		{
			string text = "";
			switch( Operator)
			{
				case EOperator.Equal:
					text = $"{argDesc} == {Describe(Value)}";
					break;
				case EOperator.NotEqual:
					text = $"{argDesc} != {Describe(Value)}";
					break;
				case EOperator.GreaterThan:
					text = $"{argDesc} > {Describe(Value)}";
					break;
				case EOperator.GreaterThanOrEqual:
					text = $"{argDesc} >= {Describe(Value)}";
					break;
				case EOperator.LessThan:
					text = $"{argDesc} < {Describe(Value)}";
					break;
				case EOperator.LessThanOrEqual:
					text = $"{argDesc} <= {Describe(Value)}";
					break;
				case EOperator.StartsWith:
					text = $"{argDesc} starts with {Describe(Value)}";
					break;
				case EOperator.EndsWith:
					text = $"{argDesc} ends with {Describe(Value)}";
					break;
				case EOperator.Contains:
					text = $"{argDesc} contains {Describe(Value)}";
					break;
				case EOperator.Between:
					text = $"{argDesc} is between {Describe(Value)} and {Describe(Value2)}";
					break;
			}
			if( Negate ) text = $"!({text})";
			return text;
		}

		public bool Evaluate(object arg)
		{
			bool result = false;
			var compArg = (arg as IComparable);

			// only direct equality check between nulls succeedes
			if( arg == null && Value == null )
			{
				if( Operator == EOperator.Equal ) return !Negate;
				if( Operator == EOperator.NotEqual ) return Negate;
				return false;
			}

			if( arg == null || Value == null )
				return false;

			// different types => not equal
			if( arg != null && Value != null && arg.GetType() != Value.GetType() )
				return false;

			if( compArg == null )
				return false;

			switch( Operator)
			{
				case EOperator.Equal:
					result = arg.Equals(Value);
					break;

				case EOperator.NotEqual:
					result = !arg.Equals(Value);
					break;

				case EOperator.GreaterThan:
					result = compArg.CompareTo(Value) > 0;
					break;

				case EOperator.GreaterThanOrEqual:
					result = compArg.CompareTo(Value) >= 0;
					break;

				case EOperator.LessThan:
				{
					result = compArg.CompareTo(Value) < 0;
					break;
				}

				case EOperator.LessThanOrEqual:
				{
					result = compArg.CompareTo( Value ) <= 0;
					break;
				}

				case EOperator.StartsWith:
				{
					var stringArg = (arg as string);
					var stringValue = (Value as string);
					if( stringArg != null && stringValue != null  )
					{
						result = stringArg.StartsWith( stringValue );
					}
					break;
				}

				case EOperator.EndsWith:
				{
					var stringArg = (arg as string);
					var stringValue = (Value as string);
					if( stringArg != null && stringValue != null  )
					{
						result = stringArg.EndsWith( stringValue );
					}
					break;
				}

				case EOperator.Contains:
				{
					var stringArg = (arg as string);
					var stringValue = (Value as string);
					result = stringArg.Contains( stringValue );
					break;
				}

				case EOperator.Between:
				{
					result = compArg.CompareTo( Value ) >= 0 && compArg.CompareTo( Value2 ) <= 0;
					break;
				}
			}

			if( Negate )
				result = !result;

			return result;
		}
	}
}
