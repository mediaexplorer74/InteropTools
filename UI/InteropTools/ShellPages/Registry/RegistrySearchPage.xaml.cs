﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using InteropTools.Providers;
using Windows.ApplicationModel.Resources.Core;
using Microsoft.Toolkit.Uwp;
using System.Threading.Tasks;

using InteropTools.CorePages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InteropTools.ShellPages.Registry
{
	/// <summary>
	///     An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class RegistrySearchPage
    {
        public string PageName => "Registry Search";
        public PageGroup PageGroup => PageGroup.Registry;

        private RegHives _currentHive = RegHives.HKEY_CLASSES_ROOT;

		private string _currentKey = "";

		private readonly ObservableCollection<FilterItem> _filterItemsList = new ObservableCollection<FilterItem>();

		private readonly IRegistryProvider _helper;

		private static readonly ObservableRangeCollection<Item> _itemsList = new ObservableRangeCollection<Item>();
		private bool _paused;
		private bool _stopped = true;

		private readonly string _resourcesKey =
		  ResourceManager.Current.MainResourceMap.GetValue("Resources/Key",
		      ResourceContext.GetForCurrentView()).ValueAsString;

		private readonly string _resourcesHive =
		  ResourceManager.Current.MainResourceMap.GetValue("Resources/Hive",
		      ResourceContext.GetForCurrentView()).ValueAsString;

		private readonly string _resourcesBinary = ResourceManager.Current.MainResourceMap.GetValue("Resources/Binary", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesHardwareResourceList = ResourceManager.Current.MainResourceMap.GetValue("Resources/Hardware_Resource_List", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesInteger = ResourceManager.Current.MainResourceMap.GetValue("Resources/Integer", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesIntegerBigEndian = ResourceManager.Current.MainResourceMap.GetValue("Resources/Integer_Big_Endian", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesLong = ResourceManager.Current.MainResourceMap.GetValue("Resources/Long", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesMultiString = ResourceManager.Current.MainResourceMap.GetValue("Resources/Multi_String", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesNone = ResourceManager.Current.MainResourceMap.GetValue("Resources/None", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesResourceList = ResourceManager.Current.MainResourceMap.GetValue("Resources/Resource_List", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesResourceRequirement = ResourceManager.Current.MainResourceMap.GetValue("Resources/Resource_Requirement", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesString = ResourceManager.Current.MainResourceMap.GetValue("Resources/String", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesSymbolicLink = ResourceManager.Current.MainResourceMap.GetValue("Resources/Symbolic_Link", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesVariableString = ResourceManager.Current.MainResourceMap.GetValue("Resources/Variable_String", ResourceContext.GetForCurrentView()).ValueAsString;
		private readonly string _resourcesUnknown = ResourceManager.Current.MainResourceMap.GetValue("Resources/Unknown", ResourceContext.GetForCurrentView()).ValueAsString;

		public RegistrySearchPage()
		{
			InitializeComponent();
			_helper = App.MainRegistryHelper;
            Refresh();
		}

		private void Refresh()
		{
			MainPivot.IsLocked = true;
			ResultsListView.ItemsSource = _itemsList;
			FilterListBox.ItemsSource = _filterItemsList;
			_filterItemsList.Add(new FilterItem
			{
				FilterTypeIndex = 0,
				FilterModeIndex = 0,
				FilterText = "",
				FuzzynessModeIndex = 0,
				ValueTypeIndex = 9,
				HiveIndex = 5
			});
		}

		private void SearchStart_Click(object sender, RoutedEventArgs e)
		{
			if (MainPivot.SelectedIndex != 0)
			{
				return;
			}

			for (var i = _itemsList.Count - 1; i >= 0; i--)
			{
				_itemsList.RemoveAt(i);
			}

			MainPivot.SelectedIndex = 1;
			SearchStart.Visibility = Visibility.Collapsed;
			SearchStop.Visibility = Visibility.Visible;
			SearchPause.Visibility = Visibility.Visible;
			AddFilter.Visibility = Visibility.Collapsed;
			_stopped = false;
			StatusBar.Visibility = Visibility.Visible;
			Search();
		}

		private bool CheckFilters(Item item)
		{
			var filterlist = _filterItemsList.ToList();
			//With fuzzy option
			//Key Name is
			var keyNameIs = filterlist.FindAll(x => (x.FilterTypeIndex == 0) && (x.FilterModeIndex == 0));
			//Key Name is not
			var keyNameIsNot = filterlist.FindAll(x => (x.FilterTypeIndex == 0) && (x.FilterModeIndex == 1));
			//Value Name is
			var valueNameIs = filterlist.FindAll(x => (x.FilterTypeIndex == 1) && (x.FilterModeIndex == 0));
			//Value Name is not
			var valueNameIsNot = filterlist.FindAll(x => (x.FilterTypeIndex == 1) && (x.FilterModeIndex == 1));
			//Value Data is
			var valueDataIs = filterlist.FindAll(x => (x.FilterTypeIndex == 3) && (x.FilterModeIndex == 0));
			//Value Data is not
			var valueDataIsNot = filterlist.FindAll(x => (x.FilterTypeIndex == 3) && (x.FilterModeIndex == 1));
			//Path is
			var pathIs = filterlist.FindAll(x => (x.FilterTypeIndex == 4) && (x.FilterModeIndex == 0));
			//Path is not
			var pathIsNot = filterlist.FindAll(x => (x.FilterTypeIndex == 4) && (x.FilterModeIndex == 1));
			//No fuzzy option
			//Value Type is
			var valueTypeIs = filterlist.FindAll(x => (x.FilterTypeIndex == 2) && (x.FilterModeIndex == 0));
			//Value Type is not
			var valueTypeIsNot = filterlist.FindAll(x => (x.FilterTypeIndex == 2) && (x.FilterModeIndex == 1));
			//Hive is
			var hiveIs = filterlist.FindAll(x => (x.FilterTypeIndex == 5) && (x.FilterModeIndex == 0));
			//Hive is not
			var hiveIsNot = filterlist.FindAll(x => (x.FilterTypeIndex == 5) && (x.FilterModeIndex == 1));

			switch (item.Type)
			{
				case RegistryItemType.HIVE:
					{
						if (FindHive.IsChecked != null && (bool) FindHive.IsChecked)
						{
							if (CheckFilter(hiveIs, item, false) && CheckFilter(hiveIsNot, item, true))
							{ return true; }
						}

						break;
					}

				case RegistryItemType.KEY:
					{
						if (FindKey.IsChecked != null && (bool) FindKey.IsChecked)
							if (CheckFilter(keyNameIs, item, false) && CheckFilter(keyNameIsNot, item, true) &&
							    CheckFilter(pathIs, item, false) && CheckFilter(pathIsNot, item, true) &&
							    CheckFilter(hiveIs, item, false) && CheckFilter(hiveIsNot, item, true))
							{
								return true;
							}

						break;
					}

				case RegistryItemType.VALUE:
					{
						if (FindValue.IsChecked != null && (bool) FindValue.IsChecked)
						{
							if (CheckFilter(valueNameIs, item, false) && CheckFilter(valueNameIsNot, item, true) &&
							    CheckFilter(valueDataIs, item, false) && CheckFilter(valueDataIsNot, item, true) &&
							    CheckFilter(pathIs, item, false) && CheckFilter(pathIsNot, item, true) &&
							    CheckFilter(valueTypeIs, item, false) && CheckFilter(valueTypeIsNot, item, true) &&
							    CheckFilter(hiveIs, item, false) && CheckFilter(hiveIsNot, item, true))
							{ return true; }
						}

						break;
					}
			}

			return false;
		}

		private static bool CheckFilter(IReadOnlyCollection<FilterItem> filters, Item item, bool inverted)
		{
			if (filters == null)
			{
				return true;
			}

			if (filters.Count == 0)
			{
				return true;
			}

			var result = false;

			foreach (var filter in filters)
			{
				if (result)
				{
					break;
				}

				switch (filter.FilterTypeIndex)
				{
					//KeyName
					case 0:

					//ValueName
					case 1:
						{
							switch (filter.FuzzynessModeIndex)
							{
								//Strict (Case sensitive)
								case 0:
									{
										if (item.Name == filter.FilterText)
										{
											result = true;
										}

										break;
									}

								//Partial (Case sensitive)
								case 1:
									{
										if (item.Name.Contains(filter.FilterText))
										{
											result = true;
										}

										break;
									}

								//Strict (Case insensitive)
								case 2:
									{
										if (item.Name.ToLower() == filter.FilterText.ToLower())
										{
											result = true;
										}

										break;
									}

								//Partial (Case insensitive)
								case 3:
									{
										if (item.Name.ToLower().Contains(filter.FilterText.ToLower()))
										{
											result = true;
										}

										break;
									}
							}

							break;
						}

					//ValueType
					case 2:
						{
							if ((uint)GetSelectedType(filter.ValueTypeIndex) == item.ValueType)
							{
								result = true;
							}

							break;
						}

					//ValueData
					case 3:
						{
							switch (filter.FuzzynessModeIndex)
							{
								//Strict (Case sensitive)
								case 0:
									{
										if (item.Value == filter.FilterText)
										{
											result = true;
										}

										break;
									}

								//Partial (Case sensitive)
								case 1:
									{
										if (item.Value.Contains(filter.FilterText))
										{
											result = true;
										}

										break;
									}

								//Strict (Case insensitive)
								case 2:
									{
										if (item.Value.ToLower() == filter.FilterText.ToLower())
										{
											result = true;
										}

										break;
									}

								//Partial (Case insensitive)
								case 3:
									{
										if (item.Value.ToLower().Contains(filter.FilterText.ToLower()))
										{
											result = true;
										}

										break;
									}
							}

							break;
						}

					//Path
					case 4:
						{
							switch (filter.FuzzynessModeIndex)
							{
								//Strict (Case sensitive)
								case 0:
									{
										if (item.Key == filter.FilterText)
										{
											result = true;
										}

										break;
									}

								//Partial (Case sensitive)
								case 1:
									{
										if (item.Key.Contains(filter.FilterText))
										{
											result = true;
										}

										break;
									}

								//Strict (Case insensitive)
								case 2:
									{
										if (item.Key.ToLower() == filter.FilterText.ToLower())
										{
											result = true;
										}

										break;
									}

								//Partial (Case insensitive)
								case 3:
									{
										if (item.Key.ToLower().Contains(filter.FilterText.ToLower()))
										{
											result = true;
										}

										break;
									}
							}

							break;
						}

					//Hive
					case 5:
						{
							if (GetSelectedHive(filter.HiveIndex) == item.Hive)
							{
								result = true;
							}

							break;
						}
				}
			}

			if (inverted)
			{
				return !result;
			}

			return result;
		}

		private static RegHives GetSelectedHive(int index)
		{
			var hive = RegHives.HKEY_LOCAL_MACHINE;
			var selectedhiveindex = index;

			switch (selectedhiveindex)
			{
				case 0:
					{
						hive = RegHives.HKEY_CURRENT_CONFIG;
						break;
					}

				case 1:
					{
						hive = RegHives.HKEY_CLASSES_ROOT;
						break;
					}

				case 2:
					{
						hive = RegHives.HKEY_CURRENT_USER;
						break;
					}

				case 3:
					{
						hive = RegHives.HKEY_CURRENT_USER_LOCAL_SETTINGS;
						break;
					}

				case 4:
					{
						hive = RegHives.HKEY_DYN_DATA;
						break;
					}

				case 5:
					{
						hive = RegHives.HKEY_LOCAL_MACHINE;
						break;
					}

				case 6:
					{
						hive = RegHives.HKEY_PERFORMANCE_DATA;
						break;
					}

				case 7:
					{
						hive = RegHives.HKEY_USERS;
						break;
					}
			}

			return hive;
		}

		private static RegTypes GetSelectedType(int index)
		{
			switch (index)
			{
				case 0:
					{
						return RegTypes.REG_BINARY;
					}

				case 1:
					{
						return RegTypes.REG_FULL_RESOURCE_DESCRIPTOR;
					}

				case 2:
					{
						return RegTypes.REG_DWORD;
					}

				case 3:
					{
						return RegTypes.REG_DWORD_BIG_ENDIAN;
					}

				case 4:
					{
						return RegTypes.REG_QWORD;
					}

				case 5:
					{
						return RegTypes.REG_MULTI_SZ;
					}

				case 6:
					{
						return RegTypes.REG_NONE;
					}

				case 7:
					{
						return RegTypes.REG_RESOURCE_LIST;
					}

				case 8:
					{
						return RegTypes.REG_RESOURCE_REQUIREMENTS_LIST;
					}

				case 9:
					{
						return RegTypes.REG_SZ;
					}

				case 10:
					{
						return RegTypes.REG_LINK;
					}

				case 11:
					{
						return RegTypes.REG_EXPAND_SZ;
					}
			}

			return RegTypes.REG_ERROR;
		}

		private void SearchStop_Click(object sender, RoutedEventArgs e)
		{
			if (MainPivot.SelectedIndex == 1)
			{
				MainPivot.SelectedIndex = 0;
				SearchStop.Visibility = Visibility.Collapsed;
				SearchPause.Visibility = Visibility.Collapsed;
				SearchStart.Visibility = Visibility.Visible;
				_stopped = true;
				_paused = false;
				SearchPause.IsChecked = false;
				Status.Text = "Idle";
				StatusBar.Visibility = Visibility.Collapsed;
				AddFilter.Visibility = Visibility.Visible;

				for (var i = _itemsList.Count - 1; i >= 0; i--)
				{
					_itemsList.RemoveAt(i);
				}
			}
		}

		private async Task<string> FindBack(RegHives hive, string key)
		{
			while (true)
			{
				if (key.Contains("\\"))
				{
					var newkey = "";
					var counter = -1;
					var lastkey = "";

					foreach (var s in key.Split(char.Parse("\\")))
					{
						counter++;
						lastkey = s;
					}

					var runcount = -1;

					foreach (var s in key.Split(char.Parse("\\")))
					{
						runcount++;

						if (runcount == counter)
						{
							break;
						}

						if (newkey == "")
						{
							newkey = s;
						}

						else
						{
							newkey = newkey + "\\" + s;
						}
					}

					var tmpitems = (List<RegistryItemCustom>) await _helper.GetRegistryItems2(hive, newkey);
					var tmpkeys = tmpitems.FindAll(i => i.Type == RegistryItemType.KEY);

					if (tmpkeys.IndexOf(tmpkeys.Find(i => i.Name == lastkey)) + 1 == tmpkeys.Count)
					{
						key = newkey;
						continue;
					}

					if (newkey == "")
					{
						newkey = tmpkeys[tmpkeys.IndexOf(tmpkeys.Find(i => i.Name == lastkey)) + 1].Name;
					}

					else
					{
						newkey = newkey + "\\" + tmpkeys[tmpkeys.IndexOf(tmpkeys.Find(i => i.Name == lastkey)) + 1].Name;
					}

					return newkey;
				}

				{
					var tmpitems = (List<RegistryItemCustom>) await _helper.GetRegistryItems2(hive, "");
					var tmpkeys = tmpitems.FindAll(i => i.Type == RegistryItemType.KEY);

					if (tmpkeys.IndexOf(tmpkeys.Find(i => i.Name == key)) + 1 == tmpkeys.Count)
					{
						return null;
					}

					var newkey = tmpkeys[tmpkeys.IndexOf(tmpkeys.Find(i => i.Name == key)) + 1].Name;
					return newkey;
				}
			}
		}

		private async Task SearchHive(RegHives hive)
		{
			var parse = true;
			var key = "";

			while (parse)
			{
				if (_stopped)
				{
					break;
				}

				if (_paused)
				{
					_currentKey = key;
					_currentHive = hive;
					break;
				}

				RunInUiThread(() =>
				{
					if (key == "")
					{
						Status.Text = hive.ToString();
					}

					else
					{
						Status.Text = hive + "\\" + key;
					}
				});
				var items = (List<RegistryItemCustom>) await _helper.GetRegistryItems2(hive, key);

				foreach (var item in items)
				{
					Item newItem = null;

					switch (item.Type)
					{
						case RegistryItemType.KEY:
							{
								newItem = new Item
								{
									DisplayName = item.Name,
									Symbol = "",
									Description = _resourcesKey,
									Hive = item.Hive,
									Key = item.Key,
									Name = item.Name,
									Type = RegistryItemType.KEY,
									Value = "N/A",
									ValueType = item.ValueType,
									DisplayHive = hive.ToString()
								};
								break;
							}

						case RegistryItemType.VALUE:
							{
								if (item.Name == "")
								{
									newItem = new Item
									{
										DisplayName = "(default)",
										Symbol = "",
										Description = GetValueTypeName(item.ValueType),
										Hive = item.Hive,
										Key = item.Key,
										Name = item.Name,
										Type = RegistryItemType.VALUE,
										Value = item.Value,
										ValueType = item.ValueType,
										DisplayHive = item.Hive.ToString()
									};
								}

								else
								{
									newItem = new Item
									{
										DisplayName = item.Name,
										Symbol = "",
										Description = GetValueTypeName(item.ValueType),
										Hive = item.Hive,
										Key = item.Key,
										Name = item.Name,
										Type = RegistryItemType.VALUE,
										Value = item.Value,
										ValueType = item.ValueType,
										DisplayHive = item.Hive.ToString()
									};
								}

								break;
							}
					}

					RunInUiThread(() =>
					{
						if (CheckFilters(newItem))
						{
							_itemsList.Add(newItem);
						}
					});
				}

				var keys = items.FindAll(i => i.Type == RegistryItemType.KEY);

				if (keys.Count == 0)
				{
					var result = await FindBack(hive, key);

					if (result != null)
					{
						key = result;
					}

					else
					{
						parse = false;
					}
				}

				else
				{
					key = keys[0].Key + "\\" + keys[0].Name;
				}
			}
		}

		private void Search()
		{
			RunInThreadPool(async () =>
			{
				var items = (List<RegistryItemCustom>) await _helper.GetRegistryHives2();

				foreach (var item in items)
				{
					if (_paused)
					{
						break;
					}

					var newitem = new Item
					{
						DisplayName = item.Name,
						Symbol = "",
						Description = _resourcesHive,
						Hive = item.Hive,
						Key = item.Key,
						Name = item.Name,
						Type = RegistryItemType.HIVE,
						Value = "N/A",
						ValueType = item.ValueType,
						DisplayHive = item.Hive.ToString()
					};
                    
					RunInUiThread(() =>
					{
						if (!CheckFilters(newitem))
						{
							return;
						}

						if (FindHive.IsChecked != null && (bool) FindHive.IsChecked)
						{
							_itemsList.Add(newitem);
						}
					});
					await SearchHive(item.Hive);
				}
			});
		}

		private async Task SearchHive(RegHives hive, string Key, bool startfromkey)
		{
			var parse = true;
			var key = "";

			if (startfromkey)
			{
				key = Key;
			}

			while (parse)
			{
				if (_stopped)
				{
					break;
				}

				if (_paused)
				{
					_currentKey = key;
					_currentHive = hive;
					break;
				}

				RunInUiThread(() =>
				{
					if (key == "")
					{
						Status.Text = hive.ToString();
					}

					else
					{
						Status.Text = hive + "\\" + key;
					}
				});
				var items = (List<RegistryItemCustom>) await _helper.GetRegistryItems2(hive, key);

				foreach (var item in items)
				{
					Item newItem = null;

					switch (item.Type)
					{
						case RegistryItemType.KEY:
							{
								newItem = new Item
								{
									DisplayName = item.Name,
									Symbol = "",
									Description = _resourcesKey,
									Hive = item.Hive,
									Key = item.Key,
									Name = item.Name,
									Type = RegistryItemType.KEY,
									Value = "N/A",
									ValueType = item.ValueType,
									DisplayHive = hive.ToString()
								};
								break;
							}

						case RegistryItemType.VALUE:
							{
								if (item.Name == "")
								{
									newItem = new Item
									{
										DisplayName = "(default)",
										Symbol = "",
										Description = GetValueTypeName(item.ValueType),
										Hive = item.Hive,
										Key = item.Key,
										Name = item.Name,
										Type = RegistryItemType.VALUE,
										Value = item.Value,
										ValueType = item.ValueType,
										DisplayHive = item.Hive.ToString()
									};
								}

								else
								{
									newItem = new Item
									{
										DisplayName = item.Name,
										Symbol = "",
										Description = GetValueTypeName(item.ValueType),
										Hive = item.Hive,
										Key = item.Key,
										Name = item.Name,
										Type = RegistryItemType.VALUE,
										Value = item.Value,
										ValueType = item.ValueType,
										DisplayHive = item.Hive.ToString()
									};
								}

								break;
							}
					}

					RunInUiThread(() =>
					{
						if (CheckFilters(newItem))
						{
							_itemsList.Add(newItem);
						}
					});
				}

				var keys = items.FindAll(i => i.Type == RegistryItemType.KEY);

				if (keys.Count == 0)
				{
					var result = await FindBack(hive, key);

					if (result != null)
					{
						key = result;
					}

					else
					{
						parse = false;
					}
				}

				else
					if (key != null)
					{
						key = keys[0].Key + "\\" + keys[0].Name;
					}
			}
		}

		private void Search(string key)
		{
			RunInThreadPool(async () =>
			{
				var items = (List<RegistryItemCustom>) await _helper.GetRegistryHives2();
				var In = false;

				foreach (var item in items)
				{
					if (_paused)
					{
						break;
					}

					if (!In && (item.Hive != _currentHive))
					{
						continue;
					}

					var newitem = new Item
					{
						DisplayName = item.Name,
						Symbol = "",
						Description = _resourcesHive,
						Hive = item.Hive,
						Key = item.Key,
						Name = item.Name,
						Type = RegistryItemType.HIVE,
						Value = "N/A",
						ValueType = item.ValueType,
						DisplayHive = item.Hive.ToString()
					};
					RunInUiThread(() =>
					{
						if (!CheckFilters(newitem))
						{
							return;
						}

						if (FindHive.IsChecked != null && (bool) FindHive.IsChecked)
						{
							_itemsList.Add(newitem);
						}
					});
					await SearchHive(item.Hive, key, !In);
					In = true;
				}
			});
		}

		private string GetValueTypeName(uint type)
		{
			switch (type)
			{
				case (uint)RegTypes.REG_BINARY:
					{
						return _resourcesBinary;
					}

				case (uint)RegTypes.REG_FULL_RESOURCE_DESCRIPTOR:
					{
						return _resourcesHardwareResourceList;
					}

				case (uint)RegTypes.REG_DWORD:
					{
						return _resourcesInteger;
					}

				case (uint)RegTypes.REG_DWORD_BIG_ENDIAN:
					{
						return _resourcesIntegerBigEndian;
					}

				case (uint)RegTypes.REG_QWORD:
					{
						return _resourcesLong;
					}

				case (uint)RegTypes.REG_MULTI_SZ:
					{
						return _resourcesMultiString;
					}

				case (uint)RegTypes.REG_NONE:
					{
						return _resourcesNone;
					}

				case (uint)RegTypes.REG_RESOURCE_LIST:
					{
						return _resourcesResourceList;
					}

				case (uint)RegTypes.REG_RESOURCE_REQUIREMENTS_LIST:
					{
						return _resourcesResourceRequirement;
					}

				case (uint)RegTypes.REG_SZ:
					{
						return _resourcesString;
					}

				case (uint)RegTypes.REG_LINK:
					{
						return _resourcesSymbolicLink;
					}

				case (uint)RegTypes.REG_EXPAND_SZ:
					{
						return _resourcesVariableString;
					}
                default:
                    {
                        return "Custom: " + type;
                    }
            }
		}

		private async void RunInUiThread(Action function)
		{
			await
			CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() => { function(); });
		}

		private async void RunInThreadPool(Action function)
		{
			await ThreadPool.RunAsync(x => { function(); });
		}

		private void ResultsListView_ItemClick(object sender, ItemClickEventArgs e)
		{
		}

		private void AddFilter_Click(object sender, RoutedEventArgs e)
		{
			_filterItemsList.Add(new FilterItem
			{
				FilterTypeIndex = 0,
				FilterModeIndex = 0,
				FilterText = "",
				FuzzynessModeIndex = 0,
				ValueTypeIndex = 9,
				HiveIndex = 5
			});
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var mainPanel = (StackPanel) ((ComboBox) sender).Parent;
			var textFilter = (StackPanel) mainPanel.Children[2];
			var valueTypeFilter = (ComboBox) mainPanel.Children[3];
			var hiveFilter = (ComboBox) mainPanel.Children[4];

			if (((ComboBox) sender).SelectedIndex == 2)
			{
				textFilter.Visibility = Visibility.Collapsed;
				valueTypeFilter.Visibility = Visibility.Visible;
				hiveFilter.Visibility = Visibility.Collapsed;
			}

			else
				if (((ComboBox) sender).SelectedIndex == 5)
				{
					textFilter.Visibility = Visibility.Collapsed;
					valueTypeFilter.Visibility = Visibility.Collapsed;
					hiveFilter.Visibility = Visibility.Visible;
				}

				else
				{
					textFilter.Visibility = Visibility.Visible;
					valueTypeFilter.Visibility = Visibility.Collapsed;
					hiveFilter.Visibility = Visibility.Collapsed;
				}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var item = (FilterItem) ((Button) sender).DataContext;
			_filterItemsList.Remove(item);
		}

		private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var sndr = (StackPanel) sender;
			var item = (Item) sndr.DataContext;
			var flyout = new MenuFlyout {Placement = FlyoutPlacementMode.Top};
			var flyoutitem = new MenuFlyoutItem
			{
				Text =
				ResourceManager.Current.MainResourceMap.GetValue("Resources/Copy_name",
				ResourceContext.GetForCurrentView()).ValueAsString
			};
			flyoutitem.Click += (sender_, e_) =>
			{
				var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
				dataPackage.SetText(item.Name);
				Clipboard.SetContent(dataPackage);
			};
			var flyoutitem2 = new MenuFlyoutItem
			{
				Text =
				ResourceManager.Current.MainResourceMap.GetValue("Resources/Copy_key_location",
				ResourceContext.GetForCurrentView()).ValueAsString
			};

			if ((item.Key == null) || (item.Key == ""))
			{
				flyoutitem2.IsEnabled = false;
			}

			flyoutitem2.Click += (sender_, e_) =>
			{
				if ((item.Key == null) || (item.Key == ""))
				{
					return;
				}

				var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
				dataPackage.SetText(item.Key);
				Clipboard.SetContent(dataPackage);
			};
			var flyoutitem3 = new MenuFlyoutItem
			{
				Text =
				ResourceManager.Current.MainResourceMap.GetValue("Resources/Copy_hive_name",
				ResourceContext.GetForCurrentView()).ValueAsString
			};
			flyoutitem3.Click += (sender_, e_) =>
			{
				var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
				dataPackage.SetText(item.Hive.ToString());
				Clipboard.SetContent(dataPackage);
			};
			var flyoutitem4 = new MenuFlyoutItem
			{
				Text =
				ResourceManager.Current.MainResourceMap.GetValue("Resources/Copy_full_details",
				ResourceContext.GetForCurrentView()).ValueAsString
			};
			flyoutitem4.Click += (sender_, e_) =>
			{
				var str = "";

				switch (item.Type)
				{
					case RegistryItemType.HIVE:
						{
							str = string.Format("Name: {0}\r\nType: {1}", item.Name, item.Type.ToString());
							break;
						}

					case RegistryItemType.KEY:
						{
							str =
							  $"[{item.Key}]\r\nName: {item.Name}\r\nType: {item.Type.ToString()}\r\nHive: {item.Hive.ToString()}";
							break;
						}

					case RegistryItemType.VALUE:
						{
							str =
							  $"[{item.Key}]\r\nName: {item.Name}\r\nType: {item.Type.ToString()}\r\nHive: {item.Hive.ToString()}\r\nValue Type: {item.ValueType.ToString()}\r\nValue: {item.Value}";
							break;
						}
				}

				if (str == "")
				{
					return;
				}

				var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
				dataPackage.SetText(str);
				Clipboard.SetContent(dataPackage);
			};

			if (flyout.Items != null)
			{
				flyout.Items.Add(flyoutitem);
				flyout.Items.Add(flyoutitem2);
				flyout.Items.Add(flyoutitem3);
				flyout.Items.Add(flyoutitem4);
			}

			flyout.ShowAt((StackPanel) sender, e.GetPosition((StackPanel) sender));
		}

		private void SearchPause_Checked(object sender, RoutedEventArgs e)
		{
			_paused = true;
		}

		private void SearchPause_Unchecked(object sender, RoutedEventArgs e)
		{
			_paused = false;
			Search(_currentKey);
		}

		public class Item
		{
			public string Name { get; set; }
			public RegistryItemType Type { get; set; }

			public RegHives Hive { get; set; }

			public string Key { get; set; }

			public string Value { get; set; }
			public uint ValueType { get; set; }

			public string Symbol { get; set; }
			public string DisplayName { get; set; }
			public string Description { get; set; }
			public string DisplayHive { get; set; }
		}

		public class FilterItem
		{
			public int FilterTypeIndex { get; set; } // 0 -> 5
			public int FilterModeIndex { get; set; } // 0 -> 1
			public string FilterText { get; set; }
			public int FuzzynessModeIndex { get; set; } // 0 -> 3
			public int ValueTypeIndex { get; set; } // 0 -> 11
			public int HiveIndex { get; set; } // 0 -> 7
		}
	}
}