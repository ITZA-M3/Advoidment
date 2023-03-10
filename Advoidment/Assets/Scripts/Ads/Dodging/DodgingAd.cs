using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DodgingAd : Advertisement
{
    public GameObject player;
    public GameObject ground;
    public GameObject groundCheck;
    public LayerMask groundLayer;
    private Vector3 scale;

    //Player variables
    private bool isGrounded;
    private float jumpForce;
    private float gravity;
    private float velocity;
    private bool canJump;

    //Enemy variables
    public GameObject enemy;
    public List<GameObject> enemies;
    public int enemyNumber;
    private float yValue;

    private bool isDead = false;
    public bool isMoving = true;

    public GameObject winScreen;
    public GameObject loseScreen;

    public AdManager adManager;

    public override bool Paused { get { return paused; } }
    public override bool Completed { get { return completed; } set { completed = value; } }

    // Start is called before the first frame update
    void Start()
    {
        adManager = GameObject.Find("AdManager").GetComponent<AdManager>();

        Difficulty = AdDifficulty.Medium;

        scale = transform.localScale;
        gravity = -9.8f * scale.y;
        jumpForce = 9f * (scale.y / 2.0f);

        enemy.GetComponent<SpriteRenderer>().enabled = false;
        yValue = enemy.transform.position.y;
        //Instantiate(enemy);
        loseScreen.GetComponent<SpriteRenderer>().enabled = false;
        winScreen.GetComponent<SpriteRenderer>().enabled = false;
        SpawnEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"IN UPADTE METHOD: {Completed}");
        adManager.ActiveAdComplete = Completed;
        adManager.ActiveAdDifficulty = Difficulty;

        if (Paused)
        {
            return;
        }

        //Enemy movement
        if (enemies.Count > 0 )
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (isMoving)
                {
                    enemies[i].transform.position += new Vector3((-5f * scale.x) * Time.deltaTime, 0, 0);
                }

                if (enemies[i])
                {
                    //Makes the enemies offscreen invisible
                    if (enemies[i].transform.localPosition.x >= 11.3f)
                    {
                        SpriteRenderer sprender = enemies[i].GetComponent<SpriteRenderer>();
                        sprender.enabled = false;
                    }

                    if (enemies[i].transform.localPosition.x <= 11.2f)
                    {
                        SpriteRenderer sprender = enemies[i].GetComponent<SpriteRenderer>();
                        sprender.enabled = true;
                    }

                    //Player dies
                    if (player.GetComponent<Collider2D>().bounds.Intersects(enemies[i].GetComponent<Collider2D>().bounds) && isDead == false)
                    {
                        Debug.Log("OWWWWW");
                        StartCoroutine(waiterDeath());
                    }

                    //Despawn enemies
                    //if (enemies[i].transform.localPosition.x <= -11.3f)
                    if (enemies[i].transform.localPosition.x <= -2.25f) {
                        Destroy(enemies[i]);
                        enemies.RemoveAt(i);
                    }
                }
            }
        }
        if (enemies.Count == 0 && isDead == false)
        {
            StartCoroutine(waiter());
        }

        //Player movement
        velocity += gravity * Time.deltaTime;

        if (isGrounded && velocity < 0)
        {
            velocity = 0;
            canJump = true;
        }

        if (Input.GetMouseButtonDown(0) && canJump)
        {
            Debug.Log("JUMP");
            velocity = jumpForce;
            canJump = false;
        }
        player.transform.Translate(new Vector3(0, velocity, 0) * Time.deltaTime);

        isGrounded = Physics2D.OverlapCircle(groundCheck.transform.position, 0.03f, groundLayer);
    }

    private void SpawnEnemies()
    {
        enemies.Clear();
        Debug.Log("Dodge those fools!");
        //float xPosition = 5 * scale.x;
        float xPosition = 2.5f * scale.x;

        enemies = new List<GameObject>(enemyNumber);

        for (int i = 0; i < enemyNumber; i ++)
        {
            GameObject newEnemy = Instantiate(enemy);
            newEnemy.transform.localScale = scale / 2.0f;
            newEnemy.transform.parent = transform;
            newEnemy.transform.localPosition = Vector3.zero;
            newEnemy.GetComponent<SpriteRenderer>().enabled = true;

            newEnemy.transform.position = new Vector2(
                transform.position.x + xPosition, yValue
                );

            enemies.Add(newEnemy);

            xPosition += (10f * scale.x);
        }

        isDead = false;
        isMoving = true;
    }

    private void DeleteEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            Destroy(enemies[i]);
        }
        SpawnEnemies();
    }

    protected override IEnumerator waiter()
    {
        //Completed = true;
        winScreen.GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(1);
        yield return Completed = true;
        Debug.Log("See ya!");
        //gameManager.activeAds = 0;
        Destroy(gameObject);
    }

    protected override IEnumerator waiterDeath()
    {
        loseScreen.GetComponent<SpriteRenderer>().enabled = true;
        isDead = true;
        isMoving = false;
        yield return new WaitForSeconds(1);
        Debug.Log("Restarting!");
        loseScreen.GetComponent<SpriteRenderer>().enabled = false;
        DeleteEnemies();
    }

    public override void CreateAd()
    {
        Instantiate(gameObject);
        gameObject.SetActive(true);
    }
}
