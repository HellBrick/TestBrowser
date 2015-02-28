using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellBrick.TestBrowser.Common
{
	public delegate void EventHandler<in TSender, in TArgs>( TSender sender, TArgs args );
}
