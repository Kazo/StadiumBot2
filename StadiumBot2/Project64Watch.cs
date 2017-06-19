using System;
using System.Net.Sockets;
using System.Threading;

namespace StadiumBot2
{
    public class Project64Watch
    {
        static TcpClient tcpClient = new TcpClient();
        public static String host = "127.0.0.1";
        public static Int32 port = 6520;
        static UInt32 pMagic = 0x34364A50;//"PJ64"
        static UInt32 pCommand;
        static UInt32 pSize;
        static UInt32 pAddress;
        static Byte[] pData;
        static UInt32 sCommand;
        static UInt32 sSize;
        static UInt32 sAddress;

        static void SendCommand(UInt32 command)
        {
            sCommand = (0x04 << 0x18) + command;
            sSize = 0x00;
            Byte[] sBuffer = new Byte[sSize + 0x0C];
            Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
            Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
            Array.Copy(BitConverter.GetBytes(sSize), 0x00, sBuffer, 0x08, 0x04);
            SendPacket(sBuffer);
        }

        public static void Connect()
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(host, port);
                Thread.Sleep(100);
            }
            catch (Exception)
            {
                tcpClient.Close();
            }
        }

        static void SendPacket(Byte[] sBuffer)
        {
            try
            {
                tcpClient.GetStream().Write(sBuffer, 0, sBuffer.Length);
            }
            catch (Exception)
            {

            }
        }

        static void ReadPacket(Byte[] Reply)
        {
            try
            {
                tcpClient.GetStream().Read(Reply, 0, Reply.Length);
            }
            catch (Exception)
            {

            }
        }

        static int Available()
        {
            try
            {
                return tcpClient.Available;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        static void SendWaitData(Byte[] sBuffer)
        {
            UInt32 TimeoutCounter = 0;
            UInt32 ResendCounter = 0;
            SendPacket(sBuffer);

            while (Available() == 0)
            {
                Thread.Sleep(1);
                TimeoutCounter++;
                if (TimeoutCounter > 500)
                {
                    if (ResendCounter < 4)
                    {
                        SendPacket(sBuffer);
                        ResendCounter++;
                    }
                    else
                    {
                        tcpClient.Close();
                        Connect();
                        ResendCounter = 0;
                        SendPacket(sBuffer);

                    }
                    TimeoutCounter = 0;
                }
            }
        }

        public static Byte Read8(UInt32 Offset)
        {
            Byte result = 0;
            sSize = 0x01;
            sAddress = Offset;
            sCommand = (0x01 << 0x18) + sAddress;
            Byte[] sBuffer = new Byte[sSize + 0x0C];
            Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
            Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
            Array.Copy(BitConverter.GetBytes(sSize), 0x00, sBuffer, 0x08, 0x04);

            SendWaitData(sBuffer);

            Byte[] Reply = new Byte[Available()];
            ReadPacket(Reply);
            pData = GetData(Reply);
            if (pData.Length != 0)
            {
                result = pData[0];
            }

            return result;
        }

        public static UInt16 Read16(UInt32 Offset)
        {
            UInt16 result = 0;

            sSize = 0x02;
            sAddress = Offset;
            sCommand = (0x01 << 0x18) + sAddress;
            Byte[] sBuffer = new Byte[sSize + 0x0C];
            Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
            Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
            Array.Copy(BitConverter.GetBytes(sSize), 0x00, sBuffer, 0x08, 0x04);

            SendWaitData(sBuffer);

            Byte[] Reply = new Byte[Available()];
            ReadPacket(Reply);
            pData = GetData(Reply);
            if (pData.Length != 0)
            {
                result = (ushort)(pData[1] + (pData[0] << 0x08));
            }

            return result;
        }

        public static Byte[] Read(UInt32 Offset, UInt32 Length)
        {
            Byte[] result = new Byte[0];

            sSize = Length;
            sAddress = Offset;
            sCommand = (0x01 << 0x18) + sAddress;
            Byte[] sBuffer = new Byte[sSize + 0x0C];
            Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
            Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
            Array.Copy(BitConverter.GetBytes(sSize), 0x00, sBuffer, 0x08, 0x04);

            SendWaitData(sBuffer);

            Byte[] Reply = new Byte[Available()];
            ReadPacket(Reply);
            pData = GetData(Reply);
            if (pData.Length != 0)
            {
                result = pData;
            }

            return result;
        }

        public static void Write(UInt32 Offset, Byte[] Data)
        {
            sCommand = (0x02 << 0x18) + Offset;
            Byte[] sBuffer = new Byte[Data.Length + 0x0C];
            Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
            Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
            Array.Copy(BitConverter.GetBytes(Data.Length), 0x00, sBuffer, 0x08, 0x04);
            Array.Copy(Data, 0x00, sBuffer, 0x0C, Data.Length);
            SendPacket(sBuffer);
        }

        public static void Write8(UInt32 Offset, Byte Value)
        {
            Byte[] Data = new byte[1];
            Data[0] = Value;
            sCommand = (0x02 << 0x18) + Offset;
            Byte[] sBuffer = new Byte[Data.Length + 0x0C];
            Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
            Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
            Array.Copy(BitConverter.GetBytes(Data.Length), 0x00, sBuffer, 0x08, 0x04);
            Array.Copy(Data, 0x00, sBuffer, 0x0C, Data.Length);
            SendPacket(sBuffer);
        }

        static Byte[] GetData(Byte[] Reply)
        {
            Byte[] result = new Byte[0];

            if (BitConverter.ToUInt32(Reply, 0x00) == 0x34364A50)
            {
                pCommand = BitConverter.ToUInt32(Reply, 0x04);
                pAddress = pCommand & 0xFFFFFF;
                pCommand = ((pCommand & 0xFF000000) >> 0x18);
                pSize = BitConverter.ToUInt32(Reply, 0x08);

                if ((pSize > 0) && (Reply.Length >= pSize + 0x0C))
                {
                    result = new Byte[pSize];
                    Array.Copy(Reply, 0x0C, result, 0x00, pSize);
                }
            }
            return result;
        }

        public static void SendInput(UInt32 Player, UInt32 Buttons)
        {
            if (Buttons != 0x00)
            {
                sCommand = (0x03 << 0x18) + (Player - 1);
                Byte[] sBuffer = new Byte[0x0C];
                Array.Copy(BitConverter.GetBytes(pMagic), 0x00, sBuffer, 0x00, 0x04);
                Array.Copy(BitConverter.GetBytes(sCommand), 0x00, sBuffer, 0x04, 0x04);
                Array.Copy(BitConverter.GetBytes(Buttons), 0x00, sBuffer, 0x08, 0x04);
                SendPacket(sBuffer);
            }
        }

        public static void SetInput(ref UInt32 Buttons, String Key)
        {
            switch (Key)
            {
                case "DPad R":
                    {
                        Buttons |= 0x01;
                    }
                    break;

                case "DPad L":
                    {
                        Buttons |= (0x01 << 0x01);
                    }
                    break;

                case "DPad D":
                    {
                        Buttons |= (0x01 << 0x02);
                    }
                    break;

                case "DPad U":
                    {
                        Buttons |= (0x01 << 0x03);
                    }
                    break;

                case "Start":
                    {
                        Buttons |= (0x01 << 0x04);
                    }
                    break;

                case "Z":
                    {
                        Buttons |= (0x01 << 0x05);
                    }
                    break;

                case "B":
                    {
                        Buttons |= (0x01 << 0x06);
                    }
                    break;

                case "A":
                    {
                        Buttons |= (0x01 << 0x07);
                    }
                    break;

                case "C Right":
                    {
                        Buttons |= (0x01 << 0x08);
                    }
                    break;

                case "C Left":
                    {
                        Buttons |= (0x01 << 0x09);
                    }
                    break;

                case "C Down":
                    {
                        Buttons |= (0x01 << 0x0A);
                    }
                    break;

                case "C Up":
                    {
                        Buttons |= (0x01 << 0x0B);
                    }
                    break;

                case "R":
                    {
                        Buttons |= (0x01 << 0x0C);
                    }
                    break;

                case "L":
                    {
                        Buttons |= (0x01 << 0x0D);
                    }
                    break;
            }
        }
    }
}
