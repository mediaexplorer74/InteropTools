using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;
using InteropTools.Resources;
using Windows.UI.Notifications;
using Windows.System.Profile;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Resources.Core; // App Resources Core
using Renci.SshNet;  // Session 


using System.Collections.Generic;
using System.Linq;

using Windows.Storage;
using Windows.System.Display;

using InteropTools.RemoteClasses.Server;

// switch shell =)
//using Shell = InteropTools.CorePages.Shell; // новая
using Shell = InteropTools.Shell; // старая

using Windows.Foundation;
using Windows.Graphics.Display;

using InteropTools.Providers;

using System.IO;
using Windows.ApplicationModel.ExtendedExecution;

using InteropTools.CorePages;

using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Markup;

using InteropTools.ContentDialogs.Core;
using Microsoft.Services.Store.Engagement;



namespace InteropTools
{

    

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {

        public class Remote
        {
            string Hostname;
            int SessionID;
        }

        // /UI/IT/Resources/Localization.cs
        public static Localization local = new Localization();

        // -------------- refactor it! --------------------------

        //public static RegPluginList regpluginlist;
        //public static PowerPluginList powerpluginlist;

        public static readonly TextResources textResources = new TextResources();

        public static readonly string RemoteLoc = ResourceManager.Current.MainResourceMap.GetValue
        (
            "Resources/The_following_Remote_device_wants_to_access_your_phone_Registry",
            ResourceContext.GetForCurrentView()
        ).ValueAsString;

        public static readonly string RemoteAllowLoc = ResourceManager.Current.MainResourceMap.GetValue("Resources/Allow", ResourceContext.GetForCurrentView()).ValueAsString;

        public static readonly string RemoteDenyLoc = ResourceManager.Current.MainResourceMap.GetValue("Resources/Deny", ResourceContext.GetForCurrentView()).ValueAsString;

        public static readonly ObservableRangeCollection<Session> Sessions = new ObservableRangeCollection<Session>();

        public static bool Fancyness = false;

        public static int? CurrentSession;

        public static IRegistryProvider RegistryHelper
        {
            get
            {
                return  null; // CurrentSession != null ? Sessions[(int)CurrentSession].Helper : null;
            }

            set
            {
                if (CurrentSession != null)
                {
                    //Sessions[(int)CurrentSession].Helper = value;
                }
            }
        }

        public static IRegistryProvider MainRegistryHelper = new MainRegistryProvider();

        public static readonly DisplayRequest DisplayRequest = new DisplayRequest();

        // External stuff
        public static readonly RemoteServer Server = new RemoteServer();

        //private static readonly Random Random = new Random();

        public static readonly string SessionId = RandomString(10);

        private static readonly Random _rng = new Random();
       
        private static string RandomString(int size)
        {
           
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }

        public static readonly List<Remote> AllowedRemotes = new List<Remote>();
        public static readonly List<Remote> DeniedRemotes = new List<Remote>();
        // End of external stuff

        private static readonly Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        private static readonly double scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
        public static readonly Size size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

        // ------------------------------------------------------





        // App () constructor 
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            /*if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Application", "RequiresPointerMode"))
                RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;*/

            UnhandledException += async (s, args) =>
            {
                args.Handled = true;

                try
                {
                    await new MessageDialog(args.Exception?.ToString() ?? string.Empty, local.App_UnhandledException).ShowAsync();
                }
                catch { }
            };

            //RefreshTile();
        }


        // ---------------------

