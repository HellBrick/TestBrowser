﻿using System;
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
	public partial class CommandBarView: UserControl
	{
		public CommandBarView()
		{
			InitializeComponent();
		}

		private void OnSettingsHyperlinkClick( object sender, RoutedEventArgs e )
		{
			SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
		}
	}
}