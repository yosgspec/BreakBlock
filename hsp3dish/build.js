const nwbuild=require("nw-builder");
const nw=new nwbuild({
	files:"./app/**",
	platforms:["win32"],
	flavor:"normal"
});
nw.on("log",console.log);
nw.build()
	.then(()=>console.log("done!"))
	.catch(e=>console.error(e));