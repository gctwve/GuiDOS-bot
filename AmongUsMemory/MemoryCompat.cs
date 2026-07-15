using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Memory
{
    public class Mem
    {
        private Process process;
        private IntPtr processHandle = IntPtr.Zero;

        [Flags]
        private enum ProcessAccess : uint
        {
            QueryInformation = 0x0400,
            VirtualMemoryRead = 0x0010
        }

        [Flags]
        private enum MemoryState : uint
        {
            Commit = 0x1000
        }

        [Flags]
        private enum MemoryProtect : uint
        {
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccess processAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, IntPtr dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool Module32FirstW(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool Module32NextW(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExePath;
        }

        public bool OpenProcess(string processName)
        {
            process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                return false;
            }

            processHandle = OpenProcess(ProcessAccess.QueryInformation | ProcessAccess.VirtualMemoryRead, false, process.Id);
            return processHandle != IntPtr.Zero;
        }

        public byte[] ReadBytes(string address, int size)
        {
            if (size <= 0)
            {
                return new byte[0];
            }

            var resolved = ResolveAddress(address);
            if (resolved == IntPtr.Zero)
            {
                return null;
            }

            var buffer = new byte[size];
            IntPtr bytesRead;
            if (!ReadProcessMemory(processHandle, resolved, buffer, (UIntPtr)buffer.Length, out bytesRead))
            {
                return null;
            }

            if (bytesRead.ToInt64() == buffer.Length)
            {
                return buffer;
            }

            Array.Resize(ref buffer, Math.Max(0, (int)bytesRead.ToInt64()));
            return buffer;
        }

        public int ReadByte(string address)
        {
            var bytes = ReadBytes(address, 1);
            return bytes == null || bytes.Length == 0 ? 0 : bytes[0];
        }

        public int ReadInt(string address)
        {
            var bytes = ReadBytes(address, 4);
            return bytes == null || bytes.Length < 4 ? 0 : BitConverter.ToInt32(bytes, 0);
        }

        public float ReadFloat(string address)
        {
            var bytes = ReadBytes(address, 4);
            return bytes == null || bytes.Length < 4 ? 0f : BitConverter.ToSingle(bytes, 0);
        }

        public IntPtr ReadPointer(IntPtr address)
        {
            var bytes = ReadBytes(address.ToInt64().ToString("X"), IntPtr.Size);
            if (bytes == null || bytes.Length < IntPtr.Size)
            {
                return IntPtr.Zero;
            }

            return IntPtr.Size == 8
                ? new IntPtr(BitConverter.ToInt64(bytes, 0))
                : new IntPtr(BitConverter.ToInt32(bytes, 0));
        }

        public IntPtr ResolveAddressPublic(string address)
        {
            return ResolveAddress(address);
        }

        public Task<IEnumerable<long>> AoBScan(string pattern)
        {
            return AoBScan(pattern, false, false);
        }

        public Task<IEnumerable<long>> AoBScan(string pattern, bool writable, bool executable)
        {
            return Task.Run(() => (IEnumerable<long>)Scan(pattern, writable, executable));
        }

        private List<long> Scan(string pattern, bool writable, bool executable)
        {
            var needle = ParsePattern(pattern);
            var results = new List<long>();
            if (processHandle == IntPtr.Zero || needle.Length == 0)
            {
                return results;
            }

            long address = 0;
            var mbiSize = new IntPtr(Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
            while (VirtualQueryEx(processHandle, new IntPtr(address), out var mbi, mbiSize) != 0)
            {
                var regionSize = mbi.RegionSize.ToInt64();
                if (regionSize <= 0)
                {
                    break;
                }

                if (IsReadable(mbi.Protect, mbi.State) && MatchesProtection(mbi.Protect, writable, executable))
                {
                    ScanRegion(mbi.BaseAddress, regionSize, needle, results);
                }

                var next = mbi.BaseAddress.ToInt64() + regionSize;
                if (next <= address)
                {
                    break;
                }

                address = next;
            }

            return results;
        }

        private void ScanRegion(IntPtr baseAddress, long regionSize, byte?[] needle, List<long> results)
        {
            const int chunkSize = 1024 * 1024;
            var overlap = Math.Max(needle.Length - 1, 0);
            long offset = 0;
            byte[] previousTail = new byte[0];

            while (offset < regionSize)
            {
                var readSize = (int)Math.Min(chunkSize, regionSize - offset);
                var chunk = new byte[readSize];
                IntPtr bytesRead;
                if (!ReadProcessMemory(processHandle, new IntPtr(baseAddress.ToInt64() + offset), chunk, (UIntPtr)chunk.Length, out bytesRead) || bytesRead.ToInt64() <= 0)
                {
                    offset += readSize;
                    previousTail = new byte[0];
                    continue;
                }

                Array.Resize(ref chunk, (int)bytesRead.ToInt64());
                var scanBuffer = previousTail.Concat(chunk).ToArray();
                var scanBase = baseAddress.ToInt64() + offset - previousTail.Length;

                for (var i = 0; i <= scanBuffer.Length - needle.Length; i++)
                {
                    if (IsMatch(scanBuffer, i, needle))
                    {
                        results.Add(scanBase + i);
                    }
                }

                var tailSize = Math.Min(overlap, chunk.Length);
                previousTail = new byte[tailSize];
                Array.Copy(chunk, chunk.Length - tailSize, previousTail, 0, tailSize);
                offset += chunk.Length;
            }
        }

        private IntPtr ResolveAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return IntPtr.Zero;
            }

            var parts = address.Split('+');
            if (parts.Length == 2)
            {
                var moduleBase = GetModuleBase(parts[0]);
                if (moduleBase == IntPtr.Zero || !TryParseHex(parts[1], out var offset))
                {
                    return IntPtr.Zero;
                }

                return new IntPtr(moduleBase.ToInt64() + offset);
            }

            return TryParseHex(address, out var value) ? new IntPtr(value) : IntPtr.Zero;
        }

        private IntPtr GetModuleBase(string moduleName)
        {
            try
            {
                var module = process.Modules.Cast<ProcessModule>()
                    .FirstOrDefault(m => string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));
                if (module != null)
                {
                    return module.BaseAddress;
                }
            }
            catch
            {
            }

            const uint TH32CS_SNAPMODULE = 0x00000008;
            const uint TH32CS_SNAPMODULE32 = 0x00000010;
            var snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)process.Id);
            if (snapshot == IntPtr.Zero || snapshot.ToInt64() == -1)
            {
                return IntPtr.Zero;
            }

            try
            {
                var entry = new MODULEENTRY32();
                entry.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));
                if (Module32FirstW(snapshot, ref entry))
                {
                    do
                    {
                        if (string.Equals(entry.szModule, moduleName, StringComparison.OrdinalIgnoreCase))
                        {
                            return entry.modBaseAddr;
                        }
                    } while (Module32NextW(snapshot, ref entry));
                }
            }
            finally
            {
                CloseHandle(snapshot);
            }

            return IntPtr.Zero;
        }

        private static bool TryParseHex(string value, out long result)
        {
            value = value.Trim();
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
            }

            return long.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        private static byte?[] ParsePattern(string pattern)
        {
            return pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Contains("?") ? (byte?)null : byte.Parse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
                .ToArray();
        }

        private static bool IsMatch(byte[] buffer, int index, byte?[] needle)
        {
            for (var i = 0; i < needle.Length; i++)
            {
                if (needle[i].HasValue && buffer[index + i] != needle[i].Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsReadable(uint protect, uint state)
        {
            if (state != (uint)MemoryState.Commit)
            {
                return false;
            }

            var p = (MemoryProtect)protect;
            return !p.HasFlag(MemoryProtect.NoAccess) && !p.HasFlag(MemoryProtect.Guard);
        }

        private static bool MatchesProtection(uint protect, bool writable, bool executable)
        {
            var p = (MemoryProtect)protect;
            if (writable && !(p.HasFlag(MemoryProtect.ReadWrite) || p.HasFlag(MemoryProtect.ExecuteReadWrite) || p.HasFlag(MemoryProtect.WriteCopy) || p.HasFlag(MemoryProtect.ExecuteWriteCopy)))
            {
                return false;
            }

            if (executable && !(p.HasFlag(MemoryProtect.Execute) || p.HasFlag(MemoryProtect.ExecuteRead) || p.HasFlag(MemoryProtect.ExecuteReadWrite) || p.HasFlag(MemoryProtect.ExecuteWriteCopy)))
            {
                return false;
            }

            return true;
        }
    }
}
