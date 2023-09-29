using System;
using System.Collections.Generic;
using UnityEngine;
using FurnitureStruct;

public class Floor : MonoBehaviour
{
    [SerializeField] public Vector2Int GridSize = new Vector2Int(10, 10);
    [SerializeField] public List<GameObject> buttons;
    [SerializeField] private GameObject[,] grid;

    public GameObject selectedObject;
    private SizeChanger sizeChanger;
    private CameraController cameraController;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float deltaTime = 0.3f;
    [SerializeField] public GameObject SizeChanger;

    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        grid = new GameObject[GridSize.x, GridSize.y];
        mainCamera = Camera.main;
        sizeChanger = GetComponent<SizeChanger>();

        foreach (var button in buttons)
        {
            button.SetActive(false);
        }
    }

    public void ChangeRoomSize()
    {
        if (!sizeChanger.isAwailable) return;

        int leftLength = sizeChanger.leftLength;
        int rightLength = sizeChanger.rightLength;
        Vector3 newPos = transform.position;
        Vector3 newScale = new Vector3(leftLength, transform.localScale.y, rightLength);

        int leftDiff = leftLength - (int)transform.localScale.x;
        int rightDiff = rightLength - (int)transform.localScale.z;

        if (leftDiff < 0 || rightDiff < 0)
        {
            ClearObjectOnBorder(leftLength, rightLength);
        }

        newPos.x -= Math.Sign(rightDiff) * (float)leftDiff / 2;
        newScale.x = leftLength;
        newPos.z -= Math.Sign(rightDiff) * (float)rightDiff / 2;
        newScale.z = rightLength;

        GridSize = new Vector2Int(leftLength, rightLength);
        GameObject[,] newGrid = new GameObject[leftLength, rightLength];

        for (int i = 0; i < Math.Min(leftLength, GridSize.x); i++)
        {
            for (int j = 0; j < Math.Min(rightLength, GridSize.y); j++)
            {
                try
                {
                    newGrid[i, j] = grid[i, j];
                }
                catch (IndexOutOfRangeException)
                {
                }
            }
        }

        grid = newGrid;
        transform.localScale = newScale;
        transform.position = newPos;
    }

    private void ClearObjectOnBorder(int newSizeLeft, int newSizeRight)
    {
        for (int i = 0; i < GridSize.x; i++)
        {
            for (int j = 0; j < GridSize.y; j++)
            {
                if ((i >= newSizeLeft || j >= newSizeRight) && grid[i, j] != null)
                {
                    var objToDestroy = grid[i, j];
                    DeleteObjectFromGrid(objToDestroy);
                    Destroy(objToDestroy);
                }
            }
        }
    }

    public void CreateNewObject(GameObject furniturePrefab)
    {
        if (selectedObject != null)
        {
            CancelSelection();
        }

        SelectObject(Instantiate(furniturePrefab));
    }

    public void DestroyObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            CancelSelection();
        }
    }

    private void SetObjectToGrid(int placeX, int placeY)
    {
        if (selectedObject == null) return;

        Furniture furniture = selectedObject.GetComponent<Furniture>();
        for (int x = 0; x < furniture.Size.x; x++)
        {
            for (int y = 0; y < furniture.Size.y; y++)
            {
                grid[placeX + x, placeY + y] = selectedObject;
            }
        }
    }

    private void DeleteObjectFromGrid(GameObject obj)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == obj)
                {
                    grid[i, j] = null;
                }
            }
        }
    }

    private bool CheckCellsForObject(GameObject obj, int placeX, int placeY)
    {
        Furniture furniture = obj.GetComponent<Furniture>();
        for (int x = 0; x < furniture.Size.x; x++)
        {
            for (int y = 0; y < furniture.Size.y; y++)
            {
                if (grid[placeX + x, placeY + y] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void CancelMovement()
    {
        if (selectedObject == null) return;

        SetStartPositionToSelectedObject();
        CancelSelection();
    }

    public void CancelSelection()
    {
        if (selectedObject == null) return;

        Vector3 selectedObjectPosition = selectedObject.transform.position;
        if (!CheckCellsForObject(selectedObject, (int)selectedObjectPosition.x, (int)selectedObjectPosition.z)) return;

        var outline = selectedObject.GetComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineHidden;
        DeleteObjectFromGrid(selectedObject);
        SetObjectToGrid((int)selectedObjectPosition.x, (int)selectedObjectPosition.z);
        selectedObject = null;

        foreach (var button in buttons)
        {
            button.SetActive(false);
        }
    }

    private void SavePositionOfSelectedObject()
    {
        if (selectedObject == null) return;
        StartPosition startPosition = new StartPosition(selectedObject.transform.position, selectedObject.transform.rotation, selectedObject.GetComponent<Furniture>().Size);
        selectedObject.GetComponent<Furniture>().StartPosition = startPosition;
    }

    private void SetStartPositionToSelectedObject()
    {
        if (selectedObject == null) return;
        StartPosition startPosition = selectedObject.GetComponent<Furniture>().StartPosition;
        selectedObject.transform.position = startPosition.position;
        selectedObject.transform.rotation = startPosition.rotation;
        selectedObject.GetComponent<Furniture>().Size = startPosition.Cells;
    }

    private void SelectObject(GameObject obj)
    {
        foreach (var button in buttons)
        {
            button.SetActive(true);
        }

        selectedObject = obj;
        SavePositionOfSelectedObject();
        var outline = selectedObject.GetComponent<Outline>();
        DeleteObjectFromGrid(selectedObject);
        outline.OutlineMode = Outline.Mode.OutlineVisible;
    }

    public void RotateSelectedObject()
    {
        var rotate = Quaternion.Euler(0, (selectedObject.transform.rotation.eulerAngles.y + 90) % 360, 0);
        selectedObject.GetComponent<Furniture>().Rotate();
        selectedObject.transform.rotation = rotate;
    }

    private void SetTransparentForObject(bool available)
    {
        if (selectedObject == null) return;
        var outline = selectedObject.GetComponent<Outline>();
        outline.OutlineColor = available ? Color.yellow : Color.red;
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = mainCamera.ScreenPointToRay(touch.position);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.tag == "FloorFurniture")
                {
                    if (deltaTime > 0)
                    {
                        deltaTime -= Time.deltaTime;
                    }
                    else
                    {
                        SelectObject(hit.collider.gameObject);
                        deltaTime = 0.3f;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (selectedObject != null)
        {
            HandleSelectedObjectMovement();
        }
        else
        {
            HandleTouchInput();
        }
    }

    private void HandleSelectedObjectMovement()
    {
        var selectedObjectSizeX = selectedObject.GetComponent<Furniture>().Size.x;
        var selectedObjectSizeY = selectedObject.GetComponent<Furniture>().Size.y;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 positionOfTouch = touch.position;

            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = mainCamera.ScreenPointToRay(positionOfTouch);

            if (groundPlane.Raycast(ray, out float position))
            {
                Vector3 worldPosition = mainCamera.ScreenPointToRay(positionOfTouch).GetPoint(position);
                worldPosition.x = Mathf.RoundToInt(worldPosition.x);
                worldPosition.z = Mathf.RoundToInt(worldPosition.z);

                // ѕроверка на валидность новой позиции.
                if (IsWithinFloorBorders(worldPosition, (int)selectedObjectSizeX, (int)selectedObjectSizeY)
                    && CheckCellsForObject(selectedObject, (int)worldPosition.x, (int)worldPosition.z))
                {
                    selectedObject.transform.position = worldPosition;
                    cameraController.allowCameraMovement = false;
                }
            }
        }
        else
        {
            cameraController.allowCameraMovement = true;
        }
    }

    private bool IsWithinFloorBorders(Vector3 position, int sizeX, int sizeY)
    {
        return position.x + sizeX <= GridSize.x && position.z + sizeY <= GridSize.y
            && position.x >= 0 && position.z >= 0;
    }
}
