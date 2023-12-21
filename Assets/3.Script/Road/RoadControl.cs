using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum RoadType
{
    Straight, // 직진 도로
    Corner, // 코너 도로
    Intersection, // 교차로
    BuildingTile, // 건물 밑에 설치될 도로
    Test // 선 없는 기본 타일
}

public enum ClickState
{ 
    None, // 초기 상태
    Create // 도로를 그리는 중
}

[Serializable]
public struct Coordinate
{
    public float x;
    public float z;

    public Coordinate(float x, float z)
    {
        this.x = x;
        this.z = z;
    }
}

public class RoadControl : MonoBehaviour
{
    public static RoadControl instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Ground Layer")]
    public LayerMask groundLayer;

    [Header("Road Prefab")]
    [SerializeField] private GameObject[] roadPrefabs; // <Index> 0:Straight, 1:Corner, 2:Intersection, 3:BuildingTile

    [Header("Click State")]
    public ClickState currentClickState = ClickState.None;

    [Header("Point")]
    public Vector3 startPoint = Vector3.zero;
    public Vector3 endPoint = Vector3.zero;

    [Header("Spawn Road")]
    public List<Coordinate> roads = new List<Coordinate>();
    private List<Coordinate> coordinates = new List<Coordinate>();
    private int roadCountX = 0;
    private int roadCountZ = 0;

    public event Action OnRoadCreated;

    private void Update()
    {
        GetMouseInput();
    }

