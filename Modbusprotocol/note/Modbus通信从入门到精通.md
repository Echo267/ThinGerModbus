# Modbus通信从入门到精通

## 1.Modbus协议基础知识

###  1.1Modbus调试软件安装和使用

- Modbus Poll

  Modbus主站或Modbus客户端
  
- Modbus Slave

  Modbus从站或Modbus服务器
  
  
  
- VSPD（Virtual Serial Port Driver）

  虚拟一对串口
  
### 1.2 Modbus协议存储区说明

- 为什么要有存储区？

	- 站在协议制定者的角度思考

	- 协议的目的是为了数据传输

	- 数据传输无非就是读取和写入

	- 存储区类型分为布尔和数据

- Modbus存储区有哪些?

	- 只读

		- 线圈

			- 输入线圈

	- 读写

		- 线圈

			- 输出线圈

	- 只读

		- 寄存器

			- 输入寄存器

	- 读写

		- 寄存器

			- 输出寄存器

- Modbus存储区代号

	-  输入线圈

		- 1区

	- 输出线圈

		- 0区

	- 输入寄存器

		- 3区

	- 输出寄存器

		- 4区

- Modbus存储区范围

	- Modbus规定，每个存储区的最大范围是65536

	- PLC地址

		- 西门子：MW100 DB1.DBD0

		- 三菱：DO XO YO

		- 欧姆龙：D0 W0

		- 绝对地址=区号 + 相对地址

	- Modbus地址：绝对地址 = 区号 +（相对地址+1）

	- 输出寄存器的第一个绝对地址就是40001

	- 地址模型

		- 长地址模型

			-  

		- 短地址模型

			-  

		- 如果我们使用不了那么多的数据，可以使用短地址模型，否则使用长地址模型

	- 相对地址和绝对地址

		- 我们一般在人为交流或说明文档，使用绝对地址

		- 一般在协议报文，都会使用相对地址

		- 因为在协议报文中，我们可以通过功能码知道是哪个存储区

### 1.3Modbus协议功能码说明

- 为什么会用功能码？

	- 协议的目的是为了数据的传输

	- 已经确定好存储区，存储区会存不同的类型

	- 那么必然会产生很多种不同的行为

	- 我们给每种行为指定一个代号，那么这个代号就是功能码

	- 功能码其实就是行为的代号

- Modbus通信会有哪些行为？

	- 读取输入线圈

	- 读取输出线圈

	- 读取输入寄存器

	- 读取输出寄存器

	- 写入输出线圈

	- 写入输出寄存器

- Modbus常用功能码有哪些？

	- 0x01

		- 读取输出线圈

	- 0x02

		- 读取输入线圈

	- 0x03

		- 读取输入寄存器

	- 0x04

		- 读取输入寄存器

	- 0x05

		- 写入单个线圈

	- 0x06

		- 写入单个寄存器

	- 0x0F

		- 写入多个线圈

	- 0x10

		- 写入多个寄存器

- Modbus还会有一些其他的功能码，比如异常，自定义

### 1.4modbus协议分类及测试

- 报文帧

	- ModbusRTU

	- ModbusASCII

	- ModbusRCP

- 通信介质

	- 串口通信

		- 232/485/422

	- 以太网通信

		- TCP/IP

		- UDP/IP

- 协议分类

	- ModbuRTU协议

	- ModbusASCII协议

	- ModbusRTUOverTCP协议

	- ModbusRTUOverUDP协议

	- ModbusASCIIOverTCP协议

	- ModbusASCIIOverUDP协议 

	- ModbusTCP协议

	- ModbusUDP协议

## 2.ModbusRTU通信报文分析

### 2.1 ModbusRTU通信格式说明

- 通用报文帧格式

	- 从站地址（1字节）

		- 要和哪个设备通信

	- 功能码（1字节）

		- 要做什么

	- 数据部分（N字节）

		- 读取发送

			- 开始地址

			- 读取数量

		- 读取接收

			- 字节计数

			- 具体数据

		- 写入单发送

			- 具体地址

			- 写入数据

		- 写入单接收

			- 具体地址

			- 写入数据

		- 写入多发送

			- 开始地址

			- 写入数量

			- 写入数据

		- 写入多接收

			- 开始地址

			- 写入数量

	- 校验部分（2字节）

		- CRC16

