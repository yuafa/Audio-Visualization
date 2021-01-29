using System.Collections;
using UnityEngine;

public class Instantiate512cubes : MonoBehaviour
{
    public GameObject _sampleCubePrefab;
    GameObject[] _sampleCubes = new GameObject[128];
    public float _maxScale = 1000f;

    private void Start()
    {
        for (int i = 0; i < _sampleCubes.Length; i++)
        {
            GameObject _instanceSampleCube = Instantiate(_sampleCubePrefab);
            _instanceSampleCube.transform.position = transform.position;
            _instanceSampleCube.transform.parent = transform;
            _instanceSampleCube.name = "SampleCube-" + i;
            transform.eulerAngles = new Vector3(0, -2.8125f * i, 0);
            _instanceSampleCube.transform.position = new Vector3(0, 0, 0.1f);
            _sampleCubes[i] = _instanceSampleCube;
        }
        transform.eulerAngles = new Vector3(0f, -90f, 0f);
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _sampleCubes.Length; i++)
        {
            _sampleCubes[i].transform.localScale = new Vector3(1f, (AudioPeer._samples[i] * _maxScale + 1f), 1f);
        }
    }
}
