using UnityEngine;
using System.Collections;

 public class PermanentData : MonoBehaviour
 {
     private static PermanentData _instance;
 
     void Awake()
     {
         if (!_instance)
             _instance = this;
         else
             Destroy(this.gameObject);
 
 		// GetComponent<GameManager>().DeadCanvas = GameObject.Find("DeadUI").GetComponent<Canvas>();
        DontDestroyOnLoad(this.gameObject);
     }
 }