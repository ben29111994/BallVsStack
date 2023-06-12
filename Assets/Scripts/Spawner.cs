using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine.EventSystems;

public class Spawner : MonoBehaviour {

    public static Spawner instance;
    public float maxVariance = 0f;
    public int count = 16;
    public GameObject InitialColumn;
    public List<GameObject> m_ColumnList;
    private int m_OldestColumnIndex;
    private int m_NewestColumnIndex;
    private float m_LastBallY;
    private int targetCounter;
    private float m_LaneSize;
    bool isStacking = false;
    bool isStartChallenge = false;
    public GameObject losePanel;
    public GameObject winPanel;
    public Text scoreText;
    public int score;
    public int bestScore;
    public int combo;
    public int stackCount;
    public int currentLevel;
    public Text currentLevelText;
    public Text nextLevelText;
    public Slider progressBar;
    int redCount = 0;
    float difficulty;
    public List<Texture> backgroundList = new List<Texture>();
    public List<GameObject> shapeList = new List<GameObject>();
    public List<Color> colorList = new List<Color>();
    public Color mainColor;
    public GameObject background;
    public GameObject menuCanvas;
    public GameObject mainCanvas;
    public bool isStart = false;
    bool isCamera = false;
    public GameObject ballManager;
    public Image splashScreen;
    public int isFever = -1;
    public GameObject feverPopup;
    public GameObject tapPlayButton;
    public GameObject compliment;
    float rot = 0;
    public List<GameObject> listEffects = new List<GameObject>();
    public GameObject confettiEffect;
    public GameObject feverBar;
    public Transform fill;
    public static bool isFeverDecrease = false;
    public GameObject ball;
    float deviation = 0;
    bool isChangeDirection = false;
    int maxRed = 4;
    public List<GameObject> effectList = new List<GameObject>();
    ParticleSystem effect;
    int shapeID;

    void Start() {
        //PlayerPrefs.DeleteAll();
        //if (!GameAnalytics._hasInitializeBeenCalled)
        //{
        //    GameAnalytics.Initialize();
        //}
        //GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "game", currentLevel);
        var ratio = Camera.main.aspect;
        if (ratio >= 0.5f && ratio <= 0.6f)
        {
            Camera.main.fieldOfView = 20;
        }
        else if (ratio >= 0.65f && ratio <= 0.8f)
        {
            Camera.main.fieldOfView = 18;
        }
        else
        {
            Camera.main.fieldOfView = 24;
        }
        Application.targetFrameRate = 60;
        isFever = -1;
        isStart = false;
        tapPlayButton.transform.DOScale(1.5f, 1).SetLoops(-1, LoopType.Yoyo);
        splashScreen.DOFade(0, 0);
        splashScreen.DOFade(1, 0.7f);
        isCamera = true;
        instance = this;
        m_LaneSize = InitialColumn.transform.localScale.y + 0.8f;
        m_ColumnList = new List<GameObject>();
        score = PlayerPrefs.GetInt("BestScore");
        bestScore = score;
        scoreText.text = "BEST: " + score.ToString();
        progressBar.value = 0;
        var stack1 = InitialColumn.transform.GetChild(0);
        var stack2 = InitialColumn.transform.GetChild(1);
        shapeID = Random.Range(0, shapeList.Count);
        effectList[shapeID].SetActive(true);
        effect = effectList[shapeID].GetComponent<ParticleSystem>();
        var shape1 = Instantiate(shapeList[shapeID]);
        var shape2 = Instantiate(shapeList[shapeID]);
        var scale1 = shape1.transform.localScale;
        var scale2 = shape2.transform.localScale;
        shape1.transform.parent = stack1.transform;
        shape2.transform.parent = stack2.transform;
        shape1.transform.localPosition = Vector3.zero;
        shape1.transform.localScale = scale1;
        shape1.transform.localEulerAngles = Vector3.zero;
        shape2.transform.localPosition = Vector3.zero;
        shape2.transform.localScale = scale2;
        shape2.transform.localEulerAngles = Vector3.zero;
        difficulty = 6;
        var randomCameraColor = Random.Range(0, backgroundList.Count);
        mainColor = colorList[Random.Range(0, colorList.Count)];
        var getColor = effect.main;
        getColor.startColor = mainColor;
        listEffects[Random.Range(0, listEffects.Count)].SetActive(true);
        currentLevel = PlayerPrefs.GetInt("CurrentLevel");
        if(currentLevel == 0)
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
            currentLevel = 1;
        }
        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = (currentLevel + 1).ToString();
        difficulty = 6 - currentLevel / 20;
        if (difficulty < 2)
        {
            difficulty = 2;
        }
        background.GetComponent<Renderer>().material.mainTexture = backgroundList[randomCameraColor];
        var startRedPos = Random.Range(3, 5);

