using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyMaterials : MonoBehaviour
{
    // Start is called before the first frame update
    #region Parameters

    // 重複登録防止のためのルートフラグ
    [System.NonSerialized] public bool IsRoot = true;

    #endregion

    void Start()
    {
        if (IsRoot)
        {
            // 配下のレンダラすべてに追加
            foreach (var r in this.GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.GetComponent<AutoDestroyMaterials>() == null) 
                {
                    r.gameObject.AddComponent<AutoDestroyMaterials>().IsRoot = false;
                }
                
            }
            // 配下のパーティクルシステムすべてに追加
            foreach (var p in this.GetComponentsInChildren<ParticleSystem>())
            {
                if (p.gameObject.GetComponent<AutoDestroyMaterials>() == null)
                {
                    p.gameObject.AddComponent<AutoDestroyMaterials>().IsRoot = false;
                }
                
            }
        }
    }

    void OnDestroy()
    {
        // レンダラのマテリアルを破棄(パーティクルシステムのレンダラも含まれる)
        var thisRenderer = this.GetComponent<Renderer>();
        if (thisRenderer != null && thisRenderer.materials != null)
        {
            foreach (var m in thisRenderer.sharedMaterials)
            {
                Destroy(m);
            }
            foreach (var m in thisRenderer.materials)
            {
                Destroy(m);
            }
        }
    }
}
