using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellBrick.TestBrowser.Common
{
	public static class StringExtensions
	{
		public static string NullIfEmpty( this string str ) => str != String.Empty ? str : null;
	}
}
