using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody rb;
    public GameObject menu;

    [Header("Точки перемещения")]
    public Transform targetA;
    public Transform targetB;

    private bool isMoving = false;

    public float speed = 5f;
    public GameObject howToPlay;
    private Transform currentTarget;
    public void LoadGame()
    {
        currentTarget = targetA;
        menu.SetActive(false);
        Invoke("LoadLevel", 3.5f);
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(1);
    }

    public void ActiveatMen()
    {
        menu.SetActive(true);
    }






    public void StartMove()
    {
        // Сброс старых таймеров, чтобы не было "каши"
        CancelInvoke();

        currentTarget = targetA;

        if (howToPlay != null) howToPlay.SetActive(true);
        if (menu != null) menu.SetActive(false);

       // Invoke("ActiveatMen", 10f);
        Invoke("MoveB", 9f);
    }

    void Update() // Для Transform используем обычный Update
    {
        if (currentTarget == null) return;

        // Движение через Translate (направление * скорость * время)
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Проверка дистанции
        if (Vector3.Distance(transform.position, currentTarget.position) <= 0.1f)
        {
            // Убираем цель, чтобы машина замерла на месте
            currentTarget = null;
        }
    }

    public void MoveB()
    {
        menu.SetActive(true);
        howToPlay.SetActive(false);
        currentTarget = targetB;
    }


}
