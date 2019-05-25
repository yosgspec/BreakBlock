﻿/*******************************************************************************
	ブロック崩し
	参考: http://nn-hokuson.hatenablog.com/entry/2017/08/17/200918

	.NetCore/WPFで制作し、各クラス化とか速度の分解と角度の追加とか。
	ゲームとか殆ど作ったことがない人が作ったガラクタ。
	もう当たり判定全部短形でいいんじゃないかなとか。
	なんか斜めからボールが入ると当たり判定がガタガタになる。
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static System.Windows.Media.ColorConverter;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BreakBlock{
	//配置オブジェクトのインターフェース
	interface IDraw{
		Shape shp{set;get;}
		float left{get;}
		float top{get;}
		float right{get;}
		float bottom{get;}
		void update();
		bool vsCircle(Vector center,float radius);
		(float,float) hit(float rad,float speed);
	}

	//ボールクラス
	class Ball: IDraw{
		List<IDraw> shps;
		public Ellipse ball;
		public Shape shp{set;get;}
		public int radius;
		public Vector pos;
		public Vector pos0;
		float deg;
		public float rad;
		public float speed;
		float speed0;
		Task isStart;
		string color;
		bool isCleared;

		public float left{get{return (float)pos.X-radius;}}
		public float top{get{return (float)pos.Y-radius;}}
		public float right{get{return (float)pos.X+radius;}}
		public float bottom{get{return (float)pos.Y+radius;}}

		public Ball(List<IDraw> shps,int radius,Vector pos,float deg,float speed,string color){
			this.shps=shps;
			this.radius=radius;
			this.color=color;
			pos0=pos;
			this.deg=deg;
			speed0=speed;
			ready();
		}

		void ready(){
			pos=pos0;
			rad=(float)(deg/180*Math.PI);
			speed=speed0;

			ball=new Ellipse();
			Win.field.Children.Add(ball);
			ball.Width=radius*2;
			ball.Height=radius*2;
			ball.Fill=new SolidColorBrush((Color)ConvertFromString(color));
			shp=(Shape)ball;
			Draw();
			isStart=Task.Delay(1500);
		}

		public void update(){
			//クリア判定
			if(shps.Count<=1){
				clear();
				return;
			}

			//動くのは用意してからちょっと待つ
			if(!isStart.IsCompleted) return;
			//ボール
			pos+=new Vector(Math.Cos(rad)*speed,Math.Sin(rad)*speed);
			//壁
			if(left<0 || Win.field.Width<right) rad=(float)(Math.PI-rad);
			if(top<0) rad=(float)(2*Math.PI-rad);
			//ボールロスト
			if(Win.field.Height<top){
				Win.field.Children.Remove(ball);
				ready();
				return;
			}

			//パドルとブロックの当たり判定
			for(var i=shps.Count-1;0<=i;i--){
				if(shps[i].vsCircle(pos,radius)){
					(rad,speed)=shps[i].hit(rad,speed);
					if(shps[i].GetType()!=typeof(Block)) continue;
					Win.field.Children.Remove(shps[i].shp);
					shps.RemoveAt(i);
				}
			}
			rad%=(float)(2*Math.PI);
			Draw();
		}

		void Draw(){
			Canvas.SetLeft(ball,left);
			Canvas.SetTop(ball,top);
		}

		//クリア処理
		void clear(){
			if(isCleared) return;
			Win.field.Children.Remove(ball);
			var tb=new TextBlock();
			Win.field.Children.Add(tb);
			tb.Text="Clear!";
			tb.FontSize=72;
			tb.Foreground=new SolidColorBrush((Color)ConvertFromString("#AAAAFF"));
			Canvas.SetLeft(tb,Win.field.Width/2-100);
			Canvas.SetTop(tb,Win.field.Height/2-50);
			isCleared=true;
		}

		public bool vsCircle(Vector center,float radius){return false;}
		public (float,float) hit(float rad,float speed){return (0,0);}
	}

	//ブロッククラス
	class Block: IDraw{
		public Rectangle block;
		public Shape shp{set;get;}
		public Vector size;
		protected Vector pos;
		Vector[] lastHit;

		public float left{get{return (float)(pos.X-size.X/2);}}
		public float top{get{return (float)(pos.Y-size.Y/2);}}
		public float right{get{return (float)(pos.X+size.X/2);}}
		public float bottom{get{return (float)(pos.Y+size.Y/2);}}

		public Block(Vector size,Vector pos,string color){
			this.size=size;
			this.pos=pos;

			block=new Rectangle();
			Win.field.Children.Add(block);
			block.Width=size.X;
			block.Height=size.Y;
			block.Fill=new SolidColorBrush((Color)ConvertFromString(color));
			shp=(Shape)block;
		}

		public virtual void update(){
			Draw();
		}

		protected void Draw(){
			Canvas.SetLeft(block,left);
			Canvas.SetTop(block,top);
		}

		//上下左右の線のセット
		Vector[][] lines{get{return new[]{
			new[]{new Vector(left,top),new Vector(right,top)},
			new[]{new Vector(right,top),new Vector(right,bottom)},
			new[]{new Vector(right,bottom),new Vector(left,bottom)},
			new[]{new Vector(left,bottom),new Vector(left,top)}
		};}}

		//当たり判定
		public bool vsCircle(Vector bPos,float radius){
			foreach(var v in lines){
				if(Block.lineVsCircle(v,bPos,radius)){
					if(v[0].X==v[1].X){
						if(v[0].X==left) bPos.X=left-radius;
						else if(v[0].X==right) bPos.X=right+radius;
					}
					else if(v[0].Y==v[1].Y){
						if(v[0].Y==top) bPos.Y=top-radius;
						else if(v[0].Y==bottom) bPos.Y=bottom+radius;
					}
					lastHit=v;
					return true;
				}
			}
			return false;
		}

		//ないせき
		static Func<Vector,Vector,double> dotProduct=(a,b)=>a.X*b.X+a.Y*b.Y;

		//線と円の当たり判定
		//理解できていないメソッド
		public static bool lineVsCircle(Vector[] p,Vector center,float radius){
			Vector lineDir=p[1]-p[0];					// パドルの方向ベクトル
			Vector n=new Vector(lineDir.Y, -lineDir.X);	// パドルの法線
			n.Normalize();

			Vector dir1=center-p[0];
			Vector dir2=center-p[1];

			double dist=Math.Abs(dotProduct(dir1,n));
			double a1=dotProduct(dir1,lineDir);
			double a2=dotProduct(dir2,lineDir);

			return (a1*a2<0 && dist<radius)? true: false;
		}

		//当たると反射する
		public virtual (float,float) hit(float radB,float speedB){
			var x=lastHit[1].X-lastHit[0].X;
			var y=lastHit[1].Y-lastHit[0].Y;
			var line=(float)(1+Math.Cos(Math.Atan2(y,x)));
			radB=(float)(line*Math.PI-radB);
			return (radB,speedB);
		}
	}

	//パドルクラス キーボードで動かせる
	class Paddle: Block{
		public float rad;
		public float accel;
		public float speed;

		public Paddle(Vector size,Vector pos,string color,float accel):base(size,pos,color){
			this.accel=accel;
		}

		//うごく
		public override void update(){
			if(CursorKey.left && 0<left){
				speed=accel;
				rad=(float)Math.PI;
				pos.X-=speed;
			}
			else if(CursorKey.right && right<Win.field.Width){
				speed=accel;
				rad=0;
				pos.X+=speed;
			}
			else speed=0;

			Draw();
		}

		//パドルのスピードでボールに変化をもたらす存在
		public override (float,float) hit(float radB,float speedB){
			(radB,speedB)=base.hit(radB,speedB);
			return ((float)(radB+Math.Cos(rad)*speed/180*Math.PI),speedB);
		}
	}

	//キーボードの状態を管理するやつ
	static class CursorKey{
		public static bool up;
		public static bool left;
		public static bool right;
		public static bool down;

		public static Action<KeyEventArgs> keyDown=keyState(true);
		public static Action<KeyEventArgs> keyUp=keyState(false);
		static Action<KeyEventArgs> keyState(bool state){
			return new Action<KeyEventArgs>(e=>{
				switch(e.Key){
					case Key.Up: up=state; break;
					case Key.Left: left=state; break;
					case Key.Right: right=state; break;
					case Key.Down: down=state; break;
				}
			});
		}
	}

	//ボールフィールドを静的に配置するための何か
	class Win{public static Canvas field;}

	//メインクラス
	public partial class MainWindow: Window{
		List<IDraw> shps;
		public MainWindow(){
			InitializeComponent();

			Win.field=Field;
			shps=new List<IDraw>();
			shps.Add(new Paddle(new Vector(100,5),new Vector(Field.Width/2,Field.Height-50),"#9999FF",20));
			for(var i=0;i<7;i++){
				for(var n=0;n<5;n++){
					shps.Add(new Block(new Vector(80,30),new Vector(50+i*90,25+n*40),"#3399FF"));
				}
			}
			var ball=new Ball(shps,10,new Vector(Field.Width/2,300),90,10,"#FF00FF");

			//インターバル
			var timer=new DispatcherTimer();
			timer.Interval=TimeSpan.FromMilliseconds(30);
			timer.Tick+=(sender,e)=>{
				foreach(var v in shps){
					v.update();
				}
				ball.update();
			};
			timer.Start();
		}

		//キーイベント
		private void keyDown(object sender,KeyEventArgs e){CursorKey.keyDown(e);}
		private void keyUp(object sender,KeyEventArgs e){CursorKey.keyUp(e);}
	}
}
