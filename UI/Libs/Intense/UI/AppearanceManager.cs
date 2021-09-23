﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Intense.UI
{
    /// <summary>
    /// Manages the appearance of the application.
    /// </summary>
    public class AppearanceManager
    {
        /// <summary>
        /// Occurs when the application accent color has changed.
        /// </summary>
        public static event EventHandler AccentColorChanged;
        /// <summary>
        /// Occurs when the system accent color has changed.
        /// </summary>
        public static event EventHandler SystemAccentColorChanged;
        /// <summary>
        /// Occurs when the theme has changed for the current view.
        /// </summary>
        public event EventHandler ThemeChanged;

        private static readonly DependencyProperty AppearanceManagerProperty = DependencyProperty.RegisterAttached("AppearanceManager", typeof(AppearanceManager), typeof(AppearanceManager), null);
        private static readonly Uri AccentBrushesSource = new Uri("ms-appx:///Intense/Themes/AccentBrushes.xaml");

        private const string SystemAccentBrushKey = "__IntenseSystemAccentBrush";

        private FrameworkElement root;

        static AppearanceManager()
        {
            // initialize SystemAccentBrush
            GetSystemAccentBrush();
        }

        private AppearanceManager(FrameworkElement root)
        {
            this.root = root;
        }

        private static void OnSystemAccentColorChanged(DependencyObject o, DependencyProperty property)
        {
            SystemAccentColorChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void OnAccentColorChanged()
        {
            AccentColorChanged?.Invoke(null, EventArgs.Empty);
        }

        private void OnThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the accent color used by the application.
        /// </summary>
        /// <remarks>Set to null to use the system accent color.</remarks>
        public static Color? AccentColor
        {
            get { return GetAccentColor(); }
            set { SetAccentColor(value); }
        }

        /// <summary>
        /// Gets the accent color defined by the system.
        /// </summary>
        public static Color SystemAccentColor
        {
            get { return GetSystemAccentBrush().Color; }
        }

        /// <summary>
        /// Gets or sets the theme for the current view.
        /// </summary>
        public ApplicationTheme Theme
        {
            get { return GetTheme(); }
            set { SetTheme(value, true); }
        }
        
        private static Color? GetAccentColor()
        {
            var dict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == AccentBrushesSource);
            if (dict != null) {
                var brush = dict.Values.OfType<SolidColorBrush>().FirstOrDefault();
                return brush?.Color;
            }

            return null;
        }

        private static void SetAccentColor(Color? color)
        {
            if (AccentColor == color) {
                return;
            }

            // clear current accent brushes dictionary
            var currentDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source == AccentBrushesSource);
            if (currentDict != null) {
                Application.Current.Resources.MergedDictionaries.Remove(currentDict);
            }

            if (color != null) {
                var dict = new ResourceDictionary { Source = AccentBrushesSource };
                foreach (var brush in dict.Values.OfType<SolidColorBrush>()) {
                    brush.Color = color.Value;
                }
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }

            // and force repaint
            ResetAllViews();

            OnAccentColorChanged();
        }

        private static SolidColorBrush GetSystemAccentBrush()
        {
            if (Application.Current.Resources.ContainsKey(SystemAccentBrushKey)) {
                return (SolidColorBrush)Application.Current.Resources[SystemAccentBrushKey];
            }

            var brush = XamlHelper.CreateSolidColorBrush("{ThemeResource SystemAccentColor}");
            brush.RegisterPropertyChangedCallback(SolidColorBrush.ColorProperty, OnSystemAccentColorChanged);

            Application.Current.Resources[SystemAccentBrushKey] = brush;

            return brush;
        }
        
        private ApplicationTheme GetTheme()
        {
            if (this.root.RequestedTheme == ElementTheme.Default) {
                return Application.Current.RequestedTheme;
            }
            if (this.root.RequestedTheme == ElementTheme.Dark) {
                return ApplicationTheme.Dark;
            }
            return ApplicationTheme.Light;
        }

        private void SetTheme(ApplicationTheme theme, bool raiseEvent)
        {
            var oldTheme = this.Theme;
            var elementTheme = ElementTheme.Default;

            if (Application.Current.RequestedTheme != theme) {
                elementTheme = ElementTheme.Light;
                if (theme == ApplicationTheme.Dark) {
                    elementTheme = ElementTheme.Dark;
                }
            }
            this.root.RequestedTheme = elementTheme;
            if (raiseEvent && oldTheme != this.Theme) {
                OnThemeChanged();
            }
        }
        
        private static void ResetAllViews()
        {
            // TODO: reset for all views
            var manager = GetForCurrentView();
            //manager.Reset();
        }

        private void Reset()
        {
            if (this.Theme == ApplicationTheme.Dark) {
                SetTheme(ApplicationTheme.Light, false);
                SetTheme(ApplicationTheme.Dark, false);
            }
            else {
                SetTheme(ApplicationTheme.Dark, false);
                SetTheme(ApplicationTheme.Light, false);
            }
        }

        /// <summary>
        /// Gets the appearance manager for the current window (app view).
        /// </summary>
        /// <returns></returns>
        public static AppearanceManager GetForCurrentView()
        {
            var root = Window.Current?.Content?.GetAncestorsAndSelf().OfType<FrameworkElement>().Last();
            if (root == null) {
                return null;
            }

            var manager = (AppearanceManager)root.GetValue(AppearanceManagerProperty);
            if (manager == null) {
                manager = new AppearanceManager(root);

                root.SetValue(AppearanceManagerProperty, manager);
            }
            return manager;
        }
    }
}
