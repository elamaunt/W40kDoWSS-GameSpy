using System;
using System.Diagnostics;
using System.Linq;
namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class FogRemover
    {
        const int fogAddress12 = 0x00745570;
        const int float512Address12 = 0x00863B18;
        const int mapSkyDistanceAddress12 = 0x007470CA;

        static readonly byte[] nope6 = new byte[6] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
        static readonly byte[] float512 = new byte[4] { 0x00, 0x00, 0x00, 0x44 };

        static readonly byte[] codeF512 = new byte[4] { 0x00, 0x00, 0xC0, 0x42 };
        static readonly byte[] jmpFog = new byte[6] { 0xD9, 0x81, 0x60, 0x0C, 0x00, 0x00 };
        static readonly byte[] jmpMapSkyDistance = new byte[6] { 0xD9, 0x9B, 0x70, 0x0C, 0x00, 0x00 };

        public static bool DisableFog(Process process)
        {
            var pHandle = ExternalMethods.OpenProcess(ExternalData.PROCESS_WM_READ | 
                ExternalData.PROCESS_WM_WRITE | ExternalData.PROCESS_VM_OPERATION, false, process.Id);
            if (IsFogEnabled(pHandle))
            {
                WriteFogCode(fogAddress12, nope6, pHandle);
                WriteFogCode(float512Address12, float512, pHandle);
                WriteFogCode(mapSkyDistanceAddress12, nope6, pHandle);
            }
            return IsFogDisabled(pHandle);
        }

        public static bool EnableFog(Process process)
        {
            var pHandle = ExternalMethods.OpenProcess(ExternalData.PROCESS_WM_READ | 
                ExternalData.PROCESS_WM_WRITE | ExternalData.PROCESS_VM_OPERATION, false, process.Id);
            if (IsFogDisabled(pHandle))
            {
                WriteFogCode(fogAddress12, jmpFog, pHandle);
                WriteFogCode(float512Address12, codeF512, pHandle);
                WriteFogCode(mapSkyDistanceAddress12, jmpMapSkyDistance, pHandle);
            }
            return IsFogEnabled(pHandle);
        }


        private static bool IsFogEnabled(IntPtr pHandle)
        {
            return CheckToggleMemory(fogAddress12, jmpFog, pHandle) &&
                CheckToggleMemory(float512Address12, codeF512, pHandle) &&
                CheckToggleMemory(mapSkyDistanceAddress12, jmpMapSkyDistance, pHandle);
        }

        private static bool IsFogDisabled(IntPtr pHandle)
        {
            return CheckToggleMemory(fogAddress12, nope6, pHandle) &&
                CheckToggleMemory(float512Address12, float512, pHandle) &&
                CheckToggleMemory(mapSkyDistanceAddress12, nope6, pHandle);
        }

        private static bool CheckToggleMemory(int addr, byte[] checkVal, IntPtr pHandle)
        {
            var tmp = new byte[checkVal.Length];

            if (ExternalMethods.ReadProcessMemory(pHandle, addr, tmp, tmp.Length, out var readCnt) && readCnt == tmp.Length && tmp.SequenceEqual(checkVal))
            {
                return true;
            }
            return false;
        }

        private static void WriteFogCode(int addr, byte[] setVal, IntPtr pHandle)
        {
            ExternalMethods.VirtualProtectEx(pHandle, addr, setVal.Length, ExternalData.PAGE_EXECUTE_READWRITE, out var oldProtect);
            ExternalMethods.WriteProcessMemory(pHandle, addr, setVal, setVal.Length, out _);
            ExternalMethods.VirtualProtectEx(pHandle, addr, setVal.Length, oldProtect, out _);
        }

    }
}
