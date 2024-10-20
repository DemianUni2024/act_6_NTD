using System.Collections;
using UnityEngine;
using Vuforia;

public class multitargetManager : MonoBehaviour
{
    public GameObject[] animals; // Animales que deben activarse o desactivarse
    private ObserverBehaviour mObserverBehaviour;

    void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();

        // Desactivar todos los animales al inicio
        DeactivateAnimals();

        if (mObserverBehaviour)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnDestroy()
    {
        if (mObserverBehaviour)
        {
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Desactivar todos los animales primero
        DeactivateAnimals();

        // Solo si el cubo es detectado
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED || targetStatus.Status == Status.LIMITED)
        {
            // Aquí determina qué cara del cubo está visible
            int visibleFaceIndex = GetVisibleFaceIndex();

            // Activa solo el animal correspondiente a la cara visible
            if (visibleFaceIndex >= 0 && visibleFaceIndex < animals.Length)
            {
                animals[visibleFaceIndex].SetActive(true);
                StartCoroutine(AnimateAnimal(animals[visibleFaceIndex])); // Iniciar la animación
            }
        }
    }

    private void DeactivateAnimals()
    {
        foreach (GameObject animal in animals)
        {
            animal.SetActive(false); // Desactiva todos los animales
        }
    }

    // Método para determinar la cara más visible del cubo
    private int GetVisibleFaceIndex()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cubePosition = transform.position;

        // Obtener la dirección desde la cámara hacia el cubo
        Vector3 directionToCube = cubePosition - cameraPosition;
        directionToCube.Normalize();

        // Definir las direcciones de las caras del cubo
        Vector3[] faceDirections = new Vector3[6];
        faceDirections[0] = transform.right;     // Cara derecha
        faceDirections[1] = -transform.right;    // Cara izquierda
        faceDirections[2] = transform.up;        // Cara arriba
        faceDirections[3] = -transform.up;       // Cara abajo
        faceDirections[4] = transform.forward;   // Cara delante
        faceDirections[5] = -transform.forward;  // Cara detrás

        float maxDot = -1f; // Para encontrar el ángulo más pequeño
        int visibleFaceIndex = -1;

        // Calcular el dot product para determinar la cara más visible
        for (int i = 0; i < faceDirections.Length; i++)
        {
            float dot = Vector3.Dot(directionToCube, faceDirections[i]);

            if (dot > maxDot)
            {
                maxDot = dot;
                visibleFaceIndex = i;
            }
        }

        return visibleFaceIndex; // Devuelve el índice de la cara más visible
    }

    private IEnumerator AnimateAnimal(GameObject animal)
    {
        // Reproduce el audio (asegúrate de tener un AudioSource en el objeto)
        AudioSource audioSource = animal.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }

        Vector3 originalPosition = animal.transform.localPosition;

        // Elegir una animación aleatoria
        int animationType = Random.Range(0, 3); // 0: vibrar, 1: subir y bajar, 2: girar

        switch (animationType)
        {
            case 0:
                yield return StartCoroutine(VibrateAnimal(animal, originalPosition));
                break;
            case 1:
                yield return StartCoroutine(BounceAnimal(animal, originalPosition));
                break;
            case 2:
                yield return StartCoroutine(RotateAnimal(animal));
                break;
        }

        // Asegúrate de que vuelva a la posición original después de la animación
        animal.transform.localPosition = originalPosition;
    }

    private IEnumerator VibrateAnimal(GameObject animal, Vector3 originalPosition)
    {
        for (int i = 0; i < 10; i++)
        {
            animal.transform.localPosition = originalPosition + Random.insideUnitSphere * 0.1f; // Vibrar ligeramente
            yield return new WaitForSeconds(0.1f);
        }
        animal.transform.localPosition = originalPosition; // Restablecer la posición
    }

    private IEnumerator BounceAnimal(GameObject animal, Vector3 originalPosition)
    {
        for (float t = 0; t < 1; t += Time.deltaTime * 0.5f) // Animación de subir y bajar
        {
            animal.transform.localPosition = new Vector3(originalPosition.x, originalPosition.y + Mathf.Sin(t * Mathf.PI) * 0.05f, originalPosition.z);
            yield return null;
        }
    }

    private IEnumerator RotateAnimal(GameObject animal)
    {
        for (float t = 0; t < 1; t += Time.deltaTime * 0.5f) // Animación de giro
        {
            animal.transform.Rotate(0, 30 * Time.deltaTime, 0); // Gira 30 grados por segundo
            yield return null;
        }
    }
}
