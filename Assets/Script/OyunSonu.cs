using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class OyunSonu : MonoBehaviour
{
    public GameObject Panel;
    public string nextLevelName; // Yüklenecek sonraki sahnenin adı

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Panel.SetActive(true);
        }
    }
}
