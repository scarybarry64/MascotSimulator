using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    [SerializeField]
    private LevelConnection _connection;

    [SerializeField]
    private string _entrance_name; //name of spawnpoint in current level

    [SerializeField]
    private string _target_scene_name;

    [SerializeField]
    private Transform _spawn_point;

    private void Start()
    {
        if (_entrance_name == LevelConnection._entrance_name) //if (_connection == LevelConnection.ActiveConnection)
        {
            FindObjectOfType<Player>().transform.position = _spawn_point.position;
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        var player = other.collider.GetComponent<Player>();
        if (player != null)
        {
            //LevelConnection.ActiveConnection = _connection;
            LevelConnection._entrance_name = _entrance_name;
            SceneManager.LoadScene(_target_scene_name);
        }
    }
}
