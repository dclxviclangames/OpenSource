using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // 1. Объявляем Enum для состояний
    public enum GameState
    {
        Idle,
        Active,
        Paused,
        Finished
    }

    // 2. Текущее состояние игры
    public GameState CurrentState = GameState.Idle;

    // Ссылка на скрипт MouseClickHandler (перетащите в Инспекторе)
    public MouseClickHandler clickHandler;

    void Start()
    {
        // Подписываем метод SwitchNextState на Action клика
        if (clickHandler != null)
        {
            clickHandler.OnLeftClickAction += SwitchNextState;
            Debug.Log("Менеджер состояния подписан на клик.");
        }
       
        Debug.Log("Начальное состояние: " + CurrentState);
    }

    // Этот метод вызывается при каждом клике через Action.Invoke()
    private void SwitchNextState()
    {
        // Используем оператор switch для обработки текущего состояния
        switch (CurrentState)
        {
            case GameState.Idle:
                CurrentState = GameState.Active;
                Debug.Log("Переход в Active");
                break;

            case GameState.Active:
                CurrentState = GameState.Paused;
                Debug.Log("Переход в Paused");
                break;

            case GameState.Paused:
                CurrentState = GameState.Finished;
                Debug.Log("Переход в Finished");
                break;

            case GameState.Finished:
                CurrentState = GameState.Idle;
                Debug.Log("Переход в Idle (Начало цикла)");
                break;
               
            default:
                // Обработка на случай, если что-то пошло не так
                Debug.LogWarning("Неизвестное состояние!");
                break;
        }

        // Здесь вы можете вызвать дополнительный Action, который оповестит
        // все остальные системы о том, что состояние ИГРЫ изменилось.
        // Пример: OnGameStateChanged?.Invoke(CurrentState);
    }

    // Обязательно отписываемся
    void OnDestroy()
    {
        if (clickHandler != null)
        {
            clickHandler.OnLeftClickAction -= SwitchNextState;
        }
    }
}

//MouseClicker

using UnityEngine;
using System; // Обязательно добавьте это using для работы с Action

public class MouseClickHandler : MonoBehaviour
{
    // Объявление Action. Он может быть public, чтобы другие скрипты могли на него подписываться.
    public Action OnLeftClickAction;

    void Update()
    {
        // Проверяем, была ли нажата левая кнопка мыши в этом кадре
        if (Input.GetMouseButtonDown(0))
        {
            // Вызываем (Invoke) событие Action.
            // Проверка на null (?) гарантирует, что мы не получим ошибку, если подписчиков еще нет.
            OnLeftClickAction?.Invoke();
        }
    }
}

using UnityEngine;

public class SubscriberExample : MonoBehaviour
{
    // Ссылка на скрипт, который вызывает Action
    public MouseClickHandler clickHandler;

    void Start()
    {
        // Подписываем метод HandleClick на событие OnLeftClickAction
        if (clickHandler != null)
        {
            clickHandler.OnLeftClickAction += HandleClick;
            Debug.Log("Подписались на событие клика.");
        }
    }

    // Этот метод будет выполняться при каждом вызове OnLeftClickAction
    private void HandleClick()
    {
        Debug.Log("Событие Action вызвано! Левая кнопка мыши нажата.");
        // Здесь ваша логика: спаун объекта, выстрел, и т.д.
    }

    // Хорошая практика: отписываться от событий, когда объект уничтожается,
    // чтобы избежать утечек памяти (memory leaks).
    void OnDestroy()
    {
        if (clickHandler != null)
        {
            clickHandler.OnLeftClickAction -= HandleClick;
            Debug.Log("Отписались от события клика.");
        }
    }
}