    private void GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDraw();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();
        }
    }

    private void StartDraw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Building") || hit.collider.CompareTag("Road")) // 마우스 클릭
            {
                if (currentClickState == ClickState.None) // 도로 그리기 시작할 건물 선택 (도로 그리기 시작)
                {
                    currentClickState = ClickState.Create;
                    startPoint = hit.collider.gameObject.transform.position;
                    //Debug.Log("그리기 시작");
                }
            }
        }
    }

    private void EndDraw()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Building") || hit.collider.CompareTag("Road")) // 마우스 떼기
            {
                endPoint = hit.collider.gameObject.transform.position;

                if (startPoint == endPoint)
                {
                    currentClickState = ClickState.None;
                    //Debug.Log("그리기 취소");
                    return;
                }

                if (currentClickState == ClickState.Create) // 도로를 연결할 건물 선택 (도로 생성)
                {
                    currentClickState = ClickState.None;

                    //Debug.Log("그리기 완료");
                    //Road_Optimize(3);
                    MeasureRoad();
                }
            }
            else // 건물 이외에서 마우스 떼기
            {
                if (currentClickState == ClickState.Create)
                {
                    currentClickState = ClickState.None;
                    //Debug.Log("그리기 취소");
                }
            }
        }

        OnRoadCreated?.Invoke();
    }

    private void Road_Optimize(int n) // start와 end의 주위 n칸에 이미 이어진 도로가 있는지 확인후 연결
    {
        float startX = startPoint.x;
        float startZ = startPoint.z;

        float endX = endPoint.x;
        float endZ = endPoint.z;

        // ( ↘ 방향 )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x + i * 2.5f, startPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x + i * 2.5f;
                        startZ = startPoint.z - j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x - i * 2.5f, endPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x - i * 2.5f;
                        endZ = endPoint.z + j * 2.5f;
                    }
                }
            }
        }
        // ( ↗ 방향 )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x + i * 2.5f, startPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x + i * 2.5f;
                        startZ = startPoint.z + j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x - i * 2.5f, endPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x - i * 2.5f;
                        endZ = endPoint.z - j * 2.5f;
                    }
                }
            }
        }
        // ( ↖ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x - i * 2.5f, startPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x - i * 2.5f;
                        startZ = startPoint.z + j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x + i * 2.5f, endPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x + i * 2.5f;
                        endZ = endPoint.z - j * 2.5f;
                    }
                }
            }
        }
        // ( ↙ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(startPoint.x - i * 2.5f, startPoint.z - j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        startX = startPoint.x - i * 2.5f;
                        startZ = startPoint.z - j * 2.5f;
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Coordinate nextCoordinate = new Coordinate(endPoint.x + i * 2.5f, endPoint.z + j * 2.5f);
                    if (RoadControl.instance.roads.Contains(nextCoordinate))
                    {
                        endX = endPoint.x + i * 2.5f;
                        endZ = endPoint.z + j * 2.5f;
                    }
                }
            }
        }

        startPoint.x = startX;
        startPoint.z = startZ;

        endPoint.x = endX;
        endPoint.z = endZ;
    }

    private void MeasureRoad() // startPoint 부터 endPoint 까지 생성할 도로의 위치를 계산하여 저장
    {
        // 도로 하나의 길이 지정 (정사각형)
        float roadWidth = 2.5f;

        // 필요 도로 개수 계산
        roadCountX = (int)(Mathf.Abs(startPoint.x - endPoint.x) / roadWidth) + 1;
        roadCountZ = (int)(Mathf.Abs(startPoint.z - endPoint.z) / roadWidth) + 1;

        #region 방향에 따라 체크
        // ( ↘ 방향 )
        // start.x < end.x
        // start.z > end.z
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j >= endPoint.z; j -= 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( ↗ 방향 )
        // start.x < end.x
        // start.z < end.z
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j <= endPoint.z; j += 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( ↖ 방향 )
        // start.x > end.x
        // start.z > end.z
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j >= endPoint.z; j -= 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( ↙ 방향 )
        // start.x > end.x
        // start.z < end.z
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 1.25f)
            {
                Check_Test(i);
            }

            for (float j = startPoint.z; j <= endPoint.z; j += 1.25f)
            {
                Check_Test_Z(j);
            }
        }
        // ( - 방향 )
        // start.x < end.x
        else if (startPoint.x < endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i <= endPoint.x; i += 2.5f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( - 방향 )
        // start.x > end.x
        else if (startPoint.x > endPoint.x && startPoint.z == endPoint.z)
        {
            for (float i = startPoint.x; i >= endPoint.x; i -= 2.5f)
            {
                Check_Straight(i, startPoint.z);
            }
        }
        // ( ↑ 방향 )
        // start.z < end.z
        else if (startPoint.z < endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j <= endPoint.z; j += 2.5f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        // ( ↓ 방향 )
        // start.z > end.z
        else if (startPoint.z > endPoint.z && startPoint.x == endPoint.x)
        {
            for (float j = startPoint.z; j >= endPoint.z; j -= 2.5f)
            {
                Check_Straight(startPoint.x, j);
            }
        }
        #endregion

        if (roadCountX == roadCountZ)
        {
            Check_Additonal_Diagonal();
        }

        if (GameManager.instance.CheckCanReach(startPoint.x, startPoint.z, endPoint.x, endPoint.z, roads, out int count_original))
        {
            // 비교 시작
            GameManager.instance.CheckCanReach(startPoint.x, startPoint.z, endPoint.x, endPoint.z, coordinates, out int count_new);

            if (count_original > count_new)
            {
                SpawnRoad(coordinates);
            }
        }
        else
        {
            SpawnRoad(coordinates);
        }
    }

    private void Check_Test(float i)
    {
        // 시작점과 끝점을 잇는 직선위의 점들만 검사
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float j = a * i + b;

        // x, z가 모두 중앙을 지날 때
        if (i % 2.5f == 0 && j % 2.5f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
        // x는 중앙을, z는 경계를 지날 때 
        else if (i % 2.5f == 0 && j % 1.25 == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j - 1.25f;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i;
            currentCord2.z = j + 1.25f;
            coordinates.Add(currentCord2);
        }
        // x는 중앙을, z는 애매할 때
        else if (i % 2.5f == 0)
        {
            float newZPoint = 0;

            if (startPoint.z < endPoint.z)
            {
                for (float t = startPoint.z; t <= endPoint.z; t += 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.z; t >= endPoint.z; t -= 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = newZPoint;
            coordinates.Add(currentCord);
        }
        // x가 경계를, z는 중앙을 지날 때
        else if (i % 1.25f == 0 && j % 2.5f == 0)
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i + 1.25f;
            currentCord2.z = j;
            coordinates.Add(currentCord2);
        }
        // x가 경계를, z는 애매할 때
        else if (i % 1.25f == 0 && j % 1.25f != 0)
        {
            float newZPoint = 0;

            if (startPoint.z < endPoint.z)
            {
                for (float t = startPoint.z; t <= endPoint.z; t += 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.z; t >= endPoint.z; t -= 2.5f)
                {
                    if (Mathf.Abs(j - t) < 1.25f)
                    {
                        newZPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = newZPoint;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = i + 1.25f;
            currentCord2.z = newZPoint;
            coordinates.Add(currentCord2);
        }
        // 둘다 경계를 지날 때
        else if ((roadCountX != roadCountZ) && (i % 1.25f == 0) && (j % 1.25f == 0))
        {
            Check_Additonal_Else(i, j);
        }
    }

    private void Check_Test_Z(float j)
    {
        // 시작점과 끝점을 잇는 직선위의 점들만 검사
        float a = (endPoint.z - startPoint.z) / (endPoint.x - startPoint.x);
        float b = startPoint.z - a * startPoint.x;
        float i = (j - b) / a;

        if (j % 1.25f == 0 && (j / 1.25f) % 2 != 0 && i % 1.25f != 0)
        {
            float newXPoint = 0;

            if (startPoint.x < endPoint.x)
            {
                for (float t = startPoint.x; t <= endPoint.x; t += 2.5f)
                {
                    if (Mathf.Abs(i - t) < 1.25f)
                    {
                        newXPoint = t;
                        break;
                    }
                }
            }
            else
            {
                for (float t = startPoint.x; t >= endPoint.x; t -= 2.5f)
                {
                    if (Mathf.Abs(i - t) < 1.25f)
                    {
                        newXPoint = t;
                        break;
                    }
                }
            }

            Coordinate currentCord = new Coordinate();
            currentCord.x = newXPoint;
            currentCord.z = j - 1.25f;
            coordinates.Add(currentCord);

            Coordinate currentCord2 = new Coordinate();
            currentCord2.x = newXPoint;
            currentCord2.z = j + 1.25f;
            coordinates.Add(currentCord2);
        }
    }

    private void Check_Straight(float i, float j)
    {
        if (i % 2.5f == 0 && j % 2.5f == 0) // 점이 칸의 정 중앙을 지날 때 해당 칸 체크
        {
            Coordinate currentCord = new Coordinate();
            currentCord.x = i;
            currentCord.z = j;
            coordinates.Add(currentCord);
        }
    }

    private void Check_Additonal_Diagonal()
    {
        // ( ↘ 방향 )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x + 2.5f * (n + 1);
                currentCord.z = startPoint.z - 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
        // ( ↗ 방향 )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x + 2.5f * (n + 1);
                currentCord.z = startPoint.z + 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
        // ( ↖ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x - 2.5f * (n + 1);
                currentCord.z = startPoint.z + 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
        // ( ↙ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            for (int n = 0; n < roadCountX - 1; n++)
            {
                Coordinate currentCord = new Coordinate();
                currentCord.x = startPoint.x - 2.5f * (n + 1);
                currentCord.z = startPoint.z - 2.5f * n;
                coordinates.Add(currentCord);
            }
        }
    }

    private void Check_Additonal_Else(float i, float j)
    {
        // ( ↘ 방향 )
        if (startPoint.x < endPoint.x && startPoint.z > endPoint.z)
        {
            //Debug.Log("↘");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i + 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( ↗ 방향 )
        else if (startPoint.x < endPoint.x && startPoint.z < endPoint.z)
        {
            //Debug.Log("↗");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( ↖ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z < endPoint.z)
        {
            //Debug.Log("↖");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i + 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
        // ( ↙ 방향 )
        else if (startPoint.x > endPoint.x && startPoint.z > endPoint.z)
        {
            //Debug.Log("↙");
            Coordinate currentCord = new Coordinate();
            currentCord.x = i - 1.25f;
            currentCord.z = j + 1.25f;
            coordinates.Add(currentCord);
        }
    }

    private void SpawnRoad(List<Coordinate> coordinates) // 저장된 위치마다 도로를 생성하는 메서드
    {
        for (int i = 0; i < coordinates.Count; i++)
        {
            if (!roads.Contains(coordinates[i])) // 해당 위치에 이미 도로가 생성되어있는지 체크
            {
                roads.Add(coordinates[i]);
                GameObject currentRoad = Instantiate(roadPrefabs[4], new Vector3(coordinates[i].x, 0.01f, coordinates[i].z), Quaternion.identity);
                currentRoad.transform.SetParent(transform);
                currentRoad.GetComponent<Road>().coordinate.x = coordinates[i].x;
                currentRoad.GetComponent<Road>().coordinate.z = coordinates[i].z;
            }
        }

        // 도로 최적화
        //DeleteOverlapRoad();

        coordinates.Clear();
    }

    private void DeleteOverlapRoad() // 목적지까지 도로를 연결했을때 굳이 필요없는 도로를 제거
    {
        foreach (Coordinate coordinate in coordinates)
        {
            if (((coordinate.x == startPoint.x) && (coordinate.z == startPoint.z)) || ((coordinate.x == endPoint.x) && (coordinate.z == endPoint.z)))
            {
                continue;
            }

            roads.Remove(coordinate);

            if (GameManager.instance.CheckCanReach(startPoint.x, startPoint.z, endPoint.x, endPoint.z, roads, out int temp))
            {
                //Debug.Log($"{coordinate.x},{coordinate.z}의 도로 제거");

                for (int i = 0; i < transform.childCount; i++)
                {
                    if ((transform.GetChild(i).GetComponent<Road>().coordinate.x == coordinate.x) && (transform.GetChild(i).GetComponent<Road>().coordinate.z == coordinate.z))
                    {
                        roads.Remove(coordinate);
                        Destroy(transform.GetChild(i).gameObject);
                        break;
                    }
                }
            }
            else
            {
                roads.Add(coordinate);
            }
        }
    }
}
