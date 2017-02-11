using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public delegate void PositionAction(VectInt3 offset);

public class Conway3D : MonoBehaviour
{
	public int dieFromExposureThreshold = 3;
	public int dieFromOvercrowdingThreshold = 3;
	public int propagateNumber = 8;

    static readonly int size = 16;

    private bool[,,,] cells = new bool[2, size, size, size];

    int population = 0;
	int lastPopulation = 0;
	int generation = 0;

	int steadyGenerationLimit = 5;
	int steadyGenerationCount = 0;

    bool tickTock = false;
    float generationInterval = 0.2f;

    public void SetNextCell(VectInt3 at, bool willBeAlive)
    {
        cells[tickTock ? 1 : 0, at.x, at.y, at.z] = willBeAlive;
    }

    public bool GetCurrentCell(VectInt3 at)
    {
        return cells[tickTock ? 0 : 1, at.x, at.y, at.z];
    }

	public void SetCurrentCell(VectInt3 at, bool isAlive)
	{
		cells[tickTock ? 0 : 1, at.x, at.y, at.z] = isAlive;
	}

    private int CreateSeed()
    {
        const int seedSize = 4;
        int seedPopulation = 0;

        PositionAction maybeSet = position =>
        {
            bool isPresent = Random.value >= 0.8;
            if (isPresent) ++seedPopulation;
            SetCurrentCell(position, isPresent);
        };

		VectInt3.Iterate3D(offset: (int)((size-seedSize)/2), length: seedSize, action: maybeSet);

        return seedPopulation;
    }

    // Use this for initialization
    void Start()
    {
        population = CreateSeed();

        StartCoroutine(Evolve());
    }

    IEnumerator Evolve()
    {
        while (population > 0) // Keep evolving while there are any cells left...
        {
			float evolutionStartTime = Time.time;
			float evolutionEndTime =
				evolutionStartTime + generationInterval; // This will determine how long we should wait at the end
		
			UpdateScene();

			population = 0;

            PositionAction evolvePosition = position =>
            {
                int neighbourCount = 0;

                PositionAction countNeighbours = countOffset =>
                {
                    var countPosition = (position + countOffset).WrapTo(offset: 0, length: size);
                    var isPresent = GetCurrentCell(countPosition);
                    if (isPresent)
                    {
                        ++neighbourCount;
                    }
                };

                VectInt3.Iterate3D(-1, 3, countNeighbours);

                bool isAlive = GetCurrentCell(position);
                bool willBeAlive;

                if (isAlive)
                {
					if (neighbourCount < dieFromExposureThreshold) // Die from exposure?
                    {
                        willBeAlive = false;
                    }
					else if (neighbourCount > dieFromOvercrowdingThreshold) // Die from overcrowding?
                    {
                        willBeAlive = false;
                    }
                    else // Live on!
                    {
                        willBeAlive = true;
                    }
                }
                else // No cell here yet: will any neighbours propagate?
                {
					willBeAlive = neighbourCount == propagateNumber;
                }

				if( willBeAlive )
				{
					
					++population;
				}

                SetNextCell(at: position, willBeAlive: willBeAlive);
            };

            VectInt3.Iterate3D(offset: 0, length: size, action: evolvePosition);

            tickTock = !tickTock;

			if (lastPopulation == population)
			{
				++steadyGenerationCount;
				if (steadyGenerationCount >= steadyGenerationLimit)
				{
					#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
					#else
					Application.Quit();
					#endif 
				}
			}
			else
			{
				steadyGenerationCount = 0;
			}

			lastPopulation = population;

			float waitTime = evolutionEndTime - Time.time;

			Debug.Log (string.Format("Generation {0}, population: {1}, waitTime: {2}", generation, population, waitTime));

			++generation;


            yield return new WaitForSeconds(Math.Max(0, waitTime));
        }
    }

    // Display

    private static float displayScale = 0.5f;

    private GameObject[,,] cellCubes = new GameObject[size, size, size];

    public void SetCellCube(VectInt3 at, bool isAlive)
    {
        GameObject existingCube = cellCubes[at.x, at.y, at.z];

        bool isPresent = (existingCube != null);
        bool shouldBePresent = GetCurrentCell(at: at);

        if (isPresent == shouldBePresent)
        {
            return;
        }

        if (shouldBePresent) // Birth a new cube
        {
            GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newCube.transform.position = new Vector3(at.x * displayScale, at.y * displayScale, at.z * displayScale);
            newCube.transform.localScale = new Vector3(displayScale, displayScale, displayScale);
            cellCubes[at.x, at.y, at.z] = newCube;
        }
        else // Remove the dead cube
        {
            Destroy(existingCube);
        }
    }

    private void UpdateScene()
    {
        PositionAction refreshCube = position =>
        {
            SetCellCube(at: position, isAlive: GetCurrentCell(at: position));
        };

        VectInt3.Iterate3D(offset: 0, length: size, action: refreshCube);
    }
}