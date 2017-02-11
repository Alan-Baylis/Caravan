using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownPlacer : MonoBehaviour
{
    [SerializeField] GameObject _town;
    [SerializeField] GameObject[] _towns;
    [SerializeField] int _numTowns;
    [SerializeField] Vector2 _minimumPosition;
    [SerializeField] Vector2 _maximumPosition;

	void Start ()
    {
        _towns = new GameObject[_numTowns];
		for (int i = 0; i < _numTowns; ++i)
        {
            GameObject currentTown = Instantiate(_town);
            Vector3 newPosition = new Vector3();
            newPosition.x = Random.Range(_minimumPosition.x, _maximumPosition.x);
            newPosition.y = 10.0f;
            newPosition.z = Random.Range(_minimumPosition.x, _maximumPosition.x);
            currentTown.transform.position = newPosition;
            _towns[i] = currentTown;
        }
	}
}
