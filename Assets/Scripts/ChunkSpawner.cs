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
    /// ������ �����
    /// </summary>
    [SerializeField] private GameObject prefabChunk;
    /// <summary>
    /// ������ ���������
    /// </summary>
    [SerializeField] private Player player;

    // ������� ������ ���� ��� ������
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
    /// ���������� ���� �� ������� ��������� ��������
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
    /// ������� ����
    /// </summary>
    private void Spawner()
    {
        ChunkGeneration chunk = GetChunkCloserToThePlayer();
        
        // ������� �������� ����� ������ ����� �� ������� ��������� ��������
        int i = chunk.CountAvailableNeighbors;
        for (; i > 0; i--)
        {
            //������� ����
            ChunkGeneration newChunk = Instantiate(prefabChunk).GetComponent<ChunkGeneration>();
            chunkList.Add(newChunk);
            // ���� ������ ���, ���������� �������, � ���� ����, ���������� ���������
            if (newChunk.CombineVertexes(chunk, Direction.Right)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.Left)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.Forward)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.Back)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.RightBack)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.RightForward)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.LeftForward)) continue;
            else if (newChunk.CombineVertexes(chunk, Direction.LeftBack)) continue;
        }

        // ���������� ���� ������� � chunk
        chunk.MergeAdjacentChunks();
    }
}
