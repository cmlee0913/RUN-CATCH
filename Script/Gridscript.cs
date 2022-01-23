using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class Gridscript : MonoBehaviour
{
    public int x_n = 21; // x길이.
    public int z_n = 21; // y길이.
    int max_tiles = 21 * 21; // 타일 최대 수 지정.
    public GameObject player;
    public GameObject text;
    float timer;
    float waitingTime = 1;

    public enum TileType // 타일 종류 이넘으로 선언.
    {
        Plain,
        Block
    }
    public TileType[] world = null;

    public void BuildWorld() // 월드 생성 함수.
    {
        player.transform.position = locateCenter(new Vector3(x_n / 2, 0, z_n / 2)); // 플레이어를 셀의 가운데로 놓음.
        int player_cell = pos2Cell(player.transform.position); // 플레이어가 서있는 좌표 셀로 만듬. 

        world = new TileType[max_tiles]; // 셀의 개수 만큼 타일 을 깔아줌.

        for (int i = 0; i < max_tiles; i++)
        {
            world[i] = TileType.Plain; // 월드를 플레인 타입으로 선언.
            if (i == player_cell) continue; // 플레이어가 서있는 셀일 경우에도 계속 진행.
        }
    }

    public Vector3 locateCenter( Vector3 pos ) // 셀 중간으로 위치 바꿔준 후 반환.
    {
        return pos + new Vector3( 0.5f, 0, 0.5f );
    }

    public int pos2Cell(Vector3 pos) // 좌표 값 이용하여 셀 넘버 반환.
    {
        return ((int)pos.z) * x_n + (int)pos.x;
    }

    public Vector3 cell2Pos(int cellno) // 셀 넘버 이용하여 좌표값 반환.
    {
        return new Vector3(cellno % x_n, 0, cellno / x_n);
    }

    public int[] findNeighbors(int cellno, TileType[] world) // 셀을 중심으로 8방향 이웃을 찾아서 배열 형태로 반환하는 함수.
    {
        List<int> neighbors = new List<int> { -1, 1, -x_n, x_n, -x_n - 1, -x_n + 1, x_n - 1, x_n + 1 };

        // 테두리 부분 뺴주기.
        if (cellno % x_n == 0) neighbors.RemoveAll((no) => { return no == -1 || no == -1 - x_n || no == -1 + x_n; });
        if (cellno % x_n == x_n - 1) neighbors.RemoveAll((no) => { return no == 1 || no == 1 - x_n || no == 1 + x_n; });
        if (cellno / x_n == 0) neighbors.RemoveAll((no) => { return no == -x_n || no == -x_n - 1 || no == -x_n + 1; });
        if (cellno / x_n == z_n - 1) neighbors.RemoveAll((no) => { return no == x_n || no == x_n - 1 || no == x_n + 1; });

        // 장애물이랑 범위 벗어나는 부분 빼주기.
        for (int i = 0; i < neighbors.Count;)
        {
            neighbors[i] += cellno; // 배열에 셀 넘버 부여.

            if (neighbors[i] < 0 || neighbors[i] >= x_n * z_n || world[neighbors[i]] == TileType.Block)
            {
                neighbors.RemoveAt(i); // 조건 맞을 경우 배열에서 제외.
            }
            else
            {
                i++; // 아닌 경우 루프.
            }
        }

        Vector3 X = cell2Pos(cellno); // 셀넘버를 좌표로 변환.

        // 장애물 뚫고 대각선 이동 금지.
        for (int i = 0; i < neighbors.Count;)
        {
            Vector3 Xp = cell2Pos(neighbors[i]); // 이웃의 셀 넘버를 좌표로 변환.
            if ((X.x - Xp.x) * (X.z - Xp.z) != 0) // 이웃과 x좌표나 z좌표가 같을 경우.
            {
                // 대각선 부분을 비교로하여 양 옆이 블록일 경우 이웃에서 삭제.
                if (world[pos2Cell(new Vector3(Xp.x, 0, X.z))] == TileType.Block && world[pos2Cell(new Vector3(X.x, 0, Xp.z))] == TileType.Block)
                {
                    neighbors.RemoveAt(i);
                    continue;
                }
            }
            i++; // 이웃의 수 만큼 반복.
        }
        return neighbors.ToArray(); // 나온 이웃 셀 리스트를 반환.
    }

    public int[] buildPath(int[] parents, int from, int to) // 움직일 길을 배열 형태로 반환한다.
    {
        if (parents == null) return null;

        List<int> path = new List<int>();
        int current = to; // 도착지점 현재에 저장.
        while (current != from) // 현재가 시작지점이 아닐 경우 반복.
        {
            path.Add(current);
            current = parents[current]; // 현재의 부모를 현재로 저장.
        }
        path.Add(from); // 모두 저장했으면 시작지점도 저장.
        
        path.Reverse(); // 배열 거꾸로 뒤집음.
        return path.ToArray(); // 반환.
    }

    public int[] BFS(int from, int to, TileType[] world)
    {
        if (from < 0 || from >= max_tiles || to < 0 || to >= max_tiles)
        {
            Debug.Log("gkgk");
            return null; // 범위 밖으로 나가는 경우 예외처리.
        }

        int[] parents = new int[max_tiles]; // 부모 배열을 맥스타일만큼 동적할당.

        for (int i = 0; i < parents.Length; i++)
        {
            parents[i] = -1; // 부모 배열을 -1로 초기화.
        }

        List<int> N = new List<int>() { from };
        //Debug.Log(N.Count);
        while (N.Count > 0)
        {
            int current = N[0];
            N.RemoveAt(0); // 초기 시작 지점을 뺴줌.
            int[] neighbors = findNeighbors(current, world); // 이웃을 찾아서 배열에 넣어둠.

            foreach (var neighbor in neighbors)
            {
                if (neighbor == to) // 이웃 중 하나가 도착지점.
                {
                    parents[neighbor] = current;
                    return buildPath(parents, from, to); // 길 그림.
                }

                if (parents[neighbor] == -1) // 이웃의 부모가 아직 설정되지 않았다.
                {
                    parents[neighbor] = current; // 현재의 타일을 이웃의 부모로 설정.
                    N.Add(neighbor); // 이웃을 배열에 추가.
                }
            }
        }
        //Debug.Log("죽음");
        timer += Time.deltaTime;
        return null; // 경로를 찾을 수 없을 때.      
    }

    public IEnumerator Move(GameObject player, Vector3 destination)
    {
        int start = pos2Cell(player.transform.position);
        int end = pos2Cell(destination);
        int[] path = null;

        path = BFS(start, end, world);
        if (timer > waitingTime)
        {
            text.SetActive(true);
        }
        List<int> remaining = new List<int>(path); // convert int array to List
        remaining.RemoveAt(0); // we don't need the first one, since the first element should be same as that of source.

        while (remaining.Count > 0)
        {
            int to = remaining[0];
            remaining.RemoveAt(0);

            Vector3 toLoc = locateCenter(cell2Pos(to));
            while (player.transform.position != toLoc)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, toLoc, 4f * Time.deltaTime);
                yield return null;
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
