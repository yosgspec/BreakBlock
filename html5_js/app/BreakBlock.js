/*******************************************************************************
	ブロック崩し
	参考: http://nn-hokuson.hatenablog.com/entry/2017/08/17/200918

	.NetCore/WPFで制作し、各クラス化とか速度の分解と角度の追加とか。
	ゲームとか殆ど作ったことがない人が作ったガラクタ。
	もう当たり判定全部短形でいいんじゃないかなとか。
	なんか斜めからボールが入ると当たり判定がガタガタになる。

	のhtml5/JavaScript版。HSP並みのゲームスピード。
	やはりこれが本来のゲームスピードなのか…?
*******************************************************************************/

"use strict";

const {Vector,CursorKey}=BBUtil;

//ボールクラス
const Ball=(()=>{
	const draw=Symbol();
	const shps=Symbol();
	const deg=Symbol();
	const speed0=Symbol();
	const isStart=Symbol;
	const color=Symbol();
	const isCleared=Symbol();
	const clear=Symbol();

	class Ball{
		get left(){return this.pos.X-this.radius;}
		get top(){return this.pos.Y-this.radius;}
		get right(){return this.pos.X+this.radius;}
		get bottom(){return this.pos.Y+this.radius;}

		constructor(_shps,radius,pos,_deg,speed,_color){
			this[shps]=_shps;
			this.radius=radius;
			this.pos0=pos;
			this[deg]=_deg;
			this[speed0]=speed;
			this[color]=_color;
			this[isCleared]=false;
			this.ready();
		}

		ready(){
			this.pos=this.pos0.clone();
			this.rad=this[deg]/180*Math.PI;
			this.speed=this[speed0];

			this[draw]();
			this[isStart]=false;
			setTimeout(()=>{this[isStart]=true},1500);
		}

		update(){
			//クリア判定
			if(this[shps].$.length<=1){
				this[clear]();
				return;
			}

			//動くのは用意してからちょっと待つ
			if(!this[isStart]) return this[draw]();
			//ボール
			this.pos.addSet(new Vector(
				Math.cos(this.rad)*this.speed,
				Math.sin(this.rad)*this.speed
			));
			//壁
			if(this.left<0 || Field.width<this.right) this.rad=Math.PI-this.rad;
			if(this.top<0) this.rad=2*Math.PI-this.rad;
			//ボールロスト
			if(Field.height<this.top){
				this.ready();
				return;
			}

			//パドルとブロックの当たり判定
			this[shps].$=this[shps].$.filter(v=>{
				if(v.vsCircle(this.pos,this.radius)){
					[this.rad,this.speed]=v.hit(this.rad,this.speed);
					if(Paddle.prototype.isPrototypeOf(v)) return true;
					return false;
				}
				return true;
			});
			this.rad%=2*Math.PI;
			this[draw]();
		}
	}

	const pvt=Ball.prototype;
	pvt[draw]=function(){
		Field2d.beginPath();
		Field2d.arc(this.pos.X,this.pos.Y,this.radius,0,2*Math.PI,false);
		Field2d.fillStyle=this[color];
		Field2d.fill();
	};

	//クリア処理
	pvt[clear]=function(){
		Field2d.font="72px メイリオ";
		Field2d.fillStyle="#AAAAFF";
		Field2d.fillText("Clear!",Field.width/2-100,Field.height/2);
		this[isCleared]=true;
	}
	return Ball;
})();

