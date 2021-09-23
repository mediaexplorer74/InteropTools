﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Intense.UI
{
    /// <summary>
    /// Describes customizations to the non-client area of the current window.
    /// </summary>
    public class WindowChrome : DependencyObject, IApplicationViewEventSink
    {
        /// <summary>
        /// Identifies the Chrome attached property.
        /// </summary>
        public static readonly DependencyProperty ChromeProperty = DependencyProperty.RegisterAttached("Chrome", typeof(WindowChrome), typeof(WindowChrome), new PropertyMetadata(null, OnChromeChanged));
        /// <summary>
        /// Identifies the AutoUpdateMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoUpdateMarginProperty = DependencyProperty.Register("AutoUpdateMargin", typeof(bool), typeof(WindowChrome), new PropertyMetadata(true));
        /// <summary>
        /// Identifies the Margin dependency property.
        /// </summary>
        public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(WindowChrome), new PropertyMetadata(null, OnMarginChanged));
        /// <summary>
        /// Identifies the StatusBarBackgroundColor dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusBarBackgroundColorProperty = DependencyProperty.Register("StatusBarBackgroundColor", typeof(Color), typeof(WindowChrome), new PropertyMetadata(null, OnStatusBarBackgroundColorChanged));
        /// <summary>
        /// Identifies the StatusBarForegroundColor dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusBarForegroundColorProperty = DependencyProperty.Register("StatusBarForegroundColor", typeof(Color), typeof(WindowChrome), new PropertyMetadata(null, OnStatusBarForegroundColorChanged));

        private FrameworkElement target;
        private bool initialized;

        private void CalculateMargin()
        {
            var appView = ApplicationView.GetForCurrentView();
            var visibleBounds = appView.VisibleBounds;
            var wndBounds = Window.Current.Bounds;

            if (visibleBounds != wndBounds)
            {
                var left = Math.Ceiling(visibleBounds.Left - wndBounds.Left);
                var top = Math.Ceiling(visibleBounds.Top - wndBounds.Top);
                var right = Math.Ceiling(wndBounds.Right - visibleBounds.Right);
                var bottom = Math.Ceiling(wndBounds.Bottom - visibleBounds.Bottom);

                this.Margin = new Thickness(left, top, right, bottom);
            }
            else
            {
                this.Margin = new Thickness();
            }
        }

        private static bool TryGetStatusBar(out StatusBar statusBar)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                statusBar = StatusBar.GetForCurrentView();
                return true;
            }
            statusBar = null;
            return false;
        }

        private static void OnChromeChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            // assign dependency object of type FrameworkElement to the chrome instance
            // this works as long as the chrome instance is not shared among dependency objects
            var chrome = (WindowChrome)args.NewValue;
            chrome?.SetTarget(o as FrameworkElement);

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var transparentColorBrush = new SolidColorBrush { Opacity = 0 };
            var transparentColor = transparentColorBrush.Color;
            titlebar.BackgroundColor = transparentColor;
            titlebar.ButtonBackgroundColor = transparentColor;
            titlebar.ButtonInactiveBackgroundColor = transparentColor;
            var solidColorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            if (solidColorBrush != null)
            {
                titlebar.ButtonForegroundColor = solidColorBrush.Color;
                titlebar.ButtonInactiveForegroundColor = solidColorBrush.Color;
            }

            var colorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            if (colorBrush != null)
            {
                titlebar.ForegroundColor = colorBrush.Color;
            }

            var hovercolor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 32;

            titlebar.ButtonHoverBackgroundColor = hovercolor;
            titlebar.ButtonHoverForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;

            hovercolor.A = 64;

            titlebar.ButtonPressedBackgroundColor = hovercolor;
            titlebar.ButtonPressedForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
        }

        private void SetTarget(FrameworkElement target)
        {
            this.target = target;
            InitializeChrome();
            ApplyMarginToTarget();
        }

        private void InitializeChrome()
        {
            if (this.initialized)
            {
                return;
            }
            this.initialized = true;

            SetStatusBarBackground();
            SetStatusBarForeground();

            ApplicationView.GetForCurrentView().RegisterEventSink(this);

            CalculateMargin();
            ApplyMarginToTarget();
        }

        private void ApplyMarginToTarget()
        {
            if (this.AutoUpdateMargin && this.target != null)
            {
                this.target.Margin = this.Margin;
            }
        }

        private void SetStatusBarBackground()
        {
            if (!this.initialized)
            {
                return;
            }
            StatusBar statusBar;
            if (TryGetStatusBar(out statusBar))
            {
                // infer opacity from alpha channel of the color
                statusBar.BackgroundOpacity = (double)this.StatusBarBackgroundColor.A / 255d;
                statusBar.BackgroundColor = this.StatusBarBackgroundColor;
            }

            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var transparentColorBrush = new SolidColorBrush { Opacity = 0 };
            var transparentColor = transparentColorBrush.Color;
            titlebar.BackgroundColor = transparentColor;
            titlebar.ButtonBackgroundColor = transparentColor;
            titlebar.ButtonInactiveBackgroundColor = transparentColor;
            var solidColorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            if (solidColorBrush != null)
            {
                titlebar.ButtonForegroundColor = solidColorBrush.Color;
                titlebar.ButtonInactiveForegroundColor = solidColorBrush.Color;
            }

            var colorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            if (colorBrush != null)
            {
                titlebar.ForegroundColor = colorBrush.Color;
            }

            var hovercolor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 32;

            titlebar.ButtonHoverBackgroundColor = hovercolor;
            titlebar.ButtonHoverForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;

            hovercolor.A = 64;

            titlebar.ButtonPressedBackgroundColor = hovercolor;
            titlebar.ButtonPressedForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
        }

        private void SetStatusBarForeground()
        {
            if (!this.initialized)
            {
                return;
            }
            StatusBar statusBar;
            if (TryGetStatusBar(out statusBar))
            {
                statusBar.ForegroundColor = this.StatusBarForegroundColor;
            }
            
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var transparentColorBrush = new SolidColorBrush { Opacity = 0 };
            var transparentColor = transparentColorBrush.Color;
            titlebar.BackgroundColor = transparentColor;
            titlebar.ButtonBackgroundColor = transparentColor;
            titlebar.ButtonInactiveBackgroundColor = transparentColor;
            var solidColorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            if (solidColorBrush != null)
            {
                titlebar.ButtonForegroundColor = solidColorBrush.Color;
                titlebar.ButtonInactiveForegroundColor = solidColorBrush.Color;
            }

            var colorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;
            if (colorBrush != null)
            {
                titlebar.ForegroundColor = colorBrush.Color;
            }

            var hovercolor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 32;

            titlebar.ButtonHoverBackgroundColor = hovercolor;
            titlebar.ButtonHoverForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;

            hovercolor.A = 64;

            titlebar.ButtonPressedBackgroundColor = hovercolor;
            titlebar.ButtonPressedForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
        }

        private static void OnMarginChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ((WindowChrome)o).ApplyMarginToTarget();
        }

        private static void OnStatusBarBackgroundColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ((WindowChrome)o).SetStatusBarBackground();
        }

        private static void OnStatusBarForegroundColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ((WindowChrome)o).SetStatusBarForeground();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the margin of the target framework element is automatically updated when the visible bounds changes.
        /// </summary>
        public bool AutoUpdateMargin
        {
            get { return (bool)GetValue(AutoUpdateMarginProperty); }
            set { SetValue(AutoUpdateMarginProperty, value); }
        }

        /// <summary>
        /// Gets the window margin.
        /// </summary>
        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            private set { SetValue(MarginProperty, value); }
        }

        /// <summary>
        /// Gets or set the status bar background color.
        /// </summary>
        public Color StatusBarBackgroundColor
        {
            get { return (Color)GetValue(StatusBarBackgroundColorProperty); }
            set { SetValue(StatusBarBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or set the status bar foreground color.
        /// </summary>
        public Color StatusBarForegroundColor
        {
            get { return (Color)GetValue(StatusBarForegroundColorProperty); }
            set { SetValue(StatusBarForegroundColorProperty, value); }
        }

        /// <summary>
        /// Retrieves the <see cref="WindowChrome"/> attached to specified dependency object instance.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static WindowChrome GetChrome(DependencyObject o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }
            return (WindowChrome)o.GetValue(ChromeProperty);
        }

        /// <summary>
        /// Attaches a <see cref="WindowChrome"/> to specified dependency object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="chrome"></param>
        public static void SetChrome(DependencyObject o, WindowChrome chrome)
        {
            o.SetValue(ChromeProperty, chrome);
        }

        void IApplicationViewEventSink.OnConsolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
        }

        void IApplicationViewEventSink.OnVisibleBoundsChanged(ApplicationView sender, object args)
        {
            CalculateMargin();
        }
    }
}
