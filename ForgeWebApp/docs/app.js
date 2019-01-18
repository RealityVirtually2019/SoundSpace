var viewer, svf;
var markup;
var ServerURL = 'http://localhost:8080';
// var ServerURL = 'https://xf79h9aa3l.execute-api.us-west-2.amazonaws.com/toolkit2';
var ThumbURL = 'https://developer.api.autodesk.com/modelderivative/v2/designdata';

// Vue.js components
window.app = new Vue({
    el: "#app",

    data: {
        form: {urn: null, token: null, scene: null, title:"Copy URN, Token & SceneID"},
        istoast: false,
        toastmsg: "na",
        Items: []
    },
    methods: {

        init: function() {
            this.Items = _adsk.files;
            this.form.scene = 'test';
        },

        loadModel: function(urn, title) {
            this.form.title = title;
            this.form.urn = urn;
            this.form.token = _adsk.token.access_token;
            options = {
                useADP: false,
                env: "AutodeskProduction",
                accessToken: _adsk.token.access_token,
                isAEC: true
            };
            viewer = new Autodesk.Viewing.Private.GuiViewer3D(document.getElementById('forgeViewer'));
            Autodesk.Viewing.Initializer(options);
            Autodesk.Viewing.Document.load(`urn:${urn}`, (doc) =>{
                var geometries = doc.getRoot().search({ 'type': 'geometry', 'role': '3d' });
                svf = doc.getViewablePath(geometries[0]);
                viewer.start(svf, { sharedPropertyDbPath: doc.getPropertyDbPath() });
            });
        },

        uploadFile: function() {
            this.Items.push({
                objectKey: 'new item',
                status: 'uploading'
            });
        },

        showtoast: function(msg) {
            console.log(msg);
            this.istoast = true;
            this.toastmsg = msg;
            setTimeout(function(){ app.istoast=false; }, 3000);
        },

        pollProgress: async function(urn) {
            const url = `${ServerURL}/api/status?urn=${urn}&token=${_adsk.token.access_token}`;
            const res = await fetch( url );
            return res.json();
        },

        createScene: function(item) {
            return; //disable this for now.
            fetch(`${ServerURL}/api/createscene?urn=${item.urn}&scene=${this.form.scene}&token=${_adsk.token.access_token}`).then((res) => res.json()).then((json) => {
                app.showtoast('processing toolkit scene...');
                const timer = setInterval( async () =>  {
                    const res = await this.pollProgress(item.urn);
                    if (res.derivatives[1].children[1])
                        app.showtoast(`...processing: ${res.derivatives[1].children[1].complete}`);
                    if (res.derivatives[1].progress != "inprogress") {
                        app.showtoast('...finished processing file');
                        clearInterval(timer);
                        console.log(res);
                    }
                },5000)
            });
        },

        setImgSrc: async function(el, urn) {
            const header = {
                method: 'GET',
                mode: 'cors',
                headers: { 'Authorization': `Bearer ${_adsk.token.access_token}` },
            };
            const url = `${ThumbURL}/${urn}/thumbnail?width=100&height=100`
            const res = await fetch( url, header );
            const blb = await res.blob();
            el.src = URL.createObjectURL(blb);
        }
    },
    directives: {
        'auth-image': {
      bind: function(el, binding) {
        app.setImgSrc(el, binding.value);
      },
        }
    }
})

app.init();
devicePixelRatio = 1.25;