//ブロッククラス
const Block=(()=>{
	const draw=Symbol();
	const lastHit=Symbol();
	const color=Symbol();
	const lines=Symbol();

	class Block{
		get left(){return this.pos.X-this.size.X/2;}
		get top(){return this.pos.Y-this.size.Y/2;}
		get right(){return this.pos.X+this.size.X/2;}
		get bottom(){return this.pos.Y+this.size.Y/2;}

		constructor(size,pos,_color,refProt={}){
			refProt.draw=draw;
			this.size=size;
			this.pos=pos;
			this[color]=_color;
		}

		update(){
			this[draw]();
		}


		//上下左右の線のセット
		get __lines(){return [
			[new Vector(this.left ,this.top),   new Vector(this.right,this.top)],
			[new Vector(this.right,this.top),   new Vector(this.right,this.bottom)],
			[new Vector(this.right,this.bottom),new Vector(this.left, this.bottom)],
			[new Vector(this.left ,this.bottom),new Vector(this.left, this.top)]
		]};

		//当たり判定
		vsCircle(bPos,radius){
			for(var v of this[lines]){
				if(lineVsCircle(v,bPos,radius)){
					if(v[0].X==v[1].X){
						if(v[0].X==this.left) bPos.X=this.left-radius;
						else if(v[0].X==this.right) bPos.X=this.right+radius;
					}
					else if(v[0].Y==v[1].Y){
						if(v[0].Y==this.top) bPos.Y=this.top-radius;
						else if(v[0].Y==this.bottom) bPos.Y=this.bottom+radius;
					}
					this.lastHit=v;
					return true;
				}
			}
			return false;
		}

		//当たると反射する
		hit(radB,speedB){
			var x=this.lastHit[1].X-this.lastHit[0].X;
			var y=this.lastHit[1].Y-this.lastHit[0].Y;
			var line=(1+Math.cos(Math.atan2(y,x)));
			radB=line*Math.PI-radB;
			return [radB,speedB];
		}
	}

	const pvt=Block.prototype;
	pvt[draw]=function(){
		Field2d.fillStyle=this[color];
		Field2d.fillRect(this.left,this.top,this.size.X,this.size.Y);
	};

	//上下左右の線のセット
	pvt.__defineGetter__(lines,function(){return [
		[new Vector(this.left ,this.top),   new Vector(this.right,this.top)],
		[new Vector(this.right,this.top),   new Vector(this.right,this.bottom)],
		[new Vector(this.right,this.bottom),new Vector(this.left, this.bottom)],
		[new Vector(this.left ,this.bottom),new Vector(this.left, this.top)]
	]});

	//線と円の当たり判定
	//理解できていないメソッド
	function lineVsCircle(p,center,radius){
		var lineDir=p[1].sub(p[0]);             //パドルの方向ベクトル
		var n=new Vector(lineDir.Y,-lineDir.X); //パドルの法線
		n.Normalize();

		var dir1=center.sub(p[0]);
		var dir2=center.sub(p[1]);

		var dist=Math.abs(dir1.dot(n));
		var a1=dir1.dot(lineDir);
		var a2=dir2.dot(lineDir);

		return a1*a2<0 && dist<radius;
	}
	return Block;
})();

//パドルクラス キーボードで動かせる
const Paddle=(()=>{
	const prot={};

	class Paddle extends Block{
		constructor(size,pos,color,accel){
			super(size,pos,color,prot);
			this.rad=0;
			this.accel=accel;
			this.speed=0;
		}

		//うごく
		update(){
			if(CursorKey.left && 0<this.left){
				this.speed=this.accel;
				this.rad=Math.PI;
				this.pos.X-=this.speed;
			}
			else if(CursorKey.right && this.right<Field.width){
				this.speed=this.accel;
				this.rad=0;
				this.pos.X+=this.speed;
			}
			else this.speed=0;

			this[prot.draw]();
		}

		//パドルのスピードでボールに変化をもたらす存在
		hit(radB,speedB){
			[radB,speedB]=super.hit(radB,speedB);
			return [radB+Math.cos(this.rad)*this.speed/180*Math.PI,speedB];
		}
	}
	return Paddle;
})();

//ボールフィールドをグローバルのに保持
var Field,Field2d;

//メインクラス
class MainWindow{
	static MainWindow(){
		Field=document.getElementById("Field");
		Field2d=Field.getContext("2d");

		const shps={$:new Array()};
		shps.$.push(new Paddle(new Vector(102,5),new Vector(Field.width/2,Field.height-50),"#66CCFF",20));
		const blockColors=["#9999FF","#99FF99","#FF9999","#99FFFF","#FFFF99","#FF99FF","#FFFFFF"];
		for(var i=0;i<7;i++){
			for(var n=0;n<5;n++){
				shps.$.push(new Block(new Vector(80,30),new Vector(50+i*90,25+n*40),blockColors[i]));
			}
		}
		const ball=new Ball(shps,10,new Vector(Field.width/2,300),90,10,"#FF00FF");

		//インターバル
		setInterval(()=>{
			Field2d.clearRect(0,0,Field.width,Field.height);
			for(var v of shps.$){
				v.update();
			}
			ball.update();
		},33);
	}
}
