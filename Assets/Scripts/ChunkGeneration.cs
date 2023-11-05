using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkGeneration : MonoBehaviour
{
    /// <summary>
    /// Цвета чанка
    /// </summary>
    [SerializeField]
    private Gradient gradient;

    [SerializeField][Min(10)]
    private int size;
    /// <summary>
    /// Размеры чанка
    /// </summary>
    public int Size => size;

    /// <summary>
    /// Максимальная высота чанка
    /// </summary>
    [SerializeField][Min(1)]
    private float maxHeight;

    /// <summary>
    /// Количество воды
    /// </summary>
    [SerializeField][Range(0, 1)]
    private float WaterValue;

    /// <summary>
    /// Объекты, которые генерируются на чанке
    /// </summary>
    [SerializeField]
    private GameObject[] objects;
    private Mesh mesh;
    private Texture2D tex;

    private Vector3[] Vertices;
    private Vector2[] uv;
    private int[] triangles;

    /// <summary>
    /// Правый чанк
    /// </summary>
    public ChunkGeneration Right {get; set;}
    /// <summary>
    /// Левый чанк
    /// </summary>
    public ChunkGeneration Left { get; set; }
    /// <summary>
    /// Передний чанк
    /// </summary>
    public ChunkGeneration Forward { get; set; }
    /// <summary>
    /// Задний чанк
    /// </summary>
    public ChunkGeneration Back { get; set; }
    /// <summary>
    /// Право-передний чанк
    /// </summary>
    public ChunkGeneration RightForward { get; set; }
    /// <summary>
    /// Право-задний чанк
    /// </summary>
    public ChunkGeneration RightBack { get; set; }
    /// <summary>
    /// Лево-передний чанк
    /// </summary>
    public ChunkGeneration LeftForward { get; set; }
    /// <summary>
    /// Лево-задний чанк
    /// </summary>
    public ChunkGeneration LeftBack { get; set; }

    /// <summary>
    /// Количество пустых чанков
    /// </summary>
    public int CountAvailableNeighbors
    {
        get
        {
            int c = 0;
            c += Right == null ? 1 : 0;
            c += Left == null ? 1 : 0;
            c += Forward == null ? 1 : 0;
            c += Back == null ? 1 : 0;
            c += RightBack == null ? 1 : 0;
            c += RightForward == null ? 1 : 0;
            c += LeftBack == null ? 1 : 0;
            c += LeftForward == null ? 1 : 0;
            return c;
        }
    }
    
    void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        GetComponent<VisibleObjects>().Distance = -Size/2 - Size/4;
        Generate();
        CreateWater();
    }

    private void Init()
    {
        tex = new Texture2D((Size + 1), (Size + 1));
        tex.filterMode = FilterMode.Trilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        GetComponent<MeshRenderer>().material.mainTexture = tex;

        mesh.name = "Procedural Grid";

        triangles = new int[Size * Size * 6];
        Vertices = new Vector3[(Size + 1) * (Size + 1)];
        uv = new Vector2[Vertices.Length];
    }

    /// <summary>
    /// Генерация мира (чанка)
    /// </summary>
    private void Generate()
    {
        Init();
        // Рисуем карту (чанк)
        float h = Random.Range(0.01f, 0.31f);
        for (int i = 0, vi = 0, ti = 0, z = 0; z <= Size; z++)
        {
            for (int x = 0; x <= Size; x++, i++)
            {
                float y = maxHeight - Mathf.PerlinNoise(x * h, z * h) * maxHeight;

                SetVertices(i,x, y, z); // устанавливаем вершину
                SetUv(i,x,z); // устанавливаем координаты текстуры
                SetColor(i); // задаем цвет
                if (x != Size && z != Size)
                {
                    SetTriangles(ti, vi);
                    ti += 6;
                    vi++;
                }
                if(x > 0 && z > 0 && z < Size && x < Size)
                    CreateObjects(i); // создаем объект (дерево, камень)
            }
            if (z != Size)
                vi++;
        }

        mesh.vertices = Vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        MeshCollider collider = GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = false;
    }

    /// <summary>
    /// Установить координаты текстуры
    /// </summary>
    /// <param name="i">Индекс вершины</param>
    /// <param name="x">Коррдината по оси x</param>
    /// <param name="z">Коррдината по оси z (или y)</param>
    private void SetUv(int i, int x, int z)
    {
        uv[i] = new Vector2((float)x / Size, (float)z / Size);
    }

    /// <summary>
    /// Установить вершину
    /// </summary>
    /// <param name="i">Индекс вершины</param>
    /// <param name="x">Коррдината по оси x</param>
    /// <param name="y">Коррдината по оси y</param>
    /// <param name="z">Коррдината по оси z</param>
    private void SetVertices(int i, int x, float y, int z)
    {
        Vertices[i] = new Vector3(x - Size / 2, y, z - Size / 2);
    }

    private void SetTriangles(int ti, int vi)
    {
        triangles[ti] = vi;
        triangles[ti + 3] = triangles[ti + 2] = vi + 1;
        triangles[ti + 4] = triangles[ti + 1] = vi + Size + 1;
        triangles[ti + 5] = vi + Size + 2;
    }

    /// <summary>
    /// Задает цвет текстуры
    /// </summary>
    /// <param name="i">Индекс вершины</param>
    private void SetColor(int i)
    {
        tex.SetPixel((int)Vertices[i].x+Size/2, (int)Vertices[i].z + Size / 2, gradient.Evaluate(Vertices[i].y / maxHeight));

        tex.Apply();
    }

    /// <summary>
    /// Создание объектов на случайных местах
    /// </summary>
    /// <param name="i">Индекс вершины</param>
    private void CreateObjects(int i)
    {
        if (Vertices[i].y < 0.8f * maxHeight && Vertices[i].y > (WaterValue+0.2f)*maxHeight)
        {
            GameObject obj = Instantiate(objects[Random.Range(0,objects.Length)],transform,false);
            obj.transform.localPosition = Vertices[i] + (Vector3.right * Random.Range(-0.5f,0.5f)) + (Vector3.forward * Random.Range(-0.5f, 0.5f)) + (Vector3.up * (obj.transform.localScale.y*2));
            obj.AddComponent<VisibleObjects>();
        }
    }

    /// <summary>
    /// Создает воду
    /// </summary>
    private void CreateWater()
    {
        #region Создание объекта воды
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cube);
        water.transform.SetParent(transform,false);
        water.name = "water";
        water.transform.localPosition = new Vector3(0, WaterValue * maxHeight, 0);
        water.transform.localScale = new Vector3(Size, 0.01f ,Size);
        Destroy(water.GetComponent<BoxCollider>());
        #endregion
        #region Создание текстуры воды
        Texture2D waterTex = new Texture2D(1, 1);
        waterTex.filterMode = FilterMode.Point;
        waterTex.wrapMode = TextureWrapMode.Clamp;
        waterTex.alphaIsTransparency = true;
        waterTex.SetPixel(0,0,new Color(0,0.5f,1,0.75f));
        waterTex.Apply();
        #endregion
        MeshRenderer r = water.GetComponent<MeshRenderer>();
        r.material.mainTexture = waterTex;
        #region Прозрачность воды // https://stackoverflow.com/questions/72309866/how-to-change-material-rendering-mode-to-fade-by-script
        r.material.SetFloat("_Glossiness", 0.75f);
        r.material.SetOverrideTag("RenderType", "Transparent");
        r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        r.material.SetInt("_ZWrite", 0);
        r.material.DisableKeyword("_ALPHATEST_ON");
        r.material.EnableKeyword("_ALPHABLEND_ON");
        r.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        r.material.SetFloat("_Mode", 3.0f);
        r.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    #endregion
    }

    public Vector3 this[int index]
    {
        get => Vertices[index];
    }

    /// <summary>
    /// Объединить соседние чанки
    /// </summary>
    public void MergeAdjacentChunks()
    {
        Right.Left = this;
        Right.Back = RightBack;
        Right.Forward = RightForward;
        Right.LeftBack = Back;
        Right.LeftForward = Forward;

        Left.Right = this;
        Left.Back = LeftBack;
        Left.Forward = LeftForward;
        Left.RightBack = Back;
        Left.RightForward = Forward;

        Back.Forward = this;
        Back.Right = RightBack;
        Back.Left = LeftBack;
        Back.RightForward = Right;
        Back.LeftForward = Left;

        Forward.Back = this;
        Forward.Right = RightForward;
        Forward.Left = LeftForward;
        Forward.RightBack = Right;
        Forward.LeftBack = Left;

        RightBack.Forward = Right;
        RightBack.Left = Back;
        RightBack.LeftForward = this;

        RightForward.Back = Right;
        RightForward.Left = Forward;
        RightForward.LeftBack = this;

        LeftBack.Forward = Left;
        LeftBack.Right = Back;
        LeftBack.RightForward = this;
        
        LeftForward.Back = Left;
        LeftForward.Right = Forward;
        LeftForward.RightBack = this;
    }

    /// <summary>
    /// Объединяет вершины чанков в заданном направлении
    /// </summary>
    /// <param name="chunk">Чанк с котором объединяются вершины</param>
    /// <param name="axis">Направление</param>
    /// <returns></returns>
    public bool CombineVertexes(ChunkGeneration chunk, Direction axis)
    {
        bool c = true;
        bool result = false;

        if (axis == Direction.Right && chunk.Right == null)
        {
            name = chunk.name + " right";
            for (int i = 0; i <= Size; i++)
            {
                Vector3 vector = Vertices[i + (Size + 1) * (Size)]; // Вершина последнего ряда (по оси х)
                vector.y = chunk[i].y; // Координата Y первого ряда (по оси х)
                Vertices[i + (Size + 1) * (Size)] = vector;

                SetColor(i + (Size + 1) * (Size));
            }
            transform.position = chunk.gameObject.transform.position + new Vector3(0, 0, -(Size));

            result = true;
            if(c)
                chunk.Right = this;
        }
        else if (axis == Direction.Left && chunk.Left == null)
        {
            name = chunk.name + " left";

            for (int i = 0; i <= Size; i++)
            {
                Vector3 vector = Vertices[i]; // Вершина первого ряда (по оси х)
                vector.y = chunk[i + (Size + 1) * (Size)].y; // Координата Y последнего ряда (по оси х)
                Vertices[i] = vector;

                SetColor(i);
            }
            transform.position = chunk.gameObject.transform.position + new Vector3(0, 0, (Size));

            result = true;
            if(c)
                chunk.Left = this;
        }
        else if (axis == Direction.Forward && chunk.Forward == null)
        {
            name = chunk.name + " forward";

            for (int i = 0; i <= Size; i++)
            {
                Vector3 vector = Vertices[Size * (i + 1) + i]; // Вершина последнего сталбца (по оси z)
                vector.y = chunk[i * (Size + 1)].y; // Координата Y первого сталбца (по оси z)
                Vertices[Size * (i + 1) + i] = vector;

                SetColor(Size * (i + 1) + i);
            }
            transform.position = chunk.gameObject.transform.position + new Vector3(-Size, 0, 0);

            result = true;

            if(c)
                chunk.Forward = this;
        }
        else if (axis == Direction.Back && chunk.Back == null)
        {
            name = chunk.name + " back";

            for (int i = 0; i <= Size; i++)
            {
                Vector3 vector = Vertices[i * (Size + 1)]; // Вершина первого сталбца (по оси z)
                vector.y = chunk[Size * (i + 1) + i].y; // Координата Y последнего сталбца (по оси z)
                Vertices[i * (Size + 1)] = vector;

                SetColor(i * (Size + 1));
            }
            transform.position = chunk.gameObject.transform.position + new Vector3(Size, 0, 0);

            result = true;

            if(c)
                chunk.Back = this;
        }
        else if (axis == Direction.RightBack && chunk.RightBack == null)
        {
            c = false;
            CombineVertexes(chunk.Right,Direction.Back);
            CombineVertexes(chunk.Back,Direction.Right);
            name = chunk.name + " rightback";

            result = true;
            chunk.RightBack = this;
        }
        else if (axis == Direction.RightForward && chunk.RightForward == null)
        {
            c = false;
            CombineVertexes(chunk.Right, Direction.Forward);
            CombineVertexes(chunk.Forward, Direction.Right);
            name = chunk.name + " rightforward";

            result = true;
            chunk.RightForward = this;
        }
        else if (axis == Direction.LeftForward && chunk.LeftForward == null)
        {
            c = false;
            CombineVertexes(chunk.Left, Direction.Forward);
            CombineVertexes(chunk.Forward, Direction.Left);
            name = chunk.name + " leftforward";

            result = true;
            chunk.LeftForward = this;
        }
        else if (axis == Direction.LeftBack && chunk.LeftBack == null)
        {
            c = false;
            CombineVertexes(chunk.Left, Direction.Back);
            CombineVertexes(chunk.Back, Direction.Left);
            name = chunk.name + " leftback";

            result = true;
            chunk.LeftBack = this;
        }

        mesh.vertices = Vertices;
        MeshCollider collider = GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = false;

        return result;
    }
}
