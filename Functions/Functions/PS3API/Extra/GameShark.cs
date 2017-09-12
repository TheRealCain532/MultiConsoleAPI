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
        int ConcatIntString(int x, int y)
        {
            string _x = x.ToString("X"), _y = y.ToString("X");
            return (int)(int.Parse(_x) * Math.Pow(10, y.ToString().Length)) + int.Parse(_y);
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


        private string[] CodeList = new string[1000];
        private int Final, _length, _jump;
        private bool Is_Connected, serial, _case;
        public string ProcessName;
        private byte[][] InputBytes = new byte[1000][],//for item modifier codes
            OrigBytes = new byte[1000][];//for toggling things.
        public int CFW()
        {
            int buff = 0;
            switch (CCAPI.GetFirmwareVersion())
            {
                case "4.78":
                    buff = 0;
                    break;
                case "4.80":
                    buff = 0x08;
                    break;
            }
            return buff;
        }
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
        private bool Process(uint PID, string Proc)
        {
            bool state = false;
            CCAPI.GetProcessName(PID, out Proc);
            state = Proc.Contains("ps1_netemu");
            Final = state ? 0x770788 : 0x2cb3f0;
            return state;
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
        private void Codes(string input, uint address, byte[] bytes)
        {
            switch (input.Substring(0, Math.Min(2, input.Length)))
            {
                case "80": //Constant Write
                    Extension.WriteBytes(address, bytes);
                    break;
                case "30": //Constant Write
                    Extension.WriteBytes(address, bytes);
                    break;
                case "10": //Increment Value
                    int[] WInc = new int[2]; //What to increase by
                    WInc[0] = bytes[0];
                    WInc[1] = bytes[1];
                    int[] TInc = new int[2]; // What we are increasing
                    TInc[0] = Extension.ReadByte(address);
                    TInc[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TInc[0] + WInc[0]));
                    Extension.WriteByte(address + 1, (byte)(TInc[1] + WInc[1]));
                    break;
                case "20": //Increment Value
                    int[] WInc2 = new int[2]; //What to increase by
                    WInc2[0] = bytes[0];
                    WInc2[1] = bytes[1];
                    int[] TInc2 = new int[2]; //What we are increasing
                    TInc2[0] = Extension.ReadByte(address);
                    TInc2[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TInc2[0] + WInc2[0]));
                    Extension.WriteByte(address + 1, (byte)(TInc2[1] + WInc2[1]));
                    break;
                case "11": //Decrement Value
                    int[] WDec = new int[2]; //What to Decrease by
                    WDec[0] = bytes[0];
                    WDec[1] = bytes[1];
                    int[] TDec = new int[2]; // What we are Decreasing
                    TDec[0] = Extension.ReadByte(address);
                    TDec[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TDec[0] - WDec[0]));
                    Extension.WriteByte(address + 1, (byte)(TDec[1] - WDec[1]));
                    break;
                case "21": //Decrement Value
                    int[] WDec2 = new int[2]; //What to Decrease by
                    WDec2[0] = bytes[0];
                    WDec2[1] = bytes[1];
                    int[] TDec2 = new int[2]; // What we are Decreasing
                    TDec2[0] = Extension.ReadByte(address);
                    TDec2[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TDec2[0] - WDec2[0]));
                    Extension.WriteByte(address + 1, (byte)(TDec2[1] - WDec2[1]));
                    break;
            }

        }
        private void Codes(string input)
        {
            uint address = GSC(input);
            byte[] bytes = GSB(input);
            Console.WriteLine("Running");
            switch (input.Substring(0, Math.Min(2, input.Length)))
            {
                case "80": //Constant Write
                    Extension.WriteBytes(address, bytes);
                    break;
                case "30": //Constant Write
                    Extension.WriteBytes(address, bytes);
                    break;
                case "10": //Increment Value
                    int[] WInc = new int[2]; //What to increase by
                    WInc[0] = bytes[0];
                    WInc[1] = bytes[1];
                    int[] TInc = new int[2]; // What we are increasing
                    TInc[0] = Extension.ReadByte(address);
                    TInc[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TInc[0] + WInc[0]));
                    Extension.WriteByte(address + 1, (byte)(TInc[1] + WInc[1]));
                    break;
                case "20": //Increment Value
                    int[] WInc2 = new int[2]; //What to increase by
                    WInc2[0] = bytes[0];
                    WInc2[1] = bytes[1];
                    int[] TInc2 = new int[2]; //What we are increasing
                    TInc2[0] = Extension.ReadByte(address);
                    TInc2[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TInc2[0] + WInc2[0]));
                    Extension.WriteByte(address + 1, (byte)(TInc2[1] + WInc2[1]));
                    break;
                case "11": //Decrement Value
                    int[] WDec = new int[2]; //What to Decrease by
                    WDec[0] = bytes[0];
                    WDec[1] = bytes[1];
                    int[] TDec = new int[2]; // What we are Decreasing
                    TDec[0] = Extension.ReadByte(address);
                    TDec[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TDec[0] - WDec[0]));
                    Extension.WriteByte(address + 1, (byte)(TDec[1] - WDec[1]));
                    break;
                case "21": //Decrement Value
                    int[] WDec2 = new int[2]; //What to Decrease by
                    WDec2[0] = bytes[0];
                    WDec2[1] = bytes[1];
                    int[] TDec2 = new int[2]; // What we are Decreasing
                    TDec2[0] = Extension.ReadByte(address);
                    TDec2[1] = Extension.ReadByte(address + 1);
                    Extension.WriteByte(address, (byte)(TDec2[0] - WDec2[0]));
                    Extension.WriteByte(address + 1, (byte)(TDec2[1] - WDec2[1]));
                    break;
            }

        }
        private void GSMagic(string input)
        {
            if (Is_Connected && ((input != "") | (input != null)))
            {
                CodeList = input.Split('\n');
                for (int i = 0; i < CodeList.Count<string>(); i++)
                {
                    if (CodeList[i] != null)
                    {
                        if (CodeList[i].StartsWith("D") | CodeList[i].StartsWith("E"))
                        {
                            switch (CodeList[i].Substring(1, Math.Min(1, CodeList[i].Length)))
                            {
                                case "0": //Equal To
                                    byte[] ToCheck0 = GSB(CodeList[i]),//Byte to Check
                                        Check0 = Extension.ReadBytes(GSC(CodeList[i]), 2);//Byte at address
                                    _case = ByteCompare(Check0, ToCheck0);
                                    break;
                                case "4": //Equal To
                                    byte[] ToCheck4 = GSB(CodeList[i]),//Byte to Check
                                        Check4 = Extension.ReadBytes(GSC(CodeList[i]), 2);//Byte at address
                                    _case = ByteCompare(Check4, ToCheck4);
                                    break;
                                case "1": //Different To
                                    byte[] ToCheck1 = GSB(CodeList[i]),//Byte to Check
                                        Check1 = Extension.ReadBytes(GSC(CodeList[i]), 2);//Byte at address
                                    _case = (Check1[1] != ToCheck1[1]);
                                    break;
                                case "2": //Less Than
                                    byte[] ToCheck2 = GSB(CodeList[i]),//Byte to Check
                                        Check2 = Extension.ReadBytes(GSC(CodeList[i]), 2);//Byte at address
                                    _case = (Check2[1] < ToCheck2[1]);
                                    break;
                                case "3": //Greater Than
                                    byte[] ToCheck3 = GSB(CodeList[i]),//Byte to Check
                                        Check3 = Extension.ReadBytes(GSC(CodeList[i]), 2);//Byte at address
                                    _case = (Check3[1] > ToCheck3[1]);
                                    break;
                                case "5": //Button Map Compare
                                    byte[] ToCheck5 = GSB(CodeList[i]), //Byte to Check
                                        Check5 = GSB(CodeList[i]); //Button Mapping
                                    _case = (ToCheck5 == Check5); //Compare with Button Mapping
                                    break;
                            }
                        }
                        else if (CodeList[i].StartsWith("5"))//Serial Repeater
                        {
                            string _Parse = CodeList[i].Split(' ')[0].Insert(4, ":").Insert(7, "-").Split(':')[1];
                            _length = int.Parse(_Parse.Split('-')[0]);
                            _jump = int.Parse(_Parse.Split('-')[1]);
                            byte[] _ToAdd = GSB(CodeList[i]);//parses first line for params
                            serial = true;
                        }
                        if (serial)
                        {
                            uint address = GSC(CodeList[i]);
                            for (uint a = 0; a < _length; a++)
                            {
                                address = (uint)(address + _jump);
                                Codes(CodeList[i], address, GSB(CodeList[i]));
                            }
                            serial = false;
                        }
                        if (_case)
                        {
                            Codes(CodeList[i], GSC(CodeList[i]), GSB(CodeList[i]));
                        }
                    }
                }
            }
        }
        private string[] _cont = { "80", "30", "10", "20", "11", "21" };
        //public void GSWrite(string[] input)
        //{
        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        if (!input[0].StartsWith(_cont[i]))
        //            GSMagic(input[i]);
        //        else
        //            Codes(input[i]);
        //    }
        //}
        //public void GSWrite(string input)
        //{
        //    if (input != "")
        //        Codes(input);
        //}
    }
}