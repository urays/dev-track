## 自定义图像处理算法生成 canls.dll 的操作说明


### 前言

> 上位机采用C#编写,而自定义的图像处理算法采用C语言编写，要在上位机上运行我们的图像处理函数，就需要将自定义算法包编译为DLL，并向上位机提供必要的函数接口。为了规范化这些接口，我们编写了相应的**配置文件**(camera.h、camera.c、interface.h、interface.c)。只需将这4个文件包含到自定义的图像分析算法源代码中，进行适当的配置后，编译生成canls.dll，就可以在上位机上运行自定义的图像分析算法了。

> **配置文件**存放在**dev**文件夹下。


### 数据包格式

```
//上位机与自定义图像分析包之间数据传送的格式定义

struct _POINT { int x, y; };
struct _POINT_SET { char name[20]; struct _POINT* pPoints; int* pCount; };
struct _FLOAT { char name[20]; float* pData; };
struct _INTEGER { char name[20]; int* pData; };

#define MAXSIZE (10)
struct _DATAPACK {
	int OFC, OIC, SFC, SIC, OPC;
	struct _FLOAT OF[MAXSIZE];     //float data is to be observed
	struct _INTEGER OI[MAXSIZE];   //integer data is to be observed
	struct _FLOAT SF[MAXSIZE];     //float data that can be set
	struct _INTEGER SI[MAXSIZE];   //integer data that can be set
	struct _POINT_SET OP[MAXSIZE]; //points data is to be observed
};
```

我们将数据分为两大类：一类是观察型数据，另一类是设置型数据。顾名思义，很好理解。如上述代码所示，以"O"字母开头的即为观察型数据(observe);以"S"字母开头的即为设置型数据(set)。另外，我们将数据又分为了3类，分别是整型数据(I)、浮点型数据(F)以及点集数据(P)，如代码中所示，OF即为观察型浮点数据，SI即为设置型整型数据，等等。需要注意的是，我们这里所定义的浮点型是结构体的形式，存储的是数据的地址指针。

```
//数据包的生成 - 我们为用户提供了相应的函数（如下3个函数），用于将数据压入到数据包中。
1. add_float_data(...)
2. add_int_data(...)
3. add_points_data(...)

//示例:
add_float_data(pkg, "O", "m_slope", pAnls.m_slope);
add_int_data(pkg, "O", "i_lastw_y", pAnls.i_lastw_y);
add_int_data(pkg, "S", "m_horz_ctl", pAnls.set.m_horz_ctl);
add_points_data(pkg, "O", "edgel", &(pAnls.edgel->line[0]), &(pAnls.edgel->endpos));
```
注意:压入数据包的均为数据的地址指针。这样做的目的在于：可以在上位机中对设置型数据进行修改。

### 与上位机的接口函数

```
//上位机与自定义图像分析包之间接口函数

//返回图像分析算法包的版本号
char* canls_dll_ver(int*, int*); 

//图像分析包初始化函数
void canls_dll_init(struct _DATAPACK*);

 //图像分析包刷新函数
void canls_dll_cls();

//图像分析执行函数
void canls_dll_run(unsigned char*); 
```

对于接口函数内容的配置，我们在**interface.c**中已经提供了相应的示例作为参考，只需根据修改提示，共需要修改**6**处地方。**部分修改示例如下**:

```
void canls_dll_run(unsigned char *src)
{
	_camerafunc(src); //该函数是必要的,不需要改。详见 camera.h

	//这里添加你的图像分析算法的分析函数 ---------------------------第5处
    //
}

void canls_dll_cls()
{
	//用于刷新状态机或者重新设置变量值 -----------------------------第6处
    //
}

```

### 配置文件为自定义图像分析包提供的函数

为了能够在不依靠物理摄像头的情况下，为图像分析算法提供可用的图像，我们在配置文件中编写了相应的函数用于实现摄像头每次获取一帧图像的功能。对应文件：camera.h和camera.c

```
//camera.h

#define FRAME_WIDTH          120
#define FRAME_HEIGHT         60   //48
#define WRITE_PIXEL          0x01 //白色像素点对应的数据值

#define POINT_WHITE(x, y)    _w(x, y)

extern void _camerafunc(unsigned char *src);
```

对于图像处理算法,我们只需要提供位置坐标并能够判断一个像素点是什么颜色的即可，故我们为用户提供了 POINT_WHITE(x, y)，用户只需通过调用它即可访问一整幅图像。

此外，我们只需在自定义的图像分析算法执行函数之前，调用_camerafunc()函数即可每次获取一帧的图像。_camerafunc()的调用参考示例如下(在配置文件中已经写入了调用)：

```
//interface.c

void canls_dll_run(unsigned char *src)
{
	//获取一帧图像(每个字节表示一个像素点)
	//访问当前帧图像数组时只需要使用 "POINT_WHITE(x, y)" 这个宏定义即可
	_camerafunc(src);

	//这里添加你的图像分析算法的分析函数 ---------------------------第5处
    //
}
```

*备注:至此配置文件(camera.h,camera.c,interface.h,interface.c)的操作步骤已经阐述完了。*

### 其它的一些东西

> 关于自定义图像分析算法的代码组织以及需要提供的函数接口，Emmm...这里就不作说明，具体请见**doc**文件夹，因为某些原因，图像分析算法的源代码不作公布。
