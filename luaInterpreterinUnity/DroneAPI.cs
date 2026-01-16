/*using UnityEngine;
using TMPro; // Для работы с текстом
using MoonSharp.Interpreter;
using System.Collections;


// Разрешаем Lua видеть этот класс
[MoonSharpUserData]
public class DroneAPI : MonoBehaviour
{
   
    private Transform droneTransform;
    public bool isMoving = false; // Флаг для ожидания анимации
    private DronsControl droneControllers;

    void Start()
    {
        droneControllers = GetComponent<DronsControl>();
    }

    public DroneAPI(Transform transform)
    {
        droneTransform = transform;
    }

    public void move()
    {
        // В реальности здесь может быть запуск анимации или LeanTween
        droneTransform.position += droneTransform.forward;
        Debug.Log("Дрон сдвинулся");
    }

    public void nomove()
    {
        // В реальности здесь может быть запуск анимации или LeanTween
        droneTransform.position -= droneTransform.forward;
        Debug.Log("Дрон сдвинулся");
    }

    public void noturn()
    {
        droneTransform.Rotate(0, -90, 0);
    }

    public void turn()
    {
        droneTransform.Rotate(0, 90, 0);
    }

   
} 

using UnityEngine;
using System.Collections.Generic;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class DroneAPI : MonoBehaviour
{
    private enum CommandType { Move, Back, TurnRight, TurnLeft }
    private Queue<CommandType> commandQueue = new Queue<CommandType>();

    private bool isExecuting = false;
    private float timer = 0f;
    public float actionDuration = 0.5f; // Время на одно действие

    // Эти методы вызывает Lua - они просто добавляют задания в очередь
    public void move() => commandQueue.Enqueue(CommandType.Move);
    public void nomove() => commandQueue.Enqueue(CommandType.Back);
    public void turn() => commandQueue.Enqueue(CommandType.TurnRight);
    public void noturn() => commandQueue.Enqueue(CommandType.TurnLeft);

    public void ClearCommands()
    {
        commandQueue.Clear();
        isExecuting = false;
        timer = 0;
    }

    void Update()
    {
        if (commandQueue.Count > 0)
        {
            timer += Time.deltaTime;

            if (timer >= actionDuration)
            {
                ExecuteNextCommand();
                timer = 0f;
            }
        }
    }

    private void ExecuteNextCommand()
    {
        CommandType cmd = commandQueue.Dequeue();
        switch (cmd)
        {
            case CommandType.Move:
                transform.position += transform.forward;
                break;
            case CommandType.Back:
                transform.position -= transform.forward;
                break;
            case CommandType.TurnRight:
                transform.Rotate(0, 90, 0);
                break;
            case CommandType.TurnLeft:
                transform.Rotate(0, -90, 0);
                break;
        }
        Debug.Log($"Выполнено: {cmd}. Осталось в очереди: {commandQueue.Count}");
    }

    
}
*/

using UnityEngine;
using System.Collections.Generic;
using MoonSharp.Interpreter;

[MoonSharpUserData] // Оставляем, чтобы Lua видел класс
public class DroneAPI
{
    private Transform droneTransform;
    private Queue<System.Action> commands = new Queue<System.Action>();
    private float timer = 0f;
    private float interval = 0.5f;

    // Конструктор теперь будет работать правильно
    public DroneAPI(Transform transform)
    {
        droneTransform = transform;
    }

    public void move() => commands.Enqueue(() => {
        droneTransform.position += droneTransform.forward;
    });

    public void nomove() => commands.Enqueue(() => {
        droneTransform.position -= droneTransform.forward;
    });

    public void turn() => commands.Enqueue(() => {
        droneTransform.Rotate(0, 90, 0);
    });

    public void noturn() => commands.Enqueue(() => {
        droneTransform.Rotate(0, -90, 0);
    });

    public void ResetQueue()
    {
        commands.Clear();
        timer = 0;
    }

    public void UpdateLogic(float deltaTime)
    {
        if (commands.Count == 0) return;
        timer += deltaTime;
        if (timer >= interval)
        {
            var action = commands.Dequeue();
            action.Invoke();
            timer = 0f;
        }
    }
}



