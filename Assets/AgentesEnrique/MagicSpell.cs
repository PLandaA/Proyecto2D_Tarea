using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSpell : MonoBehaviour
{
    // Start is called before the first frame update
    #region//Public
    public Vector3 target;
    public float velocity;
    public int damage;
    public float rotateSpeed;
    //public Collider2D[] canDamage;
    #endregion
    #region//Member
    Vector3 velocityVector;
    #endregion
    void Start()
    {
        velocityVector = (target - transform.position).normalized *velocity;
    }

    // Update is called once per frame
    void Update()
    {
        LookAt();
        transform.position += velocityVector;
        if ((target - transform.position).magnitude < 0.1f)
            Destroy(this.gameObject);
    }
    void LookAt()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.forward, (target - transform.position).normalized), rotateSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer != this.gameObject.layer)
        {
            if (collision.TryGetComponent(out AgentCold agent))
                agent.hp -= damage;
            Debug.Log("Le di al enemigo");
            Destroy(this.gameObject);

        }
    }
}
