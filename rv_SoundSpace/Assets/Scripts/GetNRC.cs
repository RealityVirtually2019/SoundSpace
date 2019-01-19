using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Autodesk.Forge.ARKit;
using SimpleJSON;

public static class GetNRC
{
    public static float getNRCFromCollider(Collider collider)
    {

        float result = 0.1f;
        ForgeProperties fp = collider.gameObject.GetComponentInParent<ForgeProperties>();
                
        if (fp != null)
        {
            JSONNode temp = fp.Properties["props"];
            Debug.Log("testing");
            foreach (var v in temp.Values)
            {
                if (v["name"] == "Absorptance")
                {
                    string val = v["value"];
                    float tryResult;
                    if (float.TryParse(val, out tryResult))
                        result = tryResult;
                    
                }
            }
        }

        return result;
    }
}
