using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.TestBrowser.Options;

namespace HellBrick.TestBrowser.Core
{
	public interface IOptionsService
	{
		TestBrowserOptions LoadOptions();
		void SaveOptions( TestBrowserOptions options );
	}
}
