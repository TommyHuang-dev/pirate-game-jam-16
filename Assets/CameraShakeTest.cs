using CameraShake;
using System.Collections;
using UnityEngine;

public class CameraShakeTest : MonoBehaviour
{
    [SerializeField] BounceShake.Params shakeParams;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         StartCoroutine(ShakeCam());
    }

    private IEnumerator ShakeCam() {
        int maxShakes = 100;
        int shakes = 0;
        while (shakes < maxShakes) {
            yield return new WaitForSeconds(2f);
            //CameraShaker.Presets.ShortShake2D();
            CameraShaker.Shake(new BounceShake(shakeParams));
        }
    }
}
