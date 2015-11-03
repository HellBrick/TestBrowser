using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HellBrick.TestBrowser.Common
{
	public static class Input
	{
		public static readonly DependencyProperty InputBindingsProperty =
			DependencyProperty.RegisterAttached( "InputBindings", typeof( InputBindingCollection ), typeof( Input ),
			new FrameworkPropertyMetadata(
				new InputBindingCollection(),
				propertyChangedCallback: ( sender, e ) =>
			   {
				   var element = sender as UIElement;
				   if ( element == null )
					   return;

				   element.InputBindings.Clear();
				   element.InputBindings.AddRange( e.NewValue as InputBindingCollection );
			   } ) );

		public static InputBindingCollection GetInputBindings( UIElement element )
		{
			return (InputBindingCollection) element.GetValue( InputBindingsProperty );
		}

		public static void SetInputBindings( UIElement element, InputBindingCollection inputBindings )
		{
			element.SetValue( InputBindingsProperty, inputBindings );
		}
	}
}
