using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HellBrick.TestBrowser.Options
{
	public class TestBrowserOptions
	{
		private static JsonSerializer _serializer = new JsonSerializer();
		public static string OptionStreamKey = "TestBrowser.Options";

		public TestBrowserOptions()
		{
			CollapsedNodes = new List<NodeKey>();
			HumanizeTestNames = true;
		}

		public static TestBrowserOptions FromStream( Stream stream )
		{
			StreamReader reader = new StreamReader( stream );
			JsonTextReader jsonReader = new JsonTextReader( reader );

			return _serializer.Deserialize<TestBrowserOptions>( jsonReader );
		}

		public void WriteToStream( Stream stream )
		{
			StreamWriter writer = new StreamWriter( stream );
			_serializer.Serialize( writer, this );
			writer.Flush();
		}

		public List<NodeKey> CollapsedNodes { get; set; }
		public bool HumanizeTestNames { get; set; }
	}
}
