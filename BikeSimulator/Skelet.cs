using UnityEngine;

public class NoteObject3D : MonoBehaviour
{
    public enum NoteType { Tap, Hold }
    public NoteType type;

    public float hitTime;      
    public float travelTime;
    public float holdDuration; // Длительность зажатия (только для Hold)
    public Vector3 startPos;
    public Vector3 targetPos;
    
    private bool isBeingHeld = false;
    private bool wasProcessed = false;
    private float holdTimer = 0f;

    void Update()
    {
        float currentDsp = (float)AudioSettings.dspTime;
        float timeSpent = currentDsp - (hitTime - travelTime);
        float t = timeSpent / travelTime;

        if (!isBeingHeld)
        {
            // Обычный полет к цели
            if (t > 1.15f && !wasProcessed) { Miss(); }
            else if (!wasProcessed) transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
        else
        {
            // Логика удержания
            holdTimer += Time.deltaTime;
            // Если игрок отпустил раньше или время вышло
            if (!Input.GetKey(FindObjectOfType<RhythmEngine3D>().hitKey) || holdTimer >= holdDuration)
            {
                FinishHold();
            }
        }
    }

    public void StartHit()
    {
        if (type == NoteType.Tap) {
            wasProcessed = true;
            FindObjectOfType<RhythmEngine3D>().AddCombo();
            Destroy(gameObject);
        } else {
            isBeingHeld = true;
            transform.position = targetPos; // Фиксируем на линии
            GetComponent<Renderer>().material.color = Color.yellow; // Визуально "горит"
        }
    }

    void FinishHold()
    {
        wasProcessed = true;
        if (holdTimer >= holdDuration * 0.8f) // Если удержал хотя бы 80% времени
            FindObjectOfType<RhythmEngine3D>().AddCombo();
        else
            FindObjectOfType<RhythmEngine3D>().MissNote();
            
        Destroy(gameObject);
    }

    void Miss() {
        wasProcessed = true;
        FindObjectOfType<RhythmEngine3D>().MissNote();
        Destroy(gameObject);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class RhythmEngine3D : MonoBehaviour
{
    public AudioSource audioSource;
    public SongData[] playlist;
    public GameObject notePrefab;
    public Transform spawnPoint, targetPoint;
    public KeyCode hitKey = KeyCode.Space;
    
    [Header("Stats")]
    public int health = 100;
    public int combo = 0;
    public Text uiText;

    private float secPerBeat, dspSongTime, nextBeatToSpawn;
    private NoteObject3D currentActiveHoldNote;

    void Start() { ChangeTrack(); }

    void Update()
    {
        float currentSongTime = (float)(AudioSettings.dspTime - dspSongTime);

        // Авто-спавн нот
        if (currentSongTime >= nextBeatToSpawn - 2.0f) {
            SpawnNote(nextBeatToSpawn);
            nextBeatToSpawn += secPerBeat;
        }

        // Обработка нажатия
        if (Input.GetKeyDown(hitKey)) CheckHit();
        
        if (uiText) uiText.text = $"HP: {health} | COMBO: {combo}";
    }

    void SpawnNote(float beatTime)
    {
        GameObject go = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
        var note = go.GetComponent<NoteObject3D>();
        note.hitTime = dspSongTime + beatTime;
        note.startPos = spawnPoint.position;
        note.targetPos = targetPoint.position;
        note.travelTime = 2.0f;

        // Рандомно делаем ноту длинной (каждая четвертая)
        if (Random.value > 0.75f) {
            note.type = NoteObject3D.NoteType.Hold;
            note.holdDuration = 0.5f; // Половина секунды зажатия
            go.transform.localScale = new Vector3(1, 1, 3); // Вытягиваем визуально
        }
    }

    void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(targetPoint.position, 1.0f);
        foreach (var col in hits) {
            var note = col.GetComponent<NoteObject3D>();
            if (note) {
                note.StartHit();
                return;
            }
        }
        MissNote(); // Нажал в пустоту
    }

    public void AddCombo() { 
        combo++; 
        UpdateVisuals();
    }

    public void MissNote() { 
        combo = 0; 
        health -= 5; 
        UpdateVisuals();
    }

    void UpdateVisuals() {
        // Логика смены цветов из предыдущего шага
        Color c = (combo >= 10) ? Color.cyan : Color.white;
        RenderSettings.ambientLight = c; 
    }

    void ChangeTrack() {
        int i = Random.Range(0, playlist.Length);
        audioSource.clip = playlist[i].clip;
        secPerBeat = 60f / playlist[i].bpm;
        audioSource.Play();
        dspSongTime = (float)AudioSettings.dspTime;
        nextBeatToSpawn = secPerBeat;
    }
}

[Header("Линии движения")]
public Transform[] spawnPoints; // Перетащи сюда 3 точки в инспекторе
public Transform[] targetPoints; // Перетащи сюда 3 точки напротив

void SpawnNote(float beatTime)
{
    // Рандомно выбираем одну из трех линий (0, 1 или 2)
    int line = Random.Range(0, spawnPoints.Length);

    GameObject go = Instantiate(notePrefab, spawnPoints[line].position, Quaternion.identity);
    var note = go.GetComponent<NoteObject3D>();
    
    note.hitTime = dspSongTime + beatTime;
    note.startPos = spawnPoints[line].position;
    note.targetPos = targetPoints[line].position; // Теперь летит по своей линии!
    note.travelTime = 2.0f;
    
    // Передаем кнопке клавишу этой линии (например: S, D, F)
    if(line == 0) note.myKey = KeyCode.S;
    if(line == 1) note.myKey = KeyCode.D;
    if(line == 2) note.myKey = KeyCode.F;
}
