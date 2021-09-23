﻿/*++

Copyright (c) 2016  Interop Tools Development Team
Copyright (c) 2017  Gustave M.

Module Name:

    Plugin.cs

Abstract:

    This module implements the WinRT Application Plugin.

Author:

    Gustave M.     (gus33000)       13-May-2017

Revision History:

    Gustave M. (gus33000) 13-May-2017

        Initial Implementation.

--*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Windows.ApplicationModel.Background;
using InteropTools.Providers.Applications.Definition;
using Windows.Management.Deployment;
using System.Text;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.ApplicationModel.AppService;

namespace InteropTools.Providers.Applications.WinRTProvider
{

    public sealed class ApplicationsProvider : IBackgroundTask
    {
        private IBackgroundTask internalTask = new ApplicationsProviderIntern();
        public void Run(IBackgroundTaskInstance taskInstance)
         => this.internalTask.Run(taskInstance);
    }

    internal class ApplicationsProviderIntern : ApplicationProvidersWithOptions
    {
        public class Item
        {
            public string DisplayName { get; set; }
            public string Description { get; set; }

            public string FullName { get; set; }

            public dynamic logo { get; set; }

            public PackageVolume volume { get; set; }

            public PackageTypes type { get; set; }
        }

        protected override async Task<string> ExecuteAsync(AppServiceConnection sender, string input, IProgress<double> progress, CancellationToken cancelToken)
        {
            var arr = input.Split(new string[] { "Q+q:8rKwjyVG\"~@<],TNH!@kcn/qUv:=3=Zs)+gU$Efc:[&Ku^qn,U}&yrRY{}byf<4DV&W!mF>R@Z8uz=>kgj~F[KeB{,]'[Veb" }, StringSplitOptions.None);

            var operation = arr.First();
            APPLICATIONS_OPERATION operationenum;
            Enum.TryParse(operation, true, out operationenum);

            List<List<string>> returnvalue = new List<List<string>>();
            List<string> returnvalue2 = new List<string>();

            switch (operationenum)
            {
                case APPLICATIONS_OPERATION.RemovePackage:
                    {
                        RemovalOptions remop;
                        Enum.TryParse<RemovalOptions>(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(1))), out remop);
                        var res = await new PackageManager().RemovePackageAsync(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(2))), remop);
                        
                        returnvalue2.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(APPLICATIONS_STATUS.SUCCESS.ToString())));
                        returnvalue.Add(returnvalue2);
                        break;
                    }
                case APPLICATIONS_OPERATION.RegisterPackage:
                    {
                        DeploymentOptions remop;
                        Enum.TryParse<DeploymentOptions>(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(1))), out remop);
                        var res = await new PackageManager().RegisterPackageAsync(new Uri(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(2)))), null, remop);

                        returnvalue2.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(APPLICATIONS_STATUS.SUCCESS.ToString())));
                        returnvalue.Add(returnvalue2);
                        break;
                    }
                case APPLICATIONS_OPERATION.AddPackage:
                    {
                        DeploymentOptions remop;
                        Enum.TryParse<DeploymentOptions>(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(1))), out remop);
                        var res = await new PackageManager().AddPackageAsync(new Uri(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(2)))), null, remop);

                        returnvalue2.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(APPLICATIONS_STATUS.SUCCESS.ToString())));
                        returnvalue.Add(returnvalue2);
                        break;
                    }
                case APPLICATIONS_OPERATION.UpdatePackage:
                    {
                        DeploymentOptions remop;
                        Enum.TryParse<DeploymentOptions>(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(1))), out remop);
                        var res = await new PackageManager().UpdatePackageAsync(new Uri(Encoding.UTF8.GetString(Convert.FromBase64String(arr.ElementAt(2)))), null, remop);

                        returnvalue2.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(APPLICATIONS_STATUS.SUCCESS.ToString())));
                        returnvalue.Add(returnvalue2);
                        break;
                    }
                case APPLICATIONS_OPERATION.QueryApplicationVolumes:
                    {
                        var vols = await new PackageManager().GetPackageVolumesAsync();
                        
                        returnvalue2.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(APPLICATIONS_STATUS.SUCCESS.ToString())));
                        returnvalue.Add(returnvalue2);

                        foreach (var item in vols)
                        {
                            List<string> itemlist = new List<string>();
                            itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.MountPoint)));
                            itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.PackageStorePath)));
                            itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Name)));
                            itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.IsSystemVolume.ToString())));
                            itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.IsOffline.ToString())));
                            itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.SupportsHardLinks.ToString())));
                            returnvalue.Add(itemlist);
                        }
                        break;
                    }
                case APPLICATIONS_OPERATION.QueryApplications:
                    {
                        try
                        {
                            //_volumelist = new ObservableCollection<VolumeDisplayitem>();
                            //_typelist = new ObservableCollection<TypeDisplayitem>();
                            //_itemsList = new ObservableRangeCollection<Item>();
                            //_filteredItemsList = new ObservableRangeCollection<Item>();
                            //_itemsList.CollectionChanged += ItemsList_CollectionChanged;
                            //_filteredItemsList.CollectionChanged += _filteredItemsList_CollectionChanged;
                            /*await RunInUiThread(() => {
                                LoadingText.Text = "Fetching available system volumes...";
                                LoadingStack.Visibility = Visibility.Visible;
                            });
                            var itemSource = AlphaKeyGroup<Item>.CreateGroups(_filteredItemsList, CultureInfo.InvariantCulture,
                                             s => s.DisplayName, true);
                            await RunInUiThread(() => {
                                ((CollectionViewSource)Resources["AppsGroups"]).Source = itemSource;
                            });*/
                            var tmplist = new List<Item>();
                            var vols = await new PackageManager().GetPackageVolumesAsync();
                            /*_volumelist.Add(new VolumeDisplayitem());

                            foreach (var vol in vols)
                            {
                                _volumelist.Add(new VolumeDisplayitem() { Volume = vol });
                            }

                            await RunInUiThread(() => {
                                VolListView.ItemsSource = _volumelist;
                                VolListView.SelectedIndex = 0;
                            });
                            await RunInUiThread(() => {
                                LoadingText.Text = "Fetching available package types...";
                                LoadingStack.Visibility = Visibility.Visible;
                            });*/
                            var pkgtypes = Enum.GetValues(typeof(PackageTypes)).Cast<PackageTypes>();
                            /*_typelist.Add(new TypeDisplayitem());

                            foreach (var type in pkgtypes)
                            {
                                _typelist.Add(new TypeDisplayitem() { Type = type });
                            }*/

                            /*await RunInUiThread(() => {
                                TypeListView.ItemsSource = _typelist;
                                TypeListView.SelectedIndex = 0;
                            });
                            await RunInUiThread(() => {
                                LoadingText.Text = "Determining the number of packages present in the system...";
                                LoadingStack.Visibility = Visibility.Visible;
                            });*/
                            int numofpkgs = 0;

                            foreach (var vol in vols)
                            {
                                foreach (var type in pkgtypes)
                                {
                                    var pkgs = vol.FindPackagesForUserWithPackageTypes("", type);
                                    numofpkgs += pkgs.Count();
                                }
                            }

                            double count = 0;

                            foreach (var vol in vols)
                            {
                                var applist = new List<Package>();

                                foreach (var type in pkgtypes)
                                {
                                    var pkgs = vol.FindPackagesForUserWithPackageTypes("", type);

                                    foreach (var package in pkgs)
                                    {
                                        count++;
                                        /*await RunInUiThread(() => {
                                            LoadingText.Text = String.Format("Fetching information for packages... ({0}%)", Math.Round(count / numofpkgs * 100, 0));
                                        });*/

                                        var arch = package.Id.Architecture.ToString();

                                        var displayname = package.Id.FamilyName;
                                        var description = arch + " " + package.Id.Version.Major + "." + package.Id.Version.Minor + "." +
                                                          package.Id.Version.Build + "." + package.Id.Version.Revision;
                                        dynamic logo = "";

                                        try
                                        {
                                            var appEntries = await package.GetAppListEntriesAsync();

                                            foreach (var appEntry in appEntries)
                                            {
                                                try
                                                {
                                                    displayname = appEntry.DisplayInfo.DisplayName;
                                                }

                                                catch
                                                {
                                                    // ignored
                                                }

                                                try
                                                {
                                                    description = appEntry.DisplayInfo.Description + "\n" + arch + " " +
                                                                  package.Id.Version.Major + "." + package.Id.Version.Minor + "." +
                                                                  package.Id.Version.Build + "." + package.Id.Version.Revision;
                                                }

                                                catch
                                                {
                                                    // ignored
                                                }

                                                try
                                                {
                                                    var logosize = new Size
                                                    {
                                                        Height = 160,
                                                        Width = 160
                                                    };
                                                    var applogo = await appEntry.DisplayInfo.GetLogo(logosize).OpenReadAsync();
                                                    /*await RunInUiThread(() => {
                                                        var bitmapImage = new BitmapImage();
                                                        bitmapImage.SetSource(applogo);
                                                        logo = bitmapImage;
                                                    });*/
                                                }

                                                catch
                                                {
                                                    // ignored
                                                }

                                                break;
                                            }
                                        }

                                        catch
                                        {
                                            // ignored
                                        }

                                        if (string.IsNullOrEmpty(displayname.Trim()))
                                        {
                                            displayname = package.Id.FamilyName;
                                        }

                                        tmplist.Add(new Item
                                        {
                                            DisplayName = displayname,
                                            FullName = package.Id.FullName,
                                            Description = description,
                                            logo = logo,
                                            volume = vol,
                                            type = type
                                        });
                                    }
                                }
                            }


                            foreach (var item in tmplist)
                            {
                                List<string> itemlist = new List<string>();
                                itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.DisplayName)));
                                itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.FullName)));
                                itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.Description)));
                                itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.logo)));
                                itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.volume.MountPoint)));
                                itemlist.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item.type.ToString())));
                                returnvalue.Add(itemlist);
                            }
                        }

                        catch (Exception caughtEx)
                        {
                            
                        }

                        returnvalue2.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(APPLICATIONS_STATUS.SUCCESS.ToString())));
                        returnvalue.Add(returnvalue2);
                        
                        break;
                    }
            }

            var returnstr = "";

            foreach (var str in returnvalue)
            {
                var str2 = string.Join("*[Pp)8/P'=Tu(pm\"fYNh#*7w27V~>bubdt#\"AF~'\\}{jwAE2uY5,~bEVfBZ2%xx+UK?c&Xr@)C6/}j?5rjuB=8+egU\\D@\"; T3M<%", str);
                if (string.IsNullOrEmpty(returnstr))
                {
                    returnstr = str2;
                }
                else
                {
                    returnstr += "Q+q:8rKwjyVG\"~@<],TNH!@kcn/qUv:=3=Zs)+gU$Efc:[&Ku^qn,U}&yrRY{}byf<4DV&W!mF>R@Z8uz=>kgj~F[KeB{,]'[Veb" + str2;
                }
            }

            return returnstr;
        }


        protected override Task<Options> GetOptions()
            => Task.FromResult<Options>(new OSRebootProviderOptions());

        protected override Guid GetOptionsGuid() => OSRebootProviderOptions.ID;

    }
}
