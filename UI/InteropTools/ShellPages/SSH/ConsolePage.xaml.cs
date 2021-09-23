﻿using System;
using System.IO;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using InteropTools.Providers;
using InteropTools.ShellPages.Core;
using Renci.SshNet;
using Renci.SshNet.Common;
using Shell = InteropTools.CorePages.Shell;
using Windows.ApplicationModel.Resources.Core;
using InteropTools.CorePages;

namespace InteropTools.ShellPages.SSH
{
	public sealed partial class ConsolePage
    {
        public string PageName => "System Console";
        public PageGroup PageGroup => PageGroup.SSH;

        public string CMDLoc = @"C:\Windows\System32\cmd.exe";
		private readonly IRegistryProvider _helper;

		public ConsolePage()
		{
			InitializeComponent();
			_helper = App.MainRegistryHelper;
            Refresh();
		}

		private async void Refresh()
		{
			Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;

			if (!await App.IsCMDSupported())
			{
				await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(
				  ResourceManager.Current.MainResourceMap.GetValue("Resources/In_order_to_use_this_page", ResourceContext.GetForCurrentView()).ValueAsString,
				  ResourceManager.Current.MainResourceMap.GetValue("Resources/You_can_t_use_this_right_now", ResourceContext.GetForCurrentView()).ValueAsString);
				var shell = (Shell)App.AppContent;
				shell.RootFrame.Navigate(typeof(WelcomePage));
				return;
			}

			RegTypes regtype;
			string regvalue;
			var ret = await _helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"System\CurrentControlSet\Control\CI", "UMCIAuditMode",
			                    RegTypes.REG_DWORD); regtype = ret.regtype; regvalue = ret.regvalue;

            if (regvalue != "1")
			{
				await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(
				  ResourceManager.Current.MainResourceMap.GetValue("Resources/In_order_to_use_this_page", ResourceContext.GetForCurrentView()).ValueAsString,
				  ResourceManager.Current.MainResourceMap.GetValue("Resources/You_can_t_use_this_right_now", ResourceContext.GetForCurrentView()).ValueAsString);
				var shell = (Shell)App.AppContent;
				shell.RootFrame.Navigate(typeof(WelcomePage));
				return;
			}

			Start();
		}

		public ShellStream ShellStream { get; set; }

		private void Start()
		{
			RunInThreadPool(() =>
			{
				try
				{
					var client = App.SshClient;
					ShellStream = client.CreateShellStream("cmd", 80, 24, 800, 600, 1024);
					ShellStream.DataReceived += Stream_DataReceived;
				}

				catch (Exception ex)
				{
					RunInUiThread(async () => { await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(ex.Message); });
				}
			});
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
		}

		private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
		{
			if (ShellStream == null)
			{
				return;
			}

			if (ShellStream.CanWrite)
			{
				ShellStream.Write(((char) args.KeyCode).ToString());
			}
		}

		private static async void RunInUiThread(Action function)
		{
			await
			CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() => { function(); });
		}

		private static async void RunInThreadPool(Action function)
		{
			await ThreadPool.RunAsync(x => { function(); });
		}

		private void Stream_DataReceived(object sender, ShellDataEventArgs e)
		{
			var data_ = e.Data;
			RunInUiThread(() =>
			{
				var curtext = ConsoleBox.Text;
				var newtext = Encoding.ASCII.GetString(data_);
				curtext = curtext + newtext.Replace("\r\r", "\r");
				ConsoleBox.Text = curtext;
				MainScroll.ChangeView(0, MainScroll.ScrollableHeight, 1);
			});
		}

		private void ConsoleBox_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var flyout = new MenuFlyout {Placement = FlyoutPlacementMode.Top};
			var flyoutitem = new MenuFlyoutItem
			{
				Text =
				ResourceManager.Current.MainResourceMap.GetValue("Resources/Paste",
				ResourceContext.GetForCurrentView()).ValueAsString
			};
			flyoutitem.Click += async (sender_, e_) =>
			{
				var content = Clipboard.GetContent();
				var str = await content.GetTextAsync();

				if (ShellStream == null)
				{
					return;
				}

				if (ShellStream.CanWrite)
				{
					ShellStream.Write(str);
				}
			};
			var datacontent = Clipboard.GetContent();

			if (!datacontent.Contains(StandardDataFormats.Text))
			{
				flyoutitem.IsEnabled = false;
			}

			var flyoutitem2 = new MenuFlyoutItem
			{
				Text =
				ResourceManager.Current.MainResourceMap.GetValue("Resources/Select_All",
				ResourceContext.GetForCurrentView()).ValueAsString
			};
			flyoutitem2.Click += (sender_, e_) => { ConsoleBox.SelectAll(); };

			if (flyout.Items != null)
			{
				flyout.Items.Add(flyoutitem);
				flyout.Items.Add(flyoutitem2);
			}

			flyout.ShowAt((TextBlock) sender, e.GetPosition((TextBlock) sender));
		}

		private void ConsoleBox_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ConsoleBox.Text.Split('\n').Length > 200)
			{
				var lines = ConsoleBox.Text.Split('\n');
				Array.Reverse(lines);
				var newcontent = new string[200];

				for (var i = 0; i < 200; i++)
				{
					newcontent[i] = lines[i];
				}

				Array.Reverse(newcontent);
				ConsoleBox.Text = string.Join("\n", newcontent);
			}

			MainScroll.ChangeView(0, MainScroll.ScrollableHeight, 1);
		}

		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			ShowKeybHack.Focus(FocusState.Keyboard);
			MainScroll.ChangeView(0, MainScroll.ScrollableHeight, 1);
		}

		private void ShowKeybHack_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			if (!ConsoleBox.Text.EndsWith(ShowKeybHack.Text) && (ShowKeybHack.Text != "") &&
			    (ShowKeybHack.Text.Length > 1))
			{
				if (ShellStream != null)
					if (ShellStream.CanWrite)
					{
						ShellStream.Write(ShowKeybHack.Text);
					}
			}

			ShowKeybHack.Text = "";
		}
	}
}