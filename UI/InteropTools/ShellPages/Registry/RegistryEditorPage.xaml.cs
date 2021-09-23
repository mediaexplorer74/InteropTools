﻿using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using InteropTools.Providers;
using Windows.ApplicationModel.Resources.Core;
using InteropTools.Classes;
using InteropTools.CorePages;
using InteropTools.ContentDialogs.Registry;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media;
using System.Linq;

namespace InteropTools.ShellPages.Registry
{
	public sealed partial class RegistryEditorPage
    {
        public string PageName => "Registry Editor";
        public PageGroup PageGroup => PageGroup.Registry;

        private readonly IRegistryProvider _helper;

		private readonly ObservableCollection<RegistryHistoryItem> _registryHistoryList =
		  new ObservableCollection<RegistryHistoryItem>();

		private readonly ObservableCollection<SuggestionItem> _suggestionList =
		  new ObservableCollection<SuggestionItem>();

		private readonly ObservableCollection<SuggestionItem> _valSuggestionList =
		  new ObservableCollection<SuggestionItem>();

		private bool initialized = false;

		public RegistryEditorPage()
		{
			InitializeComponent();
            _helper = App.MainRegistryHelper;
            
            Refresh();
            Window.Current.SizeChanged += Current_SizeChanged;
            SizeChanged += RegistryEditorPage_SizeChanged;

            Loaded += RegistryEditorPage_Loaded;
		}

