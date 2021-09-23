﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using InteropTools.Providers;
using Windows.ApplicationModel.Resources.Core;
using InteropTools.CorePages;
using System.Diagnostics;

namespace InteropTools.ShellPages.Registry
{
	public sealed partial class DefaultAppsPage
    {
        public string PageName => "Default Apps";
        public PageGroup PageGroup => PageGroup.General;

        private readonly IRegistryProvider _helper;

		private readonly ObservableCollection<AppAssotiationItem> _itemlist =
		  new ObservableCollection<AppAssotiationItem>();

		private readonly string _resUnknown = ResourceManager.Current.MainResourceMap.GetValue("Resources/Unknown", ResourceContext.GetForCurrentView()).
		                                      ValueAsString;

		private readonly string _resNeutral =
		  ResourceManager.Current.MainResourceMap.GetValue(
		    "Resources/Neutral", ResourceContext.GetForCurrentView()).ValueAsString;

		public DefaultAppsPage()
		{
			InitializeComponent();
			_helper = App.MainRegistryHelper;
            Load();
		}
        
		private static async Task RunInUiThread(Action function)
		{
			await
			CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() => { function(); });
		}

		private static async void RunInThreadPool(Action function)
		{
			await ThreadPool.RunAsync(x => { function(); });
		}

