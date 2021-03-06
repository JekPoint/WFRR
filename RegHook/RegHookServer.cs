﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using log4net;

namespace RegHook
{

    public class ServerInterface : MarshalByRefObject
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void IsInstalled(int clientPID)
        {
            _log.Info("[WFRR:RegHook] RegHook has been injected into process: " + clientPID);
        }

        public void ReportMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                _log.Info("[WFRR:RegHook] " + messages[i].Replace("{", "{{").Replace("}", "}}"));
            }
        }

        public void ReportMessage(string message)
        {
            _log.Info("[WFRR:RegHook] " + message);
        }

        public void ReportDebug(string message)
        {
#if DEBUG
            _log.Debug("[WFRR:RegHook] " + message);
#endif
        }

        public void ReportException(Exception e)
        {
            _log.Error("[WFRR:RegHook] " + e.ToString());
        }

        public void Ping() { }
    }

    public class InjectionEntryPoint : EasyHook.IEntryPoint
    {

        ServerInterface _server = null;

        Queue<string> _messageQueue = new Queue<string>();

        string vreg_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "V_REG.json");
        string vreg_json = null;
        VRegKey _vreg = null;
        IntPtr vreg_root = IntPtr.Zero;
        string vreg_root_str = null;
        string vreg_redirected = null;

        public InjectionEntryPoint(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
            _server.Ping();
        }

        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {

            _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

            try
            {
                //load V_REG.json from same location of RegHook.dll
                vreg_json = new StreamReader(vreg_path).ReadToEnd();
                _server.ReportMessage("Getting configuration from: " + vreg_path);
                _server.ReportDebug("V_REG.json: \n" + vreg_json);
                //convert to VRegKey object
                _vreg = JsonConvert.DeserializeObject<VRegKey>(vreg_json);
                //get vreg_root for redirected registry
                vreg_root_str = _vreg.VRegRedirected.Split('\\')[0];
                vreg_root = HKEY_StrToPtr(vreg_root_str);
                //get subkey of redirected location
                vreg_redirected = _vreg.VRegRedirected.Substring(vreg_root_str.Length + 1);
                _server.ReportMessage("VRegRedirected: " + vreg_redirected);
            }
            catch (Exception e)
            {
                _server.ReportException(e);
            }

            var regOpenKeyAHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegOpenKeyA"),
                new WinAPI.RegOpenKeyEx_Delegate(RegOpenKeyEx_Hook),
                this);
            regOpenKeyAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegOpenKeyA hook installed");

            var regOpenKeyWHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegOpenKeyW"),
                new WinAPI.RegOpenKeyEx_Delegate(RegOpenKeyEx_Hook),
                this);
            regOpenKeyWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegOpenKeyW hook installed");

            var regOpenKeyExAHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegOpenKeyExA"),
                new WinAPI.RegOpenKeyEx_Delegate(RegOpenKeyEx_Hook),
                this);
            regOpenKeyExAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegOpenKeyExA hook installed");

            var regOpenKeyExWHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegOpenKeyExW"),
                new WinAPI.RegOpenKeyEx_Delegate(RegOpenKeyEx_Hook),
                this);
            regOpenKeyExWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegOpenKeyExW hook installed");

            var regCreateKeyAHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegCreateKeyA"),
                new WinAPI.RegCreateKey_Delegate(RegCreateKey_Hook),
                this);
            regCreateKeyAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegCreateKeyA hook installed");

            var regCreateKeyWHook = EasyHook.LocalHook.Create(
               EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegCreateKeyW"),
               new WinAPI.RegCreateKey_Delegate(RegCreateKey_Hook),
               this);
            regCreateKeyWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegCreateKeyW hook installed");

            var regDeleteKeyAHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegDeleteKeyA"),
                new WinAPI.RegDeleteKeyEx_Delegate(RegDeleteKeyEx_Hook),
                this);
            regDeleteKeyAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegDeleteKeyA hook installed");

            var regDeleteKeyWHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegDeleteKeyW"),
                new WinAPI.RegDeleteKeyEx_Delegate(RegDeleteKeyEx_Hook),
                this);
            regDeleteKeyWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegDeleteKeyW hook installed");

            var regDeleteKeyExAHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegDeleteKeyExA"),
                new WinAPI.RegDeleteKeyEx_Delegate(RegDeleteKeyEx_Hook),
                this);
            regDeleteKeyExAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegDeleteKeyExA hook installed");

            var regDeleteKeyExWHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("advapi32.dll", "RegDeleteKeyExW"),
                new WinAPI.RegDeleteKeyEx_Delegate(RegDeleteKeyEx_Hook),
                this);
            regDeleteKeyExWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            _server.ReportMessage("RegDeleteKeyExW hook installed");

            EasyHook.RemoteHooking.WakeUpProcess();

            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    string[] queued = null;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued != null && queued.Length > 0)
                    {
                        _server.ReportMessages(queued);
                    }
                    else
                    {
                        _server.Ping();
                    }
                }
            }
            catch { }

            regOpenKeyAHook.Dispose();
            regOpenKeyWHook.Dispose();
            regOpenKeyExAHook.Dispose();
            regOpenKeyExWHook.Dispose();
            regCreateKeyAHook.Dispose();
            regCreateKeyWHook.Dispose();
            regDeleteKeyAHook.Dispose();
            regDeleteKeyWHook.Dispose();
            regDeleteKeyExAHook.Dispose();
            regDeleteKeyExWHook.Dispose();

            EasyHook.LocalHook.Release();
        }

        string HKEY_PtrToStr(IntPtr hkey)
        {
            switch (hkey.ToString())
            {
                case "-2147483648":
                    return "HKEY_CLASSES_ROOT";
                case "-2147483643":
                    return "HKEY_CURRENT_CONFIG";
                case "-2147483647":
                    return "HKEY_CURRENT_USER";
                case "-2147483646":
                    return "HKEY_LOCAL_MACHINE";
                case "-2147483645":
                    return "HKEY_USERS";
                default:
                    return "HKEY_LOCAL_MACHINE";
            }
        }

        IntPtr HKEY_StrToPtr(string hkey)
        {
            switch (hkey.ToString())
            {
                case "HKEY_CLASSES_ROOT":
                    return new IntPtr(-2147483648);
                case "HKEY_CURRENT_CONFIG":
                    return new IntPtr(-2147483643);
                case "HKEY_CURRENT_USER":
                    return new IntPtr(-2147483647);
                case "HKEY_LOCAL_MACHINE":
                    return new IntPtr(-2147483646);
                case "HKEY_USERS":
                    return new IntPtr(-2147483645);
                default:
                    return IntPtr.Zero;
            }
        }

        IntPtr RegOpenKeyEx_Hook(
            IntPtr hKey,
            string subKey,
            int ulOptions,
            int samDesired,
            ref IntPtr hkResult)
        {

            this._messageQueue.Enqueue(
                string.Format("[{0}:{1}] Calling RegOpenKeyEx {2} {3}",
                    EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(hKey), subKey));

            IntPtr result = IntPtr.Zero;
            string keyToOpen = HKEY_PtrToStr(hKey) + "\\" + subKey;

            try
            {
                foreach (VRegKeyMapping map in _vreg.Mapping)
                {
                    if (keyToOpen.ToUpper().Contains(map.Source.ToUpper()))
                    {
                        keyToOpen = keyToOpen.ToUpper().Replace(map.Source.ToUpper(), vreg_redirected + "\\" + map.Destination);
                        result = WinAPI.RegOpenKeyEx(vreg_root, keyToOpen, ulOptions, samDesired, ref hkResult);
                        this._messageQueue.Enqueue(
                            string.Format("[{0}:{1}] [Redirected] RegOpenKeyEx {2} {3} return code: {4}",
                                EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(vreg_root), keyToOpen, result));
                        if (result != IntPtr.Zero)
                        {
                            break;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }

                this._messageQueue.Enqueue(
                    string.Format("[{0}:{1}] Calling from original location...",
                        EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId()));
                result = WinAPI.RegOpenKeyEx(hKey, subKey, ulOptions, samDesired, ref hkResult);
                this._messageQueue.Enqueue(
                    string.Format("[{0}:{1}] [Origin] RegOpenKeyEx {2} {3} return code: {4}",
                        EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(hKey), subKey, result));
                return result;
            }
            catch (Exception e)
            {
                this._messageQueue.Enqueue(e.Message);
            }
            return result;
        }

        IntPtr RegCreateKey_Hook(
            IntPtr hKey,
            string subKey,
            ref IntPtr hkResult)
        {

            this._messageQueue.Enqueue(
                string.Format("[{0}:{1}] Calling RegCreateKey {2} {3}",
                    EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(hKey), subKey));

            hkResult = IntPtr.Zero;
            IntPtr result = IntPtr.Zero;
            string keyToCreate = HKEY_PtrToStr(hKey) + "\\" + subKey;

            try
            {
                foreach (VRegKeyMapping map in _vreg.Mapping)
                {
                    if (keyToCreate.ToUpper().Contains(map.Source.ToUpper()))
                    {
                        keyToCreate = keyToCreate.ToUpper().Replace(map.Source.ToUpper(), vreg_redirected + "\\" + map.Destination);
                        result = WinAPI.RegCreateKey(vreg_root, keyToCreate, ref hkResult);
                        this._messageQueue.Enqueue(
                            string.Format("[{0}:{1}] [Redirected] RegCreateKey {2} {3} return code: {4}",
                                EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(vreg_root), keyToCreate, result));
                        if (result != IntPtr.Zero)
                        {
                            break;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }

                this._messageQueue.Enqueue(
                    string.Format("[{0}:{1}] Calling from original location...",
                        EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId()));
                result = WinAPI.RegCreateKey(hKey, subKey, ref hkResult);
                this._messageQueue.Enqueue(
                    string.Format("[{0}:{1}] [Origin] RegCreateKey {2} {3} return code: {4}",
                        EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(hKey), subKey, result));
                return result;
            }
            catch (Exception e)
            {
                this._messageQueue.Enqueue(e.Message);
            }
            return result;
        }

        IntPtr RegDeleteKeyEx_Hook(
            IntPtr hKey,
            string subKey,
            int samDesired,
            int Reserved)
        {

            this._messageQueue.Enqueue(
                string.Format("[{0}:{1}] Calling RegDeleteKeyEx {2} {3}",
                    EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(hKey), subKey));

            IntPtr result = IntPtr.Zero;
            string keyToDelete = HKEY_PtrToStr(hKey) + "\\" + subKey;

            try
            {
                foreach (VRegKeyMapping map in _vreg.Mapping)
                {
                    if (keyToDelete.ToUpper().Contains(map.Source.ToUpper()))
                    {
                        keyToDelete = keyToDelete.ToUpper().Replace(map.Source.ToUpper(), vreg_redirected + "\\" + map.Destination);
                        result = WinAPI.RegDeleteKeyEx(vreg_root, keyToDelete, samDesired, Reserved);
                        this._messageQueue.Enqueue(
                            string.Format("[{0}:{1}] [Redirected] RegDeleteKeyEx {2} {3} return code: {4}",
                                EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(vreg_root), keyToDelete, result));
                        if (result != IntPtr.Zero)
                        {
                            break;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }

                this._messageQueue.Enqueue(
                    string.Format("[{0}:{1}] Calling from original location...",
                        EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId()));
                result = WinAPI.RegDeleteKeyEx(hKey, subKey, samDesired, Reserved);
                this._messageQueue.Enqueue(
                    string.Format("[{0}:{1}] [Origin] RegDeleteKeyEx {2} {3} return code: {4}",
                        EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), HKEY_PtrToStr(hKey), subKey, result));
                return result;

            }
            catch (Exception e)
            {
                this._messageQueue.Enqueue(e.Message);
            }
            return result;

        }

    }

}