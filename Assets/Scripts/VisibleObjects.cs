using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleObjects : MonoBehaviour
{
    private MeshRenderer rend;
    public int Distance = -1;

    private void Awake()
    {
        rend = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        bool isv = IsVisible(Camera.main, gameObject, Distance);

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isv);
        }

        rend.enabled = isv;
    }

    /// <summary>
    /// ������ �� �������� ������ � ���� ���������
    /// </summary>
    /// <param name="c">������ (���� ���������)</param>
    /// <param name="t">������</param>
    /// <param name="dis">���������� �� ������� ������</param>
    /// <returns>true ���� ������, false ���� �� ������</returns>
    public static bool IsVisible(Camera c, GameObject t, int dis)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
        
        if(planes == null)
            return false;

        foreach(Plane plane in planes)
            if(plane.GetDistanceToPoint(t.transform.position) < dis)
                return false;
        return true;
    }
}