        private void RegistryEditorPage_Loaded(object sender, RoutedEventArgs f)
        {
            var Size = Window.Current.Bounds;

            if (Size.Width >= 720)
            {
                HistoryBackground.Visibility = Visibility.Visible;
                Grid.SetColumn(EditorUI, 1);
                Grid.SetRow(HistoryUI, 0);
                SecondaryColumn.Width = new GridLength(328);
                OtherColumn.Width = new GridLength(1, GridUnitType.Star);
                //MainRow.Height = new GridLength(1, GridUnitType.Star);
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                HistoryBackground.Visibility = Visibility.Collapsed;
                Grid.SetColumn(EditorUI, 0);
                Grid.SetRow(HistoryUI, 1);
                SecondaryColumn.Width = new GridLength(1, GridUnitType.Star);
                OtherColumn.Width = GridLength.Auto;
                //MainRow.Height = GridLength.Auto;
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private void RegistryEditorPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Bindings.Update();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 720)
            {
                HistoryBackground.Visibility = Visibility.Visible;
                Grid.SetColumn(EditorUI, 1);
                Grid.SetRow(HistoryUI, 0);
                SecondaryColumn.Width = new GridLength(328);
                OtherColumn.Width = new GridLength(1, GridUnitType.Star);
                //MainRow.Height = new GridLength(1, GridUnitType.Star);
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                HistoryBackground.Visibility = Visibility.Collapsed;
                Grid.SetColumn(EditorUI, 0);
                Grid.SetRow(HistoryUI, 1);
                SecondaryColumn.Width = new GridLength(1, GridUnitType.Star);
                OtherColumn.Width = GridLength.Auto;
                //MainRow.Height = GridLength.Auto;
                MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private void Refresh()
		{
			Suggestions.ItemsSource = _suggestionList;
			ValSuggestions.ItemsSource = _valSuggestionList;
			var applicationData = ApplicationData.Current;
			var localSettings = applicationData.LocalSettings;
			var value = localSettings.Values["history_count"];

			if (value != null)
			{
				for (var i = 1; i <= (int)localSettings.Values["history_count"]; i++)
				{
					if (localSettings.Values["history_" + i + "_hive"] != null)
					{
						_registryHistoryList.Add(new RegistryHistoryItem
						{
							Hive = localSettings.Values["history_" + i + "_hive"].ToString(),
							Key = localSettings.Values["history_" + i + "_key"].ToString(),
							ValueName = localSettings.Values["history_" + i + "_valuename"].ToString(),
							ValueData = localSettings.Values["history_" + i + "_valuedata"].ToString(),
							Operation = localSettings.Values["history_" + i + "_operation"].ToString()
						});
					}
				}

				EmptyText.Visibility = Visibility.Collapsed;
				ClearHistoryLink.IsEnabled = true;
			}

			else
			{
				EmptyText.Visibility = Visibility.Visible;
				ClearHistoryLink.IsEnabled = false;
			}

			HistoryList.ItemsSource = _registryHistoryList;
			initialized = true;
		}
        
		private void ReadButton_Click(object sender, RoutedEventArgs e)
		{
			ReadRegistryValueFromUi();
		}

		private void ReadRegistryValueFromUi()
		{
			string data;
			uint type;
			var selectedhive = GetSelectedHive();
			var key = PathInput.Text;
			var value = ValueNameInput.Text;
			var selectedtype = GetSelectedType();

			if (ValueNameSelector.SelectedIndex == 1)
			{
				value = "";
			}

			RunInThreadPool(async () =>
			{
				var ret = await _helper.GetKeyValue(selectedhive, key, value, selectedtype); type = ret.regtype; data = ret.regvalue;
                var result = ret.returncode;
                RunInUiThread(async () =>
				{
					ValueTypeInput.Visibility = Visibility.Collapsed;
					ValueTypeInput.Text = "";

					if ((await _helper.GetKeyStatus(selectedhive, key)) == KeyStatus.FOUND)
					{
						if (ValueNameSelector.SelectedIndex != 1)
						{
							switch (type)
							{
								case (uint)RegTypes.REG_BINARY:
									{
										TypeSelector.SelectedIndex = 0;
										break;
									}

								case (uint)RegTypes.REG_FULL_RESOURCE_DESCRIPTOR:
									{
										TypeSelector.SelectedIndex = 1;
										break;
									}

								case (uint)RegTypes.REG_DWORD:
									{
										TypeSelector.SelectedIndex = 2;
										break;
									}

								case (uint)RegTypes.REG_DWORD_BIG_ENDIAN:
									{
										TypeSelector.SelectedIndex = 3;
										break;
									}

								case (uint)RegTypes.REG_QWORD:
									{
										TypeSelector.SelectedIndex = 4;
										break;
									}

								case (uint)RegTypes.REG_MULTI_SZ:
									{
										TypeSelector.SelectedIndex = 5;
										break;
									}

								case (uint)RegTypes.REG_NONE:
									{
										TypeSelector.SelectedIndex = 6;
										break;
									}

								case (uint)RegTypes.REG_RESOURCE_LIST:
									{
										TypeSelector.SelectedIndex = 7;
										break;
									}

								case (uint)RegTypes.REG_RESOURCE_REQUIREMENTS_LIST:
									{
										TypeSelector.SelectedIndex = 8;
										break;
									}

								case (uint)RegTypes.REG_SZ:
									{
										TypeSelector.SelectedIndex = 9;
										break;
									}

								case (uint)RegTypes.REG_LINK:
									{
										TypeSelector.SelectedIndex = 10;
										break;
									}

								case (uint)RegTypes.REG_EXPAND_SZ:
									{
										TypeSelector.SelectedIndex = 11;
										break;
									}

								default:
									{
										TypeSelector.SelectedIndex = 12;
										ValueTypeInput.Visibility = Visibility.Visible;
										ValueTypeInput.Text = type.ToString();
										break;
									}
							}
						}
					}

					switch (result)
					{
						case HelperErrorCodes.SUCCESS:
							{
								AddHistoryItem(GetRegistryHiveName(selectedhive), key, value, data, "Read");
								ValueDataInput.Text = data;
								break;
							}

						case HelperErrorCodes.FAILED:
							{
								ShowReadFailedMessageBox();
								break;
							}

						case HelperErrorCodes.ACCESS_DENIED:
							{
								ShowDebugMessageBox("Access denied");
								break;
							}

						case HelperErrorCodes.NOT_IMPLEMENTED:
							{
								ShowNotYetImplementedMessageBox();
								break;
							}
					}
				});
			});
		}

		private void WriteButton_Click(object sender, RoutedEventArgs e)
		{
			WriteRegistryValueFromUi();
		}

		private void WriteRegistryValueFromUi()
		{
			var selectedhive = GetSelectedHive();
			var key = PathInput.Text;
			var value = ValueNameInput.Text;
			var selectedtype = GetSelectedType();
			var valuedata = ValueDataInput.Text;

			if (ValueNameSelector.SelectedIndex == 1)
			{
				value = "";
			}

			RunInThreadPool(async () =>
			{
				var result = await _helper.SetKeyValue(selectedhive, key, value, selectedtype, valuedata);
				RunInUiThread(() =>
				{
					switch (result)
					{
						case HelperErrorCodes.SUCCESS:
							{
								AddHistoryItem(GetRegistryHiveName(selectedhive), key, value, valuedata, "Write");
								break;
							}

						case HelperErrorCodes.FAILED:
							{
								ShowDebugMessageBox("Write failed");
								break;
							}

						case HelperErrorCodes.ACCESS_DENIED:
							{
								ShowDebugMessageBox("Access denied");
								break;
							}

						case HelperErrorCodes.NOT_IMPLEMENTED:
							{
								ShowNotYetImplementedMessageBox();
								break;
							}
					}
				});
			});
		}

		private static async void ShowReadFailedMessageBox()
		{
			await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(
			  ResourceManager.Current.MainResourceMap.GetValue("Resources/We_couldn_t_read_the_specified_registry_value__no_changes_to_the_phone_registry_were_made_",
			      ResourceContext.GetForCurrentView()).ValueAsString,
			  ResourceManager.Current.MainResourceMap.GetValue("Resources/Something_went_wrong", ResourceContext.GetForCurrentView()).ValueAsString);
		}

		private static async void ShowKeyUnableToDeleteMessageBox()
		{
			await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(
			  ResourceManager.Current.MainResourceMap.GetValue("Resources/We_couldn_t_delete_the_specified_key__no_changes_to_the_phone_registry_were_made_", ResourceContext.GetForCurrentView()).ValueAsString,
			  ResourceManager.Current.MainResourceMap.GetValue("Resources/Something_went_wrong", ResourceContext.GetForCurrentView()).ValueAsString);
		}

		private static async void ShowKeyUnableToAddMessageBox()
		{
			await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(
			  ResourceManager.Current.MainResourceMap.GetValue("Resources/We_couldn_t_add_the_specified_key__no_changes_to_the_phone_registry_were_made_", ResourceContext.GetForCurrentView()).ValueAsString,
			  ResourceManager.Current.MainResourceMap.GetValue("Resources/Something_went_wrong", ResourceContext.GetForCurrentView()).ValueAsString);
		}

		private static async void ShowNotYetImplementedMessageBox()
		{
			await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(ResourceManager.Current.MainResourceMap.GetValue("Resources/Not_implemented",
			    ResourceContext.GetForCurrentView()).ValueAsString, ResourceManager.Current.MainResourceMap.GetValue("Resources/Coming_soon", ResourceContext.GetForCurrentView()).ValueAsString);
		}

		private static async void ShowDebugMessageBox(string s)
		{
			await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(s, ResourceManager.Current.MainResourceMap.GetValue("Resources/Debug",
			    ResourceContext.GetForCurrentView()).ValueAsString);
		}

		private void AddHistoryItem(string hive, string key, string valueName, string valueData, string operation)
		{
			_registryHistoryList.Add(new RegistryHistoryItem
			{
				Hive = hive,
				Key = key,
				ValueName = valueName,
				ValueData = valueData,
				Operation = operation
			});
			var applicationData = ApplicationData.Current;
			var localSettings = applicationData.LocalSettings;
			var value = localSettings.Values["history_count"];

			if (value == null)
			{
				localSettings.Values["history_count"] = 1;
			}

			else
			{
				localSettings.Values["history_count"] = (int)localSettings.Values["history_count"] + 1;
			}

			localSettings.Values["history_" + localSettings.Values["history_count"] + "_hive"] = hive;
			localSettings.Values["history_" + localSettings.Values["history_count"] + "_key"] = key;
			localSettings.Values["history_" + localSettings.Values["history_count"] + "_valuename"] = valueName;
			localSettings.Values["history_" + localSettings.Values["history_count"] + "_valuedata"] = valueData;
			localSettings.Values["history_" + localSettings.Values["history_count"] + "_operation"] = operation;
			EmptyText.Visibility = Visibility.Collapsed;
			ClearHistoryLink.IsEnabled = true;
		}

		private void HistoryList_ItemClick(object sender, ItemClickEventArgs e)
		{
			//Debug.WriteLine("Selected: {0}", e);
			var selectedRegistryHistoryItem = (RegistryHistoryItem)e.ClickedItem;

			switch (selectedRegistryHistoryItem.Hive.ToUpper())
			{
				case "HKEY_CURRENT_CONFIG":
					{
						HiveSelector.SelectedIndex = 0;
						break;
					}

				case "HKEY_CLASSES_ROOT":
					{
						HiveSelector.SelectedIndex = 1;
						break;
					}

				case "HKEY_CURRENT_USER":
					{
						HiveSelector.SelectedIndex = 2;
						break;
					}

				case "HKEY_CURRENT_USER_LOCAL_SETTINGS":
					{
						HiveSelector.SelectedIndex = 3;
						break;
					}

				case "HKEY_DYNAMIC_DATA":
				case "HKEY_DYN_DATA":
					{
						HiveSelector.SelectedIndex = 4;
						break;
					}

				case "HKEY_LOCAL_MACHINE":
					{
						HiveSelector.SelectedIndex = 5;
						break;
					}

				case "HKEY_PERFORMANCE_DATA":
					{
						HiveSelector.SelectedIndex = 6;
						break;
					}

				case "HKEY_USERS":
					{
						HiveSelector.SelectedIndex = 7;
						break;
					}
			}

			PathInput.Text = selectedRegistryHistoryItem.Key;
			ValueDataInput.Text = selectedRegistryHistoryItem.ValueData;
			ValueNameInput.Text = selectedRegistryHistoryItem.ValueName;
		}

		private void PathInput_LostFocus(object sender, RoutedEventArgs e)
		{
			MainAppBar.Visibility = Visibility.Collapsed;
			KeySuggestions.Visibility = Visibility.Collapsed;
			var selectedhive = GetSelectedHive();
			var key = PathInput.Text;
			RunInThreadPool(async () =>
			{
				var status = await _helper.GetKeyStatus(selectedhive, key);
				RunInUiThread(() =>
				{
					switch (status)
					{
						case KeyStatus.FOUND:
							{
								KeyActionButton.IsEnabled = true;
								KeyActionIcon.Symbol = Symbol.Delete;
								break;
							}

						case KeyStatus.NOT_FOUND:
							{
								KeyActionButton.IsEnabled = true;
								KeyActionIcon.Symbol = Symbol.Add;
								break;
							}

						case KeyStatus.ACCESS_DENIED:
						case KeyStatus.UNKNOWN:
							{
								KeyActionButton.IsEnabled = false;
								KeyActionIcon.Symbol = Symbol.Cancel;
								break;
							}
					}
				});
			});
		}

		private void KeyActionButton_Click(object sender, RoutedEventArgs e)
		{
			var selectedhive = GetSelectedHive();
			var key = PathInput.Text;
			RunInThreadPool(async () =>
			{
				var status = await _helper.GetKeyStatus(selectedhive, key);
				RunInUiThread(() =>
				{
					switch (status)
					{
						case KeyStatus.FOUND:
							{
								DeleteKey(selectedhive, key);
								break;
							}

						case KeyStatus.NOT_FOUND:
							{
								AddKey(selectedhive, key);
								break;
							}
					}
				});
			});
		}

		private async void DeleteKey(RegHives hive, string keypath)
		{
			await new DeleteRegKeyContentDialog(hive, keypath).ShowAsync();
		}

		private async void AddKey(RegHives hive, string keypath)
		{
			var title = ResourceManager.Current.MainResourceMap.GetValue("Resources/Do_you_really_want_to_add_that_key_", ResourceContext.GetForCurrentView()).ValueAsString;
			var content = "We will add " + keypath + " to the phone registry.";
			var command = await new InteropTools.ContentDialogs.Core.DualMessageDialogContentDialog().ShowDualMessageDialog(title, content,
			              ResourceManager.Current.MainResourceMap.GetValue("Resources/Add_the_key", ResourceContext.GetForCurrentView()).ValueAsString,
			              ResourceManager.Current.MainResourceMap.GetValue("Resources/Don_t_add_the_key", ResourceContext.GetForCurrentView()).ValueAsString);

			if (command)
				RunInThreadPool(async () =>
			{
				var status = await _helper.AddKey(hive, keypath);

				if (status == HelperErrorCodes.FAILED)
				{
					RunInUiThread(ShowKeyUnableToAddMessageBox);
				}
			});
		}

		private RegHives GetSelectedHive()
		{
			var hive = RegHives.HKEY_LOCAL_MACHINE;
			var selectedhiveindex = HiveSelector.SelectedIndex;

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

		private static string GetRegistryHiveName(RegHives hive)
		{
			return Enum.GetName(typeof(RegHives), hive);
		}

		private uint GetSelectedType()
		{
			if (TypeSelector.SelectedIndex != 12)
			{
				ValueTypeInput.Visibility = Visibility.Collapsed;

				switch (TypeSelector.SelectedIndex)
				{
					case 0:
						{
							return (uint)RegTypes.REG_BINARY;
						}

					case 1:
						{
							return (uint)RegTypes.REG_FULL_RESOURCE_DESCRIPTOR;
						}

					case 2:
						{
							return (uint)RegTypes.REG_DWORD;
						}

					case 3:
						{
							return (uint)RegTypes.REG_DWORD_BIG_ENDIAN;
						}

					case 4:
						{
							return (uint)RegTypes.REG_QWORD;
						}

					case 5:
						{
							return (uint)RegTypes.REG_MULTI_SZ;
						}

					case 6:
						{
							return (uint)RegTypes.REG_NONE;
						}

					case 7:
						{
							return (uint)RegTypes.REG_RESOURCE_LIST;
						}

					case 8:
						{
							return (uint)RegTypes.REG_RESOURCE_REQUIREMENTS_LIST;
						}

					case 9:
						{
							return (uint)RegTypes.REG_SZ;
						}

					case 10:
						{
							return (uint)RegTypes.REG_LINK;
						}

					case 11:
						{
							return (uint)RegTypes.REG_EXPAND_SZ;
						}
				}

				return 0;
			}

			uint val = 0;

			try
			{
				ValueTypeInput.Visibility = Visibility.Visible;
				val = uint.Parse(ValueTypeInput.Text);
				return val;
			}

			catch
			{
			}

			return 0;
		}

		private void ClearHistoryLink_Click(object sender, RoutedEventArgs e)
		{
			var applicationData = ApplicationData.Current;
			var localSettings = applicationData.LocalSettings;
			var value = localSettings.Values["history_count"];

			if (value != null)
			{
				for (var i = 1; i <= (int)localSettings.Values["history_count"]; i++)
				{
					localSettings.Values.Remove("history_" + i + "_hive");
					localSettings.Values.Remove("history_" + i + "_key");
					localSettings.Values.Remove("history_" + i + "_valuename");
					localSettings.Values.Remove("history_" + i + "_valuedata");
					localSettings.Values.Remove("history_" + i + "_operation");
				}

				localSettings.Values.Remove("history_count");
			}

			for (var i = _registryHistoryList.Count - 1; i >= 0; i--)
			{
				_registryHistoryList.RemoveAt(i);
			}

			EmptyText.Visibility = Visibility.Visible;
			ClearHistoryLink.IsEnabled = false;
		}

		private static async void RunInUiThread(Action function)
		{
			await
			CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
			() => { function(); });
		}

		private async void RunInThreadPool(Action function)
		{
			await ThreadPool.RunAsync(x => { function(); });
		}

		private void Suggestions_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = (SuggestionItem)e.ClickedItem;

			if (PathInput.Text.Contains(@"\"))
			{
				var tmp = PathInput.Text.Split('\\');
				Array.Resize(ref tmp, tmp.Length - 1);
				var prevkey = string.Join(@"\", tmp);
				PathInput.Text = prevkey + @"\" + item.DisplayName + @"\";
			}

			else
			{
				PathInput.Text = item.DisplayName + @"\";
			}

			PathInput.SelectionStart = PathInput.Text.Length;
			PathInput.SelectionLength = 0;
		}

		private void PathInput_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			UpdateSuggestions();
		}

		private void UpdateSuggestions()
		{
			for (var i = _suggestionList.Count - 1; i >= 0; i--)
			{
				_suggestionList.RemoveAt(i);
			}

			var hive = GetSelectedHive();
			var key = PathInput.Text;

			if (PathInput.Text.EndsWith(@"\"))
			{
				RunInThreadPool(async () =>
				{
					var itemList = await _helper.GetRegistryItems2(hive, key);
					RunInUiThread(() =>
					{
						foreach (var item in itemList)
						{
							switch (item.Type)
							{
								case RegistryItemType.KEY:
									{
										_suggestionList.Add(new SuggestionItem { DisplayName = item.Name, Symbol = "" });
										break;
									}
							}
						}
					});
				});
			}

			else
			{
				var prevkey = "";
				var current = PathInput.Text;

				if (PathInput.Text.Contains(@"\"))
				{
					var tmp = PathInput.Text.Split('\\');
					current = tmp[tmp.Length - 1];
					Array.Resize(ref tmp, tmp.Length - 1);
					prevkey = string.Join(@"\", tmp);
				}

				RunInThreadPool(async () =>
				{
					var itemList = await _helper.GetRegistryItems2(hive, prevkey);
					RunInUiThread(() =>
					{
						foreach (var item in itemList)
						{
							switch (item.Type)
							{
								case RegistryItemType.KEY:
									{
										if (item.Name.ToLower().StartsWith(current.ToLower()))
										{
											_suggestionList.Add(new SuggestionItem { DisplayName = item.Name, Symbol = "" });
										}

										break;
									}
							}
						}
					});
				});
			}
		}

		private void UpdateValueSuggestions()
		{
			for (var i = _valSuggestionList.Count - 1; i >= 0; i--)
			{
				_valSuggestionList.RemoveAt(i);
			}

			var hive = GetSelectedHive();
			var key = PathInput.Text;
			var current = ValueNameInput.Text;
			RunInThreadPool(async () =>
			{
				var itemList = await _helper.GetRegistryItems2(hive, key);
				RunInUiThread(() =>
				{
					foreach (var item in itemList)
					{
						switch (item.Type)
						{
							case RegistryItemType.VALUE:
								{
									if (item.Name.ToLower().StartsWith(current.ToLower()) &&
									    (item.ValueType == GetSelectedType()))
									{
										_valSuggestionList.Add(new SuggestionItem { DisplayName = item.Name, Symbol = "" });
									}

									break;
								}
						}
					}
				});
			});
		}

		private void PathInput_GotFocus(object sender, RoutedEventArgs e)
		{
			MainAppBar.Visibility = Visibility.Visible;
			KeySuggestions.Visibility = Visibility.Visible;
			ValueSuggestions.Visibility = Visibility.Collapsed;
			UpdateSuggestions();
		}

		private void ValueNameInput_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			UpdateValueSuggestions();
		}

		private void ValueNameInput_GotFocus(object sender, RoutedEventArgs e)
		{
			MainAppBar.Visibility = Visibility.Visible;
			ValueSuggestions.Visibility = Visibility.Visible;
			KeySuggestions.Visibility = Visibility.Collapsed;
			UpdateValueSuggestions();
		}

		private void ValueNameInput_LostFocus(object sender, RoutedEventArgs e)
		{
			MainAppBar.Visibility = Visibility.Collapsed;
			ValueSuggestions.Visibility = Visibility.Collapsed;
			UpdateValueSuggestions();
		}

		private void ValSuggestions_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = (SuggestionItem)e.ClickedItem;
			ValueNameInput.Text = item.DisplayName;
			ValueNameInput.SelectionStart = ValueNameInput.Text.Length;
			ValueNameInput.SelectionLength = 0;
		}

		public class SuggestionItem
		{
			public string DisplayName { get; set; }
			public string Symbol { get; set; }
		}

		public class RegistryHistoryItem
		{
			public string Hive { get; set; }
			public string Key { get; set; }
			public string ValueName { get; set; }
			public string ValueData { get; set; }
			public string Operation { get; set; }
		}

		private void ValueNameSelector_SelectionChanged(Object sender, SelectionChangedEventArgs e)
		{
			if (initialized)
			{
				if (ValueNameSelector.SelectedIndex == 1)
				{
					ValueNameInput.Text = "";
					ValueNameInput.IsEnabled = false;
				}

				else
				{
					ValueNameInput.Text = "";
					ValueNameInput.IsEnabled = true;
				}
			}
		}

		private bool ValidateValue(uint type, string str)
		{
			switch (type)
			{
				case (uint)RegTypes.REG_DWORD:
					{
						try
						{
							UInt32.Parse(ValueDataInput.Text);
							return true;
						}

						catch
						{
							return false;
						}
					}

				case (uint)RegTypes.REG_QWORD:
					{
						try
						{
							UInt64.Parse(ValueDataInput.Text);
							return true;
						}

						catch
						{
							return false;
						}
					}

				case (uint)RegTypes.REG_MULTI_SZ:
					{
						return true;
					}

				case (uint)RegTypes.REG_SZ:
					{
						return !ValueDataInput.Text.Contains("\n");
					}

				case (uint)RegTypes.REG_EXPAND_SZ:
					{
						return !ValueDataInput.Text.Contains("\n");
					}

				default:
					{
						try
						{
                            var buffer = StringToByteArrayFastest(ValueDataInput.Text);

							return true;
						}

						catch
						{
							return false;
						}
					}
			}
		}


        private static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < (hex.Length >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }


        private void ValueDataInput_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
		{
			if (ValidateValue(GetSelectedType(), ValueDataInput.Text))
			{
				WriteButton.IsEnabled = true;
				ValueDataBorder.BorderThickness = new Thickness(0);
			}

			else
			{
				WriteButton.IsEnabled = false;
				ValueDataBorder.BorderThickness = new Thickness(2);
			}
		}

		private void TypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (ValidateValue(GetSelectedType(), ValueDataInput.Text))
				{
					WriteButton.IsEnabled = true;
					ValueDataBorder.BorderThickness = new Thickness(0);
				}

				else
				{
					WriteButton.IsEnabled = false;
					ValueDataBorder.BorderThickness = new Thickness(2);
				}
			}

			catch
			{
			}
		}
	}
}