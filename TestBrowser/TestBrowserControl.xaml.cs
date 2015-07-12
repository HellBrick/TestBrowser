using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HellBrick.TestBrowser.Models;

namespace HellBrick.TestBrowser
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class TestBrowserControl : UserControl
	{
		public TestBrowserControl( SolutionTestBrowserModel dataContext )
		{
			DataContext = dataContext;
			InitializeComponent();
		}
	}
}