﻿using System;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Intense.Presentation;
using InteropTools.CorePages;

namespace InteropTools.Presentation
{
	public class ShellViewModel : NotifyPropertyChanged
	{
		private bool isSplitViewPaneOpen;
		private NavigationItem selectedBottomItem;
		private NavigationItem selectedTopItem;

		public ShellViewModel()
		{
			ToggleSplitViewPaneCommand = new RelayCommand(() => IsSplitViewPaneOpen = !IsSplitViewPaneOpen);
			// open splitview pane in wide state
			IsSplitViewPaneOpen = IsWideState();
		}

		public ICommand ToggleSplitViewPaneCommand { get; private set; }

		public bool IsSplitViewPaneOpen
		{
			get {
				return isSplitViewPaneOpen;
			}

			set {
				Set(ref isSplitViewPaneOpen, value);
			}
		}

		public NavigationItem SelectedTopItem
		{
			get {
				return selectedTopItem;
			}

			set
			{
				if (Set(ref selectedTopItem, value) && (value != null))
				{
					OnSelectedItemChanged(selectedTopItem);
				}
			}
		}

		public NavigationItem SelectedBottomItem
		{
			get {
				return selectedBottomItem;
			}

			set
			{
				if (Set(ref selectedBottomItem, value) && (value != null))
				{
					OnSelectedItemChanged(selectedBottomItem);
				}
			}
		}

		public void ChangeSelectedItemWithoutUpdatingThePage(NavigationItem SelectedItem)
		{
			selectedTopItem = TopItems.FirstOrDefault(m => m == SelectedItem);
			selectedBottomItem = BottomItems.FirstOrDefault(m => m == SelectedItem);

			if (SelectedItem == SelectedTopItem)
			{
				selectedBottomItem = null;
			}

			else
				if (SelectedItem == SelectedBottomItem)
				{
					selectedTopItem = null;
				}

			OnPropertyChanged("SelectedItem");
		}

		public NavigationItem SelectedItem
		{
			get {
				return selectedTopItem ?? selectedBottomItem;
			}

			set
			{
				SelectedTopItem = TopItems.FirstOrDefault(m => m == value);
				SelectedBottomItem = BottomItems.FirstOrDefault(m => m == value);
			}
		}

		public Type SelectedPageType
		{
			get {
				return SelectedItem?.PageType;
			}

			set
			{
				// select associated menu item
				SelectedTopItem = TopItems.FirstOrDefault(m => m.PageType == value);
				SelectedBottomItem = BottomItems.FirstOrDefault(m => m.PageType == value);
			}
		}

		public NavigationItemCollection TopItems { get; } = new NavigationItemCollection();

		public NavigationItemCollection BottomItems { get; } = new NavigationItemCollection();

		private void OnSelectedItemChanged(NavigationItem item)
		{
			if (item == SelectedTopItem)
			{
				SelectedBottomItem = null;
			}

			else
				if (item == SelectedBottomItem)
				{
					SelectedTopItem = null;
				}

			OnPropertyChanged("SelectedItem");
			OnPropertyChanged("SelectedPageType");

			// auto-close split view pane (only when not in widestate)
			if (!IsWideState())
			{
				IsSplitViewPaneOpen = false;
			}
		}

		// a Helper determining whether we are in a wide window state
		// mvvm purists probably don't appreciate this approach
		private bool IsWideState()
		{
			return Window.Current.Bounds.Width >= 1024;
		}
	}
}