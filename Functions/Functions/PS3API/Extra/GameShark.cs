using System;
using System.Linq;
using System.Windows.Forms;

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
        private bool add;
        private string[] CodeList = new string[0x3e8];
        private int Final;
        private bool Is_Connected;
        private Timer MagicJunk = new Timer();
        private uint[] ProceccID;
        private string ProcessName;

        private byte[] GSByte(string input)
        {
            byte[] array = StringToByteArray(input);
            Array.Reverse(array);
            return array;
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
                                bool flag6 = CCAPI.ConnectTarget();
                                CCAPI.GetProcessList(out ProceccID);
                                CCAPI.AttachProcess(ProceccID[0]);
                                if (flag6)
                                {
                                    string name = "";
                                    CCAPI.GetProcessName(ProceccID[0], out name);
                                    if (MessageBox.Show(string.Format("Your Playstion 1 Process is {0} \n Would you like to save this setting?", name.Substring(0x12)), "Save setting", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        Process();
                                        MessageBox.Show("Saved");
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            break;
                    }
                }
                else
                {
                    CCAPI.GetProcessList(out ProceccID);
                    CCAPI.GetProcessName(ProceccID[0], out ProcessName);
                    Process();
                }
            }
            char[] separator = new char[] { ' ' };
            return (Convert.ToUInt32(GSCode.Split(separator)[0].Remove(0, 2), 0x10) + ((uint) Final));
        }

        public void GSMagic(string input)
        {
            if (Is_Connected && ((input != "") | (input != null)))
            {
                char[] separator = new char[] { '\n' };
                CodeList = input.Split(separator);
                uint[] numArray = new uint[0x7a120];
                for (int i = 0; i < CodeList.Count<string>(); i++)
                {
                    if (CodeList[i] != null)
                    {
                        if (CodeList[i].StartsWith("80") | CodeList[i].StartsWith("30"))
                        {
                            char[] chArray2 = new char[] { ' ' };
                            numArray[i] = GSC(CodeList[i].Split(chArray2)[0]);
                            char[] chArray3 = new char[] { ' ' };
                            byte[] array = StringToByteArray(CodeList[i].Split(chArray3)[1]);
                            Array.Reverse(array);
                            Extension.WriteBytes(numArray[i], array);
                        }
                        if (CodeList[i].StartsWith("D0"))
                        {
                            char[] chArray4 = new char[] { ' ' };
                            numArray[i] = GSC(CodeList[i].Split(chArray4)[0]);
                            char[] chArray5 = new char[] { ' ' };
                            byte[] buffer2 = StringToByteArray(CodeList[i].Split(chArray5)[1]);
                            byte[] buffer3 = Extension.ReadBytes(numArray[i], 2);
                            char[] chArray6 = new char[] { ' ' };
                            uint offset = GSC(CodeList[i + 1].Split(chArray6)[0]);
                            char[] chArray7 = new char[] { ' ' };
                            byte[] buffer4 = StringToByteArray(CodeList[i + 1].Split(chArray7)[1]);
                            Array.Reverse(buffer4);
                            Array.Reverse(buffer3);
                            string str = BitConverter.ToString(buffer2).Replace("-", "");
                            string str2 = BitConverter.ToString(buffer3).Replace("-", "");
                            int num3 = Convert.ToInt16(buffer4[0]);
                            int num4 = Convert.ToInt16(buffer4[1]);
                            Console.WriteLine(string.Format("{0} - {1}", str, str2));
                            if (str.Replace("0", "") == str2.Replace("0", ""))
                            {
                                if (CodeList[i + 1].StartsWith("10"))
                                {
                                    add = true;
                                }
                                if (CodeList[i + 1].StartsWith("11"))
                                {
                                    add = false;
                                }
                                if (CodeList[i + 1].StartsWith("10") | CodeList[i + 1].StartsWith("11"))
                                {
                                    byte[] buffer5 = Extension.ReadBytes(offset, 2);
                                    Array.Reverse(buffer5);
                                    buffer5[0] = (byte) (buffer5[0] + num3);
                                    buffer5[1] = (byte) (buffer5[1] + num4);
                                    num3 = add ? num3++ : num3--;
                                    num4 = add ? num4++ : num4--;
                                    Extension.WriteByte(offset, buffer5[0]);
                                    Extension.WriteByte(offset + 1, buffer5[1]);
                                }
                                if (CodeList[i + 1].StartsWith("80"))
                                {
                                    Extension.WriteBytes(offset, buffer4);
                                }
                            }
                        }
                    }
                    if (CodeList[i].StartsWith("50"))
                    {
                        char[] chArray8 = new char[] { ' ' };
                        numArray[i] = GSC(CodeList[i].Split(chArray8)[0]);
                        if (CodeList[i].StartsWith("50"))
                        {
                            char[] chArray9 = new char[] { ' ' };
                            char[] chArray10 = new char[] { ':' };
                            string str4 = CodeList[i].Split(chArray9)[0].Insert(4, ":").Insert(7, "-").Split(chArray10)[1];
                            char[] chArray11 = new char[] { '-' };
                            int num5 = Convert.ToInt16(str4.Split(chArray11)[0]);
                            char[] chArray12 = new char[] { '-' };
                            int num6 = Convert.ToInt16(str4.Split(chArray12)[1]);
                            char[] chArray13 = new char[] { ' ' };
                            int num7 = Convert.ToInt32(CodeList[i].Split(chArray13)[1]);
                            for (int j = 0; j < num5; j++)
                            {
                                char[] chArray14 = new char[] { ' ' };
                                int num9 = Convert.ToInt32(CodeList[i + 1].Split(chArray14)[1]);
                                byte[] buffer6 = new byte[1];
                                for (int k = 0; k < 1; k++)
                                {
                                    buffer6[k] = Convert.ToByte(num9);
                                }
                                Extension.WriteBytes(numArray[i + 1], buffer6);
                                numArray[i] += (uint) num6;
                                num9 += num7;
                            }
                        }
                    }
                }
            }
        }

        private bool Process()
        {
            bool flag = false;
            if (ProcessName.Contains("ps1_netemu"))
            {
                flag = true;
            }
            if (ProcessName.Contains("ps1_emu"))
            {
                flag = false;
            }
            Final = flag ? 0x770788 : 0x2cb3f0;
            return flag;
        }

        public bool PSXConnect(MultiConsoleAPI api)
        {
            uint[] numArray;
            bool flag = api.CCAPI.ConnectTarget();
            api.CCAPI.GetProcessList(out numArray);
            if (flag)
            {
                api.CCAPI.AttachProcess(numArray[0]);
            }
            CCAPI.GetProcessName(numArray[0], out ProcessName);
            Process();
            if (flag)
            {
                MagicJunk.Tick += new EventHandler(GameShark.MagicJunk_Tick);
                MagicJunk.Interval = 1;
            }
            MagicJunk.Enabled = flag;
            Is_Connected = flag;
            return flag;
        }
        private static void MagicJunk_Tick(object sender, EventArgs e)
        {

        }
        private byte[] StringToByteArray(string hex)
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
    }
}

