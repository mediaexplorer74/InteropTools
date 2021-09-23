﻿using InteropTools.Providers;
using Renci.SshNet;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;

namespace InteropTools.Classes
{
	public class SSHSystemAccessHelper
	{
		public enum UnlockStates
		{
			DONE_NEEDS_REBOOT,
			NOT_DONE_REBOOT_PENDING,
			ALREADY_UNLOCKED,
			FAILED
		}

		public async Task<UnlockStates> UnlockSSHSystemAccess()
		{
			IRegistryProvider helper = App.MainRegistryHelper;
			var shell = ((CorePages.Shell)App.AppContent);
			var useCMD = await App.IsCMDSupported();

			if (useCMD)
			{
				return UnlockStates.ALREADY_UNLOCKED;
			}

			RegTypes regtype;
			String regvalue;
			var ret = await helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SYSTEM\ControlSet001\Services\MpsSvc", "Start", RegTypes.REG_DWORD); regtype = ret.regtype; regvalue = ret.regvalue;

			if (regvalue != "4")
			{
				await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"System\CurrentControlSet\Control\CI", "UMCIAuditMode",
				                   RegTypes.REG_DWORD, "1");
				var result3 = await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SYSTEM\ControlSet001\Services\MpsSvc", "Start", RegTypes.REG_DWORD, "4");

				if (result3 != HelperErrorCodes.SUCCESS)
				{
					return UnlockStates.FAILED;
				}

				return UnlockStates.NOT_DONE_REBOOT_PENDING;
			}

			ret = await helper.GetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"System\CurrentControlSet\Control\CI", "UMCIAuditMode",
			                   RegTypes.REG_DWORD); regtype = ret.regtype; regvalue = ret.regvalue;

			if (regvalue != "1")
			{
				await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"System\CurrentControlSet\Control\CI", "UMCIAuditMode",
				                   RegTypes.REG_DWORD, "1");
				return UnlockStates.NOT_DONE_REBOOT_PENDING;
			}

			try
			{
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
					var cmd = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//cmd.exe", UriKind.Absolute))).OpenStreamForReadAsync();
					var cmdmui = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//en-US//cmd.exe.mui", UriKind.Absolute))).OpenStreamForReadAsync();
					var reg = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//reg.exe", UriKind.Absolute))).OpenStreamForReadAsync();
					var regmui = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//en-US//reg.exe.mui", UriKind.Absolute))).OpenStreamForReadAsync();
					var netisol = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//CheckNetIsolation.exe", UriKind.Absolute))).OpenStreamForReadAsync();
					var netisolmui = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SSH//en-US//CheckNetIsolation.exe.mui", UriKind.Absolute))).OpenStreamForReadAsync();
					sclient.UploadFile(cmd, "/C/Windows/System32/cmd.exe");
					sclient.UploadFile(cmdmui, "/C/Windows/System32/en-US/cmd.exe.mui");
					sclient.UploadFile(reg, "/C/Windows/System32/reg.exe");
					sclient.UploadFile(regmui, "/C/Windows/System32/en-US/reg.exe.mui");
					sclient.UploadFile(netisol, "/C/Windows/System32/CheckNetIsolation.exe");
					sclient.UploadFile(netisolmui, "/C/Windows/System32/en-US/CheckNetIsolation.exe.mui");
					sclient.Disconnect();
					var SshClient = new SshClient(coninfo);
					SshClient.Connect();
					SshClient.KeepAliveInterval = new TimeSpan(0, 0, 10);
					var str = SshClient.RunCommand(@"%SystemRoot%\system32\CheckNetIsolation.exe LoopbackExempt -a -n=" + Package.Current.Id.FamilyName).Execute();
					await new InteropTools.ContentDialogs.Core.MessageDialogContentDialog().ShowMessageDialog(str);
					var result2 = await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SYSTEM\ControlSet001\Services\MpsSvc", "Start", RegTypes.REG_DWORD, "2");

					if (result2 != HelperErrorCodes.SUCCESS)
					{
						return UnlockStates.FAILED;
					}

					return UnlockStates.DONE_NEEDS_REBOOT;
				}

				catch
				{
					var result2 = await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SYSTEM\ControlSet001\Services\MpsSvc", "Start", RegTypes.REG_DWORD, "2");

					if (result2 != HelperErrorCodes.SUCCESS)
					{
						return UnlockStates.FAILED;
					}

					return UnlockStates.FAILED;
				}
			}

			catch
			{
				var result2 = await helper.SetKeyValue(RegHives.HKEY_LOCAL_MACHINE, @"SYSTEM\ControlSet001\Services\MpsSvc", "Start", RegTypes.REG_DWORD, "2");

				if (result2 != HelperErrorCodes.SUCCESS)
				{
					return UnlockStates.FAILED;
				}

				return UnlockStates.FAILED;
			}
		}
	}
}
