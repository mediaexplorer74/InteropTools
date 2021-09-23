/*++

Copyright (c) 2016  Interop Tools Development Team
Copyright (c) 2017  Gustave M.

Module Name:

    NDTKRegProvider.cs

Abstract:

    This module implements the NDTK Registry Provider Interface.

Author:

    Gustave M.     (gus33000)       20-Mar-2017

Revision History:

    Gustave M. (gus33000) 20-Mar-2017

        Initial Implementation.

--*/

// ME
#if ARM
using ndtklib;
#endif 

using System;

namespace InteropTools.Providers.OSReboot.NDTKProvider
{
    internal class NDTKRebootProvider : IRebootProvider
    {
#if ARM
        private NRPC _nrpc;
#endif

        public bool IsSupported(REBOOT_OPERATION operation)
        {
            // ME 
            bool result = false;
#if ARM
            if (_nrpc == null)
            {
                try
                {
                    _nrpc = new NRPC();
                    var ret = _nrpc.Initialize();
                }
                catch
                {
                    _nrpc = null;
                    return false;
                }
            }

            result = true;
#endif
            return result;
        }

        public REBOOT_STATUS SystemReboot()
        {
            if (!IsSupported(REBOOT_OPERATION.SystemReboot)) throw new NotImplementedException();
#if ARM
            return _nrpc.SystemReboot() == 0 ? REBOOT_STATUS.SUCCESS : REBOOT_STATUS.FAILED;
#else
            return REBOOT_STATUS.FAILED;
#endif

        }
    }
}
