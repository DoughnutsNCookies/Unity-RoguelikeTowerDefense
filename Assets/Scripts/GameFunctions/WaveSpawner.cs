using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    public static int enemiesAlive = 0;
    public Wave[] waves;

    public bool isSpawning = false;
	public Transform spawnPoint;
    // public Text waveCountdownText;

    public float timeBetweenWaves = 5f;
	private float countdown = 2f;
	private int waveIndex = 0;

	void Update()
	{
        if (isSpawning == false)
            return;
        if (enemiesAlive > 0)
            return;
        if (countdown <= 0f)
		{
			StartCoroutine(SpawnWave());
			countdown = timeBetweenWaves;
            return;
        }
		countdown -= Time.deltaTime;
		countdown = Mathf.Clamp(countdown, 0f, Mathf.Infinity);
		// Mathf.Ceil(countdown).ToString();
		// waveCountdownText.text = string.Format("{0:00.0}", countdown);
	}

	IEnumerator SpawnWave()
	{
        Wave wave = waves[waveIndex];
        for (int i = 0; i < wave.count; i++)
		{
			SpawnEnemy(wave.enemy);
			yield return new WaitForSeconds(1f / wave.rate);
		}
		waveIndex++;
		if (waveIndex == waves.Length)
		{
            this.enabled = false;
        }
	}

	void SpawnEnemy(GameObject enemy)
	{
		Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;
    }
}
