﻿#region License
/* Copyright 2012-2013 James F. Bellinger <http://www.zer7.com/software/hidsharp>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing,
   software distributed under the License is distributed on an
   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
   KIND, either express or implied.  See the License for the
   specific language governing permissions and limitations
   under the License. */
#endregion

using System.Threading;

namespace HidSharp.Platform
{
    sealed class HidSelector
    {
        public static bool IsRun;
        public static readonly HidManager Instance;
        static Thread ManagerThread; 

        static HidSelector()
        {
            foreach (var hidManager in new HidManager[]
            {
                new Windows.WinHidManager(),
                new Linux.LinuxHidManager(),
                new MacOS.MacHidManager(),
                new Unsupported.UnsupportedHidManager()
            })
            {
                if (hidManager.IsSupported)
                {
                    Instance = hidManager;
                    Instance.InitializeEventManager();
                    InitManager();
                    break;
                }
            }
        }

        public static void InitManager()
        {
            var readyEvent = new ManualResetEvent(false);

            ManagerThread = new Thread(Instance.RunImpl) { IsBackground = true, Name = "HID Manager" };
            ManagerThread.Start(readyEvent);
            readyEvent.WaitOne();
            IsRun = true;
        }

        public static void Stop()
        {
            Instance?.Stop();
            IsRun = false;
        }
    }

    public static class HidSelectorManager
    {
        public static void Stop()
        {
            if (HidSelector.IsRun)
            {
                HidSelector.Stop();
            }
        }

        public static void Start()
        {
            if (!HidSelector.IsRun)
            {
                HidSelector.InitManager();
            }
        }
    }
}
