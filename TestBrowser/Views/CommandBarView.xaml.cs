using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HellBrick.TestBrowser.Views
{
	/// <summary>
	/// Interaction logic for CommandBarView.xaml
	/// </summary>
	public partial class CommandBarView : UserControl
	{
		private static readonly TimeSpan _popupReopenThreshold = TimeSpan.FromMilliseconds( 100 );
		private DateTime? _popupClosedTimestamp;

		public CommandBarView()
		{
			InitializeComponent();
		}

		private void OnSettingsHyperlinkClick( object sender, RoutedEventArgs e )
		{
			TimeSpan timeSincePopupClosing = _popupClosedTimestamp.HasValue ? DateTime.UtcNow - _popupClosedTimestamp.Value : TimeSpan.MaxValue;
			if ( timeSincePopupClosing > _popupReopenThreshold )
				SettingsPopup.IsOpen = true;
		}

		private void OnSettingsPopupClosed( object sender, EventArgs e )
		{
			_popupClosedTimestamp = DateTime.UtcNow;
		}
	}
}