- 校验部分

	- 这里校验和串口的奇/偶/无校验是没有关系的

	- 为什么要有校验？

		- 保证数据的准确

	- 校验的本质是什么？

		- 校验的本质就是一种算法

### 2.2 01H功能码读取输出线圈

- 发送报文格式：从站地址 + 功能码 +开始线圈地址 + 线圈数量 + CRC

- 接收报文格式：从站地址 + 功能码 + 字节计数 + 数据+CRC

- 读取线圈测试：1号站点从10开始的20个线圈的值

- 发送报文：01 01 00 0A 00 14 1C 07 

- 接收报文： 01 03 03 00 00 CC 4E  一个字节表示八个线圈的值 20个线圈用三个字节

### 2.3 02H功能码读取输出线圈

- 发送报文格式：从站地址 + 功能码 +开始线圈地址 + 线圈数量 + CRC

- 接收报文格式：从站地址 + 功能码 + 字节计数 + 数据+CRC

- 读取输入线圈：读取**5号站点**从20开始的10个线圈

   演示：
  
   1.打开Slave 

<img src="https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112215433629.png" alt="image-20240112215433629" style="zoom:50%;" />

![image-20240112215550410](https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112215550410.png)

![image-20240112215702253](https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112215702253.png)

​	2.打开Poll

​	![image-20240112215945725](https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112215945725.png)

![image-20240112220206810](https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112220206810.png)

![image-20240112220219640](https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112220219640.png)

设置读写参数进行数据读取

![image-20240112220111985](https://lyhblogimage.oss-cn-beijing.aliyuncs.com/blogimga/image-20240112220111985.png)

	发送报文：05 		02 			00 14   		00 0A 		B9 8D
			站地址  	功能码     开始线圈(20开始)		发送数量	校验码
	接收报文：05 		02 			02 				03 00 		48 88
			站地址	 	功能码		字节计数			数据			校验码
	这里字节计数用02是因为一个字节8位有10个线圈(一个线圈占1位)，所以最少2个
	03 00 ：这里表示 11 0000 0000 就是我们刚刚设置的从站数据 二进制3就是11





### 2.4 03H功能码读取输出寄存器



### 2.5 04H功能码读取输入寄存器

### 2.6 05H功能码预制单线圈

### 2.7 06H功能码预置单寄存器

### 2.8 0F功能码预置多线圈

+ 发送报文格式：从站地址 + 功能码 + 起始线圈地址 + 数量 +字节计数 + 写入的数据 + CRC

+ 接受的报文：从站地址 + 功能码 + 起始线圈数量 + 字节计数 + 数量 + CRC

+ 预置多线圈：将1号站点从1开始的5个线圈写入true true False True False

+ 发送报文：01 0F 00 01 00 05 01 16 D3 58

  16: 0001 0110 从高位开始对应 

  <img src="C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20240114150452667.png" alt="image-20240114150452667" style="zoom:50%;" />

+ 接受报文：01 0F 00 01 00 05  C4 08



### 2.9 10H功能码预置多寄存器

+ 发送报文格式：从站地址 + 功能码 + 起始寄存器地址 + 数量 + 字节计数 + 数据 + CRC

+ 接受报文格式：从站地址 + 功能码 + 起始寄存器地址 + 数量 + CRC

+ 预置多寄存器：将1号站点从10开始的5个寄存器分别写入01 02 03 04 05 这里并不是字节，而是整数，每个对应2个字节

+ 发送报文：01 10 00 0A 00 05 0A 00 01 00 02 00 03 00 04 00 05 E0 60

+ 接受报文：01 10 00 0A 00 05 20 08 

  ![image-20240114151651384](C:\Users\Administrator\AppData\Roaming\Typora\typora-user-images\image-20240114151651384.png)

## 3.ModbusRTU通信库开发

### 3.1 串口连接与断开

```C#
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
```



### 3.2读取输入输出线圈
### 3.3读取输入输出寄存器
### 3.4预置单线圈与寄存器
### 3.5 预置多线圈与寄存器
### 3.6测试平台UI界面设计
### 3.7 输入输出线圈读取测试
### 3.8 输入输出寄存器读取测试
### 3.9 预置单线圈多线圈测试
### 3.10 预置单寄存器多寄存器测试

