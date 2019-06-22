/*******************************************************************************
	ブロック崩し
	参考: http://nn-hokuson.hatenablog.com/entry/2017/08/17/200918

	.NetCore/WPFで制作し、各クラス化とか速度の分解と角度の追加とか。
	ゲームとか殆ど作ったことがない人が作ったガラクタ。
	もう当たり判定全部短形でいいんじゃないかなとか。
	なんか斜めからボールが入ると当たり判定がガタガタになる。

	のHSP3dish版。jsにするとなんか遅い。きっとawaitの仕様が違うせい。
*******************************************************************************/
	
#include "BBUtil.as"

;ボールモジュール
#module Ball type, \
	shps,\
	__radius,\
	__pos,pos0,\
	deg,__rad,\
	__speed,speed0,\
	__color,\
	__isCleared

	#const isStart 0

	#modcfunc blLeft
		return __pos.X-__radius
	#modcfunc blTop
		return __pos.Y-__radius
	#modcfunc blRight
		return __pos.X+__radius
	#modcfunc blBottom
		return __pos.Y+__radius

	#define ready \
		vcClone __pos,pos0 :\
		__rad=deg/180*M_PI :\
		__speed=speed0 :\
		draw thismod :\
		btStart isStart,1500

	#define new(%1,%2,%3,%4,%5,%6,%7,%8) dimtype %1,5: newmod %1,Ball,%2,%3,%4,%5,%6,%7,%8
	#modinit array _shps,double _radius,double posX,double posY,double _deg,double _speed,str _color
		type="Ball"
		shps=_shps
		__radius=_radius
		pos0=posX,posY
		deg=_deg
		speed0=_speed
		cc2rgb __color,_color
		ready
	return

	#modfunc blUpdate
		;クリア判定
		if mlCount(shps)<=1 {
			clear thismod
			return
		}

		;動くのは用意してからちょっと待つ
		if 1!btIsFinish(isStart): draw thismod: return

		;ボール
		__pos.X+=cos(__rad)*__speed
		__pos.Y+=sin(__rad)*__speed

		;壁
		if blLeft(thismod)<0 {
			__pos.X=__radius
			__rad=M_PI-__rad
		}
		else:if fdWidth<blRight(thismod) {
			__pos.X=double(fdWidth)-__radius
			__rad=M_PI-__rad
		}
		if blTop(thismod)<0 {
			__pos.Y=__radius
			__rad=M_PI*2-__rad
		}
/*		else:if fdHeight<blBottom(thismod) {
			__pos.Y=double(fdHeight)-__radius
			__rad=M_PI*2-__rad
		}*/

		;ボールロスト
		if fdHeight<blTop(thismod) {
			ready
			return
		}
		;パドルとブロックの当たり判定
		shpsCount=mlCount(shps)
		repeat shpsCount
			rCnt=shpsCount-cnt-1
			mlRef shps,rCnt,v
			p1=bkLeft(v),bkTop(v)
			p2=bkRight(v),bkTop(v)
			if VsCircle(v,__pos,__radius) {
				hit v,__rad,__speed
				if getType(v)!="Block": continue
				mlRemove shps,rCnt
			}
		loop
		rad\=2*M_PI
		draw thismod
	return

	#modfunc local draw
		color apply3(__color)
		circle blLeft(thismod),blTop(thismod),blRight(thismod),blBottom(thismod)
	return

	;クリア処理
	#modfunc local clear
		font "メイリオ",72
		ccolor "#AAAAFF"
		pos fdWidth/2-100,fdHeight/2-50
		mes "Clear!"
		__isCleared=0
	return
#global

