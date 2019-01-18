const fetch = require('node-fetch');

const sceneid = `test`;
const FORGEURL = `https://developer.api.autodesk.com`;


class ForgeAPI {
    constructor(key, secret, bucket) {
        this.key = key;
		this.secret = secret;
		this.bucket = bucket;
		this.scope = `data:read data:write data:create bucket:read bucket:create`;
	}

	async getAccessToken () {
		const url = `${FORGEURL}/authentication/v1/authenticate`;
		const header = { 'Content-Type': 'application/x-www-form-urlencoded' }
		const payload = `grant_type=client_credentials&client_id=${this.key}&client_secret=${this.secret}&scope=${this.scope}`;
	    const res = await fetch( url, { method: 'POST', headers: header, body: payload });
	    this.token = await res.json();
		this.header = {
			'Accept': 'application/json',
			'Content-Type': 'application/json',
			'Authorization': `Bearer ${this.token.access_token}`,
		};
		return this.token;
	}

	async getBucketFiles(token = this.token.access_token) {
	    const url = `${FORGEURL}/oss/v2/buckets/${this.bucket}/objects`;
	    const res = await fetch( url, { headers: this.header });
		return res.json();
	}

	async processjob(urn, token = this.token.access_token) {
	    const url = `${FORGEURL}/modelderivative/v2/designdata/job`;
		const payload = {
		    "input": { "urn": urn },
		    "output": { "formats": [ { "type": "svf", "views": ["2d", "3d"] }]}
		};
	    const res = await fetch( url, { method: "POST", body: payload, headers: this.header });
		return res.json();
	}

	async jobstatus(urn, token = this.token.access_token) {
	    const url = `${FORGEURL}/modelderivative/v2/designdata/${urn}/manifest`;
	    const res = await fetch( url, { headers: this.header });
		return res.json();
	}

	async getThumbnail(urn, token = this.token.access_token) {
		const url = `${FORGEURL}/modelderivative/v2/designdata/${urn}/thumbnail?width=100&height=100`
	    const res = await fetch( url, {headers: { 'Authorization': `Bearer ${token}` } } );
	    return res.buffer();
	};

}

module.exports = {
    ForgeAPI
};