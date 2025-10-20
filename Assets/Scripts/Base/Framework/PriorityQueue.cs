using System;
using System.Collections.Generic;

// 이진 힙(이진 트리, Binary Heap 등)을 기반으로 하는 효율적인 우선순위 큐 클래스
// TElement 큐에 저장할 아이템의 타입
// TPriority 우선순위를 결정할 값의 타입 (IComparable 필요 => 비교해서 우선순위를 정하기 위해)

// 구조:
// 최대 힙 (Max Heap)
// 부모 노드의 키 값이 자식 노드보다 크거나 같은 완전이진트리
// key(부모노드) ≥ key(자식노드)
// 최소 힙 (Min Heap)
// 부모 노드의 키 값이 자식 노드보다 작거나 같은 완전이진트리
// key(부모노드) ≥ key(자식노드) 
public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    // (아이템, 우선순위) 튜플을 리스트에 저장하여 힙 구현
    private readonly List<(TElement Element, TPriority Priority)> _heap;
    private readonly bool _isMinHeap;

    // 큐에 저장된 아이템의 수
    public int Count => _heap.Count;

    // 우선순위 큐 생성자
    // isMinHeap = true면 최소 힙(낮은 값 우선), false면 최대 힙(높은 값 우선)
    public PriorityQueue(bool isMinHeap = true)
    {
        _heap = new List<(TElement, TPriority)>();
        _isMinHeap = isMinHeap;
    }

    // 큐에 아이템을 추가
    // 추가되면 힙 속성을 만족하도록 재정렬 시간 복잡도 O(log N)
    public void Enqueue(TElement element, TPriority priority)
    {
        // 1. 힙의 맨 마지막에 아이템 추가
        _heap.Add((element, priority));

        // 2. 힙 속성을 만족하도록 아이템을 위로 올림 (Sift Up)
        SiftUp(Count - 1);
    }

    // 큐에서 우선순위가 가장 높은 아이템을 제거하고 반환
    // 제거(O(1))하고 재정렬하기 떄문에 시간복잡도 O(log N)
    public (TElement Element, TPriority Priority) Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty.");

        // 1. 루트 아이템(우선순위가 가장 높은)을 저장
        var root = _heap[0];

        // 2. 마지막 아이템을 루트로 가져옴
        var last = _heap[Count - 1];
        // 유닛 제거에 사용한 SwapBack개념, 삭제에 시간복잡도 O(1)
        _heap[0] = last;
        _heap.RemoveAt(Count - 1); // 리스트에서 마지막 아이템 제거

        if (Count > 0)
        {
            // 3. 힙 속성을 만족하도록 새 루트를 아래로 내림 (Sift Down)
            SiftDown(0);
        }

        return root;
    }

    /// 큐에서 우선순위가 가장 높은 아이템을 제거하지 않고 반환 (O(1))
    public (TElement Element, TPriority Priority) Peek()
    {
        if (Count == 0)
            throw new InvalidOperationException("Queue is empty.");
        return _heap[0];
    }

    public void Clear()
    {
        _heap.Clear();
    }

    // --- 힙 내부 로직 ---
    // 현재 인덱스의 아이템을 위로 올려 힙 속성을 만족하도록 함
    // 인덱스 추가는 맨 마지막에 하므로, 부모와 비교하여 위치를 찾아감
    private void SiftUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (ShouldSwap(_heap[index].Priority, _heap[parentIndex].Priority))
            {
                Swap(index, parentIndex);
                index = parentIndex;
            }
            else
            {
                break;
            }
        }
    }
    // 현재 인덱스의 아이템을 아래로 내려 힙 속성을 만족하도록 함
    // 인덱스 제거는 루트에서 하므로, 자식과 비교하여 위치를 찾아감
    private void SiftDown(int index)
    {
        while (true)
        {
            int leftChildIndex = 2 * index + 1;
            int rightChildIndex = 2 * index + 2;
            int bestChildIndex = index; // 'best'는 힙 타입에 따라 달라짐

            // 왼쪽 자식과 비교
            if (leftChildIndex < Count && ShouldSwap(_heap[leftChildIndex].Priority, _heap[bestChildIndex].Priority))
            {
                bestChildIndex = leftChildIndex;
            }

            // 오른쪽 자식과 비교
            if (rightChildIndex < Count && ShouldSwap(_heap[rightChildIndex].Priority, _heap[bestChildIndex].Priority))
            {
                bestChildIndex = rightChildIndex;
            }

            // 현재 노드가 두 자식보다 우선순위가 높다면(올바른 위치) 종료
            if (bestChildIndex == index)
            {
                break;
            }

            // 자식과 위치 교환
            Swap(index, bestChildIndex);
            index = bestChildIndex; // 아래로 계속 탐색
        }
    }

    // 힙의 타입(Min/Max)에 따라 두 아이템을 교환해야 하는지 결정
    // 최소 힙: 자식이 부모보다 작으면 교환
    // 최대 힙: 자식이 부모보다 크면 교환
    private bool ShouldSwap(TPriority childPriority, TPriority parentPriority)
    {
        if (_isMinHeap)
            return childPriority.CompareTo(parentPriority) < 0; // 최소 힙: 자식이 부모보다 작으면
        else
            return childPriority.CompareTo(parentPriority) > 0; // 최대 힙: 자식이 부모보다 크면
    }

    private void Swap(int indexA, int indexB)
    {
        var temp = _heap[indexA];
        _heap[indexA] = _heap[indexB];
        _heap[indexB] = temp;
    }
}