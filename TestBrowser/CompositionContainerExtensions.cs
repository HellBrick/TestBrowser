using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HellBrick.TestBrowser
{
	public static class CompositionContainerExtensions
	{
		public static void AppendCatalog( this CompositionContainer container, IEnumerable<ComposablePartDefinition> catalog )
		{
			CompositionBatch batch = new CompositionBatch();
			foreach ( var partDefinition in catalog )
				batch.AddPart( partDefinition.CreatePart() );

			container.Compose( batch );
		}
	}
}
