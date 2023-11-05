using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    List<ChunkGeneration> chunkList;
    /// <summary>
    /// Префаб чанка
    /// </summary>
    [SerializeField] private GameObject prefabChunk;
    /// <summary>
    /// Объект персонажа
    /// </summary>
    [SerializeField] private Player player;

    // Создаем первый чанк при старте
    void Start()
    {
        chunkList = new List<ChunkGeneration>();
        ChunkGeneration firstChunk = Instantiate(prefabChunk).GetComponent<ChunkGeneration>();
        firstChunk.name = "center";
        chunkList.Add(firstChunk);
    }

    void Update()
    {
        Spawner();
    }

    /// <summary>
    /// Возвращает чанк на котором находится персонаж
    /// </summary>
    private ChunkGeneration GetChunkCloserToThePlayer()
    {
        float[] distances = chunkList.Select(d => Mathf.Abs(Vector3.Distance(player.transform.position, d.transform.position))).ToArray();
        float min = distances.Min();
        for(int i = 0; i < distances.Length; i++) 
        {
            if (distances[i] == min)
                return chunkList[i];
        }
        return null;
    }

    /// <summary>
    /// Создаем чанк
    /// </summary>
    private void Spawner()
    {
        ChunkGeneration chunk = GetChunkCloserToThePlayer();
        
        // Создаем соседние чанки вокруг чанка на котором находится персонаж
        int i = chunk.CountAvailableNeighbors;
        for (; i > 0; i--)
        {
            //Создаем чанк
            ChunkGeneration newChunk = Instantiate(prefabChunk).GetComponent<ChunkGeneration>();
            chunkList.Add(newChunk);
            // Если соседа нет, объединяем вершины, а если есть, продолжаем провоерку
            if (newChunk.CombineVertexes(chunk, Direction.Right)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.Left)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.Forward)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.Back)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.RightBack)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.RightForward)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.LeftForward)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.LeftBack)) continue;
        }

        // Объединяем всех соседей с chunk
        chunk.MergeAdjacentChunks();
    }
}