		private void Load()
		{
			RunInThreadPool(async () =>
			{
				var items = await _helper.GetRegistryItems2(RegHives.HKEY_LOCAL_MACHINE,
				                                     @"SOFTWARE\Microsoft\DefaultApplications");
				await RunInUiThread(() =>
				{
					LoadingBar.Minimum = 0;
					LoadingBar.Maximum = items.Count;
				});
				var counter = -1;

				foreach (var item in items)
				{
					counter++;
					await RunInUiThread(() => { LoadingBar.Value = counter; });
					var appasso = new AppAssotiationItem {Extension = item.Name};
					var listasso = new List<AppAssotiation>();
					RegTypes regtype;
					string regvalue;
					var ret = await _helper.GetKeyValue(RegHives.HKEY_CLASSES_ROOT, item.Name, "Content Type",
					                    RegTypes.REG_SZ);
                    regtype = ret.regtype;
                    regvalue = ret.regvalue;
					appasso.Description = regvalue;
					var items2 = await _helper.GetRegistryItems2(RegHives.HKEY_CURRENT_USER, @"Software\Classes\" + item.Name + @"\OpenWithProgids");

					if (items2 != null)
					{
						foreach (var item2 in items2)
						{
							var appAssotiation = new AppAssotiation();
							var items3 = await _helper.GetRegistryItems2(RegHives.HKEY_CURRENT_USER,
							                                      @"Software\Classes\" + item2.Name + @"\Application");

							var add
								  = true;

							foreach (var item3 in items3)
							{
								switch (item3.Name)
								{
									case "ApplicationName":
										{
											appAssotiation.Title = item3.Value;
											break;
										}

									case "ApplicationDescription":
										{
											appAssotiation.Description = item3.Value;
											break;
										}

									case "AppUserModelID":
										{
											if (item3.Value.Contains("!"))
											{
												foreach (var listAssoItem in listasso)
												{
													if (listAssoItem.Launchuri.ToLower() ==
													    item3.Value.Split('!')[0].ToLower())
													{
														Debug.WriteLine("First problem");

														add
															  = false;
													}
												}

												if (add)
												{
													if (item3.Value.Split('!')[0].ToLower() == item.Value.ToLower())
													{
														appasso.Defaultapp = listasso.Count;
													}
												}

												appAssotiation.Launchuri = item3.Value.Split('!')[0];
											}

											else
											{
												foreach (var listAssoItem in listasso)
												{
													if (listAssoItem.Launchuri.ToLower() == item3.Value.ToLower())
													{
														Debug.WriteLine("Second problem");

														add
															  = false;
													}
												}

												if (add)
												{
													if (item3.Value.ToLower() == item.Value.ToLower())
													{
														appasso.Defaultapp = listasso.Count;
													}

													appAssotiation.Launchuri = item3.Value;
												}
											}

											break;
										}
								}
							}

							if (appAssotiation.Title == null)
							{
								appAssotiation.Title = appAssotiation.Launchuri;
							}

							if (add)
							{
								Debug.WriteLine("new object " + appAssotiation.Title);
								listasso.Add(appAssotiation);
							}
						}
					}

					appasso.Applist = listasso;

					if (listasso.Count != 0)
					{
						await RunInUiThread(() => { _itemlist.Add(appasso); });
					}
				}

				try
				{
					var pfnlist = (from item in _itemlist from item2 in item.Applist select item2.Launchuri.ToLower()).ToList();
					var packages = new List<Packageinfos>();
					var pkgman = new PackageManager();
					var applist = new ObservableRangeCollection<Package>();
					applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.None));
					applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.Bundle));
					applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.Framework));
					applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.Main));

					try
					{
						applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.Optional));
					}

					catch
					{
					}

					applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.Resource));
					applist.AddRange(pkgman.FindPackagesForUserWithPackageTypes("", PackageTypes.Xap));
					var pkgs = new List<Package>();

					foreach (var item in applist)
						if (!pkgs.Contains(item))
						{
							pkgs.Add(item);
						}

					pkgs = pkgs.OrderBy(x => x.Id.FamilyName).ToList();

					foreach (var package in pkgs)
					{
						if (pfnlist.Contains(package.Id.FamilyName.ToLower()))
						{
							var app = new Packageinfos { Packagefamillyname = package.Id.FamilyName.ToLower() };
							var arch = _resUnknown;

							switch (package.Id.Architecture)
							{
								case ProcessorArchitecture.Arm:
									{
										arch = "ARM";
										break;
									}

								case ProcessorArchitecture.Neutral:
									{
										arch = _resNeutral;
										break;
									}

								case ProcessorArchitecture.Unknown:
									{
										arch = _resUnknown;
										break;
									}

								case ProcessorArchitecture.X64:
									{
										arch = "x64";
										break;
									}

								case ProcessorArchitecture.X86:
									{
										arch = "x86";
										break;
									}
							}

							app.Title = package.Id.FamilyName;
							app.Description = arch + " " + package.Id.Version.Major + "." + package.Id.Version.Minor + "." +
							                  package.Id.Version.Build + "." + package.Id.Version.Revision;

							try
							{
								var appEntries = await package.GetAppListEntriesAsync();

								foreach (var appEntry in appEntries)
								{
									try
									{
										app.Title = appEntry.DisplayInfo.DisplayName;
									}

									catch
									{
										// ignored
									}

									try
									{
										app.Description = appEntry.DisplayInfo.Description + "\n" + arch + " " +
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
											Height = 48,
											Width = 48
										};
										var applogo = await appEntry.DisplayInfo.GetLogo(logosize).OpenReadAsync();
                                        await RunInUiThread(() => 
                                        {
                                            var bitmapImage = new BitmapImage();
                                            bitmapImage.SetSource(applogo);

                                            app.logo = bitmapImage;
                                        });
									}

									catch
									{
										// ignored
									}
								}
							}
							catch
							{
								// ignored
							}

							packages.Add(app);
						}
					}

					foreach (var package in packages)
					{
						var maincounter = -1;

						foreach (var item in _itemlist)
						{
							maincounter++;
							var counter2 = -1;

							foreach (var item2 in item.Applist)
							{
								counter2++;

								if (!string.Equals(item2.Launchuri, package.Packagefamillyname, StringComparison.CurrentCultureIgnoreCase))
								{
									continue;
								}

								var maincounter1 = maincounter;
								var counter3 = counter2;
								_itemlist[maincounter1].Applist[counter3].Description = package.Description;
								_itemlist[maincounter1].Applist[counter3].logo = package.logo;
								_itemlist[maincounter1].Applist[counter3].Title = package.Title;
							}
						}
					}

					await RunInUiThread(() =>
					{
						FileAssociationsListView.ItemsSource = _itemlist;
						LoadingRing.IsActive = false;
						ProgressPanel.Visibility = Visibility.Collapsed;
					});
				}

				catch
				{
				}
			});
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = (AppAssotiationItem) ((ComboBox) sender).DataContext;
			var selectedItem = (AppAssotiation) ((ComboBox) sender).SelectedItem;

			if (selectedItem != null)
			{
				_helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\DefaultApplications", item.Extension,
				                    RegTypes.REG_SZ, selectedItem.Launchuri);
			}
		}

		private class AppAssotiationItem
		{
			public string Extension { get; internal set; }
			public string Description { get; internal set; }
			public int Defaultapp { get; internal set; }
			public List<AppAssotiation> Applist { get; internal set; }
		}

		private class AppAssotiation
		{
			public string Title { get; internal set; }
			public string Description { get; internal set; }
			public dynamic logo { get; internal set; }
			public string Launchuri { get; internal set; }
		}

		private class Packageinfos
		{
			public string Title { get; internal set; }
			public string Description { get; internal set; }
			public dynamic logo { get; internal set; }
			public string Packagefamillyname { get; internal set; }
		}
	}
}