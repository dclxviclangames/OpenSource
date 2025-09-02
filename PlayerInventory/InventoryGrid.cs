using System.Collections.Generic;
using UnityEngine;

// Класс, который представляет ячейку в сетке.
public class GridCell
{
    public ItemData item;
    public int itemStartX;
    public int itemStartY;
}

/// <summary>
/// Логика инвентарной сетки.
/// </summary>
public class InventoryGrid
{
    private GridCell[,] grid;
    private int width;
    private int height;

    public InventoryGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new GridCell();
            }
        }
    }

    public void ClearGrid()
    {
        grid = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new GridCell();
            }
        }
    }

    /// <summary>
    /// Пытается разместить предмет на сетке.
    /// </summary>
    public bool PlaceItem(ItemData item, int startX, int startY)
    {
        if (CanPlaceItem(item, startX, startY))
        {
            for (int x = 0; x < item.width; x++)
            {
                for (int y = 0; y < item.height; y++)
                {
                    grid[startX + x, startY + y].item = item;
                    grid[startX + x, startY + y].itemStartX = startX;
                    grid[startX + x, startY + y].itemStartY = startY;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Удаляет предмет с сетки.
    /// </summary>
    public void RemoveItem(ItemData item)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].item == item)
                {
                    grid[x, y].item = null;
                }
            }
        }
    }

    /// <summary>
    /// Проверяет, свободно ли место для предмета.
    /// </summary>
    public bool CanPlaceItem(ItemData item, int startX, int startY)
    {
        if (startX < 0 || startY < 0 || startX + item.width > width || startY + item.height > height)
        {
            return false;
        }

        for (int x = 0; x < item.width; x++)
        {
            for (int y = 0; y < item.height; y++)
            {
                if (grid[startX + x, startY + y].item != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Возвращает список данных о предметах для сохранения.
    /// </summary>
    public List<InventoryData> GetItemDataForSave()
    {
        List<InventoryData> data = new List<InventoryData>();
        HashSet<ItemData> savedItems = new HashSet<ItemData>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].item != null && !savedItems.Contains(grid[x, y].item))
                {
                    data.Add(new InventoryData
                    {
                        name = grid[x, y].item.name,
                        x = grid[x, y].itemStartX,
                        y = grid[x, y].itemStartY
                    });
                    savedItems.Add(grid[x, y].item);
                }
            }
        }
        return data;
    }
}
