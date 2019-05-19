#module Ball shps,__radius,__pos,pos0,deg,__rad,__speed,speed0,startTime,clr
	#uselib "winmm"
	#cfunc timeGetTime "timeGetTime"

	#modcfunc blLeft
		return __pos.X-__radius
	#modcfunc blTop
		return __pos.Y-__radius
	#modcfunc blRight
		return __pos.X+__radius
	#modcfunc blBottom
		return __pos.Y+__radius

	#define ready(%1,%2,%3,%4,%5,%6,%7,%8,%9)\
		foreach %1: shps.cnt=%1(cnt): loop :\
		__radius=%2 :\
		__pos=%3,%4 :\
		pos0=%3,%4 :\
		deg=%5 :\
		__rad=%5/180*M_PI :\
		__speed=%6 :\
		speed0=%6 :\
		clr=%7,%8,%9 :\
		startTime=timeGetTime()+1000

	#define new(%1,%2,%3,%4,%5,%6,%7,%8,%9,%10) dimtype %1,5: newmod %1,Ball,%2,%3,%4,%5,%6,%7,%8,%9,%10
	#modinit array _shps,double _radius,double posX,double posY,double _deg,double _speed,int _R,int _G,int _B
		dimtype shps,5,length(_shps)
		ready _shps,_radius,posX,posY,_deg,_speed,_R,_G,_B
	return

	#modfunc blUpdate
		;動くのは用意してからちょっと待つ
		if timeGetTime()<startTime: drow thismod: return
		;ボール
		__pos.X+=cos(__rad)*__speed
		__pos.Y+=sin(__rad)*__speed
		;壁
		if blLeft(thismod)<0 | fdWidth<blRight(thismod): __rad=M_PI-__rad
		if blTop(thismod)<0: __rad=M_PI*2-__rad
		;ボールロスト
		if(fdHeight<blTop(thismod)){
			ready shps,__radius,pos0.X,pos0.Y,deg,speed0,clr.R,clr.G,clr.B
			return
		}
		;パドルとブロックの当たり判定
		foreach shps
			p1=bkLeft(shps.cnt),bkTop(shps.cnt)
			p2=bkRight(shps.cnt),bkTop(shps.cnt)
			if(lineVsCircle@Block(p1,p2,__pos,__radius)){
				;dialog strf("p1:%.1f,%.1f p2:%.1f,%.1f pos:%.1f,%.1f r:%1.f",p1.X,p1.Y,p2.X,p2.Y,__pos.X,__pos.Y,__radius)
				hit shps(cnt),__rad,__speed
				if cnt=0: continue
				delmod shps.cnt
			}
		loop
		rad\=2*M_PI
		drow thismod
	return

	#modfunc local drow
		color clr.R,clr.G,clr.B
		circle blLeft(thismod),blTop(thismod),blRight(thismod),blBottom(thismod)
	return
#global

#module Block __size,__pos,deg,__rad,__speed,clr,lastHit
	#modcfunc bkLeft
		return __pos.X-__size.X/2
	#modcfunc bkTop
		return __pos.Y-__size.Y/2
	#modcfunc bkRight
		return __pos.X+__size.X/2
	#modcfunc bkBottom
		return __pos.Y+__size.Y/2

	#define news(%1,%2,%3,%4,%5,%6,%7,%8) newmod %1,Block,%2,%3,%4,%5,%6,%7,%8
	#modinit double sizeX,double sizeY,double posX,double posY,int _R,int _G,int _B
		__size=sizeX,sizeY
		__pos=posX,posY
		clr=_R,_G,_B
	return

	#modfunc bkUpdate
		drow thismod
	return

	#modfunc local drow
		color clr.R,clr.G,clr.B
		boxf bkLeft(thismod),bkTop(thismod),bkRight(thismod),bkBottom(thismod)
	return

	;ないせき
	#define ctype dotProduct(%1,%2) %1(X)*%2(X)+%1(Y)*%2(Y)
    #define normalize(%1) %tnormalize \
		%i=sqrt(powf(%1(X),2)+powf(%1(y),2)) :\
		%1(X)/=%p :\
		%1(Y)/=%o
	#define ctype atan2(%1,%2) atan(%1/%2)

	;線と円の当たり判定
	;理解できていないメソッド
	#defcfunc local lineVsCircle array p0,array p1,array center,double radius
		lineDir=p1.X-p0.X, p1.Y-p0.Y	; パドルの方向ベクトル
		mesv lineDir
		n=lineDir.Y,-lineDir.X			; パドルの法線
		mesv n
		normalize n
		mesv n

		dir1=center.X-p0.X, center.Y-p0.Y
		mesv dir1
		dir2=center.X-p1.X, center.Y-p1.Y
		mesv dir2

		dist=abs(dotProduct(dir1,n))
		mes dist
		a1=dotProduct(dir1,lineDir)
		mes a1
		a2=dotProduct(dir2,lineDir)
		mes a2
	if a1*a2<0 & dist<radius: return 1: else: return 0


	//当たると反射する
	#modfunc hit var _rad,var _speed
		_x=bkTop(thismod)-bkTop(thismod)
		_y=bkLeft(thismod)-bkRight(thismod)
		_line=(1+cos(atan2(_x,_y)))
		_rad=_line*M_PI-_rad
	return
#global

#module Paddle __size,__pos,deg,__rad,__speed,clr,lastHit

#global

#module MainWindow
	#deffunc local Main
		dimtype shps,5
		;shps.Add(new Paddle(new Vector(100,5),new Vector(Field.Width/2,this.Height-50),"#9999FF",20))
		news@Block shps,500,5,fdWidth/2,fdHeight-50,$99,$99,$FF
		repeat 7: i=cnt
			repeat 5
				news@Block shps,80,30,50+i*90,25+cnt*40,$33,$99,$FF
			loop
		loop
		new@Ball _ball,shps,10,fdWidth/2,300,210,10,$FF,$00,$FF

		p1=10,10: p2=90,10: _pos=76,166
		mes lineVsCircle@Block(p1,p2,_pos,10)
		
/*
100,0
0,-100
0,-1
-100,-100
-200,-100
100
-10000
-20000
False
100,0
0,-100
0,-1
50,-5
-50,-5
5
5000
-5000
True
*/

		;インターバル
		*interval
			redraw 0
			fdUpdate
			foreach shps
				bkUpdate shps(cnt)
			loop
			blUpdate _ball
			redraw 1
			await 33
		goto*interval
	return
#global