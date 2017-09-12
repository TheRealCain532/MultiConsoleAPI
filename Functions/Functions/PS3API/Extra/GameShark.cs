using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiLib;
using System.Windows.Forms;
using System.Collections;
namespace MultiLib
{
    public class GameShark
    {
        private Extension Extension
        {
            get { return new Extension(SelectAPI.ControlConsole); }
        }
        private CCAPI CCAPI
        {
            get { return new CCAPI(); }
        }
        uint BtnMonitor;
        enum CodeType
        {
            ConstantWrite,
            EqualTo,
            DifferentTo,
            LessThan,
            GreaterThan,
            IncrementValue,
            DecrementValue,
            UniversalActivator,
            UniversalDeActivator,
            AllCodeButton,
            SerialRepeater,
            ActivateAll,
            ActivateAll_Delay,
            CopyBytes,
        }
        private int[] SplitInt(int input)
        {
            int a = input;
            a = Math.Abs(a);
            int length = a.ToString().Length;
            int[] array = new int[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = a % 10;
                a /= 10;
            }
            Array.Reverse(array);
            return array;
        }
        byte[] SplitToByte(int[] input)
        {
            byte[] array = new byte[input.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = Convert.ToByte(input[i]);
            return array;
        }
        int ConcatInt(int x, int y)
        {
            return (int)(x * Math.Pow(10, y.ToString().Length)) + y;
        }
        private void Execute(CodeType T, uint[] address, byte[][] bytes)
        {
            Console.WriteLine("multiline is called");
            byte[] compare = new byte[10];
            int[] cmp = new int[10];
            switch (T)
            {
                case CodeType.EqualTo: ///E0 D0
                    compare = Extension.ReadBytes(address[0], 2);
                    if (ByteCompare(compare, bytes[0]))
                        Execute(T, address[1], bytes[1]);
                    break;
                case CodeType.DifferentTo: ///E1 D1
                    compare = Extension.ReadBytes(address[0], 2);
                    if (!ByteCompare(compare, bytes[0]))
                        Execute(T, address, bytes);
                    break;
                case CodeType.LessThan: ///E2 D2
                    compare = Extension.ReadBytes(address[0], 2);
                    cmp[0] = bytes[0][0] + bytes[0][1];
                    cmp[1] = compare[0] + compare[1];
                    if (cmp[1] < cmp[0])
                        Execute(T, address[1], bytes[1]);
                    break;
                case CodeType.GreaterThan: ///E3 D3
                    compare = Extension.ReadBytes(address[0], 2);
                    cmp[0] = bytes[0][0] + bytes[0][1];
                    cmp[1] = compare[0] + compare[1];
                    if (cmp[1] > cmp[0])
                        Execute(T, address[1], bytes[1]);
                    break;
                case CodeType.UniversalActivator: ///D4
                    compare = Extension.ReadBytes(address[0], 2);
                    if (ByteCompare(compare, bytes[0]))
                        Execute(T, address[1], bytes[1]);
                    break;
                case CodeType.SerialRepeater: ///50
                    //length is 8, parsable is 6 50 00 XX YY 00 ZZ ==> XX = length YY = jump ZZ = To Add (as increment)
                    //TTTTTTTT VVVV ==> TTTTTTTT = start address VVVV = value written at each address + ZZ
                    address[0] = address[0] - 0x2cb3f0;
                    int[] magic = SplitInt(int.Parse(address[0].ToString("X")));
                    uint TTTTTTTT = address[1];
                    int
                        XX = ConcatIntString(magic[0], magic[1]),
                        YY = ConcatInt(magic[2], magic[3]),
                        ZZ = bytes[0][0] + bytes[0][1];
                    byte[] VVVV = bytes[1];

                    Console.WriteLine(string.Format("XX = {0} YY = {1} ZZ = {2} start = {3:X} writing {4}", XX, YY, ZZ, TTTTTTTT, BitConverter.ToString(VVVV)));
                    for (int i = 0; i < XX; i++)
                    {
                        Extension.WriteBytes(TTTTTTTT, VVVV);
                        if (ZZ > 0)
                            VVVV = BitConverter.GetBytes(VVVV[0] + ZZ);
                        TTTTTTTT = TTTTTTTT + (uint)YY;
                    }
                    break;
                case CodeType.AllCodeButton: ///D5
                    compare = Extension.ReadBytes(BtnMonitor, 2);
                    if (ByteCompare(compare, bytes[0]))
                        Execute(T, address[1], bytes[1]);
                    break;
                default: break;
            }
        }

        private void Execute(CodeType T, uint address, byte[] bytes)
        {
            Console.WriteLine("single line is called");

            //Array.Reverse(bytes);
            byte[] compare = new byte[10];
            int[] cmp = new int[10];
            switch (T)
            {
                case CodeType.ConstantWrite: ///80 30
                    Extension.WriteBytes(address, bytes);
                    break;
                case CodeType.UniversalDeActivator: ///D6
                    //workin' on it
                    break;
                case CodeType.IncrementValue: ///20 
                    compare = Extension.ReadBytes(address, 2);
                    cmp[0] = bytes[0] + bytes[1]; // To Add
                    cmp[1] = compare[0] + compare[1]; // Add to
                    Extension.WriteBytes(address, BitConverter.GetBytes(cmp[0] + cmp[1]));
                    break;
                case CodeType.DecrementValue: ///21 
                    compare = Extension.ReadBytes(address, 2);
                    cmp[0] = bytes[0] + bytes[1]; // To Add
                    cmp[1] = compare[0] + compare[1]; // Add to
                    Extension.WriteBytes(address, BitConverter.GetBytes(cmp[0] - cmp[1]));
                    break;
                default :break;
            }
        }

        public void GSWrite(string input)
        {
            switch (input.Substring(0, Math.Min(2, input.Length)))
            {
                case "80":
                    Execute(CodeType.ConstantWrite, GSC(input), GSB(input));
                    break;
                case "30":
                    Execute(CodeType.ConstantWrite, GSC(input), GSB(input));
                    break;
                case "E0":
                    Execute(CodeType.ConstantWrite, GSC(input), GSB(input));
                    break;
                case "10":
                    Execute(CodeType.IncrementValue, GSC(input), GSB(input));
                    break;
                case "20":
                    Execute(CodeType.IncrementValue, GSC(input), GSB(input));
                    break;
                case "11":
                    Execute(CodeType.DecrementValue, GSC(input), GSB(input));
                    break;
                case "21":
                    Execute(CodeType.DecrementValue, GSC(input), GSB(input));
                    break;
            }

        }
        public void GSWrite(string[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i].Substring(0, Math.Min(2, input[i].Length)))
                {
                    case "E0":
                        Execute(CodeType.EqualTo, GSC(input), GSB(input));
                        break;
                    case "D0":
                        Execute(CodeType.EqualTo, GSC(input), GSB(input));
                        break;
                    case "E3":
                        Execute(CodeType.GreaterThan, GSC(input), GSB(input));
                        break;
                    case "D3":
                        Execute(CodeType.GreaterThan, GSC(input), GSB(input));
                        break;
                    case "E2":
                        Execute(CodeType.LessThan, GSC(input), GSB(input));
                        break;
                    case "D2":
                        Execute(CodeType.LessThan, GSC(input), GSB(input));
                        break;
                    case "50":
                        Execute(CodeType.SerialRepeater, GSC(input), GSB(input));
                        break;
                    case "D5":
                        Execute(CodeType.AllCodeButton, GSC(input), GSB(input));
                        break;
                }
                GSWrite(input[i]);
            }
        }
        private bool Is_Connected;
        public Boolean Connect()
        {
            uint[] PID;
            Is_Connected = CCAPI.ConnectTarget();
            CCAPI.GetProcessList(out PID);
            if (Is_Connected)
                Is_Connected = CCAPI.AttachProcess(PID[0]) >= 0;
            if (Is_Connected)
            {
                CCAPI.GetProcessName(PID[0], out ProcessName);
                Final = ProcessName.Contains("ps1_netemu") ? 0x770788 : 0x2cb3f0;
                Final = Final - CFW();
            }
            return Is_Connected;
        }

