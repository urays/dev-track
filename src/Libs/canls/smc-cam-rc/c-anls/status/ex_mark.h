
//<!-- 此文件用于被外部调用的赛道状态信息标志 -->

#ifndef _ANLS_EXMARK_H
#define _ANLS_EXMARK_H

#define AT_UNKNOW          (0x00) //未知
#define	AT_OUTER           (0x01) //出界
#define	AT_ZEBRA           (0x02) //停车线
#define	AT_STRAIGHT        (0x03) //直道

#define	AT_BLOCK_PL        (0x04) //前方疑似障碍左
#define	AT_BLOCK_PR        (0x05) //前方疑似障碍右
#define	AT_BLOCK_SL        (0x06) //障碍 左
#define	AT_BLOCK_SR        (0x07) //障碍 右

#define	AT_BEND_L          (0x08) //弯道 左
#define	AT_BEND_R          (0x09) //弯道 右

#define	AT_CROSS_PRE       (0x0a) //十字前
#define	AT_CROSS_IN        (0x0b) //十字中

#define	AT_ISLAND_PRE_L    (0x0c) //环岛前
#define	AT_ISLAND_PRE_R    (0x0d)
#define	AT_ISLAND_LANE_L   (0x0e) //环岛小巷中
#define	AT_ISLAND_LANE_R   (0x0f)
#define	AT_ISLAND_LOOP_L   (0x10) //环岛小环中
#define	AT_ISLAND_LOOP_R   (0x11)


#endif
