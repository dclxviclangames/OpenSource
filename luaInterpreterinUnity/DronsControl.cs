using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
/*using UnityEngine;
using TMPro; // Для работы с текстом


public class DronsControl: MonoBehaviour
{
    // Ссылка на поле ввода
    public TMP_InputField inputField;
    private Script luaScript;
    private DroneAPI api;

    void Start()
    {
        // Инициализируем MoonSharp один раз
        UserData.RegisterType<DroneAPI>();
        //  UserData.RegisterAssembly();
        api = new DroneAPI();
    }

    // Метод, который вызывается при нажатии на кнопку "Run"
    public void RunPlayerCode()
    {
        // Останавливаем предыдущую корутину, если она была запущена, и запускаем новую
        StopAllCoroutines();
        StartCoroutine(ExecuteLuaCodeRoutine(inputField.text));
    }

    public void RunPlayerCode()
    {
        // Очищаем старые команды перед новым запуском
        api.ClearCommands();

        try
        {
            Script luaScript = new Script(CoreModules.Preset_Complete);
            luaScript.Globals["drone"] = api;

            // Выполняем код. Lua мгновенно заполнит очередь в api
            luaScript.DoString(inputField.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lua Error: " + e.Message);
        }
    }

    private IEnumerator ExecuteLuaCodeRoutine(string code)
    {
        luaScript = new Script(CoreModules.Preset_Complete);
        luaScript.Globals["drone"] = api;

        DynValue co;
        try
        {
            // Просто загружаем чистый пользовательский код
            DynValue function = luaScript.LoadString(code);
            co = luaScript.CreateCoroutine(function);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка при подготовке Lua кода: " + e.Message);
            yield break;
        }

        // !!! ИСПРАВЛЕНИЕ: Используем стандартную проверку состояния корутины !!!
        // Продолжаем, пока состояние не станет Terminated (завершено)
        while (co.Coroutine.State != CoroutineState.Dead)
        {
            try
            {
                // Если состояние Running, возобновляем его
                if (co.Coroutine.State == CoroutineState.Running)
                {
                    co.Coroutine.Resume();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Ошибка во время выполнения Lua кода: " + e.Message);
                // При ошибке выходим из C# корутины
                yield break;
            }

            yield return null;
        }

        Debug.Log("Lua script execution finished successfully.");
    }

} */

using UnityEngine;
using TMPro;
using MoonSharp.Interpreter;

public class DronsControl : MonoBehaviour
{
    public TMP_InputField inputField;
    private DroneAPI api;

    void Start()
    {
        // Регистрируем тип для MoonSharp
        UserData.RegisterType<DroneAPI>();

        // Теперь эта строка не будет выдавать ошибку
        api = new DroneAPI(this.transform);
    }

    void Update()
    {
        // Чтобы таймер внутри API тикал
        if (api != null)
        {
            api.UpdateLogic(Time.deltaTime);
        }
    }

    public void RunPlayerCode()
    {
        api.ResetQueue();
        try
        {
            Script luaScript = new Script(CoreModules.Preset_Complete);
            luaScript.Globals["drone"] = api;
            luaScript.DoString(inputField.text);
            Debug.Log("Код Lua загружен в очередь");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка Lua: " + e.Message);
        }
    }
}
