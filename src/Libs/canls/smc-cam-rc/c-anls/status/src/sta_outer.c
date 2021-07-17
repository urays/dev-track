#include "../../suprc/inc/common.h"
//#include "../set_table.h"
#include "../../suprc/inc/sup_exfunc.h"

#include "../../conf-anls.h"
#include "../inc/sta_outer.h"

char check_outer(void)
{
	int start_column = IMAGE_CENTER;

	if (POINT_WHITE(IMAGE_CENTER, IMAGE_BOTTOM)) start_column = IMAGE_CENTER;
	else if (POINT_WHITE(IMAGE_CENTER - 20, IMAGE_BOTTOM)) start_column = IMAGE_CENTER - 20;
	else if (POINT_WHITE(IMAGE_CENTER + 20, IMAGE_BOTTOM)) start_column = IMAGE_CENTER + 20;

	if (WhiteCount_LR(EXF_BOTH, start_column, IMAGE_BOTTOM) < 15) {
		if (WhiteCount_LR(EXF_BOTH, start_column, IMAGE_BOTTOM - 3) < 15) {
			if (WhiteCount_LR(EXF_BOTH, start_column, IMAGE_BOTTOM - 5) < 15) {
				return itrue;
			}
		}
	}
	return ifalse;
}

//这里添加出界后自寻找赛道代码
//::TODO
