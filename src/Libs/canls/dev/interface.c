//interface.c -v0.1 author:urays data:2020-01-30 email:urays@foxmail.com
#include <string.h>
#include "camera.h"
#include "interface.h"

static void add_float_data(struct _DATAPACK*, char*, char*, float*);
static void add_int_data(struct _DATAPACK*, char*, char*, int*);
static void add_points_data(struct _DATAPACK*, char*, char*, struct _POINT*, int*);

//!
// 说明:此文件用于链接图像分析函数包,以便与DevTrack交互
// 需要修改的地方已经用注释的方式标出,总共6处地方
//!

//图像分析算法版本号 ----------------------------------------------第1处
#define PACKAGE_VERSION "smc-cam-rc-120-60"

//添加你的图像分析算法的头文件 要求你写的算法具有良好的封装性 --------第2处
#include "../smc-cam-rc/c-anls/extro-anls.h"

void canls_dll_init(struct _DATAPACK* pkg)
{
	//这里添加你的图像分析算法的初始化函数 (必须要有) ---------------第3处
	anls_init();

	//匹配你的参数地址 --------------------------------------------第4处
	add_float_data(pkg, "O", "m_sumerr", pAnls.m_sumerr);
	add_float_data(pkg, "O", "m_horzerr", pAnls.m_horzerr);
	add_float_data(pkg, "O", "m_curve", pAnls.m_curve);
	add_float_data(pkg, "O", "m_slope", pAnls.m_slope);
	add_float_data(pkg, "O", "m_horz_util", pAnls.m_horz_util);
	add_float_data(pkg, "O", "m_linear", pAnls.m_linear);

	add_int_data(pkg, "O", "i_lastw_y", pAnls.i_lastw_y);

	add_float_data(pkg, "S", "m_horz_k", pAnls.set.m_horz_k);
	add_float_data(pkg, "S", "m_sup_k", pAnls.set.m_sup_k);

	add_int_data(pkg, "S", "m_horz_ctl", pAnls.set.m_horz_ctl);

	add_points_data(pkg, "O", "edgel", &(pAnls.edgel->line[0]), &(pAnls.edgel->endpos));
	add_points_data(pkg, "O", "edger", &(pAnls.edger->line[0]), &(pAnls.edger->endpos));
	add_points_data(pkg, "O", "mid", &(pAnls.mid->line[0]), &(pAnls.mid->endpos));
}

void canls_dll_run(unsigned char* src)
{
	_camerafunc(src); //详见 camera.h

	//这里添加你的图像分析算法的分析函数 ---------------------------第5处
	anls_anls();
}

void canls_dll_cls()
{
	//用于刷新状态机或者重新设置变量值 -----------------------------第6处
	anls_fresh();
}

char* canls_dll_ver(int* cam_w, int* cam_h)
{
	*cam_w = FRAME_WIDTH;
	*cam_h = FRAME_HEIGHT;
	return PACKAGE_VERSION;
}

#define STRLENGTH (strlen(pname) * sizeof(char))

static int wfpi = 0, wipi = 0;
static int sipi = 0, sfpi = 0;
static int wlpi = 0;

void add_float_data(struct _DATAPACK* pack, char* type, char* pname, float* addr)
{
	if (strcmp(type, "O") == 0)
	{
		if (wfpi < MAXSIZE)
		{
			memcpy((*pack).OF[wfpi].name, pname, STRLENGTH);
			(*pack).OF[wfpi++].pData = addr;
		}
	}
	else if (strcmp(type, "S") == 0)
	{
		if (sfpi < MAXSIZE)
		{
			memcpy((*pack).SF[sfpi].name, pname, STRLENGTH);
			(*pack).SF[sfpi++].pData = addr;
		}
	}
	(*pack).OFC = wfpi, (*pack).SFC = sfpi;
}

void add_int_data(struct _DATAPACK* pack, char* type, char* pname, int* addr)
{
	if (strcmp(type, "O") == 0)
	{
		if (wipi < MAXSIZE)
		{
			memcpy((*pack).OI[wipi].name, pname, STRLENGTH);
			(*pack).OI[wipi++].pData = addr;
		}
	}
	else if (strcmp(type, "S") == 0)
	{
		if (sipi < MAXSIZE)
		{
			memcpy((*pack).SI[sipi].name, pname, STRLENGTH);
			(*pack).SI[sipi++].pData = addr;
		}
	}
	(*pack).OIC = wipi, (*pack).SIC = sipi;
}

void add_points_data(struct _DATAPACK* pack, char* type, char* pname, struct _POINT* addr1, int* addr2)
{
	if (strcmp(type, "O") == 0)
	{
		if (wlpi < MAXSIZE)
		{
			memcpy((*pack).OP[wlpi].name, pname, STRLENGTH);
			(*pack).OP[wlpi].pPoints = addr1;
			(*pack).OP[wlpi++].pCount = addr2;
		}
	}
	(*pack).OPC = wlpi;
}