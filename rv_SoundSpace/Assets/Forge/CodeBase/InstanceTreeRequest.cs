﻿//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
#if !UNITY_WSA
using System.Net;
#elif UNITY_WSA
using UnityEngine.Networking;
#endif
using UnityEngine;
using SimpleJSON;

namespace Autodesk.Forge.ARKit {

	public class InstanceTreeData {
		#region Fields
		public int dbId { get; set; }
		public GameObject obj { get; set; }
		// fragId, materialId, gameObject, jsonNode
		public List<Eppy.Tuple<int, int, GameObject, JSONNode>> fragments { get; set; }

		#endregion

		public InstanceTreeData (int _dbId, GameObject _obj) {
			dbId = _dbId;
			obj = _obj;
			fragments = new List<Eppy.Tuple<int, int, GameObject, JSONNode>> ();
		}

	}

	public class InstanceTreeRequest : RequestObjectInterface {

		#region Fields
		private Dictionary<int, InstanceTreeData> _components = new Dictionary<int, InstanceTreeData> ();

		#endregion

		#region Constructors
		public InstanceTreeRequest (IForgeLoaderInterface _loader, Uri _uri, string _bearer) : base (_loader, _uri, _bearer) {
			resolved = SceneLoadingStatus.eInstanceTree;
			compression = true;
		}

		#endregion

		#region Forge Request Object Interface
#if !UNITY_WSA
		public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null) {
			emitted = DateTime.Now;
			try {
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
				using ( client = new WebClient () ) {
					if ( callback != null ) {
						if ( compression == true )
							client.DownloadDataCompleted += new DownloadDataCompletedEventHandler (callback);
						else
							client.DownloadStringCompleted += new DownloadStringCompletedEventHandler (callback);
					}
					if ( !string.IsNullOrEmpty (bearer) )
						client.Headers.Add ("Authorization", "Bearer " + bearer);
					client.Headers.Add ("Keep-Alive", "timeout=15, max=100");
					if ( compression == true )
						client.Headers.Add ("Accept-Encoding", "gzip, deflate");
					state = SceneLoadingStatus.ePending;
					if ( compression == true )
						client.DownloadDataAsync (uri, this);
					else
						client.DownloadStringAsync (uri, this);
				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			}
		}
#elif UNITY_WSA
		public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted = DateTime.Now;
			mb.StartCoroutine (_FireRequest_ (callback)) ;
		}

		public override IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) {
			System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
			//using ( client =new UnityWebRequest (uri.AbsoluteUri) ) {
			using ( client =UnityWebRequest.Get (uri.AbsoluteUri) ) {
				//client.SetRequestHeader ("Connection", "keep-alive") ;
				//client.method =UnityWebRequest.kHttpVerbGET ;
				//if ( callback != null )
				//	client.DownloadStringCompleted +=new DownloadStringCompletedEventHandler (callback) ;
				if ( !string.IsNullOrEmpty (bearer) )
					client.SetRequestHeader ("Authorization", "Bearer " + bearer) ;
				//client.SetRequestHeader ("Keep-Alive", "timeout=15, max=100");
				if ( compression == true )
					client.SetRequestHeader ("Accept-Encoding", "gzip, deflate");
				state =SceneLoadingStatus.ePending ;
				//client.DownloadStringAsync (uri, this) ;
#if UNITY_2017_2_OR_NEWER
				yield return client.SendWebRequest () ;
#else
				yield return client.Send () ;
#endif

				if ( client.isNetworkError || client.isHttpError ) {
					Debug.Log (ForgeLoader.GetCurrentMethod () + " " + client.error + " - " + client.responseCode) ;
					state =SceneLoadingStatus.eError ;
				} else {
					//client.downloadHandler.data
					//client.downloadHandler.text
					if ( callback != null ) {
						if ( compression == true ) {
							DownloadDataCompletedEventArgs args = new DownloadDataCompletedEventArgs (null, false, this);
							args.Result = client.downloadHandler.data;
							callback (this, args);
						} else {
							DownloadStringCompletedEventArgs args =new DownloadStringCompletedEventArgs (null, false, this) ;
							args.Result =client.downloadHandler.text ;
							callback (this, args) ;
						}
					}
				}
			}
		}
