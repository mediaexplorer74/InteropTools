﻿using InteropTools.Providers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static InteropTools.ContentDialogs.Providers.Viewmodel;
using RegPlugin = AppPlugin.PluginList.PluginList<string, string, InteropTools.Providers.Registry.Definition.TransfareOptions, double>.PluginProvider;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace InteropTools.ContentDialogs.Providers
{
    public sealed partial class SelectRegistryProviderContentDialog : ContentDialog
    {
        public SelectRegistryProviderContentDialog()
        {
            this.InitializeComponent();
            var dc = this.DataContext as Viewmodel;
            dc.RegPlugins.CollectionChanged += RegPlugins_CollectionChanged;
        }
        
        private void RegPlugins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dc = this.DataContext as Viewmodel;
            if (dc.RegPlugins.Count == 0)
            {
                NoneText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                GetExtensions.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Auto.IsEnabled = false;
                Manual.IsEnabled = false;
                IsPrimaryButtonEnabled = false;
                IsSecondaryButtonEnabled = false;
            } else
            {
                NoneText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                GetExtensions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Auto.IsEnabled = true;
                Manual.IsEnabled = true;
                if (Auto.IsChecked.HasValue ? Auto.IsChecked.Value : false)
                {
                    IsPrimaryButtonEnabled = true;
                }
                IsSecondaryButtonEnabled = true;
            }
        }

        private IRegProvider provider;

        public async Task<IRegProvider> AskUserForProvider()
        {
            await ShowAsync();
            return provider;
        }
        
        private void extlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var plugin = e.ClickedItem as DisplayablePlugin;
            provider = new RegistryProvider(plugin.Plugin);
            Hide();
        }
        
        private void Auto_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                extlist.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;
            }
            catch
            {
                
            }
        }

        private void Auto_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                extlist.Visibility = Windows.UI.Xaml.Visibility.Visible;
                IsPrimaryButtonEnabled = false;
            }
            catch
            {

            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
