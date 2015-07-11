using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using HellBrick.TestBrowser.Options;

namespace HellBrick.TestBrowser.Models
{
	public class SettingsModel: PropertyChangedBase
	{
		private readonly TestBrowserOptions _options;

		public SettingsModel( TestBrowserOptions options )
		{
			_options = options;
		}

		public bool HumanizeTestNames
		{
			get { return _options.HumanizeTestNames; }
			set { _options.HumanizeTestNames = value; NotifyOfPropertyChange( nameof( HumanizeTestNames ) ); }
		}
	}
}