#endif

		//public override void CancelRequest () ;

		public override void ProcessResponse (AsyncCompletedEventArgs e) {
			//TimeSpan tm = DateTime.Now - emitted;
			//UnityEngine.Debug.Log ("Received: " + tm.TotalSeconds.ToString () + " / " + uri.ToString ());
			DownloadStringCompletedEventArgs args = e as DownloadStringCompletedEventArgs;
			string result = "";
			if ( args == null ) {
				DownloadDataCompletedEventArgs args2 = e as DownloadDataCompletedEventArgs;
				byte [] bytes = args2.Result;
				//WebHeaderCollection headerCollection = this.client.ResponseHeaders;
				//for ( int i = 0 ; i < headerCollection.Count ; i++ ) {
				//	if ( headerCollection.GetKey (i).ToLower () == "content-encoding" && headerCollection.Get (i).ToLower () == "gzip" ) {
				//		Debug.Log (headerCollection.GetKey (i));
				//	}
				//}
				byte [] b = RequestObjectInterface.Decompress (bytes);
				result = Encoding.UTF8.GetString (b);
			} else {
				result = args.Result;
			}
			try {
				lmvtkDef = JSON.Parse (result);
				if ( lmvtkDef == null )
					state = SceneLoadingStatus.eError;
				else
					state = SceneLoadingStatus.eReceived;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			} finally {
			}
		}

		public override string GetName () {
			return ("root");
		}

		public override GameObject BuildScene (string name, bool saveToDisk = false) {
			Clear ();
			GameObject pivot = null;
			try {
				gameObject = new GameObject (name);
				if ( lmvtkDef == null )
					return (null);
				ForgeProperties properties = gameObject.AddComponent<ForgeProperties> ();
				properties.Properties = lmvtkDef ["metadata"];

				foreach ( JSONNode child in lmvtkDef ["childs"].AsArray )
					IteratorNodes (child, gameObject);
				base.BuildScene (name, saveToDisk);

				pivot = ForgeLoaderEngine.SetupForSceneOrientationAndUnits (gameObject, properties.Properties);

				// Create as much Meshes requests we need to load chunks of 0.5Mb-0.7Mb pack files
				// 1-499 polys = factor of 25
				// >500 poly = factor of 17
				int compSize = 0;
				List<Eppy.Tuple<int, int>> components = new List<Eppy.Tuple<int, int>> ();
				List<Eppy.Tuple<int, int, GameObject, JSONNode>> fragments = new List<Eppy.Tuple<int, int, GameObject, JSONNode>> ();
				foreach ( KeyValuePair<int, InstanceTreeData> data in _components ) {
					//if ( data.Value.fragments.Count == 0 )
					//continue;
					foreach ( Eppy.Tuple<int, int, GameObject, JSONNode> frag in data.Value.fragments ) {
						JSONArray json = frag.Item4 ["fragments"].AsArray;
						JSONArray json2 = frag.Item4 ["fragPolys"].AsArray;
						int index = 0;
						for ( ; index < json.Count && json [index].AsInt != frag.Item1 ; index++ ) { }
						if ( index >= json2.Count )
							continue;
						compSize += json2 [index].AsInt;
						//Debug.Log (data.Key + "-" + frag.Item1 + " " + json2 [index].AsInt);
						components.Add (new Eppy.Tuple<int, int> (data.Key, frag.Item1));
						fragments.Add (frag);
					}

					if ( compSize > 29411 /* 29411 * 17 = 500kb */ ) {
						MeshesRequest reqMeshes = new MeshesRequest (loader, null, bearer, components, fragments, null);
						reqMeshes.gameObject = null;
						if ( fireRequestCallback != null )
							fireRequestCallback (this, reqMeshes);
						compSize = 0;
						components = new List<Eppy.Tuple<int, int>> ();
						fragments = new List<Eppy.Tuple<int, int, GameObject, JSONNode>> ();
					}
				}
				if ( components.Count > 0 ) {
					MeshesRequest reqMeshes = new MeshesRequest (loader, null, bearer, components, fragments, null);
					reqMeshes.gameObject = null;
					if ( fireRequestCallback != null )
						fireRequestCallback (this, reqMeshes);
				}

				if ( loader.GetMgr ()._materials.Count > 0 ) {
					MaterialsRequest reqMaterials = new MaterialsRequest (loader, null, bearer, loader.GetMgr ()._materials, lmvtkDef);
					reqMaterials.gameObject = null;
					if ( fireRequestCallback != null )
						fireRequestCallback (this, reqMaterials);
				} // Textures will be requested by the material request (one by one)

				// Now Properties
				int [] dbIds = _components.Keys.ToArray ();
				int [] [] chunks = dbIds
					.Select ((s, i) => new { Value = s, Index = i })
					.GroupBy (x => x.Index / 100)
					.Select (grp => grp.Select (x => x.Value).ToArray ())
					.ToArray ();
				for ( int i = 0 ; i < chunks.Length ; i++ ) {
					List<Eppy.Tuple<int, GameObject>> dbIdsTuple = new List<Eppy.Tuple<int, GameObject>> ();
					foreach ( int dbId in chunks [i] )
						dbIdsTuple.Add (new Eppy.Tuple<int, GameObject> (dbId, _components [dbId].obj));
					PropertiesRequest2 reqProps = new PropertiesRequest2 (loader, null, bearer, dbIdsTuple);
					reqProps.gameObject = null;
					if ( fireRequestCallback != null )
						fireRequestCallback (this, reqProps);
				}

				Clear ();
			} catch ( Exception /*ex*/ ) {
				if ( gameObject )
					GameObject.DestroyImmediate (gameObject);
				gameObject = null;
				pivot = null;
			}
			return (pivot);
		}

		#endregion

		#region Methods
		protected GameObject getNewRoot (GameObject root) {
			string name = root.name;
			if ( root.transform.Find (name + "-def") )
				return (root);
			root.name = name + "-def";
			GameObject newRoot = new GameObject (name);
			root.transform.parent = newRoot.transform;
			return (newRoot);
		}

		protected void Clear () {
			_components.Clear ();
		}

		#endregion

		#region Building Instance Tree
		protected void IteratorNodes (JSONNode node, GameObject go) {
			GameObject obj = null;
			int dbId = -1;
			switch ( node ["type"].Value ) {
				case "Transform":
					dbId = node ["id"].AsInt;
					string nodeName = buildName ("transform", dbId, 0, node ["pathid"]);
					obj = new GameObject (nodeName);
					setTransform (node, obj);
					obj.transform.parent = go.transform;

					// But requesting properties here does not really make sense and is slowing down the streaming.
					// Instead, we could get properties on demand or at the end.
					if ( !_components.ContainsKey (dbId) )
						_components.Add (dbId, new InstanceTreeData (dbId, obj));

					break;
				case "Mesh":
					dbId = node ["id"].AsInt;
					if ( !_components.ContainsKey (dbId) ) {
						string nodeName2 = buildName ("transform", dbId, 0, node ["pathid"]);
						obj = new GameObject (nodeName2);
						setTransform (node, obj);
						obj.transform.parent = go.transform;
						_components.Add (dbId, new InstanceTreeData (dbId, obj));
						go = obj;
					} else {
						go = _components [dbId].obj;
					}

					for ( int i = 0 ; i < node ["fragments"].AsArray.Count ; i++ ) {
						int fragId = node ["fragments"] [i].AsInt;
						int matId = node ["materials"] [i].AsInt;
						obj = CreateMeshObject (dbId, fragId, node ["pathid"]);
						setTransform (node ["fragTransforms"] [i].AsObject, obj);
						obj.transform.parent = go.transform;

						InstanceTreeData data = _components [dbId];
						data.fragments.Add (new Eppy.Tuple<int, int, GameObject, JSONNode> (fragId, matId, obj, node));

						loader.GetMgr ()._materials.Add (matId);
					}

					obj = null; // No childs!
					break;
				default:
					break;
			}
			if ( obj != null && node ["childs"] != null ) {
				try {
					foreach ( JSONNode child in node ["childs"].AsArray )
						IteratorNodes (child, obj);
				} catch ( System.Exception /*ex*/ ) {
				}
			}
		}

		protected static GameObject CreateMeshObject (int dbid, int fragId, string pathid) {
			string nodeName = buildName ("mesh", dbid, fragId, pathid);
			GameObject obj = new GameObject (nodeName);
			return (obj);
		}

		public static void setTransform (JSONNode node, GameObject obj) {
			//Matrix4x4 mat =Matrix4x4.identity ;
			if ( node ["mtype"] != null ) {
				switch ( node ["mtype"].Value ) {
					case "Identity":
						break;
					case "Translation":
						obj.transform.Translate (new Vector3 (node ["tr"] ["x"].AsFloat, node ["tr"] ["y"].AsFloat, node ["tr"] ["z"].AsFloat));
						break;
					case "RotationTranslation":
						obj.transform.Translate (new Vector3 (node ["tr"] ["x"].AsFloat, node ["tr"] ["y"].AsFloat, node ["tr"] ["z"].AsFloat));
						obj.transform.rotation = new Quaternion (
							node ["rt"] ["a"].AsFloat, node ["rt"] ["b"].AsFloat, node ["rt"] ["c"].AsFloat,
							node ["rt"] ["d"].AsFloat
						);
						break;
					case "UniformScaleRotationTranslation":
						obj.transform.Translate (new Vector3 (node ["tr"] ["x"].AsFloat, node ["tr"] ["y"].AsFloat, node ["tr"] ["z"].AsFloat));
						obj.transform.localScale = new Vector3 (node ["scale"].AsFloat, node ["scale"].AsFloat, node ["scale"].AsFloat);
						obj.transform.rotation = new Quaternion (
							node ["rt"] ["a"].AsFloat, node ["rt"] ["b"].AsFloat, node ["rt"] ["c"].AsFloat,
							node ["rt"] ["d"].AsFloat
						);
						break;
					case "AffineMatrix":
						Matrix4x4 mat = new Matrix4x4 ();
						mat.m00 = node ["mt"] ["m00"].AsFloat;
						mat.m01 = node ["mt"] ["m01"].AsFloat;
						mat.m02 = node ["mt"] ["m02"].AsFloat;
						mat.m03 = node ["mt"] ["m03"].AsFloat;
						mat.m10 = node ["mt"] ["m10"].AsFloat;
						mat.m11 = node ["mt"] ["m10"].AsFloat;
						mat.m12 = node ["mt"] ["m12"].AsFloat;
						mat.m13 = node ["mt"] ["m13"].AsFloat;
						mat.m20 = node ["mt"] ["m20"].AsFloat;
						mat.m21 = node ["mt"] ["m21"].AsFloat;
						mat.m22 = node ["mt"] ["m22"].AsFloat;
						mat.m23 = node ["mt"] ["m23"].AsFloat;
						mat.m30 = node ["mt"] ["m30"].AsFloat;
						mat.m31 = node ["mt"] ["m31"].AsFloat;
						mat.m32 = node ["mt"] ["m32"].AsFloat;
						mat.m33 = node ["mt"] ["m33"].AsFloat;
						obj.transform.localScale = ScaleFromMatrix (mat);
						obj.transform.rotation = RotationFromMatrix (mat);
						obj.transform.position = TranslationFromMatrix (mat);
						break;
				}
			}
		}

		protected static Vector3 TranslationFromMatrix (Matrix4x4 matrix) {
			Vector3 translate;
			translate.x = matrix.m03;
			translate.y = matrix.m13;
			translate.z = matrix.m23;
			return (translate);
		}

		protected static Quaternion RotationFromMatrix (Matrix4x4 matrix) {
			Vector3 forward;
			forward.x = matrix.m02;
			forward.y = matrix.m12;
			forward.z = matrix.m22;
			Vector3 upwards;
			upwards.x = matrix.m01;
			upwards.y = matrix.m11;
			upwards.z = matrix.m21;
			return (Quaternion.LookRotation (forward, upwards));
		}

		protected static Vector3 ScaleFromMatrix (Matrix4x4 matrix) {
			Vector3 scale = new Vector3 (
				matrix.GetColumn (0).magnitude,
				matrix.GetColumn (1).magnitude,
				matrix.GetColumn (2).magnitude
			);
			if ( Vector3.Cross (matrix.GetColumn (0), matrix.GetColumn (1)).normalized != (Vector3)matrix.GetColumn (2).normalized )
				scale.x *= -1;
			return (scale);
		}

		#endregion

	}

}