//Interaction 
using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    // Список объектов, с которыми можно взаимодействовать прямо сейчас
    private List<GameObject> interactableObjects = new List<GameObject>();

    // Вызывается, когда объект входит в зону триггера
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем наличие нужного тега или интерфейса (см. шаг 3)
        if (other.CompareTag("Weapon") || other.CompareTag("Car") || other.CompareTag("Electronics"))
        {
            if (!interactableObjects.Contains(other.gameObject))
            {
                interactableObjects.Add(other.gameObject);
                // Тут можно показать иконку "Нажмите E"
            }
        }
    }

    // Вызывается, когда объект покидает зону триггера
    private void OnTriggerExit(Collider other)
    {
        if (interactableObjects.Contains(other.gameObject))
        {
            interactableObjects.Remove(other.gameObject);
            // Тут можно скрыть иконку взаимодействия
        }
    }
   
    // Этот метод будет вызываться по клику мыши через Action!
    public void PerformAction()
    {
        // ... Логика приоритетов будет здесь (см. шаг 4) ...
    }
}

// Определите интерфейс для всего, с чем можно взаимодействовать
public interface IInteractable
{
    void Interact();
    int GetPriority(); // Метод для определения приоритета
}

public class Weapon : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Подобрали/используем оружие!");
        // Логика стрельбы или подбора
    }

    public int GetPriority()
    {
        return 2; // Приоритет 2 (например, 3=Электроника, 2=Оружие, 1=Машина)
    }
}

public class PlayerInteraction : MonoBehaviour
{
    // ... (OnTriggerEnter/Exit код выше) ...

    public void PerformAction()
    {
        if (interactableObjects.Count > 0)
        {
            // Находим объект с наивысшим приоритетом
            IInteractable bestTarget = null;
            int highestPriority = -1;

            foreach (GameObject obj in interactableObjects)
            {
                IInteractable interactable = obj.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable.GetPriority() > highestPriority)
                    {
                        highestPriority = interactable.GetPriority();
                        bestTarget = interactable;
                    }
                }
            }

            // Если нашли цель, взаимодействуем с ней
            if (bestTarget != null)
            {
                bestTarget.Interact();
                return; // Важно: выходим из функции, чтобы не делать базовую атаку
            }
        }

        // Если триггер пуст или ничего интерактивного не найдено, делаем базовую атаку
        AttackWithoutWeapon();
    }

    private void AttackWithoutWeapon()
    {
        Debug.Log("Базовая атака без оружия!");
    }
}using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    // Список объектов, с которыми можно взаимодействовать прямо сейчас
    private List<GameObject> interactableObjects = new List<GameObject>();

    // Вызывается, когда объект входит в зону триггера
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем наличие нужного тега или интерфейса (см. шаг 3)
        if (other.CompareTag("Weapon") || other.CompareTag("Car") || other.CompareTag("Electronics"))
        {
            if (!interactableObjects.Contains(other.gameObject))
            {
                interactableObjects.Add(other.gameObject);
                // Тут можно показать иконку "Нажмите E"
            }
        }
    }

    // Вызывается, когда объект покидает зону триггера
    private void OnTriggerExit(Collider other)
    {
        if (interactableObjects.Contains(other.gameObject))
        {
            interactableObjects.Remove(other.gameObject);
            // Тут можно скрыть иконку взаимодействия
        }
    }
   
    // Этот метод будет вызываться по клику мыши через Action!
    public void PerformAction()
    {
        // ... Логика приоритетов будет здесь (см. шаг 4) ...
    }
}

// Определите интерфейс для всего, с чем можно взаимодействовать
public interface IInteractable
{
    void Interact();
    int GetPriority(); // Метод для определения приоритета
}

public class Weapon : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Подобрали/используем оружие!");
        // Логика стрельбы или подбора
    }

    public int GetPriority()
    {
        return 2; // Приоритет 2 (например, 3=Электроника, 2=Оружие, 1=Машина)
    }
}

public class PlayerInteraction : MonoBehaviour
{
    // ... (OnTriggerEnter/Exit код выше) ...

    public void PerformAction()
    {
        if (interactableObjects.Count > 0)
        {
            // Находим объект с наивысшим приоритетом
            IInteractable bestTarget = null;
            int highestPriority = -1;

            foreach (GameObject obj in interactableObjects)
            {
                IInteractable interactable = obj.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (interactable.GetPriority() > highestPriority)
                    {
                        highestPriority = interactable.GetPriority();
                        bestTarget = interactable;
                    }
                }
            }

            // Если нашли цель, взаимодействуем с ней
            if (bestTarget != null)
            {
                bestTarget.Interact();
                return; // Важно: выходим из функции, чтобы не делать базовую атаку
            }
        }

