#module MList list,count
	#modcfunc mlCount
		return count
	#define new(%1,%2) dimtype %1,5: newmod %1,MList,%2
	#modinit array ary
		count=length(ary)
		dimtype list,5,count
		foreach ary: list.cnt=ary.cnt: loop
	return
	#modfunc mlSet int index,var item
		list.index=item: return
	#modfunc local mlRef int index,var item
		item=list.index: return
	#define global mlRef(%1,%2,%3) dimtype %3,5: mlRef@MList %1,%2,%3
	#modfunc mlRemove int index
		count--
		repeat count-index,index
			list(cnt)=list(cnt+1)
		loop
	return
#global

#module Ball \
	shps,\
	__radius,\
	__pos,pos0,\
	deg,__rad,\
	__speed,speed0,\
	startTime,\
	clr

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

	#define ready \
		__pos=pos0.X,pos0.Y :\
		__rad=deg/180*M_PI :\
		__speed=speed0 :\
		startTime=timeGetTime()+1500

	#define new(%1,%2,%3,%4,%5,%6,%7,%8,%9,%10) dimtype %1,5: newmod %1,Ball,%2,%3,%4,%5,%6,%7,%8,%9,%10
	#modinit array _shps,double _radius,double posX,double posY,double _deg,double _speed,int _R,int _G,int _B
		;dimtype shps,5,length(_shps)
		;foreach _shps: shps.cnt=_shps(cnt): loop
		shps=_shps
		__radius=_radius
		pos0=posX,posY
		deg=_deg
		speed0=_speed
		clr=_R,_G,_B
		ready
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
		drow thismod
	return

	#modfunc local drow
		color clr.R,clr.G,clr.B
		circle blLeft(thismod),blTop(thismod),blRight(thismod),blBottom(thismod)
	return
#global

#module Block type,\
	__size,\
	__pos,\
	deg,__rad,\
	__speed,\
	clr,\
	virtual_update,\
	lastHit0,lastHit1

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
		clr=_R,_G,_B :\
		virtual_update=*override_update

	#define news(%1,%2,%3,%4,%5,%6,%7,%8) newmod %1,Block,%2,%3,%4,%5,%6,%7,%8
	#modinit double sizeX,double sizeY,double posX,double posY,int _R,int _G,int _B
		type="Block"
		super
	return

	#modfunc bkUpdate
		gosub virtual_update
	return

	*override_update
		drow thismod
	return

	#modfunc local drow
		color clr.R,clr.G,clr.B
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
		;v0=l0.X.2,l0.Y.2: v1=l1.X.2,l1.Y.2
		;lastHit0=v0.X,v0.Y: lastHit1=v1.X,v1.Y
		;return lineVsCircle(v0,v1,bPos,_radius)

		repeat length2(l0)
			v0=l0.X.cnt,l0.Y.cnt: v1=l1.X.cnt,l1.Y.cnt
			if lineVsCircle(v0,v1,bPos,_radius) {
				if v0.X=v1.X {
					if v0.X=bkLeft(thismod) {bPos.X=bkLeft(thismod)-_radius}
					else:if v0.X=bkRight(thismod) {bPos.X=bkRight(thismod)+_radius}
				}
				else:if v0.Y=v1.Y {
					if v0.Y=bkTop(thismod) {bPos.Y=bkTop(thismod)-_radius}
					else:if v0.Y=bkBottom(thismod) {bPos.Y=bkBottom(thismod)+_radius}
				}
				lastHit0=v0.X,v0.Y: lastHit1=v1.X,v1.Y
				rtn=1: break
			}
		loop
	return rtn

	;ないせき
	#define ctype dotProduct(%1,%2) %1(X)*%2(X)+%1(Y)*%2(Y)
    #define normalize(%1) %tnormalize \
		%i=sqrt(powf(%1(X),2)+powf(%1(y),2)) :\
		%1(X)/=%p :\
		%1(Y)/=%o

	;線と円の当たり判定
	;理解できていないメソッド
	#defcfunc local lineVsCircle array p0,array p1,array center,double radius
		lineDir=p1.X-p0.X, p1.Y-p0.Y	; パドルの方向ベクトル
		n=lineDir.Y,-lineDir.X			; パドルの法線
		normalize n

		dir1=center.X-p0.X, center.Y-p0.Y
		dir2=center.X-p1.X, center.Y-p1.Y

		dist=abs(dotProduct(dir1,n))
		a1=dotProduct(dir1,lineDir)
		a2=dotProduct(dir2,lineDir)
	if a1*a2<0 & dist<radius: return 1: else: return 0


	//当たると反射する
	#modfunc hit var _rad,var _speed
		_x=lastHit1.X-lastHit0.X
		_y=lastHit1.Y-lastHit0.Y
		_line=cos(atan(_x,_y))
		_rad=_line*M_PI-_rad
	return
#global

#module Paddle type,\
	__size,\
	__pos,\
	deg,__rad,\
	__speed,\
	clr,\
	virtual_update,\
	lastHit0,lastHit1

	#define news(%1,%2,%3,%4,%5,%6,%7,%8) newmod %1,Paddle,%2,%3,%4,%5,%6,%7,%8
	#modinit double sizeX,double sizeY,double posX,double posY,int _R,int _G,int _B
		type="Paddle"
		super@Block
	return

	*override_update
		drow@Block thismod
	return
#global

#module MainWindow
	#deffunc local Main
		dimtype _shps,5
		news@Paddle _shps,700,5,fdWidth/2,fdHeight-50,$99,$99,$FF
		;shps.Add(new Paddle(new Vector(100,5),new Vector(Field.Width/2,this.Height-50),"#9999FF",20))
		repeat 5: i=cnt
			repeat 4
				news@Block _shps,70,20,80+i*115,50+cnt*40,$33,$99,$FF
			loop
		loop
		new@MList shps,_shps
		new@Ball _ball,shps,10,fdWidth/2,300,210,10,$FF,$00,$FF

		p1=10,10: p2=90,10: _pos=76,166
		mes lineVsCircle@Block(p1,p2,_pos,10)
		
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
