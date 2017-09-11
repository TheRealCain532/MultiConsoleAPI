using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using XDevkit;
using System.IO;
using MultiLib.Properties;
namespace MultiLib
{
    public class XboxAPI
    {
        private XboxConsole xbCon;
        private XboxManager xboxMgr;
        private IXboxDebugTarget dbgXbox;
        private bool activeConnection;
        private uint xbConnection;
        private static uint g;
        private static uint meh;
        private static int firstRan;
        private uint bufferAddress;
        private uint stringPointer;
        private uint floatPointer;
        private uint bytePointer;
        private byte[] nulled = new byte[100];
        private uint[] buffcheck = new uint[15];
        private uint[] result = new uint[999];

        public enum RebootType
        {
            Cold,
            Title
        }
        public string DeviceIdent { get; private set;}
        public string XboxType { get; private set; }
        public bool IsConnected { get; private set; }

        public uint connectioncode;
        bool xdev;
        public Boolean ConnectTarget()
        {
                if (!IsConnected)
                {
                    try
                    {
                        xboxMgr = new XboxManager();
                        xbCon = xboxMgr.OpenConsole(DeviceIdent);
                        dbgXbox = xbCon.DebugTarget;
                        connectioncode = xbCon.OpenConnection(null);
                    IsConnected = true;
                }
                catch
                    {
                        xbCon = null;
                        xboxMgr = null;
                        dbgXbox = null;
                        return false;
                    }
                    try
                    {
                        XboxType = xbCon.ConsoleType.ToString();
                    }
                    catch
                    {
                        XboxType = "Can't get";
                    }
                }
                return IsConnected;
        }
        public void Disconnect()
        {
            if (!IsConnected) return;

            if (xbCon != null)
                xbCon.CloseConnection(connectioncode);

            xboxMgr = null;
            dbgXbox = null;
            xbCon = null;
            IsConnected = false;
        }
        public string SendStringCommand(string command)
        {
            if (!ConnectTarget()) return null;
            string response;
            xbCon.SendTextCommand(connectioncode, command, out response);
            return response;
        }
        public bool Freeze(bool Bool = true)
        {
            Bool = !Bool;
            SendStringCommand(Bool ? "stop" : "go");
            return Bool;
        }
        public void Freeze()
        {
            if (!ConnectTarget()) return;
            SendStringCommand("stop");
        }
        public void UnFreeze()
        {
            if (!ConnectTarget()) return;
            SendStringCommand("go");
        }

        public void Reboot(RebootType rebootType)
        {
            if (!ConnectTarget()) return;
            switch (rebootType)
            {
                case RebootType.Cold:
                    SendStringCommand("reboot");
                    break;
                case RebootType.Title:
                    SendStringCommand("reboot");
                    break;
            }
        }
        public void Shutdown()
        {
            if (!ConnectTarget())
                return;
            SendStringCommand("bye");
            Disconnect();
        }
        public bool GetScreenshot(string savePath, bool freezeDuring = false)
        {
            if (!ConnectTarget())
                return false;
            if (freezeDuring)
                Freeze();
            xbCon.ScreenShot(savePath+".bmp");
            if (freezeDuring)
                UnFreeze();
            return true;
        }

