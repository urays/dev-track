# DevTrack

![devtrack](https://i.loli.net/2020/11/19/dxtEWoUV3guy7ez.png)


**> 由于Windows(CRLF)和Linux(LF)在编码上的区别,需要先在Git上执行以下配置命令(统一采用CRLF)**

```
git config --global core.autocrlf true
```

**> 采用编码格式：SWCODE，同时也支持BIN。swocde源码地址:https://github.com/urays/swcode**

**> 'test_videos'目录下的文件可以用作测试**

**> 软件自动更新源:https://gitee.com/OTRACK/devtrack-release**

### Interface Design (Beta)

![ID1](https://i.loli.net/2021/07/17/lsFyd4qVeKkIYOt.jpg)

### TODO

> 撰写说明文档

> 增加自定义图像分析包在上位机中的参数修改功能

> 完善图像标定、分析工具

> 完善数据处理工具

> 改善图像与数据的交互性能

> UTF8,CRLF代码文本编辑功能

> C语言语法分析，编译功能的支持

> 分析对象的附加处理，如摄像头空间位置变化后的图像帧实时修正、逆透视、模拟帧生成

> 扩充下位机搜集信息种类(可以增加加速度计，陀螺仪等，采集车辆运动信息)

> 无线数据传输功能的支持(可忽略)

> 植入神经网络，实现在上位机上图像处理参数自我优化


### 关于CCS

**CCS**是**Custom Controls**的简称，它是一个面向DevTrack，为了方便扩展功能以及优化代码运行效率而创建的自定义控件库。目前仍处于初步开发阶段，尚未应用于DevTrack的发行版。<br><br>**CCS/docs**文件夹下是urays对于DevTrack功能设计提出的一些实现思路，由于时间和精力有限，很多想法尚未进行代码实现 :)

### CCS (Ext)

![CCS_1](https://i.loli.net/2021/07/17/KoQ9MgEDICUrn57.jpg)

