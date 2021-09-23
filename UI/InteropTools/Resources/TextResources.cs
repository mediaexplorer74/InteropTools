﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteropTools.Resources
{
	public class TextResources
	{
        // Application Manager
        public static string ApplicationManager_DeploymentOptions = "Deployment Options";

		public static string ApplicationManager_DeploymentMode = "Deployment Mode";
		public static string ApplicationManager_DeploymentModeDesc1 = "When you set this option, the app is installed in development mode.";
		public static string ApplicationManager_DeploymentModeDesc2 = "Use this option to enable key app development scenarios.";
		public static string ApplicationManager_DeploymentModeDesc3 = "You can't use this option in conjunction with a bundle package.";

		public static string ApplicationManager_ForceApplicationShutdown = "Force Application Shutdown";
		public static string ApplicationManager_ForceApplicationShutdownDesc =
		  "If the package, or any package that depends on this package, is currently in use, the processes associated with the package are shut down forcibly so that registration can continue.";

		public static string ApplicationManager_ForceTargetApplicationShutdown = "Force Target Application Shutdown";
		public static string ApplicationManager_ForceTargetApplicationShutdownDesc =
		  "If the package is currently in use, the processes associated with the package are shut down forcibly so that registration can continue.";

		public static string ApplicationManager_InstallAllResources = "Install All Resources";
		public static string ApplicationManager_InstallAllResourcesDesc1 = "When you set this option, the app is instructed to skip resource applicability checks.";
		public static string ApplicationManager_InstallAllResourcesDesc2 = "This effectively stages or registers all resource packages, which forces applicability for all packages contained in a bundle.";
		public static string ApplicationManager_InstallAllResourcesDesc3 = "If the package is a bundle, all contained resource packages will be registered.";

		public static string ApplicationManager_None = "None";
		public static string ApplicationManager_NoneDesc = "The default behavior is used.";

		public static string ApplicationManager_Install = "Install";
		public static string ApplicationManager_InstallDesc = "Please browse to the package you want to install, then press install to install it";
		public static string ApplicationManager_InstallPlaceHolderPath = @"Example: C:\Data\Users\Documents\Test.appxbundle";
		public static string ApplicationManager_InstallStatusReady = "Ready";
		public static string ApplicationManager_InstallBrowseButton = "Browse";
		public static string ApplicationManager_InstallPrimaryButton = "Install Package";

		public static string ApplicationManager_Register = "Register";
		public static string ApplicationManager_RegisterDesc = "Please browse to the package you want to register, then press register to register it";
		public static string ApplicationManager_RegisterPlaceHolderPath = @"Example: C:\Data\Users\Documents\TestPackage\AppxManifest.xml";
		public static string ApplicationManager_RegisterStatusReady = "Ready";
		public static string ApplicationManager_RegisterBrowseButton = "Browse";
		public static string ApplicationManager_RegisterPrimaryButton = "Register Package";

		public static string ApplicationManager_Update = "Update";
		public static string ApplicationManager_UpdateDesc = "Please browse to the package you want to update, then press update to update it";
		public static string ApplicationManager_UpdatePlaceHolderPath = @"Example: C:\Data\Users\Documents\Test.appxbundle";
		public static string ApplicationManager_UpdateStatusReady = "Ready";
		public static string ApplicationManager_UpdateBrowseButton = "Browse";
		public static string ApplicationManager_UpdatePrimaryButton = "Update Package";

		public static string ApplicationManager_PackageList = "Package List";
		public static string ApplicationManager_FilterBoxPlaceHolderText = "Search a package...";
		public static string ApplicationManager_AllVolumes = "All Volumes";
		public static string ApplicationManager_MountPoint = "Mount Point";
		public static string ApplicationManager_PackageStore = "Package Store";
		public static string ApplicationManager_Name = "Name";
		public static string ApplicationManager_SystemVolume = "System Volume";
		public static string ApplicationManager_Offline = "Offline";
		public static string ApplicationManager_SupportsHardLinks = "Supports Hardlinks";
		public static string ApplicationManager_AllTypes = "All Types";

		public static string ApplicationManager_InstallInProgress = "Installing... {0}%";
		public static string ApplicationManager_InstallError = "Error: {0}";
		public static string ApplicationManager_InstallCanceled = "Installation Canceled";
		public static string ApplicationManager_InstallSucceeded = "Installation Succeeded";

		public static string ApplicationManager_RegisterInProgress = "Registering... {0}%";
		public static string ApplicationManager_RegisterError = "Error: {0}";
		public static string ApplicationManager_RegisterCanceled = "Registration Canceled";
		public static string ApplicationManager_RegisterSucceeded = "Registration Succeeded";

		public static string ApplicationManager_UpdateInProgress = "Updating... {0}%";
		public static string ApplicationManager_UpdateError = "Error: {0}";
		public static string ApplicationManager_UpdateCanceled = "Update Canceled";
		public static string ApplicationManager_UpdateSucceeded = "Update Succeeded";

		public static string ApplicationManager_PackageListLoading = "Loading Application List {0}%...";
		public static string ApplicationManager_PackageListPackageArchUnknown = "Unknown";
		public static string ApplicationManager_PackageListPackageArchNeutral = "Neutral";
		public static string ApplicationManager_PackageListPackageArchARM = "ARM";
		public static string ApplicationManager_PackageListPackageArchx86 = "x86";
		public static string ApplicationManager_PackageListPackageArchx64 = "x64";
		public static string ApplicationManager_PackageListError = "Error: {0}";
		public static string ApplicationManager_PackageListUninstall = "Uninstall";

		// Shell
		public static string Shell_SwitchSession = "Switch Session";
		public static string Shell_WelcomeTitle = "Welcome";
		public static string Shell_WelcomeDesc = "Welcome message and quick launch pages";
		public static string Shell_RegistryEditorTitle = "Registry Editor";
		public static string Shell_RegistryEditorDesc = "View and edit registry values, create and edit keys";
		public static string Shell_RegistryGroupName = "Registry";
		public static string Shell_RegistryBrowserTitle = "Registry Browser";
		public static string Shell_RegistryBrowserDesc = "View, browse, create and edit through the registry";
		public static string Shell_RegistrySearchTitle = "Registry Search";
		public static string Shell_RegistrySearchDesc = "Search and view registry hives, keys, and values";
		public static string Shell_ImportRegFileTitle = "Import Registry File";
		public static string Shell_ImportRegFileDesc = "Import a .reg or a .itreg file from this page to the registry";
		public static string Shell_InteropUnlockTitle = "Interop Unlock";
		public static string Shell_InteropUnlockDesc = "Unlock device capabilities and more";
		public static string Shell_UnlockGroupName = "Unlock";
		public static string Shell_TweaksTitle = "Tweaks";
		public static string Shell_TweaksDesc = "View the status and apply tweaks for your device";
		public static string Shell_TweakGroupName = "Tweak";
		public static string Shell_KeyboardOptionsTitle = "(WIP) Keyboard Options";
		public static string Shell_KeyboardOptionsDesc = "WIP";
		public static string Shell_DefaultAppsTitle = "Default Applications";
		public static string Shell_DefaultAppsDesc = "View and manage default applications";
		public static string Shell_GeneralGroupName = "General";
		public static string Shell_SSHAccountManagerTitle = "SSH Account Manager";
		public static string Shell_SSHAccountManagerDesc = "Manage the current SSH Authentication Method";
		public static string Shell_SSHGroupName = "SSH";
		public static string Shell_ConsoleTitle = "System Console";
		public static string Shell_ConsoleDesc = "Access CMD from SSH";
		public static string Shell_ApplicationsTitle = "Applications";
		public static string Shell_ApplicationsDescription = "View and manage applications";
		public static string Shell_CertificatesTitle = "Certificates";
		public static string Shell_CertificatesDesc = "View and manage certificates";
		public static string Shell_DeviceInfoTitle = "Device Info";
		public static string Shell_DeviceInfoDesc = "View information about your device";
		public static string Shell_RemoteAccessTitle = "Remote Access";
		public static string Shell_RemoteAccessDesc = "Access all app functionalities from another device";
		public static string Shell_SettingsTitle = "Settings";
		public static string Shell_SettingsDesc = "Application Settings";
		public static string Shell_AboutTitle = "About";
		public static string Shell_AboutDesc = "About the application and credits";

		// App
		public static string App_SuspendingDescription = "Remote Access requires the app to stay working in the background to work properly.";

        //Core app stuff
        public static string AppName = "Interop Tools (Preview)";
        public static string AppDesc = "Interop Tools (Preview)";
		public static string SomethingHappened = "Something happened";
		public static string UnknownUser = "Unknown user";
		public static string RecentlyUsed = "Recently used";
		public static string Favorites = "Favorites";
		public static string HistoryPagesSeeMore = "See more items";
		public static string SendFeedback = "Send feedback";
		public static string VisitXDA = "Visit the XDA thread";
		public static string WhatsNew = "What's new in this release?";
		public static string WhatsNewDesc = "Discover what's new, changed and fixed in this release.";
		public static string GetExtensions = "Get extensions";
		public static string GetExtensionsDesc = "Get provider extensions, and discover about other connected apps.";
		
		public static string HamCore = "Core";
		public static string HamRegistry = "Registry";
		public static string HamUnlock = "Unlock";
		public static string HamGeneral = "General";
		public static string HamTweaks = "Tweaks";
		public static string HamSSH = "SSH";
		public static string HamTitle = "Tools";
		public static string HamSearch = "Search";
		
		//About page
		public static string Credits = "Credits";
		
		//Settings page
		public static string WindowsHelloDesc = "Require authentication at startup (requires Windows Hello or a PIN to be setup on the device).";
		public static string Personalization = "Personalization";
		public static string SelectYourTheme = "Select your theme";
		public static string ThemeDefault = "Default";
		public static string ThemeDark = "Dark";
		public static string ThemeLight = "Light";
		public static string UseSystemAccent = "Use the system accent color";
		public static string ChooseAccent = "Choose your accent color";
		
		//DeviceInfoPage
		public static string DeviceName = "Device name";
		public static string DeviceInformation = "Device information";
		public static string OperatingSystem = "Operating System:";
		public static string SystemVersion = "System Version:";
		public static string DeviceForm = "Device Form:";
		public static string DeviceFamily = "Device Family:";
		public static string DeviceFamilyVersion = "Device Family Version:";
		public static string HardwareSku = "Hardware Sku:";
		public static string Manufacturer = "Manufacturer:";
		public static string Model = "Model:";
		public static string FirmwareVersion = "Firmware Version:";
		public static string HardwareVersion = "Hardware Version:";
		public static string TelemetryCollectionLevel = "Telemetry Collection Level:";
		public static string UUID = "UUID:";
		public static string HardwareID = "Hardware ID:";
		
		//NotificationLEDPage
		public static string AvailableLEDs = "Available LEDs";
		public static string Intensity = "Intensity";
		public static string Period = "Period (in milliseconds)";
		public static string Disclaimer = "To use that page you need MS_WEH_LEDALERT installed on the mobile device, the feature needs to match your OS version for best results and to have working delta/differential updates with Windows Update. Future OS updates will upgrade the feature automatically as Microsoft provisions that feature through Windows Update.";
		
		//SystemConsole
		public static string YouCantUseThisRightNow = "You can't use this right now";
		public static string YouCantUseThisRightNowDesc = @"In order to use this page, you need to have properly configured cmd access already via the SSH Account Manager, please enable UMCI and provision the cmd files before attempting to use this page. Note: To use this page directly on the phone, you need to set HKLM\System\ControlSet001\Services\MpsSvc Start value to 4, doing this will break all application deployments, so be sure to set it back to 2 after being done.";
		public static string ShowTouchKeyboard = "Show touch keyboard";
		public static string SelectAll = "Select all";
		public static string Paste = "Paste";
		
		//SSHAccountManager
		public static string AllowUMCI = "Allow running apps in a non signed environment (UMCI Audit Mode) (Redstone and higher) (Required to access CMD)";
		public static string AddUser = "Add user";
		//TODO THE REST
		
        //Page titles
        public static string AppManagerTitle = "Applications";
        public static string CertManagerTitle = "Certificates";
        public static string AboutTitle = "About";
        public static string ExtensionsTitle = "Extensions";
        public static string SettingsTitle = "Settings";
        public static string WelcomeTitle = "Welcome";
        public static string YourDeviceTitle = "Your device";
        public static string RemoteAccessTitle = "Remote access";
        public static string RegistryBrowserTitle = "Registry browser";
        public static string DefaultAppsTitle = "Default apps";
        public static string ImportRegTitle = "Import registration file";
        public static string InteropUnlockTitle = "Interop unlock";
        public static string KeyboardTitle = "Keyboard tweaks";
        public static string NotificationLEDTitle = "Notification LED";
        public static string RegEditorTitle = "Registry editor";
        public static string RegHistoryTitle = "Registry history";
        public static string RegSearchTitle = "Registry search";
        public static string x50UnlockTitle = "x50 devices interop unlock";
        public static string ConsoleTitle = "System console";
        public static string SSHManagerTitle = "SSH user manager";

        //Page descriptions
        public static string AppManagerDesc = "Install, remove and lookup applications.";
		public static string CertManagerDesc = "Install, remove and lookup certificates.";
        public static string AboutDesc = "About this application.";
        public static string ExtensionsDesc = "Install, remove and lookup extensions.";
        public static string SettingsDesc = "Configure this application.";
        public static string WelcomeDesc = "Welcome to the application!";
        public static string YourDeviceDesc = "Lookup information about your device.";
        public static string RemoteAccessDesc = "Configure the remote access feature.";
        public static string RegistryBrowserDesc = "Browse the registry.";
        public static string DefaultAppsDesc = "Manage default apps.";
        public static string ImportRegDesc = "Import a registration file.";
        public static string InteropUnlockDesc = "Unlock capabilities on your device.";
        public static string KeyboardDesc = "Configure your touch keyboard.";
        public static string NotificationLEDDesc = "Setup a notification LED";
        public static string RegEditorDesc = "Edit the device registry.";
        public static string RegHistoryDesc = "View history about registry changes made by the application.";
        public static string RegSearchDesc = "Search the registry.";
        public static string x50UnlockDesc = "Unlock capabilities on x50 devices.";
        public static string ConsoleDesc = "Open a system console.";
        public static string SSHManagerDesc = "Manage SSH users.";
		
		//Registry Value Locs
		public static string RegBinary = "Binary";
		public static string RegHardwareResourceList = "Hardware resource list";
		public static string RegInteger = "Integer";
		public static string RegIntegerBigEndian = "Integer big endian";
		public static string RegLong = "Long";
		public static string RegMultiString = "Multi string";
		public static string RegNone = "None";
		public static string RegResourceList = "Resource list";
		public static string RegResourceRequirementsList = "Resource requirements list";
		public static string RegString = "String";
		public static string RegLink = "Symbolic link";
		public static string RegExpandString = "Variable string";
		public static string RegUnknown = "Unknown";
		public static string RegCustom = "Custom: {0}";
		
		//Registry Editor Locs
		public static string WriteValue = "Write value";
		public static string ReadValue = "Read value";
		public static string KeyHive = "Registry hive";
		public static string Key = "Registry key";
		public static string ValueType = "Registry value type";
		public static string ValueNameDefault = "Default";
		public static string ValueNameNormal = "Normal";
		public static string ValueName = "Registry value name";
		public static string HistoryEmpty = "Your history list is empty. Read or write to a registry value to fill this space in.";
		public static string ClearHistory = "Clear history";
		
	}
}