        public Boolean Connection()
        {
            if (!activeConnection)
            {
                xboxMgr = new XDevkit.XboxManager();
                xbCon = xboxMgr.OpenConsole(xboxMgr.DefaultConsole);
                try
                {
                    xbConnection = xbCon.OpenConnection(null);
                }
                catch (Exception)
                {
                }
                string text;
                string text2;
                if (xbCon.DebugTarget.IsDebuggerConnected(out text, out text2))
                {
                    activeConnection = true;
                }
                xbCon.DebugTarget.ConnectAsDebugger("XboxLib", XboxDebugConnectFlags.Force);
                if (!xbCon.DebugTarget.IsDebuggerConnected(out text, out text2))
                {
                }
                activeConnection = true;
            }
            else
            {
                string text;
                string text2;
                if (xbCon.DebugTarget.IsDebuggerConnected(out text, out text2))
                {
                }
                activeConnection = false;
            }
            return activeConnection;
        }
        //public enum XNotify
        //{
        //    XBOX_LOGO,
        //    NEW_MESSAGE_LOGO,
        //    FRIEND_REQUEST_LOGO,
        //    NEW_MESSAGE,
        //    FLASHING_XBOX_LOGO,
        //    GAMERTAG_SENT_YOU_A_MESSAGE,
        //    GAMERTAG_SINGED_OUT,
        //    GAMERTAG_SIGNEDIN,
        //    GAMERTAG_SIGNED_INTO_XBOX_LIVE,
        //    GAMERTAG_SIGNED_IN_OFFLINE,
        //    GAMERTAG_WANTS_TO_CHAT,
        //    DISCONNECTED_FROM_XBOX_LIVE,
        //    DOWNLOAD,
        //    FLASHING_MUSIC_SYMBOL,
        //    FLASHING_HAPPY_FACE,
        //    FLASHING_FROWNING_FACE,
        //    FLASHING_DOUBLE_SIDED_HAMMER,
        //    GAMERTAG_WANTS_TO_CHAT_2,
        //    PLEASE_REINSERT_MEMORY_UNIT,
        //    PLEASE_RECONNECT_CONTROLLERM,
        //    GAMERTAG_HAS_JOINED_CHAT,
        //    GAMERTAG_HAS_LEFT_CHAT,
        //    GAME_INVITE_SENT,
        //    FLASH_LOGO,
        //    PAGE_SENT_TO,
        //    FOUR_2,
        //    FOUR_3,
        //    ACHIEVEMENT_UNLOCKED,
        //    FOUR_9,
        //    GAMERTAG_WANTS_TO_TALK_IN_VIDEO_KINECT,
        //    VIDEO_CHAT_INVITE_SENT,
        //    READY_TO_PLAY,
        //    CANT_DOWNLOAD_X,
        //    DOWNLOAD_STOPPED_FOR_X,
        //    FLASHING_XBOX_CONSOLE,
        //    X_SENT_YOU_A_GAME_MESSAGE,
        //    DEVICE_FULL,
        //    FOUR_7,
        //    FLASHING_CHAT_ICON,
        //    ACHIEVEMENTS_UNLOCKED,
        //    X_HAS_SENT_YOU_A_NUDGE,
        //    MESSENGER_DISCONNECTED,
        //    BLANK,
        //    CANT_SIGN_IN_MESSENGER,
        //    MISSED_MESSENGER_CONVERSATION,
        //    FAMILY_TIMER_X_TIME_REMAINING,
        //    DISCONNECTED_XBOX_LIVE_11_MINUTES_REMAINING,
        //    KINECT_HEALTH_EFFECTS,
        //    FOUR_5,
        //    GAMERTAG_WANTS_YOU_TO_JOIN_AN_XBOX_LIVE_PARTY,
        //    PARTY_INVITE_SENT,
        //    GAME_INVITE_SENT_TO_XBOX_LIVE_PARTY,
        //    KICKED_FROM_XBOX_LIVE_PARTY,
        //    NULLED,
        //    DISCONNECTED_XBOX_LIVE_PARTY,
        //    DOWNLOADED,
        //    CANT_CONNECT_XBL_PARTY,
        //    GAMERTAG_HAS_JOINED_XBL_PARTY,
        //    GAMERTAG_HAS_LEFT_XBL_PARTY,
        //    GAMER_PICTURE_UNLOCKED,
        //    AVATAR_AWARD_UNLOCKED,
        //    JOINED_XBL_PARTY,
        //    PLEASE_REINSERT_USB_STORAGE_DEVICE,
        //    PLAYER_MUTED,
        //    PLAYER_UNMUTED,
        //    FLASHING_CHAT_SYMBOL,
        //    UPDATING = 76
        //}

        internal byte[] GetBytes(uint offset, uint length)
        {
            byte[] array = new byte[length];
            GetMemory(offset, array);
            return array;
        }
        internal int MemFunc(uint offset, byte[] buffer)
        {
            xbCon.DebugTarget.GetMemory((uint)offset, (uint)buffer.Length, buffer, out result[1]);
            return (int)result[1];
        }
        internal int GetMemory(uint offset, byte[] buffer)
        {
            MemFunc(offset, buffer);
            return (int)result[1];
        }
        internal int MemFunc(ulong offset, byte[] buffer)
        {
            xbCon.DebugTarget.GetMemory((uint)offset, (uint)buffer.Length, buffer, out result[0]);
            return (int)result[0];
        }
        public int GetMemory(ulong offset, byte[] buffer)
        {
            MemFunc(offset, buffer);
            return (int)result[0];
        }