;ブロックモジュール
#module Block type,\
	__size,\
	__pos,\
	__color,\
	virtual_update,\
	virtual_hit,\
	lastHit0,lastHit1,\
	__rad,accel,__speed

	#modcfunc getType
		return type
	#modcfunc bkLeft
		return __pos.X-__size.X/2
	#modcfunc bkTop
		return __pos.Y-__size.Y/2
	#modcfunc bkRight
		return __pos.X+__size.X/2
	#modcfunc bkBottom
		return __pos.Y+__size.Y/2

	#define super \
		__size=sizeX,sizeY :\
		__pos=posX,posY :\
		cc2rgb __color,_color :\
		virtual_update=*override_update :\
		virtual_hit=*override_hit :\
		bkUpdate thismod

	#define news(%1,%2,%3,%4,%5,%6) newmod %1,Block,%2,%3,%4,%5,%6
	#modinit double sizeX,double sizeY,double posX,double posY,str _color
		type="Block"
		super
	return

	#modfunc bkUpdate
		gosub virtual_update
	return

	*override_update
		draw thismod
	return

	#modfunc local draw
		color apply3(__color)
		boxf bkLeft(thismod),bkTop(thismod),bkRight(thismod),bkBottom(thismod)
	return

	;上下左右の線のセット
	#define getLines(%1,%2,%3) ddim %2,2,4: ddim %3,2,4: __getLines %1,%2,%3
	#modfunc __getLines array l0,array l1
		l0(0,0)=bkLeft(thismod), bkTop(thismod):   l1(0,0)=bkRight(thismod),bkTop(thismod)
		l0(0,1)=bkRight(thismod),bkTop(thismod):   l1(0,1)=bkRight(thismod),bkBottom(thismod)
		l0(0,2)=bkRight(thismod),bkBottom(thismod):l1(0,2)=bkLeft(thismod), bkBottom(thismod)
		l0(0,3)=bkLeft(thismod), bkBottom(thismod):l1(0,3)=bkLeft(thismod), bkTop(thismod)
	return

	;当たり判定
	#modcfunc vsCircle array bPos,double _radius
		rtn=0
		getLines thismod,l0,l1

		repeat length2(l0)
			v0=l0.X.cnt,l0.Y.cnt
			v1=l1.X.cnt,l1.Y.cnt
			if lineVsCircle(v0,v1,bPos,_radius) {
				if v0.X=v1.X {
					if v0.X=bkLeft(thismod) {bPos.X=bkLeft(thismod)-_radius}
					else:if v0.X=bkRight(thismod) {bPos.X=bkRight(thismod)+_radius}
				}
				else:if v0.Y=v1.Y {
					if v0.Y=bkTop(thismod) {bPos.Y=bkTop(thismod)-_radius}
					else:if v0.Y=bkBottom(thismod) {bPos.Y=bkBottom(thismod)+_radius}
				}
				vcClone lastHit0,v0: vcClone lastHit1,v1
				rtn=1: break
			}
		loop
	return rtn

	;線と円の当たり判定
	;理解できていないメソッド
	#defcfunc local lineVsCircle array p0,array p1,array center,double radius
		lineDir=vcSub(p1,p0)   ;パドルの方向ベクトル
		n=lineDir.Y,-lineDir.X ;パドルの法線
		vcNormalize n

		dir1=vcSub(center,p0)
		dir2=vcSub(center,p1)

		dist=abs(vcDot(dir1,n))
		a1=vcDot(dir1,lineDir)
		a2=vcDot(dir2,lineDir)
	return a1*a2<0 & dist<radius

	#modfunc hit var _rad,var _speed
		radB=_rad: speedB=_speed
		gosub virtual_hit
		_rad=radB: _speed=speedB
	return

	;当たると反射する
	*override_hit
		_x=lastHit1.X-lastHit0.X
		_y=lastHit1.Y-lastHit0.Y
		_line=1.0+cos(atan(_y,_x))
		radB=_line*M_PI-radB
	return
#global

;パドルモジュール キーボードで動かせる
#module Paddle type,\
	__size,\
	__pos,\
	__color,\
	virtual_update,\
	virtual_hit,\
	lastHit0,lastHit1,\
	__rad,accel,__speed

	#define ctype me(%1) %1@Block
	#define news(%1,%2,%3,%4,%5,%6,%7) newmod %1,Paddle,%2,%3,%4,%5,%6,%7
	#modinit double sizeX,double sizeY,double posX,double posY,str _color,double _accel
		type="Paddle"
		super@Block
		dim __rad
		accel=_accel
		dim __speed
	return

	;うごく
	*override_update
		if left@KursorKey & 0<bkLeft(thismod) {
			me(__speed)=me(accel)
			me(__rad)=M_PI
			me(__pos).X-=me(__speed)
		}
		else:if right@KursorKey & bkRight(thismod)<fdWidth {
			me(__speed)=me(accel)
			me(__rad)=0
			me(__pos).X+=me(__speed)
		}
		else: me(__speed)=0

		draw@Block thismod
	return

	;パドルのスピードでボールに変化をもたらす存在
	*override_hit
		gosub*override_hit@Block
		me(radB)=me(radB)+cos(me(__rad))*me(__speed)/180*M_PI
	return
#global

;メインモジュール
#module MainWindow
	#deffunc local Main
		fdUpdate
		dimtype _shps,5
		news@Paddle _shps,100,5,fdWidth/2,fdHeight-50,"#66CCFF",20
		blockColors="#9999FF","#99FF99","#FF9999","#99FFFF","#FFFF99","#FF99FF","#FFFFFF"
		repeat 7: i=cnt
			repeat 5
				news@Block _shps,80,30,50+i*90,25+cnt*40,blockColors(i)
			loop
		loop
		new@MList shps,_shps
		new@Ball _ball,shps,10,fdWidth/2,300,90,10,"#FF00FF"

		;インターバル
		*interval
			redraw 0
			fdUpdate
			repeat mlCount(shps)
				mlRef shps,cnt,v
				bkUpdate v
			loop
			blUpdate _ball
			redraw 1
			await 33
		goto*interval
	return
#global
