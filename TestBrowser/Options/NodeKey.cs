using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HellBrick.TestBrowser.Models;

namespace HellBrick.TestBrowser.Options
{
	public class NodeKey: IEquatable<NodeKey>
	{
		public NodeKey( NodeType type, string name )
		{
			Type = type;
			Name = name;
		}

		public NodeType Type { get; private set; }
		public string Name { get; private set; }

		public override string ToString() => $"{Type} {Name}";

		#region IEquatable<NodeKey> Members

		public bool Equals( NodeKey other )
		{
			return
				this.Name == other.Name &&
				this.Type == other.Type;
		}

		public override bool Equals( object obj )
		{
			return
				obj is NodeKey &&
				this.Equals( (NodeKey) obj );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				const int prime = 23;

				hash = hash * prime + Type.GetHashCode();
				hash = hash * prime + Name.GetHashCode();
				return hash;
			}
		}

		#endregion
	}
}
