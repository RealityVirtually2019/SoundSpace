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
using System.Linq;
#if !UNITY_WSA
using System.Net;
using System.Security.Cryptography.X509Certificates;
#elif UNITY_WSA
using UnityEngine.Networking;
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SimpleJSON;


namespace Autodesk.Forge.ARKit {

	// Only works with the v2 server using the v1 API
	public class MaterialsRequest : RequestObjectInterface {

		#region Properties
		public HashSet<int> materialIds { get; set; }
		public Dictionary<int, Material> materials { get; set; }

		#endregion

		#region Constructors
		public MaterialsRequest (IForgeLoaderInterface _loader, Uri _uri, string _bearer, HashSet<int> _materialIds, JSONNode node) : base (_loader, _uri, _bearer) {
			resolved = SceneLoadingStatus.eMaterial;
			materialIds = _materialIds;
			lmvtkDef = node;
			materials = new Dictionary<int, Material> ();
		}

		#endregion

		#region Forge Request Object Interface
#if !UNITY_WSA
		public override void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null) {
			emitted = DateTime.Now;
			try {
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
				using ( client = new WebClient () ) {
					if ( callback != null )
						client.DownloadStringCompleted += new DownloadStringCompletedEventHandler (callback);
					if ( !string.IsNullOrEmpty (bearer) )
						client.Headers.Add ("Authorization", "Bearer " + bearer);
					client.Headers.Add ("Keep-Alive", "timeout=15, max=100");
					//if ( compression == true )
					//	client.Headers.Add ("Accept-Encoding", "gzip, deflate");
					state = SceneLoadingStatus.ePending;
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
			//using ( client =new UnityWebRequest (uri.AbsoluteUri) ) {
			System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
			using ( client =UnityWebRequest.Get (uri.AbsoluteUri) ) {
				//client.SetRequestHeader ("Connection", "keep-alive") ;
				//client.method =UnityWebRequest.kHttpVerbGET ;
				//if ( callback != null )
				//	client.DownloadStringCompleted +=new DownloadStringCompletedEventHandler (callback) ;
				if ( !string.IsNullOrEmpty (bearer) )
					client.SetRequestHeader ("Authorization", "Bearer " + bearer) ;
				//client.SetRequestHeader ("Keep-Alive", "timeout=15, max=100");
				//if ( compression == true )
				//	client.SetRequestHeader ("Accept-Encoding", "gzip, deflate");
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
						DownloadStringCompletedEventArgs args =new DownloadStringCompletedEventArgs (null, false, this) ;
						args.Result =client.downloadHandler.text ;
						callback (this, args) ;
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
			try {
				lmvtkDef = JSON.Parse (args.Result);
				state = SceneLoadingStatus.eReceived;
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			} finally {
			}
		}

		public override string GetName () {
			int key = materialIds.First ();
			return ("material-" + key.ToString ());
		}

		public string GetName (int id) {
			return ("material-" + id.ToString ());
		}

		public override GameObject BuildScene (string name, bool saveToDisk = false) {
			//foreach ( JSONNode matNode in lmvtkDef [0].AsObject ) {
			foreach ( KeyValuePair<string, JSONNode> matNode in lmvtkDef [0].AsObject ) {

				int matId = 0;
				if ( !Int32.TryParse (matNode.Key, out matId) )
					continue;

				Material material = CreateMaterial (matNode.Value, lmvtkDef [1] [matId]);
				materials.Add (matId, material);

				material.SetupMaterialWithBlendMode ((MaterialRequest.BlendMode)material.GetFloat ("_Mode"));
				material.SetMaterialKeywords (Texture.TextureType.Diffuse, false);

#if UNITY_EDITOR
				if ( saveToDisk ) {
					AssetDatabase.CreateAsset (material, ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + GetName (matId) + ".mat");
					//material =AssetDatabase.LoadAssetAtPath<Material> (ForgeConstants._resourcesPath + this.loader.PROJECTID + "/" + name + ".mat") ;
				}
#endif
			}

			base.BuildScene (name, saveToDisk);
			return (gameObject);
		}

		#endregion

		#region Simple Material
		public enum BlendMode {
			Opaque = MaterialRequest.BlendMode.Opaque,
			Cutout = MaterialRequest.BlendMode.Cutout,
			Fade = MaterialRequest.BlendMode.Fade,
			Transparent = MaterialRequest.BlendMode.Transparent
		}

		protected Material CreateMaterial (string jsonSt, string proteinSt) {
			StandardMaterial lmvMat = new StandardMaterial (jsonSt, proteinSt);
			return (CreateMaterial (lmvMat));
		}

		protected Material CreateMaterial (JSONNode json, JSONNode protein) {
			StandardMaterial lmvMat = new StandardMaterial (json, protein);
			return (CreateMaterial (lmvMat));
		}

		protected Material CreateMaterialV1 (StandardMaterial lmvMat) {
			// https://docs.unity3d.com/Manual/StandardShaderMetallicVsSpecular.html
			// Standard: The shader exposes a “metallic” value that states whether the material is metallic or not.
			// Standard (Specular setup): Choose this shader for the classic approach. 
			Material mat = new Material (
				lmvMat.isMetal == true ?
				  Shader.Find ("Standard")
				: Shader.Find ("Standard (Specular setup)")
			);

			try {
				if ( lmvMat.specular_tex != null ) {
					mat.EnableKeyword ("_SPECULARHIGHLIGHTS_OFF");
					mat.SetFloat ("_SpecularHighlights", 0f);
				}
				//mat.DisableKeyword ("_SPECULARHIGHLIGHTS_OFF") ;
				//mat.SetFloat ("_SpecularHighlights", 1f) ;
				mat.EnableKeyword ("_GLOSSYREFLECTIONS_OFF");
				mat.SetFloat ("_GlossyReflections", 0f);

				var ambiant = lmvMat.ambient;
				if ( ambiant != Color.clear )
					mat.SetColor ("_Color", ambiant);

				var diffuse = lmvMat.diffuse;
				if ( diffuse != Color.clear )
					mat.SetColor ("_Color", diffuse);

				var emissive = lmvMat.emissive;
				if ( emissive != Color.clear )
					mat.SetColor ("_EmissionColor", emissive);

				var specular = lmvMat.specular;
				if ( specular != Color.clear
					&& (
						   lmvMat.isMetal == true // In Unity3d, the texture would not show 
						&& lmvMat.diffuse_tex != null
						&& specular != Color.white
					)
				)
					mat.SetColor ("_SpecColor", specular);

				var transparent = lmvMat.transparent;
				if ( transparent ) {
					mat.SetFloat ("_Mode", (float)BlendMode.Transparent);
					mat.EnableKeyword ("_ALPHABLEND_ON");
					Color color = mat.GetColor ("_Color");
					color.a = lmvMat.transparency;
					mat.SetColor ("_Color", color);
				}

				// Create a new request to get the Textures
				if ( lmvMat.diffuse_tex != null ) {
					//TextureRequest req =new TextureRequest (loader, null, mat, Texture.TextureType.Diffuse, lmvMat.material) ;
					TextureRequest req = new TextureRequest (loader, null, bearer, mat, lmvMat.diffuse_tex, lmvMat.material);
					if ( fireRequestCallback != null )
						fireRequestCallback (this, req);
				}
				if ( lmvMat.specular_tex != null ) {
					TextureRequest req = new TextureRequest (loader, null, bearer, mat, lmvMat.specular_tex, lmvMat.material);
					if ( fireRequestCallback != null )
						fireRequestCallback (this, req);
				}
				if ( lmvMat.bump_tex != null ) {
					TextureRequest req = new TextureRequest (loader, null, bearer, mat, lmvMat.bump_tex, lmvMat.material);
					if ( fireRequestCallback != null )
						fireRequestCallback (this, req);
				}
			} catch ( System.Exception e ) {
				Debug.Log ("exception " + e.Message);
				mat = ForgeLoaderEngine.GetDefaultMaterial ();
			}
			return (mat);
		}

		protected Material CreateMaterial (StandardMaterial lmvMat) {
			// https://docs.unity3d.com/Manual/StandardShaderMetallicVsSpecular.html
			// Standard: The shader exposes a “metallic” value that states whether the material is metallic or not.
			// Standard (Specular setup): Choose this shader for the classic approach. 
			bool isMetal = lmvMat.isMetal;
			Material mat = new Material (isMetal ? Shader.Find ("Standard") : Shader.Find ("Standard (Specular setup)"));
			try {
				// Color properties
				Color color = Color.clear;
				Color diffuse = lmvMat.diffuse;
				Color ambient = lmvMat.ambient;
				Color specular = lmvMat.specular;

				color = diffuse != Color.clear ? diffuse : ambient;

				if ( color == Color.clear && isMetal )
					color = specular;

				mat.SetColor ("_Color", color);

				// Metallic properties
				if ( isMetal )
					mat.SetFloat ("_Metallic", 1);

				float glossiness = lmvMat.glossiness / 2000;
				mat.SetFloat ("_Glossiness", glossiness);
				mat.SetFloat ("_Shininess", glossiness);

				if ( specular != Color.white )
					mat.SetColor ("_SpecColor", color);

				var specularTexture = lmvMat.specular_tex;
				if ( specularTexture != null ) {
					mat.EnableKeyword ("_SPECULARHIGHLIGHTS_OFF");
					mat.SetFloat ("_SpecularHighlights", 0f);
					mat.EnableKeyword ("_GLOSSYREFLECTIONS_OFF");
					mat.SetFloat ("_GlossyReflections", 0f);
				} else {
					mat.DisableKeyword ("_SPECULARHIGHLIGHTS_OFF");
					mat.SetFloat ("_SpecularHighlights", 1f);
					mat.DisableKeyword ("_GLOSSYREFLECTIONS_OFF");
					mat.SetFloat ("_GlossyReflections", 1f);
				}

				// Emissive properties
				var emissive = lmvMat.emissive;
				if ( emissive != Color.clear )
					mat.SetColor ("_EmissionColor", emissive);

				//var specular = lmvMat.specular;
				//if ( specular != Color.clear
				//	&& (
				//		   lmvMat.isMetal == true // In Unity3d, the texture would not show 
				//		&& lmvMat.diffuse_tex != null
				//		&& specular != Color.white
				//	)
				//)
				//	mat.SetColor ("_SpecColor", specular);

				// Transparent properties
				var transparent = lmvMat.transparent;
				if ( transparent ) {
					mat.SetFloat ("_Mode", (float)BlendMode.Transparent);
					mat.EnableKeyword ("_ALPHABLEND_ON");
					color.a = lmvMat.transparency;
					mat.SetColor ("_Color", color);
				}

				// Create a new request to get the Textures
				if ( lmvMat.diffuse_tex != null ) {
					//TextureRequest req =new TextureRequest (loader, null, mat, Texture.TextureType.Diffuse, lmvMat.material) ;
					TextureRequest req = new TextureRequest (loader, null, bearer, mat, lmvMat.diffuse_tex, lmvMat.material);
					if ( fireRequestCallback != null )
						fireRequestCallback (this, req);
				}
				if ( lmvMat.specular_tex != null ) {
					TextureRequest req = new TextureRequest (loader, null, bearer, mat, lmvMat.specular_tex, lmvMat.material);
					if ( fireRequestCallback != null )
						fireRequestCallback (this, req);
				}
				if ( lmvMat.bump_tex != null ) {
					TextureRequest req = new TextureRequest (loader, null, bearer, mat, lmvMat.bump_tex, lmvMat.material);
					if ( fireRequestCallback != null )
						fireRequestCallback (this, req);
				}
			} catch ( System.Exception e ) {
				Debug.Log ("exception " + e.Message);
				mat = ForgeLoaderEngine.GetDefaultMaterial ();
			}
			return (mat);
		}


		#endregion

	}

}