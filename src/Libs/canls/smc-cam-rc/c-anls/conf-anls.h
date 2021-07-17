#ifndef _ANLS_CONF_H
#define _ANLS_CONF_H //配置文件

#define SELF_TEST //test single package
#ifndef SELF_TEST
#include "../../bsp/inc/bsp_ov7725.h"//camera header file
#endif

// - configuration images
#define IMG_HEIGHT      (60) //hight of image
#define IMG_WIDTH       (120)//width of image

#define ERROR_LINE      (5)  //line<=5,the image is not true
#define IMAGE_TOP       (0)  //0
#define IMAGE_BOTTOM    (59) //IMG_HEIGHT - 1
#define IMAGE_LEFT      (0)  //0
#define IMAGE_RIGHT     (119)//IMG_WIDTH - 1
#define IMAGE_CENTER    (60) //IMG_WIDTH >>1 

#define IMG_WHITE       (1)  //white
#define IMG_BLACK       (0)  //black

#ifndef SELF_TEST
//Image_Cache,Images are stored in a two - dimensional array called Image_Cache
//judge whether a element is white 
#define POINT_WHITE(x,y) (Image_Cache[y][x] == IMG_WHITE)
#else
#include "../../dev/camera.h"
//POINT_WHITE 定义在as_camera.h中
#endif

//black-white;towards left
#define POINT_FILTER_LEFT(x,y)  \
       (POINT_WHITE(x,y) && !POINT_WHITE(x-1,y))
//white-black;towards right
#define POINT_FILTER_RIGHT(x,y) \
       (POINT_WHITE(x,y) && !POINT_WHITE(x+1,y))
//black-white;upward
#define POINT_FILTER_UP(x,y)    \
       (POINT_WHITE(x,y) && !POINT_WHITE(x,y-1))
//white-black;downward
#define POINT_FILTER_DOWN(x,y)  \
       (POINT_WHITE(x,y) && !POINT_WHITE(x,y+1))

// - configuration switch
#ifndef SELF_TEST
#include "../../bsp/inc/bsp_board.h"  //switch

#define IMG_DRAW_EN      (Switch_1 == Switch_ON) //analysis of image drawing switch
#define IMG_SCALE_EN     (Switch_2 == Switch_ON) //dial drawing switch 
#else
#define IMG_DRAW_EN  (1)
#define IMG_SCALE_EN (0)
#endif

#endif