        public uint GSC(string GSCode)
        {
            if ((GSCode != "") | (GSCode != null))
            {
                if (ProcessName == "")
                {
                    switch (MessageBox.Show("If Process is 'ps1_emu' press 'Yes' \nif Process is ps1_netemu, press 'No' \nif Unsure, press 'Cancel'", "PS3 Not Connected", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            Final = 0x2cb3f0;
                            break;

                        case DialogResult.No:
                            Final = 0x770788;
                            break;

                        case DialogResult.Cancel:
                            try
                            {
                                bool state = Connect();
                                if (state)
                                    MessageBox.Show(string.Format("Your Playstion 1 Process is {0}", ProcessName.Substring(0x12)), "{0}", MessageBoxButtons.OK);
                            }
                            catch (Exception)
                            {
                            }
                            break;
                    }
                }
            }
            return (Convert.ToUInt32(GSCode.Split(' ')[0].Remove(0, 2), 0x10) + 0x2cb3f0);
        }
        public uint[] GSC(string[] GSCode)
        {
            uint[] array = new uint[GSCode.Length];
            if ((GSCode[0] != "") | (GSCode != null))
            {
                if (ProcessName == "")
                {
                    switch (MessageBox.Show("If Process is 'ps1_emu' press 'Yes' \nif Process is ps1_netemu, press 'No' \nif Unsure, press 'Cancel'", "PS3 Not Connected", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            Final = 0x2cb3f0;
                            break;

                        case DialogResult.No:
                            Final = 0x770788;
                            break;

                        case DialogResult.Cancel:
                            try
                            {
                                bool state = Connect();
                                if (state)
                                    MessageBox.Show(string.Format("Your Playstion 1 Process is {0}", ProcessName.Substring(0x12)), "{0}", MessageBoxButtons.OK);
                            }
                            catch (Exception)
                            {
                            }
                            break;
                    }
                }
            }
            for (int i = 0; i < GSCode.Length; i++)
                array[i] = Convert.ToUInt32(GSCode[i].Split(' ')[0].Remove(0, 2), 0x10) + 0x2cb3f0;
            return array;
        }
        public byte[] GSB(string input)

        {
            byte[] array = STB(input.Split(' ')[1]);
            Array.Reverse(array);
            return array;
        }
        public byte[][] GSB(string[] input)

        {
            byte[][] array = new byte[input.Length][];
            for (int i = 0; i < input.Length; i++)
            {
                array[i] = STB(input[i].Split(' ')[1]);
                Array.Reverse(array[i]);
            }
            return array;
        }

        private byte[] STB(string hex)
        {
            if ((hex.Length % 2) > 0)
            {
                hex = "0" + hex;
            }
            int length = hex.Length;
            byte[] buffer = new byte[((length / 2) - 1) + 1];
            for (int i = 0; i < length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(hex.Substring(i, 2), 0x10);
            }
            return buffer;
        }

        static bool ByteCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }
    }
}