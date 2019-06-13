"use strict";

const BBUtil={
	//ベクトル計算クラス 
	Vector: class Vector{
		constructor(X,Y){
			this.X=X;
			this.Y=Y;
		}
		//ベクトル加算
		add(vec){
			return new Vector(this.X+vec.X,this.Y+vec.Y);
		}
		addSet(vec){
			this.X+=vec.X;
			this.Y+=vec.Y;
		}
		//ベクトル減算
		sub(vec){
			return new Vector(this.X-vec.X,this.Y-vec.Y);
		}
		subSet(vec){
			this.X-=vec.X;
			this.Y-=vec.Y;
		}
		//ないせき
		dot(vec){
			return this.X*vec.X+this.Y*vec.Y;
		}
		//ベクトルの正規化
		Normalize(){
			var np=Math.sqrt(this.X**2+this.Y**2);
			this.X/=np;
			this.Y/=np;
		}
		//ベクトルのコピー
		clone(){
			return Object.create(this);
		}
		//ベクトルの文字列化
		toString(){
			return `Vector(${this.X}, ${self.Y})`
		}
	},

	//キーボードの状態を管理するやつ
	CursorKey: (()=>{
		var left=false;
		var up=false;
		var right=false;
		var down=false;

		class CursorKey{
			static get left(){return left;}
			static get up(){return lup}
			static get right(){return right;}
			static get down(){return down;}

			static keyDown=keyState(true);
			static keyUp=keyState(false);
		}

		function keyState(state){
			return e=>{
				switch(e.key){
					case "ArrowLeft": left=state; break;
					case "ArrowUp": up=state; break;
					case "ArrowRight": right=state; break;
					case "ArrowDown": down=state; break;
				}
			};
		}
		return CursorKey;
	})()
}