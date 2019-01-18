'use strict'
const express = require('express')
const app = express();
const { ForgeAPI } = require('./api/forgeapi')
const { Toolkit } = require('./api/toolkit')

const atob = (data) => Buffer.from(data).toString('base64');

const auth = new ForgeAPI( 
	process.env.FORGE_CLIENT_ID, 
	process.env.FORGE_CLIENT_SECRET, 
	process.env.FORGE_BUCKET);


app.use(function(req, res, next) {
  res.header("Access-Control-Allow-Origin", "*");
  res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
  next();
});

var token = "";

//////////////////
// List bucket files, access-token, thumbnail as js file
// made cached for quicker websites (and easy for local testing to avoid CORS issues)
// or... use the other more tradional two endpoints.
//////////////////
app.get('/api/_adsk.js', async (req, res) => {
	token = await auth.getAccessToken();
	const files = (await auth.getBucketFiles()).items.map( i => {
		const safeurn = atob(i.objectId).split("=")[0];
		const thumb = `thumbs/${i.objectKey}.png`;
		return {objectId:i.objectId, objectKey:i.objectKey, size:i.size, urn:`${safeurn}`, thumb:thumb };
	});
	//res.setHeader("Expires", new Date(Date.now() + (60 * token.expires_in) ).toUTCString());
	res.setHeader("Cache-Control", `public, max-age=${token.expires_in}`); // js file is cached for about 15 mins
	res.send( `var _adsk = ${JSON.stringify( {token:token, files: files })}` );
});



//////////////////
// provide access-token
//////////////////
app.get('/api/token', async (req, res) => {
	res.json( await auth.getAccessToken() );
});

//////////////////
// List files from bucket
//////////////////
app.get('/api/files', async (req, res) => {
	//res.json( (await auth.getBucketFiles()).items );
	res.json( (await auth.getBucketFiles()).items.map( i => {
		const urn = atob(i.objectId).split("=")[0];
		const thumb = `thumbs/${i.objectKey}.png`;
		return {objectId:i.objectId, objectKey:i.objectKey, size:i.size, urn:'urn:'+urn, thumb:thumb };
	}));
});


//////////////////
app.get('/api/createscene', async (req, res) => {
	if (!token)
		token = req.query.token;
	const tk = new Toolkit(token, process.env.FORGE_SCENE);
	const status1 = await tk.createScene(req.query.urn, req.query.scene);
	const status2 = await tk.processSVF(req.query.urn, req.query.scene);
	res.json(status2);
});


//////////////////
app.get('/api/status', async (req, res) => {
	if (!token)
		token = req.query.token;
	const tk = new Toolkit(token, process.env.FORGE_SCENE);
	const rst = await tk.jobStatus(req.query.urn);
	res.json( rst );
});


//////////////////
app.get('/api/upload', async (req, res) => {
	res.json( await auth.getAccessToken() );
});



app.use(express.static(__dirname + '/../docs'));

//////////////////
const port = process.env.PORT || 8080;
// <-- uncomment below line for local debugging, then type: >node server.js
app.listen(port, () => { console.log(`Server listening on port ${port}`); });
module.exports = app
