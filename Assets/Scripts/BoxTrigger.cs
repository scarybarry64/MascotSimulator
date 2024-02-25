using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoxTrigger : MonoBehaviour
{
    //destroy actor after triggering once
    [SerializeField] bool _destroy_on_trigger;
    [SerializeField] string _tag_filter;
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;


    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Enter");
        if (CheckActivatorTag(other))
        {
            onTriggerEnter.Invoke();
            if (_destroy_on_trigger)
                Destroy(gameObject);
        }
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (CheckActivatorTag(other))
        {
            onTriggerExit.Invoke();

            if (_destroy_on_trigger)
                Destroy(gameObject);
        }
    }
    public void RemoveSelf()
    {
        Destroy(gameObject);
    }
    bool CheckActivatorTag(Collider2D other)
    {
        return string.IsNullOrEmpty(_tag_filter)||
            (!string.IsNullOrEmpty(_tag_filter) && other.gameObject.CompareTag(_tag_filter));
    }
    // Start is called before the first frame update
    void Start()
    {
    }
}
