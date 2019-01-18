const fetch = require('node-fetch');

const sceneid = `test`;
const ARKITURL = `https://developer-api-beta.autodesk.io`;


class Toolkit {
    constructor(token) {
        this.token = token.access_token;
		this.header = {
			'Accept': 'application/json',
			'Content-Type': 'application/json',
			'Authorization': `Bearer ${this.token}`,
		};
	}


	async createScene(urn, sceneId) {
	    const url = `${ARKITURL}/arkit/v1/${urn}/scenes/${sceneId}`
		const payload = { "prj": { "urn": urn }}
	    const res = await fetch( url , { method: 'PUT', headers: this.header, body: JSON.stringify(payload) });
		return res.json();
	}


	async processSVF(urn, sceneId) {
	    const url = `${ARKITURL}/modelderivative/v2/arkit/job`;
	    const payload = {
			"input": { "urn": urn },
			"output": {
			  "formats": [{
				  "type": "arkit",
				  "scene": sceneId,
				}]
			}
		}
	    const res = await fetch( url , { method: 'POST', headers: this.header, body: JSON.stringify(payload) });
	    const json = await res.json();
	    console.log(JSON.stringify(json));
	    return json;
	}

	async jobStatus(urn) {
	    const url = `${ARKITURL}/modelderivative/v2/arkit/${urn}/manifest`
		const res = await fetch( url , { headers: this.header });
		return res.json();
	}

}

module.exports = {
    Toolkit
};