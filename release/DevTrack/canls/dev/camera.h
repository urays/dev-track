#ifndef _MAKE_CANLS_DLL_CAMERA_
#define _MAKE_CANLS_DLL_CAMERA_

#ifdef __cplusplus
extern "C"
{
#endif
	extern int _w(int x, int y);
	extern void _camerafunc(unsigned char* src);
#ifdef __cplusplus
}
#endif

//!
// 代替摄像头功能  提供原始赛道图像
// 120*60  120*48 ... 由采集的SWCODE确定
// 每调用一次_camerafunc,获取一帧图像
//!

#define FRAME_WIDTH          120
#define FRAME_HEIGHT         60   //48
#define WRITE_PIXEL          0x01 //白色像素点对应的数据值

//唯一可调用函数 判断某一个点是否为白色
//参数(x,y)解释：x为图像像素点水平横坐标, y为图像像素点垂直坐标。对于120*60的图像来说 x的范围为0~119  y的范围为0~59
//图像最左上角像素点坐标(0,0)  最右下角坐标(119,59) 依此类推
#define POINT_WHITE(x, y)    _w(x, y)

#endif