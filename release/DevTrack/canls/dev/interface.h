#ifndef _DEV_DLL_INTERFACE_
#define _DEV_DLL_INTERFACE_

#define MAXSIZE (10)

struct _POINT { int x, y; };
struct _POINT_SET { char name[20]; struct _POINT* pPoints; int* pCount; };
struct _FLOAT { char name[20]; float* pData; };
struct _INTEGER { char name[20]; int* pData; };
struct _DATAPACK {
	int OFC;
	struct _FLOAT OF[MAXSIZE];     //float data is to be observed
	int OIC;
	struct _INTEGER OI[MAXSIZE];   //integer data is to be observed
	int SFC;
	struct _FLOAT SF[MAXSIZE];     //float data that can be set
	int SIC;
	struct _INTEGER SI[MAXSIZE];   //integer data that can be set
	int OPC;
	struct _POINT_SET OP[MAXSIZE]; //points data is to be observed
};

#ifdef __cplusplus
extern "C"
{
#endif
	__declspec(dllexport) char* canls_dll_ver(int*, int*);
	__declspec(dllexport) void canls_dll_init(struct _DATAPACK*);
	__declspec(dllexport) void canls_dll_cls();
	__declspec(dllexport) void canls_dll_run(unsigned char*);
#ifdef __cplusplus
}
#endif
#endif