        // Refactor it!!
        public async static Task<bool> IsCMDSupported()
        {
            var helper = MainRegistryHelper;
            RegTypes regtype;
            String regvalue;
            var ret = await helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SYSTEM\ControlSet001\Services\MpsSvc", "Start", RegTypes.REG_DWORD); regtype = ret.regtype; regvalue = ret.regvalue;

            //if (regvalue != "2") return false;

            if ((SshClient != null) && (SshClient.IsConnected))
            {
                return true;
            }

            await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh", "default-shell",
                               RegTypes.REG_SZ, @"%SystemRoot%\system32\cmd.exe");
            await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh", "default-env",
                               RegTypes.REG_SZ, "currentdir,async,autoexec");
            ret = await helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh", "user-list",
                               RegTypes.REG_SZ); regtype = ret.regtype; regvalue = ret.regvalue;

            if ((regvalue == null) || (regvalue == ""))
            {
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh", "user-list",
                                   RegTypes.REG_SZ, "Sirepuser");
            }

            var add
                  = true;

            var username = "InteropTools";
            ret = await helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh", "user-list",
                               RegTypes.REG_SZ); regtype = ret.regtype; regvalue = ret.regvalue;

            if (regvalue.Contains(";"))
            {
                foreach (var user in regvalue.Split(';'))
                {
                    if (user.ToLower() == username.ToLower())
                    {
                        add
                              = false;
                    }
                }
            }

            else
            {
                if (regvalue.ToLower() == username.ToLower())
                {
                    add
                          = false;
                }
            }

            if (add)
            {
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh", "user-list",
                                   RegTypes.REG_SZ, regvalue + ";" + username);
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh\" + username,
                                   "user-name", RegTypes.REG_SZ, "LocalSystem");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh\" + username,
                                   "auth-method", RegTypes.REG_SZ, "password");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh\" + username,
                                   "user-pin", RegTypes.REG_SZ, App.SessionId);
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh\" + username,
                                   "subsystems", RegTypes.REG_SZ, "default,sftp");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh\" + username,
                                   "default-home-dir", RegTypes.REG_SZ, @"%SystemRoot%\system32\");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "default-shell", RegTypes.REG_SZ, @"%SystemRoot%\system32\cmd.exe");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-home-dir", RegTypes.REG_SZ, "C:\\");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-mkdir-rex", RegTypes.REG_SZ, ".*");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-open-dir-rex", RegTypes.REG_SZ, ".*");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-read-file-rex", RegTypes.REG_SZ, ".*");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-remove-file-rex", RegTypes.REG_SZ, ".*");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-rmdir-rex", RegTypes.REG_SZ, ".*");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-stat-rex", RegTypes.REG_SZ, ".*");
                await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, "System\\Currentcontrolset\\control\\ssh\\" + username,
                                   "sftp-write-file-rex", RegTypes.REG_SZ, ".*");
            }

            ret = await helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"system\CurrentControlSet\control\ssh\" + username,
                               "user-pin", RegTypes.REG_SZ); regtype = ret.regtype; regvalue = ret.regvalue;

            try
            {
                var Server = helper.GetHostName();
                var Username = "InteropTools";
                var Password = regvalue;
                var coninfo = new PasswordConnectionInfo(Server, Username, Password);
                coninfo.Timeout = new TimeSpan(0, 0, 5);
                coninfo.RetryAttempts = 1;
                var sclient = new SftpClient(coninfo);
                sclient.OperationTimeout = new TimeSpan(0, 0, 5);
                sclient.Connect();
                sclient.BufferSize = 4 * 1024;
                var op = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//cmd.exe", UriKind.Absolute));

                while (op.Status == AsyncStatus.Started)
                {
                }

                var op2 = op.GetResults().OpenStreamForReadAsync();

                while (op2.Status == TaskStatus.Running)
                {
                }

                var cmd = op2.Result;
                op =
                  StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//en-US//cmd.exe.mui",
                      UriKind.Absolute));

                while (op.Status == AsyncStatus.Started)
                {
                }

                op2 = op.GetResults().OpenStreamForReadAsync();

                while (op2.Status == TaskStatus.Running)
                {
                }

                var cmdmui = op2.Result;
                op = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//reg.exe", UriKind.Absolute));

                while (op.Status == AsyncStatus.Started)
                {
                }

                op2 = op.GetResults().OpenStreamForReadAsync();

                while (op2.Status == TaskStatus.Running)
                {
                }

                var reg = op2.Result;
                op =
                  StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//en-US//reg.exe.mui",
                      UriKind.Absolute));

                while (op.Status == AsyncStatus.Started)
                {
                }

                op2 = op.GetResults().OpenStreamForReadAsync();

                while (op2.Status == TaskStatus.Running)
                {
                }

                var regmui = op2.Result;
                op =
                  StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//CheckNetIsolation.exe",
                      UriKind.Absolute));

                while (op.Status == AsyncStatus.Started)
                {
                }

                op2 = op.GetResults().OpenStreamForReadAsync();

                while (op2.Status == TaskStatus.Running)
                {
                }

                var netisol = op2.Result;
                op =
                  StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//en-US//CheckNetIsolation.exe.mui",
                      UriKind.Absolute));

                while (op.Status == AsyncStatus.Started)
                {
                }

                op2 = op.GetResults().OpenStreamForReadAsync();

                while (op2.Status == TaskStatus.Running)
                {
                }

                var netisolmui = op2.Result;
                sclient.UploadFile(cmd, "/C/Windows/System32/cmd.exe");
                sclient.UploadFile(cmdmui, "/C/Windows/System32/en-US/cmd.exe.mui");
                sclient.UploadFile(reg, "/C/Windows/System32/reg.exe");
                sclient.UploadFile(regmui, "/C/Windows/System32/en-US/reg.exe.mui");
                sclient.UploadFile(netisol, "/C/Windows/System32/CheckNetIsolation.exe");
                sclient.UploadFile(netisolmui, "/C/Windows/System32/en-US/CheckNetIsolation.exe.mui");
                sclient.Disconnect();
                SshClient = new SshClient(coninfo);
                SshClient.Connect();
                SshClient.KeepAliveInterval = new TimeSpan(0, 0, 10);
                return true;
            }

            catch
            {
                SshClient = null;
                return false;
            }
        }

        // ---------------------

        public static SshClient SshClient { get; set; }

        public static UIElement AppContent
        {
            get
            {
                if (Window.Current.Content == null)
                {
                    Window.Current.Content = new CoreFrame();
                }

                CoreFrame frame = Window.Current.Content as CoreFrame;
                return frame.FrameContent;
            }

            set
            {
                if (Window.Current.Content == null)
                {
                    Window.Current.Content = new CoreFrame();
                }

                CoreFrame frame = Window.Current.Content as CoreFrame;
                frame.FrameContent = value;
            }
        }

        // ---------------------

        public ObservableCollection<ViewLifetimeControl> SecondaryViews = new ObservableCollection<ViewLifetimeControl>();
        private CoreDispatcher mainDispatcher;
        public CoreDispatcher MainDispatcher
        {
            get
            {
                return mainDispatcher;
            }
        }

        private void RefreshTile()
        {
            var devicefamily = AnalyticsInfo.VersionInfo.DeviceFamily;
            var tileimg = "generic";

            switch (devicefamily.ToLower())
            {
                case "windows.desktop":
                    {
                        tileimg = "desktop";
                        break;
                    }

                case "windows.xbox":
                    {
                        tileimg = "xbox";
                        break;
                    }

                case "windows.holographic":
                    {
                        tileimg = "holographic";
                        break;
                    }

                case "windows.team":
                    {
                        tileimg = "team";
                        break;
                    }

                case "windows.iot":
                    {
                        tileimg = "iot";
                        break;
                    }

                case "windows.mobile":
                    {
                        tileimg = "phone";
                        break;
                    }

                default:
                    {
                        tileimg = "generic";
                        break;
                    }
            }

            var content = new TileContent
            {
                Visual = new TileVisual
                {
                    TileSmall = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/Tiles/Small/" + tileimg + ".png"
                            }
                        }
                    },
                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/Tiles/Medium/" + tileimg + ".png"
                            }
                        }
                    },
                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/Tiles/Wide/" + tileimg + ".png"
                            }
                        }
                    },
                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = "Assets/Tiles/Large/" + tileimg + ".png"
                            }
                        }
                    }
                }
            };
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(content.GetXml()));
        }

        private async Task<ViewLifetimeControl> createMainPageAsync()
        {
            ViewLifetimeControl viewControl = null;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // This object is used to keep track of the views and important
                // details about the contents of those views across threads
                // In your app, you would probably want to track information
                // like the open document or page inside that window
                viewControl = ViewLifetimeControl.CreateForCurrentView();
                // Increment the ref count because we just created the view and we have a reference to it                
                viewControl.StartViewInUse();

                var frame = new Frame();
                frame.Navigate(typeof(Shell), viewControl);
                Window.Current.Content = frame;
                // This is a change from 8.1: In order for the view to be displayed later it needs to be activated.
                Window.Current.Activate();
            });

            ((App)App.Current).SecondaryViews.Add(viewControl);

            return viewControl;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // disabled, obscures the hamburger button, enable if you need it
                DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            // CoreApplication.EnablePrelaunch was introduced in Windows 10 version 1607
            bool canEnablePrelaunch = Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

            if (Window.Current.Content == null)
            {
                bool loadState = (e.PreviousExecutionState == ApplicationExecutionState.Terminated);

                //Pages.SplashScreen extendedSplash = new Pages.SplashScreen(e.SplashScreen, loadState, e.Arguments);

                Pages.SplashScreen extendedSplash = new Pages.SplashScreen(e.SplashScreen, loadState, e.Arguments);

                Window.Current.Content = extendedSplash;

                mainDispatcher = Window.Current.Dispatcher;

                if (e.PrelaunchActivated == false)
                {
                    // On Windows 10 version 1607 or later, this code signals that this app wants to participate in prelaunch
                    if (canEnablePrelaunch)
                    {
                        TryEnablePrelaunch();
                    }

                    // Ensure the current window is active
                    Window.Current.Activate();
                }
            }
            else
            {
                // second and later
                var selectedView = await createMainPageAsync();
                if (selectedView != null)
                {
                    selectedView.StartViewInUse();
                    var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(selectedView.Id, ViewSizePreference.Default, ApplicationView.GetForCurrentView().Id, ViewSizePreference.Default);

                    if (!viewShown)
                    {
                        foreach (var item in SecondaryViews)
                        {
                            if (item != selectedView)
                            {
                                var result = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(selectedView.Id, ViewSizePreference.Default, item.Id, ViewSizePreference.Default);
                                if (result)
                                    break;
                            }
                        }
                    }

                    await selectedView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Window.Current.Activate();
                    });

                    selectedView.StopViewInUse();
                }
            }
        }

        /// <summary>
        /// Encapsulates the call to CoreApplication.EnablePrelaunch() so that the JIT
        /// won't encounter that call (and prevent the app from running when it doesn't
        /// find it), unless this method gets called. This method should only
        /// be called when the caller determines that we are running on a system that
        /// supports CoreApplication.EnablePrelaunch().
        /// </summary>
        private void TryEnablePrelaunch()
        {
            CoreApplication.EnablePrelaunch(true);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }

    
}
