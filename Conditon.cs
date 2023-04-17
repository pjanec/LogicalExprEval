using Microsoft.VisualBasic;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalExprEval
{
	/// <summary>
	///   Compares the stored value to the value of the argument of Passed(data) method
	///   using selected comparison operator. Optionally performs the negation of the result.
	/// </summary>
	public class Condition : IFilter
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
		}


		/// <summary> Type of the refernce value </summary>
		public System.Type ValueType;
		
		/// <summary> Reference value compared with the arg of Evaluate(arg) </summary>
		public object Value;

		public EOperator Operator;
		
		/// <summary> Shall we negate the result of the operator comparison </summary>
		public bool Negate;

		public Condition( System.Type valueType = null, EOperator oper = EOperator.Equal, object value=null, bool negate = false )
		{
			this.ValueType = valueType;

			System.Type type = Type.GetType("System." + valueType);
			if( type != null && type.IsValueType )
			{
				this.Value = Activator.CreateInstance(type);
			}

			this.Operator = oper;
			this.Value = value;
			this.Negate = negate;
		}

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

		// IFilter implem
		public string Describe( string varName )
		{
			string text = "";
			switch( Operator)
			{
				case EOperator.Equal:
					text = $"{varName} == {Describe(Value)}";
					break;
				case EOperator.NotEqual:
					text = $"{varName} != {Describe(Value)}";
					break;
				case EOperator.GreaterThan:
					text = $"{varName} > {Describe(Value)}";
					break;
				case EOperator.GreaterThanOrEqual:
					text = $"{varName} >= {Describe(Value)}";
					break;
				case EOperator.LessThan:
					text = $"{varName} < {Describe(Value)}";
					break;
				case EOperator.LessThanOrEqual:
					text = $"{varName} <= {Describe(Value)}";
					break;
				case EOperator.StartsWith:
					text = $"{varName} starts with {Describe(Value)}";
					break;
				case EOperator.EndsWith:
					text = $"{varName} ends with {Describe(Value)}";
					break;
				case EOperator.Contains:
					text = $"{varName} contains {Describe(Value)}";
					break;
			}
			if( Negate ) text = $"!({text})";
			return text;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arg">value to compare using the contition's operator and stored reference value</param>
		/// <returns></returns>
		public bool Passed(object arg)
		{
			bool result = false;

			// only direct equality check between nulls succeedes
			if( arg == null && Value == null )
			{
				if( Operator == EOperator.Equal ) return !Negate;
				if( Operator == EOperator.NotEqual ) return Negate;
				return false;
			}

			if( arg == null || Value == null )
				return false;

			// different types => try retyping
			if( arg != null && Value != null && arg.GetType() != Value.GetType() )
			{
				try
				{
					arg = Convert.ChangeType( arg, Value.GetType() );
				}
				catch( Exception )
				{
					return false;
				}
			}

			var compArg = (arg as IComparable);
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
					var stringArg = (string) Convert.ChangeType( arg, TypeCode.String );
					var stringValue = (string) Convert.ChangeType( Value, TypeCode.String );
					if( stringArg != null && stringValue != null  )
					{
						result = stringArg.StartsWith( stringValue );
					}
					break;
				}

				case EOperator.EndsWith:
				{
					var stringArg = (string) Convert.ChangeType( arg, TypeCode.String );
					var stringValue = (string) Convert.ChangeType( Value, TypeCode.String );
					if( stringArg != null && stringValue != null  )
					{
						result = stringArg.EndsWith( stringValue );
					}
					break;
				}

				case EOperator.Contains:
				{
					var stringArg = (string) Convert.ChangeType( arg, TypeCode.String );
					var stringValue = (string) Convert.ChangeType( Value, TypeCode.String );
					if( stringArg != null && stringValue != null  )
					{
						result = stringArg.Contains( stringValue );
					}
					break;
				}
			}

			if( Negate )
				result = !result;

			return result;
		}
	}
}