        // Если триггер пуст или ничего интерактивного не найдено, делаем базовую атаку
        AttackWithoutWeapon();
    }

    private void AttackWithoutWeapon()
    {
        Debug.Log("Базовая атака без оружия!");
    }
}

//Weapon
public class Weapon : MonoBehaviour, IInteractable
{
    // Ссылка на место в руке игрока, куда прикрепится оружие
    private Transform playerHand;

    public void Interact()
    {
        // 1. Находим точку крепления в иерархии игрока (нужно передать ссылку)
        // В реальной игре вы передадите ссылку на руку игрока через GameManager или PlayerController
        playerHand = GameObject.FindGameObjectWithTag("PlayerHand").transform;

        // 2. Делаем оружие дочерним объектом для руки
        transform.SetParent(playerHand);

        // 3. Сбрасываем позицию и вращение, чтобы оно было ровно в руке
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 4. Отключаем ненужные компоненты на земле
        GetComponent<Rigidbody>().isKinematic = true; // Отключаем физику
        //GetComponent<Collider>().enabled = false; // Отключаем коллайдер земли

        // 5. Включаем логику стрельбы (если она есть в другом скрипте)
        // GetComponent<WeaponShooter>().enabled = true;

        Debug.Log("Оружие прикреплено к игроку.");
    }
   
    // ... GetPriority() ...
}
//Equip
using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    // Ссылка на пустой объект в иерархии игрока, куда крепятся предметы (рука)
    public Transform handAttachPoint;

    // Ссылка на текущий предмет в руке (может быть null, если руки пусты)
    private GameObject currentHeldItem = null;

    // Метод для скрытия текущего предмета
    public void HideHeldItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.SetActive(false);
            Debug.Log("Скрыли предмет из рук для анимации.");
        }
    }

    // Метод для показа предмета обратно
    public void ShowHeldItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.SetActive(true);
            Debug.Log("Вернули предмет в руки.");
        }
    }

    // Метод для смены предмета (вызывается при подборе оружия)
    public void EquipItem(GameObject newItem)
    {
        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem); // Удаляем старый предмет
        }

        currentHeldItem = newItem;
        // Здесь мы прикрепляем newItem к handAttachPoint, как обсуждали ранее
        newItem.transform.SetParent(handAttachPoint);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        newItem.SetActive(true); // Убеждаемся, что он виден
    }
}

using UnityEngine;
// Предположим, что IInteractable определен в другом файле
// public interface IInteractable { void Interact(); int GetPriority(); }

public class ElectronicsBox : MonoBehaviour, IInteractable
{
    public float repairDuration = 3f;
    private PlayerEquipmentManager playerEquipment;

    void Start()
    {
        // Находим менеджер игрока при старте
        playerEquipment = FindObjectOfType<PlayerEquipmentManager>();
    }

    public void Interact()
    {
        if (playerEquipment != null)
        {
            // !!! КЛЮЧЕВОЙ МОМЕНТ !!!
            // Просим менеджера спрятать текущее оружие перед началом починки
            playerEquipment.HideHeldItem();
        }

        Debug.Log("Начинаю чинить электронику...");
        // Запуск анимации починки здесь
       
        // Запускаем корутину для завершения взаимодействия через время
        StartCoroutine(FinishInteractionAfterDelay(repairDuration));
    }

    public int GetPriority()
    {
        return 3; // Высокий приоритет
    }

    // Корутина для имитации процесса починки
    System.Collections.IEnumerator FinishInteractionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
       
        Debug.Log("Починка завершена!");

        if (playerEquipment != null)
        {
            // !!! КЛЮЧЕВОЙ МОМЕНТ !!!
            // Возвращаем оружие обратно в руки после завершения
            playerEquipment.ShowHeldItem();
        }
       
        // Удаляем объект электроники после починки, если нужно
        // Destroy(gameObject);
    }
}
