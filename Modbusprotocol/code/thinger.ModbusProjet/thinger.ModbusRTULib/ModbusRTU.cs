using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace thinger.ModbusRTULib
{
    public class ModbusRTU
    {
        #region 对象和属性

        //串口通信对象
        private SerialPort serialPort;

        /// <summary>
        /// 读取超时时间
        /// </summary>
        public int ReadTimeOut { get; set; }

        /// <summary>
        /// 每次串口通信前的延时时间
        /// </summary>
        public int SleepTime { get; set; } = 10;

        /// <summary>
        /// 写入超时时间
        /// </summary>
        public int WriteTimeOut { get; set; }

        /// <summary>
        /// 最大读取时间
        /// </summary>
        public int ReceiveTimeOut { get; set; } = 5000;

        private bool dtrEnable = false;

        /// <summary>
        /// Dtr使能标志
        /// </summary>
        public bool DtrEnable
        {
            get { return dtrEnable = false; }
            set
            {
                dtrEnable = value;
                this.serialPort.DtrEnable = dtrEnable;
            }
        }

        private bool rtsEnable = false;

        /// <summary>
        /// Rts使能标志
        /// </summary>
        public bool RtsEnable
        {
            get { return rtsEnable; }
            set
            {
                rtsEnable = value;
                this.serialPort.RtsEnable = rtsEnable;
            }
        }

        #endregion

        #region Construct

        public ModbusRTU()
        {
            serialPort = new SerialPort();
        }

        #endregion

        #region 建立连接和断开连接

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns>是否成功</returns>
        public bool Connect(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            serialPort.BaudRate = baudRate;
            serialPort.Parity = parity;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Parity = parity;
            serialPort.ReadTimeout = this.ReadTimeOut;
            serialPort.WriteTimeout = this.WriteTimeOut;
            try
            {
                serialPort.Open();

            }
            catch (Exception)
            {

                return false;
            }
            return true;


        }
        
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect() 
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        #endregion

        #region 01H读取输出线圈

        /// <summary>
        /// 读取输出线圈
        /// </summary>
        /// <param name="slaveId">站地址</param>
        /// <param name="start">起始线圈地址</param>
        /// <param name="length">线圈数量</param>
        /// <returns>返回的数据</returns>
        public byte[] ReadOutputCoils(byte slaveId,ushort start,ushort length) 
        {
            //第一步：拼接报文
            List<byte> SendCommand = new List<byte>();

            //从站地址 + 功能码 +开始线圈地址 + 线圈数量 + CRC
            SendCommand.Add(slaveId);

            //功能码
            SendCommand.Add(0x01);

            //起始线圈地址
            SendCommand.Add((byte)(start / 256)); //高位
            SendCommand.Add((byte)(start % 256)); //低位

            //线圈数量
            SendCommand.Add((byte)(length / 256)); //高位
            SendCommand.Add((byte)(length % 256)); //低位

            //CRC
            byte[] crc = CRC16(SendCommand.ToArray(),SendCommand.Count);
            SendCommand.AddRange(crc);


            //第二步：发送报文，接收报文 
            //byteLength 表示线圈所占字节数
            int byteLength = length % 8 == 0 ? length / 8 : length /8 +1;
            byte[] receive = null;
            if (SendAndReceive(SendCommand.ToArray(),ref receive))
            {
                //第四步：验证报文
                if (CheckCRC(receive) && receive.Length == 5 + byteLength)
                {
                    if (receive[0] == slaveId && receive[1] == 0x01)
                    {
                        //第五步：解析报文
                        byte[] result  = new byte[byteLength];
                        Array.Copy(receive,3,result,0,byteLength);
                        return result;
                    }
                }

                
            }
            return null;

        }

        #endregion

        #region 02H读取输入线圈

        /// <summary>
        /// 读取输出线圈
        /// </summary>
        /// <param name="slaveId">站地址</param>
        /// <param name="start">起始线圈地址</param>
        /// <param name="length">长度</param>
        /// <returns>返回的数据</returns>
        public byte[] ReadInputCoils(byte slaveId, ushort start, ushort length)
        {
            //第一步：拼接报文
            List<byte> SendCommand = new List<byte>();

            //从站地址 + 功能码 +开始线圈地址 + 线圈数量 + CRC
            SendCommand.Add(slaveId);

            //功能码
            SendCommand.Add(0x02);

            //起始线圈地址
            SendCommand.Add((byte)(start / 256)); //高位
            SendCommand.Add((byte)(start % 256)); //低位

            //线圈数量
            SendCommand.Add((byte)(length / 256)); //高位
            SendCommand.Add((byte)(length % 256)); //低位

            //CRC
            byte[] crc = CRC16(SendCommand.ToArray(), SendCommand.Count);
            SendCommand.AddRange(crc);


            //第二步：发送报文，接收报文 

            int byteLength = length % 8 == 0 ? length / 8 : length / 8 + 1;
            byte[] receive = null;
            if (SendAndReceive(SendCommand.ToArray(), ref receive))
            {
                //第四步：验证报文
                if (CheckCRC(receive) && receive.Length == 5 + byteLength)
                {
                    if (receive[0] == slaveId && receive[1] == 0x02)
                    {
                        //第五步：解析报文
                        byte[] result = new byte[byteLength];
                        Array.Copy(receive, 3, result, 0, byteLength);
                        return result;
                    }
                }


            }
            return null;

        }

        #endregion

        #region 03H读取输出寄存器
            
        public byte[] ReadOutputRegisters(byte slaveId,ushort start,ushort length)
        {
            //第一步：拼接报文
            List<byte> sendCommand = new List<byte>();
            sendCommand.Add(slaveId);
            sendCommand.Add(0x03);
            sendCommand.Add((byte)(start / 256));
            sendCommand.Add((byte)(start % 256));

            //第二部：发送报文

            //第三步：接受报文

            return null;
        }

        #endregion

        /// <summary>
        /// 发送并接收报文
        /// </summary>
        /// <param name="send">发送字节数组</param>
        /// <param name="receive">接受的字节数组</param>
        /// <returns>是否成功</returns>
        private bool SendAndReceive(byte[] send,ref byte[] receive)
        {
            try
            {
                //发送报文
                this.serialPort.Write(send, 0, send.Length);

                //定义一个Buffer缓冲值
                byte[] buffer = new byte[1024];

                //定义一个内存
                MemoryStream stream = new MemoryStream();

                //定义一个开始时间
                DateTime start = DateTime.Now;

                //这样处理是为了防止一次性都不完整

                //循环读取缓冲区的数据，如果大于0，就读出来，放到内存里，如果等于0，说明读完了
                //如果每次都读不到要设置一个超时时间
                while (true)
                {
                    Thread.Sleep(SleepTime);
                    if (this.serialPort.BytesToRead > 0)
                    {
                        int count = this.serialPort.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, count);
                    }
                    else
                    {
                        if (stream.Length > 0)
                        {
                            break;
                        }
                        else if ((DateTime.Now - start).TotalMilliseconds > this.ReceiveTimeOut)
                        {
                            return false;
                        }
                    }

                }
                receive = stream.ToArray();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
           

        }

        #region CRC校验【表查法，速度很快】

        private static readonly byte[] aucCRCHi = new byte[]  {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40
        };
        private static readonly byte[] aucCRCLo = new byte[] {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
            0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
            0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
            0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
            0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
            0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
            0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
            0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
            0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
            0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
            0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
            0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
            0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
            0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
            0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
            0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
            0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
            0x41, 0x81, 0x80, 0x40
        };


        private byte[] CRC16(byte[] pucFrame, int usLen)
        {
            int i = 0;
            byte[] res = new byte[2] { 0xFF, 0xFF };
            ushort iIndex;
            while (usLen-- > 0)
            {
                iIndex = (ushort)(res[0] ^ pucFrame[i++]);
                res[0] = (byte)(res[1] ^ aucCRCHi[iIndex]);
                res[0] = aucCRCHi[iIndex];
            }
            return res;
        }
        public byte[] CRC16Calc(byte[] data)
        {
            //crc计算赋初始值
            int crc = 0xffff;
            for (int i = 0; i < data.Length; i++)
            {
                crc = crc ^ data[i];
                for (int j = 0; j < 8; j++)
                {
                    int temp;
                    temp = crc & 1;
                    crc = crc >> 1;
                    crc = crc & 0x7fff;
                    if (temp == 1)
                    {
                        crc = crc ^ 0xa001;
                    }
                    crc = crc & 0xffff;
                }
            }
            //CRC寄存器的高低位进行互换
            byte[] crc16 = new byte[2];
            //CRC寄存器的高8位变成低8位，
            crc16[1] = (byte)((crc >> 8) & 0xff);
            //CRC寄存器的低8位变成高8位
            crc16[0] = (byte)(crc & 0xff);
            return crc16;
        }
        /// <summary>
        ///  CRC校验，截取data中的一段进行CRC16校验
        /// </summary>
        /// <param name="data">校验数据，字节数组</param>
        /// <param name="offset">从头开始偏移几个byte</param>
        /// <param name="length">偏移后取几个字节byte</param>
        /// <returns>字节0是高8位，字节1是低8位</returns>
        public byte[] CRC16Calc(byte[] data, int offset, int length)
        {
            byte[] Tdata = data.Skip(offset).Take(length).ToArray();
            return CRC16Calc(Tdata);
        }
        private bool CheckCRC(byte[] value)
        {
            if (value == null) return false;
            if (value.Length <= 2)
            {
                return false;
            }
            int length = value.Length;
            byte[] buf = new byte[length - 2];
            Array.Copy(value, 0, buf, 0, buf.Length);
            byte[] CRCbuf = CRC16Calc(buf, 0, buf.Length);
            if (CRCbuf[0] == value[length - 2] && CRCbuf[1] == value[length - 1])
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
