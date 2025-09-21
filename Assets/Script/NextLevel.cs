using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    public GameObject level1;
    public GameObject level2;
    public string nextLevelName; // Yüklenecek sonraki sahnenin adı

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            level1.SetActive(false);
            level2.SetActive(true);
        }
    }
}
