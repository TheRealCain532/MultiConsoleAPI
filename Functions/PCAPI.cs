using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MultiLib
{
    public class PCAPI
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesRead);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        // Fields
        private static Process GameProcess;

        public PCAPI()
        {
        }
        // Methods
        public Boolean ConnectTarget(int pProcessId)
        {
            bool state = false;
            Process[] ProcID = Process.GetProcesses();
            for (int i = 0; i < ProcID.Length; i++)
                if (ProcID[i].Id == pProcessId)
                {
                    GameProcess = ProcID[i];
                    state = true;
                    break;
                }
                else state = false;
            return state;
        }
        public Boolean ConnectTarget(string pProcessName)
        {
            bool state = false;
            Process[] ProcID = Process.GetProcesses();
            for (int i = 0; i < ProcID.Length; i++)
                if (ProcID[i].ProcessName.Contains(pProcessName))
                {
                    GameProcess = ProcID[i];
                    state = true;
                    break;
                }
                else state = false;
            return state;
        }
        public string CurrentProcess()
        {
            return GameProcess.ProcessName;
        }
        public bool CheckProcess()
        {
            return GameProcess.ProcessName.Length > 0;
        }

        public void GetMemory(uint Address, byte[] Bytes)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
            ReadProcessMemory(hProc, (int)Address, Bytes, Bytes.Length, 0);
            CloseHandle(hProc);
        }
        public void GetMemory(ulong Address, byte[] Bytes)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
            ReadProcessMemory(hProc, (int)Address, Bytes, Bytes.Length, 0);
            CloseHandle(hProc);
        }
        public byte[] GetBytes(uint address, uint lengthByte)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
            byte[] buffer = new byte[lengthByte];
            ReadProcessMemory(hProc, (int)address, buffer, (int)lengthByte, 0);
            CloseHandle(hProc);
            return buffer;
        }
        public void SetMemory(int address, long v)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
            var val = new byte[] { (byte)v };

            int wtf = 0;
            WriteProcessMemory(hProc, new IntPtr(address), val, (UInt32)val.LongLength, out wtf);

            CloseHandle(hProc);
        }
        public void SetMemory(uint offset, byte[] Bytes)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
            var val = Bytes;

            int outbound = 0;
            WriteProcessMemory(hProc, new IntPtr(offset), val, (UInt32)val.LongLength, out outbound);

            CloseHandle(hProc);
        }
        public Extension Extension
        {
            get { return new Extension(SelectAPI.PCAPI); }
        }
        //public void WriteByte(uint offset, byte _byte)
        //{
        //    var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
        //    var val = new byte[1];
        //    val[0] = _byte;
        //    int outbound = 0;
        //    WriteProcessMemory(hProc, new IntPtr(offset), val, 1, out outbound);
        //}
        //public byte ReadByte(uint offset)
        //{
        //    var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
        //    byte[] buffer = new byte[1];
        //    ReadProcessMemory(hProc, (int)offset, buffer, 1, 0);
        //    return buffer[0];
        //}
        //public byte[] ReadBytes(uint offset, int length)
        //{
        //    var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)GameProcess.Id);
        //    var buffer = new byte[length];
        //    ReadProcessMemory(hProc, (int)offset, buffer, 1, 0);
        //    return buffer;
        //}
        //public int ReadInt(uint offset)
        //{
        //    return BitConverter.ToInt32(this.ReadBytes(offset, 4), 0);
        //}
    }

}
