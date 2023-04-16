using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicalExprEval
{
	public interface IFilter
	{
		/// <summary>
		///    Checks if given value passes the filter
		/// </summary>
		/// <param name="arg"> the value passed to the condition of the filter </param>
		/// <returns> true = the arg value passed the filter condition </returns>
		bool Passed( object arg );

		/// <summary> Provides human readable form of the filter expression </summary>
		/// <param name="argDescr"> the text used in place of the variable whose value is passed to Evalute() </param>
		/// <returns></returns>
		string Describe( string argDescr );
	}

	public class AndFilterContainer : IFilter
	{
		List<IFilter> _list;

		public AndFilterContainer(List<IFilter> list)
		{
			_list = list;
		}

		public override string ToString() => Describe("x");

		public bool Passed( object arg )
		{
			bool result = true;
			if( _list != null)
			{
				foreach( var item in _list)
				{
					result = result && item.Passed( arg );
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

	public class OrFilterContainer : IFilter
	{
		List<IFilter> _list;

		public OrFilterContainer(List<IFilter> list)
		{
			_list = list;
		}

		public override string ToString() => Describe("x");

		public bool Passed( object arg )
		{
			bool result = false;
			if( _list != null)
			{
				foreach( var item in _list)
				{
					result = result || item.Passed( arg );
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
