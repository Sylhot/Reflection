using System.Collections.Generic;
using UnityEngine;

public class MirrorReflection : MonoBehaviour
{
    public GameObject SceneObj; // Sahne nesneleri
    public List<GameObject> objectsToFlip; // Yansıtılacak nesneler
    public List<GameObject> nowFlipedObjects; // Yansıtılan nesneler
    private Dictionary<GameObject, GameObject> pairs = new Dictionary<GameObject, GameObject>();
    public Transform mirrorPlane; // Ayna düzlemi
    public float mirrorPointX; // Ayna noktası X
    public float xMovementRatios; // Yansıma ofseti
    public float moveReflectObjY; // Yansıtılan objenin y ye göre hareketi

    [Header("Yansıma Ayarları")]
    public float xInversionScale = 1f; // X ekseninde ters çevirme oranı
    public Vector3 rotationOption1 = new Vector3(30, 20, 65); // Rotasyon seçenek 1
    public Vector3 rotationOption2 = new Vector3(-30, -20, -65); // Rotasyon seçenek 2
    public bool useRotationOption1; // Hangi rotasyonu kullanacak (true = Option1, false = Option2)
    
    [Header("Ayna Scale Ayarları")]
    public float minScale = 0.5f; // En küçük scale oranı (uzakken)
    public float maxScale = 1.5f; // En büyük scale oranı (yakınken)
    public float scaleTransitionSpeed = 2f; // Scale değişim hızı

    void Start()
    {
        objectsToFlip.Clear();
        foreach (Transform child in SceneObj.transform)
        {
            if (child != null)//&& child.CompareTag("MirrorReflectable"))
            {
                objectsToFlip.Add(child.gameObject);
            }
        }
        foreach (GameObject obj in objectsToFlip)
        {
            if (obj != null && mirrorPlane != null)
            {
                Vector3 objPosition = obj.transform.position;
                float distanceFromMirror = objPosition.x - mirrorPointX;
                float reflectedX = mirrorPointX - (distanceFromMirror * xInversionScale * xMovementRatios);
                Vector3 reflectedPosition = new Vector3(reflectedX, objPosition.y - moveReflectObjY, objPosition.z);

                // Yönü yansıt
                Vector3 objScale = UpdateReflectedObjectScale(obj.transform.localScale, objPosition); //Ölçek güncelle
                objScale.x = -Mathf.Abs(objScale.x); // X eksenini ters çevir
                Quaternion reflectRotation = UpdateReflectedObjectRotation();
                GameObject reflectedObject = Instantiate(obj, reflectedPosition, reflectRotation);
                Destroy(reflectedObject.GetComponent<GuardAI>()); // Kendi scriptini kaldır
                reflectedObject.transform.localScale = objScale;

                pairs[obj] = reflectedObject;
                nowFlipedObjects.Add(reflectedObject);
            }
        }
    }
    void Update()
    {
        ObjectRemoveList();
        
        foreach (GameObject obj in objectsToFlip)
        {
            if (obj != null && mirrorPlane != null && pairs.ContainsKey(obj))
            {
                GameObject thisObjCopy = pairs[obj];
                if (thisObjCopy == null) continue;

                Vector3 objPosition = obj.transform.position;
                float distanceFromMirror = objPosition.x - mirrorPointX;
                float reflectedX = mirrorPointX - (distanceFromMirror * xInversionScale * xMovementRatios);
                Vector3 reflectedPosition = new Vector3(reflectedX, objPosition.y - moveReflectObjY, objPosition.z);
                thisObjCopy.transform.position = reflectedPosition;

                // Yönü yansıt
                Vector3 objScale = UpdateReflectedObjectScale(obj.transform.localScale, objPosition); //Ölçek güncelle
                objScale.x = -Mathf.Abs(objScale.x); // X eksenini ters çevir
                thisObjCopy.transform.localScale = objScale;
                thisObjCopy.transform.rotation = UpdateReflectedObjectRotation();// Rotasyonu güncelle
            }
        }
    }
    private Quaternion UpdateReflectedObjectRotation()
    {
        Vector3 selectedRotation = useRotationOption1 ? rotationOption1 : rotationOption2;
        return Quaternion.Euler(selectedRotation);
    }
    private Vector3 UpdateReflectedObjectScale(Vector3 objScale, Vector3 objPosition)
    {
        // Sahnedeki en uzak nesneyi bul
        float maxDistance = 0f;
        foreach (GameObject obj in objectsToFlip)
        {
            if (obj != null)
            {
                float distance = Mathf.Abs(obj.transform.position.x - mirrorPointX);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
        }
        
        // Eğer hiç nesne yoksa varsayılan değer
        if (maxDistance == 0f) maxDistance = 10f;
        
        // Orijinal nesnenin aynaya olan uzaklığını hesapla
        float distanceFromMirror = Mathf.Abs(objPosition.x - mirrorPointX);
        
        // Mesafeye göre normalize et (0 = yakın, 1 = uzak)
        float normalizedDistance = distanceFromMirror / maxDistance;
        normalizedDistance = Mathf.Clamp01(normalizedDistance);
        
        // Yakınken büyük, uzakken küçük scale
        float scaleMultiplier = Mathf.Lerp(maxScale, minScale, normalizedDistance);
        
        objScale.x = scaleMultiplier * objScale.x;
        objScale.y = scaleMultiplier * objScale.y;
        return objScale;
    }
    public void ObjectAddList(GameObject obj)
    {
        if (obj != null && !objectsToFlip.Contains(obj))
        {
            objectsToFlip.Add(obj);
            GameObject reflectedObject = Instantiate(obj, obj.transform.position, obj.transform.rotation);
            Destroy(reflectedObject.GetComponent<BulletMovement>()); // Kendi scriptini kaldır
            pairs[obj] = reflectedObject;
            nowFlipedObjects.Add(reflectedObject);
        }
    }
    public void ObjectRemoveList()
    {
        if (objectsToFlip == null) return;
        objectsToFlip.RemoveAll(x => x == null);
        if (nowFlipedObjects == null) return;
        nowFlipedObjects.RemoveAll(x => x == null);
        
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var pair in pairs)
        {
            if (pair.Key == null) // Orijinal nesne ölmüş
            {
                if (pair.Value != null) // Yansıması hala varsa
                    Destroy(pair.Value); // Yansımasını da yok et
                toRemove.Add(pair.Key);
            }
            else if (pair.Value == null) // Sadece yansıma ölmüş
            {
                toRemove.Add(pair.Key);
            }
        }
        foreach (var key in toRemove)
            pairs.Remove(key);
    }
}