/*******************************************************************************
	�u���b�N����
	�Q�l: http://nn-hokuson.hatenablog.com/entry/2017/08/17/200918

	.NetCore/WPF�Ő��삵�A�e�N���X���Ƃ����x�̕����Ɗp�x�̒ǉ��Ƃ��B
	�Q�[���Ƃ��w�Ǎ�������Ƃ��Ȃ��l��������K���N�^�B
	���������蔻��S���Z�`�ł����񂶂�Ȃ����ȂƂ��B
	�Ȃ񂩎΂߂���{�[��������Ɠ����蔻�肪�K�^�K�^�ɂȂ�B

	��html5/JavaScript�ŁB
*******************************************************************************/

"use strict";

//�{�[���N���X
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
			//�����̂͗p�ӂ��Ă��炿����Ƒ҂�
			if(!this[isStart]) return this[draw]();
			//�{�[��
			this.pos.X+=Math.cos(this.rad)*this.speed;
			this.pos.Y+=Math.sin(this.rad)*this.speed;
			//��
			if(this.left<0 || Field.width<this.right) this.rad=Math.PI-this.rad;
			if(this.top<0 /*|| Field.height<this.bottom*/) this.rad=2*Math.PI-this.rad;
			//�{�[�����X�g
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

//�u���b�N�N���X
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

	//�㉺���E�̐��̃Z�b�g
	pvt.__defineGetter__[lines]=[
		[{X:this.left ,Y:this.top},   {X:this.right,Y:this.top}],
		[{X:this.right,Y:this.top},   {X:this.right,Y:this.bottom}],
		[{X:this.right,Y:this.bottom},{X:this.left, Y:this.bottom}],
		[{X:this.left ,Y:this.bottom},{X:this.left, Y:this.top}]
	];

	//�Ȃ�����
	const dot=(a,b)=>a.X*b.X+a.Y*b.Y;

	//���Ɖ~�̓����蔻��
	//�����ł��Ă��Ȃ����\�b�h
	function lineVsCircle(p,center,radius){
		/*const lineDir=p[1]-p[0];                   //�p�h���̕����x�N�g��
		Vector n=new Vector(lineDir.Y, -lineDir.X); //�p�h���̖@��
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

//�p�h���N���X �L�[�{�[�h�œ�������
const Paddle=Block;

//�{�[���t�B�[���h���O���[�o���̂ɕێ�
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

		//�C���^�[�o��
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
		// �h��Ԃ��̐F
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