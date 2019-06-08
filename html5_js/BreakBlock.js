/*******************************************************************************
	ブロック崩し
	参考: http://nn-hokuson.hatenablog.com/entry/2017/08/17/200918

	.NetCore/WPFで制作し、各クラス化とか速度の分解と角度の追加とか。
	ゲームとか殆ど作ったことがない人が作ったガラクタ。
	もう当たり判定全部短形でいいんじゃないかなとか。
	なんか斜めからボールが入ると当たり判定がガタガタになる。

	のhtml5/JavaScript版。
*******************************************************************************/

"use strict";

//ボールクラス
const Ball=(()=>{
	const draw=Symbol();
	const shps=Symbol();
	const deg=Symbol();
	const speed0=Symbol();
	const isStart=Symbol;
	const isCleared=Symbol();
	const color=Symbol();

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
			this.ready();
		}

		ready(){
			this.pos=Object.assign({},this.pos0);
			this.rad=this[deg]/180*Math.PI;
			this.speed=this[speed0];

			this[draw]();
			this[isStart]=false;
			setTimeout(()=>{this[isStart]=true},1500);
		}

		update(){
			//動くのは用意してからちょっと待つ
			if(!this[isStart]) return this[draw]();
			//ボール
			this.pos.X+=Math.cos(this.rad)*this.speed;
			this.pos.Y+=Math.sin(this.rad)*this.speed;
			//壁
			if(this.left<0 || Field.width<this.right) this.rad=Math.PI-this.rad;
			if(this.top<0 /*|| Field.height<this.bottom*/) this.rad=2*Math.PI-this.rad;
			//ボールロスト
			if(Field.height<this.top){
				this.ready();
				return;
			}

			this.rad%=2*Math.PI;
			this[draw]();
		}
	}

	Ball.prototype[draw]=function(){
		Field2d.beginPath();
		Field2d.arc(this.pos.X,this.pos.Y,this.radius,0,2*Math.PI,false);
		Field2d.fillStyle=this[color];
		Field2d.fill();
	};
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

		constructor(size,pos,_color){
			this.size=size;
			this.pos=pos;
			this[color]=_color;
		}

		update(){
			this[draw]();
		}
	}
	const pvt=Block.prototype;
	pvt[draw]=function(){
		Field2d.fillStyle=this[color];
		Field2d.fillRect(this.left,this.top,this.size.X,this.size.Y);
	};

	//上下左右の線のセット
	pvt.__defineGetter__[lines]=[
		[{X:this.left ,Y:this.top},   {X:this.right,Y:this.top}],
		[{X:this.right,Y:this.top},   {X:this.right,Y:this.bottom}],
		[{X:this.right,Y:this.bottom},{X:this.left, Y:this.bottom}],
		[{X:this.left ,Y:this.bottom},{X:this.left, Y:this.top}]
	];

	//ないせき
	const dot=(a,b)=>a.X*b.X+a.Y*b.Y;

	//線と円の当たり判定
	//理解できていないメソッド
	function lineVsCircle(p,center,radius){
		/*const lineDir=p[1]-p[0];                   //パドルの方向ベクトル
		Vector n=new Vector(lineDir.Y, -lineDir.X); //パドルの法線
		n.Normalize();

		Vector dir1=center-p[0];
		Vector dir2=center-p[1];

		double dist=Math.Abs(dir1*n);
		double a1=dir1*lineDir;
		double a2=dir2*lineDir;

		return a1*a2<0 && dist<radius;*/
	}
	return Block;
})();

//パドルクラス キーボードで動かせる
const Paddle=Block;

//ボールフィールドをグローバルのに保持
var Field,Field2d;

class MainWindow{
	static MainWindow(){
		Field=document.getElementById("Field");
		Field2d=Field.getContext("2d");

		var shps=new Array();
		shps.push(new Paddle({X:800,Y:5},{X:Field.width/2,Y:Field.height-50},"#9999FF",20));
		for(var i=0;i<7;i++){
			for(var n=0;n<5;n++){
				shps.push(new Block({X:80,Y:30},{X:50+i*90,Y:25+n*40},["#0000FF","#00FF00","#FF0000","#00FFFF","#FFFF00","#FF00FF","#FFFFFF"][i]));
			}
		}
		var ball=new Ball(shps,10,{X:Field.width/2,Y:300},220,10,"#FF00FF");

		//インターバル
		setInterval(()=>{
			Field2d.clearRect(0,0,Field.width,Field.height);
			for(var v of shps){
				v.update();
			}
			ball.update();
		},33);
		/*///Field2d.fillRect(0,0,100,100);
		Field2d.beginPath () ;
		Field2d.arc(100,100,50,0,2*Math.PI,false);
		// 塗りつぶしの色
		Field2d.fillStyle = "#3399FF";
		Field2d.fill() ;
		var i=0;
		function loop(){
			Field2d.beginPath();
			Field2d.clearRect(0,0,Field.width,Field.height);
			Field2d.arc(i,i,10,0,Math.PI,false);
			Field2d.fillStyle="#000099";
			Field2d.fill;
			i+=3;
		}
		//setInterval(loop,33);*/
	}
}