        fill = feverBar.transform.GetChild(0);
        maxRed = 1 + (int)(currentLevel / 10);
        if(maxRed >= 4)
        {
            maxRed = 4;
        }
        count = 30 + currentLevel * 2;
        if(count >= 100)
        {
            count = 100;
        }

        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                m_ColumnList.Add(InitialColumn);
                SetColor(0);
            }
            else
            {
                GameObject newColumn = Instantiate(InitialColumn);
                newColumn.transform.parent = InitialColumn.transform.parent;
                m_ColumnList.Add(newColumn);
                PlaceColumn(m_ColumnList.Count - 1, m_ColumnList[m_ColumnList.Count - 2].transform.position, maxVariance);
            }
            if (i == startRedPos)
            {
                isStartChallenge = true;
            }
        }

        m_OldestColumnIndex = 0;
        m_NewestColumnIndex = m_ColumnList.Count - 1;
    }

    void Update() {
        if (Input.GetMouseButton(0) && !isStacking && isStart && isFever < 0)
        {
            isStacking = true;
            StartCoroutine(delayStacking(0.1f));
        }
        if (isFever >= 0 && !isStacking && isStart)
        {
            ball.GetComponent<Rigidbody>().isKinematic = true;
            ball.transform.position = new Vector3(ball.transform.position.x, Camera.main.transform.position.y - 34, ball.transform.position.z);
            isStacking = true;
            StartCoroutine(delayStacking(0.05f));
            isFever++;
            //Debug.Log(isFever);
            if (isFever >= 20)
            {
                isFever = -1;
                feverPopup.transform.DOKill();
                feverPopup.SetActive(false);
                SoundManager.instance.audioSource.pitch = 1;
                ball.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        if (isCamera)
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(0, m_ColumnList[m_OldestColumnIndex].transform.position.y + 55f, m_ColumnList[m_OldestColumnIndex].transform.position.z - 91), 5 * Time.deltaTime);

        if (isFeverDecrease)
        {
            var amount = fill.GetComponent<Image>().fillAmount;
            amount = amount - 0.0005f;
            fill.GetComponent<Image>().fillAmount = amount;
            if (fill.GetComponent<Image>().fillAmount <= 0)
            {
                isFeverDecrease = false;
                fill.GetComponent<Image>().fillAmount = 0;
                feverBar.SetActive(false);
            }
        }
    }

    public void Scoring(int combo)
    {
        score += 1 + ((int)(combo / 5));
        if (combo >= 1 && isFever == -1)
        {
            feverBar.SetActive(true);
            isFeverDecrease = false;
            fill.GetComponent<Image>().fillAmount += (float)(combo - 1) / 200;
            if (fill.GetComponent<Image>().fillAmount == 1 && isFever == -1)
            {
                isFever = 0;
                Fever();
            }
        }
        scoreText.text = score.ToString();
    }

    IEnumerator delayStacking(float time)
    {
        stackCount++;
        if (stackCount % 10 == 0)
        {
            GenerateCompliment();
        }
        if(m_OldestColumnIndex == 0)
        {
            m_OldestColumnIndex++;
        }
        var stack1 = m_ColumnList[m_OldestColumnIndex].transform.GetChild(0);
        var stack2 = m_ColumnList[m_OldestColumnIndex].transform.GetChild(1);
        stack1.transform.DOMoveX(0, time);
        stack2.transform.DOMoveX(0, time);
        SoundManager.instance.PlaySoundPitch(SoundManager.instance.glide);
        MMVibrationManager.Vibrate();
        if (shapeID == 0)
        {
            var effect = m_ColumnList[m_OldestColumnIndex].transform.GetChild(3).GetComponent<ParticleSystem>();
            var getColor = effect.main;
            if (m_ColumnList[m_OldestColumnIndex].tag == "DeadStack")
            {
                getColor.startColor = Color.red;
            }
            else
            {
                getColor.startColor = mainColor;
            }
            effect.Play();
        }
        else if (shapeID == 1)
        {
            var effect = m_ColumnList[m_OldestColumnIndex].transform.GetChild(4).GetComponent<ParticleSystem>();
            var getColor = effect.main;
            if (m_ColumnList[m_OldestColumnIndex].tag == "DeadStack")
            {
                getColor.startColor = Color.red;
            }
            else
            {
                getColor.startColor = mainColor;
            }
            effect.Play();
        }
        yield return new WaitForSeconds(time);
        //if (stackCount <= 51)
        //{
        //m_ColumnList[m_OldestColumnIndex].transform.DOKill();
        //var setPos = m_ColumnList[m_OldestColumnIndex].transform.position;
        //float modX = 0;
        //setPos.x = modX;
        //m_ColumnList[m_OldestColumnIndex].transform.position = setPos;
        //PlaceColumn(m_OldestColumnIndex, m_ColumnList[m_NewestColumnIndex].transform.position, maxVariance);
        m_NewestColumnIndex = m_OldestColumnIndex;
        m_OldestColumnIndex = (m_OldestColumnIndex + 1) % m_ColumnList.Count;
        m_LastBallY += m_LaneSize;
        //stack1.transform.localPosition = new Vector3(-3, 0, 0);
        //stack2.transform.localPosition = new Vector3(3, 0, 0);
        //}
        //else
        //{
        //    m_OldestColumnIndex = (m_OldestColumnIndex + 1) % m_ColumnList.Count;
        //}
        progressBar.value = stackCount;
        if (stackCount >= count - 1)
        {
            Win();
            isCamera = false;
        }
        isStacking = false;
    }

    private void PlaceColumn(int columnIndex, Vector3 prevColumnPos, float maximumVariance )
    {
        Vector3 movePos = new Vector3(prevColumnPos.x, prevColumnPos.y + m_LaneSize, prevColumnPos.z);
        m_ColumnList[columnIndex].transform.position = movePos;
        //rot += 5;
        if (!isChangeDirection)
        {
            deviation += 0.1f;
            if (deviation >= 2)
            {
                deviation -= 0.1f;
                isChangeDirection = true;
            }
        }
        else
        {
            deviation -= 0.1f;
            if (deviation <= 0.1f)
            {
                deviation += 0.1f;
                isChangeDirection = false;
            }
        }
        //m_ColumnList[columnIndex].transform.localEulerAngles = new Vector3(0, rot, 0);
        if (stackCount == 51)
        {
            //m_ColumnList[columnIndex].GetComponent<MeshFilter>().sharedMesh = finishMesh;
            m_ColumnList[columnIndex].name = "Finish";
            //m_ColumnList[columnIndex].GetComponent<Renderer>().material.mainTexture = finishTexture;
            //m_ColumnList[columnIndex].GetComponent<Renderer>().material.DOTiling(new Vector2(16, 16), 0);
            //var planeGore = m_ColumnList[columnIndex].transform.GetChild(0);
            //planeGore.gameObject.SetActive(false);
            //m_ColumnList[columnIndex].GetComponent<BoxCollider>().isTrigger = false;
            //planeGore.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 0);
            var modY = m_ColumnList[columnIndex].transform.position;
            modY.y += 5;
            m_ColumnList[columnIndex].transform.position = modY;
            m_ColumnList[columnIndex].transform.localScale = new Vector3(2f, 2f, 2f);
        }
        SetColor(columnIndex);
        targetCounter++;
        if (m_ColumnList[columnIndex].transform.Find(columnIndex.ToString()))
        {
            Destroy(m_ColumnList[columnIndex].transform.Find(columnIndex.ToString()).gameObject);
        }
    }

    private void SetColor(int columnIndex)
    {
        var isRed = Random.Range(0, 10);
        var stack1 = m_ColumnList[columnIndex].transform.GetChild(0);
        var stack2 = m_ColumnList[columnIndex].transform.GetChild(1);
        if(columnIndex != 0)
        {
            stack1.transform.DOMoveX(-1.5f - deviation, 0);
            stack2.transform.DOMoveX(1.5f + deviation, 0);
            var scale = 1 + deviation/5;
            m_ColumnList[columnIndex].transform.localScale = new Vector3(scale, scale, scale);
        }
        if (isRed > difficulty && isStartChallenge && columnIndex < count - 1)
        {
            if (redCount < maxRed)
            {
                stack1.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                stack1.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Color.red * 0.1f);
                stack2.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                stack2.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", Color.red * 0.1f);
                m_ColumnList[columnIndex].tag = "DeadStack";
                stack1.tag = "DeadStack";
                stack2.tag = "DeadStack";
                redCount++;
            }
            else
            {
                stack1.GetComponentInChildren<MeshRenderer>().material.color = mainColor;
                stack1.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", mainColor * 0.1f);
                stack2.GetComponentInChildren<MeshRenderer>().material.color = mainColor;
                stack2.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", mainColor * 0.1f);
                m_ColumnList[columnIndex].tag = "Stack";
                stack1.tag = "Stack";
                stack2.tag = "Stack";
                redCount = 0;
            }
        }
        else
        {
            stack1.GetComponentInChildren<MeshRenderer>().material.color = mainColor;
            stack1.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", mainColor * 0.1f);
            stack2.GetComponentInChildren<MeshRenderer>().material.color = mainColor;
            stack2.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", mainColor * 0.1f);
            m_ColumnList[columnIndex].tag = "Stack";
            stack1.tag = "Stack";
            stack2.tag = "Stack";
        }
    }

    public void RemoveBall(GameObject target)
    {
        target.SetActive(false);
        //FacebookAnalytics.Instance.LogGame_endEvent(1);
        //GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "game", score.ToString());
        isStart = false;
        losePanel.SetActive(true);
        var bestScore = PlayerPrefs.GetInt("BestScore");
        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
        }
    }

    public void Win()
    {
        isFever = -1;
        feverPopup.transform.DOKill();
        feverPopup.SetActive(false);
        SoundManager.instance.audioSource.pitch = 1;
        ball.GetComponent<Rigidbody>().isKinematic = false;
        isStart = false;
        confettiEffect.SetActive(true);
        winPanel.SetActive(true);
        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
        }
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
    }

    public void ButtonReplay()
    {
        //SoundManager.instance.PlaySound(SoundManager.instance.button);
        StartCoroutine(delayLoadScene());
    }

    IEnumerator delayLoadScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(0);
    }

    public void Fever()
    {
        feverBar.SetActive(false);
        isFeverDecrease = false;
        fill.GetComponent<Image>().fillAmount = 0;
        feverPopup.SetActive(true);
        feverPopup.transform.DOScale(1.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        var maxRange = m_NewestColumnIndex + 23;
        if(maxRange > m_ColumnList.Count - 1)
        {
            maxRange = m_ColumnList.Count - 1;
        }
        for (int i = m_NewestColumnIndex; i < maxRange; i++)
        {
            if (m_ColumnList[i].name != "Finish")
            {
                var stack1 = m_ColumnList[i].transform.GetChild(0);
                var stack2 = m_ColumnList[i].transform.GetChild(1);
                stack1.GetComponentInChildren<MeshRenderer>().material.color = mainColor;
                stack1.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", mainColor * 0.1f);
                stack2.GetComponentInChildren<MeshRenderer>().material.color = mainColor;
                stack2.GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", mainColor * 0.1f);
                m_ColumnList[i].tag = "Stack";
                m_ColumnList[i].transform.GetChild(0).tag = "Stack";
                m_ColumnList[i].transform.GetChild(1).tag = "Stack";
                redCount = 0;
            }
        }
    }

    public void ButtonStartGame()
    {
        //SoundManager.instance.PlaySound(SoundManager.instance.button);
        score = 0;
        scoreText.text = score.ToString();
        isStart = true;
        mainCanvas.SetActive(true);
        menuCanvas.SetActive(false);
        ballManager.SetActive(true);
        //FacebookAnalytics.Instance.LogGame_startEvent(1);
    }
   
    public void GenerateCompliment()
    {
        compliment.transform.DOKill();
        compliment.GetComponent<Text>().DOFade(0, 0);
        string[] complimentComponent = new string[] {
"ADMIRABLE",
"ADORABLE",
"ALLURING",
"AMAZING",
"ANGELIC",
"APPEALING",
"BEAUTEOUS",
"BEWITCHING",
"BOLD",
"BREATHTAKING",
"BREEZY",
"CAPTIVATING",
"CHARMING",
"CLASSY",
"COMELY",
"COURTEOUS",
"CREATIVE",
"CUTE",
"DAZZLING",
"DELICATE",
"DELIGHTFUL",
"DISTINGUISHED",
"DIVINE",
"ELEGANT",
"ENTHRALLING",
"ENTICING",
"EXCELLENT",
"EXQUISITE",
"FAIR",
"FASCINATING",
"FETCHING",
"FINE",
"FLAMBOYANT",
"FOXY",
"GENIAL",
"GORGEOUS",
"GRACEFUL",
"GRACIOUS",
"GRAND",
"GREAT STYLE",
"HANDSOME",
"IDEAL",
"IMPECCABLE",
"INVITING",
"LOVELY",
"MAGNETIC",
"MAGNIFICENT",
"MAJESTIC",
"MARVELOUS",
"MESMERIC",
"NICE",
"PLEASING",
"POLISHED",
"PRETTY",
"RADIANT",
"RAVISHING",
"REFINED",
"SENSUOUS",
"SHAPELY",
"SLIGHTLY",
"SPARKY",
"SPIRITED",
"SPLENDID",
"SPUNKY",
"STATUESQUE",
"STRIKING",
"STUNNING",
"SUBLIME",
"SUPERB",
"SYMMETRICAL",
"TANTALIZING",
"TEASING",
"TEMPTING",
"UNIQUE",
"WINNING",
"WONDERFUL",
"WONDROUS", };

        string nameComp = complimentComponent[Random.Range(0, complimentComponent.Length)].ToString();
        var text = nameComp;
        compliment.GetComponent<Text>().text = text;
        compliment.transform.DOPunchRotation(new Vector3(0, 0, 45), 0.5f);
        compliment.GetComponent<Text>().DOFade(1, 0);
        compliment.GetComponent<Text>().DOFade(0, 3);
    }
}
