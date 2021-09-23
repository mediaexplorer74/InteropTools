﻿using InteropTools.CorePages;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static InteropTools.ShellPages.Core.Viewmodel;
using RegPlugin = AppPlugin.PluginList.PluginList<string, string, InteropTools.Providers.Registry.Definition.TransfareOptions, double>.PluginProvider;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InteropTools.ShellPages.Core
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtensionsPage
    {
        public string PageName => "Extensions";
        public PageGroup PageGroup => PageGroup.Bottom;

        private DataTemplate dtSmall;
		private DataTemplate dtEnlarged;

		private bool loaded = false;

        public ExtensionsPage()
		{
			this.InitializeComponent();
			dtSmall = (DataTemplate)Resources["dtSmall"];
			dtEnlarged = (DataTemplate)Resources["dtEnlarged"];
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count != 0)
			{
				foreach (var item in e.RemovedItems)
				{
					((ListViewItem)(sender as ListView).ContainerFromItem(item)).ContentTemplate = dtSmall;
				}

				foreach (var item in e.AddedItems)
				{
					((ListViewItem)(sender as ListView).ContainerFromItem(e.AddedItems[0])).ContentTemplate = dtEnlarged;
				}
			}
		}

		private void ListView_ItemClick(object sender, ItemClickEventArgs e)
		{
			ListView lv = (sender as ListView);

			if (lv.SelectedItem == null)
			{
				lv.SelectedItem = e.ClickedItem;
			}

			else
			{
				if (lv.SelectedItem.Equals(e.ClickedItem))
				{
					lv.SelectedItem = null;
				}

				else
				{
					lv.SelectedItem = e.ClickedItem;
				}
			}
		}

		private async void UninstallButton_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;

            if (btn.DataContext.GetType() == typeof(DisplayableRegPlugin))
            {
                DisplayableRegPlugin ext = btn.DataContext as DisplayableRegPlugin;

                var catalog = Windows.ApplicationModel.AppExtensions.AppExtensionCatalog.Open(InteropTools.Providers.Registry.Definition.RegistryProvidersWithOptions.PLUGIN_NAME);
                await catalog.RequestRemovePackageAsync(ext.Plugin.Extension.Package.Id.FullName);
            }
            if (btn.DataContext.GetType() == typeof(DisplayablePowerPlugin))
            {
                DisplayablePowerPlugin ext = btn.DataContext as DisplayablePowerPlugin;

                var catalog = Windows.ApplicationModel.AppExtensions.AppExtensionCatalog.Open(InteropTools.Providers.OSReboot.Definition.OSRebootProvidersWithOptions.PLUGIN_NAME);
                await catalog.RequestRemovePackageAsync(ext.Plugin.Extension.Package.Id.FullName);
            }
            if (btn.DataContext.GetType() == typeof(DisplayableApplicationPlugin))
            {
                DisplayableApplicationPlugin ext = btn.DataContext as DisplayableApplicationPlugin;

                var catalog = Windows.ApplicationModel.AppExtensions.AppExtensionCatalog.Open(InteropTools.Providers.Applications.Definition.ApplicationProvidersWithOptions.PLUGIN_NAME);
                await catalog.RequestRemovePackageAsync(ext.Plugin.Extension.Package.Id.FullName);
            }
        }
        
		private void ExtensionCheckbox_Loaded(object sender, RoutedEventArgs e)
		{
			loaded = true;
		}

		private void ExtensionCheckbox_Unloaded(object sender, RoutedEventArgs e)
		{
			loaded = false;
		}
	}
}
