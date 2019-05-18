/*******************************************************************************
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
	interface IDrow{
		public Shape shp{set;get;}
		public float left{get;}
		public float top{get;}
		public float right{get;}
		public float bottom{get;}
		public void update();
		public bool vsCircle(Vector center,float radius);
		public (float,float) hit(float rad,float speed);
	}

	//ボールクラス
	class Ball: IDrow{
		List<IDrow> shps;
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

		public float left{get{return (float)pos.X-radius;}}
		public float top{get{return (float)pos.Y-radius;}}
		public float right{get{return (float)pos.X+radius;}}
		public float bottom{get{return (float)pos.Y+radius;}}

		public Ball(List<IDrow> shps,int radius,Vector pos,float deg,float speed,string color){
			ready(shps,radius,pos,deg,speed,color);
		}

		void ready(List<IDrow> shps,int radius,Vector pos,float deg,float speed,string color){
			this.shps=shps;
			this.radius=radius;
			this.pos=pos;
			pos0=pos;
			this.speed=speed;
			speed0=speed;
			this.deg=deg;
			this.rad=(float)(deg/180*Math.PI);
			this.color=color;

			ball=new Ellipse();
			Win.field.Children.Add(ball);
			ball.Width=radius*2;
			ball.Height=radius*2;
			ball.Fill=new SolidColorBrush((Color)ConvertFromString(color));
			shp=(Shape)ball;
			drow();
			isStart=Task.Delay(1000);
		}

		public void update(){
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
				ready(shps,radius,pos0,deg,speed0,color);
				return;
			}

			//パドルとブロックの当たり判定
			for(var i=shps.Count-1;0<=i;i--){
				if(shps[i].vsCircle(pos,radius)){
					(rad,speed)=shps[i].hit(rad,speed);
					if(shps[i].GetType()==typeof(Paddle)) continue;
					Win.field.Children.Remove(shps[i].shp);
					shps.RemoveAt(i);
				}
			}
			rad%=(float)(2*Math.PI);
			drow();
		}

		public void drow(){
			Canvas.SetLeft(ball,left);
			Canvas.SetTop(ball,top);
		}

		public bool vsCircle(Vector center,float radius){return false;}
		public (float,float) hit(float rad,float speed){return (0,0);}
	}

	//ブロッククラス
	class Block: IDrow{
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
			drow();
		}
		public virtual void drow(){
			Canvas.SetLeft(block,left);
			Canvas.SetTop(block,top);
		}

		//上下左右の線のセット
		Vector[][] lines{get{return new []{
			new[]{new Vector(left,top),new Vector(right,top)},
			new[]{new Vector(right,top),new Vector(right,bottom)},
			new[]{new Vector(right,bottom),new Vector(left,bottom)},
			new[]{new Vector(left,bottom),new Vector(left,top)}
		};}}

		//当たり判定
		public bool vsCircle(Vector center,float radius){
			foreach(var v in lines){
				if(Block.lineVsCircle(v,center,radius)){
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
		static bool lineVsCircle(Vector[] p,Vector center,float radius){
			Vector lineDir=(p[1]-p[0]);				   // パドルの方向ベクトル
			Vector n = new Vector(lineDir.Y, -lineDir.X); // パドルの法線
			n.Normalize();

			Vector dir1=center-p[0];
			Vector dir2=center-p[1];

			double dist=Math.Abs(dotProduct(dir1,n));
			double a1=dotProduct(dir1,lineDir);
			double a2=dotProduct(dir2,lineDir);

			return (a1*a2<0 && dist<radius)? true: false;
		}

		//当たると反射する
		public virtual (float,float) hit(float rad,float speed){
			var line=(float)(1+Math.Cos(Math.Atan2(
				lastHit[1].Y-lastHit[0].Y,
				lastHit[1].X-lastHit[0].X
			)));
			return ((float)(line*Math.PI-rad),speed);
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

			drow();
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
		List<IDrow> shps;
		public MainWindow(){
			InitializeComponent();

			Win.field=Field;
			shps=new List<IDrow>();
			shps.Add(new Paddle(new Vector(100,5),new Vector(Field.Width/2,this.Height-50),"#99F",20));
			for(var i=0;i<7;i++){
				for(var n=0;n<5;n++){
					shps.Add(new Block(new Vector(80,40),new Vector(90+i*90,50+n*50),"#39F"));
				}
			}
			var ball=new Ball(shps,10,new Vector(Field.Width/2,300),90,10,"#F0F");

			//インターバル
			var timer=new DispatcherTimer();
			timer.Interval=TimeSpan.FromMilliseconds(33);
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