        public byte[] GetMemory(uint address, uint length)
        {
            byte[] array = new byte[length];
            xbCon.DebugTarget.GetMemory(address, length, array, out XboxAPI.g);
            xbCon.DebugTarget.InvalidateMemoryCache(true, address, length);
            return array;
        }
        internal byte[] WideChar(string text)
        {
            byte[] array = new byte[text.Length * 2 + 2];
            int num = 1;
            array[0] = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char value = text[i];
                array[num] = Convert.ToByte(value);
                num += 2;
            }
            return array;
        }
        public void SetMemory(uint address, byte[] data)
        {
            xbCon.DebugTarget.SetMemory(address, (uint)data.Length, data, out XboxAPI.g);
        }
        internal static byte[] getData(long[] argument)
        {
            byte[] array = new byte[argument.Length * 8];
            int num = 0;
            for (int i = 0; i < argument.Length; i++)
            {
                long value = argument[i];
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                bytes.CopyTo(array, num);
                num += 8;
            }
            return array;
        }
        public uint SystemCall(params object[] arg)
        {
            long[] array = new long[9];
            if (!activeConnection)
            {
                Connection();
            }
            if (XboxAPI.firstRan == 0)
            {
                byte[] array2 = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array2, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array2);
                bufferAddress = BitConverter.ToUInt32(array2, 0);
                XboxAPI.firstRan = 1;
                stringPointer = bufferAddress + 1500u;
                floatPointer = bufferAddress + 2700u;
                bytePointer = bufferAddress + 3200u;
                xbCon.DebugTarget.SetMemory(bufferAddress, 100u, nulled, out XboxAPI.meh);
                xbCon.DebugTarget.SetMemory(stringPointer, 100u, nulled, out XboxAPI.meh);
            }
            if (bufferAddress == 0u)
            {
                byte[] array3 = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array3, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array3);
                bufferAddress = BitConverter.ToUInt32(array3, 0);
            }
            stringPointer = bufferAddress + 1500u;
            floatPointer = bufferAddress + 2700u;
            bytePointer = bufferAddress + 3200u;
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < arg.Length; i++)
            {
                object obj = arg[i];
                if (obj is byte)
                {
                    byte[] value = (byte[])obj;
                    array[num2] = (long)((ulong)BitConverter.ToUInt32(value, 0));
                }
                else if (obj is byte[])
                {
                    byte[] array4 = (byte[])obj;
                    xbCon.DebugTarget.SetMemory(bytePointer, (uint)array4.Length, array4, out XboxAPI.meh);
                    array[num2] = (long)((ulong)bytePointer);
                    bytePointer += (uint)(array4.Length + 2);
                }
                else if (obj is float)
                {
                    float value2 = float.Parse(Convert.ToString(obj));
                    byte[] bytes = BitConverter.GetBytes(value2);
                    xbCon.DebugTarget.SetMemory(floatPointer, (uint)bytes.Length, bytes, out XboxAPI.meh);
                    array[num2] = (long)((ulong)floatPointer);
                    floatPointer += (uint)(bytes.Length + 2);
                }
                else if (obj is float[])
                {
                    byte[] array5 = new byte[12];
                    for (int j = 0; j <= 2; j++)
                    {
                        byte[] array6 = new byte[4];
                        Buffer.BlockCopy((Array)obj, j * 4, array6, 0, 4);
                        Array.Reverse(array6);
                        Buffer.BlockCopy(array6, 0, array5, 4 * j, 4);
                    }
                    xbCon.DebugTarget.SetMemory(floatPointer, (uint)array5.Length, array5, out XboxAPI.meh);
                    array[num2] = (long)((ulong)floatPointer);
                    floatPointer += 2u;
                }
                else if (obj is string)
                {
                    byte[] bytes2 = Encoding.ASCII.GetBytes(Convert.ToString(obj));
                    xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes2.Length, bytes2, out XboxAPI.meh);
                    array[num2] = (long)((ulong)stringPointer);
                    string text = Convert.ToString(obj);
                    stringPointer += (uint)(text.Length + 1);
                }
                else
                {
                    array[num2] = Convert.ToInt64(obj);
                }
                num++;
                num2++;
            }
            byte[] data = XboxAPI.getData(array);
            xbCon.DebugTarget.SetMemory(bufferAddress + 8u, (uint)data.Length, data, out XboxAPI.meh);
            byte[] bytes3 = BitConverter.GetBytes(num);
            Array.Reverse(bytes3);
            xbCon.DebugTarget.SetMemory(bufferAddress + 4u, 4u, bytes3, out XboxAPI.meh);
            Thread.Sleep(0);
            byte[] bytes4 = BitConverter.GetBytes(2181038080u);
            Array.Reverse(bytes4);
            xbCon.DebugTarget.SetMemory(bufferAddress, 4u, bytes4, out XboxAPI.meh);
            Thread.Sleep(50);
            byte[] array7 = new byte[4];
            xbCon.DebugTarget.GetMemory(bufferAddress + 4092u, 4u, array7, out XboxAPI.meh);
            xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092u, 4u);
            Array.Reverse(array7);
            return BitConverter.ToUInt32(array7, 0);
        }
        public uint ResolveFunction(string titleID, uint ord)
        {
            if (XboxAPI.firstRan == 0)
            {
                byte[] array = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array);
                bufferAddress = BitConverter.ToUInt32(array, 0);
                XboxAPI.firstRan = 1;
                stringPointer = bufferAddress + 1500u;
                floatPointer = bufferAddress + 2700u;
                bytePointer = bufferAddress + 3200u;
                xbCon.DebugTarget.SetMemory(bufferAddress, 100u, nulled, out XboxAPI.meh);
                xbCon.DebugTarget.SetMemory(stringPointer, 100u, nulled, out XboxAPI.meh);
            }
            byte[] bytes = Encoding.ASCII.GetBytes(titleID);
            xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes.Length, bytes, out XboxAPI.meh);
            long[] array2 = new long[2];
            array2[0] = (long)((ulong)stringPointer);
            string text = Convert.ToString(titleID);
            stringPointer += (uint)(text.Length + 1);
            array2[1] = (long)((ulong)ord);
            byte[] data = XboxAPI.getData(array2);
            xbCon.DebugTarget.SetMemory(bufferAddress + 8u, (uint)data.Length, data, out XboxAPI.meh);
            byte[] bytes2 = BitConverter.GetBytes(2181038081u);
            Array.Reverse(bytes2);
            xbCon.DebugTarget.SetMemory(bufferAddress, 4u, bytes2, out XboxAPI.meh);
            Thread.Sleep(50);
            byte[] array3 = new byte[4];
            xbCon.DebugTarget.GetMemory(bufferAddress + 4092u, 4u, array3, out XboxAPI.meh);
            xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092u, 4u);
            Array.Reverse(array3);
            return BitConverter.ToUInt32(array3, 0);
        }
        //public void Notify(XboxAPI.XNotify type, string text)
        //{
        //    try
        //    {
        //        byte[] array = WideChar(text);
        //        SystemCall(new object[]
        //        {
        //        ResolveFunction("xam.xex", 656u),
        //        Convert.ToUInt32(type),
        //        0,
        //        2,
        //        array,
        //        0
        //        });
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Something Happened!\n\nMake sure you are using XRPC.xex!", "Oh Crap!");
        //    }
        //}
        //public void Notify(string text)
        //{
        //    byte[] array = WideChar(text);
        //    SystemCall(new object[]
        //    {
        //        ResolveFunction("xam.xex", 656u),
        //        Convert.ToUInt32(XNotify.FLASHING_HAPPY_FACE),
        //        0,
        //        2,
        //        array,
        //        0
        //    });
        //}
        internal float[] toFloatArray(double[] arr)
        {
            if (arr == null)
            {
                return null;
            }
            int num = arr.Length;
            float[] array = new float[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = (float)arr[i];
            }
            return array;
        }
        public uint Call(uint address, params object[] arg)
        {
            long[] array = new long[9];
            if (!activeConnection)
            {
                Connection();
            }
            if (XboxAPI.firstRan == 0)
            {
                byte[] array2 = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array2, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array2);
                bufferAddress = BitConverter.ToUInt32(array2, 0);
                XboxAPI.firstRan = 1;
                stringPointer = bufferAddress + 1500u;
                floatPointer = bufferAddress + 2700u;
                bytePointer = bufferAddress + 3200u;
                xbCon.DebugTarget.SetMemory(bufferAddress, 100u, nulled, out XboxAPI.meh);
                xbCon.DebugTarget.SetMemory(stringPointer, 100u, nulled, out XboxAPI.meh);
            }
            if (bufferAddress == 0u)
            {
                byte[] array3 = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array3, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array3);
                bufferAddress = BitConverter.ToUInt32(array3, 0);
            }
            stringPointer = bufferAddress + 1500u;
            floatPointer = bufferAddress + 2700u;
            bytePointer = bufferAddress + 3200u;
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < arg.Length; i++)
            {
                object obj = arg[i];
                if (obj is byte)
                {
                    byte[] value = (byte[])obj;
                    array[num2] = (long)((ulong)BitConverter.ToUInt32(value, 0));
                }
                else if (obj is byte[])
                {
                    byte[] array4 = (byte[])obj;
                    xbCon.DebugTarget.SetMemory(bytePointer, (uint)array4.Length, array4, out XboxAPI.meh);
                    array[num2] = (long)((ulong)bytePointer);
                    bytePointer += (uint)(array4.Length + 2);
                }
                else if (obj is float)
                {
                    float value2 = float.Parse(Convert.ToString(obj));
                    byte[] bytes = BitConverter.GetBytes(value2);
                    xbCon.DebugTarget.SetMemory(floatPointer, (uint)bytes.Length, bytes, out XboxAPI.meh);
                    array[num2] = (long)((ulong)floatPointer);
                    floatPointer += (uint)(bytes.Length + 2);
                }
                else if (obj is float[])
                {
                    byte[] array5 = new byte[12];
                    for (int j = 0; j <= 2; j++)
                    {
                        byte[] array6 = new byte[4];
                        Buffer.BlockCopy((Array)obj, j * 4, array6, 0, 4);
                        Array.Reverse(array6);
                        Buffer.BlockCopy(array6, 0, array5, 4 * j, 4);
                    }
                    xbCon.DebugTarget.SetMemory(floatPointer, (uint)array5.Length, array5, out XboxAPI.meh);
                    array[num2] = (long)((ulong)floatPointer);
                    floatPointer += 2u;
                }
                else if (obj is string)
                {
                    byte[] bytes2 = Encoding.ASCII.GetBytes(Convert.ToString(obj));
                    xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes2.Length, bytes2, out XboxAPI.meh);
                    array[num2] = (long)((ulong)stringPointer);
                    string text = Convert.ToString(obj);
                    stringPointer += (uint)(text.Length + 1);
                }
                else
                {
                    array[num2] = Convert.ToInt64(obj);
                }
                num++;
                num2++;
            }
            byte[] data = XboxAPI.getData(array);
            xbCon.DebugTarget.SetMemory(bufferAddress + 8u, (uint)data.Length, data, out XboxAPI.meh);
            byte[] bytes3 = BitConverter.GetBytes(num);
            Array.Reverse(bytes3);
            xbCon.DebugTarget.SetMemory(bufferAddress + 4u, 4u, bytes3, out XboxAPI.meh);
            Thread.Sleep(0);
            byte[] bytes4 = BitConverter.GetBytes(address);
            Array.Reverse(bytes4);
            xbCon.DebugTarget.SetMemory(bufferAddress, 4u, bytes4, out XboxAPI.meh);
            Thread.Sleep(50);
            byte[] array7 = new byte[4];
            xbCon.DebugTarget.GetMemory(bufferAddress + 4092u, 4u, array7, out XboxAPI.meh);
            xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092u, 4u);
            Array.Reverse(array7);
            return BitConverter.ToUInt32(array7, 0);
        }
        public uint CallSysFunction(uint address, params object[] arg)
        {
            long[] array = new long[9];
            if (!activeConnection)
            {
                Connection();
            }
            if (XboxAPI.firstRan == 0)
            {
                byte[] array2 = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array2, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array2);
                bufferAddress = BitConverter.ToUInt32(array2, 0);
                XboxAPI.firstRan = 1;
                stringPointer = bufferAddress + 1500u;
                floatPointer = bufferAddress + 2700u;
                bytePointer = bufferAddress + 3200u;
                xbCon.DebugTarget.SetMemory(bufferAddress, 100u, nulled, out XboxAPI.meh);
                xbCon.DebugTarget.SetMemory(stringPointer, 100u, nulled, out XboxAPI.meh);
            }
            if (bufferAddress == 0u)
            {
                byte[] array3 = new byte[4];
                xbCon.DebugTarget.GetMemory(2445314222u, 4u, array3, out XboxAPI.meh);
                xbCon.DebugTarget.InvalidateMemoryCache(true, 2445314222u, 4u);
                Array.Reverse(array3);
                bufferAddress = BitConverter.ToUInt32(array3, 0);
            }
            stringPointer = bufferAddress + 1500u;
            floatPointer = bufferAddress + 2700u;
            bytePointer = bufferAddress + 3200u;
            int num = 0;
            int num2 = 0;
            array[num2] = (long)((ulong)address);
            num2++;
            for (int i = 0; i < arg.Length; i++)
            {
                object obj = arg[i];
                if (obj is byte)
                {
                    byte[] value = (byte[])obj;
                    array[num2] = (long)((ulong)BitConverter.ToUInt32(value, 0));
                }
                else if (obj is byte[])
                {
                    byte[] array4 = (byte[])obj;
                    xbCon.DebugTarget.SetMemory(bytePointer, (uint)array4.Length, array4, out XboxAPI.meh);
                    array[num2] = (long)((ulong)bytePointer);
                    bytePointer += (uint)(array4.Length + 2);
                }
                else if (obj is float)
                {
                    float value2 = float.Parse(Convert.ToString(obj));
                    byte[] bytes = BitConverter.GetBytes(value2);
                    xbCon.DebugTarget.SetMemory(floatPointer, (uint)bytes.Length, bytes, out XboxAPI.meh);
                    array[num2] = (long)((ulong)floatPointer);
                    floatPointer += (uint)(bytes.Length + 2);
                }
                else if (obj is float[])
                {
                    byte[] array5 = new byte[12];
                    for (int j = 0; j <= 2; j++)
                    {
                        byte[] array6 = new byte[4];
                        Buffer.BlockCopy((Array)obj, j * 4, array6, 0, 4);
                        Array.Reverse(array6);
                        Buffer.BlockCopy(array6, 0, array5, 4 * j, 4);
                    }
                    xbCon.DebugTarget.SetMemory(floatPointer, (uint)array5.Length, array5, out XboxAPI.meh);
                    array[num2] = (long)((ulong)floatPointer);
                    floatPointer += 2u;
                }
                else if (obj is string)
                {
                    byte[] bytes2 = Encoding.ASCII.GetBytes(Convert.ToString(obj));
                    xbCon.DebugTarget.SetMemory(stringPointer, (uint)bytes2.Length, bytes2, out XboxAPI.meh);
                    array[num2] = (long)((ulong)stringPointer);
                    string text = Convert.ToString(obj);
                    stringPointer += (uint)(text.Length + 1);
                }
                else
                {
                    array[num2] = Convert.ToInt64(obj);
                }
                num++;
                num2++;
            }
            byte[] data = XboxAPI.getData(array);
            xbCon.DebugTarget.SetMemory(bufferAddress + 8u, (uint)data.Length, data, out XboxAPI.meh);
            byte[] bytes3 = BitConverter.GetBytes(num);
            Array.Reverse(bytes3);
            xbCon.DebugTarget.SetMemory(bufferAddress + 4u, 4u, bytes3, out XboxAPI.meh);
            Thread.Sleep(0);
            byte[] bytes4 = BitConverter.GetBytes(2181038080u);
            Array.Reverse(bytes4);
            xbCon.DebugTarget.SetMemory(bufferAddress, 4u, bytes4, out XboxAPI.meh);
            Thread.Sleep(50);
            byte[] array7 = new byte[4];
            xbCon.DebugTarget.GetMemory(bufferAddress + 4092u, 4u, array7, out XboxAPI.meh);
            xbCon.DebugTarget.InvalidateMemoryCache(true, bufferAddress + 4092u, 4u);
            Array.Reverse(array7);
            return BitConverter.ToUInt32(array7, 0);
        }

    }
}
